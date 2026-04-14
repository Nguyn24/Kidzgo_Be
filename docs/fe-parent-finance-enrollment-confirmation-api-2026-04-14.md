# Tài Liệu API FE - Tài Chính Phụ Huynh và PDF Xác Nhận Nhập Học

Ngày cập nhật: `2026-04-14`

Tài liệu này ghi lại thay đổi mới cho FE:

- `GET /api/parent/overview` bổ sung dữ liệu học phí/công nợ và tiến độ gói học.
- `POST /api/registrations/{id}/enrollment-confirmation-pdf` xuất phiếu xác nhận nhập học PDF sau khi học viên đã được xếp lớp.

## 1. Role và phạm vi dữ liệu

| Role | Dữ liệu được xem | Phạm vi | Hành động được phép |
| --- | --- | --- | --- |
| Parent | Dashboard của học sinh đã liên kết với parent profile hiện tại | `own`, theo `studentProfileId` trong query hoặc `studentId` trong token | `view` parent overview |
| Admin | Registration, enrollment, PDF xác nhận nhập học | `all` theo code hiện tại | `view`, `create`, `edit`, `cancel`, `assign`, `transfer`, `upgrade`, `generatePdf`, `regeneratePdf` |
| ManagementStaff | Registration, enrollment, PDF xác nhận nhập học | `all` theo code hiện tại; chưa enforce branch/department filter trong endpoint này | `view`, `create`, `edit`, `cancel`, `assign`, `transfer`, `upgrade`, `generatePdf`, `regeneratePdf` |
| AccountantStaff | Không được gọi endpoint PDF theo attribute hiện tại | `none` với endpoint PDF | Không có quyền trên endpoint PDF |
| Teacher | Không được gọi các endpoint trong tài liệu này | `none` | Không có quyền |
| Student | Không được gọi các endpoint trong tài liệu này | `none` | Không có quyền |

Lưu ý:

- `GET /api/parent/overview` hiện chỉ dùng `[Authorize]`, nhưng handler bắt buộc user hiện tại phải có `ProfileType.Parent` active và học sinh phải nằm trong `ParentStudentLinks`.
- `POST /api/registrations/{id}/enrollment-confirmation-pdf` dùng `[Authorize(Roles = "Admin,ManagementStaff")]`.
- Nếu cần dùng phạm vi `department/branch` cho `ManagementStaff`, BE cần bổ sung filter branch trong handler/controller sau. Hiện tại endpoint PDF chưa enforce điều này.

## 2. Ma trận quyền

| API | Parent | Admin | ManagementStaff | AccountantStaff | Teacher | Student |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/parent/overview` | `view own` | Không phải flow chính | Không phải flow chính | Không phải flow chính | No | No |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | No | `generate/regenerate all` | `generate/regenerate all` | No | No | No |

## 3. Danh sách API

### 3.1. `GET /api/parent/overview`

Mục đích:

- Trả dashboard tổng hợp cho parent theo học sinh đã chọn.
- FE dùng để hiển thị:
  - tên chương trình/gói học
  - tổng số buổi, đã học, còn lại
  - tổng công nợ chuẩn từ BE
  - hạn đóng gần nhất và số ngày còn lại/quá hạn

Thay đổi mới:

- Thêm `programName`
- Thêm `packageName`
- Thêm `totalSessions`
- Thêm `usedSessions`
- Thêm `remainingSessions`
- Thêm `outstandingAmount`
- Thêm `nextDueDate`
- Thêm `daysUntilDue`
- `tuitionDue` cũ được map về cùng giá trị với `outstandingAmount` để FE cũ không bị lệch.

Quyền:

- Authenticated user.
- User phải có parent profile active.
- Học sinh phải linked với parent profile hiện tại.

Phạm vi dữ liệu:

- `own`: chỉ trả dữ liệu của học sinh selected trong token hoặc `studentProfileId` query.
- Nếu parent truyền `studentProfileId` không linked với mình, API trả lỗi `404`.

Params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | Học sinh cần xem. Nếu không truyền, BE dùng `studentId` trong token. |
| `classId` | `Guid?` | No | Lọc theo lớp active của học sinh. Đồng thời dùng để chọn registration/class liên quan nếu hợp lệ. |
| `sessionId` | `Guid?` | No | Lọc upcoming session theo session cụ thể. |
| `fromDate` | `DateTime?` | No | Ngày bắt đầu cho các thống kê theo khoảng thời gian. Mặc định `now - 1 month`. |
| `toDate` | `DateTime?` | No | Ngày kết thúc cho các thống kê theo khoảng thời gian. Mặc định `now + 1 month`. |

Business logic mới:

- Số buổi không suy từ lịch học hay attendance.
- BE lấy `totalSessions`, `usedSessions`, `remainingSessions` từ `Registration`.
- Registration hiện hành được chọn theo các status:
  - `Studying`
  - `ClassAssigned`
  - `Paused`
- Nếu có nhiều registration hợp lệ, BE ưu tiên `Studying`, sau đó registration có `ActualStartDate`, `ExpectedStartDate`, `RegistrationDate` mới nhất.
- `outstandingAmount` tính từ invoice status `Pending` và `Overdue`, trừ đi tổng payment đã ghi nhận trên invoice.
- `nextDueDate` chỉ lấy từ invoice còn nợ thực sự sau khi trừ payment.
- `daysUntilDue = nextDueDate - today` theo ngày Việt Nam. Giá trị âm nghĩa là đã quá hạn.

Response thành công:

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "statistics": {},
    "studentProfiles": [],
    "classes": [],
    "upcomingSessions": [],
    "recentAttendances": [],
    "makeupCredits": [],
    "pendingHomeworks": [],
    "recentExams": [],
    "reports": [],
    "pendingInvoices": [],
    "activeMissions": [],
    "openTickets": [],
    "studentInfo": {
      "id": "00000000-0000-0000-0000-000000000000",
      "displayName": "Student name",
      "level": 1,
      "totalStars": 20,
      "xp": 100
    },
    "classInfo": {
      "id": "00000000-0000-0000-0000-000000000000",
      "code": "KID-001",
      "title": "Kidz Program",
      "studentProfileId": "00000000-0000-0000-0000-000000000000",
      "status": "Active"
    },
    "attendanceRate": 90.0,
    "homeworkCompletion": 80.0,
    "xp": 100,
    "level": 1,
    "streak": 3,
    "stars": 20,
    "nextClasses": [],
    "pendingApprovals": [],
    "tuitionDue": 1500000,
    "programName": "English Foundation",
    "packageName": "24 sessions",
    "totalSessions": 24,
    "usedSessions": 10,
    "remainingSessions": 14,
    "outstandingAmount": 1500000,
    "nextDueDate": "2026-04-30T00:00:00",
    "daysUntilDue": 16,
    "unreadNotifications": 2
  }
}
```

Response lỗi:

HTTP `401 Unauthorized`

- Token thiếu hoặc không hợp lệ.

HTTP `404 Not Found`

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "ParentProfile",
  "status": 404,
  "detail": "not found"
}
```

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Student",
  "status": 404,
  "detail": "Student not linked to this parent"
}
```

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "StudentId",
  "status": 404,
  "detail": "No student selected in token"
}
```

Validation rule:

- `studentProfileId` nếu truyền phải là GUID hợp lệ.
- `classId` nếu truyền phải là GUID hợp lệ.
- `sessionId` nếu truyền phải là GUID hợp lệ.
- `fromDate` và `toDate` phải parse được thành `DateTime`.
- Parent chỉ được xem học sinh linked với parent profile hiện tại.

### 3.2. `POST /api/registrations/{id}/enrollment-confirmation-pdf`

Mục đích:

- Xuất phiếu xác nhận nhập học PDF cho registration đã được xếp lớp.
- FE dùng `pdfUrl` để hiển thị link tải/xem phiếu.
- Nếu PDF đã tồn tại và `regenerate = false`, BE trả lại file cũ, không tạo lại.
- Nếu `regenerate = true`, BE tạo lại PDF và cập nhật `ClassEnrollment.EnrollmentConfirmationPdfUrl`.

Quyền:

- `Admin`
- `ManagementStaff`

Phạm vi dữ liệu:

- `all` theo code hiện tại.
- Chưa enforce branch/department scope trong endpoint này.

Endpoint:

```http
POST /api/registrations/{id}/enrollment-confirmation-pdf?track=primary&regenerate=false
```

Params:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `id` | `Guid` | Yes | N/A | Registration ID. |
| `track` | `string` | No | `primary` | Track cần xuất PDF. Giá trị hợp lệ theo helper hiện tại: `primary`, `secondary`. Giá trị khác sẽ fallback về `primary`. |
| `regenerate` | `bool` | No | `false` | `false`: dùng PDF cũ nếu đã có. `true`: tạo lại PDF. |

Body:

- Không có request body.

Business logic:

- BE tìm registration theo `{id}`.
- BE normalize `track`:
  - `secondary` -> `RegistrationTrackType.Secondary`
  - các giá trị khác -> `RegistrationTrackType.Primary`
- BE tìm active enrollment theo:
  - `RegistrationId == id`
  - `Track == trackType`
  - `Status == EnrollmentStatus.Active`
- Nếu enrollment đã có `EnrollmentConfirmationPdfUrl` và `regenerate = false`, BE trả response với `reusedExistingPdf = true`.
- Nếu cần tạo mới:
  - lấy thông tin học sinh, phụ huynh, chi nhánh, lớp, chương trình, gói học
  - lấy ngày học đầu tiên từ `StudentSessionAssignments` status `Assigned`
  - generate PDF bằng `IEnrollmentConfirmationPdfGenerator`
  - lưu `EnrollmentConfirmationPdfUrl`, `EnrollmentConfirmationPdfGeneratedAt`, `EnrollmentConfirmationPdfGeneratedBy`
  - trả URL download qua `IFileStorageService.GetDownloadUrl`

Response thành công:

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "00000000-0000-0000-0000-000000000000",
    "enrollmentId": "00000000-0000-0000-0000-000000000001",
    "track": "primary",
    "pdfUrl": "https://cdn.example.com/enrollment-confirmation.pdf",
    "pdfGeneratedAt": "2026-04-14T07:00:00Z",
    "reusedExistingPdf": false,
    "enrollDate": "2026-04-14",
    "firstStudyDate": "2026-04-16",
    "studentName": "Student name",
    "classCode": "KID-001",
    "classTitle": "Kidz Program",
    "programName": "English Foundation",
    "tuitionPlanName": "24 sessions",
    "tuitionAmount": 4800000,
    "currency": "VND"
  }
}
```

Response thành công khi dùng lại PDF cũ:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "00000000-0000-0000-0000-000000000000",
    "enrollmentId": "00000000-0000-0000-0000-000000000001",
    "track": "primary",
    "pdfUrl": "https://cdn.example.com/existing.pdf",
    "pdfGeneratedAt": "2026-04-10T07:00:00Z",
    "reusedExistingPdf": true,
    "enrollDate": "2026-04-10",
    "firstStudyDate": "2026-04-12",
    "studentName": "Student name",
    "classCode": "KID-001",
    "classTitle": "Kidz Program",
    "programName": "English Foundation",
    "tuitionPlanName": "24 sessions",
    "tuitionAmount": 4800000,
    "currency": "VND"
  }
}
```

Response lỗi:

HTTP `401 Unauthorized`

- Token thiếu hoặc không hợp lệ.

HTTP `403 Forbidden`

- User không có role `Admin` hoặc `ManagementStaff`.

HTTP `404 Not Found`

Registration không tồn tại:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Registration.NotFound",
  "status": 404,
  "detail": "Registration with ID {id} not found"
}
```

Không có active enrollment cho registration/track:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Registration.EnrollmentNotFound",
  "status": 404,
  "detail": "No active enrollment was found for registration {id} and track 'primary'."
}
```

HTTP `500 Internal Server Error`

Lỗi generate PDF/file storage:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Server failure",
  "status": 500,
  "detail": "An unexpected error occurred"
}
```

Validation rule:

- `id` phải là GUID hợp lệ.
- `track` nên gửi `primary` hoặc `secondary`.
- Nếu FE gửi `track` khác `secondary`, BE hiện tại fallback về `primary`, không trả validation error.
- `regenerate` phải parse được thành boolean.
- Registration phải tồn tại.
- Registration phải có active enrollment đúng track.
- Enrollment phải active; enrollment `Paused` hoặc `Dropped` không được xuất PDF theo endpoint hiện tại.

## 4. Định nghĩa status

### 4.1. `RegistrationStatus`

| Status | Ý nghĩa |
| --- | --- |
| `New` | Registration mới tạo, chưa xếp lớp. |
| `WaitingForClass` | Đang chờ xếp lớp hoặc còn track chưa có lớp. |
| `ClassAssigned` | Đã xếp lớp nhưng entry type không phải vào học ngay, ví dụ `Makeup` hoặc `Retake`. |
| `Studying` | Đã xếp lớp và đang học. |
| `Paused` | Registration đang bảo lưu/tạm dừng. |
| `Completed` | Hoàn thành gói học/khóa học. |
| `Cancelled` | Đã hủy registration. |

Luồng chuyển trạng thái chính:

```text
New
  -> WaitingForClass
  -> ClassAssigned
  -> Studying
  -> Completed

New/WaitingForClass/ClassAssigned/Studying
  -> Cancelled

Studying
  -> Paused
  -> Studying
```

Lưu ý theo code hiện tại:

- `AssignClass` dùng entry type để resolve status:
  - `Immediate` -> `Studying`
  - `Makeup` -> `ClassAssigned`
  - `Retake` -> `ClassAssigned`
  - `Wait` -> `WaitingForClass`
- `MarkAttendance` có thể set `Completed` khi `RemainingSessions == 0`.
- `CancelRegistration` set registration về `Cancelled` và drop active enrollment liên quan.

### 4.2. `EnrollmentStatus`

| Status | Ý nghĩa |
| --- | --- |
| `Active` | Học viên đang active trong lớp. |
| `Paused` | Enrollment đang tạm dừng/bảo lưu. |
| `Dropped` | Học viên đã rời lớp/drop enrollment. |

Luồng chuyển trạng thái chính:

```text
Active -> Paused -> Active
Active -> Dropped
Paused -> Dropped
```

Ràng buộc với PDF:

- API enrollment confirmation PDF chỉ chấp nhận enrollment `Active`.

### 4.3. `InvoiceStatus`

| Status | Ý nghĩa |
| --- | --- |
| `Pending` | Hóa đơn đang chờ thanh toán. |
| `Paid` | Hóa đơn đã thanh toán. |
| `Overdue` | Hóa đơn đã quá hạn. |
| `Cancelled` | Hóa đơn đã hủy. |

Liên quan `GET /api/parent/overview`:

- `outstandingAmount` chỉ tính invoice `Pending` và `Overdue`.
- `Paid` và `Cancelled` không tính vào công nợ.
- Invoice `Pending/Overdue` nhưng đã đủ payment thì không tính vào `nextDueDate`.

### 4.4. `RegistrationTrackType`

| API value | Domain value | Ý nghĩa |
| --- | --- | --- |
| `primary` | `Primary` | Track chương trình chính. |
| `secondary` | `Secondary` | Track chương trình phụ/secondary program. |

## 5. Các trường hợp trả lỗi

| API | Case | HTTP | Code/Title |
| --- | --- | --- | --- |
| `GET /api/parent/overview` | Chưa login/token sai | `401` | Standard auth error |
| `GET /api/parent/overview` | User không có parent profile active | `404` | `ParentProfile` |
| `GET /api/parent/overview` | Không có selected student | `404` | `StudentId` |
| `GET /api/parent/overview` | Student không linked với parent | `404` | `Student` |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Chưa login/token sai | `401` | Standard auth error |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Không có role `Admin,ManagementStaff` | `403` | Standard authorization error |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Registration không tồn tại | `404` | `Registration.NotFound` |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Không có active enrollment theo track | `404` | `Registration.EnrollmentNotFound` |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Lỗi generate PDF/file storage | `500` | `Server failure` |

## 6. Ghi chú cho FE

- FE không cần tự tính `remainingSessions` từ attendance nữa; dùng trực tiếp `data.remainingSessions`.
- FE nên dùng `data.outstandingAmount` cho tổng nợ; `data.tuitionDue` chỉ là alias/backward compatibility.
- FE nên dùng `data.nextDueDate` và `data.daysUntilDue` thay vì tự đoán invoice nào là kỳ đóng chính.
- Nếu `daysUntilDue < 0`, có thể hiển thị quá hạn.
- Nếu `nextDueDate = null`, hiện tại không có invoice còn nợ có due date.
- Khi gọi PDF, nếu FE muốn ép tạo lại file mới thì truyền `regenerate=true`.
- Nếu registration có secondary track, FE cần truyền `track=secondary` để xuất đúng phiếu của enrollment secondary.
