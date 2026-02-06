using CNHVirtualAPI.Data;
using CNHVirtualAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminUsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminUsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminUserResponse>>> GetAdminUsers()
    {
        try
        {
            var users = await _context.AdminUsers
                .OrderBy(u => u.Nome)
                .ToListAsync();

            var response = users.Select(u => new AdminUserResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Ativo = u.Ativo,
                DataCriacao = u.DataCriacao
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADMIN_USERS] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar usuários" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminUserResponse>> GetAdminUser(int id)
    {
        try
        {
            var user = await _context.AdminUsers.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            var response = new AdminUserResponse
            {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADMIN_USERS] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar usuário" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserResponse>> CreateAdminUser([FromBody] AdminUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome é obrigatório" });

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email é obrigatório" });

            if (string.IsNullOrWhiteSpace(request.Senha))
                return BadRequest(new { message = "Senha é obrigatória" });

            // Verificar se email já existe
            var existingUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                return BadRequest(new { message = "Email já cadastrado" });

            // Hash da senha
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            var user = new AdminUser
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = senhaHash,
                Ativo = request.Ativo,
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _context.AdminUsers.Add(user);
            await _context.SaveChangesAsync();

            var response = new AdminUserResponse
            {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao
            };

            return CreatedAtAction(nameof(GetAdminUser), new { id = user.Id }, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADMIN_USERS] Erro ao criar usuário: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao criar usuário" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminUserResponse>> UpdateAdminUser(int id, [FromBody] AdminUserUpdateRequest request)
    {
        try
        {
            var user = await _context.AdminUsers.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome é obrigatório" });

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email é obrigatório" });

            // Verificar se email já existe em outro usuário
            var existingUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != id);

            if (existingUser != null)
                return BadRequest(new { message = "Email já cadastrado" });

            user.Nome = request.Nome;
            user.Email = request.Email;
            user.Ativo = request.Ativo;
            user.DataAtualizacao = DateTime.Now;

            // Se senha foi informada, atualiza o hash
            if (!string.IsNullOrWhiteSpace(request.NovaSenha))
            {
                user.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
            }

            await _context.SaveChangesAsync();

            var response = new AdminUserResponse
            {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADMIN_USERS] Erro ao atualizar usuário: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar usuário" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAdminUser(int id)
    {
        try
        {
            var user = await _context.AdminUsers.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            // Não permite deletar se for o último usuário ativo
            var activeUsersCount = await _context.AdminUsers.CountAsync(u => u.Ativo);
            if (activeUsersCount == 1 && user.Ativo)
            {
                return BadRequest(new { message = "Não é possível deletar o último usuário ativo do sistema" });
            }

            _context.AdminUsers.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuário removido com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADMIN_USERS] Erro ao deletar usuário: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao deletar usuário" });
        }
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStatus(int id)
    {
        try
        {
            var user = await _context.AdminUsers.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            // Se está tentando desativar, verifica se não é o último usuário ativo
            if (user.Ativo)
            {
                var activeUsersCount = await _context.AdminUsers.CountAsync(u => u.Ativo);
                if (activeUsersCount == 1)
                {
                    return BadRequest(new { message = "Não é possível desativar o último usuário ativo do sistema" });
                }
            }

            user.Ativo = !user.Ativo;
            user.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Usuário {(user.Ativo ? "ativado" : "desativado")} com sucesso", ativo = user.Ativo });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADMIN_USERS] Erro ao alternar status: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar status do usuário" });
        }
    }
}

public class AdminUserRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public class AdminUserUpdateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NovaSenha { get; set; }
    public bool Ativo { get; set; } = true;
}

public class AdminUserResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}
