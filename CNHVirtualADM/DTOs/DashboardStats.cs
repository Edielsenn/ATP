namespace CNHVirtualADM.DTOs;

public class DashboardStats
{
    public int TotalClientes { get; set; }
    public int TotalPedidos { get; set; }
    public int PagamentosPendentes { get; set; }
    public int PagamentosConfirmados { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal ReceitaMes { get; set; }
    public int AssinaturasAtivas { get; set; }
    public List<VendaDiaria> VendasUltimos7Dias { get; set; } = new();
}

public class VendaDiaria
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
}
