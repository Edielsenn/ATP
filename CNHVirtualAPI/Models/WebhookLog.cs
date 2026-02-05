namespace CNHVirtualAPI.Models;

public class WebhookLog
{
    public int Id { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public bool ProcessadoComSucesso { get; set; } = false;
    public string? MensagemErro { get; set; }
    public DateTime DataRecebimento { get; set; } = DateTime.Now;
    public DateTime? DataProcessamento { get; set; }
}
