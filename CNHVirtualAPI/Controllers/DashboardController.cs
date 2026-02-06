using CNHVirtualAPI.Data;
using CNHVirtualAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsResponse>> GetDashboardStats()
    {
        try
        {
            var now = DateTime.Now;
            var primeiroMes = new DateTime(now.Year, now.Month, 1);
            var ultimos7Dias = now.AddDays(-6).Date;

            // Total de clientes
            var totalClientes = await _context.Clientes.CountAsync();

            // Total de pedidos
            var totalPedidos = await _context.Pedidos.CountAsync();

            // Pagamentos
            var pagamentosPendentes = await _context.Pagamentos
                .CountAsync(p => p.Status == "PENDING");

            var pagamentosConfirmados = await _context.Pagamentos
                .CountAsync(p => p.Status == "CONFIRMED" || p.Status == "RECEIVED");

            // Receita total
            var receitaTotal = await _context.Pagamentos
                .Where(p => p.Status == "CONFIRMED" || p.Status == "RECEIVED")
                .SumAsync(p => (decimal?)p.Valor) ?? 0m;

            // Receita do mês
            var receitaMes = await _context.Pagamentos
                .Where(p => (p.Status == "CONFIRMED" || p.Status == "RECEIVED") &&
                           p.DataPagamento.HasValue && p.DataPagamento.Value >= primeiroMes)
                .SumAsync(p => (decimal?)p.Valor) ?? 0m;

            // Assinaturas ativas
            var assinaturasAtivas = await _context.Assinaturas
                .CountAsync(a => a.Status == "ACTIVE" && a.DataFim > now);

            // Vendas dos últimos 7 dias
            var vendasUltimos7Dias = await _context.Pagamentos
                .Where(p => (p.Status == "CONFIRMED" || p.Status == "RECEIVED") &&
                           p.DataPagamento.HasValue && p.DataPagamento.Value.Date >= ultimos7Dias)
                .GroupBy(p => p.DataPagamento.Value.Date)
                .Select(g => new VendaDiariaDto
                {
                    Data = g.Key,
                    Valor = g.Sum(p => p.Valor),
                    Quantidade = g.Count()
                })
                .OrderBy(v => v.Data)
                .ToListAsync();

            // Preencher dias sem vendas
            var vendasCompletas = new List<VendaDiariaDto>();
            for (int i = 6; i >= 0; i--)
            {
                var data = now.AddDays(-i).Date;
                var venda = vendasUltimos7Dias.FirstOrDefault(v => v.Data == data);

                vendasCompletas.Add(venda ?? new VendaDiariaDto
                {
                    Data = data,
                    Valor = 0,
                    Quantidade = 0
                });
            }

            var stats = new DashboardStatsResponse
            {
                TotalClientes = totalClientes,
                TotalPedidos = totalPedidos,
                PagamentosPendentes = pagamentosPendentes,
                PagamentosConfirmados = pagamentosConfirmados,
                ReceitaTotal = receitaTotal,
                ReceitaMes = receitaMes,
                AssinaturasAtivas = assinaturasAtivas,
                VendasUltimos7Dias = vendasCompletas
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] Erro: {ex.Message}");
            return StatusCode(500, new { message = "Erro ao carregar estatísticas" });
        }
    }
}
