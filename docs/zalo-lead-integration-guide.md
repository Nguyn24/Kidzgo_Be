# Hướng dẫn tích hợp Zalo Lead - UC-014: Tạo Lead từ Zalo

## Tổng quan

**UC-014: Tạo Lead từ Zalo** cho phép hệ thống tự động nhận Lead từ Zalo Mini App hoặc Zalo Official Account (OA) và tạo Lead trong hệ thống.

## Yêu cầu

- Zalo Official Account (OA) hoặc Zalo Mini App đã được đăng ký
- Webhook URL từ Zalo có thể gọi về backend
- Backend API có thể nhận và xử lý webhook từ Zalo

---

## 1. Setup Zalo Official Account / Mini App

### Bước 1: Đăng ký Zalo OA hoặc Mini App

1. Truy cập: https://developers.zalo.me/
2. Đăng nhập bằng tài khoản Zalo Business
3. Tạo mới **Official Account** hoặc **Mini App**

### Bước 2: Lấy thông tin từ Zalo Developer Console

Sau khi đăng nhập vào Zalo Developer Console:

1. **Vào mục "Quản lý ứng dụng" hoặc "App Management"**
2. **Lấy các thông tin sau:**

   - **App ID** (`AppId`):
     - Tìm trong phần "Thông tin ứng dụng"
     - Ví dụ: `1234567890123456789`

   - **App Secret** (`AppSecret`):
     - Vào mục "Bảo mật" hoặc "Security"
     - Copy App Secret (giữ bí mật!)
     - Ví dụ: `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`

   - **OA ID** (nếu dùng Official Account):
     - Tìm trong phần "Thông tin tài khoản"
     - Ví dụ: `1234567890123456789`

### Bước 3: Cấu hình Webhook URL và Verify Token

Trong Zalo Developer Console:

1. Vào mục **"Webhook"** hoặc **"Cài đặt Webhook"**
2. Đặt Webhook URL: `https://your-backend-domain.com/webhooks/zalo/lead`
   - Ví dụ: `https://kidzgo-be.onrender.com/webhooks/zalo/lead`
3. **Tạo Webhook Verify Token** (quan trọng):
   - Đây là một chuỗi secret mà **BẠN TỰ TẠO** (không phải lấy từ Zalo)
   - Tạo một chuỗi ngẫu nhiên, an toàn (ví dụ: dùng password generator)
   - Ví dụ: `kidzgo-zalo-webhook-6789-secret-token-xyz123`
   - **Lưu ý**: Token này phải giống nhau ở 2 nơi:
     - Trong Zalo Developer Console (khi setup webhook)
     - Trong `appsettings.json` của bạn (field `WebhookVerifyToken`)
   - Zalo sẽ dùng token này để verify webhook khi gọi GET request
4. Nhập **Webhook Verify Token** vào Zalo Console
5. Chọn các sự kiện (Events) cần nhận:
   - `user_send_text`: User gửi tin nhắn text
   - `user_send_image`: User gửi ảnh
   - `user_send_location`: User gửi vị trí
   - `user_open_app`: User mở Mini App
   - `form_submit`: User submit form trong Mini App
6. Lưu lại để Zalo gọi về khi có sự kiện

---

## 2. Cấu hình trong Backend

### Bước 1: Thêm Zalo config vào `appsettings.json` hoặc Environment Variables

**Option 1: Thêm vào `appsettings.Production.json` (cho production):**

```json
{
  "Zalo": {
    "AppId": "your-zalo-app-id",
    "AppSecret": "your-zalo-app-secret",
    "OAId": "your-zalo-oa-id",
    "WebhookVerifyToken": "kidzgo-zalo-webhook-2024-secret-token-xyz123",
    "BaseUrl": "https://openapi.zalo.me/v2.0"
  }
}
```

**Lưu ý về `WebhookVerifyToken`**:
- Đây là token **BẠN TỰ TẠO**, không phải lấy từ Zalo
- Tạo một chuỗi secret ngẫu nhiên, an toàn (khuyến nghị: ít nhất 32 ký tự)
- Token này phải **GIỐNG NHAU** ở 2 nơi:
  1. Trong Zalo Developer Console (khi setup webhook)
  2. Trong `appsettings.json` của bạn
- Zalo sẽ dùng token này để verify webhook khi gọi GET request đến endpoint của bạn
- Có thể dùng password generator hoặc tạo random string để tạo token

**Option 2: Sử dụng Environment Variables (khuyến nghị cho production):**

```bash
Zalo__AppId=your-zalo-app-id
Zalo__AppSecret=your-zalo-app-secret
Zalo__OAId=your-zalo-oa-id
Zalo__WebhookVerifyToken=your-webhook-verify-token
Zalo__BaseUrl=https://openapi.zalo.me/v2.0
```

### Bước 2: Tạo Zalo Configuration Class

Tạo file `Kidzgo.Infrastructure/Configuration/ZaloSettings.cs`:

```csharp
namespace Kidzgo.Infrastructure.Configuration;

public class ZaloSettings
{
    public string AppId { get; set; } = null!;
    public string AppSecret { get; set; } = null!;
    public string? OAId { get; set; }
    public string WebhookVerifyToken { get; set; } = null!;
    public string BaseUrl { get; set; } = "https://openapi.zalo.me/v2.0";
}
```

### Bước 3: Register Zalo Settings trong DependencyInjection

Thêm vào `Kidzgo.Infrastructure/DependencyInjection.cs`:

```csharp
services.Configure<ZaloSettings>(
    configuration.GetSection("Zalo"));
```

---

## 3. Triển khai Webhook Endpoint

### Bước 1: Tạo Zalo Webhook Controller

Tạo file `Kidzgo.API/Controllers/ZaloWebhookController.cs`:

```csharp
using Kidzgo.API.Extensions;
using Kidzgo.Application.Leads.CreateLeadFromZalo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("webhooks/zalo")]
[ApiController]
public class ZaloWebhookController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IConfiguration _configuration;

    public ZaloWebhookController(ISender mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    /// <summary>
    /// Webhook verification endpoint (GET) - Zalo sẽ gọi để verify webhook
    /// </summary>
    [HttpGet("lead")]
    public IResult VerifyWebhook(
        [FromQuery] string? oa_id,
        [FromQuery] string? timestamp,
        [FromQuery] string? nonce,
        [FromQuery] string? signature)
    {
        // Verify webhook signature
        var verifyToken = _configuration["Zalo:WebhookVerifyToken"];
        var appSecret = _configuration["Zalo:AppSecret"];

        if (string.IsNullOrEmpty(verifyToken) || string.IsNullOrEmpty(appSecret))
        {
            return Results.BadRequest("Zalo configuration missing");
        }

        // Zalo webhook verification logic
        // Tham khảo: https://developers.zalo.me/docs/api/official-account-api/webhook
        var expectedSignature = CalculateSignature(oa_id, timestamp, nonce, verifyToken, appSecret);

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
        // Verify webhook signature từ header
        var signature = Request.Headers["X-Zalo-Signature"].FirstOrDefault() ?? string.Empty;
        
        // Verify signature (implement theo Zalo documentation)
        if (!VerifyZaloSignature(request, signature))
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
                return Results.Ok(new { message = "Event not handled" });
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
            Source = "Zalo",
            // Extract thông tin từ message nếu có
            ContactName = ExtractContactName(request.Message?.Text),
            Phone = ExtractPhone(request.Message?.Text),
            Email = ExtractEmail(request.Message?.Text),
            // Metadata từ Zalo
            ZaloMetadata = new
            {
                Timestamp = request.Timestamp,
                Event = request.Event,
                UserId = request.UserId
            }
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    private async Task<IResult> HandleFormSubmit(
        ZaloWebhookRequest request,
        CancellationToken cancellationToken)
    {
        // Parse thông tin từ form submission
        var formData = request.FormData; // Giả sử Zalo gửi form data trong request

        var command = new CreateLeadFromZaloCommand
        {
            ZaloUserId = request.UserId,
            ZaloOAId = request.OAId,
            ContactName = formData?.GetValueOrDefault("contactName")?.ToString(),
            Phone = formData?.GetValueOrDefault("phone")?.ToString(),
            Email = formData?.GetValueOrDefault("email")?.ToString(),
            BranchPreference = formData?.ContainsKey("branchId") == true 
                ? Guid.Parse(formData["branchId"].ToString()!) 
                : null,
            ProgramInterest = formData?.GetValueOrDefault("programInterest")?.ToString(),
            Notes = formData?.GetValueOrDefault("notes")?.ToString(),
            Source = "Zalo",
            ZaloMetadata = new
            {
                Timestamp = request.Timestamp,
                Event = request.Event,
                UserId = request.UserId,
                FormId = formData?.GetValueOrDefault("formId")?.ToString()
            }
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    private string? ExtractContactName(string? message)
    {
        // Implement logic để extract tên từ message
        // Có thể dùng regex hoặc AI
        return null;
    }

    private string? ExtractPhone(string? message)
    {
        // Implement logic để extract số điện thoại từ message
        // Regex pattern: \b(0|\+84)[0-9]{9,10}\b
        return null;
    }

    private string? ExtractEmail(string? message)
    {
        // Implement logic để extract email từ message
        // Regex pattern: \b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b
        return null;
    }

    private bool VerifyZaloSignature(ZaloWebhookRequest request, string signature)
    {
        // Implement Zalo signature verification
        // Tham khảo: https://developers.zalo.me/docs/api/official-account-api/webhook
        var appSecret = _configuration["Zalo:AppSecret"];
        // Calculate expected signature và so sánh
        return true; // Placeholder
    }

    private string CalculateSignature(string? oaId, string? timestamp, string? nonce, string verifyToken, string appSecret)
    {
        // Implement Zalo signature calculation
        // Tham khảo Zalo documentation
        return string.Empty; // Placeholder
    }
}

// Request model cho Zalo webhook
public class ZaloWebhookRequest
{
    public string Event { get; set; } = null!;
    public string? UserId { get; set; }
    public string? OAId { get; set; }
    public long? Timestamp { get; set; }
    public ZaloMessage? Message { get; set; }
    public Dictionary<string, object>? FormData { get; set; }
}

public class ZaloMessage
{
    public string? Text { get; set; }
    public string? Type { get; set; }
}
```

### Bước 2: Tạo CreateLeadFromZalo Command

Tạo file `Kidzgo.Application/Leads/CreateLeadFromZalo/CreateLeadFromZaloCommand.cs`:

```csharp
using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.CreateLeadFromZalo;

public sealed class CreateLeadFromZaloCommand : ICommand<CreateLeadFromZaloResponse>
{
    public string? ZaloUserId { get; init; }
    public string? ZaloOAId { get; init; }
    public string? ContactName { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public Guid? BranchPreference { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
    public string Source { get; init; } = "Zalo";
    public string? Message { get; init; }
    public object? ZaloMetadata { get; init; }
}
```

### Bước 3: Tạo Command Handler

Tạo file `Kidzgo.Application/Leads/CreateLeadFromZalo/CreateLeadFromZaloCommandHandler.cs`:

```csharp
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.CreateLeadFromZalo;

public sealed class CreateLeadFromZaloCommandHandler(
    IDbContext context
) : ICommandHandler<CreateLeadFromZaloCommand, CreateLeadFromZaloResponse>
{
    public async Task<Result<CreateLeadFromZaloResponse>> Handle(
        CreateLeadFromZaloCommand command,
        CancellationToken cancellationToken)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(command.ContactName) && 
            string.IsNullOrWhiteSpace(command.Phone) && 
            string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Failure<CreateLeadFromZaloResponse>(
                LeadErrors.InvalidContactInfo);
        }

        // Check if lead already exists (by phone or email or zalo_id)
        var existingLead = await context.Leads
            .FirstOrDefaultAsync(l =>
                (!string.IsNullOrWhiteSpace(command.Phone) && l.Phone == command.Phone) ||
                (!string.IsNullOrWhiteSpace(command.Email) && l.Email == command.Email) ||
                (!string.IsNullOrWhiteSpace(command.ZaloUserId) && l.ZaloId == command.ZaloUserId),
                cancellationToken);

        if (existingLead is not null)
        {
            // Update existing lead instead of creating new one
            existingLead.TouchCount++;
            existingLead.UpdatedAt = DateTime.UtcNow;
            
            // Update contact info if provided
            if (!string.IsNullOrWhiteSpace(command.ContactName))
                existingLead.ContactName = command.ContactName;
            if (!string.IsNullOrWhiteSpace(command.Phone))
                existingLead.Phone = command.Phone;
            if (!string.IsNullOrWhiteSpace(command.Email))
                existingLead.Email = command.Email;
            if (!string.IsNullOrWhiteSpace(command.ZaloUserId))
                existingLead.ZaloId = command.ZaloUserId;

            await context.SaveChangesAsync(cancellationToken);

            return new CreateLeadFromZaloResponse
            {
                Id = existingLead.Id,
                IsNewLead = false,
                Message = "Lead updated (existing lead found)"
            };
        }

        // Create new lead
        var now = DateTime.UtcNow;
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Source = LeadSource.Zalo,
            ContactName = command.ContactName ?? "Unknown",
            Phone = command.Phone,
            ZaloId = command.ZaloUserId,
            Email = command.Email,
            BranchPreference = command.BranchPreference,
            ProgramInterest = command.ProgramInterest,
            Notes = command.Notes ?? $"Created from Zalo. Message: {command.Message}",
            Status = LeadStatus.New,
            TouchCount = 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Leads.Add(lead);

        // Create Lead Activity để track
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = LeadActivityType.Note,
            Content = $"Lead created from Zalo. Event: {command.ZaloMetadata}",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateLeadFromZaloResponse
        {
            Id = lead.Id,
            IsNewLead = true,
            Message = "Lead created successfully"
        };
    }
}
```

---

## 4. Security & Validation

### Webhook Signature Verification

Zalo gửi signature trong header để verify request. Cần implement verification logic:

```csharp
private bool VerifyZaloSignature(ZaloWebhookRequest request, string receivedSignature)
{
    var appSecret = _configuration["Zalo:AppSecret"];
    var timestamp = request.Timestamp?.ToString() ?? string.Empty;
    var data = $"{request.OAId}{timestamp}{appSecret}";
    
    using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
    var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    var expectedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    
    return receivedSignature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
}
```

### Rate Limiting

Nên thêm rate limiting cho webhook endpoint để tránh spam:

```csharp
[EnableRateLimiting("ZaloWebhookPolicy")]
[HttpPost("lead")]
public async Task<IResult> ReceiveLeadFromZalo(...)
```

---

## 5. Testing

### Test Webhook Verification (GET)

```bash
curl -X GET "https://your-api.com/webhooks/zalo/lead?oa_id=123&timestamp=1234567890&nonce=abc123&signature=xyz"
```

### Test Webhook với Form Submit (POST)

```bash
curl -X POST "https://your-api.com/webhooks/zalo/lead" \
  -H "Content-Type: application/json" \
  -H "X-Zalo-Signature: your-signature" \
  -d '{
    "event": "form_submit",
    "user_id": "zalo-user-id-123",
    "oa_id": "your-oa-id",
    "timestamp": 1234567890,
    "form_data": {
      "contactName": "Nguyễn Văn A",
      "phone": "0912345678",
      "email": "test@example.com",
      "branchId": "branch-guid-here",
      "programInterest": "English for Kids"
    }
  }'
```

### Test với Text Message

```bash
curl -X POST "https://your-api.com/webhooks/zalo/lead" \
  -H "Content-Type: application/json" \
  -H "X-Zalo-Signature: your-signature" \
  -d '{
    "event": "user_send_text",
    "user_id": "zalo-user-id-123",
    "oa_id": "your-oa-id",
    "timestamp": 1234567890,
    "message": {
      "text": "Tôi muốn đăng ký học. Tên: Nguyễn Văn A, SĐT: 0912345678"
    }
  }'
```

---

## 6. Error Handling

### Common Errors

1. **Invalid Signature**: Webhook signature không khớp
   - Response: `401 Unauthorized`
   - Action: Kiểm tra AppSecret và signature calculation

2. **Missing Required Fields**: Thiếu thông tin contact
   - Response: `400 Bad Request`
   - Action: Validate và return error message

3. **Duplicate Lead**: Lead đã tồn tại
   - Response: `200 OK` với `IsNewLead = false`
   - Action: Update existing lead thay vì tạo mới

---

## 7. Monitoring & Logging

### Logging

Nên log tất cả webhook requests để debug:

```csharp
_logger.LogInformation(
    "Received Zalo webhook: Event={Event}, UserId={UserId}, OAId={OAId}",
    request.Event, request.UserId, request.OAId);
```

### Metrics

Track các metrics:
- Số lượng leads tạo từ Zalo mỗi ngày
- Tỷ lệ duplicate leads
- Response time của webhook endpoint

---

## 8. Tài liệu tham khảo

- **Zalo Official Account API**: https://developers.zalo.me/docs/api/official-account-api/webhook
- **Zalo Mini App API**: https://developers.zalo.me/docs/api/mini-app-api
- **Webhook Security**: https://developers.zalo.me/docs/api/official-account-api/webhook#security

---

## 9. Checklist triển khai

- [ ] Đăng ký Zalo OA/Mini App
- [ ] Lấy App ID, App Secret, OA ID
- [ ] Cấu hình Webhook URL trong Zalo Console
- [ ] Thêm Zalo config vào appsettings.json
- [ ] Tạo ZaloWebhookController
- [ ] Implement webhook verification (GET)
- [ ] Implement webhook handler (POST)
- [ ] Tạo CreateLeadFromZaloCommand và Handler
- [ ] Implement signature verification
- [ ] Test webhook với Zalo sandbox
- [ ] Deploy và test với production
- [ ] Setup monitoring và logging
- [ ] Document API cho team

---

## 10. Lưu ý quan trọng

1. **Security**: Luôn verify webhook signature để tránh fake requests
2. **Idempotency**: Xử lý duplicate leads (cùng phone/email/zalo_id)
3. **Error Handling**: Luôn return 200 OK cho Zalo (trừ khi signature invalid) để tránh retry
4. **Rate Limiting**: Implement rate limiting để tránh spam
5. **Logging**: Log đầy đủ để debug khi có vấn đề

---

## 11. Next Steps

Sau khi triển khai UC-014, có thể implement:
- **UC-024**: Tự động track SLA khi lead được tạo
- **UC-025**: Tự động tăng touch count khi có interaction từ Zalo
- **UC-026**: Tự động set next_action_at dựa trên lead status

