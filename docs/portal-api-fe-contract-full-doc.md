# Tài Liệu Contract API Portal Cho FE

Tài liệu này tổng hợp nhóm API portal mà FE đang cần cho Parent, Student, Teacher, Staff và Management, map theo các route backend hiện đang có trong code.

Phần tài chính chuyên biệt đã được loại khỏi phạm vi tài liệu này. Tuy nhiên, một số dashboard và approvals vẫn có field liên quan invoice vì đó là payload hiện đang tồn tại trong response.

## 1. Mục Tiêu Và Phạm Vi

- `FE endpoint` là tên endpoint FE đang kỳ vọng theo mock UI.
- `BE route` là route backend hiện đang expose trong code.
- Tài liệu này ưu tiên mô tả contract hiện có để FE có thể dùng ngay.
- Các route "tạm dùng" sẽ được ghi rõ để FE biết đang gọi alias hay route thay thế.

## 2. Vai Trò, Phạm Vi Dữ Liệu, Hành Động

| Role | Xem dữ liệu gì | Phạm vi thực tế | Hành động được phép | Ghi chú |
| --- | --- | --- | --- | --- |
| `Parent` | Dashboard, tests, homework, tickets, media, progress, approvals, account | `own` student liên kết với parent; một số API lấy selected student trong token, một số API fallback sang student đầu tiên liên kết | `view`, `edit own account` | `overview` yêu cầu student được chọn nếu token không có `StudentId` |
| `Student` | Dashboard, tests, reports, media | `own` student profile trong token | `view` | Chưa có route student materials trong mapping tài liệu này |
| `Teacher` | Dashboard, timesheet | `own` classes và `own` timesheet; Admin/Management có thể query timesheet theo `teacherUserId` | `view` | Dashboard chỉ mở cho role `Teacher` |
| `Staff` | Dashboard, students, pending enrollments, announcements | Chú ý: nghiệp vụ là `department/branch`, nhưng một vài API hiện tại chưa filter branch | `view`, `create announcement` | `GET /api/staff/students` hiện lấy global top 200, không filter branch |
| `ManagementStaff` | Dashboard, students, media, templates | Chủ yếu `department/branch`; một số route generic mở rộng hơn | `view`, `create`, `edit`, `delete` | `management dashboard` hiện map qua `/api/me/management-staff/overview` |
| `Admin` | Templates, accounts, media, sessions, timesheet xem hộ | `all` hoặc route generic | `view`, `create`, `edit`, `delete` | `GET /api/admin/users` hiện controller đang không khóa `[Authorize]` |

Quy ước scope trong tài liệu:

- `own`: dữ liệu của chính user hoặc student được gắn với current user.
- `department`: dữ liệu theo `BranchId` của current user.
- `all`: không filter theo current branch hoặc current user.

## 3. Cấu Trúc Response Chung

### 3.1. Envelope Thành Công

Tất cả API đang dùng `ApiResult<T>`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 3.2. Response Phân Trang

Những API trả `Page<T>` sẽ có dạng:

```json
{
  "isSuccess": true,
  "data": {
    "items": [],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 0
  }
}
```

Field getter như `hasPreviousPage` và `hasNextPage` có thể được serializer expose, nhưng FE nên coi `items`, `pageNumber`, `totalPages`, `totalCount` là các field ổn định.

### 3.3. Format Lỗi

Backend đang dùng `Results.Problem(...)` cho lỗi nghiệp vụ:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "Title is required"
    }
  ]
}
```

### 3.4. Các HTTP Status Chung

| HTTP status | Nguồn lỗi |
| --- | --- |
| `200 OK` | Query/update thành công |
| `201 Created` | Create thành công |
| `400 Bad Request` | Validation error hoặc request format sai |
| `401 Unauthorized` | Thiếu/hết hạn token |
| `403 Forbidden` | Token có nhưng không đủ role |
| `404 Not Found` | Entity không tồn tại hoặc không thuộc scope được phép |
| `409 Conflict` | Xung đột nghiệp vụ, duplicate, invalid transition |
| `500 Internal Server Error` | Lỗi hệ thống không được map rõ ràng |

## 4. Bảng Tổng Hợp Endpoint FE -> BE

| FE endpoint | BE route hiện có | Method | Scope | Vai trò chính | Trạng thái |
| --- | --- | --- | --- | --- | --- |
| `/parent/dashboard` | `/api/parent/overview` | `GET` | `own` | Parent | Đang dùng route thay thế |
| `/parent/children/{id}/overview` | `/api/parent/overview?studentProfileId={id}` | `GET` | `own` | Parent | Đang dùng route thay thế |
| `/parent/children/{id}/tests` | `/api/parent/tests?studentProfileId={id}` | `GET` | `own` | Parent | Có sẵn |
| `/parent/children/{id}/homework` | `/api/parent/homework?studentProfileId={id}` | `GET` | `own` | Parent | Có sẵn |
| `/parent/tickets` | `/api/tickets?mine=true` | `GET` | `own` | Parent | Đang dùng route generic |
| `/parent/children/{id}/media` | `/api/parent/media?studentProfileId={id}` | `GET` | `own` | Parent | Có sẵn |
| `/parent/children/{id}/progress` | `/api/parent/progress?studentProfileId={id}` | `GET` | `own` | Parent | Có sẵn |
| `/parent/approvals` | `/api/parent/approvals` | `GET` | `own` | Parent | Có sẵn |
| `/parent/account` | `/api/parent/account` | `GET` | `own` | Parent | Có sẵn |
| `/parent/account` | `/api/parent/account` | `PUT` | `own` | Parent | Có sẵn |
| `/student/dashboard` | `/api/student/dashboard` | `GET` | `own` | Student | Có sẵn |
| `/student/tests` | `/api/student/tests` | `GET` | `own` | Student | Có sẵn |
| `/student/tests/{id}` | `/api/student/tests/{id}` | `GET` | `own` | Student | Có sẵn |
| `/student/reports/lessons` | `/api/student/reports?type=lesson` | `GET` | `own` | Student | Đang dùng route thay thế |
| `/student/reports/monthly` | `/api/student/reports?type=monthly` | `GET` | `own` | Student | Đang dùng route thay thế |
| `/student/media` | `/api/student/media` | `GET` | `own` | Student | Có sẵn |
| `/teacher/dashboard` | `/api/teacher/dashboard` | `GET` | `own` | Teacher | Có sẵn |
| `/teacher/timesheet` | `/api/teacher/timesheet` | `GET` | `own` / `all` theo role | Teacher/Admin/Management | Có sẵn |
| `/staff/dashboard` | `/api/staff/dashboard` | `GET` | `department` | Staff | Có sẵn |
| `/staff/students` | `/api/staff/students` | `GET` | Thực tế đang `all` top 200 | Staff | Có sẵn, scope rộng hơn mong đợi |
| `/staff/enrollments/pending` | `/api/staff/enrollments/pending` | `GET` | `all` trong current implementation | Staff | Có sẵn |
| `/staff/announcements` | `/api/staff/announcements/history` | `GET` | `department` / history | Staff | Đang dùng route thay thế |
| `/staff/announcements` | `/api/staff/announcements` | `POST` | `department` / targeted | Staff | Có sẵn |
| `/management/dashboard` | `/api/me/management-staff/overview` | `GET` | `department` | Management | Đang dùng route thay thế |
| `/management/students` | `/api/staff-management/students` | `GET` | Thực tế đang `all` top 200 | Management | Có sẵn, scope rộng hơn mong đợi |
| `/management/accounts` | `/api/admin/users` | `GET` | `all` | Admin/Management | Đang dùng route thay thế |
| `/management/media` | `/api/staff-management/media` | `GET` | `all` non-deleted media | Management | Có sẵn |
| `/management/notification-templates` | `/api/notifications/templates` | `GET` | `all` | Admin/Management | Có sẵn |
| `/management/notification-templates` | `/api/notifications/templates` | `POST` | `all` | Admin/Management | Có sẵn |
| `/management/notification-templates/{id}` | `/api/notifications/templates/{id}` | `GET` | `all` | Admin/Management | Có sẵn |
| `/management/notification-templates/{id}` | `/api/notifications/templates/{id}` | `PUT` | `all` | Admin/Management | Có sẵn |
| `/management/notification-templates/{id}` | `/api/notifications/templates/{id}` | `DELETE` | `all` | Admin/Management | Có sẵn |

## 5. API Phụ Huynh

### 5.1. Dashboard Phụ Huynh / Tổng Quan Con

- FE endpoints:
  - `GET /parent/dashboard`
  - `GET /parent/children/{id}/overview`
- BE routes:
  - `GET /api/parent/overview`
  - `GET /api/me/parent/overview`
- Role / scope / action: `Parent` / `own selected child` / `view`
- Mục đích: trả về dashboard tổng hợp cho parent và có thể dùng lại cho `ChildOverviewCard`.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không, nhưng gần như bắt buộc nếu token không có `StudentId` | Child muốn xem |
| `classId` | `Guid?` | Không | Lọc theo lớp |
| `sessionId` | `Guid?` | Không | Lọc session cụ thể |
| `fromDate` | `DateTime?` | Không | Mặc định `now - 1 month` |
| `toDate` | `DateTime?` | Không | Mặc định `now + 1 month` |

Dữ liệu trả về thành công:

- `statistics`
  - `totalStudents`
  - `totalClasses`
  - `upcomingSessions`
  - `availableMakeupCredits`
  - `pendingHomeworks`
  - `pendingInvoices`
  - `activeMissions`
  - `totalStars`
- `studentProfiles[]`
  - `id`, `displayName`, `level`, `totalStars`, `xp`
- `classes[]`
  - `id`, `code`, `title`, `studentProfileId`, `status`
- `upcomingSessions[]`
  - `id`, `classId`, `classCode`, `studentProfileId`, `plannedDatetime`, `status`
- `recentAttendances[]`
  - `sessionId`, `classCode`, `sessionDate`, `attendanceStatus`
- `makeupCredits[]`
  - `id`, `status`, `expiresAt`, `usedSessionId`
- `pendingHomeworks[]`
  - `id`, `title`, `classId`, `classCode`, `studentProfileId`, `dueDate`, `submissionStatus`
- `recentExams[]`
  - `id`, `title`, `classId`, `classCode`, `studentProfileId`, `examDate`, `score`
- `reports[]`
  - `id`, `studentProfileId`, `classCode`, `reportMonth`, `status`
- `pendingInvoices[]`
  - `id`, `invoiceNumber`, `studentProfileId`, `amount`, `paymentStatus`, `dueDate`
- `activeMissions[]`
  - `id`, `title`, `studentProfileId`, `status`, `starReward`
- `openTickets[]`
  - `id`, `title`, `status`, `createdAt`
- `studentInfo`
- `classInfo`
- `attendanceRate`
- `homeworkCompletion`
- `xp`
- `level`
- `streak`
- `stars`
- `nextClasses`
- `pendingApprovals`
- `tuitionDue`
- `unreadNotifications`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `ParentProfile` | Current user không có parent profile active |
| `404` | `StudentId` | Token không có student được chọn và request không truyền `studentProfileId` |
| `404` | `Student` | Student không liên kết với parent hiện tại |
| `500` | Server failure | Lỗi hệ thống |

Validation và lưu ý:

- `studentProfileId` phải thuộc `ParentStudentLinks` của current parent.
- `pendingApprovals` trong `overview` hiện đang được set thành list rỗng; nếu FE cần approvals thật sự, gọi `GET /api/parent/approvals`.
- Nếu FE dùng cho `ChildOverviewCard`, nên truyền rõ `studentProfileId`.

### 5.2. Danh Sách Bài Test Của Con

- FE endpoint: `GET /parent/children/{id}/tests`
- BE route: `GET /api/parent/tests?studentProfileId={id}`
- Role / scope / action: `Parent` / `own linked child` / `view`
- Mục đích: danh sách kết quả test của child.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không | Nếu bỏ trống, controller fallback `StudentId` trong token hoặc child đầu tiên liên kết |

Dữ liệu trả về thành công:

- List item:
  - `id`
  - `title`
  - `type`
  - `subject`
  - `className`
  - `testDate`
  - `duration`
  - `status`
  - `score`
  - `maxScore`
  - `percentage`
  - `averageScore`
  - `rank`
  - `totalStudents`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student không liên kết với current parent |

Validation và lưu ý:

- `id` trong item là `ExamResult.Id`, không phải `Exam.Id`.
- `status` hiện là string được derive từ `gradedAt != null ? "Graded" : "Pending"`.

### 5.3. Danh Sách Bài Tập Của Con

- FE endpoint: `GET /parent/children/{id}/homework`
- BE route: `GET /api/parent/homework?studentProfileId={id}`
- Role / scope / action: `Parent` / `own linked child` / `view`
- Mục đích: danh sách bài tập của child.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không | Có fallback như parent tests |
| `status` | `string?` | Không | Parse sang `HomeworkStatus`; string sai sẽ bị bỏ qua |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

Dữ liệu trả về thành công:

- `items[]`
  - `id`
  - `subject`
  - `title`
  - `description`
  - `dueDate`
  - `status`
  - `submittedAt`
  - `score`
  - `priority`
  - `attachmentCount`
- `pageNumber`
- `totalPages`
- `totalCount`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student không liên kết với current parent |

Validation và lưu ý:

- `priority` là field derive: `Late` và `Missing` => `High`, còn lại => `Normal`.
- Sort hiện tại theo `Assignment.DueAt` tăng dần.

### 5.4. Danh Sách Ticket Parent

- FE endpoint: `GET /parent/tickets`
- BE route: `GET /api/tickets?mine=true`
- Role / scope / action: `Parent` / `own` / `view`
- Mục đích: danh sách support ticket của current user.

Query params FE nên dùng:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `mine` | `bool?` | Nên có | FE parent nên set `true` |
| `status` | `TicketStatus?` | Không | `Open`, `InProgress`, `Resolved`, `Closed` |
| `category` | `TicketCategory?` | Không | `Homework`, `Finance`, `Schedule`, `Tech`, `Other` |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

Dữ liệu trả về thành công:

- `tickets.items[]`
  - `id`
  - `openedByUserId`
  - `openedByUserName`
  - `openedByProfileId`
  - `openedByProfileName`
  - `branchId`
  - `branchName`
  - `classId`
  - `classCode`
  - `classTitle`
  - `category`
  - `subject`
  - `message`
  - `status`
  - `type`
  - `assignedToUserId`
  - `assignedToUserName`
  - `createdAt`
  - `updatedAt`
  - `commentCount`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- `mine=true` trong handler sẽ lấy ticket có `OpenedByUserId == currentUserId` hoặc `AssignedToUserId == currentUserId`.
- Đối với parent, thực tế thường là ticket do parent mở; tuy nhiên contract kỹ thuật vẫn bao gồm ticket assigned nếu có.

### 5.5. Media Của Con

- FE endpoint: `GET /parent/children/{id}/media`
- BE route: `GET /api/parent/media?studentProfileId={id}`
- Role / scope / action: `Parent` / `own linked child` / `view`
- Mục đích: album media của child.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không | Có fallback nếu không truyền |
| `type` | `string?` | Không | Parse sang `MediaType`: `Photo`, `Video`, `Document`; string sai sẽ bị bỏ qua |

Dữ liệu trả về thành công:

- `albums[]`
  - `albumId`
  - `title`
  - `type`
  - `date`
  - `coverUrl`
  - `count`
- `items[]`
  - `id`
  - `albumId`
  - `title`
  - `type`
  - `date`
  - `coverUrl`
  - `count`
  - `url`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student không liên kết với current parent |

Validation và lưu ý:

- API hiện lấy tối đa 100 media mới nhất.
- `albums` được group theo `albumId + yyyy-MM + type`.

### 5.6. Tiến Độ Của Con

- FE endpoint: `GET /parent/children/{id}/progress`
- BE route: `GET /api/parent/progress?studentProfileId={id}`
- Role / scope / action: `Parent` / `own linked child` / `view`
- Mục đích: tổng hợp tiến độ học tập, điểm và nhận xét.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không | Có fallback nếu không truyền |

Dữ liệu trả về thành công:

- `attendanceRate`
- `overallProgress`
- `skills[]`
  - `skill`
  - `average`
- `recentScores[]`
  - `id`, `title`, `subject`, `score`, `maxScore`, `gradedAt`
- `teacherComments[]`
  - `id`, `content`, `author`, `createdAt`
- `monthlySummaries[]`
  - `id`, `month`, `year`, `status`, `summary`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student không liên kết với current parent |

Validation và lưu ý:

- `overallProgress` là trung bình phần trăm điểm của 12 bài thi gần nhất.
- `skills` hiện được group theo `subject`.

### 5.7. Danh Sách Phê Duyệt Parent

- FE endpoint: `GET /parent/approvals`
- BE route: `GET /api/parent/approvals`
- Role / scope / action: `Parent` / `own linked child` / `view`
- Mục đích: danh sách mục cần parent xử lý.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không | Có fallback nếu không truyền |
| `status` | `string?` | Không | So khớp với field `status` của item |

Dữ liệu trả về thành công:

- Item chung:
  - `id`
  - `title`
  - `description`
  - `type`
  - `status`
  - `createdAt`
- `dueAt`
- `actionUrl`

Nguồn dữ liệu hiện tại:

- `PauseEnrollment`
- `Invoice`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student không liên kết với current parent |

Validation và lưu ý:

- API này hiện bao gồm item invoice pending/overdue, không chỉ approval nghiệp vụ học tập.
- `status` filter là so sánh string, không parse enum.

### 5.8. Chi Tiết Tài Khoản Parent

- FE endpoint: `GET /parent/account`
- BE route: `GET /api/parent/account`
- Role / scope / action: `Parent` / `own` / `view`
- Mục đích: trả về current user và parent profile hiện tại.

Query params: không có.

Dữ liệu trả về thành công:

- `user`
  - `id`, `userName`, `fullName`, `email`, `phoneNumber`, `role`, `branchId`, `branch`
  - `profiles[]`
  - `selectedProfileId`
  - `permissions[]`
  - `isActive`
  - `avatarUrl`
  - `lastLoginAt`, `lastSeenAt`, `isOnline`, `offlineDurationSeconds`
  - `createdAt`, `updatedAt`
- `parentProfile`
  - Hiện trả raw `Profile` entity, các field chính gồm:
  - `id`, `userId`, `profileType`, `displayName`, `name`, `gender`, `dateOfBirth`
  - `zaloId`, `avatarUrl`, `isApproved`, `isActive`, `isDeleted`
  - `lastLoginAt`, `lastSeenAt`, `createdAt`, `updatedAt`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `Users.NotFound` hoặc query failure | Current user lookup thất bại |

Validation và lưu ý:

- Controller không ép role `Parent`, chỉ ép authenticated user.
- Nếu current user không có parent profile, `parentProfile` có thể là `null`.

### 5.9. Cập Nhật Tài Khoản Parent

- FE endpoint: `PUT /parent/account`
- BE route: `PUT /api/parent/account`
- Role / scope / action: `Parent` / `own` / `edit`
- Mục đích: update thông tin current user và display name của profile được gửi lên.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `fullName` | `string?` | Không | Update nếu khác null/empty |
| `email` | `string?` | Không | Kiểm tra format và unique |
| `phoneNumber` | `string?` | Không | Kiểm tra SDT Việt Nam và unique |
| `avatarUrl` | `string?` | Không | Update nếu có |
| `profiles` | `UpdateProfileRequest[]?` | Không | Hiện chỉ dùng `id`, `displayName`; `isActive` bị bỏ qua |

`profiles[]`:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Profile cần update |
| `displayName` | `string?` | Không | Update nếu có |
| `isActive` | `bool?` | Không | Hiện không được command sử dụng |

Dữ liệu trả về thành công:

- `id`
- `userName`
- `fullName`
- `email`
- `phoneNumber`
- `role`
- `branchId`
- `avatarUrl`
- `isActive`
- `createdAt`
- `updatedAt`
- `profiles[]`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | Email sai format hoặc phone không hợp lệ |
| `401` | Unauthorized | Chưa login |
| `404` | `Users.NotFound` | Current user không tồn tại |
| `409` | `User.EmailAlreadyExists` | Email đã được user khác sử dụng |
| `409` | `Users.PhoneNumberNotUnique` | Số điện thoại đã tồn tại |

Validation và lưu ý:

- Phone number được normalize theo số Việt Nam.
- Field không truyền lên sẽ giữ nguyên.

## 6. API Học Sinh

### 6.1. Dashboard Học Sinh

- FE endpoint: `GET /student/dashboard`
- BE route: `GET /api/student/dashboard`
- Role / scope / action: `Student` / `own` / `view`
- Mục đích: dashboard tổng hợp học sinh.

Query params: không có.

Dữ liệu trả về thành công:

- `displayName`
- `stats`
  - `activeClasses`
  - `attendancePercent`
  - `xp`
  - `level`
  - `stars`
- `notices[]`
  - `id`, `title`, `content`, `kind`, `priority`, `createdAt`, `isRead`
- `todayClass`
  - `sessionId`, `classId`, `className`, `plannedDate`, `startTime`, `endTime`, `teacherName`, `roomName`, `status`
- `teacherNote`
- `pendingTasks[]`
  - `id`, `title`, `dueDate`, `status`, `className`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student profile không tìm thấy từ token |

Validation và lưu ý:

- `notices` hiện lấy thông báo 14 ngày gần nhất.
- `todayClass` chỉ lấy session đầu tiên trong ngày.

### 6.2. Danh Sách Bài Test Của Học Sinh

- FE endpoint: `GET /student/tests`
- BE route: `GET /api/student/tests`
- Role / scope / action: `Student` / `own` / `view`
- Mục đích: danh sách kết quả test của học sinh.

Query params: không có.

Dữ liệu trả về thành công:

- List item:
  - `id`
  - `title`
  - `type`
  - `subject`
  - `className`
  - `testDate`
  - `duration`
  - `status`
  - `score`
  - `maxScore`
  - `percentage`
  - `averageScore`
  - `rank`
  - `totalStudents`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student profile không tìm thấy |

Validation và lưu ý:

- `id` là `ExamResult.Id`, không phải `Exam.Id`.

### 6.3. Chi Tiết Bài Test Của Học Sinh

- FE endpoint: `GET /student/tests/{id}`
- BE route: `GET /api/student/tests/{id}`
- Role / scope / action: `Student` / `own` / `view`
- Mục đích: chi tiết một kết quả test.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | `ExamResult.Id` |

Dữ liệu trả về thành công:

- `examResult`
  - `id`
  - `title`
  - `type`
  - `subject`
  - `className`
  - `testDate`
  - `duration`
  - `status`
  - `score`
  - `maxScore`
  - `percentage`
  - `averageScore`
  - `rank`
  - `totalStudents`
  - `feedback`
- `skillBreakdown[]`
- `sections[]`
- `feedback`
- `ranking`
  - `rank`, `totalStudents`
- `answerSheet[]`
- `improvement`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student profile không tìm thấy |
| `404` | `ExamResult` | Result id không thuộc học sinh hiện tại hoặc không tồn tại |

Validation và lưu ý:

- `skillBreakdown`, `sections`, `answerSheet` hiện đang là empty array placeholder.

### 6.4. Báo Cáo Của Học Sinh

- FE endpoints:
  - `GET /student/reports/lessons`
  - `GET /student/reports/monthly`
- BE route: `GET /api/student/reports`
- Role / scope / action: `Student` / `own` / `view`
- Mục đích: lấy lesson reports, monthly summaries, và progress summary.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `type` | `string?` | Không | `lesson`, `monthly`, hoặc bỏ trống |

Dữ liệu trả về thành công theo `type`:

- Nếu `type=lesson`
  - `lessonReports[]`
    - `id`, `sessionId`, `className`, `reportDate`, `status`, `feedback`
- Nếu `type=monthly`
  - `monthlySummaries[]`
    - `id`, `month`, `year`, `status`, `summary`
- Nếu bỏ trống
  - `lessonReports[]`
  - `progressSummary`
    - `totalLessonReports`
    - `publishedMonthlyReports`
  - `monthlySummaries[]`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student profile không tìm thấy |

Validation và lưu ý:

- FE route `reports/lessons` và `reports/monthly` hiện là alias query param `type`.
- Khi `type` bỏ trống, API này cũng là nguồn hiện có duy nhất đang trả `progressSummary`.

### 6.5. Media Của Học Sinh

- FE endpoint: `GET /student/media`
- BE route: `GET /api/student/media`
- Role / scope / action: `Student` / `own` / `view`
- Mục đích: album media của học sinh.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `type` | `string?` | Không | Parse sang `MediaType`; sai sẽ bị bỏ qua |

Dữ liệu trả về thành công:

- `albums[]`
  - `albumId`, `title`, `type`, `date`, `coverUrl`, `count`
- `items[]`
  - `id`, `albumId`, `title`, `type`, `contentType`, `date`, `coverUrl`, `count`, `url`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `404` | `StudentProfile` | Student profile không tìm thấy |

Validation và lưu ý:

- API hiện lấy tối đa 100 media mới nhất.

## 7. API Giáo Viên

### 7.1. Dashboard Giáo Viên

- FE endpoint: `GET /teacher/dashboard`
- BE route: `GET /api/teacher/dashboard`
- Role / scope / action: `Teacher` / `own classes` / `view`
- Mục đích: dashboard giáo viên và lịch hôm nay.

Query params: không có.

Dữ liệu trả về thành công:

- `stats`
  - `totalClasses`, `totalStudents`, `upcomingSessions`, `pendingHomeworks`, `pendingReports`, `openTickets`
- `todayClasses[]`
  - item cua `upcomingSessions` loc theo hom nay
- `upcomingClasses[]`
  - `id`, `classId`, `classCode`, `plannedDatetime`, `status`, `attendanceMarked`
- `alerts[]`
  - `id`, `title`, `status`, `createdAt`
- `recentActivities[]`
  - `sessionId`, `classCode`, `sessionDate`, `presentCount`, `absentCount`
- `pendingTasks[]`
  - union của `pendingHomeworks` và `pendingReports`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không phải role `Teacher` |

Validation và lưu ý:

- `pendingHomeworks` hiện trong query handler đang là list rỗng `TODO`.
- `todayClasses` là subset của `upcomingClasses`.

### 7.2. Bảng Chấm Công Giáo Viên

- FE endpoint: `GET /teacher/timesheet`
- BE route: `GET /api/teacher/timesheet`
- Role / scope / action:
  - `Teacher` => `own`
  - `Admin`, `ManagementStaff` => có thể xem theo `teacherUserId`
- Mục đích: bảng chấm công và tổng hợp theo tháng.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `teacherUserId` | `Guid?` | Không | Admin/Management có thể truyền |
| `year` | `int?` | Không | Lọc theo năm |

Dữ liệu trả về thành công:

- `monthlyData[]`
  - `month`
  - `hours`
  - `income`
  - `rate`
  - `classCount`
  - `status`
- `yearlySummary`
  - `totalHours`
  - `totalIncome`
- `averagePerMonth`
- `totalClasses`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Role không được phép |

Validation và lưu ý:

- Teacher role sẽ bị ép xem dữ liệu của chính mình, bỏ qua `teacherUserId` query.

## 8. API Nhân Viên

### 8.1. Dashboard Nhân Viên

- FE endpoint: `GET /staff/dashboard`
- BE route: `GET /api/staff/dashboard`
- Role / scope / action: `Staff`, `ManagementStaff`, `Admin` / `department` / `view`
- Mục đích: dashboard portal nhân viên.

Query params: không có.

Dữ liệu trả về thành công:

- `activeStudents`
- `tuitionCollected`
- `pendingRegistrations`
- `recentActivities[]`
  - union của `pendingReports` và `openTickets` từ management overview

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc roles của controller |
| `404` | `User` | Current user không có branch |

Validation và lưu ý:

- `tuitionCollected` hiện đang hardcode `0m`.
- Dashboard này đang dùng `GetManagementStaffOverviewQuery`, không dùng `GetStaffOverviewQuery`.

### 8.2. Danh Sách Học Viên Cho Staff

- FE endpoint: `GET /staff/students`
- BE route: `GET /api/staff/students`
- Role / scope / action: `Staff`, `ManagementStaff`, `Admin` / thực tế đang `all` / `view`
- Mục đích: danh sách học viên để staff xử lý.

Query params: không có.

Dữ liệu trả về thành công:

- item:
  - `studentId`
  - `fullName`
  - `phone`
  - `status`
  - `course`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc roles của controller |

Validation và lưu ý:

- API hiện lấy tối đa 200 học sinh theo `DisplayName`.
- Chưa có filter theo branch như FE mong muốn.

### 8.3. Danh Sách Enrollment Chờ Duyệt

- FE endpoint: `GET /staff/enrollments/pending`
- BE route: `GET /api/staff/enrollments/pending`
- Role / scope / action: `Staff`, `ManagementStaff`, `Admin` / thực tế đang `all` / `view`
- Mục đích: danh sách đăng ký chờ duyệt.

Query params: không có.

Dữ liệu trả về thành công:

- item:
  - `id`
  - `studentName`
  - `branchName`
  - `programName`
  - `className`
  - `enrollDate`
  - `status`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc roles của controller |

Validation và lưu ý:

- Hiện chỉ lấy registration có `Status = New` hoặc `WaitingForClass`.

### 8.4. Lịch Sử Thông Báo Staff

- FE endpoint: `GET /staff/announcements`
- BE route: `GET /api/staff/announcements/history`
- Role / scope / action: `Staff`, `ManagementStaff`, `Admin` / lịch sử announcement / `view`
- Mục đích: lịch sử các đợt broadcast notification.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `senderRole` | `string?` | Không | Lọc theo sender role |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

Dữ liệu trả về thành công:

- `broadcasts.items[]`
  - `id`
  - `createdAt`
  - `channel`
  - `title`
  - `content`
  - `deeplink`
  - `kind`
  - `priority`
  - `branchId`
  - `classId`
  - `studentProfileId`
  - `senderRole`
  - `senderName`
  - `targetRole`
  - `recipientCount`
  - `createdCount`
  - `deliveredCount`
  - `pendingCount`
  - `sentCount`
  - `failedCount`
- `broadcasts.pageNumber`
- `broadcasts.totalPages`
- `broadcasts.totalCount`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc roles của controller |

Validation và lưu ý:

- Query handler hiện chỉ group notification có `SenderRole` trong `Admin` hoặc `ManagementStaff`.
- Nếu POST announcement tạo với default `SenderRole = "Staff"`, item có thể không xuất hiện trong history này. Đây là gap hiện tại trong code.

### 8.5. Tạo Thông Báo Staff

- FE endpoint: `POST /staff/announcements`
- BE route: `POST /api/staff/announcements`
- Role / scope / action: `Staff`, `ManagementStaff`, `Admin` / targeted recipients / `create`
- Mục đích: tạo broadcast notification cho một nhóm target.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `title` | `string` | Có | Max `200` |
| `content` | `string?` | Không | Max `1000` |
| `deeplink` | `string?` | Không | Deeplink FE |
| `channel` | `NotificationChannel` | Không | Mặc định `InApp` |
| `kind` | `string?` | Không | Loại thông báo |
| `priority` | `string?` | Không | Priority hiện là string tự do |
| `senderRole` | `string?` | Không | Nếu bỏ trống, controller staff set mặc định `Staff` |
| `senderName` | `string?` | Không | Nếu bỏ trống, handler lấy tên current user |
| `role` | `string?` | Không | Target role, hỗ trợ chuỗi role và alias |
| `branchId` | `Guid?` | Không | Filter recipient theo branch |
| `classId` | `Guid?` | Không | Filter recipient theo class |
| `studentProfileId` | `Guid?` | Không | Gửi cho 1 student/profile |
| `userIds` | `Guid[]?` | Không | Recipient ưu tiên cao nhất |
| `profileIds` | `Guid[]?` | Không | Map sang user ids |

Dữ liệu trả về thành công:

- `id`
- `campaignId`
- `createdAt`
- `createdCount`
- `deliveredCount`
- `createdNotificationIds[]`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | Thiếu `title`, title quá dài, content quá dài, không có recipient filter |
| `400` | `Notification.NoRecipients` | Có filter nhưng không tìm thấy recipient |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc roles của controller |

Validation và lưu ý:

- Phải có ít nhất một filter recipient: `role`, `branchId`, `classId`, `studentProfileId`, `userIds`, `profileIds`.
- Thứ tự ưu tiên recipient trong handler:
  - `userIds`
  - `profileIds`
  - `studentProfileId`
  - `classId`
  - `branchId`
  - `role`

## 9. API Quản Lý

### 9.1. Dashboard Quản Lý

- FE endpoint: `GET /management/dashboard`
- BE route: `GET /api/me/management-staff/overview`
- Role / scope / action: intended `ManagementStaff`; controller hiện đang gate `Roles = "Staff"` / `department` / `view`
- Mục đích: dashboard quản lý tổng hợp theo branch của current user.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `classId` | `Guid?` | Không | Lọc theo lớp |
| `studentProfileId` | `Guid?` | Không | Lọc theo học sinh |
| `leadId` | `Guid?` | Không | Lọc theo lead |
| `enrollmentId` | `Guid?` | Không | Lọc theo enrollment |
| `fromDate` | `DateTime?` | Không | Mặc định `now - 1 month` |
| `toDate` | `DateTime?` | Không | Mặc định `now + 1 month` |

Dữ liệu trả về thành công:

- `statistics`
  - `totalLeads`
  - `totalEnrollments`
  - `totalClasses`
  - `upcomingSessions`
  - `pendingMakeupCredits`
  - `pendingLeaveRequests`
  - `pendingReports`
  - `openTickets`
- `recentLeads[]`
  - `id`, `name`, `phoneNumber`, `status`, `createdAt`
- `recentEnrollments[]`
  - `id`, `classCode`, `studentName`, `enrollDate`, `status`
- `classes[]`
  - `id`, `code`, `title`, `enrollmentCount`, `capacity`, `status`
- `upcomingSessions[]`
  - `id`, `classId`, `classCode`, `plannedDatetime`, `status`
- `pendingMakeupCredits[]`
  - `id`, `studentName`, `status`, `expiresAt`
- `pendingLeaveRequests[]`
  - `id`, `studentName`, `requestDate`, `status`
- `pendingReports[]`
  - `id`, `studentName`, `classCode`, `status`, `reportMonth`
- `openTickets[]`
  - `id`, `title`, `status`, `priority`, `createdAt`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Role không qua authorize attribute |
| `404` | `User` | Current user không tồn tại hoặc không có `BranchId` |

Validation và lưu ý:

- Controller route hiện đang là `/api/me/management-staff/overview`, không phải `/api/management/dashboard`.
- Authorization attribute hiện đang là `Roles = "Staff"`, không phải `ManagementStaff`. Đây là mismatch cần note lại với team.

### 9.2. Danh Sách Học Sinh Management

- FE endpoint: `GET /management/students`
- BE route: `GET /api/staff-management/students`
- Role / scope / action: `ManagementStaff`, `Admin` / thực tế đang `all` / `view`
- Mục đích: danh sách học sinh phục vụ màn management.

Query params: không có.

Dữ liệu trả về thành công:

- item:
  - `studentId`
  - `fullName`
  - `className`
  - `attendanceRate`
  - `makeupCount`
  - `notes`
  - `email`
  - `phone`
  - `status`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc roles của controller |

Validation và lưu ý:

- API hiện lấy tối đa 200 học sinh.
- Chưa có filter theo branch như FE note.

### 9.3. Danh Sách Tài Khoản Management

- FE endpoint: `GET /management/accounts`
- BE route: `GET /api/admin/users`
- Role / scope / action: intended `Admin`/`Management` / `all` / `view`
- Mục đích: danh sách account người dùng.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `isActive` | `bool?` | Không | Lọc active/inactive |
| `role` | `string?` | Không | Parse sang `UserRole`; chỉ nhận `Admin`, `ManagementStaff`, `AccountantStaff`, `Teacher`, `Parent` |
| `branchId` | `Guid?` | Không | Lọc theo branch |
| `pageNumber` | `int` | Không | Mặc định FE tự quy ước; query cần có giá trị hợp lệ |
| `pageSize` | `int` | Không | Mặc định FE tự quy ước; query cần có giá trị hợp lệ |

Dữ liệu trả về thành công:

- `items[]`
  - `id`
  - `username`
  - `name`
  - `phoneNumber`
  - `email`
  - `role`
  - `branchId`
  - `branchName`
  - `isActive`
  - `isDeleted`
  - `lastLoginAt`
  - `lastSeenAt`
  - `isOnline`
  - `offlineDurationSeconds`
  - `createdAt`
  - `updatedAt`
  - `profiles[]`
    - `id`, `profileType`, `displayName`, `isActive`, `lastLoginAt`, `lastSeenAt`, `isOnline`, `offlineDurationSeconds`, `createdAt`
- `pageNumber`
- `totalPages`
- `totalCount`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `400` | `Users.InvalidRole` | `role` không parse được về enum |
| `500` | Server failure | Lỗi hệ thống |

Validation và lưu ý:

- `AdminUserController` hiện đang comment dòng `[Authorize(Roles = "Admin")]`.
- Vì vậy route này hiện không được khóa role ở controller. FE vẫn nên coi đây là route management/admin only về mặt nghiệp vụ.
- `role=Staff` và `role=Student` hiện không parse được vì `UserRole` enum không có 2 giá trị này.

### 9.4. Media Management

- FE endpoint: `GET /management/media`
- BE route: `GET /api/staff-management/media`
- Role / scope / action: `ManagementStaff`, `Admin` / `all` non-deleted media / `view`
- Mục đích: danh sách media cần management xem/duyệt.

Query params: không có.

Dữ liệu trả về thành công:

- item:
  - `id`
  - `title`
  - `className`
  - `month`
  - `status`
  - `type`
  - `uploader`
  - `uploadDate`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc roles của controller |

Validation và lưu ý:

- API hiện lấy tối đa 100 media gần nhất, không phân trang.
- `status` hiện map từ `ApprovalStatus`.

### 9.5. Danh Sách Notification Template

- FE endpoint: `GET /management/notification-templates`
- BE route: `GET /api/notifications/templates`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `view`
- Mục đích: danh sách template thông báo.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `channel` | `NotificationChannel?` | Không | `InApp`, `ZaloOa`, `Push`, `Email` |
| `isActive` | `bool?` | Không | Lọc active/inactive |
| `isDeleted` | `bool?` | Không | Nếu bỏ trống, handler mặc định `false` |
| `pageNumber` | `int` | Không | Mặc định `1`, phải > 0 |
| `pageSize` | `int` | Không | Mặc định `10`, `<= 100` |

Dữ liệu trả về thành công:

- `templates.items[]`
  - `id`
  - `code`
  - `channel`
  - `title`
  - `content`
  - `placeholders[]`
  - `placeholdersRaw`
  - `isActive`
  - `isDeleted`
  - `createdAt`
  - `updatedAt`
  - `status`
  - `category`
  - `usageCount`
- `templates.pageNumber`
- `templates.totalPages`
- `templates.totalCount`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | `pageNumber <= 0` hoặc `pageSize <= 0` hoặc `pageSize > 100` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Mặc định API không trả template soft-deleted nếu `isDeleted` không được truyền.

### 9.6. Tạo Notification Template

- FE endpoint: `POST /management/notification-templates`
- BE route: `POST /api/notifications/templates`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `create`
- Mục đích: tạo template thông báo mới.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `code` | `string` | Có | Max `100`, unique |
| `channel` | `NotificationChannel` | Có | Phải là enum hợp lệ |
| `title` | `string` | Có | Max `255` |
| `content` | `string?` | Không | Nội dung template |
| `placeholders` | `string?` | Không | Raw placeholders string |
| `category` | `string?` | Không | Nếu bỏ trống, handler tự infer |
| `isActive` | `bool` | Không | Mặc định `true` |

Dữ liệu trả về thành công:

- `id`
- `code`
- `channel`
- `title`
- `content`
- `placeholders`
- `category`
- `usageCount`
- `isActive`
- `isDeleted`
- `createdAt`
- `updatedAt`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | Thiếu `code`, `title`, `channel` invalid |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |
| `409` | `NotificationTemplate.CodeAlreadyExists` | `code` đã tồn tại |

Validation và lưu ý:

- `category` sẽ được suy ra nếu không gửi lên.

### 9.7. Chi Tiết Notification Template

- FE endpoint: `GET /management/notification-templates/{id}`
- BE route: `GET /api/notifications/templates/{id}`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `view`
- Mục đích: chi tiết 1 template.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Template id |

Dữ liệu trả về thành công:

- `id`
- `code`
- `channel`
- `title`
- `content`
- `placeholders[]`
- `placeholdersRaw`
- `isActive`
- `isDeleted`
- `createdAt`
- `updatedAt`
- `status`
- `category`
- `usageCount`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |
| `404` | `NotificationTemplate.NotFound` | Không tìm thấy id |

Validation và lưu ý:

- API detail vẫn có thể trả template đã soft-delete.

### 9.8. Cập Nhật Notification Template

- FE endpoint: `PUT /management/notification-templates/{id}`
- BE route: `PUT /api/notifications/templates/{id}`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `edit`
- Mục đích: cập nhật template.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Template id |

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `channel` | `NotificationChannel` | Có | Enum hợp lệ |
| `title` | `string` | Có | Max `255` |
| `content` | `string?` | Không | Nội dung |
| `placeholders` | `string?` | Không | Raw placeholders string |
| `category` | `string?` | Không | Nếu bỏ trống, handler tự infer |
| `isActive` | `bool` | Có | Active/inactive |

Dữ liệu trả về thành công:

- `id`
- `code`
- `channel`
- `title`
- `content`
- `placeholders`
- `category`
- `usageCount`
- `isActive`
- `isDeleted`
- `createdAt`
- `updatedAt`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | `id` rỗng, `title` rỗng, `channel` invalid |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |
| `404` | `NotificationTemplate.NotFound` | Không tìm thấy id |
| `409` | `NotificationTemplate.Deleted` | Template đã bị soft-delete |

Validation và lưu ý:

- Update không cho đổi `code`.

### 9.9. Xóa Notification Template

- FE endpoint: `DELETE /management/notification-templates/{id}`
- BE route: `DELETE /api/notifications/templates/{id}`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `delete`
- Mục đích: soft delete template.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Template id |

Dữ liệu trả về thành công:

- `id`
- `code`
- `title`
- `isDeleted`
- `updatedAt`

Response lỗi:

| HTTP | Title / code | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | `id` rỗng |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |
| `404` | `NotificationTemplate.NotFound` | Không tìm thấy id |
| `409` | `NotificationTemplate.AlreadyDeleted` | Template đã bị xóa mềm trước đó |

Validation và lưu ý:

- Delete hiện là soft delete, không xóa record khỏi DB.

## 10. Định nghĩa status

### 10.1. HomeworkStatus

| Status | Ý nghĩa |
| --- | --- |
| `Assigned` | Đã giao cho học sinh, chưa nộp |
| `Submitted` | Học sinh đã nộp |
| `Graded` | Đã chấm điểm |
| `Late` | Nộp trễ hạn |
| `Missing` | Quá hạn nhưng chưa nộp |

### 10.2. TicketStatus

| Status | Ý nghĩa |
| --- | --- |
| `Open` | Ticket mới tạo |
| `InProgress` | Đang được xử lý |
| `Resolved` | Đã xử lý xong |
| `Closed` | Đã đóng ticket |

Ticket transition hiện có trong code:

| From | To | Ghi chú |
| --- | --- | --- |
| `Open` | `InProgress` | Hợp lệ |
| `InProgress` | `Resolved` | Hợp lệ |
| `Resolved` | `Closed` | Hợp lệ |

Rule hiện tại:

- Chỉ cho phép move forward.
- Không cho quay ngược.
- `AssignTicket` nếu ticket đang `Open` sẽ auto set `InProgress`.

### 10.3. SessionStatus

| Status | Ý nghĩa |
| --- | --- |
| `Scheduled` | Đã lên lịch |
| `Completed` | Đã học xong |
| `Cancelled` | Đã hủy |

### 10.4. ReportStatus

| Status | Ý nghĩa |
| --- | --- |
| `Draft` | Bản nháp |
| `Review` | Đang chờ review |
| `Approved` | Đã duyệt |
| `Rejected` | Bị từ chối |
| `Published` | Đã công bố |

### 10.5. NotificationStatus

| Status | Ý nghĩa |
| --- | --- |
| `Pending` | Mới tạo, chưa gửi xong |
| `Sent` | Gửi thành công |
| `Failed` | Gửi thất bại |

Notification transition liên quan:

| From | To | Ghi chú |
| --- | --- | --- |
| `Pending` | `Sent` | Sau khi dispatcher/gửi thành công |
| `Pending` | `Failed` | Gửi thất bại |
| `Failed` | `Pending` | Khi gọi retry API |

### 10.6. RegistrationStatus

| Status | Ý nghĩa |
| --- | --- |
| `New` | Mới tạo |
| `WaitingForClass` | Đang chờ xếp lớp |
| `ClassAssigned` | Đã xếp lớp |
| `Studying` | Đang học |
| `Paused` | Bảo lưu |
| `Completed` | Hoàn thành |
| `Cancelled` | Đã hủy |

Staff pending enrollments hiện chỉ đọc `New` và `WaitingForClass`.

### 10.7. Trạng Thái Notification Template

| Field | Ý nghĩa |
| --- | --- |
| `isActive = true` | Template đang hoạt động |
| `isActive = false` | Template bị tắt |
| `isDeleted = true` | Template đã soft-delete |
| `status = "Active"` | Field derive từ `isActive` |
| `status = "Inactive"` | Field derive từ `isActive` |

### 10.8. Trạng Thái Và Loại Media

Approval status:

| Status | Ý nghĩa |
| --- | --- |
| `Pending` | Chờ duyệt |
| `Approved` | Đã duyệt |
| `Rejected` | Bị từ chối |

Media type:

| Type | Ý nghĩa |
| --- | --- |
| `Photo` | Ảnh |
| `Video` | Video |
| `Document` | Tài liệu |

## 11. Ma Trận Phân Quyền Theo Role

| API group | Parent | Student | Teacher | Staff | ManagementStaff | Admin |
| --- | --- | --- | --- | --- | --- | --- |
| Parent overview/tests/homework/media/progress/approvals/account | Yes | No | No | No | No | No |
| Student dashboard/tests/reports/media | No | Yes | No | No | No | No |
| Teacher dashboard | No | No | Yes | No | No | No |
| Teacher timesheet | No | No | Yes (own) | No | Yes | Yes |
| Staff dashboard/students/enrollments | No | No | No | Yes | Yes | Yes |
| Staff announcements history/create | No | No | No | Yes | Yes | Yes |
| Management dashboard/students/media | No | No | No | No | Yes | Yes |
| Management accounts | No | No | No | No | Intended Yes | Intended Yes |
| Notification template CRUD | No | No | No | No | Yes | Yes |

Lưu ý:

- Permission matrix trên đây là theo ý nghĩa nghiệp vụ và controller hiện có.
- Một số route generic/auth có implementation chưa khóa role chặt chẽ; xem mục 13.

## 12. Validation Rule Và Các Trường Hợp Trả Lỗi

### 12.1. Rule Tổng Hợp

| Rule | APIs | Lỗi trả về |
| --- | --- | --- |
| Phải có token hợp lệ | Tất cả route có `[Authorize]` | `401 Unauthorized` |
| Phải có role đúng | Teacher, Staff, Management, Template APIs | `403 Forbidden` |
| Parent chỉ được xem child đã liên kết | Parent overview/tests/homework/media/progress/approvals | `404 StudentProfile` hoặc `404 Student` |
| `studentProfileId` có thể cần bắt buộc nếu token không có selected student | Parent overview | `404 StudentId` |
| Email phải đúng format | PUT parent account | `400 Validation.General` |
| Phone phải là số Việt Nam hợp lệ | PUT parent account | `400 Validation.General` |
| Email phải unique | PUT parent account | `409 User.EmailAlreadyExists` |
| Phone phải unique | PUT parent account | `409 Users.PhoneNumberNotUnique` |
| Title announcement bắt buộc, max 200 | POST staff announcements | `400 Validation.General` |
| Content announcement max 1000 | POST staff announcements | `400 Validation.General` |
| Phải có ít nhất 1 recipient filter | POST staff announcements | `400 Validation.General` |
| `pageNumber > 0`, `pageSize <= 100` | GET notification templates | `400 Validation.General` |
| `code` template phải unique | POST template | `409 NotificationTemplate.CodeAlreadyExists` |
| Không update template đã xóa mềm | PUT template | `409 NotificationTemplate.Deleted` |
| Không xóa lại template đã xóa mềm | DELETE template | `409 NotificationTemplate.AlreadyDeleted` |

### 12.2. Lưu ý về enum và query string

- Với một số API đọc dữ liệu như parent homework/media, invalid enum string trong query không phát sinh `400`, mà sẽ bị bỏ qua.
- Với template APIs, `channel` invalid sẽ bị validator chặn và trả `400 Validation.General`.
- Với management accounts, `role` query được parse thủ công; role sai có thể trả `400 Users.InvalidRole`.

## 13. Known Gaps Và Implementation Notes

### 13.1. Khoảng Trống Về Route

- FE `parent/dashboard` và `management/dashboard` hiện đang dùng route thay thế, chưa có facade route đúng tên FE kỳ vọng.
- FE `student/reports/lessons` và `student/reports/monthly` hiện đang map vào cùng một route `/api/student/reports` với query param `type`.

### 13.2. Khoảng Trống Về Scope

- `GET /api/staff/students` hiện không filter branch; response scope rộng hơn mong đợi "theo chi nhánh".
- `GET /api/staff-management/students` hiện không filter branch.
- `GET /api/staff-management/media` hiện lấy `all` media non-deleted, không phân branch.

### 13.3. Khoảng Trống Về Auth

- `GET /api/admin/users` hiện đang comment authorize attribute.
- `GET /api/me/management-staff/overview` hiện đang gate `Roles = "Staff"` thay vì `ManagementStaff`.
- `GET /api/parent/account` và một số route parent controller chỉ dùng `[Authorize]`, không ép role `Parent` ở controller.

### 13.4. Khoảng Trống Về Dữ Liệu

- Parent `overview.pendingApprovals` hiện đang luôn là list rỗng.
- Staff announcement history query hiện chỉ đọc notification có `SenderRole` là `Admin` hoặc `ManagementStaff`, trong khi POST staff announcement mặc định set `SenderRole = "Staff"`.
- Staff dashboard `tuitionCollected` hiện đang hardcode `0m`.
- Teacher dashboard `pendingHomeworks` hiện đang là list rỗng trong query handler.

## 14. Nguồn code chính

- `Kidzgo.API/Controllers/ParentController.cs`
- `Kidzgo.API/Controllers/StudentController.cs`
- `Kidzgo.API/Controllers/TeacherController.cs`
- `Kidzgo.API/Controllers/StaffController.cs`
- `Kidzgo.API/Controllers/StaffManagementController.cs`
- `Kidzgo.API/Controllers/UserController.cs`
- `Kidzgo.API/Controllers/TicketController.cs`
- `Kidzgo.API/Controllers/NotificationController.cs`
- `Kidzgo.API/Controllers/AdminUserController.cs`
