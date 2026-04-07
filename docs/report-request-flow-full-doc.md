# Luồng Report Request: Admin giao yêu cầu làm báo cáo ưu tiên cho giáo viên

Ngày cập nhật: 2026-04-07

Màn hình gợi ý: `Report Requests -> Session Reports -> Monthly Reports -> Notifications`

## 1. Phạm vi và nguồn kiểm tra

Tài liệu này mô tả phần thay đổi mới cho yêu cầu:

> Admin có thể gửi yêu cầu làm báo cáo ưu tiên cho một học sinh cụ thể hoặc một lớp cụ thể cho Teacher để Teacher làm trước và gửi lại cho Admin duyệt. Teacher nhận được thông báo trực tiếp.

Các file chính liên quan:

| Nhóm | File |
| --- | --- |
| API | `Kidzgo.API/Controllers/ReportRequestController.cs` |
| Request DTO | `Kidzgo.API/Requests/ReportRequestRequests.cs` |
| Application | `Kidzgo.Application/ReportRequests/*` |
| Workflow sync | `Kidzgo.Application/ReportRequests/Shared/ReportRequestWorkflow.cs` |
| Session report hook | `Kidzgo.Application/SessionReports/SubmitSessionReport/*`, `ApproveSessionReport/*`, `RejectSessionReport/*` |
| Monthly report hook | `Kidzgo.Application/MonthlyReports/SubmitMonthlyReport/*`, `ApproveMonthlyReport/*`, `RejectMonthlyReport/*` |
| Domain | `Kidzgo.Domain/Reports/ReportRequest.cs`, `ReportRequestType.cs`, `ReportRequestStatus.cs`, `ReportRequestPriority.cs` |
| Error | `Kidzgo.Domain/Reports/Errors/ReportRequestErrors.cs` |
| EF config | `Kidzgo.Infrastructure/Configuration/ReportRequestConfiguration.cs` |
| Migration | `Kidzgo.Infrastructure/Migrations/20260407042557_AddReportRequests.cs` |

Ngoài phạm vi:

| Ngoài phạm vi | Ghi chú |
| --- | --- |
| UI Web Portal | Backend đã có API, FE cần build màn danh sách request, form tạo request và notification UI |
| Realtime notification transport | Backend đã tạo `Notification`; việc hiển thị realtime phụ thuộc luồng notification hiện có |
| Thay thế SessionReport/MonthlyReport | `ReportRequest` là workflow bổ sung, không thay thế báo cáo thật |

## 2. Ý tưởng nghiệp vụ

`ReportRequest` là một phiếu giao việc báo cáo. Nó tách biệt với báo cáo thật.

| Khái niệm | Ý nghĩa |
| --- | --- |
| `ReportRequest` | Task Admin giao cho Teacher, có độ ưu tiên, deadline, message, target học sinh/lớp và trạng thái xử lý |
| `SessionReport` | Báo cáo buổi học thật |
| `StudentMonthlyReport` | Báo cáo tháng thật |
| `Notification` | Thông báo gửi cho Teacher khi có request mới |

Luồng cũ vẫn chạy bình thường:

| Luồng cũ | Có thay đổi bắt buộc không |
| --- | --- |
| Teacher tạo/update/submit Session Report | Không bắt buộc đi qua `ReportRequest` |
| Admin approve/reject/publish Session Report | Không bắt buộc đi qua `ReportRequest` |
| Teacher generate/update/submit Monthly Report | Không bắt buộc đi qua `ReportRequest` |
| Admin approve/reject/publish Monthly Report | Không bắt buộc đi qua `ReportRequest` |

Điểm mới:

| Điểm mới | Ý nghĩa |
| --- | --- |
| Admin/Management tạo report request | Giao việc ưu tiên cho Teacher |
| Teacher xem queue request của mình | Biết report nào cần làm trước |
| Notification trực tiếp cho Teacher | Teacher được báo ngay khi có request |
| Đồng bộ trạng thái request | Submit report thật sẽ cập nhật request liên quan sang `Submitted`; approve/reject sẽ cập nhật sang `Approved` hoặc `Rejected` |

## 3. Role, dữ liệu được xem và phạm vi dữ liệu

### 3.1. Mỗi role được xem dữ liệu gì

| Role | Dữ liệu xem được trong luồng Report Request |
| --- | --- |
| `Admin` | Toàn bộ report request, bao gồm request của mọi Teacher, mọi lớp, mọi học sinh |
| `ManagementStaff` | Toàn bộ report request, tương tự Admin |
| `Teacher` | Chỉ xem được report request được assign cho chính mình |
| `Parent` | Không có quyền với report request |
| `Student` | Không có quyền với report request |
| `AccountantStaff` | Không có quyền với report request |
| `Anonymous` | Không có quyền |

### 3.2. Phạm vi dữ liệu

| Role | Phạm vi hiện tại |
| --- | --- |
| `Admin` | `all` |
| `ManagementStaff` | `all` |
| `Teacher` | `own`, theo `AssignedTeacherUserId == currentUser.Id` |
| `Parent` | Không có access |
| `Student` | Không có access |
| `AccountantStaff` | Không có access |
| `Anonymous` | Không có access |

Ghi chú:

| Ghi chú | Nội dung |
| --- | --- |
| Data scope theo chi nhánh/phòng ban | Chưa có rule `department` trong luồng này |
| Teacher filter | Handler tự filter xuống request của Teacher hiện tại, kể cả khi query truyền `assignedTeacherUserId` khác |
| Admin/Management filter | Có thể lọc theo Teacher, học sinh, lớp, tháng/năm, status, priority |

### 3.3. Các hành động được phép

| Hành động | Admin | ManagementStaff | Teacher | Parent | Student | AccountantStaff |
| --- | --- | --- | --- | --- | --- | --- |
| Tạo report request | Có | Có | Không | Không | Không | Không |
| Xem danh sách request | Có, toàn bộ | Có, toàn bộ | Có, chỉ own | Không | Không | Không |
| Xem chi tiết request | Có, toàn bộ | Có, toàn bộ | Có, chỉ own | Không | Không | Không |
| Đánh dấu request hoàn tất | Có | Có | Có, chỉ own | Không | Không | Không |
| Hủy request | Có | Có | Không | Không | Không | Không |
| Submit báo cáo thật | Theo quyền report cũ | Theo quyền report cũ | Có ở luồng report cũ | Không | Không | Không |
| Approve/reject báo cáo thật | Có | Có | Không | Không | Không | Không |
| Publish báo cáo thật | Theo quyền report cũ | Theo quyền report cũ | Không | Không | Không | Không |

## 4. Response contract chung

### 4.1. Success response

Các API mới dùng `MatchOk()` hoặc `MatchCreated()` và trả envelope chung:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Với `POST /api/report-requests`, HTTP status là `201 Created`.

Với `GET`, `complete`, `cancel`, HTTP status là `200 OK`.

### 4.2. DTO chính: ReportRequestDto

Response detail request trả object tương tự:

```json
{
  "id": "guid",
  "reportType": "Monthly",
  "status": "Requested",
  "priority": "High",
  "assignedTeacherUserId": "guid",
  "assignedTeacherName": "Teacher Name",
  "requestedByUserId": "guid",
  "requestedByName": "Admin Name",
  "targetStudentProfileId": "guid",
  "targetStudentName": "Student Name",
  "targetClassId": "guid",
  "targetClassCode": "KIDZ-01",
  "targetClassTitle": "Kidz English",
  "targetSessionId": null,
  "targetSessionDate": null,
  "month": 4,
  "year": 2026,
  "message": "Ưu tiên làm báo cáo cho bé này trước.",
  "dueAt": "2026-04-10T10:00:00Z",
  "linkedSessionReportId": null,
  "linkedMonthlyReportId": "guid",
  "submittedAt": null,
  "createdAt": "2026-04-07T04:25:57Z",
  "updatedAt": "2026-04-07T04:25:57Z"
}
```

### 4.3. Error response

Các lỗi business trả về dạng `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "ReportRequest.TeacherNotAssigned",
  "status": 400,
  "detail": "Assigned teacher is not assigned to the target class or session"
}
```

Map HTTP code:

| Loại lỗi | HTTP code |
| --- | --- |
| `Validation` | `400` |
| `NotFound` | `404` |
| Không đủ role | `403` |
| Thiếu token/token sai | `401` |
| Lỗi ngoài dự kiến | `500` |

## 5. Danh sách API

### 5.1. Tạo report request

| Thuộc tính | Nội dung |
| --- | --- |
| Endpoint | `POST /api/report-requests` |
| Role | `Admin`, `ManagementStaff` |
| Mục đích | Tạo yêu cầu làm báo cáo ưu tiên cho Teacher và gửi notification trực tiếp cho Teacher |

Body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `reportType` | enum string/int: `Session`, `Monthly` | Có | Loại request |
| `assignedTeacherUserId` | `Guid` | Có | Teacher được giao |
| `targetStudentProfileId` | `Guid?` | Không | Học sinh mục tiêu |
| `targetClassId` | `Guid?` | Không | Lớp mục tiêu |
| `targetSessionId` | `Guid?` | Có nếu `reportType=Session` | Buổi học mục tiêu |
| `month` | `int?` | Có nếu `reportType=Monthly` | Tháng báo cáo, từ 1 đến 12 |
| `year` | `int?` | Có nếu `reportType=Monthly` | Năm báo cáo |
| `priority` | enum string/int: `Low`, `Normal`, `High`, `Urgent` | Không | Mặc định `High` |
| `message` | `string?` | Không | Nội dung Admin gửi cho Teacher, tối đa 1000 ký tự ở DB config |
| `dueAt` | `DateTime?` | Không | Deadline gợi ý |
| `notificationChannel` | enum: `InApp`, các channel notification hiện có | Không | Mặc định `InApp` |

Ví dụ tạo request báo cáo tháng cho một học sinh trong một lớp:

```json
{
  "reportType": "Monthly",
  "assignedTeacherUserId": "11111111-1111-1111-1111-111111111111",
  "targetStudentProfileId": "22222222-2222-2222-2222-222222222222",
  "targetClassId": "33333333-3333-3333-3333-333333333333",
  "month": 4,
  "year": 2026,
  "priority": "Urgent",
  "message": "Ưu tiên làm báo cáo tháng cho bé này trước giúp Admin.",
  "dueAt": "2026-04-10T10:00:00Z",
  "notificationChannel": "InApp"
}
```

Ví dụ tạo request báo cáo buổi cho một học sinh:

```json
{
  "reportType": "Session",
  "assignedTeacherUserId": "11111111-1111-1111-1111-111111111111",
  "targetStudentProfileId": "22222222-2222-2222-2222-222222222222",
  "targetSessionId": "44444444-4444-4444-4444-444444444444",
  "priority": "High",
  "message": "Nhờ cô làm báo cáo buổi này trước.",
  "notificationChannel": "InApp"
}
```

Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "reportType": "Monthly",
    "status": "Requested",
    "priority": "Urgent",
    "assignedTeacherUserId": "guid",
    "targetStudentProfileId": "guid",
    "targetClassId": "guid",
    "month": 4,
    "year": 2026,
    "message": "Ưu tiên làm báo cáo tháng cho bé này trước giúp Admin.",
    "dueAt": "2026-04-10T10:00:00Z",
    "linkedMonthlyReportId": "guid",
    "createdAt": "2026-04-07T04:25:57Z",
    "updatedAt": "2026-04-07T04:25:57Z"
  }
}
```

Lỗi thường gặp:

| Code | HTTP | Message |
| --- | --- | --- |
| `ReportRequest.TargetRequired` | 400 | `A report request must target at least one student or one class` |
| `ReportRequest.TeacherNotFound` | 404 | `Assigned teacher was not found` |
| `ReportRequest.StudentNotFound` | 404 | `Target student was not found` |
| `ReportRequest.ClassNotFound` | 404 | `Target class was not found` |
| `ReportRequest.SessionRequired` | 400 | `A session report request must include sessionId` |
| `ReportRequest.SessionNotFound` | 404 | `Target session was not found` |
| `ReportRequest.MonthYearRequired` | 400 | `A monthly report request must include month and year` |
| `ReportRequest.TeacherNotAssigned` | 400 | `Assigned teacher is not assigned to the target class or session` |
| `ReportRequest.StudentNotInClass` | 400 | `Target student is not enrolled in the target class` |
| `ReportRequest.StudentNotInSession` | 400 | `Target student is not assigned to the target session` |
| `ReportRequest.ClassSessionMismatch` | 400 | `Target class does not match the selected session` |
| `ReportRequest.MonthlyClassRequiredForStudent` | 400 | `Monthly report request for this student requires classId because the student has none or multiple active classes in the selected month` |

### 5.2. Danh sách report request

| Thuộc tính | Nội dung |
| --- | --- |
| Endpoint | `GET /api/report-requests` |
| Role | `Teacher`, `Admin`, `ManagementStaff` |
| Mục đích | Xem danh sách request; Teacher xem queue của mình, Admin/Management xem toàn bộ |

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `reportType` | `ReportRequestType?` | Không | `Session` hoặc `Monthly` |
| `status` | `ReportRequestStatus?` | Không | `Requested`, `InProgress`, `Submitted`, `Approved`, `Rejected`, `Cancelled` |
| `priority` | `ReportRequestPriority?` | Không | `Low`, `Normal`, `High`, `Urgent` |
| `assignedTeacherUserId` | `Guid?` | Không | Chỉ có tác dụng cho Admin/Management; Teacher vẫn bị filter về chính mình |
| `targetStudentProfileId` | `Guid?` | Không | Lọc theo học sinh |
| `targetClassId` | `Guid?` | Không | Lọc theo lớp |
| `month` | `int?` | Không | Lọc request tháng |
| `year` | `int?` | Không | Lọc request năm |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

Success:

```json
{
  "isSuccess": true,
  "data": {
    "reportRequests": {
      "items": [
        {
          "id": "guid",
          "reportType": "Monthly",
          "status": "Requested",
          "priority": "High",
          "assignedTeacherUserId": "guid",
          "assignedTeacherName": "Teacher Name",
          "requestedByUserId": "guid",
          "requestedByName": "Admin Name",
          "targetStudentProfileId": "guid",
          "targetStudentName": "Student Name",
          "targetClassId": "guid",
          "targetClassCode": "KIDZ-01",
          "targetClassTitle": "Kidz English",
          "month": 4,
          "year": 2026,
          "message": "Ưu tiên làm trước.",
          "dueAt": "2026-04-10T10:00:00Z",
          "linkedMonthlyReportId": "guid",
          "createdAt": "2026-04-07T04:25:57Z",
          "updatedAt": "2026-04-07T04:25:57Z"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

Lỗi thường gặp:

| Code | HTTP | Message |
| --- | --- | --- |
| `User.NotFound` | 404 | `User was not found` |
| Model binding enum sai | 400 | Tùy response mặc định của ASP.NET |
| Auth | 401/403 | Thiếu token hoặc không đủ role |

### 5.3. Chi tiết report request

| Thuộc tính | Nội dung |
| --- | --- |
| Endpoint | `GET /api/report-requests/{id}` |
| Role | `Teacher`, `Admin`, `ManagementStaff` |
| Mục đích | Xem chi tiết một request |

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Id của `ReportRequest` |

Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "reportType": "Session",
    "status": "Requested",
    "priority": "High",
    "assignedTeacherUserId": "guid",
    "requestedByUserId": "guid",
    "targetStudentProfileId": "guid",
    "targetClassId": "guid",
    "targetSessionId": "guid",
    "targetSessionDate": "2026-04-07T09:00:00Z",
    "message": "Nhờ cô làm báo cáo buổi này trước.",
    "createdAt": "2026-04-07T04:25:57Z",
    "updatedAt": "2026-04-07T04:25:57Z"
  }
}
```

Lỗi thường gặp:

| Code | HTTP | Message |
| --- | --- | --- |
| `ReportRequest.NotFound` | 404 | `Report request with Id = '{id}' was not found` |
| `ReportRequest.Unauthorized` | 400 | Teacher chỉ được xem request được assign cho chính mình |
| Auth | 401/403 | Thiếu token hoặc không đủ role |

### 5.4. Đánh dấu report request hoàn tất

| Thuộc tính | Nội dung |
| --- | --- |
| Endpoint | `POST /api/report-requests/{id}/complete` |
| Role | `Teacher`, `Admin`, `ManagementStaff` |
| Mục đích | Đánh dấu task request đã hoàn tất/gửi lại để Admin duyệt; hữu ích nhất với request theo cả lớp |

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Id của `ReportRequest` |

Body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `linkedSessionReportId` | `Guid?` | Không | Chỉ dùng khi request type là `Session` |
| `linkedMonthlyReportId` | `Guid?` | Không | Chỉ dùng khi request type là `Monthly` |

Ví dụ:

```json
{
  "linkedMonthlyReportId": "55555555-5555-5555-5555-555555555555"
}
```

Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "reportType": "Monthly",
    "status": "Submitted",
    "priority": "High",
    "linkedMonthlyReportId": "55555555-5555-5555-5555-555555555555",
    "submittedAt": "2026-04-07T05:00:00Z",
    "updatedAt": "2026-04-07T05:00:00Z"
  }
}
```

Ghi chú sử dụng:

| Case | Có cần gọi API này không |
| --- | --- |
| Request theo một học sinh cụ thể | Thường không cần; khi Teacher submit report thật, backend tự đồng bộ request sang `Submitted` |
| Request theo một lớp cụ thể | Nên gọi sau khi Teacher hoàn tất phần request của cả lớp, vì một request lớp có thể liên quan nhiều report |
| Muốn link request với report thật đã có | Có thể truyền `linkedSessionReportId` hoặc `linkedMonthlyReportId` |

Lỗi thường gặp:

| Code | HTTP | Message |
| --- | --- | --- |
| `ReportRequest.NotFound` | 404 | `Report request with Id = '{id}' was not found` |
| `ReportRequest.Unauthorized` | 400 | `You can only complete your own report requests` |
| `ReportRequest.InvalidStatus` | 400 | Không thể complete request ở status hiện tại |
| `ReportRequest.InvalidReportLink` | 400 | Link sai loại report, ví dụ link monthly report vào session request |
| `ReportRequest.ReportMismatch` | 400 | Report được link không khớp Teacher, student, class, session, month hoặc year của request |
| `SessionReport.NotFound` | 404 | Không tìm thấy session report được link |
| `MonthlyReport.NotFound` | 404 | Không tìm thấy monthly report được link |

### 5.5. Hủy report request

| Thuộc tính | Nội dung |
| --- | --- |
| Endpoint | `POST /api/report-requests/{id}/cancel` |
| Role | `Admin`, `ManagementStaff` |
| Mục đích | Hủy một request chưa cần xử lý nữa |

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Id của `ReportRequest` |

Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Cancelled",
    "updatedAt": "2026-04-07T05:00:00Z"
  }
}
```

Lỗi thường gặp:

| Code | HTTP | Message |
| --- | --- | --- |
| `ReportRequest.NotFound` | 404 | `Report request with Id = '{id}' was not found` |
| `ReportRequest.InvalidStatus` | 400 | Không thể cancel request ở status hiện tại, ví dụ đã `Approved` hoặc đã `Cancelled` |
| Auth | 401/403 | Thiếu token hoặc không đủ role |

## 6. Status definition

### 6.1. ReportRequestType

| Type | Ý nghĩa |
| --- | --- |
| `Session` | Request yêu cầu làm báo cáo buổi |
| `Monthly` | Request yêu cầu làm báo cáo tháng |

### 6.2. ReportRequestPriority

| Priority | Ý nghĩa |
| --- | --- |
| `Low` | Ưu tiên thấp |
| `Normal` | Ưu tiên bình thường |
| `High` | Ưu tiên cao, là mặc định khi tạo từ API request |
| `Urgent` | Ưu tiên khẩn cấp |

Danh sách request được sort theo status đang cần xử lý trước, sau đó theo priority `Urgent -> High -> Normal -> Low`, sau đó theo `dueAt`.

### 6.3. ReportRequestStatus

| Status | Ý nghĩa |
| --- | --- |
| `Requested` | Admin/Management đã tạo request, Teacher chưa hoàn tất |
| `InProgress` | Trạng thái dự phòng cho việc Teacher đang xử lý; hiện chưa có API riêng để chuyển sang trạng thái này |
| `Submitted` | Teacher đã hoàn tất request hoặc report liên quan đã được submit để Admin duyệt |
| `Approved` | Admin/Management đã approve report liên quan |
| `Rejected` | Admin/Management đã reject report liên quan |
| `Cancelled` | Admin/Management đã hủy request |

### 6.4. Luồng chuyển trạng thái

Luồng chính:

| Từ status | Sang status | Trigger |
| --- | --- | --- |
| `Requested` | `Submitted` | Teacher submit report thật khớp request học sinh cụ thể, hoặc gọi `POST /api/report-requests/{id}/complete` |
| `InProgress` | `Submitted` | Gọi `complete` hoặc report thật được submit |
| `Submitted` | `Approved` | Admin/Management approve report thật liên quan |
| `Submitted` | `Rejected` | Admin/Management reject report thật liên quan |
| `Requested` | `Approved` | Nếu request đã link sẵn report và report được approve trực tiếp |
| `Requested` | `Rejected` | Nếu request đã link sẵn report và report được reject trực tiếp |
| `Requested` | `Cancelled` | Admin/Management gọi `cancel` |
| `InProgress` | `Cancelled` | Admin/Management gọi `cancel` |
| `Submitted` | `Cancelled` | Admin/Management gọi `cancel`, hiện handler cho phép nếu chưa `Approved` hoặc `Cancelled` |

Ghi chú:

| Ghi chú | Nội dung |
| --- | --- |
| Request theo học sinh | Workflow tự tìm request khớp `teacher + student + session/class/month/year` để cập nhật khi submit report |
| Request theo lớp | Không tự biết toàn bộ lớp đã xong khi từng report riêng lẻ được submit; FE/Teacher nên dùng `complete` khi hoàn tất cả lớp |
| Report cũ không có request | Không bị ảnh hưởng; submit/approve/reject chạy như trước |

## 7. Permission matrix theo role

| Capability | Admin | ManagementStaff | Teacher | Parent | Student | AccountantStaff | Anonymous |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Create report request | Y | Y | - | - | - | - | - |
| List all report requests | Y | Y | - | - | - | - | - |
| List own assigned requests | - | - | Y | - | - | - | - |
| View any request detail | Y | Y | - | - | - | - | - |
| View own request detail | - | - | Y | - | - | - | - |
| Complete any request | Y | Y | - | - | - | - | - |
| Complete own request | - | - | Y | - | - | - | - |
| Cancel request | Y | Y | - | - | - | - | - |
| Receive notification as assigned teacher | - | - | Y | - | - | - | - |
| Submit Session/Monthly Report | Theo luồng report cũ | Theo luồng report cũ | Y | - | - | - | - |
| Approve/Reject report thật | Y | Y | - | - | - | - | - |

Ký hiệu:

| Ký hiệu | Ý nghĩa |
| --- | --- |
| `Y` | Được phép |
| `-` | Không có quyền |

## 8. Validation rule và rule kiểm tra dữ liệu

### 8.1. Rule chung khi tạo request

| Rule | Lỗi trả về |
| --- | --- |
| Phải truyền ít nhất `targetStudentProfileId` hoặc `targetClassId` | `400 ReportRequest.TargetRequired` |
| `assignedTeacherUserId` phải là user tồn tại, role `Teacher`, chưa bị xóa | `404 ReportRequest.TeacherNotFound` |
| User tạo request phải tồn tại | `404 User.NotFound` |
| Nếu truyền `targetStudentProfileId`, profile phải tồn tại và là `ProfileType.Student` | `404 ReportRequest.StudentNotFound` |
| Request tạo xong sẽ sinh `Notification` cho Teacher | Không phải lỗi; là side effect của create |

### 8.2. Rule cho request báo cáo buổi

| Rule | Lỗi trả về |
| --- | --- |
| `reportType=Session` phải có `targetSessionId` | `400 ReportRequest.SessionRequired` |
| Session phải tồn tại | `404 ReportRequest.SessionNotFound` |
| Nếu truyền `targetClassId`, class phải khớp với `session.ClassId` | `400 ReportRequest.ClassSessionMismatch` |
| Teacher được giao phải là planned teacher, actual teacher, main teacher hoặc assistant teacher của session/class | `400 ReportRequest.TeacherNotAssigned` |
| Nếu request theo học sinh, học sinh phải được assign vào session | `400 ReportRequest.StudentNotInSession` |
| Nếu đã có `SessionReport` cho session + student, request sẽ link sẵn vào `linkedSessionReportId` | Không lỗi |

### 8.3. Rule cho request báo cáo tháng

| Rule | Lỗi trả về |
| --- | --- |
| `reportType=Monthly` phải có `month` và `year` | `400 ReportRequest.MonthYearRequired` |
| `month` phải nằm trong 1 đến 12 | `400 ReportRequest.MonthYearRequired` |
| Nếu chỉ truyền học sinh mà không truyền lớp, hệ thống chỉ tự suy ra lớp khi học sinh có đúng 1 lớp active/paused trong tháng | `400 ReportRequest.MonthlyClassRequiredForStudent` |
| Nếu truyền lớp, lớp phải tồn tại | `404 ReportRequest.ClassNotFound` |
| Teacher được giao phải là main teacher hoặc assistant teacher của lớp | `400 ReportRequest.TeacherNotAssigned` |
| Nếu request theo học sinh + lớp, học sinh phải active/paused trong lớp | `400 ReportRequest.StudentNotInClass` |
| Nếu chưa có monthly report cho học sinh + lớp + tháng/năm, hệ thống tạo draft report | Không lỗi |
| Nếu request theo cả lớp, hệ thống tạo draft monthly report còn thiếu cho các học sinh active/paused của lớp | Không lỗi |

### 8.4. Rule khi complete request

| Rule | Lỗi trả về |
| --- | --- |
| Request phải tồn tại | `404 ReportRequest.NotFound` |
| Teacher chỉ được complete request của chính mình | `400 ReportRequest.Unauthorized` |
| Chỉ complete được request ở `Requested` hoặc `InProgress` | `400 ReportRequest.InvalidStatus` |
| Không được link session report vào monthly request | `400 ReportRequest.InvalidReportLink` |
| Không được link monthly report vào session request | `400 ReportRequest.InvalidReportLink` |
| Report được link phải khớp teacher/student/class/session/month/year của request | `400 ReportRequest.ReportMismatch` |
| Report được link phải tồn tại | `404 SessionReport.NotFound` hoặc `404 MonthlyReport.NotFound` |

### 8.5. Rule khi cancel request

| Rule | Lỗi trả về |
| --- | --- |
| Request phải tồn tại | `404 ReportRequest.NotFound` |
| Không cancel request đã `Approved` hoặc `Cancelled` | `400 ReportRequest.InvalidStatus` |
| Chỉ `Admin` và `ManagementStaff` được cancel | `403 Forbidden` |

## 9. Các trường hợp trả lỗi nổi bật

### 9.1. Admin tạo request thiếu target

Body lỗi:

```json
{
  "reportType": "Monthly",
  "assignedTeacherUserId": "11111111-1111-1111-1111-111111111111",
  "month": 4,
  "year": 2026
}
```

Response:

```json
{
  "title": "ReportRequest.TargetRequired",
  "status": 400,
  "detail": "A report request must target at least one student or one class"
}
```

### 9.2. Monthly request cho học sinh có nhiều lớp nhưng không truyền classId

Trường hợp này quan trọng vì hệ thống hiện đã hỗ trợ một học sinh học nhiều chương trình/lớp.

Response:

```json
{
  "title": "ReportRequest.MonthlyClassRequiredForStudent",
  "status": 400,
  "detail": "Monthly report request for this student requires classId because the student has none or multiple active classes in the selected month"
}
```

Cách xử lý FE:

| Cách xử lý | Nội dung |
| --- | --- |
| Truyền thêm `targetClassId` | Chọn đúng lớp/chương trình mà Admin muốn ưu tiên |
| Hoặc tạo request theo lớp | Không truyền `targetStudentProfileId`, chỉ truyền `targetClassId`, `month`, `year` |

### 9.3. Teacher không thuộc lớp/session được giao

Response:

```json
{
  "title": "ReportRequest.TeacherNotAssigned",
  "status": 400,
  "detail": "Assigned teacher is not assigned to the target class or session"
}
```

### 9.4. Teacher complete request của người khác

Response:

```json
{
  "title": "ReportRequest.Unauthorized",
  "status": 400,
  "detail": "You can only complete your own report requests"
}
```

### 9.5. Link sai report khi complete

Ví dụ link `linkedMonthlyReportId` vào request `Session`.

Response:

```json
{
  "title": "ReportRequest.InvalidReportLink",
  "status": 400,
  "detail": "Cannot link a monthly report to a session report request"
}
```

### 9.6. Link report không khớp request

Ví dụ request của học sinh A nhưng link report của học sinh B.

Response:

```json
{
  "title": "ReportRequest.ReportMismatch",
  "status": 400,
  "detail": "Linked report does not match this request"
}
```

## 10. Flow gợi ý cho FE

### 10.1. Admin giao request cho Teacher

| Bước | FE/API |
| --- | --- |
| 1 | Admin mở form tạo request |
| 2 | Chọn `reportType`: `Session` hoặc `Monthly` |
| 3 | Chọn Teacher được giao |
| 4 | Chọn học sinh hoặc lớp |
| 5 | Nếu `Session`, chọn session |
| 6 | Nếu `Monthly`, chọn tháng/năm và nên chọn class nếu học sinh học nhiều lớp |
| 7 | Chọn priority, due date, message |
| 8 | Gọi `POST /api/report-requests` |
| 9 | Backend tạo `ReportRequest` và `Notification` cho Teacher |

### 10.2. Teacher xử lý request theo một học sinh

| Bước | FE/API |
| --- | --- |
| 1 | Teacher mở `GET /api/report-requests?status=Requested` |
| 2 | Teacher chọn request cần làm |
| 3 | Teacher đi qua màn SessionReport hoặc MonthlyReport cũ |
| 4 | Teacher submit report thật bằng API cũ |
| 5 | Backend tự chuyển request liên quan sang `Submitted` |
| 6 | Admin approve/reject report thật bằng API cũ |
| 7 | Backend tự chuyển request sang `Approved` hoặc `Rejected` |

### 10.3. Teacher xử lý request theo cả lớp

| Bước | FE/API |
| --- | --- |
| 1 | Teacher mở request theo lớp |
| 2 | Teacher làm các report cần thiết trong lớp |
| 3 | Teacher submit từng report thật bằng API cũ |
| 4 | Khi hoàn tất phạm vi request, FE gọi `POST /api/report-requests/{id}/complete` |
| 5 | Request chuyển sang `Submitted` |
| 6 | Admin duyệt các report thật; nếu request có link report cụ thể thì workflow sẽ đồng bộ trạng thái theo report đó |

## 11. Ghi chú implementation hiện tại

| Ghi chú | Nội dung |
| --- | --- |
| ReportRequest không thay thế report thật | Teacher vẫn dùng `SessionReportController` và `MonthlyReportController` để làm báo cáo |
| Notification | Khi tạo request, backend tạo `Notification` với `Kind = report_request`, `TargetRole = Teacher`, `Priority` theo request, `Deeplink = /report-requests/{id}` |
| Request theo học sinh | Có auto sync khi Teacher submit report |
| Request theo lớp | Cần FE/Teacher gọi `complete` khi hoàn tất vì một request lớp có thể đại diện nhiều report |
| Monthly draft | Tạo request monthly theo học sinh/lớp có thể tạo sẵn monthly report draft còn thiếu |
| Role Parent/Student | Không có quyền trong luồng request, dù họ có thể xem báo cáo đã publish theo luồng report cũ nếu API cũ cho phép |
| InProgress | Enum đã có nhưng hiện chưa có endpoint riêng để chuyển `Requested -> InProgress` |

