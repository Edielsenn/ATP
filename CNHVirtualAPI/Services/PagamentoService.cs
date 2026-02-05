using CNHVirtualAPI.Data;
using CNHVirtualAPI.DTOs;
using CNHVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Services;

public class PagamentoService
{
    private readonly ApplicationDbContext _context;
    private readonly AsaasService _asaasService;
    private readonly ILogger<PagamentoService> _logger;

    public PagamentoService(
        ApplicationDbContext context,
        AsaasService asaasService,
        ILogger<PagamentoService> logger)
    {
        _context = context;
        _asaasService = asaasService;
        _logger = logger;
    }

    public async Task<PagamentoResponse?> ProcessarPagamentoAsync(PagamentoRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Buscar plano
            var plano = await _context.Planos
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId && p.Ativo);

            if (plano == null)
            {
                _logger.LogError($"Plano {request.PlanoId} não encontrado");
                return null;
            }

            // 2. Criar ou buscar cliente
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == request.Cliente.Email);

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    Nome = request.Cliente.Nome,
                    Email = request.Cliente.Email,
                    CPF = request.Cliente.CPF,
                    Telefone = request.Cliente.Telefone,
                    CEP = request.Cliente.CEP,
                    Endereco = request.Cliente.Endereco,
                    Numero = request.Cliente.Numero,
                    Complemento = request.Cliente.Complemento,
                    Bairro = request.Cliente.Bairro,
                    Cidade = request.Cliente.Cidade,
                    Estado = request.Cliente.Estado
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            // 3. Criar cliente no ASAAS
            var asaasCliente = await _asaasService.CriarClienteAsync(request.Cliente);
            if (asaasCliente == null)
            {
                _logger.LogError("Erro ao criar cliente no ASAAS");
                return null;
            }

            // 4. Criar pedido
            var valorFinal = plano.PrecoPromocional ?? plano.Preco;
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                PlanoId = plano.Id,
                Numero = GerarNumeroPedido(),
                ValorTotal = plano.Preco,
                ValorDesconto = plano.PrecoPromocional.HasValue ? plano.Preco - plano.PrecoPromocional.Value : 0,
                ValorFinal = valorFinal,
                Status = "PENDING"
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // 5. Criar pagamento no ASAAS
            AsaasPaymentResponse? asaasPagamento = null;

            if (request.FormaPagamento == "BOLETO")
            {
                asaasPagamento = await _asaasService.CriarPagamentoBoletoAsync(
                    asaasCliente.Id,
                    valorFinal,
                    DateTime.Now.AddDays(3),
                    $"Plano {plano.Nome} - Pedido {pedido.Numero}"
                );
            }
            else if (request.FormaPagamento == "CREDIT_CARD" && request.Cartao != null)
            {
                asaasPagamento = await _asaasService.CriarPagamentoCartaoAsync(
                    asaasCliente.Id,
                    valorFinal,
                    $"Plano {plano.Nome} - Pedido {pedido.Numero}",
                    request.Cartao
                );
            }

            if (asaasPagamento == null)
            {
                _logger.LogError("Erro ao criar pagamento no ASAAS");
                await transaction.RollbackAsync();
                return null;
            }

            // 6. Criar registro de pagamento
            var pagamento = new Pagamento
            {
                PedidoId = pedido.Id,
                AsaasPaymentId = asaasPagamento.Id,
                FormaPagamento = request.FormaPagamento,
                Status = MapearStatusAsaas(asaasPagamento.Status),
                Valor = valorFinal,
                DataVencimento = asaasPagamento.DueDate,
                BoletoUrl = asaasPagamento.BankSlipUrl,
                LinhaDigitavel = asaasPagamento.IdentificationField,
                CodigoBarras = asaasPagamento.Barcode,
                CartaoBandeira = asaasPagamento.CreditCardBrand,
                CartaoUltimosDigitos = asaasPagamento.CreditCardNumber
            };

            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();

            // 7. Se pagamento confirmado, criar assinatura
            if (pagamento.Status == "CONFIRMED" || pagamento.Status == "RECEIVED")
            {
                await CriarAssinaturaAsync(pedido, plano);
                pedido.Status = "CONFIRMED";
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new PagamentoResponse
            {
                PagamentoId = pagamento.Id,
                PedidoId = pedido.Id,
                NumeroPedido = pedido.Numero,
                Status = pagamento.Status,
                FormaPagamento = pagamento.FormaPagamento,
                Valor = pagamento.Valor,
                BoletoUrl = pagamento.BoletoUrl,
                LinhaDigitavel = pagamento.LinhaDigitavel,
                DataVencimento = pagamento.DataVencimento,
                Mensagem = pagamento.Status == "CONFIRMED" ?
                    "Pagamento confirmado! Acesso liberado." :
                    "Pedido criado com sucesso. Aguardando pagamento."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento");
            await transaction.RollbackAsync();
            return null;
        }
    }

    private async Task CriarAssinaturaAsync(Pedido pedido, Plano plano)
    {
        var assinatura = new Assinatura
        {
            ClienteId = pedido.ClienteId,
            PlanoId = pedido.PlanoId,
            PedidoId = pedido.Id,
            Status = "ACTIVE",
            DataInicio = DateTime.Now,
            DataFim = DateTime.Now.AddDays(plano.DuracaoDias)
        };

        _context.Assinaturas.Add(assinatura);
        await _context.SaveChangesAsync();
    }

    private string GerarNumeroPedido()
    {
        return $"PED-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private string MapearStatusAsaas(string statusAsaas)
    {
        return statusAsaas.ToUpper() switch
        {
            "PENDING" => "PENDING",
            "RECEIVED" => "RECEIVED",
            "CONFIRMED" => "CONFIRMED",
            "OVERDUE" => "OVERDUE",
            "REFUNDED" => "REFUNDED",
            "RECEIVED_IN_CASH" => "RECEIVED",
            "REFUND_REQUESTED" => "REFUNDED",
            _ => "PENDING"
        };
    }

    public async Task<bool> ProcessarWebhookAsync(string paymentId, string evento)
    {
        try
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Pedido)
                    .ThenInclude(pe => pe.Plano)
                .FirstOrDefaultAsync(p => p.AsaasPaymentId == paymentId);

            if (pagamento == null)
            {
                _logger.LogWarning($"Pagamento {paymentId} não encontrado no webhook");
                return false;
            }

            // Consultar status atualizado no ASAAS
            var asaasPagamento = await _asaasService.ConsultarPagamentoAsync(paymentId);
            if (asaasPagamento == null)
                return false;

            // Atualizar status do pagamento
            var statusAnterior = pagamento.Status;
            pagamento.Status = MapearStatusAsaas(asaasPagamento.Status);
            pagamento.DataAtualizacao = DateTime.Now;

            if (asaasPagamento.PaymentDate.HasValue)
                pagamento.DataPagamento = asaasPagamento.PaymentDate;

            if (asaasPagamento.ClientPaymentDate.HasValue)
                pagamento.DataConfirmacao = asaasPagamento.ClientPaymentDate;

            // Se pagamento foi confirmado, criar assinatura
            if ((pagamento.Status == "CONFIRMED" || pagamento.Status == "RECEIVED") &&
                statusAnterior != "CONFIRMED" && statusAnterior != "RECEIVED")
            {
                await CriarAssinaturaAsync(pagamento.Pedido, pagamento.Pedido.Plano);
                pagamento.Pedido.Status = "CONFIRMED";
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao processar webhook para pagamento {paymentId}");
            return false;
        }
    }
}
