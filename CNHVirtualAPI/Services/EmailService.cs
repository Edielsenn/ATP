using CNHVirtualAPI.Data;
using CNHVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace CNHVirtualAPI.Services;

public class EmailService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ApplicationDbContext context, ILogger<EmailService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> EnviarEmailCredenciais(Cliente cliente, string senha)
    {
        try
        {
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.Codigo == "CREDENCIAIS_ACESSO" && t.Ativo);

            if (template == null)
            {
                _logger.LogWarning("[EMAIL] Template CREDENCIAIS_ACESSO não encontrado");
                return false;
            }

            // Substituir variáveis no template
            var assunto = template.Assunto
                .Replace("{{NOME_CLIENTE}}", cliente.Nome);

            var corpo = template.CorpoHtml
                .Replace("{{NOME_CLIENTE}}", cliente.Nome)
                .Replace("{{CPF}}", cliente.CPF ?? "")
                .Replace("{{SENHA}}", senha)
                .Replace("{{URL_ACESSO}}", await GetConfigValue("URL_PLATAFORMA_ALUNO") ?? "https://aluno.cnhvirtual.com");

            return await EnviarEmail(cliente.Email, assunto, corpo);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[EMAIL] Erro ao enviar credenciais: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EnviarEmailBoasVindas(Cliente cliente)
    {
        try
        {
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.Codigo == "BEM_VINDO" && t.Ativo);

            if (template == null)
            {
                _logger.LogWarning("[EMAIL] Template BEM_VINDO não encontrado");
                return false;
            }

            var assunto = template.Assunto
                .Replace("{{NOME_CLIENTE}}", cliente.Nome);

            var corpo = template.CorpoHtml
                .Replace("{{NOME_CLIENTE}}", cliente.Nome);

            return await EnviarEmail(cliente.Email, assunto, corpo);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[EMAIL] Erro ao enviar boas-vindas: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EnviarEmail(string destinatario, string assunto, string corpoHtml, string? corpoTexto = null)
    {
        try
        {
            var smtpHost = await GetConfigValue("SMTP_HOST");
            var smtpPort = await GetConfigValue("SMTP_PORT");
            var smtpUsername = await GetConfigValue("SMTP_USERNAME");
            var smtpPassword = await GetConfigValue("SMTP_PASSWORD");
            var smtpFromEmail = await GetConfigValue("SMTP_FROM_EMAIL");
            var smtpFromName = await GetConfigValue("SMTP_FROM_NAME");
            var smtpUseSsl = await GetConfigValue("SMTP_USE_SSL");

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) ||
                string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("[EMAIL] Configurações SMTP não encontradas");
                return false;
            }

            using var client = new SmtpClient(smtpHost, int.Parse(smtpPort))
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = smtpUseSsl?.ToLower() == "true"
            };

            var message = new MailMessage
            {
                From = new MailAddress(smtpFromEmail ?? smtpUsername, smtpFromName ?? "CNH Virtual"),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true
            };

            message.To.Add(destinatario);

            if (!string.IsNullOrEmpty(corpoTexto))
            {
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(corpoTexto, null, "text/plain"));
            }

            await client.SendMailAsync(message);

            _logger.LogInformation($"[EMAIL] Email enviado com sucesso para {destinatario}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[EMAIL] Erro ao enviar email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TestarConfiguracao(string emailTeste)
    {
        try
        {
            var assunto = "Teste de Configuração SMTP - CNH Virtual";
            var corpo = @"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #0081f2;'>✓ Configuração SMTP Funcionando!</h2>
                    <p>Este é um email de teste enviado pelo sistema CNH Virtual.</p>
                    <p>Se você recebeu este email, significa que suas configurações SMTP estão corretas.</p>
                    <hr>
                    <small style='color: #666;'>CNH Virtual - Sistema de Gestão</small>
                </body>
                </html>";

            return await EnviarEmail(emailTeste, assunto, corpo);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[EMAIL] Erro ao testar configuração: {ex.Message}");
            return false;
        }
    }

    private async Task<string?> GetConfigValue(string key)
    {
        var config = await _context.Configuracoes
            .FirstOrDefaultAsync(c => c.Chave == key);
        return config?.Valor;
    }
}
