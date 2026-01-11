# Mock AI Implementation Guide

## Khái niệm

**Mock Implementation** = Làm giả (fake) response từ AI service để:
- ✅ FE có thể test UI ngay lập tức
- ✅ BE có thể implement workflow đầy đủ
- ✅ Không cần chờ Python API thật
- ✅ Dễ dàng switch sang real API sau này

---

## Ví dụ: UC-176 - AI tạo draft Monthly Report

### Luồng thực tế (khi có Python API):

```
1. Teacher trigger "Generate AI Report"
   ↓
2. BE aggregate data (attendance, homework, test, mission, notes)
   ↓
3. BE gọi Python API: POST /a6/generate-monthly-report
   {
     "student": { "student_id": "...", "name": "Ngọc Anh" },
     "range": { "from_date": "2024-07-01", "to_date": "2024-07-31" },
     "session_feedbacks": [...],
     "recent_reports": [...]
   }
   ↓
4. Python AI service trả về:
   {
     "draft_text": "Trong tháng 7, Ngọc Anh...",
     "sections": {
       "overview": "...",
       "strengths": ["Phát âm tốt", "..."],
       "improvements": ["Tốc độ đọc chậm", "..."],
       ...
     }
   }
   ↓
5. BE lưu vào DB: draft_content = response
   ↓
6. FE hiển thị draft cho Teacher xem/chỉnh sửa
```

### Luồng Mock (development - không cần Python API):

```
1. Teacher trigger "Generate AI Report"
   ↓
2. BE aggregate data (attendance, homework, test, mission, notes)
   ↓
3. BE gọi Mock Service (thay vì Python API)
   ↓
4. Mock Service trả về data giả (hardcoded hoặc template):
   {
     "draft_text": "[MOCK] Trong tháng 7, học sinh đã có tiến bộ...",
     "sections": {
       "overview": "[MOCK] Tổng quan về tháng 7...",
       "strengths": ["[MOCK] Điểm mạnh 1", "[MOCK] Điểm mạnh 2"],
       "improvements": ["[MOCK] Cần cải thiện 1"],
       ...
     }
   }
   ↓
5. BE lưu vào DB: draft_content = mock response
   ↓
6. FE hiển thị draft cho Teacher xem/chỉnh sửa (UI hoạt động bình thường!)
```

---

## Implementation Code

### 1. Interface (Abstraction Layer)

```csharp
// Kidzgo.Application/Abstraction/Reports/IAiReportGenerator.cs
namespace Kidzgo.Application.Abstraction.Reports;

public interface IAiReportGenerator
{
    Task<string> GenerateDraftAsync(
        string dataJson,
        string studentName,
        int month,
        int year,
        CancellationToken cancellationToken = default);
}
```

### 2. Mock Implementation (Development)

```csharp
// Kidzgo.Infrastructure/AI/MockAiReportGenerator.cs
namespace Kidzgo.Infrastructure.AI;

public class MockAiReportGenerator : IAiReportGenerator
{
    public Task<string> GenerateDraftAsync(
        string dataJson,
        string studentName,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        // Parse dataJson để lấy thông tin thực (nếu cần)
        // Nhưng response thì dùng template giả

        var mockResponse = new
        {
            draft_text = $"[MOCK AI] Báo cáo tháng {month}/{year} cho {studentName}",
            sections = new
            {
                overview = $"[MOCK] Trong tháng {month}/{year}, {studentName} đã có những tiến bộ đáng kể trong việc học tiếng Anh. Học sinh tham gia đầy đủ các buổi học và hoàn thành bài tập đúng hạn.",
                strengths = new[]
                {
                    "[MOCK] Thái độ học tập tích cực",
                    "[MOCK] Phát âm tốt hơn so với tháng trước",
                    "[MOCK] Tích cực tham gia hoạt động nhóm"
                },
                improvements = new[]
                {
                    "[MOCK] Cần cải thiện tốc độ đọc",
                    "[MOCK] Cần mở rộng vốn từ vựng",
                    "[MOCK] Cần luyện tập thêm về ngữ pháp"
                },
                highlights = new[]
                {
                    $"[MOCK] Hoàn thành {GetRandomNumber(8, 12)} bài tập trong tháng",
                    "[MOCK] Đạt điểm cao trong bài kiểm tra giữa tháng"
                },
                goals_next_month = new[]
                {
                    "[MOCK] Luyện đọc 10 phút mỗi ngày",
                    "[MOCK] Học thêm 15 từ vựng mới",
                    "[MOCK] Thực hành nói tiếng Anh với bạn bè"
                },
                source_summary = "[MOCK] Dữ liệu được tổng hợp từ session reports, homework submissions, và exam results"
            }
        };

        // Serialize thành JSON string (giống như Python API sẽ trả về)
        var jsonResponse = JsonSerializer.Serialize(mockResponse, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        return Task.FromResult(jsonResponse);
    }

    private int GetRandomNumber(int min, int max)
    {
        return new Random().Next(min, max);
    }
}
```

### 3. Real Implementation (Production - khi có Python API)

```csharp
// Kidzgo.Infrastructure/AI/HttpAiReportGenerator.cs
namespace Kidzgo.Infrastructure.AI;

public class HttpAiReportGenerator : IAiReportGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl; // "https://your-python-api.com"

    public HttpAiReportGenerator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AiService:BaseUrl"] ?? throw new InvalidOperationException("AiService:BaseUrl not configured");
    }

    public async Task<string> GenerateDraftAsync(
        string dataJson,
        string studentName,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        // Parse dataJson thành MonthlyReportRequest
        var data = JsonSerializer.Deserialize<MonthlyReportData>(dataJson);
        
        var request = new MonthlyReportRequest
        {
            Student = new StudentInfo
            {
                StudentId = data.StudentProfileId.ToString(),
                Name = studentName
            },
            Range = new DateRange
            {
                FromDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd"),
                ToDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("yyyy-MM-dd")
            },
            SessionFeedbacks = data.SessionFeedbacks,
            RecentReports = data.RecentReports,
            Language = "vi"
        };

        // Gọi Python API
        var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/a6/generate-monthly-report",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MonthlyReportResponse>(cancellationToken: cancellationToken);
        
        // Serialize lại thành JSON string để lưu vào DB
        return JsonSerializer.Serialize(result);
    }
}
```

### 4. Dependency Injection (Switch giữa Mock và Real)

```csharp
// Kidzgo.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ...

        // Switch giữa Mock và Real dựa trên config
        var useMockAi = configuration.GetValue<bool>("AiService:UseMock", true);

        if (useMockAi)
        {
            // Development: Dùng Mock
            services.AddScoped<IAiReportGenerator, MockAiReportGenerator>();
            services.AddScoped<IAiHomeworkGradingService, MockAiHomeworkGradingService>();
            // ...
        }
        else
        {
            // Production: Dùng Real API
            services.AddHttpClient<IAiReportGenerator, HttpAiReportGenerator>(client =>
            {
                client.BaseAddress = new Uri(configuration["AiService:BaseUrl"]!);
                client.Timeout = TimeSpan.FromMinutes(5);
            });
            // ...
        }

        return services;
    }
}
```

### 5. Configuration (appsettings.json)

```json
{
  "AiService": {
    "UseMock": true,  // true = dùng Mock, false = gọi Python API thật
    "BaseUrl": "https://your-python-api.com"  // Chỉ dùng khi UseMock = false
  }
}
```

---

## Ví dụ: UC-130 - AI chấm Homework

### Mock Implementation

```csharp
// Kidzgo.Infrastructure/AI/MockAiHomeworkGradingService.cs
public class MockAiHomeworkGradingService : IAiHomeworkGradingService
{
    public Task<GradeResponse> GradeTextAsync(GradeTextRequest request, CancellationToken ct)
    {
        // Phân tích sơ bộ student_answer_text để tạo feedback hợp lý
        var answerLength = request.StudentAnswerText?.Length ?? 0;
        var score = answerLength > 50 ? 8 : answerLength > 20 ? 6 : 4;

        var mockResponse = new GradeResponse
        {
            Score = score,
            MaxScore = 10,
            Summary = $"[MOCK AI] Bài làm của học sinh có độ dài {answerLength} ký tự. Điểm số: {score}/10.",
            Strengths = new[]
            {
                "[MOCK] Nội dung đúng yêu cầu",
                "[MOCK] Sử dụng từ vựng phù hợp"
            },
            Issues = new[]
            {
                "[MOCK] Cần cải thiện ngữ pháp",
                "[MOCK] Cần viết dài hơn"
            },
            Suggestions = new[]
            {
                "[MOCK] Ôn lại thì quá khứ đơn",
                "[MOCK] Luyện viết thêm 2-3 câu mỗi ngày"
            },
            ExtractedStudentAnswer = request.StudentAnswerText,
            Confidence = new ConfidenceScore
            {
                Score = 0.75,
                Extraction = 0.9
            },
            Warnings = new[] { "[MOCK] Đây là kết quả mock, chỉ dùng để test UI" }
        };

        return Task.FromResult(mockResponse);
    }

    public Task<GradeResponse> GradeImageAsync(GradeImageRequest request, Stream imageStream, CancellationToken ct)
    {
        // Tương tự, trả về mock response
        return Task.FromResult(new GradeResponse
        {
            Score = 7,
            MaxScore = 10,
            Summary = "[MOCK AI] Đã phân tích ảnh bài làm. Chữ viết rõ ràng, có một số lỗi chính tả.",
            // ...
        });
    }
}
```

---

## Lợi ích của Mock Implementation

### 1. Development không bị block
- ✅ FE có thể test UI ngay
- ✅ BE có thể implement workflow đầy đủ
- ✅ Không cần chờ Python team

### 2. Dễ test
- ✅ Predictable responses (luôn biết kết quả)
- ✅ Không tốn tiền API calls
- ✅ Test nhanh hơn (không cần network)

### 3. Dễ switch
- ✅ Chỉ cần đổi config: `UseMock: false`
- ✅ Không cần sửa code logic
- ✅ Interface giống nhau

### 4. Demo/Staging
- ✅ Có thể demo với mock data
- ✅ Không cần setup Python API phức tạp

---

## Best Practices

### 1. Mock data nên realistic
```csharp
// ❌ BAD: Quá đơn giản
return "Mock response";

// ✅ GOOD: Giống real response
return new MonthlyReportResponse
{
    DraftText = $"[MOCK] Báo cáo tháng {month}/{year}...",
    Sections = new Sections
    {
        Overview = "...",
        Strengths = new[] { "...", "..." },
        // ...
    }
};
```

### 2. Đánh dấu rõ ràng là Mock
```csharp
// ✅ Luôn có prefix "[MOCK]" hoặc "[MOCK AI]"
Summary = "[MOCK AI] Bài làm tốt..."
```

### 3. Có thể dùng template với real data
```csharp
// ✅ Parse real data, nhưng generate response bằng template
var realAttendanceCount = data.AttendanceData.Total;
var mockOverview = $"[MOCK] Học sinh tham gia {realAttendanceCount} buổi học...";
```

### 4. Log khi dùng Mock
```csharp
_logger.LogWarning("Using Mock AI Service. Set AiService:UseMock=false to use real API.");
```

---

## Tóm tắt

**Mock Implementation = Làm giả response từ AI để:**
1. ✅ FE test UI ngay (không cần chờ Python API)
2. ✅ BE implement workflow đầy đủ
3. ✅ Dễ switch sang real API (chỉ đổi config)
4. ✅ Development nhanh hơn, không bị block

**Với UC-176:**
- Mock sẽ trả về draft report giả (có prefix "[MOCK]")
- FE vẫn có thể test flow: Generate → Review → Edit → Submit → Approve → Publish
- Khi có Python API, chỉ cần đổi `UseMock: false` là xong!

