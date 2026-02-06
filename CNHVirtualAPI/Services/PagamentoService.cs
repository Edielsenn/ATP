using CNHVirtualAPI.Data;
using CNHVirtualAPI.DTOs;
using CNHVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Services;

public class PagamentoService
{
    private readonly ApplicationDbContext _context;
    private readonly AsaasService _asaasService;
    private readonly EmailService _emailService;
    private readonly ILogger<PagamentoService> _logger;
    private readonly IConfiguration _configuration;

    public PagamentoService(
        ApplicationDbContext context,
        AsaasService asaasService,
        EmailService emailService,
        ILogger<PagamentoService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _asaasService = asaasService;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
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

            string? senhaGerada = null;
            bool isNovoCliente = false;

            if (cliente == null)
            {
                // Gerar senha aleatória para novo cliente
                senhaGerada = GerarSenhaAleatoria();
                var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaGerada);

                cliente = new Cliente
                {
                    Nome = request.Cliente.Nome,
                    Email = request.Cliente.Email,
                    CPF = request.Cliente.CPF,
                    Telefone = request.Cliente.Telefone,
                    SenhaHash = senhaHash,
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
                isNovoCliente = true;
            }

            // 3. Criar cliente no ASAAS (ou simular se não houver chave API)
            var apiKey = _configuration["Asaas:ApiKey"];
            var modoSimulacao = string.IsNullOrWhiteSpace(apiKey);

            AsaasCustomerResponse? asaasCliente = null;

            if (modoSimulacao)
            {
                _logger.LogWarning("[SIMULAÇÃO] Modo de simulação ativado - ASAAS API Key não configurada");
                // Criar dados simulados
                asaasCliente = new AsaasCustomerResponse
                {
                    Id = $"cus_sim_{Guid.NewGuid().ToString("N")[..12]}",
                    Name = request.Cliente.Nome,
                    Email = request.Cliente.Email
                };
            }
            else
            {
                asaasCliente = await _asaasService.CriarClienteAsync(request.Cliente);
                if (asaasCliente == null)
                {
                    _logger.LogError("Erro ao criar cliente no ASAAS");
                    return null;
                }
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

            // 5. Criar pagamento no ASAAS (ou simular)
            AsaasPaymentResponse? asaasPagamento = null;

            if (modoSimulacao)
            {
                _logger.LogWarning($"[SIMULAÇÃO] Criando pagamento simulado: {request.FormaPagamento}");
                // Criar dados simulados baseado no tipo de pagamento
                asaasPagamento = new AsaasPaymentResponse
                {
                    Id = $"pay_sim_{Guid.NewGuid().ToString("N")[..12]}",
                    Customer = asaasCliente!.Id,
                    BillingType = request.FormaPagamento,
                    Value = valorFinal,
                    Status = "PENDING",
                    DueDate = DateTime.Now.AddDays(3)
                };

                if (request.FormaPagamento == "BOLETO")
                {
                    asaasPagamento.BankSlipUrl = $"https://simulacao.asaas.com/boleto/{asaasPagamento.Id}";
                    asaasPagamento.IdentificationField = "23793.38128 60000.000000 00000.000000 0 00000000000000";
                    asaasPagamento.Barcode = "23793381286000000000000000000000000000000000";
                }
                else if (request.FormaPagamento == "PIX")
                {
                    asaasPagamento.BankSlipUrl = $"https://simulacao.asaas.com/pix/{asaasPagamento.Id}";
                }
            }
            else
            {
                if (request.FormaPagamento == "BOLETO")
                {
                    asaasPagamento = await _asaasService.CriarPagamentoBoletoAsync(
                        asaasCliente!.Id,
                        valorFinal,
                        DateTime.Now.AddDays(3),
                        $"Plano {plano.Nome} - Pedido {pedido.Numero}"
                    );
                }
                else if (request.FormaPagamento == "CREDIT_CARD" && request.Cartao != null)
                {
                    asaasPagamento = await _asaasService.CriarPagamentoCartaoAsync(
                        asaasCliente!.Id,
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
                await CriarAssinaturaAsync(pedido, plano, cliente, senhaGerada, isNovoCliente);
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

    private async Task CriarAssinaturaAsync(Pedido pedido, Plano plano, Cliente cliente, string? senhaGerada, bool isNovoCliente)
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

        // Enviar email com credenciais para novos clientes
        if (isNovoCliente && !string.IsNullOrEmpty(senhaGerada))
        {
            try
            {
                await _emailService.EnviarEmailCredenciais(cliente, senhaGerada);
                _logger.LogInformation($"[PAGAMENTO] Email de credenciais enviado para {cliente.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[PAGAMENTO] Erro ao enviar email de credenciais para {cliente.Email}");
            }
        }
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

    private string GerarSenhaAleatoria()
    {
        // Gera uma senha de 8 caracteres com letras e números
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Evita caracteres ambíguos
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
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
                // Carregar cliente para envio de credenciais
                var cliente = await _context.Clientes.FindAsync(pagamento.Pedido.ClienteId);
                if (cliente != null)
                {
                    // Para webhooks, verificar se o cliente já tem senha
                    string? senhaGerada = null;
                    bool isNovoCliente = false;

                    if (string.IsNullOrEmpty(cliente.SenhaHash))
                    {
                        // Cliente não tem senha, gerar uma
                        senhaGerada = GerarSenhaAleatoria();
                        cliente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaGerada);
                        cliente.DataAtualizacao = DateTime.Now;
                        isNovoCliente = true;
                        await _context.SaveChangesAsync();
                    }

                    await CriarAssinaturaAsync(pagamento.Pedido, pagamento.Pedido.Plano, cliente, senhaGerada, isNovoCliente);
                }
                else
                {
                    // Fallback se cliente não for encontrado (não deveria acontecer)
                    var clienteFallback = new Cliente();
                    await CriarAssinaturaAsync(pagamento.Pedido, pagamento.Pedido.Plano, clienteFallback, null, false);
                }
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
