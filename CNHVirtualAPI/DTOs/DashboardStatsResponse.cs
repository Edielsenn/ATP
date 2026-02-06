namespace CNHVirtualAPI.DTOs;

public class DashboardStatsResponse
{
    public int TotalClientes { get; set; }
    public int TotalPedidos { get; set; }
    public int PagamentosPendentes { get; set; }
    public int PagamentosConfirmados { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal ReceitaMes { get; set; }
    public int AssinaturasAtivas { get; set; }
    public List<VendaDiariaDto> VendasUltimos7Dias { get; set; } = new();
}

public class VendaDiariaDto
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
}
