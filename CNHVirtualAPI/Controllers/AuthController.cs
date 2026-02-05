using CNHVirtualAPI.DTOs;
using CNHVirtualAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _authService.LoginAsync(request);

            if (resultado == null)
                return Unauthorized(new { mensagem = "Email ou senha inválidos" });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login");
            return StatusCode(500, new { mensagem = "Erro interno ao fazer login" });
        }
    }

    // Endpoint temporário para gerar hash de senha
    [HttpGet("generate-hash/{password}")]
    public IActionResult GenerateHash(string password)
    {
        var hash = AuthService.HashPassword(password);
        return Ok(new { password, hash });
    }
}
