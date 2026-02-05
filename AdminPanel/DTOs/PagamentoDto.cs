namespace CNHVirtualAdmin.DTOs;

public class PagamentoDto
{
    public int Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string PlanoNome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public string? AsaasPaymentId { get; set; }
    public string? BoletoUrl { get; set; }
    public string? LinhaDigitavel { get; set; }
}
