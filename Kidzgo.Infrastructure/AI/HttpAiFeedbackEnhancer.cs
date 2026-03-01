using System.Net.Http.Json;
using System.Text.Json;
using Kidzgo.Application.Abstraction.Reports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.AI;

/// <summary>
/// Implementation of IAiFeedbackEnhancer that calls AI-KidzGo API (Python FastAPI)
/// UC-174: AI enhance draft feedback
/// </summary>
public sealed class HttpAiFeedbackEnhancer : IAiFeedbackEnhancer
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger _logger;

    public HttpAiFeedbackEnhancer(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HttpAiFeedbackEnhancer> logger)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AiService:BaseUrl"] 
            ?? throw new InvalidOperationException("AiService:BaseUrl not configured");
        _logger = logger;
    }

    public async Task<EnhancedFeedbackResult> EnhanceAsync(
        string draft,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft))
        {
            throw new ArgumentException("Draft feedback cannot be null or empty", nameof(draft));
        }

        // Build request
        var request = new
        {
            draft = draft,
            language = "vi"
        };

        try
        {
            _logger.LogInformation("Calling AI feedback enhancer for draft: {Draft}", draft);

            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/a9/enhance-feedback",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("A9 API returned {StatusCode}: {Error}", response.StatusCode, errorContent);
                throw new InvalidOperationException(
                    $"A9 API returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<A9EnhanceResponse>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken: cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException("A9 API returned null response");
            }

            _logger.LogInformation("Successfully enhanced feedback");

            return new EnhancedFeedbackResult
            {
                OriginalFeedback = draft,
                EnhancedFeedback = result.Enhanced
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call A9 API at {BaseUrl}", _baseUrl);
            throw new InvalidOperationException(
                $"Failed to call A9 API: {ex.Message}. Make sure AI-KidzGo service is running at {_baseUrl}",
                ex);
        }
    }
}

/// <summary>
/// Response from A9 API
/// </summary>
public class A9EnhanceResponse
{
    public string Enhanced { get; set; } = string.Empty;
}
