namespace CNHVirtualAPI.DTOs;

public class PagamentoResponse
{
    public int PagamentoId { get; set; }
    public int PedidoId { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal Valor { get; set; }

    // Dados do Boleto (se aplicável)
    public string? BoletoUrl { get; set; }
    public string? LinhaDigitavel { get; set; }
    public DateTime? DataVencimento { get; set; }

    // Mensagem
    public string Mensagem { get; set; } = string.Empty;
}
