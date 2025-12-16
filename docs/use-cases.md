# KidzGo – Use Case chi tiết (bản tham khảo tạm thời)

Tóm tắt các luồng chính để dev bám vào chia task. Nếu nghiệp vụ đổi sẽ cập nhật sau.

---

## 1. Đăng nhập & Portal tách biệt

**Actor:** Parent, Student, Teacher, Staff, Admin  
**Mục tiêu:** Vào đúng portal với bảo mật PIN.

### 1.1 Parent/Student Portal
- Kênh: Web hoặc Zalo Mini App (đã liên kết tài khoản).
- Bước:
  1) Đăng nhập bằng Email + Mật khẩu (hoặc SSO Mini App).  
  2) Hiện danh sách Profile của user: Parent, các Student.  
  3) Chọn Profile:  
     - Parent → yêu cầu nhập PIN của profile Parent.  
     - Student → vào thẳng Student Portal.  
- Quyền: Chỉ xem/ thao tác dữ liệu bản thân (Student) hoặc con (Parent).

### 1.2 Portal nội bộ (Teacher/Staff/Admin)
- Kênh: Web nội bộ.
- Bước:
  1) Đăng nhập Email + Mật khẩu (tài khoản role TEACHER/STAFF/ADMIN).  
  2) Vào portal nội bộ đúng role; Teacher/Staff bị ràng buộc branch.

---

## 2. Leave Request & MakeUpCredit (luật 24h)

**Actor:** Parent (tạo đơn), Staff (tạo/duyệt), Teacher (điểm danh), Student  
**Mục tiêu:** Xin nghỉ, auto duyệt ≥24h, tạo credit, xếp học bù.

### 2.1 Tạo và duyệt đơn
- Parent tạo đơn nghỉ 1 ngày hoặc dài ngày; Staff có thể tạo dùm.
- Nếu đơn nghỉ 1 ngày được tạo **trước ≥24h** so với buổi học → auto-approve + tạo MakeUpCredit.
- Nếu đơn nghỉ sau 24h hoặc dài ngày → chờ Staff Approve/Reject.

### 2.2 Điểm danh buổi học
- Teacher chỉ tick Present/Absent và bấm “Nộp điểm danh” (set ActualTeacher).
- Hệ thống tự gán loại vắng: WITH_NOTICE_24H / UNDER_24H / NO_NOTICE / LONG_TERM.
- Học sinh nghỉ hợp lệ sinh MakeUpCredit; điểm danh vẫn là Absent nhưng hiển thị cờ “MakeUp” để GV biết.

### 2.3 Xếp buổi học bù
- Staff đề xuất danh sách buổi bù phù hợp (lớp tương đương).  
- Parent chọn buổi bù → hệ thống tự trừ 1 MakeUpCredit, tạo allocation.  
- Buổi bù được điểm danh với `Participation = MAKEUP` (hiển thị “Buổi bù”, không cần Present/Absent).

---

## 3. Homework & AI chấm/chấm tay

**Actor:** Teacher, Student, Parent (theo dõi)  
**Mục tiêu:** Giao bài, nộp bài, chấm điểm, theo dõi trạng thái.

### Luồng
1) Teacher tạo bài tập (form chuẩn: lớp/buổi, sách, trang, kỹ năng, submission type, max score, reward stars, gắn mission).  
2) Hệ thống auto-assign cho tất cả học sinh lớp (status = ASSIGNED).  
3) Student nộp bài (file/ảnh/text/link/quiz); quá hạn chuyển LATE/MISSING.  
4) Teacher chấm (GRADED), nhập điểm/feedback; nếu có AI chấm thì xem/chỉnh trước khi lưu.  
5) Parent/Student xem điểm, feedback, lịch sử.

---

## 4. Báo cáo tháng hỗ trợ AI

**Actor:** Teacher, Staff, Admin, Parent/Student (viewer)  
**Mục tiêu:** Sinh draft AI, review, approve, publish.

### Luồng
1) Hằng buổi: Teacher ghi quick notes theo học sinh.  
2) Cuối tháng: Job gom dữ liệu (attendance, homework, test, mission, notes) → gọi AI tạo draft.  
3) Teacher chỉnh sửa draft, submit (status REVIEW).  
4) Staff/Admin comment/chỉnh nếu cần, Approve → publish.  
5) Parent/Student xem báo cáo (HTML/PDF); gửi thông báo Zalo OA/Email khi publish.

---

## 5. Gamification (Sao & XP tách biệt)

**Actor:** Teacher/Staff (tạo mission, chỉnh sao/XP), Student/Parent (xem, đổi quà)  
**Mục tiêu:** Tạo động lực qua nhiệm vụ, sao, XP, level, cửa hàng.

### Luồng chính
- Tạo Mission (scope CLASS/STUDENT/GROUP), reward_sao, thời gian, loại (homework streak, reading streak, no unexcused absence, custom).  
- Hệ thống track progress; khi hoàn thành:  
  - Cộng **Sao (Star Points)** để đổi quà.  
  - Cộng **XP** để tăng **Level**.  
- Teacher/Staff có thể cộng/trừ Sao **và** XP thủ công kèm lý do.  
- Store: học sinh đổi quà → trừ sao; duyệt/trao quà; auto-confirm sau 3 ngày nếu cần.

---

## 6. Media & tương tác cá nhân

**Actor:** Teacher/Staff (upload, duyệt), Parent/Student (xem)  
**Mục tiêu:** Album theo tháng, cá nhân/lớp; thông báo cá nhân.

### Luồng
- Teacher upload ảnh/video, gắn tag Lớp/Học sinh/Tháng, loại hiển thị (class/personal).  
- Staff/Admin duyệt/publish lên gallery.  
- Parent/Student xem album lớp và cá nhân; tùy policy cho tải.  
- Teacher/Staff có thể gửi thông báo cá nhân/nhóm nhỏ qua Zalo OA/Mini App (nhắc bài tập, khen thưởng…).

---

## 7. Ticket hỗ trợ

**Actor:** Parent/Student (mở), Teacher/Staff (xử lý), Admin (giám sát)  
**Mục tiêu:** Ghi nhận, định tuyến, phản hồi, lưu lịch sử.

### Luồng
1) Parent/Student mở ticket: chọn bộ phận/giáo viên (homework, finance, schedule, tech…).  
2) Ticket gán cho Staff/Teacher phù hợp; có thread comment, đính kèm.  
3) Teacher/Staff phản hồi, đổi trạng thái (OPEN/IN_PROGRESS/RESOLVED/CLOSED).  
4) Lưu lịch sử, SLA theo dõi thời gian phản hồi/giải quyết.

---

## 8. Tài chính & PayOS

**Actor:** Accountant/Staff, Parent (thanh toán)  
**Mục tiêu:** Xuất hóa đơn, thu tiền PayOS/cash, đối soát, công nợ, sổ quỹ.

### Luồng chính
1) Tạo hóa đơn (MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE), sinh QR/link PayOS.  
2) Thanh toán: PayOS webhook hoặc thu tiền mặt/chuyển khoản → ghi `payments`, update invoice, ghi `cashbook_entries` (CASH_IN).  
3) Công nợ: xem trạng thái PENDING/PAID/OVERDUE, báo cáo tuổi nợ.  
4) Sổ quỹ: mọi thu/chi (invoice, payroll, expense) ghi vào `cashbook_entries`, kèm chứng từ.

---

## 9. Payroll & Hợp đồng (BHXH)

**Actor:** Accountant/Staff, Teacher/Staff (nhận lương), Admin (duyệt)  
**Mục tiêu:** Tính lương dạy, lương staff, trả lương, ghi quỹ.

### Dữ liệu nguồn
- Teaching: `session_roles` (MAIN/ASSISTANT/CLUB/WORKSHOP) + đơn giá/allowance.  
- Staff: `contracts` (base salary, **social_insurance_salary**), `shift_attendance`, phụ cấp/khấu trừ.  

### Luồng
1) Mở kỳ lương (`payroll_runs` DRAFT).  
2) Sinh `payroll_lines` từ session_roles (teaching) và contracts/shift_attendance (staff), thêm allowance/deduction.  
3) Approve kỳ lương → khóa dữ liệu tính.  
4) Trả lương (`payroll_payments`), ghi chi vào `cashbook_entries`, set run = PAID.

---

## 10. Notification (Zalo OA / Email / Push)

**Actor:** Hệ thống, Staff, Teacher  
**Mục tiêu:** Gửi nhắc nhở/hóa đơn/báo cáo/buổi bù/mission/media.

### Luồng
- Chọn template (notification/email), fill placeholders, tạo `notifications` (PENDING).  
- Worker gửi qua kênh ZALO_OA/EMAIL/PUSH, cập nhật trạng thái SENT/FAILED, lưu deeplink mở đúng màn hình (TKB, invoice, report, mission, media…).  
- Retry nếu FAILED; log để audit.

---

## 11. CRM/Lead & Placement

**Actor:** Staff (Sales/Operations), Parent (lead)  
**Mục tiêu:** Nhận lead, đặt lịch test, nhập điểm, chuyển đổi ghi danh.

### Luồng
1) Nhận lead (form web/Zalo/referral/offline) với source/campaign/branch preference.  
2) Đặt lịch placement test, nhập kết quả (listening/speaking/reading/writing), level đề xuất, file scan.  
3) Nếu chốt: tạo profile Student, link Parent, cập nhật lead → ENROLLED; nếu không → LOST/NO_SHOW.  
4) Báo cáo conversion, no-show, touch count, response time.

---

## 12. Dashboard & Audit

**Actor:** Admin/Manager, Staff  
**Mục tiêu:** Theo dõi KPI và audit.

- Học vụ: attendance, homework completion, MakeUpCredit usage, mission completion, level/sao.  
- Tài chính: doanh thu, PayOS vs cash, công nợ, dòng tiền.  
- Audit logs thao tác trên entity quan trọng; backup/restore; retry webhook/job nền.

---

## 13. Chương trình / Lớp / Lịch / Session & SessionRoles

**Actor:** Admin, Operations Staff, Teacher  
**Mục tiêu:** Tạo chương trình, mở lớp, sinh lịch buổi, phân công giáo viên/phòng/role, xử lý thay đổi TKB.

### 13.1 Chương trình & gói học phí
- Tạo/chỉnh `programs` (level, total_sessions, unit_price_session) và `tuition_plans` theo branch.
- `unit_price_session` dùng tính EXTRA_PAID, tránh double-charge.

### 13.2 Mở lớp
- Tạo `classes`: branch, program, code/title, capacity, main/assistant teacher dự kiến, schedule_pattern (RRULE/JSON).
- Kiểm tra trùng code, ràng buộc branch theo user mở lớp.

### 13.3 Sinh lịch buổi (sessions)
- Từ schedule_pattern → sinh `sessions` với planned_datetime, planned_room_id, planned_teacher_id/assistant_id, duration, participation_type=MAIN, status=SCHEDULED.
- Kiểm tra xung đột phòng/giáo viên (overlap); cảnh báo/gợi ý phòng/slot khác.

### 13.4 Phân công role chi tiết (session_roles)
- Với buổi hoàn thành, lưu `session_roles` cho từng người: MAIN_TEACHER / ASSISTANT / CLUB / WORKSHOP, kèm payable_unit_price, payable_allowance → đầu vào payroll.
- Hỗ trợ nhiều role trên một buổi (main + assistant + club).

### 13.5 Cập nhật/hoãn/đổi/hủy buổi
- Đổi giờ/phòng/teacher: cập nhật planned_*; nếu đã có attendances/makeup, gửi notification đổi lịch.
- Hủy buổi: status=CANCELLED; nếu buổi MAIN bị hủy, áp chính sách bù (có thể sinh MakeUpCredit tùy business); gửi thông báo.

### 13.6 Ghi danh & đa lớp
- `class_enrollments`: is_main phân biệt lớp chính/bổ trợ; kiểm tra capacity.
- Học sinh có thể đa lớp; participation_type ở session (MAIN/MAKEUP/EXTRA_PAID/FREE/TRIAL) dùng cho quyền lợi & tính phí.

### 13.7 Liên kết với module khác
- Attendance/MakeUp: `sessions` là gốc để điểm danh, tạo MakeUpCredit.
- Lesson plan/Homework/Exams: bám vào `sessions/classes`.
- Payroll: giờ dạy lấy từ `session_roles`.
- Invoices: `invoice_lines.session_ids` có thể gắn buổi để truy vết tính tiền.

