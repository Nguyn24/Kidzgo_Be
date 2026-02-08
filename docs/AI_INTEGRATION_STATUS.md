# Tình trạng tích hợp AI với Backend .NET

## 📊 Hiện trạng

### ✅ Đã có:
1. **Interface `IAiReportGenerator`** - Định nghĩa contract
2. **API A6 Reports** - Đã chạy và sẵn sàng nhận request
3. **Monthly Report endpoints** - Đã implement đầy đủ

### ❌ Chưa có:
1. **Implementation `HttpAiReportGenerator`** - Chưa có class thực tế
2. **Endpoint để generate draft bằng AI** - Chưa có endpoint gọi AI
3. **Dependency Injection** - Chưa register `IAiReportGenerator`

---

## 🔄 Flow mong muốn (sau khi implement)

### **Khi user dùng Swagger của Backend .NET:**

```
User (Swagger Backend .NET)
  ↓
1. Gọi POST /api/monthly-reports/{reportId}/generate-draft
   (hoặc tích hợp vào endpoint hiện có)
  ↓
2. Backend .NET:
   - Lấy data từ MonthlyReportData (attendance, homework, test, mission, notes)
   - Lấy session feedbacks từ SessionReports
   - Lấy recent reports (3 tháng gần nhất)
   - Tạo request body cho A6 API
  ↓
3. Backend .NET gọi AI-KidzGo:
   POST http://localhost:8000/a6/generate-monthly-report
   {
     "student": {...},
     "range": {...},
     "session_feedbacks": [...],
     "recent_reports": [...],
     "teacher_notes": "..."
   }
  ↓
4. AI-KidzGo xử lý:
   - Dùng Gemini AI (nếu có key)
   - Hoặc rule-based (nếu không có)
   - Trả về draft_text và sections
  ↓
5. Backend .NET nhận response:
   {
     "ai_used": true,
     "draft_text": "...",
     "sections": {...}
   }
  ↓
6. Backend .NET lưu vào database:
   - StudentMonthlyReport.DraftContent = JSON của response
   - Cập nhật UpdatedAt
  ↓
7. Trả về cho user:
   {
     "id": "...",
     "draftContent": "...",
     "updatedAt": "..."
   }
```

---

## 🚀 Cần implement

### **1. Tạo HttpAiReportGenerator**

**File:** `Kidzgo.Infrastructure/AI/HttpAiReportGenerator.cs`

```csharp
using Kidzgo.Application.Abstraction.Reports;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kidzgo.Infrastructure.AI;

public class HttpAiReportGenerator : IAiReportGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HttpAiReportGenerator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AiService:BaseUrl"] 
            ?? throw new InvalidOperationException("AiService:BaseUrl not configured");
    }

    public async Task<string> GenerateDraftAsync(
        string dataJson,
        string studentName,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        // Parse dataJson để lấy aggregated data
        // Tạo request body theo schema của A6
        // Gọi POST /a6/generate-monthly-report
        // Parse response và trả về JSON string
    }
}
```

### **2. Tạo endpoint mới (hoặc tích hợp vào endpoint hiện có)**

**Option A: Tạo endpoint riêng**

```csharp
/// <summary>
/// UC-176: Generate draft Monthly Report bằng AI
/// </summary>
[HttpPost("{reportId:guid}/generate-draft")]
[Authorize(Roles = "Teacher")]
public async Task<IResult> GenerateMonthlyReportDraft(
    [FromRoute] Guid reportId,
    CancellationToken cancellationToken = default)
{
    // 1. Lấy report và data
    // 2. Gọi _aiReportGenerator.GenerateDraftAsync(...)
    // 3. Lưu vào StudentMonthlyReport.DraftContent
    // 4. Trả về response
}
```

**Option B: Tích hợp vào UpdateMonthlyReportDraft**

Thêm parameter `useAi: bool = false` vào request, nếu `true` thì gọi AI.

### **3. Register trong DependencyInjection**

**File:** `Kidzgo.Infrastructure/DependencyInjection.cs`

```csharp
// Add HttpClient for AI service
services.AddHttpClient<IAiReportGenerator, HttpAiReportGenerator>(client =>
{
    var baseUrl = configuration["AiService:BaseUrl"] 
        ?? "http://localhost:8000";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});
```

### **4. Cấu hình appsettings.json**

```json
{
  "AiService": {
    "BaseUrl": "http://localhost:8000",
    "UseMock": false
  }
}
```

---

## 📝 Tóm tắt

| Câu hỏi | Trả lời |
|---------|---------|
| **Swagger backend .NET có gọi AI-KidzGo không?** | ❌ **Chưa**, cần implement |
| **Có endpoint nào gọi AI không?** | ❌ **Chưa**, cần tạo mới |
| **AI-KidzGo đã sẵn sàng chưa?** | ✅ **Có**, API đã chạy |
| **Cần làm gì tiếp theo?** | 1. Implement `HttpAiReportGenerator`<br>2. Tạo endpoint generate draft<br>3. Register DI<br>4. Test end-to-end |

---

## 🧪 Test flow sau khi implement

1. **Start AI-KidzGo:** `uvicorn app.main:app --reload`
2. **Start Backend .NET:** `dotnet run`
3. **Mở Swagger Backend:** http://localhost:5000/swagger
4. **Gọi endpoint generate draft:**
   - `POST /api/monthly-reports/{reportId}/generate-draft`
5. **Kiểm tra:**
   - Response có `draftContent`
   - Database có lưu kết quả
   - AI-KidzGo nhận được request và trả về response

