using System.Text.Json;
using CNHVirtualAPI.Data;
using CNHVirtualAPI.Models;
using CNHVirtualAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly PagamentoService _pagamentoService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        PagamentoService pagamentoService,
        ApplicationDbContext context,
        ILogger<WebhookController> logger)
    {
        _pagamentoService = pagamentoService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("asaas")]
    public async Task<IActionResult> ReceberWebhookAsaas([FromBody] JsonElement payload)
    {
        var webhookLog = new WebhookLog
        {
            Evento = payload.GetProperty("event").GetString() ?? "UNKNOWN",
            PayloadJson = payload.ToString(),
            DataRecebimento = DateTime.Now
        };

        try
        {
            _logger.LogInformation($"Webhook recebido: {webhookLog.Evento}");

            var evento = webhookLog.Evento;
            var payment = payload.GetProperty("payment");
            var paymentId = payment.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(paymentId))
            {
                webhookLog.ProcessadoComSucesso = false;
                webhookLog.MensagemErro = "PaymentId não encontrado no payload";
                _context.WebhookLogs.Add(webhookLog);
                await _context.SaveChangesAsync();
                return BadRequest(new { mensagem = "PaymentId inválido" });
            }

            // Processar webhook baseado no evento
            bool sucesso = evento switch
            {
                "PAYMENT_CREATED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_UPDATED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_CONFIRMED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_RECEIVED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_OVERDUE" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_DELETED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_RESTORED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_REFUNDED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_RECEIVED_IN_CASH" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_CHARGEBACK_REQUESTED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_CHARGEBACK_DISPUTE" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_AWAITING_CHARGEBACK_REVERSAL" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_DUNNING_RECEIVED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                "PAYMENT_DUNNING_REQUESTED" => await _pagamentoService.ProcessarWebhookAsync(paymentId, evento),
                _ => true // Eventos não tratados são considerados sucesso
            };

            webhookLog.ProcessadoComSucesso = sucesso;
            webhookLog.DataProcessamento = DateTime.Now;

            if (!sucesso)
            {
                webhookLog.MensagemErro = "Erro ao processar webhook";
            }

            _context.WebhookLogs.Add(webhookLog);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Webhook processado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook");

            webhookLog.ProcessadoComSucesso = false;
            webhookLog.MensagemErro = ex.Message;
            webhookLog.DataProcessamento = DateTime.Now;

            _context.WebhookLogs.Add(webhookLog);
            await _context.SaveChangesAsync();

            return StatusCode(500, new { mensagem = "Erro ao processar webhook" });
        }
    }
}
