# UC-174: AI Enhance Draft Feedback - Mock Implementation

## Tổng quan

UC-174 là API preview - KHÔNG lưu vào DB. Teacher gửi draft, AI trả về enhanced version để preview.

---

## Luồng hoạt động

### Thực tế (khi có Python API):

```
1. Teacher viết draft feedback (tự nhiên)
   ↓
2. POST /api/session-reports/ai/enhance-feedback
   { "draft": "Hôm nay em học tốt lắm" }
   ↓
3. BE → Python AI (/a9/enhance-feedback)
   ↓
4. AI trả về enhanced version
   ↓
5. FE hiển thị cho Teacher preview
   ↓
6. Teacher edit → submit chính thức
```

### Mock (development):

```
1. Teacher gửi draft
   ↓
2. BE → MockAiFeedbackEnhancer
   ↓
3. Trả về template-based enhanced
   ↓
4. FE preview (hoạt động bình thường!)
```

---

## Implementation

### 1. Interface

```csharp
// Kidzgo.Application/Abstraction/Reports/IAiFeedbackEnhancer.cs
namespace Kidzgo.Application.Abstraction.Reports;

public interface IAiFeedbackEnhancer
{
    Task<EnhancedFeedbackResult> EnhanceAsync(
        string draft,
        string? sessionId = null,
        string? studentProfileId = null,
        string? studentName = null,
        CancellationToken cancellationToken = default);
}

public class EnhancedFeedbackResult
{
    public string EnhancedFeedback { get; set; } = string.Empty;
    public string OriginalFeedback { get; set; } = string.Empty;
    public bool IsMock { get; set; }
}
```

### 2. Mock Implementation

```csharp
// Kidzgo.Infrastructure/AI/MockAiFeedbackEnhancer.cs
namespace Kidzgo.Infrastructure.AI;

public class MockAiFeedbackEnhancer : IAiFeedbackEnhancer
{
    public Task<EnhancedFeedbackResult> EnhanceAsync(
        string draft,
        string? sessionId = null,
        string? studentProfileId = null,
        string? studentName = null,
        CancellationToken cancellationToken = default)
    {
        var enhanced = EnhanceTemplate(draft, studentName);

        return Task.FromResult(new EnhancedFeedbackResult
        {
            OriginalFeedback = draft,
            EnhancedFeedback = enhanced,
            IsMock = true
        });
    }

    private string EnhanceTemplate(string draft, string? studentName)
    {
        var result = draft;

        // Simple word replacements (mock)
        var replacements = new Dictionary<string, string>
        {
            { "hôm nay", "trong buổi học" },
            { "em", studentName ?? "học sinh" },
            { "tốt lắm", "có tiến bộ" },
            { "lắm", "cao" },
            { "làm bài", "hoàn thành bài tập" },
            { "đầy đủ", "đầy đủ các nội dung được giao" },
            { "chăm chỉ", "thể hiện sự chăm chỉ trong học tập" },
            { "học tốt", "có kết quả học tập tốt" },
            { "ngoan", "có thái độ học tập tích cực" },
            { "được", "đạt được" }
        };

        foreach (var (key, value) in replacements)
        {
            result = result.Replace(key, value, StringComparison.OrdinalIgnoreCase);
        }

        if (!result.EndsWith(".") && !result.EndsWith("!") && !result.EndsWith("?"))
        {
            result += ".";
        }

        return $"[MOCK] {char.ToUpper(result[0])}{result.Substring(1)}";
    }
}
```

### 3. Real Implementation (Production)

```csharp
// Kidzgo.Infrastructure/AI/HttpAiFeedbackEnhancer.cs
namespace Kidzgo.Infrastructure.AI;

public class HttpAiFeedbackEnhancer : IAiFeedbackEnhancer
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HttpAiFeedbackEnhancer(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AiService:BaseUrl"] ?? throw new InvalidOperationException();
    }

    public async Task<EnhancedFeedbackResult> EnhanceAsync(
        string draft,
        string? sessionId = null,
        string? studentProfileId = null,
        string? studentName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            draft = draft,
            student_name = studentName,
            language = "vi"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/a9/enhance-feedback",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EnhanceResponse>(cancellationToken: cancellationToken);

        return new EnhancedFeedbackResult
        {
            OriginalFeedback = draft,
            EnhancedFeedback = result.Enhanced,
            IsMock = false
        };
    }
}
```

### 4. Controller

```csharp
// Kidzgo.Api/Controllers/SessionReportsController.cs
[ApiController]
[Route("api/session-reports")]
public class SessionReportsController : ControllerBase
{
    private readonly IAiFeedbackEnhancer _feedbackEnhancer;

    [HttpPost("ai/enhance-feedback")]
    public async Task<IActionResult> EnhanceFeedback(
        [FromBody] EnhanceFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _feedbackEnhancer.EnhanceAsync(
            request.Draft,
            request.SessionId,
            request.StudentProfileId,
            request.StudentName,
            cancellationToken);

        return Ok(new
        {
            isSuccess = true,
            data = new
            {
                enhancedFeedback = result.EnhancedFeedback,
                originalFeedback = result.OriginalFeedback,
                isMock = result.IsMock
            }
        });
    }
}

public class EnhanceFeedbackRequest
{
    public string Draft { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? StudentProfileId { get; set; }
    public string? StudentName { get; set; }
}
```

### 5. Configuration

```json
// appsettings.json
{
  "AiService": {
    "UseMock": true,
    "BaseUrl": "https://your-python-api.com"
  }
}
```

---

## API Request/Response

### Request

```json
POST /api/session-reports/ai/enhance-feedback
{
  "draft": "Hôm nay em học tốt lắm, làm bài đầy đủ"
}
```

### Response

```json
{
  "isSuccess": true,
  "data": {
    "enhancedFeedback": "[MOCK] Trong buổi học học sinh có tiến bộ cao, hoàn thành bài tập đầy đủ các nội dung được giao.",
    "originalFeedback": "Hôm nay em học tốt lắm, làm bài đầy đủ",
    "isMock": true
  }
}
```

---

## Tóm tắt

| Đặc điểm | Mô tả |
|-----------|-------|
| **Loại** | Preview API - KHÔNG lưu DB |
| **Input** | Chỉ cần `draft` (optional: sessionId, studentName) |
| **Output** | Enhanced feedback + original feedback |
| **Mock** | Template-based word replacement |
| **Switch** | Đổi `AiService:UseMock` trong config |
