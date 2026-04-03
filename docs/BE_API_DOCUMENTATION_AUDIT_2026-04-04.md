# BE API Documentation Audit - Whole System

Ngày audit: 2026-04-04

## 1. Phạm vi

- Rà toàn bộ `Kidzgo.API/Controllers/*.cs`.
- Đối chiếu với `docs/API-Usage-Guide.md`.
- Mục tiêu tài liệu:
- xác định backend hiện có bao nhiêu module/API thực tế
- chỉ ra phần nào đã được tài liệu hóa, phần nào chưa
- chỉ ra các controller đã có doc nhưng body đang thiếu endpoint quan trọng
- nêu các rủi ro cấu trúc route/quyền truy cập/tổ chức tài liệu

## 2. Kết luận nhanh

- Backend hiện có `43` controllers với khoảng `371` HTTP endpoints.
- `API-Usage-Guide.md` hiện chỉ có body tài liệu đến section `22. Session Report APIs`.
- Phần body hiện cover khoảng `22` controllers với khoảng `211` endpoints.
- Còn `21` controllers với khoảng `160` endpoints chưa có body tài liệu tương ứng.
- Mục lục của `API-Usage-Guide.md` đang khai tới section `41`, nhưng nội dung thực tế dừng ở section `22`.
- Ngoài phần thiếu doc hoàn toàn, còn có nhiều controller đã có doc nhưng doc mới cover một phần, điển hình là `Notification`, `AdminUser`, `Session`, `Student`, `Lead`, `FileUpload`.

## 3. Toàn cảnh module backend

### 3.1 Nhóm identity, admin, profile

- `AuthenticateController`
- `UserController`
- `AdminUserController`
- `ProfileController`
- `ParentController`
- `BranchController`
- `AuditLogController`
- `DashboardController`

### 3.2 Nhóm vận hành học vụ

- `ProgramController`
- `TuitionPlanController`
- `ClassroomController`
- `ClassController`
- `EnrollmentController`
- `RegistrationController`
- `SessionController`
- `AttendanceController`
- `LeaveRequestController`
- `PauseEnrollmentRequestController`
- `MakeupController`
- `StudentController`
- `TeacherController`

### 3.3 Nhóm học tập, nội dung, báo cáo

- `HomeworkController`
- `QuestionBankController`
- `TeachingMaterialsController`
- `LessonPlanController`
- `LessonPlanTemplateController`
- `ExamController`
- `MonthlyReportController`
- `SessionReportController`
- `MediaController`
- `MissionController`
- `GamificationController`

### 3.4 Nhóm CRM, communication, finance, integration

- `LeadController`
- `PlacementTestController`
- `TicketController`
- `NotificationController`
- `InvoiceController`
- `BlogController`
- `FileUploadController`
- `PayOSWebhookController`
- `ZaloWebhookController`
- `LookupController`
- `EmailController`

## 4. Gap lớn nhất giữa code và tài liệu

### 4.1 `API-Usage-Guide.md` dừng giữa chừng

- Mục lục có section `23` đến `41`, gồm các nhóm như homework, mission, attendance, parent, profile, gamification, webhook.
- Nhưng body thực tế hiện dừng ở `22. Session Report APIs`.
- Điều này làm tài liệu tạo cảm giác là “đã có full doc”, nhưng thực ra phần nửa sau chưa viết.

### 4.2 Có mục lục nhưng chưa có body

- Các nhóm được hứa trong mục lục nhưng chưa có nội dung body gồm:
- `Homework APIs`
- `Exercise APIs`
- `Mission APIs`
- `Lesson Plan APIs`
- `Lesson Plan Template APIs`
- `Attendance APIs`
- `Invoice APIs`
- `Makeup Credit APIs`
- `Leave Request APIs`
- `Parent APIs`
- `Profile APIs`
- `Gamification APIs`
- `Branch APIs`
- `User APIs`
- `Session Management APIs`
- `Notification Template APIs`
- `Webhook APIs`

### 4.3 Có controller thật nhưng không nằm trong body doc

| Controller | Base route | HTTP endpoints |
|---|---|---:|
| `AttendanceController` | `api/attendance` | 4 |
| `AuditLogController` | `api/audit-logs` | 2 |
| `BranchController` | `api/branches` | 7 |
| `DashboardController` | `api/dashboard` | 6 |
| `GamificationController` | `api/gamification` | 30 |
| `HomeworkController` | `api/homework` | 15 |
| `InvoiceController` | `api/invoices` | 8 |
| `LeaveRequestController` | `api/leave-requests` | 6 |
| `LessonPlanController` | `api/lesson-plans` | 4 |
| `LessonPlanTemplateController` | `api/lesson-plan-templates` | 5 |
| `MakeupController` | `api/makeup-credits` | 9 |
| `MissionController` | `api/missions` | 7 |
| `ParentController` | `api/parent` | 8 |
| `PauseEnrollmentRequestController` | `api/pause-enrollment-requests` | 8 |
| `PayOSWebhookController` | `webhooks/payos` | 1 |
| `ProfileController` | `api/profiles` | 9 |
| `QuestionBankController` | `api/question-bank` | 4 |
| `RegistrationController` | `api/registrations` | 10 |
| `TeachingMaterialsController` | `api/teaching-materials` | 6 |
| `UserController` | `api/me` | 9 |
| `ZaloWebhookController` | `webhooks/zalo` | 2 |

### 4.4 Có doc nhưng doc chỉ cover một phần

| Controller | Code hiện có | Body doc hiện có | Gap nổi bật |
|---|---:|---:|---|
| `AdminUserController` | 10 | 5 | Thiếu `assign-branch`, `change-pin`, `profile/reactivate`, `approve`, `monthly-leave-limit` |
| `NotificationController` | 12 | 4 | Thiếu template CRUD, retry notification, register/delete device token |
| `SessionController` | 13 | 1 | Thiếu create, generate, list, update, bulk update, cancel, complete, conflict check, session roles |
| `StudentController` | 12 | 2 | Thiếu homework submit/list/detail/feedback, hint, recommendation, speaking analysis |
| `TeacherController` | 3 | 2 | Thiếu endpoint lấy danh sách học sinh trong lớp |
| `LeadController` | 16 | 10 | Thiếu `self-assign`, `statuses`, children CRUD |
| `FileUploadController` | 10 | 3 | Thiếu avatar, blog media, lesson plan uploads, monthly report upload, download URL |
| `PlacementTestController` | 10 | 9 | Thiếu `retake` |
| `SessionReportController` | 11 | 10 | Thiếu `comments` |
| `UserController` | 9 | 3 được ghi rải trong Auth section | Thiếu các role overview endpoints |

## 5. Những controller/mảng nghiệp vụ đáng chú ý

### 5.1 User/Profile/Admin hiện không thiếu code, thiếu doc và chuẩn hóa ownership

- `UserController` đang giữ:
- `GET /api/me`
- `PUT /api/me`
- `POST /api/me/logout`
- các overview theo role như `admin/overview`, `teacher/overview`, `parent/overview`
- `ProfileController` có profile CRUD, get list/detail, reactivate, link/unlink parent-student.
- `AdminUserController` ngoài user CRUD còn chứa action thuộc domain khác như:
- approve profile
- change parent pin
- assign branch
- update monthly leave limit cho program

- Rủi ro:
- route ownership đang hơi pha trộn giữa `user`, `profile`, `program`
- doc hiện tại không phản ánh cấu trúc này nên FE rất dễ hiểu sai nơi cần gọi API

### 5.2 Session và Registration là mảng học vụ lớn nhưng doc đang thiếu nặng

- `SessionController` thực tế đã là một module vận hành đầy đủ:
- generate session từ pattern
- create/update session
- bulk update theo class
- cancel/complete
- check conflict
- CRUD session roles

- `RegistrationController` cũng đã có flow vận hành thật:
- create/update/cancel registration
- suggest classes
- assign class
- waiting list
- transfer class
- upgrade tuition plan

- Nhưng phần này hiện chưa được phản ánh đầy đủ trong doc tổng.

### 5.3 Homework, Gamification, Teaching Materials là mảng lớn nhưng gần như chưa có body doc

- `HomeworkController` đã có create/update/delete assignment, link mission, grading, AI quick-grade, submission history/detail.
- `StudentController` còn mở thêm layer student-facing cho homework và AI speaking.
- `GamificationController` có tới `30` endpoints, là một trong những controller lớn nhất hiện tại.
- `TeachingMaterialsController` đã có upload/get/preview/download bundle riêng.

- Đây là các module đủ lớn để FE hoặc mobile tích hợp trực tiếp, nên việc chưa có doc là gap rõ ràng.

### 5.4 Notification và File Upload đang bị “under-documented”

- Notification guide hiện mới ghi inbox/broadcast/history/read.
- Code thực tế còn có:
- notification template CRUD
- retry failed notification
- register/delete device token cho push

- File upload guide hiện mới ghi upload, delete, transform.
- Code thực tế còn có upload chuyên biệt cho:
- avatar
- blog
- lesson plan media
- lesson plan template
- lesson plan materials
- monthly report
- download URL

## 6. Rủi ro cấu trúc và tính nhất quán hiện tại

### 6.1 Mục lục gây hiểu nhầm coverage

- Người đọc nhìn vào mục lục sẽ tưởng tài liệu đã phủ toàn bộ BE.
- Thực tế body chưa có gần nửa sau.

### 6.2 Có section hứa trong mục lục nhưng chưa thấy controller riêng

- `Exercise APIs` đang có trong mục lục.
- Hiện tại không thấy controller API riêng tên `ExerciseController` trong `Kidzgo.API/Controllers`.
- Cần chốt:
- hoặc bỏ section này khỏi mục lục
- hoặc viết rõ exercise đang nằm nhúng trong module khác

### 6.3 Một số route đang đặt ở controller không đúng domain nhất

- `PUT /api/admin/users/approve` thực chất là approve profile.
- `PATCH /api/admin/programs/{programId}/monthly-leave-limit` hiện lại nằm trong `AdminUserController`.
- Điều này không chặn hệ thống chạy, nhưng làm doc và mental model của FE khó ổn định.

### 6.4 Một số authorize/comment cho thấy cần rà lại bảo mật

- `AdminUserController` hiện đang comment `[Authorize(Roles = "Admin")]`.
- `BranchController` có `GetAllBranches` đang comment authorize role.
- Với các endpoint admin/ops, doc cần phản ánh đúng quyền thực tế sau khi code được chốt lại.

## 7. Đánh giá coverage theo mức ưu tiên tài liệu hóa

### 7.1 Ưu tiên P0

- `SessionController`
- `RegistrationController`
- `UserController`
- `ProfileController`
- `HomeworkController`
- `NotificationController`

Lý do:
- đây là các module core cho FE portal
- code đã nhiều endpoint thật
- doc hiện thiếu hoặc thiếu nặng

### 7.2 Ưu tiên P1

- `GamificationController`
- `TeachingMaterialsController`
- `InvoiceController`
- `ParentController`
- `BranchController`
- `AttendanceController`
- `LeaveRequestController`
- `PauseEnrollmentRequestController`
- `MakeupController`

### 7.3 Ưu tiên P2

- `AuditLogController`
- `DashboardController`
- `MissionController`
- `LessonPlanController`
- `LessonPlanTemplateController`
- `QuestionBankController`
- `PayOSWebhookController`
- `ZaloWebhookController`

## 8. Đề xuất cách sửa doc tổng

- Bước 1: sửa `API-Usage-Guide.md` để body thực sự có đủ section `23` tới `41`.
- Bước 2: với các controller đã có doc nhưng thiếu endpoint, bổ sung ngay theo route thật đang chạy.
- Bước 3: chuẩn hóa lại mục lục để không lặp/không treo section không tồn tại.
- Bước 4: cân nhắc tách doc lớn thành các file domain riêng:
- `api-auth-admin.md`
- `api-academic-operations.md`
- `api-learning-content.md`
- `api-crm-finance-communication.md`
- `api-integrations-webhooks.md`

## 9. Kết luận

- Vấn đề của backend hiện tại không phải là thiếu API toàn hệ thống.
- Vấn đề lớn hơn là tài liệu đang không phản ánh đúng quy mô thật của codebase.
- Ở mức code, backend đã có rất nhiều module đã usable.
- Ở mức tài liệu, hiện vẫn còn khoảng `21` controllers và khoảng `160` endpoints chưa có body doc tương ứng.
- Nếu cần một bản “doc toàn bộ”, hướng đúng là viết audit và tài liệu hóa theo toàn hệ thống như trên, không tách riêng một nhánh như `profile approval`.
