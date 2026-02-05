namespace CNHVirtualAPI.Models;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int PlanoId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal ValorDesconto { get; set; } = 0;
    public decimal ValorFinal { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, CONFIRMED, CANCELLED
    public DateTime DataPedido { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;

    // Navigation properties
    public Cliente Cliente { get; set; } = null!;
    public Plano Plano { get; set; } = null!;
    public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
}
