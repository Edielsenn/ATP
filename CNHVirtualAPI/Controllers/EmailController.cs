using CNHVirtualAPI.Data;
using CNHVirtualAPI.Models;
using CNHVirtualAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public EmailController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // SMTP Configuration
    [HttpGet("smtp-config")]
    public async Task<ActionResult<SmtpConfigResponse>> GetSmtpConfig()
    {
        try
        {
            var configs = await _context.Configuracoes
                .Where(c => c.Chave.StartsWith("SMTP_"))
                .ToListAsync();

            var response = new SmtpConfigResponse
            {
                Host = configs.FirstOrDefault(c => c.Chave == "SMTP_HOST")?.Valor ?? "",
                Port = configs.FirstOrDefault(c => c.Chave == "SMTP_PORT")?.Valor ?? "587",
                Username = configs.FirstOrDefault(c => c.Chave == "SMTP_USERNAME")?.Valor ?? "",
                Password = configs.FirstOrDefault(c => c.Chave == "SMTP_PASSWORD")?.Valor ?? "",
                FromEmail = configs.FirstOrDefault(c => c.Chave == "SMTP_FROM_EMAIL")?.Valor ?? "",
                FromName = configs.FirstOrDefault(c => c.Chave == "SMTP_FROM_NAME")?.Valor ?? "CNH Virtual",
                UseSsl = configs.FirstOrDefault(c => c.Chave == "SMTP_USE_SSL")?.Valor?.ToLower() == "true"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao buscar config SMTP: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar configuração" });
        }
    }

    [HttpPost("smtp-config")]
    public async Task<ActionResult> SaveSmtpConfig([FromBody] SmtpConfigRequest request)
    {
        try
        {
            var configs = new Dictionary<string, string>
            {
                { "SMTP_HOST", request.Host ?? "" },
                { "SMTP_PORT", request.Port ?? "587" },
                { "SMTP_USERNAME", request.Username ?? "" },
                { "SMTP_PASSWORD", request.Password ?? "" },
                { "SMTP_FROM_EMAIL", request.FromEmail ?? "" },
                { "SMTP_FROM_NAME", request.FromName ?? "CNH Virtual" },
                { "SMTP_USE_SSL", request.UseSsl.ToString().ToLower() }
            };

            foreach (var kvp in configs)
            {
                var config = await _context.Configuracoes
                    .FirstOrDefaultAsync(c => c.Chave == kvp.Key);

                if (config == null)
                {
                    config = new Configuracao
                    {
                        Chave = kvp.Key,
                        Valor = kvp.Value,
                        DataCriacao = DateTime.Now
                    };
                    _context.Configuracoes.Add(config);
                }
                else
                {
                    config.Valor = kvp.Value;
                    config.DataAtualizacao = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Configuração SMTP salva com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao salvar config SMTP: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao salvar configuração" });
        }
    }

    [HttpPost("test")]
    public async Task<ActionResult> TestSmtpConfig([FromBody] TestEmailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.EmailTeste))
                return BadRequest(new { message = "Email de teste é obrigatório" });

            var success = await _emailService.TestarConfiguracao(request.EmailTeste);

            if (success)
                return Ok(new { message = "Email de teste enviado com sucesso! Verifique sua caixa de entrada." });
            else
                return BadRequest(new { message = "Falha ao enviar email de teste. Verifique suas configurações SMTP." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao testar config: {ex.Message}");
            return StatusCode(500, new { message = $"Erro ao testar configuração: {ex.Message}" });
        }
    }

    // Templates
    [HttpGet("templates")]
    public async Task<ActionResult<List<EmailTemplateResponse>>> GetTemplates()
    {
        try
        {
            var templates = await _context.EmailTemplates
                .OrderBy(t => t.Nome)
                .ToListAsync();

            var response = templates.Select(t => new EmailTemplateResponse
            {
                Id = t.Id,
                Nome = t.Nome,
                Codigo = t.Codigo,
                Assunto = t.Assunto,
                Ativo = t.Ativo,
                DataCriacao = t.DataCriacao
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao buscar templates: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar templates" });
        }
    }

    [HttpGet("templates/{id}")]
    public async Task<ActionResult<EmailTemplateDetailResponse>> GetTemplate(int id)
    {
        try
        {
            var template = await _context.EmailTemplates.FindAsync(id);

            if (template == null)
                return NotFound(new { message = "Template não encontrado" });

            var response = new EmailTemplateDetailResponse
            {
                Id = template.Id,
                Nome = template.Nome,
                Codigo = template.Codigo,
                Assunto = template.Assunto,
                CorpoHtml = template.CorpoHtml,
                CorpoTexto = template.CorpoTexto,
                Ativo = template.Ativo
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao buscar template: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar template" });
        }
    }

    [HttpPost("templates")]
    public async Task<ActionResult<EmailTemplateResponse>> CreateTemplate([FromBody] EmailTemplateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome é obrigatório" });

            if (string.IsNullOrWhiteSpace(request.Codigo))
                return BadRequest(new { message = "Código é obrigatório" });

            // Verificar se código já existe
            var exists = await _context.EmailTemplates.AnyAsync(t => t.Codigo == request.Codigo);
            if (exists)
                return BadRequest(new { message = "Já existe um template com este código" });

            var template = new EmailTemplate
            {
                Nome = request.Nome,
                Codigo = request.Codigo,
                Assunto = request.Assunto ?? "",
                CorpoHtml = request.CorpoHtml ?? "",
                CorpoTexto = request.CorpoTexto,
                Ativo = request.Ativo,
                DataCriacao = DateTime.Now
            };

            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();

            var response = new EmailTemplateResponse
            {
                Id = template.Id,
                Nome = template.Nome,
                Codigo = template.Codigo,
                Assunto = template.Assunto,
                Ativo = template.Ativo,
                DataCriacao = template.DataCriacao
            };

            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao criar template: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao criar template" });
        }
    }

    [HttpPut("templates/{id}")]
    public async Task<ActionResult<EmailTemplateResponse>> UpdateTemplate(int id, [FromBody] EmailTemplateRequest request)
    {
        try
        {
            var template = await _context.EmailTemplates.FindAsync(id);

            if (template == null)
                return NotFound(new { message = "Template não encontrado" });

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome é obrigatório" });

            template.Nome = request.Nome;
            template.Assunto = request.Assunto ?? "";
            template.CorpoHtml = request.CorpoHtml ?? "";
            template.CorpoTexto = request.CorpoTexto;
            template.Ativo = request.Ativo;
            template.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            var response = new EmailTemplateResponse
            {
                Id = template.Id,
                Nome = template.Nome,
                Codigo = template.Codigo,
                Assunto = template.Assunto,
                Ativo = template.Ativo,
                DataCriacao = template.DataCriacao
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao atualizar template: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar template" });
        }
    }

    [HttpDelete("templates/{id}")]
    public async Task<ActionResult> DeleteTemplate(int id)
    {
        try
        {
            var template = await _context.EmailTemplates.FindAsync(id);

            if (template == null)
                return NotFound(new { message = "Template não encontrado" });

            // Não permitir deletar templates padrão
            if (template.Codigo == "CREDENCIAIS_ACESSO" || template.Codigo == "BEM_VINDO")
                return BadRequest(new { message = "Não é possível deletar templates padrão do sistema" });

            _context.EmailTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Template removido com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao deletar template: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao deletar template" });
        }
    }

    [HttpPost("templates/init-defaults")]
    public async Task<ActionResult> InitDefaultTemplates()
    {
        try
        {
            // Template de Credenciais
            if (!await _context.EmailTemplates.AnyAsync(t => t.Codigo == "CREDENCIAIS_ACESSO"))
            {
                var credenciaisTemplate = new EmailTemplate
                {
                    Nome = "Credenciais de Acesso",
                    Codigo = "CREDENCIAIS_ACESSO",
                    Assunto = "Suas credenciais de acesso - CNH Virtual",
                    CorpoHtml = @"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: #0081f2; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>🚗 CNH Virtual</h1>
    </div>
    <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333;'>Olá, {{NOME_CLIENTE}}!</h2>
        <p style='color: #666; line-height: 1.6;'>
            Seja bem-vindo(a) à CNH Virtual! Seu plano foi ativado com sucesso e você já pode começar seus estudos.
        </p>
        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #0081f2;'>
            <h3 style='margin-top: 0; color: #333;'>Suas Credenciais de Acesso:</h3>
            <p style='margin: 10px 0;'><strong>Login (CPF):</strong> {{CPF}}</p>
            <p style='margin: 10px 0;'><strong>Senha:</strong> {{SENHA}}</p>
        </div>
        <p style='color: #666;'>
            Acesse a plataforma através do link abaixo:
        </p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{{URL_ACESSO}}' style='background: #0081f2; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Acessar Plataforma
            </a>
        </div>
        <p style='color: #999; font-size: 14px; border-top: 1px solid #ddd; padding-top: 20px; margin-top: 30px;'>
            <strong>Importante:</strong> Guarde estas credenciais em local seguro. Recomendamos que você altere sua senha no primeiro acesso.
        </p>
    </div>
    <div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>
        <p>CNH Virtual - Sistema de Preparação para Habilitação</p>
        <p>Este é um email automático, por favor não responda.</p>
    </div>
</body>
</html>",
                    Ativo = true,
                    DataCriacao = DateTime.Now
                };
                _context.EmailTemplates.Add(credenciaisTemplate);
            }

            // Template de Boas-vindas
            if (!await _context.EmailTemplates.AnyAsync(t => t.Codigo == "BEM_VINDO"))
            {
                var boasVindasTemplate = new EmailTemplate
                {
                    Nome = "Boas-vindas",
                    Codigo = "BEM_VINDO",
                    Assunto = "Bem-vindo(a) à CNH Virtual!",
                    CorpoHtml = @"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: #0081f2; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>🚗 Bem-vindo(a)!</h1>
    </div>
    <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #333;'>Olá, {{NOME_CLIENTE}}!</h2>
        <p style='color: #666; line-height: 1.6;'>
            É um prazer ter você conosco na CNH Virtual! Estamos animados para acompanhar sua jornada rumo à habilitação.
        </p>
        <p style='color: #666; line-height: 1.6;'>
            Nossa plataforma oferece conteúdo completo para você estudar teoria, praticar com milhares de questões e fazer simulados ilimitados.
        </p>
        <p style='color: #666; line-height: 1.6;'>
            Se tiver qualquer dúvida, nossa equipe de suporte está à disposição para ajudar!
        </p>
        <div style='text-align: center; margin: 30px 0;'>
            <p style='color: #333; font-size: 18px; font-weight: bold;'>Bons estudos! 🎓</p>
        </div>
    </div>
    <div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>
        <p>CNH Virtual - Sistema de Preparação para Habilitação</p>
    </div>
</body>
</html>",
                    Ativo = true,
                    DataCriacao = DateTime.Now
                };
                _context.EmailTemplates.Add(boasVindasTemplate);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Templates padrão criados com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Erro ao criar templates padrão: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao criar templates padrão" });
        }
    }
}

// DTOs
public class SmtpConfigRequest
{
    public string? Host { get; set; }
    public string? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public bool UseSsl { get; set; } = true;
}

public class SmtpConfigResponse
{
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
}

public class TestEmailRequest
{
    public string EmailTeste { get; set; } = string.Empty;
}

public class EmailTemplateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Assunto { get; set; }
    public string? CorpoHtml { get; set; }
    public string? CorpoTexto { get; set; }
    public bool Ativo { get; set; } = true;
}

public class EmailTemplateResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class EmailTemplateDetailResponse : EmailTemplateResponse
{
    public string CorpoHtml { get; set; } = string.Empty;
    public string? CorpoTexto { get; set; }
}
