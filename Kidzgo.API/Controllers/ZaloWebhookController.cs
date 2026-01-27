using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Leads.CreateLead;
using Kidzgo.Application.Leads.CreateLeadFromZalo;
using Kidzgo.Infrastructure.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Kidzgo.API.Controllers;

[Route("webhooks/zalo")]
[ApiController]
public class ZaloWebhookController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ZaloSettings _zaloSettings;

    public ZaloWebhookController(ISender mediator, IOptions<ZaloSettings> zaloSettings)
    {
        _mediator = mediator;
        _zaloSettings = zaloSettings.Value;
    }

    /// <summary>
    /// Webhook verification endpoint (GET) - Zalo sẽ gọi để verify webhook
    /// UC-014: Verify Zalo webhook
    /// </summary>
    [HttpGet("lead")]
    public IResult VerifyWebhook(
        [FromQuery] string? oa_id,
        [FromQuery] string? timestamp,
        [FromQuery] string? nonce,
        [FromQuery] string? signature)
    {
        // Verify webhook signature
        if (string.IsNullOrEmpty(_zaloSettings.WebhookVerifyToken) || 
            string.IsNullOrEmpty(_zaloSettings.AppSecret))
        {
            return Results.BadRequest(new { error = "Zalo configuration missing" });
        }

        // Zalo webhook verification logic
        // Tham khảo: https://developers.zalo.me/docs/api/official-account-api/webhook
        var expectedSignature = CalculateSignature(oa_id, timestamp, nonce, _zaloSettings.WebhookVerifyToken, _zaloSettings.AppSecret);

        if (signature != expectedSignature)
        {
            return Results.Unauthorized();
        }

        // Return challenge string for verification
        return Results.Ok(new { challenge = nonce });
    }

    /// <summary>
    /// UC-014: Webhook endpoint để nhận Lead từ Zalo (POST)
    /// </summary>
    [HttpPost("lead")]
    public async Task<IResult> ReceiveLeadFromZalo(
        [FromBody] ZaloWebhookRequest request,
        CancellationToken cancellationToken)
    {
        // Verify webhook signature từ header (nếu có)
        var signature = Request.Headers["X-Zalo-Signature"].FirstOrDefault() ?? string.Empty;
        
        // Verify signature (implement theo Zalo documentation)
        if (!string.IsNullOrEmpty(signature) && !VerifyZaloSignature(request, signature))
        {
            return Results.Unauthorized();
        }

        // Xử lý các loại event khác nhau
        switch (request.Event)
        {
            case "user_send_text":
                // User gửi tin nhắn text - có thể chứa thông tin lead
                return await HandleTextMessage(request, cancellationToken);
            
            case "form_submit":
                // User submit form trong Mini App - thông tin lead đầy đủ
                return await HandleFormSubmit(request, cancellationToken);
            
            case "user_open_app":
                // User mở Mini App - có thể track nhưng chưa tạo lead
                return Results.Ok(new { message = "App opened" });
            
            default:
                return Results.Ok(new { message = "Event not handled", event_type = request.Event });
        }
    }

    private async Task<IResult> HandleTextMessage(
        ZaloWebhookRequest request,
        CancellationToken cancellationToken)
    {
        // Parse thông tin từ tin nhắn text
        // Có thể dùng AI hoặc pattern matching để extract thông tin
        
        var command = new CreateLeadFromZaloCommand
        {
            ZaloUserId = request.UserId,
            ZaloOAId = request.OAId,
            Message = request.Message?.Text,
            ContactName = ExtractContactName(request.Message?.Text) ?? "Unknown",
            Phone = ExtractPhone(request.Message?.Text),
            Email = ExtractEmail(request.Message?.Text),
            Notes = $"Created from Zalo text message: {request.Message?.Text}"
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    private async Task<IResult> HandleFormSubmit(
        ZaloWebhookRequest request,
        CancellationToken cancellationToken)
    {
        // Parse thông tin từ form submission
        var formData = request.FormData;

        var command = new CreateLeadFromZaloCommand
        {
            ZaloUserId = request.UserId,
            ZaloOAId = request.OAId,
            ContactName = formData?.GetValueOrDefault("contactName")?.ToString() ?? "Unknown",
            Phone = formData?.GetValueOrDefault("phone")?.ToString(),
            Email = formData?.GetValueOrDefault("email")?.ToString(),
            BranchPreference = formData?.ContainsKey("branchId") == true && 
                Guid.TryParse(formData["branchId"]?.ToString(), out var branchId)
                ? branchId
                : null,
            ProgramInterest = formData?.GetValueOrDefault("programInterest")?.ToString(),
            Notes = formData?.GetValueOrDefault("notes")?.ToString() ?? 
                $"Created from Zalo form submission. Event: {request.Event}"
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    private string? ExtractContactName(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        // Simple pattern: "Tên: ..." hoặc "Họ tên: ..."
        var patterns = new[]
        {
            @"(?:Tên|Họ tên|Họ và tên)[\s:]+([A-Za-zÀ-ỹ\s]+)",
            @"^([A-Za-zÀ-ỹ\s]{2,})" // Nếu bắt đầu bằng tên
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, pattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private string? ExtractPhone(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        // Pattern: Vietnamese phone numbers
        var pattern = @"\b(0|\+84)[0-9]{9,10}\b";
        var match = System.Text.RegularExpressions.Regex.Match(message, pattern);
        
        if (match.Success)
        {
            var phone = match.Value;
            // Normalize: +84 -> 0
            if (phone.StartsWith("+84"))
            {
                phone = "0" + phone.Substring(3);
            }
            return phone;
        }

        return null;
    }

    private string? ExtractEmail(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        // Pattern: Email
        var pattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
        var match = System.Text.RegularExpressions.Regex.Match(message, pattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return match.Success ? match.Value : null;
    }

    private bool VerifyZaloSignature(ZaloWebhookRequest request, string receivedSignature)
    {
        if (string.IsNullOrEmpty(_zaloSettings.AppSecret))
            return false;

        var timestamp = request.Timestamp?.ToString() ?? string.Empty;
        var data = $"{request.OAId}{timestamp}{_zaloSettings.AppSecret}";
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_zaloSettings.AppSecret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var expectedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        
        return receivedSignature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
    }

    private string CalculateSignature(string? oaId, string? timestamp, string? nonce, string verifyToken, string appSecret)
    {
        // Zalo signature calculation
        // Format: SHA256(oa_id + timestamp + nonce + verify_token + app_secret)
        var data = $"{oaId}{timestamp}{nonce}{verifyToken}{appSecret}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}

