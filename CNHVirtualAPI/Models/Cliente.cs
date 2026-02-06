namespace CNHVirtualAPI.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? CPF { get; set; }
    public DateTime? DataNascimento { get; set; }

    // Autenticação
    public string? SenhaHash { get; set; }

    // Endereço
    public string? CEP { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }

    // Controle
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;

    // Navigation properties
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
}
