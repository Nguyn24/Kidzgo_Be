## A3 / A6 / A7 / A8 – Flow & hướng dẫn build

Tài liệu này mô tả cách backend KidzGo gọi AI service, map dữ liệu với DB, và lưu kết quả cho 4 nhóm agent:
- **A3**: chấm bài (text / image / link)
- **A6**: báo cáo tháng
- **A7**: OCR biên lai/chứng từ
- **A8**: speaking/phonics

Chi tiết schema API & prompt: xem `docs/InputDataAI.md` và `docs/Template&PromtAI.md`.

---

## 1. A3 – Homework grading (text / image / link)

**Mục tiêu:** chấm điểm và sinh feedback cho từng bài tập của học sinh.

### 1.1 Flow backend

1) Khi teacher chấm bài hoặc khi cần auto-chấm:
- Lấy `homework_student` theo (assignment_id, student_profile_id).
- Chuẩn bị context:
  - `homework_id` = homework_assignments.id
  - `student_id` = student_profile_id
  - `skill`, `instructions`, `rubric`, `expected_answer_text` từ cấu hình bài tập (hoặc nhập thêm).
  - Bài làm học sinh:
    - Text: từ input của học sinh (ví dụ nội dung text trong attachments hoặc field riêng).
    - Image/file/link: map sang endpoint tương ứng (grade-image / grade-link).

2) Gọi AI service theo `InputDataAI.md`:
- Text: `POST /a3/grade-text` với `GradeTextRequest`.
- Ảnh: `POST /a3/grade-image` (multipart).
- Link: `POST /a3/grade-link`.

3) Nhận `GradeResponse`:
- JSON có `score`, `max_score`, `summary`, `strengths`, `issues`, `suggestions`, `extracted_student_answer`, `confidence`, `warnings`.

4) Lưu vào DB:
- Bảng `homework_student`:
  - `score` ← `GradeResponse.score`.
  - `status` ← `GRADED`.
  - `graded_at` ← now.
  - `ai_feedback` (jsonb) ← nguyên object trả về (có thể thêm `max_score` nếu muốn).
  - `ai_version` ← tên phiên bản model (ví dụ `"gpt-4.1-mini"`).

### 1.2 Acceptance

- Nếu AI trả về lỗi (timeout, invalid JSON) → không cập nhật `score`; lưu log error riêng.
- Mỗi lần chấm lại có thể overwrite `score`, `ai_feedback`, `ai_version` (tùy policy – có thể log history ở chỗ khác nếu cần).

---

## 2. A6 – Monthly reports

**Mục tiêu:** AI sinh bản nháp báo cáo tháng từ dữ liệu session_reports và history.

### 2.1 Chuẩn bị dữ liệu

1) Chạy job `monthly_report_jobs` (per branch, per month):
- Lấy danh sách học sinh active trong tháng.
- Với mỗi học sinh:
  - `session_feedbacks`: query `session_reports` trong range tháng (date, feedback).
  - `recent_reports`: 3 tháng trước từ `student_monthly_reports` (nếu có) – tóm tắt thành list {month, overview, strengths[], improvements[], highlights[], goals_next_month[]}.
  - Thông tin student (name, program) từ profiles/classes/programs.

2) Tạo payload `MonthlyReportRequest` như trong `InputDataAI.md`.

### 2.2 Gọi AI & lưu kết quả

1) Gọi `POST /a6/generate-monthly-report`.
2) Nhận `MonthlyReportResponse`:
- `draft_text` và `sections` (overview, strengths, improvements, highlights, goals_next_month, source_summary).

3) Lưu DB:
- Bảng `student_monthly_reports`:
  - Nếu chưa có record cho (student, month, year) → tạo mới:
    - `draft_content` = full JSON response (hoặc ít nhất `sections`).
    - `final_content` = NULL.
    - `status` = `DRAFT`.
    - `ai_version` = tên model.

### 2.3 Review & publish (không gọi AI)

1) Teacher mở báo cáo tháng → xem `draft_content`, chỉnh thành `final_content`, `status` → `REVIEW`, set `submitted_by`.
2) Staff/Admin review:
- Nếu OK: `status` = `APPROVED`, set `reviewed_by`, `reviewed_at`, `published_at`.
- Gửi notification cho Parent/Student (Zalo/Email).

---

## 3. A7 – OCR biên lai / chứng từ

**Mục tiêu:** hỗ trợ nhập nhanh payments/cashbook từ ảnh biên lai.

### 3.1 Flow backend

1) User upload ảnh biên lai trên màn hình nhập quỹ hoặc xác nhận payment.
2) BE gửi file đến `POST /a7/extract-payment-proof`:
- `direction` = `"IN"` hoặc `"OUT"`.
- `branch_id` theo branch của user.

3) Nhận `PaymentProofExtractResponse`:
- `fields` (direction, branch_id, transaction_datetime, amount, currency, bank_name, transaction_id, content, sender/receiver...).
- `confidence` (per field).
- `raw_text`, `warnings[]`.

### 3.2 Mapping vào DB

- Hóa đơn + payment:
  - Nếu user đang đứng trên 1 `invoice`:
    - Gợi ý `amount`, `paid_at` (`transaction_datetime`), `reference_code` (`transaction_id`) để tạo `payments`.
- Sổ quỹ:
  - Tạo/điền trước `cashbook_entries`:
    - `type` = direction → `CASH_IN` hoặc `CASH_OUT`.
    - `amount`, `currency`, `entry_date` (`transaction_datetime`), `description` (content + bank info).
    - `branch_id`, `created_by`, `attachment_url` (link ảnh biên lai).
    - **Mới**: `ocr_metadata` (jsonb) = toàn bộ response từ A7 (fields + confidence + raw_text + warnings) để audit/debug.

### 3.3 Acceptance

- Nếu confidence thấp (ví dụ <0.5 cho amount) → hiển thị cảnh báo, yêu cầu user kiểm tra lại trước khi lưu.
- `ocr_metadata` chỉ dùng nội bộ; không expose trực tiếp cho phụ huynh.

---

## 4. A8 – Speaking / Phonics

**Mục tiêu:** chấm speaking/phonics, sinh transcript, feedback và practice plan.

### 4.1 Flow với transcript (analyze-transcript)

1) Homework speaking đã có transcript (hoặc user dán text):
- Chuẩn bị `AnalyzeTranscriptRequest`:
  - `context`: homework_id, student_id, mode ("speaking"/"phonics"), target_words[], expected_text?, instructions?, language.
  - `transcript`: text.

2) Gọi `POST /a8/analyze-transcript`.
3) Nhận `AnalyzeSpeakingResponse`:
- `overall_score`, `pronunciation_score`, `fluency_score`, `accuracy_score`, `phonics_issues[]`, `speaking_issues[]`, `suggestions[]`, `practice_plan[]`, `confidence`, `warnings[]`, và `transcript` (chuẩn hóa).

4) Lưu vào DB:
- `homework_student`:
  - `score` = `overall_score` (hoặc quy ước khác).
  - `ai_feedback` = full JSON response.
  - `ai_version` = model version.
  - `status` = `GRADED`, `graded_at` = now.

### 4.2 Flow với media (analyze-media)

1) Học sinh upload audio/video (attachments).
2) BE gửi file đến `POST /a8/analyze-media` với context tương tự.
3) Agent A8:
- Trích transcript,
- Gọi nội bộ logic giống A8 transcript → trả về cùng schema.
4) Lưu DB giống 4.1.

### 4.3 Acceptance

- Nếu không trích transcript được → `confidence.transcript` thấp, `warnings` có message; BE vẫn lưu `ai_feedback` để teacher xem và có thể override.

---

## 5. Gợi ý implementation (BE)

- Tạo service layer `AiGradingService`, `AiReportsService`, `AiOcrService`, `AiSpeakingService`:
  - Đảm nhận mapping từ entity → request DTO AI và ngược lại.
- Tách config endpoint/timeout/model-version ra file cấu hình (để thay đổi model mà không đổi code nhiều).
- Log mọi call AI (request/response rút gọn + status) ra bảng log riêng hoặc file log để debug khi cần (ngoài scope schema chính).


