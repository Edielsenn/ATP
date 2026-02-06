using CNHVirtualAPI.Data;
using CNHVirtualAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfiguracoesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ConfiguracoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("asaas")]
    public async Task<ActionResult<AsaasConfigResponse>> GetAsaasConfig()
    {
        try
        {
            var apiKeyConfig = await _context.Configuracoes
                .FirstOrDefaultAsync(c => c.Chave == "ASAAS_API_KEY");

            var sandboxConfig = await _context.Configuracoes
                .FirstOrDefaultAsync(c => c.Chave == "ASAAS_IS_SANDBOX");

            if (apiKeyConfig == null)
            {
                return NotFound(new { message = "Configuração não encontrada" });
            }

            var response = new AsaasConfigResponse
            {
                ApiKey = apiKeyConfig.Valor,
                IsSandbox = sandboxConfig?.Valor == "true"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIG] Erro ao buscar: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar configuração" });
        }
    }

    [HttpPost("asaas")]
    public async Task<ActionResult> SaveAsaasConfig([FromBody] AsaasConfigRequest request)
    {
        try
        {
            // Atualizar ou criar API Key
            var apiKeyConfig = await _context.Configuracoes
                .FirstOrDefaultAsync(c => c.Chave == "ASAAS_API_KEY");

            if (apiKeyConfig == null)
            {
                apiKeyConfig = new Configuracao
                {
                    Chave = "ASAAS_API_KEY",
                    Valor = request.ApiKey,
                    DataCriacao = DateTime.Now
                };
                _context.Configuracoes.Add(apiKeyConfig);
            }
            else
            {
                apiKeyConfig.Valor = request.ApiKey;
                apiKeyConfig.DataAtualizacao = DateTime.Now;
            }

            // Atualizar ou criar configuração de Sandbox
            var sandboxConfig = await _context.Configuracoes
                .FirstOrDefaultAsync(c => c.Chave == "ASAAS_IS_SANDBOX");

            if (sandboxConfig == null)
            {
                sandboxConfig = new Configuracao
                {
                    Chave = "ASAAS_IS_SANDBOX",
                    Valor = request.IsSandbox.ToString().ToLower(),
                    DataCriacao = DateTime.Now
                };
                _context.Configuracoes.Add(sandboxConfig);
            }
            else
            {
                sandboxConfig.Valor = request.IsSandbox.ToString().ToLower();
                sandboxConfig.DataAtualizacao = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"[CONFIG] Configuração ASAAS salva com sucesso");

            return Ok(new { message = "Configuração salva com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIG] Erro ao salvar: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao salvar configuração" });
        }
    }

    [HttpGet("get")]
    public async Task<ActionResult<ConfigValueResponse>> GetConfig([FromQuery] string key)
    {
        try
        {
            var config = await _context.Configuracoes
                .FirstOrDefaultAsync(c => c.Chave == key);

            if (config == null)
            {
                return Ok(new ConfigValueResponse { Valor = null });
            }

            return Ok(new ConfigValueResponse { Valor = config.Valor });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIG] Erro ao buscar {key}: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar configuração" });
        }
    }

    [HttpPost("set")]
    public async Task<ActionResult> SetConfig([FromBody] ConfigSetRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Key))
                return BadRequest(new { message = "Chave é obrigatória" });

            var config = await _context.Configuracoes
                .FirstOrDefaultAsync(c => c.Chave == request.Key);

            if (config == null)
            {
                config = new Configuracao
                {
                    Chave = request.Key,
                    Valor = request.Value ?? string.Empty,
                    DataCriacao = DateTime.Now
                };
                _context.Configuracoes.Add(config);
            }
            else
            {
                config.Valor = request.Value ?? string.Empty;
                config.DataAtualizacao = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Configuração salva com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONFIG] Erro ao salvar {request.Key}: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao salvar configuração" });
        }
    }
}

public class AsaasConfigRequest
{
    public string ApiKey { get; set; } = string.Empty;
    public bool IsSandbox { get; set; }
}

public class AsaasConfigResponse
{
    public string ApiKey { get; set; } = string.Empty;
    public bool IsSandbox { get; set; }
}

public class ConfigValueResponse
{
    public string? Valor { get; set; }
}

public class ConfigSetRequest
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}
