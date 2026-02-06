namespace CNHVirtualAPI.Models;

public class Plano
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? DescricaoCurta { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public int DuracaoDias { get; set; }
    public int? ValidadeDias { get; set; }
    public int? LimiteTentativas { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Destaque { get; set; } = false;
    public int Ordem { get; set; } = 0;
    public DateTime? DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    // Navigation properties
    public ICollection<PlanoRecurso> Recursos { get; set; } = new List<PlanoRecurso>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
}
