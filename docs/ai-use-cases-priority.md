# Phân loại Use Cases: Thủ công vs AI & Thứ tự ưu tiên

## Tổng quan

Dựa trên `ai-flow.md`, `InputDataAI.md`, và `Template&PromtAI.md`, có **4 AI Agents**:
- **A3**: Homework Grading (text/image/link)
- **A6**: Monthly Reports generation
- **A7**: OCR Payment Proof/Receipts
- **A8**: Speaking/Phonics analysis

---

## 1. A3 - Homework Grading

### Use Cases liên quan AI:
- **UC-130**: AI chấm Homework (A3 - grade-text/grade-image/grade-link)
- **UC-131**: Xem/chỉnh sửa kết quả AI chấm

### Use Cases thủ công (ưu tiên làm trước):
- **UC-117**: Tạo Homework Assignment
- **UC-118**: Xem danh sách Homework Assignments
- **UC-119**: Xem chi tiết Homework Assignment
- **UC-120**: Cập nhật Homework Assignment
- **UC-121**: Xóa Homework Assignment
- **UC-122**: Tự động assign homework cho tất cả học sinh lớp
- **UC-123**: Gắn Homework với Mission
- **UC-124**: Thiết lập reward stars cho Homework
- **UC-125**: Học sinh nộp Homework (FILE/IMAGE/TEXT/LINK/QUIZ)
- **UC-126**: Xem danh sách Homework đã nộp
- **UC-127**: Xem chi tiết Homework submission
- **UC-128**: Teacher chấm Homework (GRADED) - **THỦ CÔNG**
- **UC-129**: Nhập điểm và feedback cho Homework - **THỦ CÔNG**
- **UC-132**: Đánh dấu Homework quá hạn (LATE/MISSING)
- **UC-133**: Xem lịch sử Homework của học sinh

### Thứ tự implement:
1. ✅ **Phase 1 (Thủ công)**: UC-117 → UC-125 → UC-128 → UC-129
2. ⏸️ **Phase 2 (AI)**: UC-130, UC-131 (sau khi có Python API mock)

### Luồng thủ công:
```
Teacher tạo Homework (UC-117)
  ↓
Student nộp bài (UC-125)
  ↓
Teacher xem submission (UC-127)
  ↓
Teacher chấm thủ công (UC-128, UC-129)
  ↓
Lưu score, feedback vào DB
```

### Luồng AI (sau này):
```
Teacher tạo Homework (UC-117)
  ↓
Student nộp bài (UC-125)
  ↓
Teacher trigger AI chấm (UC-130)
  ↓
Gọi Python API: POST /a3/grade-text (hoặc grade-image/grade-link)
  ↓
Nhận GradeResponse → Lưu vào ai_feedback (UC-131)
  ↓
Teacher xem/chỉnh sửa kết quả AI (UC-131)
```

---

## 2. A6 - Monthly Reports

### Use Cases liên quan AI:
- **UC-176**: AI tạo draft Monthly Report (A6 - generate-monthly-report)

### Use Cases thủ công (ưu tiên làm trước):
- **UC-174**: Tạo Monthly Report Job
- **UC-175**: Gom dữ liệu cho Monthly Report (attendance, homework, test, mission, notes) - **CÓ THỂ LÀM THỦ CÔNG**
- **UC-177**: Teacher xem danh sách Monthly Report Jobs
- **UC-178**: Xem trạng thái Monthly Report Job
- **UC-179**: Teacher xem draft Monthly Report
- **UC-180**: Teacher chỉnh sửa draft Monthly Report
- **UC-181**: Teacher submit Monthly Report (REVIEW)
- **UC-182**: Staff/Admin comment Monthly Report
- **UC-183**: Staff/Admin approve Monthly Report
- **UC-184**: Staff/Admin reject Monthly Report
- **UC-185**: Publish Monthly Report
- **UC-186**: Parent/Student xem Monthly Report
- **UC-187**: Export Monthly Report sang PDF
- **UC-187a**: Gửi thông báo khi publish Monthly Report

### Thứ tự implement:
1. ✅ **Phase 1 (Thủ công)**: UC-174 → UC-175 (aggregate data) → UC-179 → UC-180 → UC-181 → UC-183 → UC-185
2. ⏸️ **Phase 2 (AI)**: UC-176 (sau khi có Python API mock)

### Luồng thủ công:
```
Staff tạo Monthly Report Job (UC-174)
  ↓
Aggregate data thủ công (UC-175):
  - Query attendance records
  - Query homework submissions
  - Query exam results
  - Query mission progress
  - Query session reports
  ↓
Tạo StudentMonthlyReport với draft_content = NULL (UC-179)
  ↓
Teacher tự viết draft_content (UC-180)
  ↓
Teacher submit (UC-181)
  ↓
Staff/Admin approve (UC-183)
  ↓
Publish (UC-185)
```

### Luồng AI (sau này):
```
Staff tạo Monthly Report Job (UC-174)
  ↓
Aggregate data (UC-175) → Tạo MonthlyReportData
  ↓
Gọi Python API: POST /a6/generate-monthly-report
  - Input: MonthlyReportRequest (student, session_feedbacks, recent_reports)
  - Output: MonthlyReportResponse (draft_text, sections)
  ↓
Lưu draft_content từ AI (UC-176)
  ↓
Teacher xem/chỉnh sửa (UC-179, UC-180)
  ↓
Teacher submit (UC-181)
  ↓
Staff/Admin approve (UC-183)
  ↓
Publish (UC-185)
```

---

## 3. A7 - OCR Payment Proof

### Use Cases liên quan AI:
- **Không có trong use-cases-list.md** nhưng có trong `ai-flow.md`
- **Có thể thêm**: UC-268a: OCR extract payment proof từ ảnh

### Use Cases thủ công (ưu tiên làm trước):
- **UC-253**: Tạo Invoice
- **UC-254**: Xem danh sách Invoices
- **UC-255**: Xem chi tiết Invoice
- **UC-256**: Cập nhật Invoice
- **UC-257**: Hủy Invoice
- **UC-258**: Tạo Invoice Lines
- **UC-259**: Gắn session_ids vào Invoice Line
- **UC-260**: Sinh PayOS payment link
- **UC-261**: Sinh PayOS QR code
- **UC-262**: Xem trạng thái Invoice
- **UC-263**: Đánh dấu Invoice OVERDUE
- **UC-264**: Thanh toán qua PayOS (webhook)
- **UC-265**: Thanh toán bằng tiền mặt
- **UC-266**: Thanh toán chuyển khoản
- **UC-267**: Xác nhận thanh toán (Staff)
- **UC-268**: Upload chứng từ thanh toán - **THỦ CÔNG**
- **UC-269**: Cập nhật Invoice status sau thanh toán
- **UC-270**: Xem lịch sử Payments của Invoice

### Thứ tự implement:
1. ✅ **Phase 1 (Thủ công)**: UC-253 → UC-268 (upload ảnh thủ công, nhập thông tin thủ công)
2. ⏸️ **Phase 2 (AI)**: OCR extract (sau khi có Python API mock)

### Luồng thủ công:
```
Staff tạo Invoice (UC-253)
  ↓
Parent thanh toán (UC-264, UC-265, UC-266)
  ↓
Staff upload ảnh biên lai (UC-268)
  ↓
Staff nhập thông tin thanh toán thủ công:
  - amount
  - transaction_datetime
  - transaction_id
  - bank_name
  - ...
  ↓
Xác nhận thanh toán (UC-267)
```

### Luồng AI (sau này):
```
Staff tạo Invoice (UC-253)
  ↓
Parent thanh toán (UC-264, UC-265, UC-266)
  ↓
Staff upload ảnh biên lai (UC-268)
  ↓
Gọi Python API: POST /a7/extract-payment-proof
  - Input: file (image), direction, branch_id
  - Output: PaymentProofExtractResponse (fields, confidence, raw_text, warnings)
  ↓
Auto-fill form với dữ liệu từ OCR
  ↓
Staff kiểm tra và confirm (UC-267)
```

---

## 4. A8 - Speaking/Phonics

### Use Cases liên quan AI:
- **UC-130**: AI chấm Homework (có thể dùng A8 nếu là speaking/phonics homework)
- **Có thể thêm**: UC-130a: AI chấm Speaking/Phonics (analyze-transcript/analyze-media)

### Use Cases thủ công (ưu tiên làm trước):
- **UC-128**: Teacher chấm Homework (GRADED) - **THỦ CÔNG**
- **UC-129**: Nhập điểm và feedback cho Homework - **THỦ CÔNG**

### Thứ tự implement:
1. ✅ **Phase 1 (Thủ công)**: UC-128, UC-129 (chấm speaking thủ công)
2. ⏸️ **Phase 2 (AI)**: UC-130 với A8 (sau khi có Python API mock)

### Luồng thủ công:
```
Student upload audio/video (UC-125)
  ↓
Teacher nghe và chấm thủ công (UC-128, UC-129)
  ↓
Nhập:
  - pronunciation_score
  - fluency_score
  - accuracy_score
  - feedback
```

### Luồng AI (sau này):
```
Student upload audio/video (UC-125)
  ↓
Teacher trigger AI chấm (UC-130)
  ↓
Gọi Python API: POST /a8/analyze-media (hoặc analyze-transcript)
  - Input: file, mode, target_words, expected_text
  - Output: AnalyzeSpeakingResponse (scores, issues, suggestions, practice_plan)
  ↓
Lưu vào ai_feedback (UC-131)
  ↓
Teacher xem/chỉnh sửa (UC-131)
```

---

## 5. Session Reports (AI Summary)

### Use Cases liên quan AI:
- **UC-171**: AI generate summary từ Session Reports

### Use Cases thủ công (ưu tiên làm trước):
- **UC-163**: Teacher tạo Session Report
- **UC-164**: Teacher ghi feedback cho từng học sinh
- **UC-165**: Xem danh sách Session Reports
- **UC-166**: Xem chi tiết Session Report
- **UC-167**: Cập nhật Session Report
- **UC-168**: Filter Session Reports theo date range
- **UC-169**: Xem Session Reports của giáo viên trong tháng
- **UC-170**: Tổng hợp Session Reports theo date range - **THỦ CÔNG**
- **UC-172**: Teacher xem và chỉnh sửa AI summary
- **UC-173**: Đánh dấu Session Report đã được tổng hợp

### Thứ tự implement:
1. ✅ **Phase 1 (Thủ công)**: UC-163 → UC-170 (tổng hợp thủ công)
2. ⏸️ **Phase 2 (AI)**: UC-171 (sau khi có Python API mock)

---

## Tổng kết: Thứ tự ưu tiên implement

### 🟢 Phase 1: Thủ công (Làm ngay - không cần AI)

#### 1. Homework Module (Priority: HIGH)
- ✅ UC-117: Tạo Homework Assignment
- ✅ UC-118: Xem danh sách Homework
- ✅ UC-119: Xem chi tiết Homework
- ✅ UC-125: Học sinh nộp Homework
- ✅ UC-126: Xem danh sách Homework đã nộp
- ✅ UC-127: Xem chi tiết Homework submission
- ✅ UC-128: Teacher chấm Homework (THỦ CÔNG)
- ✅ UC-129: Nhập điểm và feedback (THỦ CÔNG)
- ✅ UC-132: Đánh dấu quá hạn
- ✅ UC-133: Xem lịch sử Homework

#### 2. Monthly Reports Module (Priority: HIGH)
- ✅ UC-174: Tạo Monthly Report Job
- ✅ UC-175: Gom dữ liệu (aggregate thủ công)
- ✅ UC-177: Xem danh sách Jobs
- ✅ UC-178: Xem trạng thái Job
- ✅ UC-179: Teacher xem draft
- ✅ UC-180: Teacher chỉnh sửa draft (THỦ CÔNG - tự viết)
- ✅ UC-181: Teacher submit
- ✅ UC-182: Staff/Admin comment
- ✅ UC-183: Staff/Admin approve
- ✅ UC-184: Staff/Admin reject
- ✅ UC-185: Publish
- ✅ UC-186: Parent/Student xem
- ✅ UC-187: Export PDF
- ✅ UC-187a: Gửi notification

#### 3. Finance/Payment Module (Priority: MEDIUM)
- ✅ UC-253: Tạo Invoice
- ✅ UC-254-257: Quản lý Invoice
- ✅ UC-264-266: Thanh toán
- ✅ UC-268: Upload chứng từ (THỦ CÔNG - nhập thông tin thủ công)
- ✅ UC-267: Xác nhận thanh toán

#### 4. Session Reports Module (Priority: MEDIUM)
- ✅ UC-163: Teacher tạo Session Report
- ✅ UC-164: Teacher ghi feedback
- ✅ UC-165-169: Xem/quản lý Session Reports
- ✅ UC-170: Tổng hợp thủ công

### ⏸️ Phase 2: AI Integration (Sau khi có Python API mock)

#### 1. A3 - Homework Grading
- ⏸️ UC-130: AI chấm Homework
- ⏸️ UC-131: Xem/chỉnh sửa kết quả AI

#### 2. A6 - Monthly Reports
- ⏸️ UC-176: AI tạo draft Monthly Report

#### 3. A7 - OCR Payment
- ⏸️ UC-268a: OCR extract payment proof (nếu thêm use case)

#### 4. A8 - Speaking/Phonics
- ⏸️ UC-130a: AI chấm Speaking/Phonics (nếu thêm use case)

#### 5. Session Reports AI Summary
- ⏸️ UC-171: AI generate summary từ Session Reports

---

## Gợi ý Implementation

### 1. Tạo AI Service Interfaces (sẵn sàng cho Phase 2)
```csharp
// Kidzgo.Application/Abstraction/AI/IAiHomeworkGradingService.cs
public interface IAiHomeworkGradingService
{
    Task<GradeResponse> GradeTextAsync(GradeTextRequest request, CancellationToken ct);
    Task<GradeResponse> GradeImageAsync(GradeImageRequest request, Stream imageStream, CancellationToken ct);
    Task<GradeResponse> GradeLinkAsync(GradeLinkRequest request, CancellationToken ct);
}

// Kidzgo.Application/Abstraction/AI/IAiMonthlyReportService.cs
public interface IAiMonthlyReportService
{
    Task<MonthlyReportResponse> GenerateMonthlyReportAsync(MonthlyReportRequest request, CancellationToken ct);
}

// Kidzgo.Application/Abstraction/AI/IAiOcrService.cs
public interface IAiOcrService
{
    Task<PaymentProofExtractResponse> ExtractPaymentProofAsync(PaymentProofExtractRequest request, Stream imageStream, CancellationToken ct);
}

// Kidzgo.Application/Abstraction/AI/IAiSpeakingService.cs
public interface IAiSpeakingService
{
    Task<AnalyzeSpeakingResponse> AnalyzeTranscriptAsync(AnalyzeTranscriptRequest request, CancellationToken ct);
    Task<AnalyzeSpeakingResponse> AnalyzeMediaAsync(AnalyzeMediaRequest request, Stream mediaStream, CancellationToken ct);
}
```

### 2. Mock Implementation cho Development
```csharp
// Kidzgo.Infrastructure/AI/MockAiHomeworkGradingService.cs
public class MockAiHomeworkGradingService : IAiHomeworkGradingService
{
    public Task<GradeResponse> GradeTextAsync(GradeTextRequest request, CancellationToken ct)
    {
        // Return mock data để FE test UI
        return Task.FromResult(new GradeResponse
        {
            Score = 8,
            MaxScore = 10,
            Summary = "Bài làm tốt, có vài lỗi nhỏ.",
            // ...
        });
    }
}
```

### 3. Real Implementation (khi có Python API)
```csharp
// Kidzgo.Infrastructure/AI/HttpAiHomeworkGradingService.cs
public class HttpAiHomeworkGradingService : IAiHomeworkGradingService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl; // Python API base URL

    public async Task<GradeResponse> GradeTextAsync(GradeTextRequest request, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/a3/grade-text", request, ct);
        return await response.Content.ReadFromJsonAsync<GradeResponse>(ct);
    }
}
```

---

## Kết luận

**Ưu tiên implement:**
1. ✅ **Homework Module** (thủ công) - UC-117 đến UC-129, UC-132, UC-133
2. ✅ **Monthly Reports Module** (thủ công) - UC-174, UC-175, UC-177-187
3. ✅ **Finance/Payment Module** (thủ công) - UC-253-270
4. ✅ **Session Reports Module** (thủ công) - UC-163-170, UC-172, UC-173

**Sau khi có Python API:**
- ⏸️ Tích hợp A3, A6, A7, A8 vào các use cases tương ứng

