# KidzGo DB Schema (dbdiagram.io) – Giải thích & quan hệ

File sơ đồ: `db/schema.dbml` (import trực tiếp dbdiagram.io). Nội dung dưới đây giải thích chức năng, quan hệ chính, và cách thực thi để đáp ứng các use-case.

## ⚠️ THAY ĐỔI RBAC (Updated)

**Thay đổi chính về cấu trúc RBAC**:
- `users` có thêm `role` enum: `PARENT`, `STUDENT`, `ADMIN`, `TEACHER`, `STAFF`
- `profiles` **chỉ dùng cho PARENT/STUDENT** (bỏ TEACHER/STAFF/ADMIN)
- ADMIN/TEACHER/STAFF dùng trực tiếp `users` với `role`, không dùng `profiles`
- Tất cả các bảng reference ADMIN/TEACHER/STAFF: chuyển từ `profiles.id` → `users.id`
- STUDENT vẫn dùng `profiles.id` (không thay đổi)

**Chi tiết thay đổi**:
- Xem phần **2. Khối User/Branch/RBAC** để biết chi tiết về cấu trúc mới
- Tất cả các bảng có thay đổi đều được đánh dấu `(CHANGED: ...)` trong phần mô tả attribute

## 1. Nguyên tắc chung
- Mọi thực thể chính gắn `branch_id` để hỗ trợ multi-branch; admin nhìn toàn bộ, giáo viên/staff bị ràng buộc branch theo `users.branch_id`.
- Đăng nhập dùng `users` (email/mật khẩu) với `role` enum (PARENT/STUDENT/ADMIN/TEACHER/STAFF).
- **PARENT/STUDENT**: dùng chung account, sau login chọn profile (PARENT hoặc STUDENT).
- **ADMIN/TEACHER/STAFF**: dùng account riêng, không dùng profile, role lưu trực tiếp trong `users.role`.
- Quyền hạn: ADMIN/TEACHER/STAFF dùng trực tiếp `users.role`; PARENT/STUDENT có thể dùng `profile_roles` (tùy chọn).
- Status/type dùng varchar (enum app-level) để dễ mở rộng; có thể đổi sang enum DB nếu cần.
- `jsonb` dùng cho nội dung động (giáo án, AI draft, attachment list…).

## 2. Khối User/Branch/RBAC

### 2.1. Cấu trúc RBAC mới (Updated)

**Thay đổi chính**:
- `users` có `role` enum: `PARENT`, `STUDENT`, `ADMIN`, `TEACHER`, `STAFF`
- `profiles` chỉ dùng cho **PARENT/STUDENT** (không dùng cho ADMIN/TEACHER/STAFF)
- ADMIN/TEACHER/STAFF dùng trực tiếp `users` với `role`, không cần `profiles`

**Login & Authentication**:
- **PARENT/STUDENT**: 
  - Dùng **tài khoản chung `users`** (email + mật khẩu) với `role = PARENT` hoặc `role = STUDENT`
  - Sau login, chọn profile (PARENT hoặc STUDENT) từ danh sách profiles của user đó
  - Chọn PARENT → vào Parent Portal, **bắt buộc nhập PIN** (`profiles.pin_hash`)
  - Chọn STUDENT → vào Student Portal (không dùng PIN)
- **ADMIN/TEACHER/STAFF**: 
  - Mỗi người dùng **tài khoản riêng** (`users` với `role = ADMIN/TEACHER/STAFF`)
  - Đăng nhập trực tiếp, không chọn profile
  - `users.pin_hash` bắt buộc cho ADMIN/TEACHER/STAFF (bảo mật)
  - `users.branch_id` bắt buộc cho TEACHER/STAFF (ràng buộc chi nhánh), null cho ADMIN (xem toàn hệ thống)

**Bảng chính**:
- `branches`: danh mục chi nhánh
- `users`: 
  - Tài khoản đăng nhập với `role` enum (PARENT/STUDENT/ADMIN/TEACHER/STAFF)
  - `display_name`: bắt buộc cho ADMIN/TEACHER/STAFF, optional cho PARENT/STUDENT
  - `pin_hash`: bắt buộc cho ADMIN/TEACHER/STAFF, optional cho PARENT/STUDENT
  - `branch_id`: bắt buộc cho TEACHER/STAFF, null cho ADMIN/PARENT/STUDENT
- `profiles`: 
  - **Chỉ dùng cho PARENT/STUDENT** (`profile_type` chỉ có PARENT hoặc STUDENT)
  - Liên kết với `users` qua `user_id`
  - `pin_hash`: bắt buộc cho PARENT profile (khi chọn profile)
  - Không có `branch_id` (không cần cho PARENT/STUDENT)
- `parent_student_links`: map phụ huynh ↔ học sinh; hỗ trợ 1 phụ huynh nhiều con và (nếu cần) 1 học sinh nhiều người giám hộ
- **Lưu ý**: Bảng `roles` và `profile_roles` đã được bỏ (REMOVED). Tất cả role được quản lý trực tiếp qua `users.role` enum.

**Reference trong các bảng**:
- **ADMIN/TEACHER/STAFF**: Tất cả các bảng reference từ `profiles.id` → chuyển sang `users.id`
  - Ví dụ: `classes.main_teacher_id`, `sessions.planned_teacher_id`, `attendances.marked_by`, `invoices.issued_by`, `contracts.staff_user_id`, v.v.
- **STUDENT**: Vẫn dùng `profiles.id` (vì student dùng profile)
  - Ví dụ: `class_enrollments.student_profile_id`, `attendances.student_profile_id`, `homework_student.student_profile_id`, v.v.

**UI & Permission**:
- Chức năng Parent/Student tách biệt với Teacher/Staff cả về UI lẫn permission
- ADMIN/TEACHER/STAFF: kiểm soát theo `users.role` và `users.branch_id`
- PARENT/STUDENT: kiểm soát theo `profiles.profile_type`

## 3. Khối Chương trình/Lớp/Lịch
- `programs`: khóa/chương trình, số buổi, giá tham chiếu.
- `classrooms`: phòng học theo branch, có capacity/note để tránh trùng phòng.
- `classes`: lớp thuộc branch + program; lưu giáo viên chính/trợ giảng dự kiến, lịch pattern (RRULE/JSON); trạng thái PLANNED/ACTIVE/CLOSED; `capacity` để kiểm soát sĩ số.
- `class_enrollments`: ghi danh học sinh vào lớp; `is_main` phân biệt lớp chính hay bổ trợ; `tuition_plan_id` cho phép tùy biến học phí theo lớp/học sinh.
- `sessions`: từng buổi học (instance) sinh ra từ lớp/lịch; chứa giáo viên/phòng dự kiến và thực tế, thời lượng; `participation_type` MAIN/MAKEUP/EXTRA_PAID/FREE/TRIAL để tính tiền/MakeUp; trạng thái SCHEDULED/COMPLETED/CANCELLED.
- `session_roles`: chi tiết ai dạy buổi nào với vai trò cụ thể (MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP); lưu `payable_unit_price` và `payable_allowance` để tính lương. Một session có thể có nhiều `session_roles` (ví dụ: 1 giáo viên chính + 1 trợ giảng + 1 giáo viên club). Bảng này là nguồn dữ liệu để tính lương trong `payroll_lines` (component_type=TEACHING/TA/CLUB/WORKSHOP).

## 4. Khối Nghỉ/Điểm danh/Học bù

Khối này quản lý việc nghỉ học, điểm danh và học bù của học sinh. Luồng hoạt động: **Yêu cầu nghỉ** → **Điểm danh** → **Tạo credit học bù** → **Phân bổ vào buổi bù**.

### 4.1. `leave_requests` - Yêu cầu nghỉ học

**Mục đích**: Lưu trữ yêu cầu nghỉ học của học sinh trước khi buổi học diễn ra.

**Các attribute quan trọng**:
- `student_profile_id`: Học sinh xin nghỉ
- `class_id`: Lớp học liên quan
- `session_date` / `end_date`: Ngày nghỉ (hỗ trợ nghỉ nhiều ngày)
- `notice_hours`: **Số giờ báo trước** - quan trọng để áp dụng luật 24h
- `status`: PENDING → APPROVED/REJECTED

**Luật 24h**: Nếu học sinh báo trước ≥ 24 giờ (`notice_hours >= 24`), khi được APPROVED sẽ tự động tạo `makeup_credits`. Nếu < 24h hoặc không báo trước, không được credit học bù.

**Ví dụ**:
- Học sinh A xin nghỉ buổi học ngày 15/03, báo trước 30 giờ → APPROVED → Tạo credit
- Học sinh B xin nghỉ buổi học ngày 16/03, báo trước 12 giờ → APPROVED → Không tạo credit

### 4.2. `attendances` - Điểm danh buổi học

**Mục đích**: Ghi nhận sự có mặt/vắng mặt của học sinh trong từng buổi học cụ thể.

**Các attribute quan trọng**:
- `session_id`: Buổi học được điểm danh
- `student_profile_id`: Học sinh được điểm danh
- `attendance_status`: PRESENT (có mặt) / ABSENT (vắng) / MAKEUP (học bù)
- `absence_type`: Phân loại vắng:
  - `WITH_NOTICE_24H`: Vắng có báo trước ≥ 24h (có `leave_request` APPROVED)
  - `UNDER_24H`: Vắng có báo nhưng < 24h
  - `NO_NOTICE`: Vắng không báo trước
  - `LONG_TERM`: Nghỉ dài hạn (bệnh, gia đình...)
- `marked_by`: Giáo viên điểm danh
- `marked_at`: Thời gian điểm danh

**Unique constraint**: Mỗi học sinh chỉ có 1 record điểm danh cho mỗi buổi học `(session_id, student_profile_id)`.

**Luồng xử lý**:
1. Giáo viên điểm danh: Chọn PRESENT/ABSENT
2. Hệ thống tự động xác định `absence_type` dựa trên:
   - Có `leave_request` APPROVED với `notice_hours >= 24` → `WITH_NOTICE_24H`
   - Có `leave_request` APPROVED với `notice_hours < 24` → `UNDER_24H`
   - Không có `leave_request` → `NO_NOTICE`
3. Nếu `absence_type = WITH_NOTICE_24H` hoặc `LONG_TERM` → Tạo `makeup_credits`

### 4.3. `makeup_credits` - Credit học bù

**Mục đích**: Lưu trữ "quyền học bù" của học sinh, được tạo tự động từ vắng hợp lệ.

**Các attribute quan trọng**:
- `student_profile_id`: Học sinh sở hữu credit
- `source_session_id`: **Buổi học nghỉ hợp lệ** (buổi học mà học sinh đã nghỉ, tạo ra credit này)
- `status`: 
  - `AVAILABLE`: Chưa sử dụng, có thể dùng
  - `USED`: Đã sử dụng (đã học bù)
  - `EXPIRED`: Hết hạn (quá `expires_at`)
- `created_reason`: Lý do tạo credit:
  - `APPROVED_LEAVE_24H`: Nghỉ có báo trước ≥ 24h
  - `LONG_TERM`: Nghỉ dài hạn
- `expires_at`: Thời hạn sử dụng (ví dụ: 3 tháng sau khi tạo)
- `used_session_id`: **Buổi học bù đã dùng** (buổi học mà học sinh tham gia để học bù, null nếu chưa dùng)

**Sự khác biệt giữa `source_session_id` và `used_session_id`**:

| Attribute | Mục đích | Thời điểm | Giá trị |
|-----------|----------|-----------|---------|
| `source_session_id` | **Buổi học nghỉ** - Buổi học mà học sinh đã nghỉ hợp lệ, tạo ra credit này | Khi tạo credit (ngay từ đầu) | Luôn có giá trị (NOT NULL) |
| `used_session_id` | **Buổi học bù** - Buổi học mà học sinh tham gia để học bù, sử dụng credit này | Khi staff gán credit vào buổi bù | NULL khi chưa dùng, có giá trị khi đã dùng |

**Ví dụ cụ thể**:

```
Tình huống: Học sinh A nghỉ buổi học Thứ 2 (15/03), được học bù vào buổi Thứ 6 (20/03)

1. Khi nghỉ buổi 15/03 (báo trước ≥ 24h):
   → Tạo makeup_credit:
     - source_session_id: session_15_03 (buổi học nghỉ)
     - used_session_id: NULL (chưa học bù)
     - status: AVAILABLE

2. Staff gán credit vào buổi học bù 20/03:
   → Cập nhật makeup_credit:
     - source_session_id: session_15_03 (vẫn giữ nguyên - buổi nghỉ)
     - used_session_id: session_20_03 (buổi học bù mới)
     - status: USED

3. Kết quả:
   - source_session_id: Buổi học nghỉ (15/03) - không thay đổi
   - used_session_id: Buổi học bù (20/03) - được cập nhật khi gán
```

**Lưu ý**:
- `source_session_id` là **bất biến** (immutable) - không bao giờ thay đổi sau khi tạo credit
- `used_session_id` bắt đầu là **NULL**, được cập nhật khi credit được sử dụng
- Một credit chỉ có thể dùng 1 lần, nên `used_session_id` chỉ được set 1 lần
- Có thể query: "Học sinh nào nghỉ buổi X?" → `source_session_id = X`
- Có thể query: "Buổi học nào được dùng để học bù?" → `used_session_id = X`

**Cách tạo credit**:
- Tự động: Khi `leave_request` được APPROVED với `notice_hours >= 24`
- Tự động: Khi `attendance` có `absence_type = WITH_NOTICE_24H` hoặc `LONG_TERM`
- Thủ công: Staff có thể tạo credit đặc biệt

**Ví dụ**:
```
Học sinh A nghỉ buổi học ngày 15/03 (báo trước 30h) → APPROVED
→ Tạo makeup_credit:
  - source_session_id: session_15_03
  - status: AVAILABLE
  - expires_at: 15/06/2024 (3 tháng sau)
  - created_reason: APPROVED_LEAVE_24H
```

### 4.4. `makeup_allocations` - Phân bổ credit vào buổi bù

**Mục đích**: Gán credit học bù vào một buổi học cụ thể để học sinh tham gia học bù.

**Các attribute quan trọng**:
- `makeup_credit_id`: Credit được phân bổ
- `target_session_id`: Buổi học bù được gán (học sinh sẽ học buổi này để bù)
- `assigned_by`: Staff gán credit
- `assigned_at`: Thời gian gán

**Luồng hoạt động**:
1. Staff xem danh sách `makeup_credits` có `status = AVAILABLE`
2. Chọn một buổi học phù hợp (`target_session_id`) - có thể là buổi khác lớp, khác giờ
3. Tạo `makeup_allocation` để gán credit vào buổi đó
4. Cập nhật `makeup_credit.status = USED` và `used_session_id = target_session_id`
5. Khi học sinh tham gia buổi bù, điểm danh với `attendance_status = MAKEUP`

**Ví dụ**:
```
Học sinh A có credit từ buổi 15/03
Staff gán vào buổi học ngày 20/03 (lớp khác, cùng level)
→ Tạo makeup_allocation:
  - makeup_credit_id: credit_15_03
  - target_session_id: session_20_03
→ Cập nhật makeup_credit:
  - status: USED
  - used_session_id: session_20_03
→ Khi điểm danh buổi 20/03:
  - attendance_status: MAKEUP
```

### 4.5. Quan hệ giữa các bảng

**Luồng hoàn chỉnh**:

```
1. leave_requests (yêu cầu nghỉ)
   ↓ (nếu APPROVED và notice_hours >= 24)
   
2. attendances (điểm danh - vắng)
   ↓ (nếu absence_type = WITH_NOTICE_24H)
   
3. makeup_credits (tạo credit học bù)
   ↓ (staff gán vào buổi học phù hợp)
   
4. makeup_allocations (phân bổ credit)
   ↓ (học sinh tham gia buổi bù)
   
5. attendances (điểm danh - MAKEUP)
```

**Lưu ý quan trọng**:
- Một `makeup_credit` chỉ có thể dùng 1 lần (1:1 với `makeup_allocation`)
- Một buổi học có thể có nhiều học sinh học bù (nhiều `makeup_allocations` → cùng `target_session_id`)
- `makeup_credits` có thể hết hạn (`expires_at`), cần kiểm tra trước khi gán
- Học sinh có thể có nhiều credit chưa dùng (`status = AVAILABLE`)

## 5. Khối Giáo án/Bài tập/Notes

Khối này quản lý giáo án giảng dạy và bài tập về nhà. Luồng hoạt động: **Template giáo án** → **Giáo án buổi học** → **Giao bài tập** → **Học sinh nộp bài** → **Chấm điểm**.

### 5.1. `lesson_plan_templates` - Template giáo án

**Mục đích**: Lưu trữ khung giáo án chuẩn theo chương trình, cấp độ và số thứ tự buổi học. Template này được tạo một lần và có thể tái sử dụng cho nhiều lớp học cùng chương trình.

**Các attribute quan trọng**:
- `program_id`: Chương trình học (ví dụ: "Tiếng Anh Level 1")
- `level`: Cấp độ (ví dụ: "Beginner", "Intermediate")
- `session_index`: Số thứ tự buổi học trong chương trình (ví dụ: buổi 1, 2, 3...)
- `structure_json`: Cấu trúc giáo án dạng JSON, chứa:
  - Mục tiêu học tập
  - Nội dung bài học (vocabulary, grammar, activities)
  - Thời gian phân bổ cho từng phần
  - Tài liệu cần thiết
  - Bài tập về nhà dự kiến
- `is_active` / `is_deleted`: Quản lý template (soft delete)

**Ví dụ structure_json**:
```json
{
  "objectives": ["Học từ vựng về gia đình", "Luyện phát âm"],
  "activities": [
    {"name": "Warm-up", "duration": 10, "description": "Game giới thiệu"},
    {"name": "Vocabulary", "duration": 20, "words": ["father", "mother", "sister"]},
    {"name": "Practice", "duration": 30, "type": "pair-work"}
  ],
  "homework_planned": "Làm bài tập trang 12-15"
}
```

**Cách sử dụng**: Admin/Giáo viên tạo template một lần, sau đó giáo viên có thể tham khảo khi tạo giáo án thực tế cho từng buổi học.

### 5.2. `lesson_plans` - Giáo án buổi học

**Mục đích**: Lưu trữ giáo án thực tế cho từng buổi học cụ thể. Mỗi buổi học có một giáo án, giáo viên có thể tham khảo template nhưng điều chỉnh theo tình hình thực tế.

**Các attribute quan trọng**:
- `session_id`: Buổi học cụ thể (quan hệ nghiệp vụ: 1:1 - mỗi buổi học chỉ có một giáo án; nhưng trong DB hiện tại chưa có unique constraint nên về mặt kỹ thuật là 1:N)
- `template_id`: Template tham khảo (nullable - có thể không dùng template)
- `planned_content` (jsonb): Nội dung dự kiến trước buổi học (copy từ template hoặc tự soạn)
- `actual_content` (jsonb): Nội dung thực tế đã dạy (điều chỉnh sau buổi học)
- `actual_homework`: Bài tập về nhà thực tế đã giao (có thể khác với planned)
- `teacher_notes`: Ghi chú của giáo viên (học sinh nào cần chú ý, phần nào khó, v.v.)
- `submitted_by`: Giáo viên nộp giáo án
- `submitted_at`: Thời gian nộp (thường sau buổi học)

**Luồng hoạt động**:
1. **Trước buổi học**: Giáo viên tạo `lesson_plan` với `planned_content` (có thể copy từ template)
2. **Trong buổi học**: Giáo viên điều chỉnh theo tình hình thực tế
3. **Sau buổi học**: Giáo viên cập nhật `actual_content`, `actual_homework`, `teacher_notes` và submit

**Ví dụ**:
```
Buổi học 15/03 - Lớp Tiếng Anh Level 1, Buổi 5

Trước buổi học:
- planned_content: Copy từ template session_index=5
- actual_content: NULL

Sau buổi học:
- planned_content: Giữ nguyên (để so sánh)
- actual_content: {"activities": [...], "notes": "Học sinh A cần luyện thêm phát âm"}
- actual_homework: "Làm bài tập trang 12-15, thêm bài viết về gia đình"
- teacher_notes: "Buổi học tốt, nhưng cần chú ý học sinh B"
```

**Lợi ích của planned vs actual**:
- So sánh kế hoạch và thực tế để cải thiện giảng dạy
- Theo dõi tiến độ chương trình
- Đánh giá hiệu quả giáo án

### 5.3. `homework_assignments` - Bài tập về nhà

**Mục đích**: Lưu trữ bài tập được giao cho lớp học hoặc buổi học cụ thể. Một bài tập có thể gán cho cả lớp hoặc chỉ một buổi học.

**Các attribute quan trọng**:
- `class_id`: Lớp học (bắt buộc)
- `session_id`: Buổi học cụ thể (nullable - null nếu bài tập chung cho cả lớp)
- `title`: Tiêu đề bài tập
- `description`: Mô tả chi tiết yêu cầu
- `due_at`: Hạn nộp bài
- `book`: Sách (ví dụ: "Family and Friends 1")
- `pages`: Trang (ví dụ: "12-15")
- `skills`: Kỹ năng luyện tập (ví dụ: "Reading, Writing, Grammar")
- `submission_type`: Loại nộp bài:
  - `FILE`: Nộp file (PDF, Word, v.v.)
  - `IMAGE`: Nộp ảnh (chụp bài làm)
  - `TEXT`: Nhập text trực tiếp
  - `LINK`: Nộp link (Google Docs, v.v.)
  - `QUIZ`: Làm quiz trực tuyến
- `max_score`: Điểm tối đa
- `reward_stars`: Số sao thưởng khi hoàn thành (gamification)
- `mission_id`: Liên kết với mission (nếu bài tập là một phần của nhiệm vụ)

**Luồng tạo bài tập**:
1. Giáo viên tạo `homework_assignment` cho lớp/buổi học
2. Hệ thống tự động tạo `homework_student` cho tất cả học sinh trong lớp (status = ASSIGNED)
3. Học sinh nhận thông báo có bài tập mới

**Ví dụ**:
```
Bài tập: "Viết đoạn văn về gia đình"
- class_id: Lớp Tiếng Anh Level 1
- session_id: NULL (bài tập chung, không gắn buổi cụ thể)
- book: "Family and Friends 1"
- pages: "12-15"
- skills: "Writing"
- submission_type: TEXT
- due_at: 2024-03-20 23:59
- reward_stars: 5
```

### 5.4. `homework_student` - Bài tập của học sinh

**Mục đích**: Lưu trữ trạng thái bài tập của từng học sinh. Mỗi học sinh có một record riêng cho mỗi bài tập.

**Các attribute quan trọng**:
- `assignment_id`: Bài tập được giao
- `student_profile_id`: Học sinh
- `status`: Trạng thái:
  - `ASSIGNED`: Đã được giao, chưa làm
  - `SUBMITTED`: Đã nộp bài
  - `GRADED`: Đã được chấm điểm
  - `LATE`: Nộp muộn
  - `MISSING`: Không nộp (sau hạn)
- `submitted_at`: Thời gian nộp bài
- `graded_at`: Thời gian chấm điểm
- `score`: Điểm số (từ 0 đến max_score)
- `teacher_feedback`: Nhận xét của giáo viên
- `ai_feedback` (jsonb): Feedback tự động từ AI (nếu có)
- `attachments` (jsonb): Danh sách file đính kèm (JSON array)

**Unique constraint**: Mỗi học sinh chỉ có 1 record cho mỗi bài tập `(assignment_id, student_profile_id)`.

**Luồng hoạt động**:

1. **Giao bài tập**:
   ```
   Giáo viên tạo homework_assignment
   → Hệ thống tự động tạo homework_student cho tất cả học sinh:
     - status: ASSIGNED
     - submitted_at: NULL
     - score: NULL
   ```

2. **Học sinh nộp bài**:
   ```
   Học sinh upload file/nhập text/nộp link
   → Cập nhật homework_student:
     - status: SUBMITTED (hoặc LATE nếu quá hạn)
     - submitted_at: Thời gian nộp
     - attachments: [{"url": "...", "type": "file"}]
   ```

3. **Giáo viên chấm điểm**:
   ```
   Giáo viên xem bài, chấm điểm, viết feedback
   → Cập nhật homework_student:
     - status: GRADED
     - graded_at: Thời gian chấm
     - score: 8.5 (ví dụ)
     - teacher_feedback: "Bài viết tốt, nhưng cần chú ý ngữ pháp"
   ```

4. **AI Feedback (tùy chọn)**:
   ```
   Hệ thống có thể tự động phân tích bài làm và tạo feedback
   → Cập nhật ai_feedback:
     {
       "strengths": ["Từ vựng phong phú"],
       "improvements": ["Cần luyện thêm ngữ pháp"],
       "suggestions": ["Thử dùng câu phức hơn"]
     }
   ```

**Ví dụ hoàn chỉnh**:
```
Bài tập: "Viết đoạn văn về gia đình"
Học sinh: Nguyễn Văn A

1. Giao bài (15/03):
   homework_student:
   - status: ASSIGNED
   - submitted_at: NULL

2. Nộp bài (18/03, trước hạn):
   homework_student:
   - status: SUBMITTED
   - submitted_at: 2024-03-18 20:30
   - attachments: [{"url": "https://...", "type": "text"}]

3. Chấm điểm (19/03):
   homework_student:
   - status: GRADED
   - graded_at: 2024-03-19 10:00
   - score: 8.5
   - teacher_feedback: "Bài viết tốt, từ vựng phong phú. Cần chú ý thì quá khứ."
   - ai_feedback: {"grammar_score": 7, "vocabulary_score": 9}
```

### 5.5. Quan hệ giữa các bảng

**Luồng hoàn chỉnh**:

```
1. lesson_plan_templates (Template giáo án)
   ↓ (Giáo viên tham khảo khi tạo giáo án)
   
2. lesson_plans (Giáo án buổi học)
   ↓ (Giáo viên giao bài tập sau buổi học)
   
3. homework_assignments (Bài tập được giao)
   ↓ (Hệ thống tự động tạo cho từng học sinh)
   
4. homework_student (Trạng thái bài tập của học sinh)
   ↓ (Học sinh nộp bài)
   
5. homework_student (Cập nhật: SUBMITTED → GRADED)
```

**Quan hệ chi tiết**:

- `programs` → `lesson_plan_templates` (1:N): Một chương trình có nhiều template (theo session_index)
- `sessions` → `lesson_plans` (1:1): Mỗi buổi học có một giáo án
- `lesson_plan_templates` → `lesson_plans` (1:N): Một template có thể dùng cho nhiều buổi học
- `classes` → `homework_assignments` (1:N): Một lớp có nhiều bài tập
- `sessions` → `homework_assignments` (1:N): Một buổi học có thể có nhiều bài tập
- `homework_assignments` → `homework_student` (1:N): Một bài tập có nhiều record (mỗi học sinh một record)
- `profiles` → `homework_student` (1:N): Một học sinh có nhiều bài tập



## 6. Khối Kiểm tra định kỳ
- `exams`: kỳ kiểm tra (progress/mid/final/speaking...).
- `exam_results`: điểm & nhận xét per student; hỗ trợ đính kèm scan.



## 7. Khối Báo cáo tháng + AI

Khối này quản lý việc tạo báo cáo tháng tự động bằng AI cho từng học sinh. Luồng hoạt động: **Job batch** → **AI tạo draft** → **Review & chỉnh sửa** → **Approve** → **Publish**.

### 7.1. `monthly_report_jobs` - Job batch tạo báo cáo

**Mục đích**: Quản lý các job batch tự động tạo báo cáo tháng cho tất cả học sinh trong một chi nhánh. Job này chạy định kỳ (ví dụ: cuối mỗi tháng) để sinh draft báo cáo từ AI.

**Các attribute quan trọng**:
- `month` / `year`: Tháng và năm của báo cáo (ví dụ: tháng 3/2024)
- `branch_id`: Chi nhánh (job chạy theo từng branch)
- `status`: Trạng thái job:
  - `PENDING`: **Đang chờ chạy** - Job đã được tạo nhưng chưa được background worker nhận và xử lý. Dùng để:
    - Quản lý queue: Nhiều job có thể ở trạng thái PENDING, worker sẽ lấy từng job một để xử lý
    - Scheduling: Có thể tạo job trước thời điểm cần chạy (ví dụ: tạo job vào ngày 30, chạy vào ngày 1 tháng sau)
    - Retry logic: Nếu job FAILED, có thể reset về PENDING để retry
    - Monitoring: Admin có thể xem có bao nhiêu job đang chờ xử lý
  - `GENERATING`: Đang chạy (AI đang tạo báo cáo) - Worker đã nhận job và đang xử lý
  - `DONE`: Hoàn thành (đã tạo xong tất cả báo cáo) - Job đã hoàn thành thành công
  - `FAILED`: Thất bại (có lỗi xảy ra) - Job gặp lỗi, có thể retry bằng cách reset về PENDING
- `started_at` / `finished_at`: Thời gian bắt đầu/kết thúc job
- `ai_payload_ref`: Reference đến payload gửi cho AI (để debug hoặc retry nếu lỗi)

**Luồng hoạt động**:
1. **Khởi tạo job**: Hệ thống tạo `monthly_report_jobs` với `status = PENDING` (ví dụ: cuối tháng 3, tạo job cho tháng 3/2024)
   - Job được đưa vào queue, chờ background worker nhận
   - `started_at` và `finished_at` vẫn là NULL
   
2. **Worker nhận job**: Background worker (cron job/queue worker) lấy job có `status = PENDING`
   - Cập nhật `status = GENERATING`
   - Cập nhật `started_at = now()`
   - Đảm bảo chỉ có 1 worker xử lý job này (tránh duplicate processing)
3. **Thu thập dữ liệu**: Hệ thống thu thập dữ liệu cho từng học sinh:
   - Điểm danh (attendances)
   - Bài tập (homework_student)
   - Kết quả kiểm tra (exam_results)
   - Tiến độ nhiệm vụ (mission_progress)
   - Giao dịch sao (star_transactions)
4. **Gọi AI**: Gửi dữ liệu cho AI service để tạo draft báo cáo
5. **Tạo báo cáo**: Với mỗi học sinh, tạo `student_monthly_reports` với `draft_content` từ AI
6. **Hoàn thành**: Cập nhật `status = DONE`, `finished_at = now()`

**Ví dụ chi tiết về luồng PENDING**:
```
Ngày 31/03/2024, 23:00 - Cron job tự động tạo job:
→ Tạo monthly_report_jobs:
  - month: 3
  - year: 2024
  - branch_id: Chi nhánh Hà Nội
  - status: PENDING  ← Job đang chờ trong queue
  - started_at: NULL
  - finished_at: NULL

Ngày 01/04/2024, 00:00 - Background worker nhận job:
→ Worker query: SELECT * FROM monthly_report_jobs WHERE status = 'PENDING' LIMIT 1
→ Worker cập nhật:
  - status: GENERATING  ← Đang xử lý
  - started_at: 2024-04-01 00:00:00

Ngày 01/04/2024, 00:05 - Worker đang xử lý:
→ Thu thập dữ liệu 50 học sinh
→ Gọi AI tạo 50 báo cáo draft
→ Tạo 50 student_monthly_reports với draft_content

Ngày 01/04/2024, 00:15 - Hoàn thành:
→ status: DONE
→ finished_at: 2024-04-01 00:15:00
```

**Tại sao cần PENDING?**:

1. **Queue Management**: Nhiều job có thể được tạo cùng lúc, nhưng worker chỉ xử lý từng job một. PENDING giúp quản lý queue:
   ```
   Queue:
   - Job tháng 3 (PENDING) ← Worker sẽ lấy job này
   - Job tháng 2 (PENDING) ← Chờ đến lượt
   - Job tháng 1 (PENDING) ← Chờ đến lượt
   ```

2. **Tránh duplicate processing**: Khi worker nhận job, ngay lập tức chuyển sang GENERATING, đảm bảo không có worker khác xử lý cùng job:
   ```
   Worker 1: SELECT job WHERE status='PENDING' → Job A
   Worker 1: UPDATE job SET status='GENERATING' → Job A đã được lock
   Worker 2: SELECT job WHERE status='PENDING' → Job B (không lấy Job A nữa)
   ```

3. **Scheduling**: Có thể tạo job trước, chạy sau:
   ```
   Ngày 30/03: Tạo job với status=PENDING (chưa chạy)
   Ngày 01/04: Worker mới nhận và chạy job
   ```

4. **Retry logic**: Nếu job FAILED, có thể reset về PENDING để retry:
   ```
   Job FAILED → Admin reset status=PENDING → Worker retry lại
   ```

5. **Monitoring**: Admin có thể monitor:
   ```
   SELECT COUNT(*) FROM monthly_report_jobs WHERE status='PENDING'
   → Có bao nhiêu job đang chờ xử lý?
   ```

### 7.2. `student_monthly_reports` - Báo cáo tháng của học sinh

**Mục đích**: Lưu trữ báo cáo tháng cho từng học sinh, bao gồm draft từ AI và nội dung cuối cùng sau khi review/chỉnh sửa.

**Các attribute quan trọng**:
- `student_profile_id`: Học sinh
- `month` / `year`: Tháng và năm của báo cáo
- `draft_content` (jsonb): **Nội dung draft từ AI** - Báo cáo tự động được AI tạo ra, chưa được review
- `final_content` (jsonb): **Nội dung cuối cùng** - Báo cáo sau khi được giáo viên/staff review và chỉnh sửa
- `status`: Trạng thái báo cáo:
  - `DRAFT`: Draft từ AI, chưa được review
  - `REVIEW`: Đang được review/chỉnh sửa
  - `APPROVED`: Đã được duyệt, sẵn sàng publish
  - `REJECTED`: Bị từ chối (cần tạo lại hoặc chỉnh sửa nhiều)
- `ai_version`: Phiên bản AI model đã dùng (ví dụ: "gpt-4", "claude-3") - để track và so sánh chất lượng
- `submitted_by`: Giáo viên/staff submit báo cáo để review
- `reviewed_by`: Người review (teacher/staff/admin)
- `reviewed_at`: Thời gian review
- `published_at`: Thời gian publish (khi gửi cho phụ huynh)

**Cấu trúc nội dung báo cáo (draft_content/final_content)**:
```json
{
  "summary": "Tổng quan tháng học tập",
  "attendance": {
    "total_sessions": 12,
    "present": 11,
    "absent": 1,
    "makeup": 1
  },
  "homework": {
    "assigned": 8,
    "completed": 7,
    "average_score": 8.5
  },
  "exams": [
    {"type": "Progress Test", "score": 8.5, "comment": "Tốt"}
  ],
  "achievements": [
    "Hoàn thành 5 nhiệm vụ",
    "Nhận 50 sao"
  ],
  "strengths": ["Từ vựng tốt", "Chăm chỉ làm bài tập"],
  "improvements": ["Cần luyện thêm phát âm"],
  "recommendations": ["Luyện đọc thêm sách tiếng Anh"]
}
```

**Luồng hoạt động**:

1. **Tạo draft từ AI** (tự động):
   ```
   monthly_report_jobs chạy
   → AI tạo draft_content
   → Tạo student_monthly_reports:
     - draft_content: {...} (từ AI)
     - final_content: NULL
     - status: DRAFT
   ```

2. **Review & chỉnh sửa** (giáo viên/staff):
   ```
   Giáo viên xem draft_content
   → Chỉnh sửa nội dung
   → Cập nhật final_content
   → status: REVIEW
   → submitted_by: giáo viên
   ```

3. **Approve** (staff/admin):
   ```
   Staff/Admin review final_content
   → Nếu OK: status: APPROVED
   → reviewed_by: staff/admin
   → reviewed_at: now()
   → Nếu không OK: status: REJECTED (cần chỉnh sửa lại)
   ```

4. **Publish** (gửi cho phụ huynh):
   ```
   Hệ thống publish báo cáo đã APPROVED
   → published_at: now()
   → Gửi notification cho phụ huynh
   → Phụ huynh xem báo cáo qua app/web
   ```

**Ví dụ hoàn chỉnh**:
```
Học sinh: Nguyễn Văn A
Tháng: 3/2024

1. AI tạo draft (01/04/2024):
   - draft_content: {"summary": "...", "attendance": {...}}
   - final_content: NULL
   - status: DRAFT
   - ai_version: "gpt-4"

2. Giáo viên edit (02/04/2024):
   - draft_content: Giữ nguyên (để so sánh)
   - final_content: {"summary": "...", "attendance": {...}, "note": "Học sinh tiến bộ tốt"}
   - status: REVIEW (chờ review)
   - submitted_by: Cô Lan (giáo viên)

3. Staff approve (03/04/2024):
   - status: APPROVED
   - reviewed_by: Anh Nam (staff)
   - reviewed_at: 2024-04-03 10:00

4. Publish (03/04/2024):
   - published_at: 2024-04-03 14:00
   - Gửi notification cho phụ huynh
   - Phụ huynh xem báo cáo trên app
```

**Lưu ý về `draft_content` vs `final_content`**:
- `draft_content`: Nội dung từ AI, không thay đổi sau khi tạo (để so sánh và đánh giá chất lượng AI)
- `final_content`: Nội dung sau khi chỉnh sửa, có thể khác hoàn toàn với draft
- Cả hai đều lưu để:
  - So sánh chất lượng AI (draft vs final)
  - Audit trail (biết được AI tạo gì, giáo viên chỉnh gì)
  - Cải thiện prompt AI dựa trên sự khác biệt

### 7.3. `report_comments` - Comment review báo cáo

**Mục đích**: Lưu trữ các comment trao đổi giữa giáo viên, staff và admin trong quá trình review báo cáo. Tạo thành một thread thảo luận.

**Các attribute quan trọng**:
- `report_id`: Báo cáo được comment
- `commenter_id`: Người comment (teacher/staff/admin)
- `content`: Nội dung comment
- `created_at`: Thời gian comment

**Luồng sử dụng**:
1. **Giáo viên comment**: Giáo viên xem draft, comment về điểm cần chỉnh sửa
2. **Staff comment**: Staff review và comment về nội dung
3. **Admin comment**: Admin có thể comment về chất lượng hoặc yêu cầu chỉnh sửa
4. **Thread trao đổi**: Tất cả comment tạo thành một thread, giúp theo dõi quá trình review

**Ví dụ**:
```
Báo cáo tháng 3/2024 - Học sinh A

Comment 1 (02/04, Cô Lan - Giáo viên):
"Báo cáo tốt, nhưng cần thêm phần về điểm mạnh trong phát âm"

Comment 2 (02/04, Anh Nam - Staff):
"Đã chỉnh sửa theo gợi ý của cô Lan. Vui lòng review lại."

Comment 3 (03/04, Chị Hoa - Admin):
"Báo cáo đã OK, có thể approve và publish."
```

### 7.4. Quan hệ giữa các bảng

**Luồng hoàn chỉnh**:

```
1. monthly_report_jobs (Job batch tạo báo cáo)
   ↓ (Chạy định kỳ, ví dụ: cuối mỗi tháng)
   
2. Thu thập dữ liệu học sinh
   ↓ (attendances, homework, exams, missions...)
   
3. Gọi AI service
   ↓ (Tạo draft_content)
   
4. student_monthly_reports (Tạo báo cáo draft)
   ↓ (Giáo viên/staff review)
   
5. report_comments (Comment trao đổi)
   ↓ (Chỉnh sửa final_content)
   
6. student_monthly_reports (Approve)
   ↓ (Publish)
   
7. Gửi notification cho phụ huynh
```

**Quan hệ chi tiết**:
- `branches` → `monthly_report_jobs` (1:N): Một branch có nhiều job (theo tháng)
- `monthly_report_jobs` → `student_monthly_reports` (1:N): Một job tạo nhiều báo cáo (mỗi học sinh một báo cáo)
- `profiles` → `student_monthly_reports` (1:N): Một học sinh có nhiều báo cáo tháng
- `student_monthly_reports` → `report_comments` (1:N): Một báo cáo có nhiều comment
- `profiles` → `report_comments` (1:N): Một người có thể comment nhiều báo cáo

**Lưu ý quan trọng**:
- `monthly_report_jobs` chạy theo batch - không tạo báo cáo thủ công từng học sinh
- `draft_content` không thay đổi sau khi tạo - chỉ `final_content` được chỉnh sửa
- `ai_version` giúp track chất lượng AI - có thể so sánh giữa các phiên bản
- `report_comments` tạo thread trao đổi - giúp collaboration giữa teacher/staff/admin
- Báo cáo chỉ publish khi `status = APPROVED` và `published_at` được set
- Mỗi học sinh chỉ có 1 báo cáo cho mỗi tháng (có thể thêm unique constraint trên `student_profile_id, month, year`)

## 8. Khối Gamification

Khối này quản lý hệ thống gamification để khuyến khích học sinh học tập thông qua nhiệm vụ, sao thưởng, level và đổi quà. Luồng hoạt động: **Tạo nhiệm vụ** → **Học sinh hoàn thành** → **Nhận sao** → **Tăng level** → **Đổi quà**.

### 8.1. `missions` - Nhiệm vụ

**Mục đích**: Lưu trữ các nhiệm vụ/challenge được tạo để khuyến khích học sinh học tập. Nhiệm vụ có thể áp dụng cho cả lớp, nhóm học sinh hoặc từng học sinh cụ thể.

**Các attribute quan trọng**:
- `title`: Tiêu đề nhiệm vụ (ví dụ: "Làm bài tập liên tục 7 ngày")
- `description`: Mô tả chi tiết yêu cầu
- `scope`: Phạm vi áp dụng:
  - `CLASS`: Áp dụng cho cả lớp (tất cả học sinh trong lớp)
  - `STUDENT`: Áp dụng cho một học sinh cụ thể
  - `GROUP`: Áp dụng cho nhóm học sinh cụ thể (định nghĩa trong `target_group`)
- `target_class_id`: Lớp mục tiêu (nếu `scope = CLASS`)
- `target_group` (jsonb): Nhóm học sinh cụ thể (nếu `scope = GROUP`), ví dụ:
  ```json
  {
    "student_ids": ["uuid1", "uuid2", "uuid3"],
    "group_name": "Nhóm học sinh tích cực"
  }
  ```
- `mission_type`: Loại nhiệm vụ:
  - `HOMEWORK_STREAK`: Làm bài tập liên tục X ngày
  - `READING_STREAK`: Đọc sách liên tục X ngày
  - `NO_UNEXCUSED_ABSENCE`: Không vắng không phép trong tháng
  - `CUSTOM`: Nhiệm vụ tùy chỉnh (giáo viên/staff tự định nghĩa)
- `start_at` / `end_at`: Thời gian bắt đầu và kết thúc nhiệm vụ
- `reward_stars`: Số sao thưởng khi hoàn thành nhiệm vụ
- `created_by`: Người tạo nhiệm vụ (teacher/staff/admin)

**Ví dụ**:
```
Mission: "Làm bài tập liên tục 7 ngày"
- scope: CLASS
- target_class_id: Lớp Tiếng Anh Level 1
- mission_type: HOMEWORK_STREAK
- start_at: 2024-03-01 00:00
- end_at: 2024-03-31 23:59
- reward_stars: 50
→ Tất cả học sinh trong lớp sẽ được gán nhiệm vụ này
```

### 8.2. `mission_progress` - Tiến độ nhiệm vụ

**Mục đích**: Lưu trữ tiến độ của từng học sinh đối với mỗi nhiệm vụ. Mỗi học sinh có một record riêng cho mỗi nhiệm vụ.

**Các attribute quan trọng**:
- `mission_id`: Nhiệm vụ
- `student_profile_id`: Học sinh
- `status`: Trạng thái:
  - `ASSIGNED`: Đã được gán, chưa bắt đầu
  - `IN_PROGRESS`: Đang thực hiện
  - `COMPLETED`: Đã hoàn thành
  - `EXPIRED`: Hết hạn (quá `end_at` mà chưa hoàn thành)
- `progress_value`: Giá trị tiến độ (tùy loại nhiệm vụ):
  - `HOMEWORK_STREAK`: Số ngày streak hiện tại (ví dụ: 5/7)
  - `READING_STREAK`: Số ngày đọc liên tục
  - `NO_UNEXCUSED_ABSENCE`: Số ngày không vắng (0 = đã vắng)
- `completed_at`: Thời gian hoàn thành (null nếu chưa hoàn thành)
- `verified_by`: Người xác nhận hoàn thành (teacher/staff, nullable)

**Unique constraint**: Mỗi học sinh chỉ có 1 record cho mỗi nhiệm vụ `(mission_id, student_profile_id)`.

**Luồng hoạt động**:

1. **Gán nhiệm vụ** (tự động khi tạo mission):
   ```
   Tạo mission với scope=CLASS
   → Hệ thống tự động tạo mission_progress cho tất cả học sinh trong lớp:
     - status: ASSIGNED
     - progress_value: 0
   ```

2. **Cập nhật tiến độ** (tự động hoặc thủ công):
   ```
   Học sinh làm bài tập ngày 1
   → Hệ thống cập nhật:
     - status: IN_PROGRESS
     - progress_value: 1
   
   Học sinh làm bài tập ngày 2
   → progress_value: 2
   
   ...
   
   Học sinh làm bài tập ngày 7
   → progress_value: 7
   → status: COMPLETED
   → completed_at: now()
   ```

3. **Xác nhận và thưởng sao**:
   ```
   Khi status = COMPLETED:
   → verified_by: Giáo viên xác nhận (nếu cần)
   → Tạo star_transaction với amount = mission.reward_stars
   → Cập nhật student_levels (tăng XP)
   ```

**Ví dụ hoàn chỉnh**:
```
Mission: "Làm bài tập liên tục 7 ngày" (reward: 50 sao)
Học sinh: Nguyễn Văn A

Ngày 1 (01/03): Làm bài tập
→ mission_progress:
  - status: IN_PROGRESS
  - progress_value: 1

Ngày 2-6: Tiếp tục làm bài tập
→ progress_value: 2, 3, 4, 5, 6

Ngày 7 (07/03): Làm bài tập
→ status: COMPLETED
→ progress_value: 7
→ completed_at: 2024-03-07 20:00
→ verified_by: Cô Lan (giáo viên)
→ Tạo star_transaction: +50 sao
```

### 8.3. `star_transactions` - Giao dịch sao (Ledger)

**Mục đích**: Lưu trữ tất cả giao dịch sao của học sinh dưới dạng ledger (sổ cái). Mỗi giao dịch ghi nhận số sao tăng/giảm, lý do và số dư sau giao dịch để audit.

**Các attribute quan trọng**:
- `student_profile_id`: Học sinh
- `amount`: Số lượng sao (+ hoặc -)
  - Số dương: Nhận sao (hoàn thành nhiệm vụ, làm bài tập, v.v.)
  - Số âm: Mất sao (đổi quà, điều chỉnh, v.v.)
- `reason`: Lý do giao dịch (ví dụ: "Hoàn thành nhiệm vụ làm bài tập 7 ngày")
- `source_type`: Nguồn giao dịch:
  - `MISSION`: Từ nhiệm vụ (hoàn thành mission)
  - `MANUAL`: Thủ công (giáo viên/staff thưởng/phạt)
  - `HOMEWORK`: Từ bài tập (hoàn thành bài tập có reward_stars)
  - `TEST`: Từ kiểm tra (điểm cao được thưởng)
  - `ADJUSTMENT`: Điều chỉnh (sửa lỗi, bù trừ)
- `source_id`: ID của nguồn (mission_id, homework_id, exam_id, v.v.)
- `balance_after`: **Số dư sau giao dịch** - Quan trọng để audit và tính toán số dư hiện tại
- `created_by`: Người tạo giao dịch (hệ thống/giáo viên/staff)

**Tại sao cần `balance_after`?**:
- **Audit trail**: Biết được số dư tại mỗi thời điểm, không cần tính toán lại
- **Tính toán nhanh**: Có thể lấy số dư hiện tại từ giao dịch mới nhất
- **Debug**: Dễ dàng phát hiện lỗi tính toán nếu có
- **Lịch sử**: Xem được số dư tại bất kỳ thời điểm nào

**Luồng hoạt động**:

1. **Nhận sao** (tự động):
   ```
   Học sinh hoàn thành mission (50 sao)
   → Tạo star_transaction:
     - amount: +50
     - reason: "Hoàn thành nhiệm vụ làm bài tập 7 ngày"
     - source_type: MISSION
     - source_id: mission_id
     - balance_after: 150 (số dư cũ 100 + 50)
   ```

2. **Mất sao** (đổi quà):
   ```
   Học sinh đổi quà (100 sao)
   → Tạo star_transaction:
     - amount: -100
     - reason: "Đổi quà: Bút chì"
     - source_type: ADJUSTMENT (hoặc tạo source_type mới: REDEMPTION)
     - source_id: reward_redemption_id
     - balance_after: 50 (số dư cũ 150 - 100)
   ```

3. **Thưởng thủ công**:
   ```
   Giáo viên thưởng học sinh (20 sao)
   → Tạo star_transaction:
     - amount: +20
     - reason: "Thưởng tích cực trong lớp"
     - source_type: MANUAL
     - created_by: Giáo viên
     - balance_after: 70 (số dư cũ 50 + 20)
   ```

**Ví dụ ledger**:
```
Học sinh: Nguyễn Văn A

Giao dịch 1 (01/03):
- amount: +50 (Hoàn thành mission)
- balance_after: 50

Giao dịch 2 (05/03):
- amount: +10 (Hoàn thành bài tập)
- balance_after: 60

Giao dịch 3 (10/03):
- amount: -30 (Đổi quà)
- balance_after: 30

Giao dịch 4 (15/03):
- amount: +20 (Thưởng thủ công)
- balance_after: 50

→ Số dư hiện tại: 50 sao (từ giao dịch cuối cùng)
```

### 8.4. `student_levels` - Level học sinh

**Mục đích**: Lưu trữ level và XP hiện tại của học sinh. Level được tính dựa trên tổng số sao hoặc XP tích lũy.

**Các attribute quan trọng**:
- `student_profile_id`: Học sinh (unique - 1:1)
- `current_level`: Level hiện tại (ví dụ: "Bronze", "Silver", "Gold", "Platinum")
- `current_xp`: XP hiện tại (có thể tính từ tổng sao hoặc riêng)
- `updated_at`: Thời gian cập nhật lần cuối

**Cách tính level** (tùy business logic):
```
Bronze: 0-100 sao
Silver: 101-300 sao
Gold: 301-600 sao
Platinum: 601+ sao
```

**Luồng cập nhật**:
```
Khi có star_transaction mới:
→ Tính tổng sao hiện tại: SUM(amount) hoặc balance_after từ giao dịch cuối
→ Tính level mới dựa trên tổng sao
→ Cập nhật student_levels:
  - current_level: "Silver" (nếu tổng sao = 150)
  - current_xp: 150 (hoặc tính riêng)
  - updated_at: now()
```

**Ví dụ**:
```
Học sinh: Nguyễn Văn A
Tổng sao: 150
→ current_level: "Silver"
→ current_xp: 150
```

### 8.5. `reward_store_items` - Item cửa hàng

**Mục đích**: Lưu trữ các quà có thể đổi bằng sao trong cửa hàng.

**Các attribute quan trọng**:
- `title`: Tên quà (ví dụ: "Bút chì", "Sticker", "Sách truyện")
- `description`: Mô tả quà
- `cost_stars`: Giá (số sao cần để đổi)
- `quantity`: Số lượng còn lại (giảm khi có người đổi)
- `is_active`: Trạng thái hoạt động (có thể đổi không)
- `is_deleted`: Đánh dấu xóa mềm (ẩn khỏi cửa hàng)

**Ví dụ**:
```
Item: "Bút chì màu"
- cost_stars: 30
- quantity: 50
- is_active: true
```

### 8.6. `reward_redemptions` - Đổi quà

**Mục đích**: Lưu trữ các yêu cầu đổi quà của học sinh và trạng thái xử lý.

**Các attribute quan trọng**:
- `item_id`: Item được đổi
- `student_profile_id`: Học sinh đổi
- `status`: Trạng thái:
  - `REQUESTED`: Học sinh đã yêu cầu đổi
  - `APPROVED`: Staff đã duyệt, chuẩn bị trao quà
  - `DELIVERED`: Staff đã trao quà cho học sinh (chờ học sinh xác nhận)
  - `RECEIVED`: Học sinh đã xác nhận nhận quà (hoặc auto sau 3 ngày từ DELIVERED)
  - `CANCELLED`: Hủy yêu cầu (hết hàng, không hợp lệ, v.v.)
- `handled_by`: Staff xử lý yêu cầu (duyệt)
- `handled_at`: Thời gian duyệt (APPROVED)
- `delivered_at`: Thời gian staff trao quà (DELIVERED) - dùng để tính 3 ngày auto confirm
- `received_at`: Thời gian học sinh xác nhận nhận quà hoặc auto confirm (RECEIVED)

**Luồng hoạt động**:

1. **Học sinh yêu cầu đổi quà**:
   ```
   Học sinh chọn item (cost: 30 sao)
   → Kiểm tra số dư: balance >= 30?
   → Tạo reward_redemption:
     - status: REQUESTED
     - Tạo star_transaction: -30 sao (trừ trước)
   ```

2. **Staff duyệt**:
   ```
   Staff xem yêu cầu
   → Kiểm tra còn hàng (quantity > 0)
   → Cập nhật:
     - status: APPROVED
     - handled_by: Staff
     - handled_at: now()
     - Giảm quantity của item: -1
   ```

3. **Staff trao quà**:
   ```
   Staff trao quà cho học sinh (gặp mặt hoặc gửi qua phụ huynh)
   → Cập nhật:
     - status: DELIVERED
     - delivered_at: now()
     - handled_by: Staff (có thể là staff khác với người duyệt)
   → Học sinh nhận thông báo: "Quà đã được trao, vui lòng xác nhận"
   ```

4. **Học sinh xác nhận nhận quà**:
   ```
   Học sinh bấm nút "Đã nhận quà" trên app
   → Cập nhật:
     - status: RECEIVED
     - received_at: now()
   → Hoàn tất quy trình đổi quà
   ```

5. **Auto xác nhận sau 3 ngày**:
   ```
   Background job chạy định kỳ (ví dụ: mỗi ngày)
   → Query: SELECT * FROM reward_redemptions 
            WHERE status = 'DELIVERED' 
            AND delivered_at < NOW() - INTERVAL '3 days'
   → Cập nhật tự động:
     - status: RECEIVED
     - received_at: now()
   → Gửi notification: "Quà của bạn đã được tự động xác nhận"
   ```

6. **Hủy** (nếu cần):
   ```
   Nếu hết hàng hoặc không hợp lệ (trước khi DELIVERED):
   → status: CANCELLED
   → Hoàn lại sao: Tạo star_transaction: +30
   → Hoàn lại quantity của item: +1
   ```

**Ví dụ hoàn chỉnh**:
```
Học sinh: Nguyễn Văn A (số dư: 50 sao)

1. Yêu cầu đổi "Bút chì màu" (30 sao) - 01/03:
   → Tạo reward_redemption: status = REQUESTED
   → Tạo star_transaction: -30, balance_after = 20

2. Staff duyệt (02/03):
   → status: APPROVED
   → handled_by: Anh Nam
   → handled_at: 2024-03-02 10:00
   → quantity của item: 50 → 49

3. Staff trao quà (05/03):
   → status: DELIVERED
   → delivered_at: 2024-03-05 14:00
   → handled_by: Chị Hoa (staff trao quà)
   → Học sinh nhận notification: "Quà đã được trao, vui lòng xác nhận"

4a. Học sinh xác nhận (06/03):
   → status: RECEIVED
   → received_at: 2024-03-06 18:00
   → Hoàn tất

4b. Hoặc auto xác nhận sau 3 ngày (08/03):
   → Background job chạy
   → Kiểm tra: delivered_at (05/03) + 3 ngày = 08/03
   → status: RECEIVED (auto)
   → received_at: 2024-03-08 00:00
   → Gửi notification: "Quà đã được tự động xác nhận"
```

### 8.7. Quan hệ giữa các bảng

**Luồng hoàn chỉnh**:

```
1. missions (Tạo nhiệm vụ)
   ↓ (Tự động gán cho học sinh)
   
2. mission_progress (Tiến độ của từng học sinh)
   ↓ (Học sinh hoàn thành)
   
3. star_transactions (Nhận sao thưởng)
   ↓ (Tích lũy sao)
   
4. student_levels (Tăng level)
   ↓ (Dùng sao đổi quà)
   
5. reward_redemptions (Yêu cầu đổi quà)
   ↓ (Trừ sao)
   
6. star_transactions (Mất sao)
```

**Quan hệ chi tiết**:
- `classes` → `missions` (1:N): Một lớp có nhiều nhiệm vụ
- `missions` → `mission_progress` (1:N): Một nhiệm vụ có nhiều tiến độ (mỗi học sinh một record)
- `profiles` → `mission_progress` (1:N): Một học sinh có nhiều tiến độ nhiệm vụ
- `profiles` → `star_transactions` (1:N): Một học sinh có nhiều giao dịch sao
- `profiles` → `student_levels` (1:1): Mỗi học sinh có một level hiện tại
- `reward_store_items` → `reward_redemptions` (1:N): Một item có thể được đổi nhiều lần
- `profiles` → `reward_redemptions` (1:N): Một học sinh có thể đổi nhiều quà

**Lưu ý quan trọng**:
- `mission_progress` tự động tạo khi tạo mission với scope CLASS/STUDENT/GROUP
- `star_transactions` là ledger - mọi giao dịch sao đều được ghi lại
- `balance_after` trong `star_transactions` giúp tính số dư nhanh và audit
- `student_levels` được cập nhật khi có giao dịch sao mới
- `reward_redemptions` trừ sao ngay khi REQUESTED (không phải khi APPROVED) - để tránh đổi quá số lượng
- `reward_redemptions` có luồng mới: REQUESTED → APPROVED → DELIVERED → RECEIVED
- `DELIVERED`: Staff trao quà, học sinh cần xác nhận hoặc auto confirm sau 3 ngày
- Background job tự động chuyển `DELIVERED` → `RECEIVED` sau 3 ngày từ `delivered_at`
- Nếu `reward_redemptions` bị CANCELLED (trước DELIVERED), cần hoàn lại sao bằng cách tạo `star_transaction` dương


## 9. Khối CRM/Placement

### 9.1. Mục tiêu
- Thu thập và nuôi dưỡng lead từ landing/Zalo/referral/offline.
- Đặt lịch placement test, lưu kết quả và gợi ý level/program phù hợp.
- Chuyển đổi lead thành học sinh, liên kết với hồ sơ phụ huynh (parent profile).

### 9.2. Lead
- `leads`: lưu thông tin liên hệ (tên, phone, email, kênh, campaign, branch mong muốn).
- `source` (kênh gốc): LANDING (form web), ZALO (OA/chatbot), REFERRAL (giới thiệu), OFFLINE (walk-in/sự kiện); dùng để phân tích hiệu quả kênh.
- `contact_name`: người liên hệ chính (thường là phụ huynh).
- `phone` / `zalo_id` / `email`: kênh liên hệ; `zalo_id` dùng khi khác số điện thoại hoặc muốn nhắn OA.
- `branch_preference`: chi nhánh mong muốn học.
- `program_interest`: chương trình/level quan tâm ban đầu.
- `notes`: ghi chú tự do của sales.
- `owner_staff_id`: staff sales chăm sóc chính (CHANGED: từ owner_profile_id → owner_staff_id, reference users.id với role=STAFF); đổi owner ghi nhận vào history.
- Trạng thái đề xuất: `NEW` → `CONTACTED` → `BOOKED_TEST` → `TEST_DONE` → `ENROLLED` / `LOST`.
  - `NO_SHOW` dùng để đánh dấu buổi placement test mà lead/học sinh không tham dự. (vẫn có thể re-book).
  - `ENROLLED` lưu `converted_student_profile_id` để trace sang học sinh.
- SLA gợi ý: thời gian phản hồi đầu tiên (`first_response_at`) và số lần chạm (`touch_count`). Luồng: khi lần chạm đầu tiên diễn ra, set `first_response_at`; mỗi lần ghi `lead_activities` thì tăng `touch_count` + cập nhật SLA/report.
- Log tương tác: `lead_activities` (call/note/zalo/sms/email), kèm `next_action_at` để follow-up. Luồng: mỗi lần chăm sóc tạo một activity, optionally đặt `next_action_at`; màn hình worklist lấy các lead có `next_action_at` sắp tới hoặc đã quá hạn để nhắc follow-up.

### 9.3. Placement test
- `placement_tests`: lịch test và kết quả; gắn `lead_id` (nếu chưa là học sinh) hoặc `student_profile_id` (retake/đánh giá lại).
- Thuộc tính chính: thời gian test, `room` hoặc `meeting_link`, `invigilator_user_id` (CHANGED: từ invigilator_profile_id → invigilator_user_id, reference users.id với role=TEACHER/STAFF), `status` (SCHEDULED/NO_SHOW/COMPLETED/CANCELLED).
- Kết quả: điểm từng kỹ năng (listening/speaking/reading/writing), nhận xét giáo viên, level đề xuất, program đề xuất, file đính kèm (recording/scan).
- Sau khi COMPLETED, cập nhật lead sang `TEST_DONE`; nếu phụ huynh đồng ý học, chuyển `ENROLLED`.

### 9.4. Chuyển đổi lead → học sinh
- Khi chốt đăng ký:
  - Tạo `profiles` (STUDENT) và liên kết `parent_student_links` với phụ huynh tương ứng.
  - Ghi `converted_student_profile_id` và `converted_at` trong `leads`.
  - Đóng trạng thái lead (`ENROLLED`) nhưng vẫn giữ toàn bộ history để báo cáo hiệu quả kênh.

### 9.5. Báo cáo/đo lường
- : NEW → CONTACTED → BOOKED_TEST → TEST_DONE → ENROLLED/LOST (tính conversion rate theo branch/campaign/owner).
- Tỷ lệ no-show test, lead response time, số lần chạm trước khi chốt.
- Kênh hiệu quả: phân tích theo `source`/`campaign`/`branch`.

### 9.6. Luồng hoạt động (end-to-end)
```
1) Inbound lead
   - Tạo lead với source/campaign/branch_preference.
   - Gán owner (sales) và đặt next_action_at.

2) Liên hệ & chăm sóc
   - Ghi lead_activities (call/zalo/sms/email/note).
   - Cập nhật status: NEW → CONTACTED.

3) Đặt lịch placement test
   - Tạo placement_test (lead_id, scheduled_at, room/meeting_link, invigilator).
   - Cập nhật status lead: BOOKED_TEST.

4) Thực hiện test
   - Update placement_test.status: COMPLETED hoặc NO_SHOW.
   - Nhập điểm, nhận xét, level/program đề xuất, đính kèm file.
   - Nếu COMPLETED: status lead → TEST_DONE.
   - Nếu NO_SHOW: giữ BOOKED_TEST/NO_SHOW, có thể re-book (ghi thêm placement_test mới).

5) Chốt đăng ký
   - Nếu phụ huynh đồng ý: tạo profile STUDENT + parent_student_links (nếu chưa có).
   - Ghi converted_student_profile_id, converted_at; lead → ENROLLED.
   - Nếu không đăng ký: lead → LOST (giữ history để báo cáo kênh).

6) Báo cáo & SLA
   - Theo dõi first_response_at, touch_count, no_show_rate, conversion_rate theo source/campaign/branch/owner.
```

--lead_activities ghi log tất cả tương tác chăm sóc lead (call/Zalo/SMS/email/note) để:
Theo dõi lịch sử chăm sóc và người thực hiện.
Lên nhắc việc tiếp theo qua next_action_at.
Tính SLA và hiệu quả (số lần chạm, thời gian phản hồi).
Làm bằng chứng khi đổi owner hoặc bàn giao giữa sales.


## 10. Khối Media
- `media_assets`: ảnh/video, tag branch/class/student, tháng (YYYY-MM), visibility (class/personal/public_parent).


## 11. Khối Tài chính/PayOS/Sổ quỹ/Lương
- `tuition_plans`: bảng giá học phí theo program/branch; có `unit_price_session` để tính phát sinh EXTRA_PAID (học bù/thêm).
- `invoices`: hóa đơn (MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE); lưu trạng thái và link/QR PayOS để phụ huynh tự thanh toán.
- `invoice_lines`: dòng chi tiết (quantity/unit_price/description); `session_ids` dùng truy vết buổi học bị tính phí.
- `payments`: kết quả thanh toán (PAYOS/CASH/BANK_TRANSFER). Webhook PayOS map vào đây, set trạng thái invoice và tạo bút toán quỹ.
- `cashbook_entries`: sổ quỹ thu/chi; mỗi entry gắn nguồn INVOICE/PAYROLL/EXPENSE/ADJUSTMENT để đối soát và làm báo cáo quỹ.
- `contracts`: hợp đồng lao động (full/part-time), chứa base_salary hoặc hourly_rate, thời gian hiệu lực để tính lương đúng kỳ.
- `shift_attendance`: chấm công/ca cho staff part-time; dữ liệu gốc để tính giờ làm.
  - Thuộc tính: `staff_user_id` (CHANGED: từ staff_profile_id → staff_user_id, reference users.id với role=STAFF), `contract_id`, `shift_date`, `shift_hours`, `role`, `approved_by/approved_at` (reference users.id với role=ADMIN/STAFF) để khóa công sau khi duyệt.
  
- `payroll_runs`: kỳ lương theo branch, luồng DRAFT → APPROVED → PAID; khóa/snapshot trước khi chi trả.
  - Thuộc tính: `period_start/period_end`, `branch_id`, `status` (DRAFT/APPROVED/PAID), `approved_by`, `paid_at`, `created_at`.
- `payroll_lines`: chi tiết lương theo thành phần (teaching/base/overtime/allowance/deduction); `source_id` trỏ đến `session_roles` (giờ dạy), `contracts` (base) hoặc expense (deduction) để audit.
  - Thuộc tính: `payroll_run_id`, `staff_user_id` (CHANGED: từ staff_profile_id → staff_user_id, reference users.id với role=STAFF/TEACHER), `component_type` (TEACHING/TA/CLUB/WORKSHOP/BASE/OVERTIME/ALLOWANCE/DEDUCTION), `source_id`, `amount`, `description`, `is_paid`, `paid_at`.
- `session_roles`: log phân công nhân sự cho từng buổi học (`session_id`, `staff_user_id` (CHANGED: từ staff_profile_id → staff_user_id, reference users.id với role=TEACHER/STAFF), `role` MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP), kèm `payable_unit_price`, `payable_allowance` để tính lương teaching.
  - Khác nhau:
    - `shift_attendance`: chấm ca/giờ làm (thường part-time, không buộc gắn buổi học cụ thể), khóa bởi approved_by/approved_at.
    - `session_roles`: gắn trực tiếp vào buổi học (session) để biết ai dạy/trợ giảng và đơn giá/allowance áp cho buổi đó.
- `payroll_payments`: bản ghi chi lương; lưu phương thức và liên kết `cashbook_entries` để khớp quỹ chi.
  - Thuộc tính: `payroll_run_id`, `staff_user_id` (CHANGED: từ staff_profile_id → staff_user_id, reference users.id với role=STAFF/TEACHER), `amount`, `method` (BANK_TRANSFER/CASH), `paid_at`, `cashbook_entry_id` để liên kết bút toán chi.

**Luồng thu học phí:**
1) Lập hóa đơn: tạo `invoices` + `invoice_lines` (gắn `session_ids` nếu thu theo buổi), sinh link/QR PayOS.
2) Thu tiền:
   - Online: webhook PayOS → ghi `payments` (PAYOS) → cập nhật invoice → tạo `cashbook_entries` thu.
   - Offline: thu tiền mặt/chuyển khoản → tạo `payments` (CASH/BANK_TRANSFER) + `cashbook_entries` thu, cập nhật invoice thủ công.
3) Đối soát: xem `cashbook_entries` theo nguồn INVOICE, so sánh `payments` vs `invoices` để phát hiện thiếu/nhầm.

**Luồng lương:**
1) Thu thập giờ công: `shift_attendance` (part-time) + `session_roles` (giờ dạy).
2) Tính lương: mở `payroll_runs` (DRAFT), tạo `payroll_lines` từ contracts và session_roles, bổ sung phụ cấp/khấu trừ.
3) Phê duyệt: đổi `payroll_runs` → APPROVED để khóa dữ liệu tính.
4) Chi trả: tạo `payroll_payments`, ghi bút toán chi vào `cashbook_entries`, set `payroll_runs` → PAID.


## 12. Khối Notification & Ticket
- `notifications`: log gửi Zalo OA/push/email; lưu payload, trạng thái (PENDING/SENT/FAILED), response id, deeplink mở đúng màn hình (mini-app/web). Dùng để retry và audit.
- `notification_templates`: template đa kênh theo code; có placeholders + i18n để render nội dung động, giảm lỗi gõ tay.
- `email_templates`: template subject/body cho email giao dịch/hệ thống (nhắc học phí, biên lai, reset PIN...).
- `tickets`: phiếu hỗ trợ do phụ huynh/teacher/staff mở; trường chính: type/category, status, priority, assignee, opened_by, closed_at; phục vụ SLA hỗ trợ.
- `ticket_comments`: thread hội thoại/trao đổi; lưu người comment, nội dung, file đính kèm (nếu có).

**Luồng gửi thông báo:**
1) Chọn template (`notification_templates`/`email_templates`) + fill placeholders.
2) Tạo record `notifications` (channel, target, payload, deeplink) với trạng thái PENDING.
3) Worker gửi ra Zalo OA/push/email; cập nhật status (SENT/FAILED) và response id; có thể retry nếu FAILED.

**Luồng ticket:**
1) Người dùng mở ticket (`tickets`) với type/category + priority + mô tả; hệ thống gán assignee (hoặc hàng đợi).
2) Trao đổi qua `ticket_comments` (text/file), cập nhật status (OPEN/IN_PROGRESS/RESOLVED/CLOSED) và assignee khi cần.
3) Đóng ticket: set closed_at, giữ history comment để tra soát và báo cáo SLA.


## 13. Audit
- `audit_logs`: trace thao tác (before/after) cho các entity quan trọng.

## 14. Soft delete & integrity bổ sung
- Các bảng cần version/ẩn thay vì xóa: `programs`, `lesson_plan_templates`, `tuition_plans`, `reward_store_items`, `notification_templates` có cờ `is_deleted`.
- Ràng buộc unique:
  - `attendances`: (session_id, student_profile_id).
  - `homework_student`: (assignment_id, student_profile_id).
  - `mission_progress`: (mission_id, student_profile_id).

















## 15. Chi tiết các bảng và Attribute

### 15.1. Khối User/Branch/RBAC

#### `branches` - Chi nhánh
- `id` (uuid, PK): Định danh duy nhất chi nhánh
- `code` (varchar(32), unique): Mã chi nhánh (ví dụ: "HN001", "HCM001")
- `name` (varchar(255)): Tên chi nhánh
- `address` (text): Địa chỉ chi nhánh
- `contact_phone` (varchar(32)): Số điện thoại liên hệ
- `contact_email` (varchar(255)): Email liên hệ
- `is_active` (boolean): Trạng thái hoạt động
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

#### `users` - Tài khoản đăng nhập
- `id` (uuid, PK): Định danh user
- `email` (varchar(255), unique): Email đăng nhập (unique)
- `password_hash` (text): Hash mật khẩu (bcrypt/argon2)
- `role` (varchar(20)): **Role enum - PARENT/STUDENT/ADMIN/TEACHER/STAFF** (NEW)
- `display_name` (varchar(255)): Tên hiển thị (bắt buộc cho ADMIN/TEACHER/STAFF, optional cho PARENT/STUDENT) (NEW)
- `pin_hash` (text): Hash PIN (bắt buộc cho ADMIN/TEACHER/STAFF, optional cho PARENT/STUDENT) (NEW)
- `branch_id` (uuid, FK → branches.id, nullable): Chi nhánh (bắt buộc cho TEACHER/STAFF, null cho ADMIN/PARENT/STUDENT) (NEW)
- `is_active` (boolean): Trạng thái tài khoản
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

**Lưu ý**: 
- ADMIN/TEACHER/STAFF: `role`, `display_name`, `pin_hash`, `branch_id` (cho TEACHER/STAFF) là bắt buộc
- PARENT/STUDENT: `role` là bắt buộc, các field khác optional (có thể lưu trong profiles)

#### `profiles` - Profile người dùng (CHỈ DÙNG CHO PARENT/STUDENT)
- `id` (uuid, PK): Định danh profile
- `user_id` (uuid, FK → users.id): Liên kết với user (user phải có `role = PARENT` hoặc `role = STUDENT`)
- `profile_type` (varchar(20)): Loại profile - **CHỈ CÓ PARENT/STUDENT** (CHANGED: bỏ TEACHER/STAFF)
- `display_name` (varchar(255)): Tên hiển thị
- `pin_hash` (text): Hash PIN (bắt buộc cho PARENT profile khi chọn profile)
- `is_active` (boolean): Trạng thái profile
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

**Lưu ý**: 
- `branch_id` đã bị bỏ (CHANGED: không cần cho PARENT/STUDENT)
- Chỉ dùng cho PARENT/STUDENT, không dùng cho ADMIN/TEACHER/STAFF

#### `parent_student_links` - Liên kết phụ huynh-học sinh
- `id` (uuid, PK): Định danh liên kết
- `parent_profile_id` (uuid, FK → profiles.id): Profile phụ huynh (profile_type=PARENT)
- `student_profile_id` (uuid, FK → profiles.id): Profile học sinh (profile_type=STUDENT)
- `created_at` (timestamptz): Thời gian tạo liên kết

**Lưu ý**: Bảng `roles` và `profile_roles` đã được bỏ (REMOVED). Tất cả role được quản lý trực tiếp qua `users.role` enum (PARENT/STUDENT/ADMIN/TEACHER/STAFF).

### 15.2. Khối Chương trình/Lớp/Lịch

#### `programs` - Chương trình học
- `id` (uuid, PK): Định danh chương trình
- `name` (varchar(255)): Tên chương trình (ví dụ: "Tiếng Anh Level 1")
- `level` (varchar(100)): Cấp độ (ví dụ: "Beginner", "Intermediate")
- `total_sessions` (int): Tổng số buổi học
- `default_tuition_amount` (numeric): Học phí mặc định
- `unit_price_session` (numeric): Giá mỗi buổi học (dùng cho EXTRA_PAID)
- `description` (text): Mô tả chương trình
- `is_active` (boolean): Trạng thái hoạt động
- `is_deleted` (boolean): Đánh dấu xóa mềm

#### `classrooms` - Phòng học
- `id` (uuid, PK): Định danh phòng
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `name` (varchar(100)): Tên phòng (ví dụ: "Phòng A101")
- `capacity` (int): Sức chứa tối đa (số học sinh)
- `note` (text): Ghi chú về phòng (thiết bị, đặc điểm)
- `is_active` (boolean): Trạng thái hoạt động

#### `classes` - Lớp học
- `id` (uuid, PK): Định danh lớp
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `program_id` (uuid, FK → programs.id): Chương trình học
- `code` (varchar(50), unique): Mã lớp (ví dụ: "ENG-L1-2024-01")
- `title` (varchar(255)): Tên lớp
- `main_teacher_id` (uuid, FK → users.id, nullable): Giáo viên chính dự kiến (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `assistant_teacher_id` (uuid, FK → users.id, nullable): Trợ giảng dự kiến (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `start_date` (date): Ngày bắt đầu lớp
- `end_date` (date): Ngày kết thúc lớp
- `status` (varchar(20)): Trạng thái - PLANNED/ACTIVE/CLOSED
- `capacity` (int): Sĩ số tối đa
- `schedule_pattern` (text): Lịch học dạng RRULE hoặc JSON (ví dụ: "Thứ 2,4,6 lúc 18:00")
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

#### `class_enrollments` - Ghi danh học sinh
- `id` (uuid, PK): Định danh ghi danh
- `class_id` (uuid, FK → classes.id): Lớp học
- `student_profile_id` (uuid, FK → profiles.id): Học sinh
- `enroll_date` (date): Ngày ghi danh
- `status` (varchar(20)): Trạng thái - ACTIVE/PAUSED/DROPPED
- `is_main` (boolean): Lớp chính (true) hay lớp bổ trợ (false)
- `tuition_plan_id` (uuid, FK → tuition_plans.id, nullable): Gói học phí áp dụng

#### `sessions` - Buổi học
- `id` (uuid, PK): Định danh buổi học
- `class_id` (uuid, FK → classes.id): Lớp học
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `planned_datetime` (timestamptz): Thời gian dự kiến
- `planned_room_id` (uuid, FK → classrooms.id, nullable): Phòng dự kiến
- `planned_teacher_id` (uuid, FK → users.id, nullable): Giáo viên dự kiến (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `planned_assistant_id` (uuid, FK → users.id, nullable): Trợ giảng dự kiến (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `duration_minutes` (int): Thời lượng buổi học (phút)
- `participation_type` (varchar(20)): Loại tham gia - MAIN/MAKEUP/EXTRA_PAID/FREE/TRIAL
- `status` (varchar(20)): Trạng thái - SCHEDULED/COMPLETED/CANCELLED
- `actual_datetime` (timestamptz, nullable): Thời gian thực tế (nếu khác dự kiến)
- `actual_room_id` (uuid, FK → classrooms.id, nullable): Phòng thực tế
- `actual_teacher_id` (uuid, FK → users.id, nullable): Giáo viên thực tế (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `actual_assistant_id` (uuid, FK → users.id, nullable): Trợ giảng thực tế (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

#### `session_roles` - Vai trò trong buổi học
- `id` (uuid, PK): Định danh
- `session_id` (uuid, FK → sessions.id): Buổi học
- `staff_user_id` (uuid, FK → users.id): Staff/Teacher tham gia (CHANGED: từ staff_profile_id → staff_user_id, role=TEACHER/STAFF)
- `role` (varchar(30)): Vai trò - MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP
- `payable_unit_price` (numeric): Đơn giá tính lương cho vai trò này
- `payable_allowance` (numeric): Phụ cấp (nếu có)

### 15.3. Khối Nghỉ/Điểm danh/Học bù

#### `leave_requests` - Yêu cầu nghỉ học
- `id` (uuid, PK): Định danh yêu cầu
- `student_profile_id` (uuid, FK → profiles.id): Học sinh xin nghỉ (giữ nguyên - student dùng profile)
- `class_id` (uuid, FK → classes.id): Lớp học
- `session_date` (date): Ngày nghỉ (ngày đầu tiên nếu nghỉ nhiều ngày)
- `end_date` (date, nullable): Ngày kết thúc nghỉ (null nếu nghỉ 1 ngày)
- `reason` (text): Lý do nghỉ
- `notice_hours` (int): Số giờ báo trước (để áp dụng luật 24h)
- `status` (varchar(20)): Trạng thái - PENDING/APPROVED/REJECTED
- `requested_at` (timestamptz): Thời gian yêu cầu
- `approved_by` (uuid, FK → users.id, nullable): Người duyệt (CHANGED: từ profiles.id → users.id, role=STAFF)
- `approved_at` (timestamptz, nullable): Thời gian duyệt

#### `attendances` - Điểm danh
- `id` (uuid, PK): Định danh điểm danh
- `session_id` (uuid, FK → sessions.id): Buổi học
- `student_profile_id` (uuid, FK → profiles.id): Học sinh (giữ nguyên - student dùng profile)
- `attendance_status` (varchar(20)): Trạng thái - PRESENT/ABSENT/MAKEUP
- `absence_type` (varchar(30)): Loại vắng - WITH_NOTICE_24H/UNDER_24H/NO_NOTICE/LONG_TERM
- `marked_by` (uuid, FK → users.id): Giáo viên điểm danh (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `marked_at` (timestamptz): Thời gian điểm danh
- **Unique constraint**: (session_id, student_profile_id)

#### `makeup_credits` - Credit học bù
- `id` (uuid, PK): Định danh credit
- `student_profile_id` (uuid, FK → profiles.id): Học sinh sở hữu credit
- `source_session_id` (uuid, FK → sessions.id): **Buổi học nghỉ hợp lệ** - Buổi học mà học sinh đã nghỉ, tạo ra credit này (bất biến, luôn có giá trị)
- `status` (varchar(20)): Trạng thái - AVAILABLE/USED/EXPIRED
- `created_reason` (varchar(30)): Lý do tạo - APPROVED_LEAVE_24H/LONG_TERM
- `expires_at` (timestamptz, nullable): Thời hạn sử dụng (ví dụ: 3 tháng sau khi tạo)
- `used_session_id` (uuid, FK → sessions.id, nullable): **Buổi học bù đã dùng** - Buổi học mà học sinh tham gia để học bù, sử dụng credit này (NULL khi chưa dùng, được set khi credit được gán vào buổi bù)
- `created_at` (timestamptz): Thời gian tạo credit

**Lưu ý về `source_session_id` vs `used_session_id`**:
- `source_session_id`: Buổi học nghỉ (đầu vào) - không thay đổi
- `used_session_id`: Buổi học bù (đầu ra) - được cập nhật khi sử dụng credit
- Ví dụ: Nghỉ buổi 15/03 (`source_session_id`) → Học bù buổi 20/03 (`used_session_id`)

#### `makeup_allocations` - Phân bổ credit vào buổi bù
- `id` (uuid, PK): Định danh
- `makeup_credit_id` (uuid, FK → makeup_credits.id): Credit được phân bổ
- `target_session_id` (uuid, FK → sessions.id): Buổi học bù được gán
- `assigned_by` (uuid, FK → users.id): Staff gán (CHANGED: từ profiles.id → users.id, role=STAFF)
- `assigned_at` (timestamptz): Thời gian gán

### 15.4. Khối Giáo án/Bài tập

#### `lesson_plan_templates` - Template giáo án
- `id` (uuid, PK): Định danh template
- `program_id` (uuid, FK → programs.id): Chương trình
- `level` (varchar(100)): Cấp độ
- `session_index` (int): Số thứ tự buổi học trong chương trình
- `structure_json` (jsonb): Cấu trúc giáo án (JSON)
- `is_active` (boolean): Trạng thái hoạt động
- `is_deleted` (boolean): Đánh dấu xóa mềm
- `created_by` (uuid, FK → users.id): Người tạo (CHANGED: từ profiles.id → users.id, role=ADMIN/STAFF)
- `created_at` (timestamptz): Thời gian tạo

#### `lesson_plans` - Giáo án buổi học
- `id` (uuid, PK): Định danh giáo án
- `session_id` (uuid, FK → sessions.id): Buổi học
- `template_id` (uuid, FK → lesson_plan_templates.id, nullable): Template tham khảo
- `planned_content` (jsonb): Nội dung dự kiến (theo template)
- `actual_content` (jsonb): Nội dung thực tế đã dạy
- `actual_homework` (text): Bài tập về nhà thực tế
- `teacher_notes` (text): Ghi chú của giáo viên
- `submitted_by` (uuid, FK → users.id): Giáo viên nộp (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `submitted_at` (timestamptz): Thời gian nộp

#### `homework_assignments` - Bài tập
- `id` (uuid, PK): Định danh bài tập
- `class_id` (uuid, FK → classes.id): Lớp học
- `session_id` (uuid, FK → sessions.id, nullable): Buổi học (null nếu bài tập chung)
- `title` (varchar(255)): Tiêu đề bài tập
- `description` (text): Mô tả chi tiết
- `due_at` (timestamptz): Hạn nộp
- `book` (varchar(255)): Sách (ví dụ: "Family and Friends 1")
- `pages` (varchar(50)): Trang (ví dụ: "12-15")
- `skills` (varchar(100)): Kỹ năng (ví dụ: "Reading, Writing")
- `submission_type` (varchar(20)): Loại nộp - FILE/IMAGE/TEXT/LINK/QUIZ
- `max_score` (numeric): Điểm tối đa
- `reward_stars` (int): Số sao thưởng khi hoàn thành
- `mission_id` (uuid, FK → missions.id, nullable): Liên kết với mission (nếu có)
- `created_by` (uuid, FK → users.id): Người tạo (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `created_at` (timestamptz): Thời gian tạo

#### `homework_student` - Bài tập của học sinh
- `id` (uuid, PK): Định danh
- `assignment_id` (uuid, FK → homework_assignments.id): Bài tập
- `student_profile_id` (uuid, FK → profiles.id): Học sinh
- `status` (varchar(20)): Trạng thái - ASSIGNED/SUBMITTED/GRADED/LATE/MISSING
- `submitted_at` (timestamptz, nullable): Thời gian nộp
- `graded_at` (timestamptz, nullable): Thời gian chấm
- `score` (numeric, nullable): Điểm số
- `teacher_feedback` (text): Nhận xét của giáo viên
- `ai_feedback` (jsonb): Feedback từ AI (nếu có)
- `attachments` (jsonb): Danh sách file đính kèm (JSON array)
- **Unique constraint**: (assignment_id, student_profile_id)

### 15.5. Khối Kiểm tra định kỳ

#### `exams` - Kỳ kiểm tra
- `id` (uuid, PK): Định danh kỳ kiểm tra
- `class_id` (uuid, FK → classes.id): Lớp học
- `exam_type` (varchar(30)): Loại kiểm tra - PLACEMENT/PROGRESS/MIDTERM/FINAL/SPEAKING
- `date` (date): Ngày kiểm tra
- `max_score` (numeric): Điểm tối đa
- `description` (text): Mô tả
- `created_by` (uuid, FK → users.id): Người tạo (CHANGED: từ profiles.id → users.id, role=TEACHER)

#### `exam_results` - Kết quả kiểm tra
- `id` (uuid, PK): Định danh kết quả
- `exam_id` (uuid, FK → exams.id): Kỳ kiểm tra
- `student_profile_id` (uuid, FK → profiles.id): Học sinh (giữ nguyên - student dùng profile)
- `score` (numeric): Điểm số
- `comment` (text): Nhận xét
- `attachment_url` (text, nullable): **Link file scan bài làm** - URL đến file ảnh/PDF scan bài kiểm tra của học sinh. Dùng để:
  - Lưu trữ bản scan bài làm trên giấy (nếu kiểm tra viết tay)
  - Phụ huynh/học sinh có thể xem lại bài làm đã chấm
  - Lưu trữ bằng chứng điểm số (backup, audit)
  - Hỗ trợ review lại khi có khiếu nại về điểm số
  - Lưu trữ bài làm quan trọng (midterm, final) để tham khảo sau này
- `graded_by` (uuid, FK → users.id): Người chấm (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `graded_at` (timestamptz): Thời gian chấm

**Mục đích sử dụng `attachment_url`**:

1. **Lưu trữ bài làm scan**: Khi học sinh làm bài trên giấy, giáo viên scan và upload lên hệ thống (S3/CDN), lưu URL vào `attachment_url`

2. **Minh chứng điểm số**: File scan là bằng chứng về bài làm và điểm số đã chấm, hỗ trợ giải quyết tranh chấp

3. **Xem lại bài làm**: Phụ huynh và học sinh có thể xem lại bài làm đã chấm qua link này

4. **Lưu trữ lâu dài**: Bài kiểm tra quan trọng (midterm, final) được lưu trữ để tham khảo sau này

**Ví dụ sử dụng**:
```
Kỳ kiểm tra: Midterm Test - Lớp Tiếng Anh Level 1
Học sinh: Nguyễn Văn A
Điểm: 8.5/10

Giáo viên chấm bài trên giấy, scan bài làm và upload lên S3:
→ attachment_url: "https://s3.amazonaws.com/kidzgo/exams/2024/midterm/student_A_scan.pdf"

Khi phụ huynh xem kết quả:
- Thấy điểm: 8.5/10
- Thấy nhận xét: "Bài làm tốt, cần luyện thêm phần ngữ pháp"
- Click vào attachment_url để xem scan bài làm đã chấm
```

**Lưu ý**:
- `attachment_url` là nullable - không phải tất cả bài kiểm tra đều cần scan (ví dụ: quiz online không cần)
- File thường lưu trên S3/CDN, không lưu trực tiếp trong database
- Format file: PDF hoặc ảnh (JPG, PNG)
- Có thể có nhiều trang (scan nhiều trang thành 1 file PDF)

### 15.6. Khối Báo cáo tháng + AI

#### `monthly_report_jobs` - Job tạo báo cáo tháng
- `id` (uuid, PK): Định danh job
- `month` (int): Tháng (1-12)
- `year` (int): Năm
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `status` (varchar(20)): Trạng thái - PENDING/GENERATING/DONE/FAILED
- `started_at` (timestamptz, nullable): Thời gian bắt đầu
- `finished_at` (timestamptz, nullable): Thời gian kết thúc
- `ai_payload_ref` (text, nullable): Reference đến payload AI (để debug/retry)

#### `student_monthly_reports` - Báo cáo tháng của học sinh
- `id` (uuid, PK): Định danh báo cáo
- `student_profile_id` (uuid, FK → profiles.id): Học sinh (giữ nguyên - student dùng profile)
- `month` (int): Tháng (1-12)
- `year` (int): Năm
- `draft_content` (jsonb): Nội dung draft từ AI
- `final_content` (jsonb): Nội dung cuối cùng (sau khi review/chỉnh sửa)
- `status` (varchar(20)): Trạng thái - DRAFT/REVIEW/APPROVED/REJECTED
- `ai_version` (varchar(50)): Phiên bản AI model đã dùng
- `submitted_by` (uuid, FK → users.id, nullable): Người submit (CHANGED: từ profiles.id → users.id, role=TEACHER)
- `reviewed_by` (uuid, FK → users.id, nullable): Người review (CHANGED: từ profiles.id → users.id, role=STAFF/ADMIN)
- `reviewed_at` (timestamptz, nullable): Thời gian review
- `published_at` (timestamptz, nullable): Thời gian publish

#### `report_comments` - Comment review báo cáo
- `id` (uuid, PK): Định danh comment
- `report_id` (uuid, FK → student_monthly_reports.id): Báo cáo
- `commenter_id` (uuid, FK → users.id): Người comment (CHANGED: từ profiles.id → users.id, role=TEACHER/STAFF/ADMIN)
- `content` (text): Nội dung comment
- `created_at` (timestamptz): Thời gian comment

### 15.7. Khối Gamification

#### `missions` - Nhiệm vụ
- `id` (uuid, PK): Định danh nhiệm vụ
- `title` (varchar(255)): Tiêu đề nhiệm vụ
- `description` (text): Mô tả
- `scope` (varchar(20)): Phạm vi - CLASS/STUDENT/GROUP
- `target_class_id` (uuid, FK → classes.id, nullable): Lớp mục tiêu (nếu scope=CLASS)
- `target_group` (jsonb, nullable): Nhóm học sinh cụ thể (nếu scope=GROUP)
- `mission_type` (varchar(50)): Loại nhiệm vụ - HOMEWORK_STREAK/READING_STREAK/NO_UNEXCUSED_ABSENCE/CUSTOM
- `start_at` (timestamptz): Thời gian bắt đầu
- `end_at` (timestamptz): Thời gian kết thúc
- `reward_stars` (int): Số sao thưởng khi hoàn thành
- `created_by` (uuid, FK → users.id): Người tạo (CHANGED: từ profiles.id → users.id, role=TEACHER/STAFF)
- `created_at` (timestamptz): Thời gian tạo

#### `mission_progress` - Tiến độ nhiệm vụ
- `id` (uuid, PK): Định danh
- `mission_id` (uuid, FK → missions.id): Nhiệm vụ
- `student_profile_id` (uuid, FK → profiles.id): Học sinh (giữ nguyên - student dùng profile)
- `status` (varchar(20)): Trạng thái - ASSIGNED/IN_PROGRESS/COMPLETED/EXPIRED
- `progress_value` (numeric): Giá trị tiến độ (ví dụ: số ngày streak)
- `completed_at` (timestamptz, nullable): Thời gian hoàn thành
- `verified_by` (uuid, FK → users.id, nullable): Người xác nhận (CHANGED: từ profiles.id → users.id, role=TEACHER/STAFF)
- **Unique constraint**: (mission_id, student_profile_id)

#### `star_transactions` - Giao dịch sao
- `id` (uuid, PK): Định danh giao dịch
- `student_profile_id` (uuid, FK → profiles.id): Học sinh (giữ nguyên - student dùng profile)
- `amount` (int): Số lượng sao (+ hoặc -)
- `reason` (varchar(100)): Lý do (ví dụ: "Hoàn thành bài tập", "Đổi quà")
- `source_type` (varchar(30)): Nguồn - MISSION/MANUAL/HOMEWORK/TEST/ADJUSTMENT
- `source_id` (uuid, nullable): ID của nguồn (mission_id, homework_id...)
- `balance_after` (int): Số dư sau giao dịch (để audit)
- `created_by` (uuid, FK → users.id): Người tạo giao dịch (CHANGED: từ profiles.id → users.id, role=TEACHER/STAFF hoặc system)
- `created_at` (timestamptz): Thời gian tạo

#### `student_levels` - Level học sinh
- `id` (uuid, PK): Định danh
- `student_profile_id` (uuid, FK → profiles.id, unique): Học sinh (1:1)
- `current_level` (varchar(50)): Level hiện tại (ví dụ: "Bronze", "Silver", "Gold")
- `current_xp` (int): XP hiện tại
- `updated_at` (timestamptz): Thời gian cập nhật

#### `reward_store_items` - Item cửa hàng
- `id` (uuid, PK): Định danh item
- `title` (varchar(255)): Tên quà
- `description` (text): Mô tả
- `cost_stars` (int): Giá (số sao)
- `quantity` (int): Số lượng còn lại
- `is_active` (boolean): Trạng thái hoạt động
- `is_deleted` (boolean): Đánh dấu xóa mềm
- `created_at` (timestamptz): Thời gian tạo

#### `reward_redemptions` - Đổi quà
- `id` (uuid, PK): Định danh
- `item_id` (uuid, FK → reward_store_items.id): Item được đổi
- `student_profile_id` (uuid, FK → profiles.id): Học sinh đổi (giữ nguyên - student dùng profile)
- `status` (varchar(20)): Trạng thái - REQUESTED/APPROVED/DELIVERED/RECEIVED/CANCELLED
- `handled_by` (uuid, FK → users.id, nullable): Staff xử lý (duyệt) (CHANGED: từ profiles.id → users.id, role=STAFF)
- `handled_at` (timestamptz, nullable): Thời gian duyệt (APPROVED)
- `delivered_at` (timestamptz, nullable): Thời gian staff trao quà (DELIVERED) - dùng để tính 3 ngày auto confirm
- `received_at` (timestamptz, nullable): Thời gian học sinh xác nhận nhận quà hoặc auto confirm (RECEIVED)
- `created_at` (timestamptz): Thời gian yêu cầu

**Luồng status mới**:
- `REQUESTED` → `APPROVED` → `DELIVERED` → `RECEIVED` (hoặc auto sau 3 ngày)
- `DELIVERED`: Staff đã trao quà, chờ học sinh xác nhận
- `RECEIVED`: Học sinh đã xác nhận hoặc auto confirm sau 3 ngày từ `delivered_at`
- Background job tự động chuyển `DELIVERED` → `RECEIVED` nếu sau 3 ngày không xác nhận

### 15.8. Khối CRM/Placement

#### `leads` - Lead khách hàng
- `id` (uuid, PK): Định danh lead.
- `source` (varchar(30)): Kênh gốc - LANDING (form web), ZALO (OA/chatbot), REFERRAL (giới thiệu), OFFLINE (walk-in/sự kiện).
- `campaign` (varchar(100), nullable): Tag chiến dịch/quảng cáo để đo hiệu quả.
- `contact_name` (varchar(255)): Tên liên hệ (phụ huynh hoặc người giới thiệu).
- `phone` (varchar(50)): Số điện thoại chính.
- `zalo_id` (varchar(100), nullable): Zalo ID (nếu khác phone).
- `email` (varchar(255), nullable): Email liên hệ.
- `branch_preference` (uuid, FK → branches.id, nullable): Chi nhánh mong muốn.
- `program_interest` (varchar(255)): Chương trình quan tâm ban đầu.
- `notes` (text): Ghi chú tự do.
- `status` (varchar(30)): Trạng thái chăm sóc - NEW/CONTACTED/BOOKED_TEST/TEST_DONE/ENROLLED/LOST. NO_SHOW là tình huống test không tham dự (lưu trong placement_tests).
- `owner_staff_id` (uuid, FK → users.id, nullable): Sales phụ trách hiện tại (CHANGED: từ profiles.id → users.id, role=STAFF).
- `first_response_at` (timestamptz, nullable): Thời điểm phản hồi đầu tiên (đo SLA).
- `touch_count` (int, nullable): Số lần chạm đã thực hiện.
- `next_action_at` (timestamptz, nullable): Lịch follow-up tiếp theo.
- `converted_student_profile_id` (uuid, FK → profiles.id, nullable): Học sinh được tạo khi ghi danh (giữ nguyên - student dùng profile).
- `converted_at` (timestamptz, nullable): Thời điểm chuyển trạng thái ENROLLED.
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật.

#### `placement_tests` - Test xếp lớp
- `id` (uuid, PK): Định danh test
- `lead_id` (uuid, FK → leads.id, nullable): Lead (nếu chưa là học sinh)
- `student_profile_id` (uuid, FK → profiles.id, nullable): Học sinh (nếu đã ghi danh) (giữ nguyên - student dùng profile)
- `scheduled_at` (timestamptz): Thời gian hẹn test
- `result_score` (numeric, nullable): Điểm số
- `level_recommendation` (varchar(100), nullable): Cấp độ đề xuất
- `notes` (text): Ghi chú
- `attachment_url` (text, nullable): Link file kết quả
- `invigilator_user_id` (uuid, FK → users.id, nullable): Người thực hiện test (CHANGED: từ invigilator_profile_id → invigilator_user_id, role=TEACHER/STAFF)

### 15.9. Khối Media

#### `media_assets` - Tài nguyên media
- `id` (uuid, PK): Định danh media
- `uploader_id` (uuid, FK → users.id): Người upload (CHANGED: từ profiles.id → users.id, role=TEACHER/STAFF)
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `class_id` (uuid, FK → classes.id, nullable): Lớp học (nếu liên quan)
- `student_profile_id` (uuid, FK → profiles.id, nullable): Học sinh (nếu liên quan) (giữ nguyên - student dùng profile)
- `month_tag` (varchar(7)): Tháng (format YYYY-MM, ví dụ: "2024-03")
- `type` (varchar(10)): Loại - PHOTO/VIDEO
- `url` (text): URL file (S3/CDN)
- `caption` (text, nullable): Chú thích
- `visibility` (varchar(20)): Quyền xem - CLASS_ONLY/PERSONAL/PUBLIC_PARENT
- `created_at` (timestamptz): Thời gian upload

### 15.10. Khối Tài chính/PayOS/Sổ quỹ/Lương

#### `tuition_plans` - Gói học phí
- `id` (uuid, PK): Định danh gói
- `branch_id` (uuid, FK → branches.id, nullable): Chi nhánh (null nếu dùng chung)
- `program_id` (uuid, FK → programs.id): Chương trình
- `total_sessions` (int): Tổng số buổi
- `tuition_amount` (numeric): Tổng học phí
- `unit_price_session` (numeric): Giá mỗi buổi (dùng cho EXTRA_PAID)
- `currency` (varchar(10)): Đơn vị tiền tệ (VND/USD)
- `is_active` (boolean): Trạng thái hoạt động
- `is_deleted` (boolean): Đánh dấu xóa mềm

#### `invoices` - Hóa đơn
- `id` (uuid, PK): Định danh hóa đơn
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `student_profile_id` (uuid, FK → profiles.id): Học sinh (giữ nguyên - student dùng profile)
- `class_id` (uuid, FK → classes.id, nullable): Lớp học (nếu liên quan)
- `type` (varchar(30)): Loại - MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE
- `amount` (numeric): Tổng tiền
- `currency` (varchar(10)): Đơn vị tiền tệ
- `due_date` (date): Hạn thanh toán
- `status` (varchar(20)): Trạng thái - PENDING/PAID/OVERDUE/CANCELLED
- `description` (text): Mô tả
- `payos_payment_link` (text, nullable): Link thanh toán PayOS
- `payos_qr` (text, nullable): QR code PayOS
- `issued_at` (timestamptz): Thời gian phát hành
- `issued_by` (uuid, FK → users.id): Người phát hành (CHANGED: từ profiles.id → users.id, role=STAFF)

#### `invoice_lines` - Chi tiết hóa đơn
- `id` (uuid, PK): Định danh dòng
- `invoice_id` (uuid, FK → invoices.id): Hóa đơn
- `item_type` (varchar(30)): Loại item - SESSION_MAIN/SESSION_EXTRA/MATERIAL/EVENT
- `quantity` (int): Số lượng
- `unit_price` (numeric): Đơn giá
- `description` (text): Mô tả
- `session_ids` (jsonb, nullable): Danh sách session IDs (nếu item_type là SESSION_*)

#### `payments` - Thanh toán
- `id` (uuid, PK): Định danh thanh toán
- `invoice_id` (uuid, FK → invoices.id): Hóa đơn
- `method` (varchar(20)): Phương thức - PAYOS/CASH/BANK_TRANSFER
- `amount` (numeric): Số tiền thanh toán
- `paid_at` (timestamptz): Thời gian thanh toán
- `reference_code` (varchar(100), nullable): Mã tham chiếu (PayOS transaction code)
- `confirmed_by` (uuid, FK → users.id, nullable): Người xác nhận (CHANGED: từ profiles.id → users.id, role=STAFF)
- `evidence_url` (text, nullable): Link chứng từ (với BANK_TRANSFER)

#### `cashbook_entries` - Sổ quỹ
- `id` (uuid, PK): Định danh giao dịch
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `type` (varchar(10)): Loại - CASH_IN/CASH_OUT
- `amount` (numeric): Số tiền
- `currency` (varchar(10)): Đơn vị tiền tệ
- `description` (text): Mô tả
- `related_type` (varchar(30)): Loại liên quan - INVOICE/PAYROLL/EXPENSE/ADJUSTMENT
- `related_id` (uuid, nullable): ID của bảng liên quan
- `entry_date` (date): Ngày giao dịch
- `created_by` (uuid, FK → users.id): Người tạo (CHANGED: từ profiles.id → users.id, role=STAFF)
- `attachment_url` (text, nullable): Link chứng từ
- `created_at` (timestamptz): Thời gian tạo

#### `contracts` - Hợp đồng lao động
- `id` (uuid, PK): Định danh hợp đồng
- `staff_user_id` (uuid, FK → users.id): Staff (CHANGED: từ staff_profile_id → staff_user_id, role=STAFF/TEACHER)
- `contract_type` (varchar(20)): Loại - PROBATION/FIXED_TERM/INDEFINITE/PART_TIME
- `start_date` (date): Ngày bắt đầu
- `end_date` (date, nullable): Ngày kết thúc (null nếu INDEFINITE)
- `base_salary` (numeric, nullable): Lương cơ bản (với full-time)
- `hourly_rate` (numeric, nullable): Lương theo giờ (với part-time)
- `allowance_fixed` (numeric, nullable): Phụ cấp cố định
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `is_active` (boolean): Trạng thái hiệu lực

#### `shift_attendance` - Chấm công ca làm việc
- `id` (uuid, PK): Định danh ca
- `staff_user_id` (uuid, FK → users.id): Staff (CHANGED: từ staff_profile_id → staff_user_id, role=STAFF)
- `contract_id` (uuid, FK → contracts.id, nullable): Hợp đồng
- `shift_date` (date): Ngày ca
- `shift_hours` (numeric): Số giờ làm việc
- `role` (varchar(50)): Vai trò trong ca
- `approved_by` (uuid, FK → users.id, nullable): Người duyệt (CHANGED: từ profiles.id → users.id, role=ADMIN/STAFF)
- `approved_at` (timestamptz, nullable): Thời gian duyệt

#### `payroll_runs` - Kỳ lương
- `id` (uuid, PK): Định danh kỳ lương
- `period_start` (date): Ngày bắt đầu kỳ
- `period_end` (date): Ngày kết thúc kỳ
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `status` (varchar(20)): Trạng thái - DRAFT/APPROVED/PAID
- `approved_by` (uuid, FK → users.id, nullable): Người duyệt (CHANGED: từ profiles.id → users.id, role=ADMIN/STAFF)
- `paid_at` (timestamptz, nullable): Thời gian thanh toán
- `created_at` (timestamptz): Thời gian tạo

#### `payroll_lines` - Chi tiết lương
- `id` (uuid, PK): Định danh dòng lương
- `payroll_run_id` (uuid, FK → payroll_runs.id): Kỳ lương
- `staff_user_id` (uuid, FK → users.id): Staff (CHANGED: từ staff_profile_id → staff_user_id, role=STAFF/TEACHER)
- `component_type` (varchar(30)): Thành phần - TEACHING/TA/CLUB/WORKSHOP/BASE/OVERTIME/ALLOWANCE/DEDUCTION
- `source_id` (uuid, nullable): ID nguồn (session_roles.id cho TEACHING/TA/CLUB/WORKSHOP, contracts.id cho BASE)
- `amount` (numeric): Số tiền
- `description` (text): Mô tả
- `is_paid` (boolean): Đã thanh toán chưa
- `paid_at` (timestamptz, nullable): Thời gian thanh toán

#### `payroll_payments` - Thanh toán lương
- `id` (uuid, PK): Định danh thanh toán
- `payroll_run_id` (uuid, FK → payroll_runs.id): Kỳ lương
- `staff_user_id` (uuid, FK → users.id): Staff (CHANGED: từ staff_profile_id → staff_user_id, role=STAFF/TEACHER)
- `amount` (numeric): Số tiền thanh toán
- `method` (varchar(20)): Phương thức - BANK_TRANSFER/CASH
- `paid_at` (timestamptz): Thời gian thanh toán
- `cashbook_entry_id` (uuid, FK → cashbook_entries.id, nullable): Liên kết với sổ quỹ

### 15.11. Khối Notification & Ticket

#### `notifications` - Thông báo
- `id` (uuid, PK): Định danh thông báo
- `recipient_user_id` (uuid, FK → users.id): Người nhận (CHANGED: thêm field mới, có thể là bất kỳ role nào)
- `recipient_profile_id` (uuid, FK → profiles.id, nullable): Profile người nhận (optional, cho PARENT/STUDENT) (CHANGED: từ required → optional)
- `channel` (varchar(20)): Kênh - ZALO_OA/PUSH/EMAIL
- `title` (varchar(255)): Tiêu đề
- `content` (text): Nội dung
- `deeplink` (text, nullable): Link deep link đến màn hình app/web
- `status` (varchar(20)): Trạng thái - PENDING/SENT/FAILED
- `sent_at` (timestamptz, nullable): Thời gian gửi
- `template_id` (varchar(100), nullable): ID template đã dùng
- `created_at` (timestamptz): Thời gian tạo

#### `notification_templates` - Template thông báo
- `id` (uuid, PK): Định danh template
- `code` (varchar(100), unique): Mã template (ví dụ: "HOMEWORK_DUE", "SESSION_REMINDER")
- `channel` (varchar(20)): Kênh - ZALO_OA/PUSH/EMAIL
- `title` (varchar(255)): Tiêu đề template
- `content` (text): Nội dung template (có placeholders)
- `placeholders` (jsonb): Danh sách placeholders (ví dụ: {"student_name", "class_name"})
- `is_active` (boolean): Trạng thái hoạt động
- `is_deleted` (boolean): Đánh dấu xóa mềm
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

#### `email_templates` - Template email
- `id` (uuid, PK): Định danh template
- `code` (varchar(100), unique): Mã template
- `subject` (varchar(255)): Subject email
- `body` (text): Body email (HTML/text)
- `placeholders` (jsonb): Danh sách placeholders
- `is_active` (boolean): Trạng thái hoạt động
- `is_deleted` (boolean): Đánh dấu xóa mềm
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

#### `tickets` - Ticket hỗ trợ
- `id` (uuid, PK): Định danh ticket
- `opened_by_user_id` (uuid, FK → users.id): Người mở ticket (CHANGED: thêm field mới, có thể là bất kỳ role nào)
- `opened_by_profile_id` (uuid, FK → profiles.id, nullable): Profile người mở (optional, cho PARENT/STUDENT) (CHANGED: từ required → optional)
- `branch_id` (uuid, FK → branches.id): Chi nhánh
- `class_id` (uuid, FK → classes.id, nullable): Lớp học (nếu liên quan)
- `category` (varchar(30)): Danh mục - HOMEWORK/FINANCE/SCHEDULE/TECH
- `message` (text): Nội dung ticket
- `status` (varchar(20)): Trạng thái - OPEN/IN_PROGRESS/RESOLVED/CLOSED
- `assigned_to_user_id` (uuid, FK → users.id, nullable): Người được gán xử lý (CHANGED: từ assigned_to_profile_id → assigned_to_user_id, role=STAFF)
- `created_at`, `updated_at` (timestamptz): Thời gian tạo/cập nhật

#### `ticket_comments` - Comment ticket
- `id` (uuid, PK): Định danh comment
- `ticket_id` (uuid, FK → tickets.id): Ticket
- `commenter_user_id` (uuid, FK → users.id): Người comment (CHANGED: từ commenter_id → commenter_user_id, có thể là bất kỳ role nào)
- `commenter_profile_id` (uuid, FK → profiles.id, nullable): Profile người comment (optional, cho PARENT/STUDENT) (CHANGED: thêm field mới)
- `message` (text): Nội dung comment
- `attachment_url` (text, nullable): Link file đính kèm
- `created_at` (timestamptz): Thời gian comment

### 15.12. Khối Audit

#### `audit_logs` - Log audit
- `id` (uuid, PK): Định danh log
- `actor_user_id` (uuid, FK → users.id, nullable): Người thực hiện (CHANGED: từ actor_id → actor_user_id, có thể là bất kỳ role nào)
- `actor_profile_id` (uuid, FK → profiles.id, nullable): Profile người thực hiện (optional, cho PARENT/STUDENT) (CHANGED: thêm field mới)
- `action` (varchar(100)): Hành động (ví dụ: "CREATE_CLASS", "UPDATE_INVOICE")
- `entity_type` (varchar(100)): Loại entity (ví dụ: "classes", "invoices")
- `entity_id` (uuid): ID của entity
- `data_before` (jsonb, nullable): Dữ liệu trước khi thay đổi
- `data_after` (jsonb, nullable): Dữ liệu sau khi thay đổi
- `created_at` (timestamptz): Thời gian thực hiện

## 16. Quan hệ giữa các bảng (Entity Relationships)

### 1. Quan hệ User/Branch/RBAC (UPDATED)
- `users` → `profiles` (1:N): Một user có thể có nhiều profile (CHỈ PARENT/STUDENT, không có TEACHER/STAFF/ADMIN).
- `users` → `branches` (N:1): Mỗi user TEACHER/STAFF thuộc một branch (qua `users.branch_id`), ADMIN không ràng buộc branch.
- `profiles` → `parent_student_links` (N:M): Phụ huynh và học sinh liên kết qua bảng trung gian.
- **Lưu ý**: Bảng `roles` và `profile_roles` đã được bỏ (REMOVED). Role được quản lý trực tiếp qua `users.role` enum.
- **ADMIN/TEACHER/STAFF**: Tất cả các bảng reference từ `profiles.id` → chuyển sang `users.id` (ví dụ: `classes.main_teacher_id`, `sessions.planned_teacher_id`, `invoices.issued_by`, v.v.).
- **STUDENT**: Vẫn dùng `profiles.id` (ví dụ: `class_enrollments.student_profile_id`, `attendances.student_profile_id`, v.v.).

### 2. Quan hệ Chương trình/Lớp/Session
- `branches` → `programs`: Chương trình có thể dùng chung hoặc riêng theo branch (không có FK trực tiếp, quản lý qua `tuition_plans`).
- `branches` → `classrooms` (1:N): Mỗi phòng thuộc một branch.
- `branches` → `classes` (1:N): Mỗi lớp thuộc một branch.
- `programs` → `classes` (1:N): Mỗi lớp thuộc một chương trình.
- `classes` → `class_enrollments` (1:N): Một lớp có nhiều học sinh ghi danh.
- `profiles` → `class_enrollments` (1:N): Một học sinh có thể ghi danh nhiều lớp.
- `classes` → `sessions` (1:N): Một lớp có nhiều buổi học.
- `classrooms` → `sessions` (1:N): Một phòng có thể được dùng cho nhiều buổi học (qua `planned_room_id` và `actual_room_id`).
- `users` → `sessions` (1:N): Giáo viên có thể dạy nhiều buổi (qua `planned_teacher_id`, `actual_teacher_id`, `planned_assistant_id`, `actual_assistant_id`, reference users.id với role=TEACHER) (CHANGED: từ profiles → users).
- `sessions` → `session_roles` (1:N): Một buổi học có thể có nhiều người tham gia với vai trò khác nhau (giáo viên chính, trợ giảng, club teacher...).
- `users` → `session_roles` (1:N): Một staff/teacher có thể tham gia nhiều buổi với vai trò khác nhau (CHANGED: từ profiles → users).

**Lưu ý về `sessions` vs `session_roles`:**
- `sessions.planned_teacher_id` / `actual_teacher_id`: Thông tin giáo viên dự kiến/thực tế (đơn giản, 1 giáo viên chính, 1 trợ giảng).
- `session_roles`: Chi tiết hơn, hỗ trợ nhiều người tham gia với vai trò khác nhau và lưu đơn giá để tính lương. Ví dụ: 1 buổi có thể có MAIN_TEACHER (500k), ASSISTANT (200k), CLUB teacher (300k).

### 3. Quan hệ Điểm danh/MakeUp
- `sessions` → `attendances` (1:N): Một buổi học có nhiều điểm danh (mỗi học sinh một record).
- `profiles` → `attendances` (1:N): Một học sinh có nhiều điểm danh.
- `sessions` → `leave_requests` (1:N): Một buổi học có thể có nhiều yêu cầu nghỉ.
- `profiles` → `leave_requests` (1:N): Một học sinh có thể có nhiều yêu cầu nghỉ.
- `sessions` → `makeup_credits` (1:N): Một buổi học có thể tạo nhiều credit (nếu nhiều học sinh nghỉ hợp lệ).
- `sessions` → `makeup_credits.used_session_id` (1:N): Một buổi học có thể được dùng để bù cho nhiều credit.
- `makeup_credits` → `makeup_allocations` (1:1): Một credit được gán vào một buổi bù cụ thể.

### 4. Quan hệ Giáo án/Bài tập
- `programs` → `lesson_plan_templates` (1:N): Một chương trình có nhiều template giáo án.
- `sessions` → `lesson_plans` (1:N về mặt DB, nhưng nghiệp vụ là 1:1): Mỗi buổi học chỉ nên có một giáo án. **Lưu ý**: Schema hiện tại chưa có unique constraint trên `lesson_plans.session_id`, nên về mặt database cho phép nhiều giáo án cho một buổi học. Nên thêm unique constraint để đảm bảo 1:1.
- `lesson_plan_templates` → `lesson_plans` (1:N): Một template có thể dùng cho nhiều buổi học.
- `classes` → `homework_assignments` (1:N): Một lớp có nhiều bài tập.
- `sessions` → `homework_assignments` (1:N): Một buổi học có thể có nhiều bài tập.
- `homework_assignments` → `homework_student` (1:N): Một bài tập có nhiều submission (mỗi học sinh một record).
- `profiles` → `homework_student` (1:N): Một học sinh có nhiều bài tập đã làm.

### 5. Quan hệ Kiểm tra/Báo cáo
- `classes` → `exams` (1:N): Một lớp có nhiều kỳ kiểm tra.
- `exams` → `exam_results` (1:N): Một kỳ kiểm tra có nhiều kết quả (mỗi học sinh một record).
- `profiles` → `exam_results` (1:N): Một học sinh có nhiều kết quả kiểm tra.
- `profiles` → `student_monthly_reports` (1:N): Một học sinh có nhiều báo cáo tháng.
- `student_monthly_reports` → `report_comments` (1:N): Một báo cáo có nhiều comment review.

### 6. Quan hệ Gamification
- `classes` → `missions` (1:N): Một lớp có thể có nhiều nhiệm vụ.
- `missions` → `mission_progress` (1:N): Một nhiệm vụ có nhiều tiến độ (mỗi học sinh một record).
- `profiles` → `mission_progress` (1:N): Một học sinh có nhiều tiến độ nhiệm vụ.
- `profiles` → `star_transactions` (1:N): Một học sinh có nhiều giao dịch sao.
- `profiles` → `student_levels` (1:1): Mỗi học sinh có một level hiện tại.
- `reward_store_items` → `reward_redemptions` (1:N): Một item có thể được đổi nhiều lần.
- `profiles` → `reward_redemptions` (1:N): Một học sinh có thể đổi nhiều quà.

### 7. Quan hệ CRM/Placement
- `branches` → `leads` (1:N): Một branch có nhiều lead.
- `users` → `leads.owner_staff_id` (1:N): Một staff có thể quản lý nhiều lead (CHANGED: từ profiles → users, role=STAFF).
- `leads` → `placement_tests` (1:N): Một lead có thể có nhiều lần test.
- `profiles` → `placement_tests` (1:N): Một học sinh có thể có nhiều lần test.

### 8. Quan hệ Media
- `branches` → `media_assets` (1:N): Một branch có nhiều media.
- `classes` → `media_assets` (1:N): Một lớp có nhiều media.
- `profiles` → `media_assets` (1:N): Một học sinh/giáo viên có nhiều media.

### 9. Quan hệ Tài chính/Lương
- `branches` → `tuition_plans` (1:N): Một branch có nhiều gói học phí.
- `programs` → `tuition_plans` (1:N): Một chương trình có nhiều gói học phí (theo branch).
- `branches` → `invoices` (1:N): Một branch có nhiều hóa đơn.
- `profiles` → `invoices` (1:N): Một học sinh có nhiều hóa đơn.
- `classes` → `invoices` (1:N): Một lớp có thể liên quan đến nhiều hóa đơn.
- `invoices` → `invoice_lines` (1:N): Một hóa đơn có nhiều dòng chi tiết.
- `invoices` → `payments` (1:N): Một hóa đơn có thể thanh toán nhiều lần (partial payment).
- `branches` → `cashbook_entries` (1:N): Một branch có nhiều giao dịch sổ quỹ.
- `users` → `contracts` (1:N): Một staff có thể có nhiều hợp đồng (theo thời gian) (CHANGED: từ profiles → users, role=STAFF/TEACHER).
- `branches` → `contracts` (1:N): Một branch có nhiều hợp đồng lao động.
- `contracts` → `shift_attendance` (1:N): Một hợp đồng có nhiều ca làm việc.
- `sessions` → `session_roles` (1:N): Một buổi học có nhiều vai trò (đã giải thích ở trên).
- `session_roles` → `payroll_lines` (1:N): Một session_role có thể tạo nhiều dòng lương (nếu tính theo nhiều kỳ).
- `contracts` → `payroll_lines` (1:N): Một hợp đồng có thể tạo nhiều dòng lương (BASE salary).
- `branches` → `payroll_runs` (1:N): Một branch có nhiều kỳ lương.
- `payroll_runs` → `payroll_lines` (1:N): Một kỳ lương có nhiều dòng chi tiết.
- `users` → `payroll_lines` (1:N): Một staff có nhiều dòng lương (CHANGED: từ profiles → users, role=STAFF/TEACHER).
- `payroll_runs` → `payroll_payments` (1:N): Một kỳ lương có thể thanh toán nhiều lần.
- `cashbook_entries` → `payroll_payments` (1:1): Mỗi thanh toán lương ghi một dòng sổ quỹ.

### 10. Quan hệ Notification/Ticket
- `users` → `notifications` (1:N): Một user nhận nhiều thông báo (CHANGED: từ profiles → users, có thể là bất kỳ role nào).
- `profiles` → `notifications` (1:N): Một profile nhận nhiều thông báo (optional, cho PARENT/STUDENT).
- `notification_templates` → `notifications` (1:N): Một template có thể dùng cho nhiều thông báo.
- `users` → `tickets` (1:N): Một user có thể mở nhiều ticket (CHANGED: từ profiles → users, có thể là bất kỳ role nào).
- `profiles` → `tickets` (1:N): Một profile có thể mở nhiều ticket (optional, cho PARENT/STUDENT).
- `branches` → `tickets` (1:N): Một branch có nhiều ticket.
- `classes` → `tickets` (1:N): Một lớp có thể có nhiều ticket.
- `tickets` → `ticket_comments` (1:N): Một ticket có nhiều comment.

### 11. Quan hệ Audit
- `users` → `audit_logs` (1:N): Một user thực hiện nhiều thao tác được audit (CHANGED: từ profiles → users, có thể là bất kỳ role nào).
- `profiles` → `audit_logs` (1:N): Một profile thực hiện nhiều thao tác được audit (optional, cho PARENT/STUDENT).

## 17. Gợi ý thực thi & mapping use-case
- Multi-branch: mọi bảng nghiệp vụ có `branch_id`; truy vấn luôn kèm filter branch dựa trên `users.branch_id` (trừ admin).
- Login + profile + PIN: 
  - **PARENT/STUDENT**: auth trả về danh sách profiles; khi chọn PARENT yêu cầu PIN (`profiles.pin_hash`); set context từ profile.
  - **ADMIN/TEACHER/STAFF**: auth trực tiếp, yêu cầu PIN (`users.pin_hash`); set context branch = `users.branch_id` (null cho admin).
- Điểm danh + MakeUp: teacher chỉ mark Present/Absent; service dựa `leave_requests` + `notice_hours` + `absence_type` để tạo `makeup_credits`; staff gán qua `makeup_allocations`.
- Giáo án/bài tập: tạo template theo `program`, sinh `lesson_plans` khi tạo sessions; homework gán class/buổi, auto tạo `homework_student`.
- Kiểm tra + báo cáo tháng: `exams` + `exam_results` cấp dữ liệu; `student_monthly_reports` chứa draft/final; comment/approve/publish.
- Gamification: `missions` → `mission_progress`; hoàn thành sinh `star_transactions`; level tính từ tổng sao/xp; redeem quà qua `reward_redemptions`.
- Tài chính: hóa đơn & PayOS → `payments` → update `invoices` + ghi `cashbook_entries` (CASH_IN); lương/payout ghi `payroll_runs/lines/payments` + `cashbook_entries` (CASH_OUT).
- Tính lương: Khi session COMPLETED, tạo `session_roles` cho mỗi người tham gia (MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP) với `payable_unit_price` và `payable_allowance`. Khi chạy payroll, query `session_roles` trong kỳ → tạo `payroll_lines` với `component_type`=TEACHING/TA/CLUB/WORKSHOP và `source_id`=session_roles.id. BASE salary lấy từ `contracts`, OVERTIME/ALLOWANCE/DEDUCTION từ các nguồn khác.
- Media: upload kèm tag class/student/month để render album theo tháng cho phụ huynh.
- Notification: `notifications` lưu deeplink tới màn hình mini app/web (TKB, invoice, report, mission, media…).

