# Monthly Reports Implementation - Phân tích thiếu sót

## 1. Domain Model - Thiếu fields/entities

### 1.1. ReportStatus enum - Thiếu Published
**Hiện tại:**
```csharp
public enum ReportStatus
{
    Draft,
    Review,
    Approved,
    Rejected
}
```

**Thiếu:**
- `Published` status - Cần để track khi report đã publish (UC-185)

**Cần thêm:**
```csharp
public enum ReportStatus
{
    Draft,
    Review,
    Approved,
    Rejected,
    Published  // <-- THIẾU
}
```

### 1.2. StudentMonthlyReport - Thiếu fields
**Thiếu:**
1. **PDF Export URL** - UC-187 cần export PDF nhưng không có field lưu URL
   ```csharp
   public string? PdfUrl { get; set; }
   public DateTime? PdfGeneratedAt { get; set; }
   ```

2. **ClassId** - UC-177 filter "theo lớp/học viên" nhưng không có link với Class
   ```csharp
   public Guid? ClassId { get; set; }
   public Class? Class { get; set; }
   ```

3. **JobId** - Không link với MonthlyReportJob để biết report thuộc job nào
   ```csharp
   public Guid? JobId { get; set; }
   public MonthlyReportJob? Job { get; set; }
   ```

4. **Timestamps** - Thiếu CreatedAt, UpdatedAt
   ```csharp
   public DateTime CreatedAt { get; set; }
   public DateTime UpdatedAt { get; set; }
   ```

5. **Version tracking** - Không track lịch sử chỉnh sửa (UC-180)
   ```csharp
   public int Version { get; set; } = 1;
   public string? LastEditedBy { get; set; }
   public DateTime? LastEditedAt { get; set; }
   ```

### 1.3. MonthlyReportJob - Thiếu fields
**Thiếu:**
1. **Timestamps**
   ```csharp
   public DateTime CreatedAt { get; set; }
   public DateTime UpdatedAt { get; set; }
   ```

2. **Error tracking** - Khi AI fail cần lưu error message
   ```csharp
   public string? ErrorMessage { get; set; }
   public int? RetryCount { get; set; }
   ```

3. **CreatedBy** - Ai tạo job
   ```csharp
   public Guid? CreatedBy { get; set; }
   public User? CreatedByUser { get; set; }
   ```

### 1.4. Thiếu entity: MonthlyReportData (Raw data aggregation)
**UC-175** nói "Gom dữ liệu cho Monthly Report (attendance, homework, test, mission, notes)" nhưng không có entity lưu raw data.

**Cần thêm:**
```csharp
public class MonthlyReportData : Entity
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid StudentProfileId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    
    // Aggregated data (JSON)
    public string? AttendanceData { get; set; }  // JSON: { total, present, absent, makeup }
    public string? HomeworkData { get; set; }    // JSON: { total, completed, pending, average }
    public string? TestData { get; set; }        // JSON: { tests: [{ type, score, date }] }
    public string? MissionData { get; set; }     // JSON: { completed, stars, xp }
    public string? NotesData { get; set; }       // JSON: { sessionReports: [...] }
    
    public DateTime CreatedAt { get; set; }
    
    public StudentMonthlyReport Report { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
}
```

---

## 2. Use Cases - Thiếu workflow

### 2.1. Thiếu: Tạo Monthly Report thủ công (không qua AI)
**Hiện tại:** Chỉ có UC-174 (Tạo Monthly Report Job) → AI generate

**Thiếu:** Teacher có thể tạo report thủ công từ đầu
- **UC-174a**: Teacher tạo Monthly Report thủ công (không qua AI job)
  - Teacher chọn student, month, year
  - Tạo report với status = Draft
  - Teacher tự viết content

### 2.2. Thiếu: Retry AI generation
**Hiện tại:** UC-178 có status FAILED nhưng không có cách retry

**Thiếu:**
- **UC-178a**: Retry AI generation khi job FAILED
- **UC-178b**: Xem error message khi AI fail

### 2.3. Thiếu: Xem lịch sử chỉnh sửa
**UC-180** nói "Teacher chỉnh sửa draft" nhưng không có use case xem lịch sử

**Thiếu:**
- **UC-180a**: Xem lịch sử chỉnh sửa Monthly Report (version history)

### 2.4. Thiếu: Unpublish
**UC-185** có Publish nhưng không có Unpublish

**Thiếu:**
- **UC-185a**: Unpublish Monthly Report (chuyển từ Published về Approved)

---

## 3. API Endpoints - Thiếu endpoints

### 3.1. Thiếu: Tạo report thủ công
```
POST /teacher/reports/manual
Body: { studentProfileId, month, year, classId?, draftContent }
```

### 3.2. Thiếu: Retry AI job
```
POST /reports/jobs/{jobId}/retry
```

### 3.3. Thiếu: Xem error khi AI fail
```
GET /reports/jobs/{jobId}/error
```

### 3.4. Thiếu: Xem version history
```
GET /reports/{reportId}/history
```

### 3.5. Thiếu: Unpublish
```
POST /reports/{reportId}/unpublish
```

### 3.6. Thiếu: Generate PDF (trigger)
```
POST /reports/{reportId}/generate-pdf
```

---

## 4. Workflow Integration - Thiếu

### 4.1. Notification Integration
**UC-187a** cần gửi notification khi publish nhưng:
- Không có integration với Notification module
- Không có event handler cho publish action

**Cần:**
- Domain event: `MonthlyReportPublishedEvent`
- Event handler: Gửi notification cho Parent/Student

### 4.2. AI Service Integration
**UC-176** cần AI service nhưng:
- Không có interface `IAiReportGenerator`
- Không có mock implementation cho development

**Cần:**
```csharp
public interface IAiReportGenerator
{
    Task<string> GenerateDraftAsync(MonthlyReportData data, CancellationToken ct);
}
```

### 4.3. PDF Generation Service
**UC-187** cần export PDF nhưng:
- Không có service generate PDF từ HTML/JSON
- Không có storage cho PDF files

**Cần:**
```csharp
public interface IPdfReportGenerator
{
    Task<string> GeneratePdfAsync(string htmlContent, CancellationToken ct);
}
```

---

## 5. Data Aggregation Logic - Thiếu

### 5.1. UC-175: Gom dữ liệu
**Thiếu implementation cho:**
- Aggregate attendance data (từ Attendance records)
- Aggregate homework data (từ Homework submissions)
- Aggregate test data (từ ExamResults)
- Aggregate mission data (từ Missions, Stars, XP)
- Aggregate session reports (từ SessionReports)

**Cần service:**
```csharp
public interface IMonthlyReportDataAggregator
{
    Task<MonthlyReportData> AggregateDataAsync(
        Guid studentProfileId, 
        int month, 
        int year, 
        CancellationToken ct);
}
```

---

## 6. Validation & Business Rules - Thiếu

### 6.1. Validation rules
- Một student chỉ có 1 report/tháng? (unique constraint)
- Chỉ publish khi status = Approved?
- Chỉ edit khi status = Draft hoặc Review?
- PDF chỉ generate khi status = Published?

### 6.2. Authorization rules
- Teacher chỉ xem/edit reports của lớp mình?
- Staff/Admin xem tất cả?
- Parent chỉ xem reports của con mình?

---

## 7. Error Handling - Thiếu

### 7.1. Error classes
- `MonthlyReportErrors.NotFound`
- `MonthlyReportErrors.AlreadyExists`
- `MonthlyReportErrors.InvalidStatus`
- `MonthlyReportErrors.AiGenerationFailed`
- `MonthlyReportErrors.PdfGenerationFailed`

---

## Tổng kết

### Priority 1 (Critical - Cần có ngay):
1. ✅ Thêm `Published` vào `ReportStatus`
2. ✅ Thêm `PdfUrl` vào `StudentMonthlyReport`
3. ✅ Thêm `ClassId` vào `StudentMonthlyReport`
4. ✅ Thêm `JobId` vào `StudentMonthlyReport`
5. ✅ Thêm timestamps (CreatedAt, UpdatedAt)
6. ✅ Tạo entity `MonthlyReportData` cho raw data
7. ✅ Tạo service `IMonthlyReportDataAggregator`
8. ✅ Tạo interface `IAiReportGenerator` (có mock)
9. ✅ Tạo interface `IPdfReportGenerator`
10. ✅ Tạo domain events cho publish

### Priority 2 (Important - Nên có):
11. ✅ Use case tạo report thủ công
12. ✅ Use case retry AI job
13. ✅ Use case unpublish
14. ✅ Error handling classes
15. ✅ Version tracking

### Priority 3 (Nice to have):
16. ✅ Version history API
17. ✅ Advanced error tracking trong Job
18. ✅ Audit log cho mọi thay đổi

