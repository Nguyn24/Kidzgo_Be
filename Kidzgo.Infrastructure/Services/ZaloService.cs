using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Infrastructure.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kidzgo.Infrastructure.Services;

/// <summary>
/// Service for sending messages via Zalo Official Account
/// UC-328: Gửi Notification qua Zalo OA
/// </summary>
public class ZaloService : IZaloService
{
    private readonly ZaloSettings _settings;
    private readonly ILogger<ZaloService> _logger;
    private readonly HttpClient _httpClient;

    public ZaloService(
        IOptions<ZaloSettings> settings,
        ILogger<ZaloService> logger,
        HttpClient httpClient)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl ?? "https://openapi.zalo.me/v2.0");
    }

    public async Task<bool> SendMessageAsync(
        string zaloUserId,
        string message,
        CancellationToken cancellationToken = default)
    {
        return await SendMessageWithDeeplinkAsync(zaloUserId, message, null, cancellationToken);
    }

    public async Task<bool> SendMessageWithDeeplinkAsync(
        string zaloUserId,
        string message,
        string? deeplink = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.OAId) || 
                string.IsNullOrWhiteSpace(_settings.AppId) ||
                string.IsNullOrWhiteSpace(_settings.AppSecret))
            {
                _logger.LogWarning("Zalo configuration is missing. Cannot send message.");
                return false;
            }

            // Get access token (you may need to implement token caching)
            var accessToken = await GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("Failed to get Zalo access token");
                return false;
            }

            // Build message payload
            var payload = new
            {
                recipient = new { user_id = zaloUserId },
                message = new
                {
                    text = message,
                    attachment = !string.IsNullOrWhiteSpace(deeplink) ? new
                    {
                        type = "template",
                        payload = new
                        {
                            template_type = "button",
                            text = message,
                            buttons = new[]
                            {
                                new
                                {
                                    type = "web_url",
                                    title = "Xem chi tiết",
                                    url = deeplink
                                }
                            }
                        }
                    } : null
                }
            };

            var url = $"/oa/v3/message?access_token={accessToken}";
            var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation(
                    "Zalo message sent successfully to user {ZaloUserId}. Response: {Response}",
                    zaloUserId, responseContent);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Failed to send Zalo message to user {ZaloUserId}. Status: {Status}, Error: {Error}",
                    zaloUserId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Zalo message to user {ZaloUserId}", zaloUserId);
            return false;
        }
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Zalo OA access token endpoint
            // Reference: https://developers.zalo.me/docs/api/official-account-api/tai-lieu/gioi-thieu
            var url = $"/oa/access_token?app_id={_settings.AppId}&app_secret={_settings.AppSecret}&grant_type=client_credentials";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                if (result.TryGetProperty("access_token", out var tokenElement))
                {
                    return tokenElement.GetString();
                }
            }
            
            _logger.LogWarning("Failed to get Zalo access token. Status: {Status}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Zalo access token");
            return null;
        }
    }
}

