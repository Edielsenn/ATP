using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CNHVirtualAPI.Data;
using CNHVirtualAPI.DTOs;
using CNHVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace CNHVirtualAPI.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        Console.WriteLine($"[AUTH] Tentativa de login - Email: {request.Email}");

        var admin = await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Email == request.Email && a.Ativo);

        if (admin == null)
        {
            Console.WriteLine($"[AUTH] Usuário não encontrado ou inativo");
            return null;
        }

        Console.WriteLine($"[AUTH] Usuário encontrado: {admin.Email}");
        Console.WriteLine($"[AUTH] Senha recebida: '{request.Senha}' (length: {request.Senha?.Length})");
        Console.WriteLine($"[AUTH] Hash no banco: {admin.SenhaHash}");

        // Verificar senha
        bool senhaCorreta = BCrypt.Net.BCrypt.Verify(request.Senha, admin.SenhaHash);
        Console.WriteLine($"[AUTH] Verificação de senha: {senhaCorreta}");

        if (!senhaCorreta)
            return null;

        // Gerar token JWT
        var token = GerarToken(admin);

        return new LoginResponse
        {
            Token = token,
            Nome = admin.Nome,
            Email = admin.Email,
            Expiracao = DateTime.Now.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"))
        };
    }

    private string GerarToken(AdminUser admin)
    {
        var secret = _configuration["Jwt:Secret"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expirationHours = double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Nome),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
