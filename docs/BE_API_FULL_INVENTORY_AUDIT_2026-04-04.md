# BE API Full Inventory Audit

Ngày audit: 2026-04-04

## 1. Phạm vi

- Rà toàn bộ `Kidzgo.API/Controllers/*.cs`
- Đối chiếu với `docs/API-Usage-Guide.md`
- Gom tất cả vào một file duy nhất để nhìn được:
- inventory toàn bộ API modules
- route gốc của từng controller
- số lượng HTTP endpoints
- trạng thái tài liệu hóa hiện tại
- các gap lớn giữa code và doc

## 2. Tổng quan nhanh

- Tổng số controllers: `43`
- Tổng số HTTP endpoints: khoảng `371`
- Số controllers đã có body tài liệu trong `API-Usage-Guide.md`: khoảng `22`
- Số controllers chưa có body tài liệu tương ứng: `21`
- Số endpoints thuộc nhóm chưa có body tài liệu: khoảng `160`
- `API-Usage-Guide.md` hiện dừng ở section `22. Session Report APIs`
- Mục lục hiện khai tới section `41`, tức doc hiện bị thiếu phần thân khá lớn

## 3. Kết luận nhanh

- Backend không thiếu API ở mức tổng thể.
- Vấn đề lớn nhất hiện tại là tài liệu không phản ánh đúng quy mô codebase.
- Có 3 loại gap chính:
- module có code nhưng chưa có body doc
- module có doc nhưng doc mới cover một phần endpoint thật
- route ownership và authorize ở vài chỗ chưa nhất quán, làm FE khó suy luận

## 4. Inventory toàn bộ controllers

| Controller | Base route | Endpoints | Trạng thái doc | Ghi chú |
|---|---|---:|---|---|
| `AdminUserController` | `api/admin/users` | 10 | Partial | Có doc user CRUD, thiếu approve profile, assign branch, change pin, reactivate, monthly leave limit |
| `AttendanceController` | `api/attendance` | 4 | Missing | Có code, chưa có body doc |
| `AuditLogController` | `api/audit-logs` | 2 | Missing | Có code, chưa có body doc |
| `AuthenticateController` | `api/auth/` | 11 | Covered | Đã có doc tương đối đầy đủ |
| `BlogController` | `api/blogs` | 8 | Covered | Đã có doc |
| `BranchController` | `api/branches` | 7 | Missing | Có code, chưa có body doc |
| `ClassController` | `api/classes` | 8 | Covered | Đã có doc |
| `ClassroomController` | `api/classrooms` | 7 | Covered | Đã có doc |
| `DashboardController` | `api/dashboard` | 6 | Missing | Có dashboard tổng và theo mảng, chưa có body doc |
| `EmailController` | `api/` | 2 | Covered | Đã có doc |
| `EnrollmentController` | `api/enrollments` | 9 | Covered | Đã có doc |
| `ExamController` | `api/exams` | 22 | Covered | Đã có doc |
| `FileUploadController` | `api/files` | 10 | Partial | Doc mới cover upload/delete/transform, thiếu avatar/blog/lesson-plan/report/download |
| `GamificationController` | `api/gamification` | 30 | Missing | Module lớn, chưa có body doc |
| `HomeworkController` | `api/homework` | 15 | Missing | Có code khá đầy đủ, chưa có body doc |
| `InvoiceController` | `api/invoices` | 8 | Missing | Có code, chưa có body doc |
| `LeadController` | `api/leads` | 16 | Partial | Có doc phần core, thiếu self-assign, statuses, children CRUD |
| `LeaveRequestController` | `api/leave-requests` | 6 | Missing | Có code, chưa có body doc |
| `LessonPlanController` | `api/lesson-plans` | 4 | Missing | Có code, chưa có body doc |
| `LessonPlanTemplateController` | `api/lesson-plan-templates` | 5 | Missing | Có code, chưa có body doc |
| `LookupController` | `api` | 3 | Covered | Được ghi dưới Master Data APIs |
| `MakeupController` | `api/makeup-credits` | 9 | Missing | Có code, chưa có body doc |
| `MediaController` | `api/media` | 8 | Covered | Đã có doc |
| `MissionController` | `api/missions` | 7 | Missing | Có code, chưa có body doc |
| `MonthlyReportController` | `api/monthly-reports` | 14 | Covered | Đã có doc |
| `NotificationController` | `api/notifications` | 12 | Partial | Có doc inbox/broadcast/history/read, thiếu templates, retry, device-token |
| `ParentController` | `api/parent` | 8 | Missing | Có code, chưa có body doc |
| `PauseEnrollmentRequestController` | `api/pause-enrollment-requests` | 8 | Missing | Có code, chưa có body doc |
| `PayOSWebhookController` | `webhooks/payos` | 1 | Missing | Có code, chưa có body doc |
| `PlacementTestController` | `api/placement-tests` | 10 | Partial | Có doc gần đủ, thiếu retake |
| `ProfileController` | `api/profiles` | 9 | Missing | Có code, chưa có body doc |
| `ProgramController` | `api/programs` | 7 | Covered | Đã có doc |
| `QuestionBankController` | `api/question-bank` | 4 | Missing | Có code, chưa có body doc |
| `RegistrationController` | `api/registrations` | 10 | Missing | Có code vận hành thật, chưa có body doc |
| `SessionController` | `api/sessions` | 13 | Partial | Doc mới có session by id, thiếu gần như toàn bộ session management |
| `SessionReportController` | `api/session-reports` | 11 | Partial | Thiếu comments |
| `StudentController` | `api/students` | 12 | Partial | Doc mới cover classes/timetable, thiếu homework + AI speaking |
| `TeacherController` | `api/teacher` | 3 | Partial | Thiếu class students |
| `TeachingMaterialsController` | `api/teaching-materials` | 6 | Missing | Có code, chưa có body doc |
| `TicketController` | `api/tickets` | 8 | Covered | Đã có doc |
| `TuitionPlanController` | `api/tuition-plans` | 7 | Covered | Đã có doc |
| `UserController` | `api/me` | 9 | Partial | Có `GET /api/me`, logout trong auth doc, thiếu update/me và role overview endpoints |
| `ZaloWebhookController` | `webhooks/zalo` | 2 | Missing | Có code, chưa có body doc |

## 5. Các nhóm đã có doc tương đối đầy đủ

- `AuthenticateController`
- `ProgramController`
- `TuitionPlanController`
- `ClassroomController`
- `ClassController`
- `EnrollmentController`
- `ExamController`
- `TicketController`
- `BlogController`
- `LookupController`
- `MediaController`
- `MonthlyReportController`
- phần core của `PlacementTestController`
- phần core của `SessionReportController`

Ghi chú:
- “Covered” ở đây nghĩa là đã có body doc đáng kể.
- Không có nghĩa là contract trong doc chắc chắn đã 100% khớp code hiện tại.

## 6. Các nhóm có code nhưng chưa có body doc

### 6.1 Học vụ và vận hành

- `AttendanceController`
- `BranchController`
- `RegistrationController`
- `LeaveRequestController`
- `PauseEnrollmentRequestController`
- `MakeupController`

### 6.2 Học tập và nội dung

- `HomeworkController`
- `LessonPlanController`
- `LessonPlanTemplateController`
- `TeachingMaterialsController`
- `QuestionBankController`
- `MissionController`
- `GamificationController`

### 6.3 User/Profile/Portal

- `AuditLogController`
- `DashboardController`
- `ParentController`
- `ProfileController`
- `UserController` chưa có body doc riêng đúng nghĩa

### 6.4 Finance và integration

- `InvoiceController`
- `PayOSWebhookController`
- `ZaloWebhookController`

## 7. Các nhóm có doc nhưng đang thiếu endpoint thật

### 7.1 `AdminUserController`

- Đã được doc:
- get users
- get user by id
- create
- update
- delete

- Chưa thấy trong body doc:
- `PATCH /api/admin/users/{id}/assign-branch`
- `PUT /api/admin/users/{profileId}/change-pin`
- `PATCH /api/admin/users/profile/{id}/reactivate`
- `PUT /api/admin/users/approve`
- `PATCH /api/admin/programs/{programId}/monthly-leave-limit`

### 7.2 `NotificationController`

- Đã được doc:
- get notifications
- broadcast
- broadcast history
- mark as read

- Chưa thấy trong body doc:
- template CRUD dưới `api/notifications/templates`
- retry failed notification
- register device token
- delete device token

### 7.3 `SessionController`

- Body doc hiện mới có `GET /api/sessions/{id}`.
- Code thực tế còn có:
- generate from pattern
- create session
- get sessions
- update session
- bulk update by class
- cancel
- complete
- check conflicts
- CRUD session roles

### 7.4 `StudentController`

- Đã được doc:
- classes
- timetable

- Chưa thấy trong body doc:
- submit homework
- submit multiple-choice homework
- my homework list
- submitted homework list
- homework detail/submission
- hint
- recommendations
- feedback list
- speaking analysis

### 7.5 `LeadController`

- Đã được doc core lead flow.
- Chưa thấy trong body doc:
- `POST /api/leads/{id}/self-assign`
- `GET /api/leads/statuses`
- children list/create/update/delete

### 7.6 `FileUploadController`

- Đã được doc:
- upload file
- delete file
- transform

- Chưa thấy trong body doc:
- avatar upload
- blog media upload
- lesson plan media upload
- lesson plan template upload
- lesson plan materials upload
- monthly report upload
- download URL

### 7.7 `PlacementTestController`

- Chưa thấy doc cho:
- `POST /api/placement-tests/{id}/retake`

### 7.8 `SessionReportController`

- Chưa thấy doc cho:
- `POST /api/session-reports/{id}/comments`

### 7.9 `TeacherController`

- Chưa thấy doc cho:
- `GET /api/teacher/classes/{classId}/students`

### 7.10 `UserController`

- Có nhắc `GET /api/me` và logout trong Auth section.
- Nhưng chưa có body doc rõ cho:
- `PUT /api/me`
- `GET /api/me/admin/overview`
- `GET /api/me/teacher/overview`
- `GET /api/me/staff/overview`
- `GET /api/me/management-staff/overview`
- `GET /api/me/accountant-staff/overview`
- `GET /api/me/parent/overview`

## 8. Chi tiết đầy đủ cho nhóm `Missing`

### 8.1 `AttendanceController` - `api/attendance`

- `POST /api/attendance/{sessionId}`: chấm công session
- `GET /api/attendance/{sessionId}`: lấy attendance của một session
- `GET /api/attendance/students`: lấy lịch sử attendance theo học sinh
- `PUT /api/attendance/{sessionId}/students/{studentProfileId}`: cập nhật attendance của một học sinh trong session

### 8.2 `AuditLogController` - `api/audit-logs`

- `GET /api/audit-logs`: lấy danh sách audit logs
- `GET /api/audit-logs/{id}`: lấy chi tiết audit log

### 8.3 `BranchController` - `api/branches`

- `POST /api/branches`: tạo branch
- `GET /api/branches`: lấy branch public/active
- `GET /api/branches/all`: lấy tất cả branches
- `GET /api/branches/{id}`: lấy chi tiết branch
- `PUT /api/branches/{id}`: cập nhật branch
- `DELETE /api/branches/{id}`: xóa branch
- `PATCH /api/branches/{id}/status`: đổi trạng thái branch

### 8.4 `DashboardController` - `api/dashboard`

- `GET /api/dashboard/overall`: dashboard tổng
- `GET /api/dashboard/student`: dashboard student summary
- `GET /api/dashboard/academic`: dashboard học vụ
- `GET /api/dashboard/finance`: dashboard tài chính
- `GET /api/dashboard/hr`: dashboard nhân sự
- `GET /api/dashboard/leads`: dashboard CRM/leads

### 8.5 `GamificationController` - `api/gamification`

- `POST /api/gamification/stars/add`
- `POST /api/gamification/stars/deduct`
- `POST /api/gamification/xp/add`
- `POST /api/gamification/xp/deduct`
- `GET /api/gamification/stars/transactions`
- `GET /api/gamification/stars/balance`
- `GET /api/gamification/level`
- `GET /api/gamification/stars/balance/me`
- `GET /api/gamification/level/me`
- `GET /api/gamification/attendance-streak`
- `POST /api/gamification/attendance-streak/check-in`
- `GET /api/gamification/attendance-streak/me`
- `POST /api/gamification/reward-store/items`
- `GET /api/gamification/reward-store/items`
- `GET /api/gamification/reward-store/items/{id}`
- `GET /api/gamification/reward-store/items/active`
- `PUT /api/gamification/reward-store/items/{id}`
- `DELETE /api/gamification/reward-store/items/{id}`
- `PATCH /api/gamification/reward-store/items/{id}/toggle-status`
- `POST /api/gamification/reward-redemptions`
- `GET /api/gamification/reward-redemptions`
- `GET /api/gamification/reward-redemptions/{id}`
- `GET /api/gamification/reward-redemptions/me`
- `PATCH /api/gamification/reward-redemptions/{id}/approve`
- `PATCH /api/gamification/reward-redemptions/{id}/cancel`
- `PATCH /api/gamification/reward-redemptions/{id}/mark-delivered`
- `PATCH /api/gamification/reward-redemptions/{id}/confirm-received`
- `PATCH /api/gamification/reward-redemptions/batch-deliver`
- `GET /api/gamification/settings`
- `PUT /api/gamification/settings`

### 8.6 `HomeworkController` - `api/homework`

- `POST /api/homework`
- `POST /api/homework/multiple-choice`
- `POST /api/homework/multiple-choice/from-bank`
- `GET /api/homework`
- `GET /api/homework/{id}`
- `PUT /api/homework/{id}`
- `DELETE /api/homework/{id}`
- `POST /api/homework/{id}/link-mission`
- `PUT /api/homework/{id}/reward-stars`
- `POST /api/homework/submissions/{homeworkStudentId}/grade`
- `POST /api/homework/submissions/{homeworkStudentId}/quick-grade`
- `PUT /api/homework/submissions/{homeworkStudentId}/mark-status`
- `GET /api/homework/students/{studentProfileId}/history`
- `GET /api/homework/submissions`
- `GET /api/homework/submissions/{homeworkStudentId}`

### 8.7 `InvoiceController` - `api/invoices`

- `POST /api/invoices`
- `GET /api/invoices`
- `GET /api/invoices/parents/{parentProfileId}`
- `GET /api/invoices/{id}`
- `PUT /api/invoices/{id}`
- `DELETE /api/invoices/{id}`
- `POST /api/invoices/{id}/payos/create-link`
- `PATCH /api/invoices/{id}/mark-overdue`

### 8.8 `LeaveRequestController` - `api/leave-requests`

- `POST /api/leave-requests`
- `GET /api/leave-requests`
- `GET /api/leave-requests/{id}`
- `PUT /api/leave-requests/{id}/approve`
- `PUT /api/leave-requests/approve-bulk`
- `PUT /api/leave-requests/{id}/reject`

### 8.9 `LessonPlanController` - `api/lesson-plans`

- `POST /api/lesson-plans`
- `GET /api/lesson-plans/{id}`
- `GET /api/lesson-plans/classes/{classId}/syllabus`
- `PUT /api/lesson-plans/{id}`

### 8.10 `LessonPlanTemplateController` - `api/lesson-plan-templates`

- `POST /api/lesson-plan-templates`
- `GET /api/lesson-plan-templates/{id}`
- `GET /api/lesson-plan-templates`
- `POST /api/lesson-plan-templates/import`
- `PUT /api/lesson-plan-templates/{id}`

### 8.11 `MakeupController` - `api/makeup-credits`

- `POST /api/makeup-credits`
- `GET /api/makeup-credits`
- `GET /api/makeup-credits/all`
- `GET /api/makeup-credits/{id}`
- `POST /api/makeup-credits/{id}/use`
- `POST /api/makeup-credits/{id}/expire`
- `GET /api/makeup-credits/{id}/parent/get-available-sessions`
- `GET /api/makeup-credits/allocations`
- `GET /api/makeup-credits/students`

### 8.12 `MissionController` - `api/missions`

- `POST /api/missions`
- `GET /api/missions`
- `GET /api/missions/{id}`
- `PUT /api/missions/{id}`
- `DELETE /api/missions/{id}`
- `GET /api/missions/{id}/progress`
- `GET /api/missions/me/progress`

### 8.13 `ParentController` - `api/parent`

- `GET /api/parent/overview`
- `GET /api/parent/students-with-makeup-or-leave`
- `GET /api/parent/timetable`
- `GET /api/parent/attendance`
- `GET /api/parent/exam-results`
- `GET /api/parent/invoices`
- `GET /api/parent/media`
- `GET /api/parent/notifications`

### 8.14 `PauseEnrollmentRequestController` - `api/pause-enrollment-requests`

- `POST /api/pause-enrollment-requests`
- `GET /api/pause-enrollment-requests`
- `GET /api/pause-enrollment-requests/{id}`
- `PUT /api/pause-enrollment-requests/{id}/approve`
- `PUT /api/pause-enrollment-requests/approve-bulk`
- `PUT /api/pause-enrollment-requests/{id}/reject`
- `PUT /api/pause-enrollment-requests/{id}/cancel`
- `PUT /api/pause-enrollment-requests/{id}/outcome`

### 8.15 `PayOSWebhookController` - `webhooks/payos`

- `POST /webhooks/payos`

### 8.16 `ProfileController` - `api/profiles`

- `POST /api/profiles`
- `GET /api/profiles`
- `GET /api/profiles/{id}`
- `PUT /api/profiles/{id}`
- `DELETE /api/profiles/{id}`
- `PUT /api/profiles/{id}/reactivate`
- `GET /api/profiles/{id}/reactivate-and-update`
- `POST /api/profiles/link`
- `POST /api/profiles/unlink`

### 8.17 `QuestionBankController` - `api/question-bank`

- `POST /api/question-bank`
- `GET /api/question-bank`
- `POST /api/question-bank/import`
- `POST /api/question-bank/ai-generate`

### 8.18 `RegistrationController` - `api/registrations`

- `POST /api/registrations`
- `GET /api/registrations`
- `GET /api/registrations/{id}`
- `PUT /api/registrations/{id}`
- `PATCH /api/registrations/{id}/cancel`
- `GET /api/registrations/{id}/suggest-classes`
- `POST /api/registrations/{id}/assign-class`
- `GET /api/registrations/waiting-list`
- `POST /api/registrations/{id}/transfer-class`
- `POST /api/registrations/{id}/upgrade`

### 8.19 `TeachingMaterialsController` - `api/teaching-materials`

- `POST /api/teaching-materials/upload`
- `GET /api/teaching-materials`
- `GET /api/teaching-materials/lesson-bundle`
- `GET /api/teaching-materials/{id}`
- `GET /api/teaching-materials/{id}/preview`
- `GET /api/teaching-materials/{id}/download`

### 8.20 `UserController` - `api/me`

- `GET /api/me`
- `PUT /api/me`
- `POST /api/me/logout`
- `GET /api/me/admin/overview`
- `GET /api/me/teacher/overview`
- `GET /api/me/staff/overview`
- `GET /api/me/management-staff/overview`
- `GET /api/me/accountant-staff/overview`
- `GET /api/me/parent/overview`

### 8.21 `ZaloWebhookController` - `webhooks/zalo`

- `GET /webhooks/zalo/lead`
- `POST /webhooks/zalo/lead`

## 9. Chi tiết đầy đủ cho phần còn thiếu ở nhóm `Partial`

### 9.1 `AdminUserController` - các endpoint chưa được body doc cover

- `PATCH /api/admin/users/{id}/assign-branch`
- `PUT /api/admin/users/{profileId}/change-pin`
- `PATCH /api/admin/users/profile/{id}/reactivate`
- `PUT /api/admin/users/approve`
- `PATCH /api/admin/programs/{programId}/monthly-leave-limit`

### 9.2 `FileUploadController` - các endpoint chưa được body doc cover

- `POST /api/files/avatar`
- `POST /api/files/blog`
- `POST /api/files/lesson-plan/media`
- `POST /api/files/lesson-plan/template`
- `POST /api/files/lesson-plan/materials`
- `POST /api/files/reports/monthly`
- `GET /api/files/download`

### 9.3 `LeadController` - các endpoint chưa được body doc cover

- `POST /api/leads/{id}/self-assign`
- `GET /api/leads/statuses`
- `GET /api/leads/{leadId}/children`
- `POST /api/leads/{leadId}/children`
- `PUT /api/leads/{leadId}/children/{childId}`
- `DELETE /api/leads/{leadId}/children/{childId}`

### 9.4 `NotificationController` - các endpoint chưa được body doc cover

- `POST /api/notifications/templates`
- `GET /api/notifications/templates`
- `GET /api/notifications/templates/{id}`
- `PUT /api/notifications/templates/{id}`
- `DELETE /api/notifications/templates/{id}`
- `POST /api/notifications/{id}/retry`
- `POST /api/notifications/device-token`
- `DELETE /api/notifications/device-token`

### 9.5 `PlacementTestController` - endpoint chưa được body doc cover

- `POST /api/placement-tests/{id}/retake`

### 9.6 `SessionController` - các endpoint chưa được body doc cover

- `POST /api/sessions/generate-from-pattern`
- `POST /api/sessions`
- `GET /api/sessions`
- `PUT /api/sessions/{sessionId}`
- `PUT /api/sessions/by-class`
- `POST /api/sessions/{sessionId}/cancel`
- `POST /api/sessions/{sessionId}/complete`
- `POST /api/sessions/check-conflicts`
- `POST /api/sessions/{sessionId}/roles`
- `GET /api/sessions/{sessionId}/roles`
- `PUT /api/sessions/roles/{sessionRoleId}`
- `DELETE /api/sessions/roles/{sessionRoleId}`

### 9.7 `SessionReportController` - endpoint chưa được body doc cover

- `POST /api/session-reports/{id}/comments`

### 9.8 `StudentController` - các endpoint chưa được body doc cover

- `POST /api/students/homework/submit`
- `POST /api/students/homework/multiple-choice/submit`
- `GET /api/students/homework/my`
- `GET /api/students/homework/submitted`
- `GET /api/students/homework/{homeworkStudentId}`
- `POST /api/students/homework/{homeworkStudentId}/hint`
- `POST /api/students/homework/{homeworkStudentId}/recommendations`
- `GET /api/students/homework/feedback/my`
- `POST /api/students/homework/{homeworkStudentId}/speaking-analysis`
- `POST /api/students/ai-speaking/analyze`

### 9.9 `TeacherController` - endpoint chưa được body doc cover

- `GET /api/teacher/classes/{classId}/students`

### 9.10 `UserController` - các endpoint chưa được body doc cover

- `PUT /api/me`
- `GET /api/me/admin/overview`
- `GET /api/me/teacher/overview`
- `GET /api/me/staff/overview`
- `GET /api/me/management-staff/overview`
- `GET /api/me/accountant-staff/overview`
- `GET /api/me/parent/overview`

## 10. Các module lớn cần chú ý đặc biệt

### 10.1 `GamificationController`

- Có tới `30` endpoints.
- Đây là một trong những module lớn nhất hiện tại.
- Nếu FE/mobile dùng thật, việc chưa có body doc là gap lớn.

### 10.2 `HomeworkController` + `StudentController`

- Thực tế homework không chỉ nằm ở một controller.
- `HomeworkController` là teacher/staff-facing.
- `StudentController` là student-facing homework/submission/AI speaking.
- Nếu viết doc, nên gom thành cùng một domain “Homework APIs”.

### 10.3 `SessionController` + `RegistrationController`

- Đây là mảng vận hành học vụ chính.
- Code hiện có khá đầy đủ cho flow production.
- Nhưng doc hiện mới phản ánh rất ít.

### 10.4 `UserController` + `ProfileController` + `AdminUserController`

- 3 controllers này đang chia nhau các action identity/admin/profile.
- Nếu tài liệu không viết rõ ownership, FE rất dễ gọi sai route.

## 11. Rủi ro cấu trúc hiện tại

### 11.1 Mục lục không phản ánh đúng body

- `API-Usage-Guide.md` có TOC tới section `41`.
- Nhưng body thực tế chỉ tới section `22`.
- Đây là dạng tài liệu “trông như full nhưng thực ra chưa full”.

### 11.2 Có section trong TOC nhưng chưa thấy controller riêng

- `Exercise APIs` đang có trong mục lục.
- Hiện chưa thấy `ExerciseController` riêng trong `Kidzgo.API/Controllers`.
- Cần chốt lại:
- bỏ khỏi TOC
- hoặc ghi rõ exercise đang nằm trong module khác

### 11.3 Route ownership hơi pha trộn

- `PUT /api/admin/users/approve` là approve profile, không phải approve user
- `PATCH /api/admin/programs/{programId}/monthly-leave-limit` lại nằm trong `AdminUserController`

- Điều này chưa chắc sai về runtime.
- Nhưng về doc và mental model thì không ổn.

### 11.4 Authorize ở vài chỗ cần rà lại cùng lúc với doc

- `AdminUserController` đang comment `[Authorize(Roles = "Admin")]`
- `BranchController` có `GetAllBranches` đang comment authorize role

- Khi viết doc chính thức, nên chốt quyền thật đang muốn dùng, không chỉ chép route từ code

## 12. Đề xuất ưu tiên tài liệu hóa

### 12.1 P0

- `SessionController`
- `RegistrationController`
- `ProfileController`
- `UserController`
- `HomeworkController`
- `NotificationController`

### 12.2 P1

- `GamificationController`
- `TeachingMaterialsController`
- `InvoiceController`
- `ParentController`
- `BranchController`
- `AttendanceController`
- `LeaveRequestController`
- `PauseEnrollmentRequestController`
- `MakeupController`

### 12.3 P2

- `AuditLogController`
- `DashboardController`
- `MissionController`
- `LessonPlanController`
- `LessonPlanTemplateController`
- `QuestionBankController`
- `PayOSWebhookController`
- `ZaloWebhookController`

## 13. Đề xuất cách tổ chức doc sau audit

- Cách 1: tiếp tục giữ `API-Usage-Guide.md` nhưng viết nốt toàn bộ section còn thiếu
- Cách 2: tách thành nhiều file dễ maintain hơn:
- `api-auth-admin-profile.md`
- `api-academic-operations.md`
- `api-learning-content.md`
- `api-crm-finance.md`
- `api-integrations-webhooks.md`

- Nếu vẫn giữ 1 file duy nhất:
- cần cập nhật mục lục và body cùng lúc
- cần tránh để TOC đi trước body như hiện tại

## 14. Kết luận

- Bản chất hệ thống hiện tại là “code đã đi xa hơn doc”.
- Backend đã có khá nhiều API usable cho production hoặc tích hợp FE.
- Gap lớn nhất không phải thiếu chức năng, mà là thiếu tài liệu toàn cục và thiếu đồng bộ giữa tài liệu với code.
- File này là inventory tổng hợp một chỗ để từ đây có thể viết tiếp doc chuẩn mà không bị sót module.
