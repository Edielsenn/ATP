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
                .Where(p => p.Ativo)
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

    [HttpGet("{id}")]
    public async Task<ActionResult<PlanoResponse>> GetPlano(int id)
    {
        try
        {
            var plano = await _context.Planos
                .Include(p => p.Recursos)
                .Where(p => p.Id == id && p.Ativo)
                .FirstOrDefaultAsync();

            if (plano == null)
                return NotFound(new { message = "Plano não encontrado" });

            var response = new PlanoResponse
            {
                Id = plano.Id,
                Nome = plano.Nome,
                Descricao = plano.Descricao ?? string.Empty,
                Preco = plano.Preco,
                PrecoPromocional = plano.PrecoPromocional,
                DuracaoDias = plano.DuracaoDias,
                Destaque = plano.Destaque,
                Ativo = plano.Ativo,
                Recursos = plano.Recursos.Where(r => r.Incluido).Select(r => r.Descricao).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANOS] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao buscar plano" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<PlanoResponse>> CreatePlano([FromBody] PlanoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome do plano é obrigatório" });

            if (request.Preco <= 0)
                return BadRequest(new { message = "Preço deve ser maior que zero" });

            if (request.DuracaoDias <= 0)
                return BadRequest(new { message = "Duração deve ser maior que zero" });

            var plano = new Plano
            {
                Nome = request.Nome,
                Descricao = request.Descricao ?? string.Empty,
                DescricaoCurta = request.DescricaoCurta ?? string.Empty,
                Preco = request.Preco,
                PrecoPromocional = request.PrecoPromocional,
                DuracaoDias = request.DuracaoDias,
                ValidadeDias = request.ValidadeDias ?? 0,
                LimiteTentativas = request.LimiteTentativas ?? 0,
                Destaque = request.Destaque,
                Ativo = request.Ativo,
                Ordem = request.Ordem,
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            if (request.Recursos != null && request.Recursos.Any())
            {
                foreach (var recurso in request.Recursos)
                {
                    plano.Recursos.Add(new PlanoRecurso
                    {
                        Descricao = recurso.Descricao,
                        Incluido = recurso.Incluido,
                        Ordem = recurso.Ordem
                    });
                }
            }

            _context.Planos.Add(plano);
            await _context.SaveChangesAsync();

            var response = new PlanoResponse
            {
                Id = plano.Id,
                Nome = plano.Nome,
                Descricao = plano.Descricao,
                Preco = plano.Preco,
                PrecoPromocional = plano.PrecoPromocional,
                DuracaoDias = plano.DuracaoDias,
                Destaque = plano.Destaque,
                Ativo = plano.Ativo,
                Recursos = plano.Recursos.Where(r => r.Incluido).Select(r => r.Descricao).ToList()
            };

            return CreatedAtAction(nameof(GetPlano), new { id = plano.Id }, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANOS] Erro ao criar plano: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao criar plano" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlanoResponse>> UpdatePlano(int id, [FromBody] PlanoRequest request)
    {
        try
        {
            var plano = await _context.Planos
                .Include(p => p.Recursos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plano == null)
                return NotFound(new { message = "Plano não encontrado" });

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { message = "Nome do plano é obrigatório" });

            if (request.Preco <= 0)
                return BadRequest(new { message = "Preço deve ser maior que zero" });

            if (request.DuracaoDias <= 0)
                return BadRequest(new { message = "Duração deve ser maior que zero" });

            plano.Nome = request.Nome;
            plano.Descricao = request.Descricao ?? string.Empty;
            plano.DescricaoCurta = request.DescricaoCurta ?? string.Empty;
            plano.Preco = request.Preco;
            plano.PrecoPromocional = request.PrecoPromocional;
            plano.DuracaoDias = request.DuracaoDias;
            plano.ValidadeDias = request.ValidadeDias ?? 0;
            plano.LimiteTentativas = request.LimiteTentativas ?? 0;
            plano.Destaque = request.Destaque;
            plano.Ativo = request.Ativo;
            plano.Ordem = request.Ordem;
            plano.DataAtualizacao = DateTime.Now;

            // Atualizar recursos
            if (request.Recursos != null)
            {
                // Remover recursos existentes
                _context.PlanoRecursos.RemoveRange(plano.Recursos);

                // Adicionar novos recursos
                plano.Recursos.Clear();
                foreach (var recurso in request.Recursos)
                {
                    plano.Recursos.Add(new PlanoRecurso
                    {
                        PlanoId = plano.Id,
                        Descricao = recurso.Descricao,
                        Incluido = recurso.Incluido,
                        Ordem = recurso.Ordem
                    });
                }
            }

            await _context.SaveChangesAsync();

            var response = new PlanoResponse
            {
                Id = plano.Id,
                Nome = plano.Nome,
                Descricao = plano.Descricao,
                Preco = plano.Preco,
                PrecoPromocional = plano.PrecoPromocional,
                DuracaoDias = plano.DuracaoDias,
                Destaque = plano.Destaque,
                Ativo = plano.Ativo,
                Recursos = plano.Recursos.Where(r => r.Incluido).Select(r => r.Descricao).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANOS] Erro ao atualizar plano: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao atualizar plano" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlano(int id)
    {
        try
        {
            var plano = await _context.Planos.FindAsync(id);

            if (plano == null)
                return NotFound(new { message = "Plano não encontrado" });

            // Verificar se o plano tem pedidos ou assinaturas associados
            var temPedidos = await _context.Pedidos.AnyAsync(p => p.PlanoId == id);
            var temAssinaturas = await _context.Assinaturas.AnyAsync(a => a.PlanoId == id);

            if (temPedidos || temAssinaturas)
            {
                // Soft delete - apenas desativar o plano
                plano.Ativo = false;
                plano.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Plano desativado com sucesso (possui pedidos/assinaturas associados)" });
            }

            // Hard delete - remover completamente
            _context.Planos.Remove(plano);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Plano removido com sucesso" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANOS] Erro ao deletar plano: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao deletar plano" });
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

public class PlanoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? DescricaoCurta { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public int DuracaoDias { get; set; }
    public int? ValidadeDias { get; set; }
    public int? LimiteTentativas { get; set; }
    public bool Destaque { get; set; } = false;
    public bool Ativo { get; set; } = true;
    public int Ordem { get; set; } = 0;
    public List<RecursoRequest>? Recursos { get; set; }
}

public class RecursoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public bool Incluido { get; set; } = true;
    public int Ordem { get; set; } = 0;
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