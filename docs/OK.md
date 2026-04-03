# Luồng: Monthly Report Theo Lớp/Chương Trình

Ngày cập nhật: 2026-04-03

Màn hình: `Monthly Report Jobs -> Monthly Reports -> Draft / Review / Publish`

## 1. Phạm vi và nguồn kiểm tra

Tài liệu này được đối chiếu trực tiếp từ code hiện tại, chủ yếu ở các file:

- `Kidzgo.API/Controllers/MonthlyReportController.cs`
- `Kidzgo.API/Requests/MonthlyReportRequests.cs`
- `Kidzgo.Application/MonthlyReports/*`
- `Kidzgo.Infrastructure/Services/MonthlyReportDataAggregator.cs`
- `Kidzgo.Infrastructure/AI/HttpAiReportGenerator.cs`
- `Kidzgo.Domain/Reports/*`
- `AI-KidzGo/app/agents/a6_reports/*`

Ngoài phạm vi:

- Giao diện Web Portal / Zalo Mini App
- Notification UI sau khi publish report
- PDF template / giao diện trình bày nội dung PDF
- Các flow `Session Report`, `Attendance`, `Homework`, `Exam`, `Mission` ngoài phần dùng để aggregate cho monthly report

## 2. Tóm tắt luồng nghiệp vụ hiện tại

1. `Admin` hoặc `ManagementStaff` tạo `Monthly Report Job` theo `branch + month + year`.
2. `Admin` hoặc `ManagementStaff` trigger aggregate dữ liệu cho job.
3. Hệ thống tạo hoặc cập nhật `StudentMonthlyReport` theo từng cặp `student + class + month + year`.
4. Với case một học sinh học 2 lớp / 2 chương trình trong cùng tháng, hệ thống tạo 2 report riêng.
5. Dữ liệu aggregate hiện được scope theo `ClassId` cho `attendance`, `homework`, `test`, `session report`, `topics`.
6. `mission` hiện được scope theo `ClassId` cho phần mission của lớp; `current level/xp` vẫn là dữ liệu toàn cục của học sinh.
7. `Teacher` của lớp gọi AI để sinh draft report.
8. `Teacher` có thể chỉnh sửa draft và submit sang trạng thái review.
9. `Admin` hoặc `ManagementStaff` comment, approve hoặc reject report.
10. Khi report được approve, `Admin` hoặc `ManagementStaff` có thể publish.
11. Có thể generate / regenerate PDF khi report đã có nội dung.
12. `Parent` hiện chỉ xem được report `Published` của con mình.

## 3. Role, dữ liệu được xem và phạm vi dữ liệu

### 3.1. Mỗi role được xem dữ liệu gì

| Role | Dữ liệu xem được trong luồng này |
| --- | --- |
| `Admin` | Toàn bộ job monthly report, toàn bộ student monthly report, dữ liệu aggregate, comment, PDF |
| `ManagementStaff` | Toàn bộ job monthly report, toàn bộ student monthly report, dữ liệu aggregate, comment, PDF |
| `Teacher` | Danh sách / chi tiết monthly report của các lớp mà mình là `MainTeacher` hoặc `AssistantTeacher` |
| `Parent` | Danh sách / chi tiết monthly report `Published` của các học sinh có liên kết với phụ huynh |
| `Student` | Hiện chưa có API access ở controller, dù query handler có logic profile student |
| `AccountantStaff` | Không có API nào trong luồng này |

### 3.2. Phạm vi dữ liệu (own / department / all)

| Role | Phạm vi dữ liệu hiện tại |
| --- | --- |
| `Admin` | `all` |
| `ManagementStaff` | `all` |
| `Teacher` | `own classes` |
| `Parent` | `own children`, chỉ `published` |
| `Student` | Chưa có access thực tế qua controller |
| `AccountantStaff` | Không có access |

Ghi chú implementation hiện tại:

- Monthly report hiện được định danh theo `student + class + month + year`, không còn chỉ theo `student + month + year`.
- Một học sinh học 2 lớp / 2 chương trình trong cùng tháng sẽ có 2 report riêng.
- `Teacher` bị giới hạn theo lớp của mình ở query handler.
- `Parent` chỉ xem được report đã `Published`.
- `Student` có logic filter ở query handler nhưng controller chưa mở role `Student`.

### 3.3. Các hành động được phép

| Module | Admin | ManagementStaff | Teacher | Parent | Student | AccountantStaff |
| --- | --- | --- | --- | --- | --- | --- |
| `Monthly Report Jobs` | view, create, aggregate | view, create, aggregate | không có | không có | không có | không có |
| `Monthly Reports list/detail` | view all | view all | view own classes | view published của con | chưa có API access | không có |
| `AI Draft` | không có | không có | generate own classes | không có | không có | không có |
| `Edit Draft` | không có | không có | edit own classes | không có | không có | không có |
| `Submit Review` | không có | không có | submit theo role `Teacher`; handler chưa enforce own class | không có | không có | không có |
| `Comment` | create | create | không có | không có | không có | không có |
| `Approve / Reject` | approve, reject | approve, reject | không có | không có | không có | không có |
| `Publish` | publish | publish | không có | không có | không có | không có |
| `Generate PDF` | generate | generate | generate theo role `Teacher`; handler chưa enforce own class | không có | không có | không có |

## 4. Response contract chung

### 4.1. Success response mặc định

Hầu hết endpoint dùng `MatchOk()` hoặc `MatchCreated()` và trả envelope:

```json
{
  "isSuccess": true,
  "data": {}
}
```

- `POST` tạo mới: HTTP `201 Created`
- `GET/PUT/POST action`: HTTP `200 OK`

### 4.2. Error response business / domain

Các lỗi business từ `Result.Failure(...)` được map sang `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "MonthlyReport.InvalidStatus",
  "status": 400,
  "detail": "Cannot publish Monthly Report with status 'Draft'"
}
```

Map HTTP code hiện tại:

- `Validation` -> `400`
- `NotFound` -> `404`
- `Conflict` -> `409`
- `Failure` -> `500`

### 4.3. Error response auth

- `401 Unauthorized`: thiếu token / token không hợp lệ
- `403 Forbidden`: có token nhưng không đủ role

## 5. Danh sách API theo màn hình

## 5.1. Monthly Report Jobs

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `POST /api/monthly-reports/jobs` | `Admin`, `ManagementStaff` | Tạo job monthly report cho một chi nhánh và một tháng | Body: `month:int` bắt buộc; `year:int` bắt buộc; `branchId:Guid` bắt buộc | `201`; `data:{ id, month, year, branchId, status="Pending", createdBy, createdAt }` | `400 MonthlyReport.InvalidMonth`; `400 MonthlyReport.InvalidYear`; `404 Branch.NotFound` |
| `GET /api/monthly-reports/jobs` | `Admin`, `ManagementStaff` | Danh sách job monthly report | Query: `branchId?:Guid`; `month?:int`; `year?:int`; `status?:string`; `pageNumber:int=1`; `pageSize:int=10` | `200`; `data:{ jobs:{ items[], pageNumber, totalPages, totalCount, hasPreviousPage, hasNextPage } }`; mỗi item có `id, month, year, branchId, branchName, status, startedAt, finishedAt, errorMessage, retryCount, createdBy, createdByName, createdAt, reportCount` | Sai `status` string hiện bị bỏ qua, không trả `400` |
| `GET /api/monthly-reports/jobs/{jobId}` | `Admin`, `ManagementStaff` | Xem chi tiết một job và các report con | Path: `jobId:Guid` | `200`; `data:{ id, month, year, branchId, branchName, status, startedAt, finishedAt, errorMessage, retryCount, createdBy, createdByName, createdAt, updatedAt, reportCount, reports[] }` | `404 MonthlyReportJob.NotFound` |
| `POST /api/monthly-reports/jobs/{jobId}/aggregate` | `Admin`, `ManagementStaff` | Trigger aggregate dữ liệu cho toàn bộ enrollment trong job | Path: `jobId:Guid` | `200`; `data:{ totalReportsCreated, totalReportsUpdated, reportIds[], durationMs, totalEnrollmentsProcessed }` | `404 MonthlyReportJob.NotFound`; `500 MonthlyReport.DataAggregationFailed` khi có lỗi tầng aggregate lớn |

Monthly Report Job notes và caveats hiện tại:

- Aggregate job hiện lấy enrollment `Active` hoặc `Paused` trong branch.
- Aggregate hiện group theo `student + class`, không còn chỉ theo `student`.
- `MonthlyReportJobStatus.Failed` có trong enum nhưng handler aggregate hiện không set explicit `Failed` khi có lỗi toàn cục; lỗi cục bộ theo enrollment sẽ log rồi tiếp tục.

## 5.2. Monthly Reports

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `GET /api/monthly-reports` | `Teacher`, `Admin`, `ManagementStaff`, `Parent` | Danh sách monthly report có filter | Query: `studentProfileId?:Guid`; `classId?:Guid`; `jobId?:Guid`; `month?:int`; `year?:int`; `status?:string`; `pageNumber:int=1`; `pageSize:int=10` | `200`; `data:{ reports:{ items[], pageNumber, totalPages, totalCount, hasPreviousPage, hasNextPage } }`; mỗi item có `id, studentProfileId, studentName, classId, className, programId, programName, jobId, month, year, status, publishedAt, createdAt, updatedAt` | `404 User.NotFound`; `404 MonthlyReport.Unauthorized` khi parent không có profile; sai `status` string hiện bị bỏ qua |
| `GET /api/monthly-reports/{reportId}` | `Teacher`, `Admin`, `ManagementStaff`, `Parent` | Xem chi tiết report và dữ liệu aggregate | Path: `reportId:Guid` | `200`; `data:{ id, studentProfileId, studentName, classId, className, programId, programName, jobId, month, year, draftContent, finalContent, status, pdfUrl, pdfGeneratedAt, submittedBy, submittedByName, reviewedBy, reviewedByName, reviewedAt, publishedAt, createdAt, updatedAt, data:{ attendanceData, homeworkData, testData, missionData, notesData, topicsData }, comments[] }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.Unauthorized`; `400 MonthlyReport.NotPublished`; `404 User.NotFound` |
| `POST /api/monthly-reports/{reportId}/generate-draft` | `Teacher` | Dùng AI sinh draft monthly report từ dữ liệu aggregate mới nhất | Path: `reportId:Guid` | `200`; `data:{ id, draftContent, updatedAt }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.Unauthorized`; `400 MonthlyReport.NoData`; `500 MonthlyReport.AiGenerationFailed`; `500 MonthlyReport.AiServiceUnavailable`; `500 MonthlyReport.EmptyDraft` |
| `PUT /api/monthly-reports/{reportId}/draft` | `Teacher` | Chỉnh sửa draft content của report | Path: `reportId:Guid`; Body: `draftContent:string` bắt buộc | `200`; `data:{ id, draftContent, updatedAt }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.InvalidStatus`; `400 MonthlyReport.Unauthorized` |
| `POST /api/monthly-reports/{reportId}/submit` | `Teacher` | Submit report sang review | Path: `reportId:Guid` | `200`; `data:{ id, status="Review", submittedBy, updatedAt }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.InvalidStatus` |
| `POST /api/monthly-reports/{reportId}/comments` | `Admin`, `ManagementStaff` | Thêm comment vào report | Path: `reportId:Guid`; Body: `content:string` bắt buộc | `201`; `data:{ id, reportId, commenterId, content, createdAt }` | `404 MonthlyReport.NotFound` |
| `POST /api/monthly-reports/{reportId}/approve` | `Admin`, `ManagementStaff` | Approve report đang review | Path: `reportId:Guid` | `200`; `data:{ id, status="Approved", reviewedBy, reviewedAt, updatedAt }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.InvalidStatus` |
| `POST /api/monthly-reports/{reportId}/reject` | `Admin`, `ManagementStaff` | Reject report đang review | Path: `reportId:Guid` | `200`; `data:{ id, status="Rejected", reviewedBy, reviewedAt, updatedAt }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.InvalidStatus` |
| `POST /api/monthly-reports/{reportId}/publish` | `Admin`, `ManagementStaff` | Publish report đã approved để phụ huynh xem | Path: `reportId:Guid` | `200`; `data:{ id, status="Published", publishedAt, updatedAt, pdfUrl }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.InvalidStatus` |
| `POST /api/monthly-reports/{reportId}/generate-pdf` | `Teacher`, `Admin`, `ManagementStaff` | Generate hoặc regenerate PDF cho report đã có content | Path: `reportId:Guid` | `200`; `data:{ reportId, pdfUrl, pdfGeneratedAt }` | `404 MonthlyReport.NotFound`; `400 MonthlyReport.NoContent`; `400 MonthlyReport.InvalidStatus`; `500 MonthlyReport.PdfGenerationFailed` |

Monthly Report notes và caveats hiện tại:

- Aggregate hiện tạo report theo `student + class + month + year`, nên một học sinh có thể có nhiều report trong cùng tháng.
- `programId/programName` hiện được lấy từ `report.Class.Program`.
- `data.attendanceData`, `homeworkData`, `testData`, `missionData`, `notesData`, `topicsData` hiện là JSON string, không phải object đã parse sẵn.
- `GenerateDraft` luôn refresh lại aggregate class-scoped trước khi gọi AI.
- `UpdateDraft` nếu content không phải JSON hợp lệ sẽ tự bọc thành JSON string.
- `UpdateDraft` cho phép sửa khi status là `Draft`, `Review`, `Rejected`; nếu đang `Rejected` thì sau khi sửa sẽ tự quay về `Draft`.
- `GenerateDraft` hiện không check status hiện tại của report; nếu teacher của lớp gọi lại thì handler sẽ set report về `Draft`.
- `Publish` sẽ copy `DraftContent` sang `FinalContent` nếu `FinalContent` đang rỗng.
- `Parent` chỉ xem được report `Published`.
- `Student` chưa gọi được API vì controller chưa mở role `Student`, dù query handler có logic profile student.
- `Submit` hiện chỉ bị chặn theo role `Teacher` ở controller; handler chưa verify teacher có thuộc lớp của report hay không.
- `GeneratePDF` hiện chỉ bị chặn theo role ở controller; handler chưa verify teacher ownership theo lớp.

## 6. Dữ liệu aggregate của Monthly Report

### 6.1. AttendanceData

JSON string dạng:

```json
{
  "total": 12,
  "present": 10,
  "absent": 1,
  "makeup": 1,
  "notMarked": 0,
  "percentage": 91.67
}
```

### 6.2. HomeworkData

JSON string dạng:

```json
{
  "total": 8,
  "completed": 5,
  "submitted": 2,
  "pending": 1,
  "late": 0,
  "missing": 0,
  "average": 86.5,
  "completionRate": 87.5,
  "topics": ["Family", "Present Simple"],
  "skills": ["Reading", "Writing"],
  "grammarTags": ["Present Simple"],
  "vocabularyTags": ["Family members"],
  "speakingAssignments": 2,
  "aiSupportedAssignments": 3
}
```

### 6.3. TestData

JSON string dạng:

```json
{
  "total": 2,
  "tests": [
    {
      "examId": "GUID",
      "type": "Midterm",
      "score": 18,
      "maxScore": 20,
      "date": "2026-03-20",
      "comment": "Good progress"
    }
  ]
}
```

### 6.4. MissionData

JSON string dạng:

```json
{
  "completed": 3,
  "total": 4,
  "inProgress": 1,
  "stars": 20,
  "xp": 100,
  "currentLevel": "5",
  "currentXp": 1200
}
```

Ghi chú implementation hiện tại:

- `completed/total/inProgress/stars/xp` đang phản ánh mission `Scope=Class` của đúng `report.ClassId` trong tháng.
- `currentLevel/currentXp` là dữ liệu toàn cục của học sinh, không tách theo lớp.

### 6.5. NotesData

JSON string dạng:

```json
{
  "total": 4,
  "sessionReports": [
    {
      "sessionId": "GUID",
      "sessionDate": "2026-03-10",
      "reportDate": "2026-03-10",
      "teacherName": "Teacher A",
      "feedback": "Bé tập trung tốt",
      "aiGeneratedSummary": null
    }
  ]
}
```

### 6.6. TopicsData

JSON string dạng:

```json
{
  "total": 3,
  "topics": ["Unit 1", "Unit 2", "Unit 3"],
  "lessonContents": ["Family vocabulary", "Present simple review"]
}
```

## 7. Status definition và luồng chuyển trạng thái

### 7.1. MonthlyReportJobStatus

| Status | Ý nghĩa |
| --- | --- |
| `Pending` | Job mới tạo, chưa aggregate |
| `Generating` | Đang chạy aggregate dữ liệu |
| `Done` | Aggregate hoàn tất |
| `Failed` | Có trong enum, nhưng hiện chưa được set explicit ở handler aggregate |

Luồng chuyển trạng thái hiện tại:

- Tạo job -> `Pending`
- Trigger aggregate -> `Generating`
- Aggregate hoàn tất -> `Done`

Ghi chú:

- Handler hiện không set `Failed` ở cuối luồng khi có lỗi toàn cục; lỗi theo từng enrollment sẽ log và tiếp tục.

### 7.2. ReportStatus

| Status | Ý nghĩa |
| --- | --- |
| `Draft` | Report ở trạng thái nháp |
| `Review` | Teacher đã submit chờ staff/admin review |
| `Approved` | Staff/Admin đã duyệt |
| `Rejected` | Staff/Admin đã từ chối, chờ giáo viên sửa lại |
| `Published` | Đã phát hành cho phụ huynh xem |

Luồng chuyển trạng thái chính:

- Aggregate tạo report mới -> `Draft`
- Generate draft AI -> giữ `Draft`
- Generate draft AI hiện có thể reset report về `Draft` từ trạng thái khác nếu teacher của lớp gọi endpoint
- Edit draft khi đang `Rejected` -> quay về `Draft`
- Submit -> `Draft -> Review`
- Approve -> `Review -> Approved`
- Reject -> `Review -> Rejected`
- Publish -> `Approved -> Published`

Rule hiện tại:

- `Submit` chỉ cho phép khi report đang `Draft`
- `Approve` chỉ cho phép khi report đang `Review`
- `Reject` chỉ cho phép khi report đang `Review`
- `Publish` chỉ cho phép khi report đang `Approved`
- `UpdateDraft` cho phép khi report là `Draft`, `Review`, `Rejected`
- `GenerateDraft` hiện không chặn theo status hiện tại; handler luôn set report về `Draft`

## 8. Permission matrix theo role

| Capability | Admin | ManagementStaff | Teacher | Parent | Student | AccountantStaff |
| --- | --- | --- | --- | --- | --- | --- |
| Create monthly report job | Y | Y | - | - | - | - |
| View monthly report jobs | Y | Y | - | - | - | - |
| View monthly report job detail | Y | Y | - | - | - | - |
| Aggregate monthly report data | Y | Y | - | - | - | - |
| View monthly report list | Y | Y | own classes | own children published | - | - |
| View monthly report detail | Y | Y | own classes | own children published | - | - |
| Generate AI draft | - | - | own classes | - | - | - |
| Edit draft | - | - | own classes | - | - | - |
| Submit report | - | - | role `Teacher`; chưa enforce own class | - | - | - |
| Comment report | Y | Y | - | - | - | - |
| Approve report | Y | Y | - | - | - | - |
| Reject report | Y | Y | - | - | - | - |
| Publish report | Y | Y | - | - | - | - |
| Generate PDF | Y | Y | role `Teacher`; chưa enforce own class | - | - | - |

Ký hiệu:

- `Y`: được phép
- `own classes`: chỉ lớp mình đang dạy
- `own children published`: chỉ report đã publish của con mình
- `-`: không có quyền

## 9. Validation rule và rule kiểm tra dữ liệu

### 9.1. Monthly Report Job

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| `month` phải từ `1..12` | create job | `400 MonthlyReport.InvalidMonth` |
| `year` phải từ `2000..2100` | create job | `400 MonthlyReport.InvalidYear` |
| `branchId` phải tồn tại | create job | `404 Branch.NotFound` |
| `jobId` phải tồn tại | get job by id / aggregate | `404 MonthlyReportJob.NotFound` |

### 9.2. Monthly Reports list / detail

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| User hiện tại phải tồn tại | list / detail | `404 User.NotFound` |
| Teacher chỉ xem được report của lớp mình dạy | list / detail | `400 MonthlyReport.Unauthorized` |
| Parent phải có profile | list / detail | `404 MonthlyReport.Unauthorized` |
| Parent chỉ xem được report của con mình | detail | `400 MonthlyReport.Unauthorized` |
| Parent chỉ xem được report `Published` | detail | `400 MonthlyReport.NotPublished` |
| `status` query sai enum | list jobs / list reports | hiện bị bỏ qua, không trả `400` |

### 9.3. Draft / Review / Publish

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| Chỉ `Teacher` mới generate draft | generate draft | `400 MonthlyReport.Unauthorized` |
| Teacher chỉ generate/edit report của lớp mình | generate draft / update draft | `400 MonthlyReport.Unauthorized` |
| Không có aggregated data để generate | generate draft | `400 MonthlyReport.NoData` |
| AI trả draft rỗng | generate draft | `500 MonthlyReport.EmptyDraft` |
| AI service unreachable | generate draft | `500 MonthlyReport.AiServiceUnavailable` |
| Chỉ submit khi `Draft` | submit | `400 MonthlyReport.InvalidStatus` |
| Chỉ approve khi `Review` | approve | `400 MonthlyReport.InvalidStatus` |
| Chỉ reject khi `Review` | reject | `400 MonthlyReport.InvalidStatus` |
| Chỉ publish khi `Approved` | publish | `400 MonthlyReport.InvalidStatus` |
| Update draft chỉ cho `Draft`, `Review`, `Rejected` | update draft | `400 MonthlyReport.InvalidStatus` |
| Khi update draft, nếu content không phải JSON hợp lệ | update draft | không lỗi; handler tự serialize thành JSON string |
| Generate draft hiện không chặn theo status hiện tại | generate draft | không có lỗi status; handler set lại `Draft` |
| Submit chưa verify teacher ownership theo lớp | submit | hiện không có lỗi ownership ở handler |
| Generate PDF chưa verify teacher ownership theo lớp | generate pdf | hiện không có lỗi ownership ở handler |

### 9.4. PDF

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| Report phải tồn tại | generate pdf | `404 MonthlyReport.NotFound` |
| Report phải có `DraftContent` hoặc `FinalContent` | generate pdf | `400 MonthlyReport.NoContent` |
| Draft không có nội dung thì không generate PDF | generate pdf | `400 MonthlyReport.InvalidStatus` |
| Lỗi generator/storage | generate pdf | `500 MonthlyReport.PdfGenerationFailed` |

## 10. Các trường hợp trả lỗi nổi bật theo luồng

### 10.1. Tạo và chạy Job

- Tạo job với `month = 13` -> `400 MonthlyReport.InvalidMonth`
- Tạo job với `year = 1999` -> `400 MonthlyReport.InvalidYear`
- Tạo job với `branchId` không tồn tại -> `404 Branch.NotFound`
- Aggregate job không tồn tại -> `404 MonthlyReportJob.NotFound`

### 10.2. Xem report

- Teacher xem report không thuộc lớp mình -> `400 MonthlyReport.Unauthorized`
- Parent xem report không thuộc con mình -> `400 MonthlyReport.Unauthorized`
- Parent xem report chưa publish -> `400 MonthlyReport.NotPublished`
- Report không tồn tại -> `404 MonthlyReport.NotFound`

### 10.3. Sinh draft và chỉnh sửa

- Teacher khác lớp gọi `generate-draft` -> `400 MonthlyReport.Unauthorized`
- AI service không chạy -> `500 MonthlyReport.AiServiceUnavailable`
- AI trả draft rỗng -> `500 MonthlyReport.EmptyDraft`
- Edit report đang `Approved` hoặc `Published` -> `400 MonthlyReport.InvalidStatus`

### 10.4. Submit / Approve / Reject / Publish

- Submit report không ở trạng thái `Draft` -> `400 MonthlyReport.InvalidStatus`
- Approve report không ở trạng thái `Review` -> `400 MonthlyReport.InvalidStatus`
- Reject report không ở trạng thái `Review` -> `400 MonthlyReport.InvalidStatus`
- Publish report không ở trạng thái `Approved` -> `400 MonthlyReport.InvalidStatus`

### 10.5. Generate PDF

- Generate PDF khi report chưa có content -> `400 MonthlyReport.NoContent`
- Generate PDF lỗi ở tầng PDF/storage -> `500 MonthlyReport.PdfGenerationFailed`

## 11. Current implementation notes cần lưu ý khi làm tài liệu/UI

1. Monthly report hiện tạo theo `student + class + month + year`, nên một học sinh có thể có nhiều report trong cùng tháng.
2. Điều này giải quyết case `1 bé học 2 chương trình / 2 lớp` bằng cách tạo 2 report riêng.
3. `attendance`, `homework`, `test`, `notes`, `topics` hiện đều aggregate theo `ClassId`.
4. `homeworkData` đã hấp thụ thêm metadata mới như `topics`, `skills`, `grammarTags`, `vocabularyTags`, `speakingAssignments`, `aiSupportedAssignments`.
5. `missionData` hiện tách theo lớp cho phần mission count/stars/xp của tháng, nhưng `currentLevel/currentXp` vẫn là dữ liệu global của học sinh.
6. `GET /api/monthly-reports/{reportId}` hiện trả `programId`, `programName`, `topicsData`; trước đó các field này chưa đi ra response đầy đủ.
7. `Student` chưa có quyền gọi API monthly report ở controller, dù query handler có logic filter theo student profile.
8. `MonthlyReportJobStatus.Failed` có trong enum nhưng chưa thấy handler aggregate set explicit ở luồng hiện tại.
9. `UpdateDraft` hiện cho phép sửa cả khi report đang `Review`; nếu nghiệp vụ muốn khóa hoàn toàn khi đã submit thì cần chỉnh thêm.
10. `GenerateDraft` hiện không check status hiện tại và sẽ set report về `Draft`; điều này có thể reopen cả report đã `Approved` hoặc `Published` nếu teacher của lớp gọi endpoint.
11. `Submit` hiện thiếu ownership check theo lớp ở handler; bất kỳ user role `Teacher` có `reportId` hợp lệ đều có thể submit.
12. `GeneratePDF` hiện thiếu ownership check theo lớp ở handler; teacher có thể generate PDF nếu biết `reportId`.
13. `data.*` trong detail response hiện là JSON string; FE nếu muốn render đẹp cần tự parse.
