using CNHVirtualAPI.Data;
using CNHVirtualAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WebhooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Webhook Subscriptions
    [HttpGet("subscriptions")]
    public async Task<ActionResult<List<WebhookSubscriptionResponse>>> GetSubscriptions()
    {
        try
        {
            var subscriptions = await _context.WebhookSubscriptions
                .OrderByDescending(w => w.DataCriacao)
                .ToListAsync();

            var response = subscriptions.Select(w => new WebhookSubscriptionResponse
            {
                Id = w.Id,
                Nome = w.Nome,
                Url = w.Url,
                Eventos = w.Eventos,
                Ativo = w.Ativo,
                DataCriacao = w.DataCriacao
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar webhooks" });
        }
    }

    [HttpGet("subscriptions/{id}")]
    public async Task<ActionResult<WebhookSubscriptionResponse>> GetSubscription(int id)
    {
        try
        {
            var subscription = await _context.WebhookSubscriptions.FindAsync(id);

            if (subscription == null)
                return NotFound(new { message = "Webhook não encontrado" });

            var response = new WebhookSubscriptionResponse
            {
                Id = subscription.Id,
                Nome = subscription.Nome,
                Url = subscription.Url,
                Eventos = subscription.Eventos,
                Ativo = subscription.Ativo,
                Secret = subscription.Secret,
                DataCriacao = subscription.DataCriacao
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar webhook" });
        }
    }

    [HttpPost("subscriptions")]
    public async Task<ActionResult<WebhookSubscriptionResponse>> CreateSubscription([FromBody] WebhookSubscriptionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome é obrigatório" });

            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { message = "URL é obrigatória" });

            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                return BadRequest(new { message = "URL inválida" });

            var subscription = new WebhookSubscription
            {
                Nome = request.Nome,
                Url = request.Url,
                Eventos = request.Eventos ?? "[]",
                Ativo = request.Ativo,
                Secret = request.Secret,
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _context.WebhookSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var response = new WebhookSubscriptionResponse
            {
                Id = subscription.Id,
                Nome = subscription.Nome,
                Url = subscription.Url,
                Eventos = subscription.Eventos,
                Ativo = subscription.Ativo,
                DataCriacao = subscription.DataCriacao
            };

            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro ao criar: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao criar webhook" });
        }
    }

    [HttpPut("subscriptions/{id}")]
    public async Task<ActionResult<WebhookSubscriptionResponse>> UpdateSubscription(int id, [FromBody] WebhookSubscriptionRequest request)
    {
        try
        {
            var subscription = await _context.WebhookSubscriptions.FindAsync(id);

            if (subscription == null)
                return NotFound(new { message = "Webhook não encontrado" });

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome é obrigatório" });

            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest(new { message = "URL é obrigatória" });

            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                return BadRequest(new { message = "URL inválida" });

            subscription.Nome = request.Nome;
            subscription.Url = request.Url;
            subscription.Eventos = request.Eventos ?? "[]";
            subscription.Ativo = request.Ativo;
            subscription.Secret = request.Secret;
            subscription.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            var response = new WebhookSubscriptionResponse
            {
                Id = subscription.Id,
                Nome = subscription.Nome,
                Url = subscription.Url,
                Eventos = subscription.Eventos,
                Ativo = subscription.Ativo,
                DataCriacao = subscription.DataCriacao
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro ao atualizar: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar webhook" });
        }
    }

    [HttpDelete("subscriptions/{id}")]
    public async Task<ActionResult> DeleteSubscription(int id)
    {
        try
        {
            var subscription = await _context.WebhookSubscriptions.FindAsync(id);

            if (subscription == null)
                return NotFound(new { message = "Webhook não encontrado" });

            _context.WebhookSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Webhook removido com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro ao deletar: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao deletar webhook" });
        }
    }

    [HttpPut("subscriptions/{id}/toggle-status")]
    public async Task<ActionResult> ToggleSubscriptionStatus(int id)
    {
        try
        {
            var subscription = await _context.WebhookSubscriptions.FindAsync(id);

            if (subscription == null)
                return NotFound(new { message = "Webhook não encontrado" });

            subscription.Ativo = !subscription.Ativo;
            subscription.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Webhook {(subscription.Ativo ? "ativado" : "desativado")} com sucesso", ativo = subscription.Ativo });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro ao alternar status: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar status" });
        }
    }

    // Webhook Logs
    [HttpGet("logs")]
    public async Task<ActionResult<List<WebhookLogResponse>>> GetLogs([FromQuery] int? limit = 50)
    {
        try
        {
            var logs = await _context.WebhookLogs
                .OrderByDescending(w => w.DataRecebimento)
                .Take(limit ?? 50)
                .ToListAsync();

            var response = logs.Select(w => new WebhookLogResponse
            {
                Id = w.Id,
                Evento = w.Evento,
                ProcessadoComSucesso = w.ProcessadoComSucesso,
                MensagemErro = w.MensagemErro,
                DataRecebimento = w.DataRecebimento,
                DataProcessamento = w.DataProcessamento
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro ao buscar logs: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar logs" });
        }
    }

    [HttpGet("logs/{id}")]
    public async Task<ActionResult<WebhookLogDetailResponse>> GetLog(int id)
    {
        try
        {
            var log = await _context.WebhookLogs.FindAsync(id);

            if (log == null)
                return NotFound(new { message = "Log não encontrado" });

            var response = new WebhookLogDetailResponse
            {
                Id = log.Id,
                Evento = log.Evento,
                PayloadJson = log.PayloadJson,
                ProcessadoComSucesso = log.ProcessadoComSucesso,
                MensagemErro = log.MensagemErro,
                DataRecebimento = log.DataRecebimento,
                DataProcessamento = log.DataProcessamento
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOKS] Erro ao buscar log: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar log" });
        }
    }
}

// DTOs
public class WebhookSubscriptionRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Eventos { get; set; }
    public bool Ativo { get; set; } = true;
    public string? Secret { get; set; }
}

public class WebhookSubscriptionResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Eventos { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string? Secret { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class WebhookLogResponse
{
    public int Id { get; set; }
    public string Evento { get; set; } = string.Empty;
    public bool ProcessadoComSucesso { get; set; }
    public string? MensagemErro { get; set; }
    public DateTime DataRecebimento { get; set; }
    public DateTime? DataProcessamento { get; set; }
}

public class WebhookLogDetailResponse : WebhookLogResponse
{
    public string PayloadJson { get; set; } = string.Empty;
}
