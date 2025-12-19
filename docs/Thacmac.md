
## 1. ✅ Mã PIN Profile - Validate kiểu dữ liệu
**Yêu cầu**: Validate PIN < 10 số trước khi hash
**Thay đổi**:
- `profiles.pin_hash`: Thay đổi từ `text` → `varchar(97)` (PBKDF2-SHA512 format: 64-char hash + '-' + 32-char salt)
- Validate PIN < 10 số ở application layer trước khi hash
**Tác dụng**: Giới hạn độ dài PIN hợp lý, tối ưu storage, đảm bảo bảo mật với hash + salt

---

## 2. ✅ Program - Thêm khóa ngoại Branch
**Yêu cầu**: Program thêm khóa ngoại Branch, các chi nhánh không xài chung 1 program
**Thay đổi**:
- `programs.branch_id` (uuid, FK → branches.id): Thêm field mới, required
- Migration: `AddBranchIdToProgram`
**Tác dụng**: Mỗi chi nhánh có program riêng, không chia sẻ chung, đảm bảo data isolation giữa các branch

---

## 4. ✅ Bài tập/Quiz - Bảng mới cho Exercises
**Yêu cầu**: Làm thêm cái bảng để tạo bài tập như quiz hay điền vào như gg form hẳn trên web, các bài tập này (reading, listening) nghiên hướng về multiple choice, writing, học sinh nhập chữ như một ô text
**Bảng mới**:
- `exercises`: Bài tập/quiz (READING/LISTENING/WRITING)
- `exercise_questions`: Câu hỏi trong bài tập (MULTIPLE_CHOICE/TEXT_INPUT)
- `exercise_submissions`: Bài nộp của học sinh
- `exercise_submission_answers`: Chi tiết câu trả lời từng câu hỏi
**Tác dụng**: 
- Hỗ trợ tạo bài tập dạng quiz/form như Google Forms
- Reading/Listening: Multiple choice (auto-grading)
- Writing: Text input (manual grading bởi teacher)
- Lưu trữ đầy đủ submission và answers để review

---

## 5. ✅ Exam Results - Attachment URLs JSON
**Yêu cầu**: Attachment url của exam đổi thành json để tải nhiều ảnh
**Thay đổi**:
- `exam_results.attachment_url` (text) → `attachment_urls` (jsonb)
- Lưu trữ dạng JSON array of strings: `["url1", "url2", ...]`
**Tác dụng**: Cho phép upload và lưu nhiều ảnh scan bài làm thay vì chỉ 1 ảnh

---

## 6. ✅ Session Reports - Bảng mới cho báo cáo buổi học
**Yêu cầu**: Thêm bảng report sao mỗi buổi học cho teacher, cứ mỗi buổi học teacher sẽ note feedback/báo cáo từng bạn (báo cáo theo ngày học), lưu lại dữ liệu để cuối tháng, teacher có thể chọn ngày tháng từ bao nhiêu tới bao nhiêu để xem lại feedback của mình để tổng hợp báo cáo tháng, (mở rộng thêm là sau tổng hợp AI sẽ prompt dữ liệu lại hoàn chỉnh để teacher sửa và nộp)
**Trạng thái**: ✅ Đã hoàn thành
**Bảng mới**:
- `session_reports`: Báo cáo feedback của teacher cho từng học sinh mỗi buổi học
  - `session_id`, `student_profile_id`, `teacher_user_id`
  - `report_date`: Ngày báo cáo (để filter theo date range)
  - `feedback`: Feedback của teacher
  - `ai_generated_summary`: AI-generated summary (cho monthly compilation)
  - `is_monthly_compiled`: Đã được tổng hợp vào monthly report chưa
**Tác dụng**:
- Teacher có thể note feedback cho từng học sinh sau mỗi buổi học
- Filter theo date range để tổng hợp báo cáo tháng
- AI hỗ trợ generate summary từ các feedback để teacher review và chỉnh sửa

---

## 7. ✅ Reward Redemption - Lưu tên item
**Yêu cầu**: Bảng reward redemption nên lưu lại tên item dc đổi
**Trạng thái**: ✅ Đã hoàn thành
**Thay đổi**:
- `reward_redemptions.item_name` (varchar(255)): Thêm field mới, required
- Lưu tên item tại thời điểm đổi (để tránh mất dữ liệu nếu item bị xóa/đổi tên)
**Tác dụng**: Lưu lại tên item tại thời điểm redemption để audit trail, không bị ảnh hưởng nếu item sau này bị xóa hoặc đổi tên

---

## 8. ✅ Mission - Progress per Question
**Yêu cầu**: Mission nên quy định khi trả lời số câu hỏi thì xong 1 câu hỏi thì được bao nhiêu %. Thêm thanh progress
**Trạng thái**: ✅ Đã hoàn thành
**Thay đổi**:
- `missions.total_questions` (int, nullable): Tổng số câu hỏi
- `missions.progress_per_question` (numeric, nullable): Phần trăm tiến độ mỗi câu hỏi (ví dụ: 10% nếu 10 câu)
**Tác dụng**: 
- Tính toán progress bar dựa trên số câu đã trả lời
- Ví dụ: 10 câu hỏi, mỗi câu = 10% → trả lời 5 câu = 50% progress

---

## 9. ✅ Mission - Thêm attribute EXP
**Yêu cầu**: Mission thêm attribute exp
**Trạng thái**: ✅ Đã hoàn thành
**Thay đổi**:
- `missions.reward_exp` (int, nullable): Số EXP thưởng khi hoàn thành mission
**Tác dụng**: Thưởng EXP cho học sinh khi hoàn thành mission, ngoài stars

---

## 10. ✅ Attendance Streak - Bảng mới cho điểm danh hàng ngày
**Yêu cầu**: Thêm 1 bảng để điểm danh (nó là streak điểm danh theo ngày) có nghĩa là cứ vào web sẽ là 1 điểm danh (sau đó sẽ + 1 sao và 5 exp, ví dụ)
**Trạng thái**: ✅ Đã hoàn thành
**Bảng mới**:
- `attendance_streaks`: Streak điểm danh hàng ngày
  - `student_profile_id`, `attendance_date`
  - `current_streak`: Số ngày streak hiện tại
  - `reward_stars`: Số sao thưởng (ví dụ: 1)
  - `reward_exp`: Số EXP thưởng (ví dụ: 5)
  - Unique constraint: (student_profile_id, attendance_date)
**Tác dụng**:
- Khuyến khích học sinh vào web hàng ngày để điểm danh
- Tạo streak (chuỗi ngày liên tiếp) để gamification
- Thưởng stars và exp mỗi lần điểm danh

---

## 12. ✅ Blog - Bảng mới cho Blog Posts
**Yêu cầu**: Admin, staff tạo Blog hiện ra landing page
**Trạng thái**: ✅ Đã hoàn thành
**Bảng mới**:
- `blogs`: Blog posts cho landing page
  - `title`, `summary`, `content` (HTML/Markdown)
  - `featured_image_url`: Ảnh đại diện
  - `created_by`: Admin/Staff user
  - `is_published`: Trạng thái xuất bản
  - `published_at`: Thời gian xuất bản
**Tác dụng**: Admin/Staff có thể tạo blog posts để hiển thị trên landing page, marketing content

---

## 13. ✅ Work Hours Tracking - Bảng mới cho giờ làm việc
**Yêu cầu**: Thêm giờ làm cho tụi nó, 1 tháng ngta làm bao nhiêu thời gian. Làm tối thiểu bao nhiêu giờ để nhận lương, làm dư thì sao. Phần chấm công của giáo viên được tính dựa vào số buổi dạy và giờ
**Thay đổi**:
- `contracts.minimum_monthly_hours` (numeric, nullable): Số giờ làm tối thiểu mỗi tháng để nhận lương
- `contracts.overtime_rate_multiplier` (numeric, nullable): Hệ số nhân lương overtime (ví dụ: 1.5x, 2x)
**Bảng mới**:
- `monthly_work_hours`: Giờ làm việc hàng tháng
  - `staff_user_id`, `contract_id`, `branch_id`
  - `year`, `month` (1-12)
  - `total_hours`: Tổng số giờ làm trong tháng
  - `teaching_hours`: Số giờ dạy (từ sessions, cho giáo viên)
  - `regular_hours`: Số giờ làm thường (từ shift_attendance)
  - `overtime_hours`: Số giờ làm thêm (vượt quá minimum)
  - `teaching_sessions`: Số buổi dạy (cho giáo viên)
  - `is_locked`: Đã khóa chưa (để tính lương, không cho sửa)
  - Unique constraint: (staff_user_id, contract_id, year, month)
**Tác dụng**:
- Theo dõi giờ làm việc hàng tháng cho staff/teachers
- Tính toán overtime dựa trên minimum_monthly_hours
- Giáo viên: tính giờ dạy từ sessions và duration
- Staff: tính giờ làm từ shift_attendance
- Lock mechanism để đảm bảo tính toán lương chính xác
---

## 14. ⚠️ Session Role - Xem xét bỏ
- `session_roles` lưu thông tin payroll riêng (`payable_unit_price`, `payable_allowance`) khác với `sessions` (planned_teacher_id, actual_teacher_id)
- Cho phép linh hoạt tính lương: cùng 1 session có thể có nhiều roles (MAIN_TEACHER + ASSISTANT + CLUB)
- Cần thiết cho payroll calculation
**Quyết định**: Giữ lại `session_roles` vì có mục đích riêng cho payroll, không thừa

--