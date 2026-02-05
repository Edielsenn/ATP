namespace CNHVirtualAPI.Models;

public class Pagamento
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public string? AsaasPaymentId { get; set; }
    public string FormaPagamento { get; set; } = string.Empty; // BOLETO, CREDIT_CARD, PIX
    public string Status { get; set; } = "PENDING"; // PENDING, CONFIRMED, RECEIVED, OVERDUE, REFUNDED, CANCELLED
    public decimal Valor { get; set; }
    public decimal? ValorRecebido { get; set; }
    public DateTime? DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public DateTime? DataConfirmacao { get; set; }

    // Dados do Boleto
    public string? BoletoUrl { get; set; }
    public string? LinhaDigitavel { get; set; }
    public string? CodigoBarras { get; set; }

    // Dados do Cartão
    public string? CartaoBandeira { get; set; }
    public string? CartaoUltimosDigitos { get; set; }

    // Controle
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;

    // Navigation properties
    public Pedido Pedido { get; set; } = null!;
}
