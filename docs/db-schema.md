# KidzGo DB Schema (dbdiagram.io) – Giải thích & quan hệ

File sơ đồ: `db/schema.dbml` (import trực tiếp dbdiagram.io). Nội dung dưới đây giải thích chức năng, quan hệ chính, và cách thực thi để đáp ứng các use-case.

## Nguyên tắc chung
- Mọi thực thể chính gắn `branch_id` để hỗ trợ multi-branch; admin nhìn toàn bộ, giáo viên/staff bị ràng buộc branch theo `profiles`.
- Đăng nhập dùng `users` (email/mật khẩu) + chọn `profiles` (Parent/Student/Teacher/Staff); profile nội bộ có `pin_hash`.
- Quyền hạn qua `roles` + `profile_roles`; giáo viên/staff có branch cố định, admin không ràng buộc.
- Status/type dùng varchar (enum app-level) để dễ mở rộng; có thể đổi sang enum DB nếu cần.
- `jsonb` dùng cho nội dung động (giáo án, AI draft, attachment list…).

## Khối User/Branch/RBAC
- `branches`: danh mục chi nhánh.
- `users`: tài khoản đăng nhập chung.
- `profiles`: profile cụ thể (Parent/Student/Teacher/Staff), liên kết user; `pin_hash` bắt buộc với Teacher/Staff khi chọn profile; `branch_id` bắt buộc cho Teacher/Staff.
- `parent_student_links`: map phụ huynh ↔ học sinh; hỗ trợ 1 phụ huynh có nhiều con và (nếu cần) 1 học sinh có nhiều người giám hộ. Mỗi liên kết là một record giữa `parent_profile` (profile_type=PARENT) và `student_profile` (profile_type=STUDENT).
- `roles`, `profile_roles`: RBAC gán theo profile (không gán trực tiếp cho user); admin có quyền toàn bộ branch, role khác bị giới hạn branch theo `profile.branch_id`.
- Lưu ý multi-role cho cùng một user:
  - Một user có thể có nhiều profile (ví dụ vừa là Admin vừa là Teacher): tạo 2 profile cùng `user_id`, mỗi profile mang role phù hợp.
  - Vì Teacher/Staff bị khóa branch và yêu cầu PIN, còn Admin cần scope toàn hệ thống, nên tách profile giúp không bị dính ràng buộc branch/PIN khi vào vai Admin.

## Khối Chương trình/Lớp/Lịch
- `programs`: khóa/chương trình, số buổi, giá tham chiếu.
- `classrooms`: phòng học theo branch, có capacity/note để tránh trùng phòng.
- `classes`: lớp thuộc branch + program; lưu giáo viên chính/trợ giảng dự kiến, lịch pattern (RRULE/JSON); trạng thái PLANNED/ACTIVE/CLOSED; `capacity` để kiểm soát sĩ số.
- `class_enrollments`: ghi danh học sinh vào lớp; `is_main` phân biệt lớp chính hay bổ trợ; `tuition_plan_id` cho phép tùy biến học phí theo lớp/học sinh.
- `sessions`: từng buổi học (instance) sinh ra từ lớp/lịch; chứa giáo viên/phòng dự kiến và thực tế, thời lượng; `participation_type` MAIN/MAKEUP/EXTRA_PAID/FREE/TRIAL để tính tiền/MakeUp; trạng thái SCHEDULED/COMPLETED/CANCELLED.

## Khối Nghỉ/Điểm danh/Học bù
- `leave_requests`: yêu cầu nghỉ, lưu notice_hours để áp dụng luật 24h.
- `attendances`: điểm danh buổi; `absence_type` phân loại vắng; gán `marked_by`.
- `makeup_credits`: credit tạo từ vắng hợp lệ; `used_session_id` lưu buổi bù đã dùng.
- `makeup_allocations`: staff gán credit vào buổi bù cụ thể.

## Khối Giáo án/Bài tập/Notes
- `lesson_plan_templates`: giáo án khung theo program/level/session_index.
- `lesson_plans`: phiếu giáo án cho từng session, chứa planned vs actual, homework thực tế, notes.
- `homework_assignments`: bài tập cho lớp/buổi; cấu hình sách/trang/kỹ năng, submission_type, reward_stars.
- `homework_student`: trạng thái bài tập từng học sinh, nộp bài, chấm điểm, AI feedback, file đính kèm. Ràng buộc unique (assignment_id, student_profile_id).

## Khối Kiểm tra định kỳ
- `exams`: kỳ kiểm tra (progress/mid/final/speaking...).
- `exam_results`: điểm & nhận xét per student; hỗ trợ đính kèm scan.

## Khối Báo cáo tháng + AI
- `monthly_report_jobs`: batch sinh draft từ AI theo tháng/branch.
- `student_monthly_reports`: draft/final nội dung, status DRAFT/REVIEW/APPROVED/REJECTED; ai_version.
- `report_comments`: thread review giữa teacher/staff/admin.

## Khối Gamification
- `missions`: nhiệm vụ (scope class/student/group), loại mission, thời gian, sao thưởng.
- `mission_progress`: trạng thái nhiệm vụ từng học sinh; unique (mission_id, student_profile_id).
- `star_transactions`: ledger sao (+/-) với reason/source, lưu balance_after để audit.
- `student_levels`: level/xp hiện tại.
- `reward_store_items`, `reward_redemptions`: cửa hàng và đổi quà.

## Khối CRM/Placement
- `leads`: lead từ landing/Zalo/referral; owner staff; trạng thái chăm sóc.
- `placement_tests`: lịch & kết quả test xếp lớp, gắn lead hoặc student.

## Khối Media
- `media_assets`: ảnh/video, tag branch/class/student, tháng (YYYY-MM), visibility (class/personal/public_parent).

## Khối Tài chính/PayOS/Sổ quỹ/Lương
- `tuition_plans`: học phí theo program/branch; unit_price_session để tính EXTRA_PAID.
- `invoices`: hóa đơn (MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE), trạng thái, PayOS link/QR.
- `invoice_lines`: chi tiết dòng, quantity/unit_price, session_ids nếu cần trace.
- `payments`: kết quả thanh toán (PayOS/cash/bank); webhook PayOS map vào đây và update invoice.
- `cashbook_entries`: sổ quỹ thu/chi, liên kết INVOICE/PAYROLL/EXPENSE/ADJUSTMENT.
- `contracts`: hợp đồng lao động (full/part-time), base_salary/hourly_rate, hiệu lực.
- `shift_attendance`: chấm công/ca cho staff part-time.
- `session_roles`: ai dạy buổi nào, role & đơn giá/allowance để tính lương teaching/TA/club.
- `payroll_runs`: kỳ lương theo branch, status DRAFT/APPROVED/PAID.
- `payroll_lines`: chi tiết thành phần lương (teaching/base/overtime/allowance/deduction), nguồn session/contract.
- `payroll_payments`: thanh toán lương, method, link cashbook.

## Khối Notification & Ticket
- `notifications`: log gửi Zalo OA/push/email + deeplink target mini-app/web.
- `notification_templates`: template nội dung theo channel/code, placeholders, hỗ trợ tái sử dụng và i18n.
- `email_templates`: template email (subject/body, placeholders) để gửi mail giao dịch/hệ thống.
- `tickets`: phản hồi/hỗ trợ; mở bởi phụ huynh/teacher/staff; assign người xử lý.
- `ticket_comments`: thread trao đổi.

## Audit
- `audit_logs`: trace thao tác (before/after) cho các entity quan trọng.

## Soft delete & integrity bổ sung
- Các bảng cần version/ẩn thay vì xóa: `programs`, `lesson_plan_templates`, `tuition_plans`, `reward_store_items`, `notification_templates` có cờ `is_deleted`.
- Ràng buộc unique:
  - `attendances`: (session_id, student_profile_id).
  - `homework_student`: (assignment_id, student_profile_id).
  - `mission_progress`: (mission_id, student_profile_id).

## Gợi ý thực thi & mapping use-case
- Multi-branch: mọi bảng nghiệp vụ có `branch_id`; truy vấn luôn kèm filter branch dựa trên profile (trừ admin).
- Login + profile + PIN: auth trả về danh sách profiles; khi chọn Teacher/Staff yêu cầu PIN; set context branch = profile.branch_id.
- Điểm danh + MakeUp: teacher chỉ mark Present/Absent; service dựa `leave_requests` + `notice_hours` + `absence_type` để tạo `makeup_credits`; staff gán qua `makeup_allocations`.
- Giáo án/bài tập: tạo template theo `program`, sinh `lesson_plans` khi tạo sessions; homework gán class/buổi, auto tạo `homework_student`.
- Kiểm tra + báo cáo tháng: `exams` + `exam_results` cấp dữ liệu; `student_monthly_reports` chứa draft/final; comment/approve/publish.
- Gamification: `missions` → `mission_progress`; hoàn thành sinh `star_transactions`; level tính từ tổng sao/xp; redeem quà qua `reward_redemptions`.
- Tài chính: hóa đơn & PayOS → `payments` → update `invoices` + ghi `cashbook_entries` (CASH_IN); lương/payout ghi `payroll_runs/lines/payments` + `cashbook_entries` (CASH_OUT).
- Media: upload kèm tag class/student/month để render album theo tháng cho phụ huynh.
- Notification: `notifications` lưu deeplink tới màn hình mini app/web (TKB, invoice, report, mission, media…).

## Import dbdiagram.io
1) Mở dbdiagram.io → Import → dán nội dung `db/schema.dbml`.
2) Chọn DBML. Các comment enum dùng cho app layer; có thể chuyển sang enum DB nếu muốn.

## Điều chỉnh/Tuỳ chọn
- Có thể tách bảng lookup cho enum (attendance_status, absence_type, mission_type…) nếu muốn khóa cứng.
- Nếu cần audit file/media lưu S3, thêm trường metadata (size, mime).
- Nếu dùng Postgres: nên thêm partial index trên (profile_type, branch_id), (student_profile_id, status) cho bảng lớn như `homework_student`, `attendances`.
- Với PayOS webhook: lưu thêm bảng `webhook_events` nếu cần idempotency.‬‬

