namespace CNHVirtualAPI.DTOs;

public class PagamentoRequest
{
    public int PlanoId { get; set; }
    public ClienteDto Cliente { get; set; } = new();
    public string FormaPagamento { get; set; } = string.Empty; // BOLETO, CREDIT_CARD
    public CartaoDto? Cartao { get; set; }
}

public class ClienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class CartaoDto
{
    public string HolderName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Ccv { get; set; } = string.Empty;
}
