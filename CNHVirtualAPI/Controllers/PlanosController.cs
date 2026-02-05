using CNHVirtualAPI.Data;
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
    public async Task<IActionResult> GetPlanos()
    {
        var planos = await _context.Planos
            .Include(p => p.Recursos.OrderBy(r => r.Ordem))
            .Where(p => p.Ativo)
            .OrderBy(p => p.Ordem)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                p.Descricao,
                p.DescricaoCurta,
                p.Preco,
                p.PrecoPromocional,
                p.DuracaoDias,
                p.ValidadeDias,
                p.LimiteTentativas,
                p.Destaque,
                p.Ordem,
                Recursos = p.Recursos.Select(r => new
                {
                    r.Id,
                    r.Descricao,
                    r.Incluido,
                    r.Ordem
                }).ToList()
            })
            .ToListAsync();

        return Ok(planos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlano(int id)
    {
        var plano = await _context.Planos
            .Include(p => p.Recursos.OrderBy(r => r.Ordem))
            .Where(p => p.Id == id && p.Ativo)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                p.Descricao,
                p.DescricaoCurta,
                p.Preco,
                p.PrecoPromocional,
                p.DuracaoDias,
                p.ValidadeDias,
                p.LimiteTentativas,
                p.Destaque,
                Recursos = p.Recursos.Select(r => new
                {
                    r.Id,
                    r.Descricao,
                    r.Incluido,
                    r.Ordem
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (plano == null)
            return NotFound(new { mensagem = "Plano não encontrado" });

        return Ok(plano);
    }
}
