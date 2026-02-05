using CNHVirtualAPI.Data;
using CNHVirtualAPI.DTOs;
using CNHVirtualAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagamentosController : ControllerBase
{
    private readonly PagamentoService _pagamentoService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PagamentosController> _logger;

    public PagamentosController(
        PagamentoService pagamentoService,
        ApplicationDbContext context,
        ILogger<PagamentosController> logger)
    {
        _pagamentoService = pagamentoService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("processar")]
    public async Task<IActionResult> ProcessarPagamento([FromBody] PagamentoRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _pagamentoService.ProcessarPagamentoAsync(request);

            if (resultado == null)
                return BadRequest(new { mensagem = "Erro ao processar pagamento" });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento");
            return StatusCode(500, new { mensagem = "Erro interno ao processar pagamento" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPagamento(int id)
    {
        var pagamento = await _context.Pagamentos
            .Include(p => p.Pedido)
                .ThenInclude(pe => pe.Cliente)
            .Include(p => p.Pedido)
                .ThenInclude(pe => pe.Plano)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.AsaasPaymentId,
                p.FormaPagamento,
                p.Status,
                p.Valor,
                p.ValorRecebido,
                p.DataVencimento,
                p.DataPagamento,
                p.DataConfirmacao,
                p.BoletoUrl,
                p.LinhaDigitavel,
                p.CartaoBandeira,
                p.CartaoUltimosDigitos,
                Pedido = new
                {
                    p.Pedido.Id,
                    p.Pedido.Numero,
                    p.Pedido.ValorFinal,
                    p.Pedido.Status,
                    Cliente = new
                    {
                        p.Pedido.Cliente.Nome,
                        p.Pedido.Cliente.Email
                    },
                    Plano = new
                    {
                        p.Pedido.Plano.Nome
                    }
                }
            })
            .FirstOrDefaultAsync();

        if (pagamento == null)
            return NotFound(new { mensagem = "Pagamento não encontrado" });

        return Ok(pagamento);
    }

    [HttpGet("pedido/{numeroPedido}")]
    public async Task<IActionResult> GetPagamentoPorPedido(string numeroPedido)
    {
        var pagamento = await _context.Pagamentos
            .Include(p => p.Pedido)
            .Where(p => p.Pedido.Numero == numeroPedido)
            .Select(p => new
            {
                p.Id,
                p.Status,
                p.FormaPagamento,
                p.Valor,
                p.BoletoUrl,
                p.LinhaDigitavel,
                p.DataVencimento,
                Pedido = new
                {
                    p.Pedido.Numero,
                    p.Pedido.Status
                }
            })
            .FirstOrDefaultAsync();

        if (pagamento == null)
            return NotFound(new { mensagem = "Pagamento não encontrado" });

        return Ok(pagamento);
    }
}
