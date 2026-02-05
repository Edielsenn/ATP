namespace CNHVirtualAPI.Models;

public class PlanoRecurso
{
    public int Id { get; set; }
    public int PlanoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Incluido { get; set; } = true;
    public int Ordem { get; set; } = 0;

    // Navigation properties
    public Plano Plano { get; set; } = null!;
}
