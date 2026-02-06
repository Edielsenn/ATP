namespace CNHVirtualAPI.Models;

public class EmailTemplate
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty; // Ex: CREDENCIAIS_ACESSO, BEM_VINDO
    public string Assunto { get; set; } = string.Empty;
    public string CorpoHtml { get; set; } = string.Empty;
    public string? CorpoTexto { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAtualizacao { get; set; }
}
