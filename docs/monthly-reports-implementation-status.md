# Monthly Reports Implementation Status - Phân tích Use Case 11 (Không tính AI)

## Tổng quan Use Case 11 (Không tính AI)

### 11.1. Tạo và quản lý Monthly Reports
- ✅ UC-174: Tạo Monthly Report Job
- ⚠️ UC-175: Gom dữ liệu cho Monthly Report (session reports, attendance, homework, test, mission, notes)
- ❌ UC-176: AI tạo draft Monthly Report (BỎ QUA - AI)
- ✅ UC-177: Xem danh sách Monthly Report Jobs
- ✅ UC-178: Xem trạng thái Monthly Report Job (PENDING/GENERATING/DONE/FAILED)

### 11.2. Review và Publish Monthly Reports
- ❌ UC-179: Teacher xem draft Monthly Report
- ❌ UC-180: Teacher chỉnh sửa draft Monthly Report
- ❌ UC-181: Teacher submit Monthly Report (REVIEW)
- ❌ UC-182: Staff/Admin comment Monthly Report
- ❌ UC-183: Staff/Admin approve Monthly Report
- ❌ UC-184: Staff/Admin reject Monthly Report
- ❌ UC-185: Publish Monthly Report
- ❌ UC-186: Parent/Student xem Monthly Report
- ❌ UC-187: Gửi thông báo khi publish Monthly Report

---

## 1. Domain Models - Đã có ✅

### 1.1. Entities đã có
- ✅ `StudentMonthlyReport` - Entity chính cho monthly report
  - Có đủ fields: Id, StudentProfileId, ClassId, JobId, Month, Year
  - Có DraftContent, FinalContent (JSONB)
  - Có Status (ReportStatus enum với Published)
  - Có PdfUrl, PdfGeneratedAt
  - Có SubmittedBy, ReviewedBy, ReviewedAt, PublishedAt
  - Có timestamps: CreatedAt, UpdatedAt
  - Có navigation properties: StudentProfile, Class, Job, SubmittedByUser, ReviewedByUser, ReportComments

- ✅ `MonthlyReportJob` - Entity cho job tạo reports
  - Có đủ fields: Id, Month, Year, BranchId, Status
  - Có StartedAt, FinishedAt, AiPayloadRef
  - Có ErrorMessage, RetryCount
  - Có CreatedBy, CreatedAt, UpdatedAt
  - Có navigation properties: Branch, CreatedByUser, Reports

- ✅ `MonthlyReportData` - Entity lưu raw aggregated data
  - Có đủ fields: Id, ReportId, StudentProfileId, Month, Year
  - Có AttendanceData, HomeworkData, TestData, MissionData, NotesData (JSONB)
  - Có CreatedAt, UpdatedAt
  - Có navigation properties: Report, StudentProfile
  - Có unique constraint: one data record per report

- ✅ `ReportComment` - Entity cho comments
  - Có navigation với StudentMonthlyReport và User

- ✅ `ReportStatus` enum
  - Có đủ: Draft, Review, Approved, Rejected, Published ✅

- ✅ `MonthlyReportJobStatus` enum
  - Có đủ: Pending, Generating, Done, Failed ✅

### 1.2. Error Handling
- ✅ `MonthlyReportErrors` class
  - Có đủ: NotFound, JobNotFound, AlreadyExists, InvalidStatus
  - Có: AiGenerationFailed, PdfGenerationFailed, DataAggregationFailed
  - Có: StudentProfileNotFound, ClassNotFound

---

## 2. Database & Infrastructure - Đã có ✅

### 2.1. DbContext
- ✅ `IDbContext` có DbSet cho:
  - MonthlyReportJobs
  - StudentMonthlyReports
  - ReportComments
- ⚠️ `ApplicationDbContext` có `MonthlyReportData` nhưng **THIẾU trong IDbContext interface**
  - Cần thêm: `DbSet<MonthlyReportData> MonthlyReportData { get; }` vào IDbContext
  - SessionReports
  - Attendances
  - HomeworkStudents
  - ExamResults
  - Missions, MissionProgresses
  - StarTransactions

### 2.2. Entity Configurations
- ✅ `StudentMonthlyReportConfiguration`
- ✅ `MonthlyReportJobConfiguration`
- ✅ `MonthlyReportDataConfiguration`
- ✅ Có migrations đã tạo

---

## 3. Data Sources - Đã có Entities ✅

### 3.1. Session Reports
- ✅ `SessionReport` entity
  - Có: SessionId, StudentProfileId, TeacherUserId, ReportDate
  - Có: Feedback, AiGeneratedSummary
  - Có: IsMonthlyCompiled (để đánh dấu đã dùng)
  - Có: CreatedAt, UpdatedAt

### 3.2. Attendance
- ✅ `Attendance` entity
  - Có: SessionId, StudentProfileId, AttendanceStatus, AbsenceType
  - Có: MarkedBy, MarkedAt, Note
  - Có navigation: Session, StudentProfile

### 3.3. Homework
- ✅ `HomeworkStudent` entity
  - Có: AssignmentId, StudentProfileId, Status
  - Có: SubmittedAt, GradedAt, Score, TeacherFeedback, AiFeedback
  - Có: Attachments

### 3.4. Exam Results
- ✅ `ExamResult` entity
  - Có: ExamId, StudentProfileId, Score, Comment
  - Có: AttachmentUrls, GradedBy, GradedAt
  - Có: CreatedAt, UpdatedAt

### 3.5. Missions
- ✅ `Mission` entity
- ✅ `MissionProgress` entity
  - Có: MissionId, StudentProfileId, Status, ProgressValue
  - Có: CompletedAt, VerifiedBy

### 3.6. Stars & XP
- ✅ `StarTransaction` entity
- ✅ `StudentLevel` entity (có trong DbContext)

---

## 4. Services & Interfaces - THIẾU ❌

### 4.1. Data Aggregation Service
- ⚠️ `IMonthlyReportDataAggregator` - **CHỈ CÓ INTERFACE, CHƯA CÓ IMPLEMENTATION**
  - Interface định nghĩa: `AggregateDataAsync(studentProfileId, month, year)`
  - Trả về: `Task<string>` (JSON string)
  - **CẦN**: Implementation class để:
    - Query SessionReports trong tháng
    - Query Attendances trong tháng
    - Query HomeworkStudents trong tháng
    - Query ExamResults trong tháng
    - Query MissionProgresses trong tháng
    - Query StarTransactions, StudentLevel
    - Aggregate thành JSON theo format của MonthlyReportData

### 4.2. Application Use Cases - THIẾU HOÀN TOÀN ❌

**Không có folder `MonthlyReports` trong `Kidzgo.Application`**

Cần tạo các use cases sau:

#### UC-174: Tạo Monthly Report Job
- `CreateMonthlyReportJob` command/handler
- Input: Month, Year, BranchId, CreatedBy
- Logic:
  - Tạo MonthlyReportJob với Status = Pending
  - Lưu vào database

#### UC-175: Gom dữ liệu cho Monthly Report
- `AggregateMonthlyReportData` command/handler
- Input: StudentProfileId, Month, Year, ReportId
- Logic:
  - Gọi IMonthlyReportDataAggregator.AggregateDataAsync()
  - Tạo MonthlyReportData với dữ liệu đã aggregate
  - Đánh dấu SessionReports đã dùng (IsMonthlyCompiled = true)
  - Lưu vào database

#### UC-177: Xem danh sách Monthly Report Jobs
- `GetMonthlyReportJobs` query/handler
- Input: BranchId?, Month?, Year?, Status?
- Output: List<MonthlyReportJob>

#### UC-178: Xem trạng thái Monthly Report Job
- `GetMonthlyReportJobById` query/handler
- Input: JobId
- Output: MonthlyReportJob với Reports

#### UC-179: Teacher xem draft Monthly Report
- `GetMonthlyReportById` query/handler
- Input: ReportId
- Output: StudentMonthlyReport với MonthlyReportData
- Authorization: Teacher chỉ xem reports của lớp mình

#### UC-180: Teacher chỉnh sửa draft Monthly Report
- `UpdateMonthlyReportDraft` command/handler
- Input: ReportId, DraftContent
- Logic:
  - Validate: Status phải là Draft hoặc Review
  - Update DraftContent
  - Update UpdatedAt

#### UC-181: Teacher submit Monthly Report
- `SubmitMonthlyReport` command/handler
- Input: ReportId, SubmittedBy
- Logic:
  - Validate: Status phải là Draft
  - Update Status = Review
  - Update SubmittedBy, UpdatedAt

#### UC-182: Staff/Admin comment Monthly Report
- `AddReportComment` command/handler
- Input: ReportId, Comment, CommentedBy
- Logic:
  - Tạo ReportComment
  - Lưu vào database

#### UC-183: Staff/Admin approve Monthly Report
- `ApproveMonthlyReport` command/handler
- Input: ReportId, ReviewedBy
- Logic:
  - Validate: Status phải là Review
  - Update Status = Approved
  - Update ReviewedBy, ReviewedAt, UpdatedAt

#### UC-184: Staff/Admin reject Monthly Report
- `RejectMonthlyReport` command/handler
- Input: ReportId, ReviewedBy
- Logic:
  - Validate: Status phải là Review
  - Update Status = Rejected
  - Update ReviewedBy, ReviewedAt, UpdatedAt

#### UC-185: Publish Monthly Report
- `PublishMonthlyReport` command/handler
- Input: ReportId
- Logic:
  - Validate: Status phải là Approved
  - Update Status = Published
  - Copy DraftContent → FinalContent (nếu chưa có)
  - Update PublishedAt, UpdatedAt
  - Trigger domain event: MonthlyReportPublishedEvent (cho UC-187)

#### UC-186: Parent/Student xem Monthly Report
- `GetMonthlyReportById` query/handler (dùng chung với UC-179)
- Authorization: Parent chỉ xem reports của con mình
- Output: StudentMonthlyReport với FinalContent (nếu Published)

#### UC-187: Gửi thông báo khi publish Monthly Report
- Event handler: `MonthlyReportPublishedEventHandler`
- Logic:
  - Lắng nghe domain event: MonthlyReportPublishedEvent
  - Gửi notification cho Parent/Student qua Zalo/Email/Push

---

## 5. API Controllers - THIẾU ❌

**Không có `MonthlyReportController` trong `Kidzgo.API/Controllers`**

Cần tạo controller với các endpoints:

```
POST   /api/monthly-reports/jobs                    - UC-174: Tạo Monthly Report Job
GET    /api/monthly-reports/jobs                    - UC-177: Xem danh sách Jobs
GET    /api/monthly-reports/jobs/{jobId}            - UC-178: Xem trạng thái Job
POST   /api/monthly-reports/jobs/{jobId}/aggregate  - UC-175: Gom dữ liệu (trigger cho tất cả students trong job)

GET    /api/monthly-reports/{reportId}              - UC-179, UC-186: Xem report
PUT    /api/monthly-reports/{reportId}/draft        - UC-180: Chỉnh sửa draft
POST   /api/monthly-reports/{reportId}/submit      - UC-181: Submit report
POST   /api/monthly-reports/{reportId}/approve     - UC-183: Approve report
POST   /api/monthly-reports/{reportId}/reject      - UC-184: Reject report
POST   /api/monthly-reports/{reportId}/publish     - UC-185: Publish report

POST   /api/monthly-reports/{reportId}/comments    - UC-182: Thêm comment
GET    /api/monthly-reports/{reportId}/comments    - Xem comments
```

---

## 6. Validation & Business Rules - CẦN IMPLEMENT ❌

### 6.1. Validation Rules
- ✅ Unique constraint: Một student chỉ có 1 report/tháng (có trong MonthlyReportErrors.AlreadyExists)
- ❌ Chỉ publish khi status = Approved (cần validate trong handler)
- ❌ Chỉ edit khi status = Draft hoặc Review (cần validate trong handler)
- ❌ PDF chỉ generate khi status = Published (chưa có use case này)

### 6.2. Authorization Rules
- ❌ Teacher chỉ xem/edit reports của lớp mình (cần implement trong handlers)
- ❌ Staff/Admin xem tất cả (cần implement trong handlers)
- ❌ Parent chỉ xem reports của con mình (cần implement trong handlers)

---

## 7. Domain Events - THIẾU ❌

- ❌ `MonthlyReportPublishedEvent` - Cần tạo để trigger notification (UC-187)

---

## 8. Integration với Notification Module - THIẾU ❌

- ❌ Event handler: `MonthlyReportPublishedEventHandler`
- ❌ Cần gửi notification qua Zalo/Email/Push khi publish

---

## Tổng kết: Những gì CẦN LÀM để implement Use Case 11 (không tính AI)

### ✅ Đã có (Domain & Infrastructure):
1. ✅ Domain entities: StudentMonthlyReport, MonthlyReportJob, MonthlyReportData, ReportComment
2. ✅ Enums: ReportStatus, MonthlyReportJobStatus
3. ✅ Error classes: MonthlyReportErrors
4. ✅ Entity configurations
5. ✅ Database migrations
6. ✅ DbContext có đủ DbSets
7. ✅ Data source entities: SessionReport, Attendance, HomeworkStudent, ExamResult, Mission, MissionProgress

### ⚠️ Có interface nhưng chưa có implementation:
1. ⚠️ `IMonthlyReportDataAggregator` - CẦN implementation class

### ❌ Thiếu hoàn toàn (Application & API):
1. ❌ **Application Use Cases** (13 use cases):
   - UC-174: CreateMonthlyReportJob
   - UC-175: AggregateMonthlyReportData
   - UC-177: GetMonthlyReportJobs
   - UC-178: GetMonthlyReportJobById
   - UC-179: GetMonthlyReportById
   - UC-180: UpdateMonthlyReportDraft
   - UC-181: SubmitMonthlyReport
   - UC-182: AddReportComment
   - UC-183: ApproveMonthlyReport
   - UC-184: RejectMonthlyReport
   - UC-185: PublishMonthlyReport
   - UC-186: GetMonthlyReportById (dùng chung với UC-179)
   - UC-187: MonthlyReportPublishedEventHandler

2. ❌ **API Controller**: MonthlyReportController với 10+ endpoints

3. ❌ **Domain Events**: MonthlyReportPublishedEvent

4. ❌ **Validation & Authorization**: Business rules trong handlers

5. ❌ **Integration**: Notification handler cho UC-187

---

## Kết luận

**Use Case 11 CHƯA ĐỦ để implement** (không tính AI). 

### Đã có:
- ✅ Domain models đầy đủ
- ✅ Database schema đầy đủ
- ✅ Data source entities đầy đủ

### Còn thiếu:
- ❌ **100% Application layer** (use cases, handlers, commands, queries)
- ❌ **100% API layer** (controller, endpoints)
- ❌ **Implementation của IMonthlyReportDataAggregator**
- ❌ **Domain events và event handlers**
- ❌ **Validation & Authorization logic**

### Ưu tiên implement:
1. **Priority 1**: IMonthlyReportDataAggregator implementation
2. **Priority 2**: UC-174, UC-175, UC-177, UC-178 (tạo job và aggregate data)
3. **Priority 3**: UC-179, UC-180, UC-181 (teacher workflow)
4. **Priority 4**: UC-182, UC-183, UC-184 (staff/admin workflow)
5. **Priority 5**: UC-185, UC-186, UC-187 (publish và notification)

