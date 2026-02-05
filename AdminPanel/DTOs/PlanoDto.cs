namespace CNHVirtualAdmin.DTOs;

public class PlanoDto
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
