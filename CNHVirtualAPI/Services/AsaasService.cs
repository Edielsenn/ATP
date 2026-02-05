using System.Text;
using System.Text.Json;
using CNHVirtualAPI.DTOs;

namespace CNHVirtualAPI.Services;

public class AsaasService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AsaasService> _logger;

    public AsaasService(HttpClient httpClient, IConfiguration configuration, ILogger<AsaasService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var apiKey = _configuration["Asaas:ApiKey"];
        var apiUrl = _configuration["Asaas:ApiUrl"];

        _httpClient.BaseAddress = new Uri(apiUrl!);
        _httpClient.DefaultRequestHeaders.Add("access_token", apiKey);
    }

    public async Task<AsaasCustomerResponse?> CriarClienteAsync(ClienteDto cliente)
    {
        try
        {
            var payload = new
            {
                name = cliente.Nome,
                email = cliente.Email,
                cpfCnpj = cliente.CPF.Replace(".", "").Replace("-", ""),
                phone = cliente.Telefone,
                mobilePhone = cliente.Telefone,
                postalCode = cliente.CEP.Replace("-", ""),
                address = cliente.Endereco,
                addressNumber = cliente.Numero,
                complement = cliente.Complemento,
                province = cliente.Bairro,
                notificationDisabled = false
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/customers", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao criar cliente no ASAAS: {responseContent}");
                return null;
            }

            return JsonSerializer.Deserialize<AsaasCustomerResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente no ASAAS");
            return null;
        }
    }

    public async Task<AsaasPaymentResponse?> CriarPagamentoBoletoAsync(
        string customerId,
        decimal valor,
        DateTime dataVencimento,
        string descricao)
    {
        try
        {
            var splitPercentage = decimal.Parse(_configuration["Asaas:SplitPercentage"] ?? "0");
            var splitWalletId = _configuration["Asaas:SplitWalletId"];

            var payload = new
            {
                customer = customerId,
                billingType = "BOLETO",
                value = valor,
                dueDate = dataVencimento.ToString("yyyy-MM-dd"),
                description = descricao,
                split = splitPercentage > 0 && !string.IsNullOrEmpty(splitWalletId) ? new[]
                {
                    new
                    {
                        walletId = splitWalletId,
                        percentualValue = splitPercentage
                    }
                } : null
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/payments", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao criar pagamento no ASAAS: {responseContent}");
                return null;
            }

            return JsonSerializer.Deserialize<AsaasPaymentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pagamento boleto no ASAAS");
            return null;
        }
    }

    public async Task<AsaasPaymentResponse?> CriarPagamentoCartaoAsync(
        string customerId,
        decimal valor,
        string descricao,
        CartaoDto cartao)
    {
        try
        {
            var splitPercentage = decimal.Parse(_configuration["Asaas:SplitPercentage"] ?? "0");
            var splitWalletId = _configuration["Asaas:SplitWalletId"];

            var payload = new
            {
                customer = customerId,
                billingType = "CREDIT_CARD",
                value = valor,
                dueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                description = descricao,
                creditCard = new
                {
                    holderName = cartao.HolderName,
                    number = cartao.Number.Replace(" ", ""),
                    expiryMonth = cartao.ExpiryMonth,
                    expiryYear = cartao.ExpiryYear,
                    ccv = cartao.Ccv
                },
                creditCardHolderInfo = new
                {
                    name = cartao.HolderName,
                    cpfCnpj = "",
                    postalCode = "",
                    addressNumber = "",
                    phone = ""
                },
                split = splitPercentage > 0 && !string.IsNullOrEmpty(splitWalletId) ? new[]
                {
                    new
                    {
                        walletId = splitWalletId,
                        percentualValue = splitPercentage
                    }
                } : null
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/payments", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao criar pagamento cartão no ASAAS: {responseContent}");
                return null;
            }

            return JsonSerializer.Deserialize<AsaasPaymentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pagamento cartão no ASAAS");
            return null;
        }
    }

    public async Task<AsaasPaymentResponse?> ConsultarPagamentoAsync(string paymentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/payments/{paymentId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao consultar pagamento no ASAAS: {responseContent}");
                return null;
            }

            return JsonSerializer.Deserialize<AsaasPaymentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pagamento no ASAAS");
            return null;
        }
    }
}

// DTOs para respostas do ASAAS
public class AsaasCustomerResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AsaasPaymentResponse
{
    public string Id { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string BillingType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InvoiceUrl { get; set; }
    public string? BankSlipUrl { get; set; }
    public string? IdentificationField { get; set; }
    public string? Barcode { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ClientPaymentDate { get; set; }
    public string? CreditCardBrand { get; set; }
    public string? CreditCardNumber { get; set; }
}
