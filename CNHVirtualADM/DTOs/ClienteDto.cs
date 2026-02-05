namespace CNHVirtualADM.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public int TotalPedidos { get; set; }
    public int AssinaturasAtivas { get; set; }
    public string? AsaasCustomerId { get; set; }
}
