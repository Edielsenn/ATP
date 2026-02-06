namespace CNHVirtualAPI.Models;

public class Configuracao
{
    public int Id { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public DateTime? DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}
