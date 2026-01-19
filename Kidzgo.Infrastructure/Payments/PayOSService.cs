using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kidzgo.Application.Abstraction.Payments;
using Microsoft.Extensions.Options;

namespace Kidzgo.Infrastructure.Payments;

public sealed class PayOSService : IPayOSService
{
    private readonly HttpClient _httpClient;
    private readonly PayOSOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PayOSService(HttpClient httpClient, IOptions<PayOSOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-client-id", _options.ClientId.ToString());
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
    }

    public async Task<PayOSCreateLinkResponse> CreatePaymentLinkAsync(
        PayOSCreateLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            orderCode = request.OrderCode,
            amount = request.Amount,
            description = request.Description,
            items = request.Items,
            cancelUrl = request.CancelUrl,
            returnUrl = request.ReturnUrl,
            expiredAt = request.ExpiredAt
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v2/payment-requests", content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new PayOSCreateLinkResponse
            {
                Error = (int)response.StatusCode,
                Message = $"PayOS API error: {responseContent}"
            };
        }

        var result = JsonSerializer.Deserialize<PayOSCreateLinkResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result ?? new PayOSCreateLinkResponse
        {
            Error = -1,
            Message = "Failed to parse PayOS response"
        };
    }

    public bool VerifyWebhookSignature(string signature, string data)
    {
        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var computedSignature = Convert.ToHexString(hashBytes).ToLower();

            return string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
