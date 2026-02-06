namespace CNHVirtualAPI.Models;

public class WebhookSubscription
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Eventos { get; set; } = string.Empty; // JSON array de eventos
    public bool Ativo { get; set; } = true;
    public string? Secret { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAtualizacao { get; set; }
}
