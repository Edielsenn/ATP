namespace CNHVirtualAPI.Models;

public class Assinatura
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int PlanoId { get; set; }
    public int PedidoId { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, EXPIRED, CANCELLED
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public int TentativasUsadas { get; set; } = 0;
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;

    // Navigation properties
    public Cliente Cliente { get; set; } = null!;
    public Plano Plano { get; set; } = null!;
    public Pedido Pedido { get; set; } = null!;
}
