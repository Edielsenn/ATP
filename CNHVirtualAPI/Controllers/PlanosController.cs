using CNHVirtualAPI.Data;
using CNHVirtualAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PlanosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlanoResponse>>> GetPlanos()
    {
        try
        {
            var planos = await _context.Planos
                .Include(p => p.Recursos)
                .OrderBy(p => p.Ordem)
                .ToListAsync();

            var response = planos.Select(p => new PlanoResponse
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao ?? string.Empty,
                Preco = p.Preco,
                PrecoPromocional = p.PrecoPromocional,
                DuracaoDias = p.DuracaoDias,
                Destaque = p.Destaque,
                Ativo = p.Ativo,
                Recursos = p.Recursos.Where(r => r.Incluido).Select(r => r.Descricao).ToList()
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANOS] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar planos" });
        }
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStatus(int id)
    {
        try
        {
            var plano = await _context.Planos.FindAsync(id);

            if (plano == null)
                return NotFound(new { message = "Plano não encontrado" });

            plano.Ativo = !plano.Ativo;
            plano.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Plano {(plano.Ativo ? "ativado" : "desativado")} com sucesso", ativo = plano.Ativo });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANOS] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar status do plano" });
        }
    }
}

public class PlanoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public int DuracaoDias { get; set; }
    public bool Destaque { get; set; }
    public bool Ativo { get; set; }
    public List<string> Recursos { get; set; } = new();
}