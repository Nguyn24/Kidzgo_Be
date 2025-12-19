# Flow Tính Lương (Payroll Flow)

Tài liệu này mô tả chi tiết flow tính lương từ đầu đến cuối qua tất cả các bảng liên quan trong hệ thống KidzGo.

---

## 📋 Tổng quan

Hệ thống tính lương hỗ trợ 2 loại nhân sự:
- **Giáo viên (Teachers)**: Lương dựa trên số buổi dạy và giờ dạy từ `sessions`
- **Staff**: Lương dựa trên base salary và giờ làm từ `shift_attendance`

Tất cả đều có thể có **overtime** nếu vượt quá `minimum_monthly_hours` trong hợp đồng.

---

## 🔗 Các bảng liên quan

### 1. Bảng cơ sở (Base Tables)

#### `contracts` - Hợp đồng lao động
- **Mục đích**: Lưu thông tin hợp đồng và các thông số tính lương
- **Fields quan trọng**:
  - `staff_user_id`: Staff/Teacher
  - `contract_type`: PROBATION/FIXED_TERM/INDEFINITE/PART_TIME
  - `base_salary`: Lương cơ bản (full-time)
  - `hourly_rate`: Lương theo giờ (part-time)
  - `allowance_fixed`: Phụ cấp cố định
  - `minimum_monthly_hours`: Số giờ làm tối thiểu mỗi tháng
  - `overtime_rate_multiplier`: Hệ số nhân lương overtime (1.5x, 2x, ...)
  - `social_insurance_salary`: Mức lương đóng BHXH

#### `sessions` - Buổi học
- **Mục đích**: Lưu thông tin buổi học (cho giáo viên)
- **Fields quan trọng**:
  - `duration_minutes`: Thời lượng buổi học
  - `status`: SCHEDULED/COMPLETED/CANCELLED
  - `planned_datetime`, `actual_datetime`: Thời gian buổi học

#### `session_roles` - Vai trò trong buổi học
- **Mục đích**: Lưu chi tiết ai dạy buổi nào với đơn giá cụ thể
- **Fields quan trọng**:
  - `session_id`: Buổi học
  - `staff_user_id`: Giáo viên/Staff tham gia
  - `role`: MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP
  - `payable_unit_price`: Đơn giá trả cho vai trò này
  - `payable_allowance`: Phụ cấp (nếu có)

#### `shift_attendance` - Chấm công ca làm việc
- **Mục đích**: Lưu giờ làm việc của Staff (không phải giáo viên)
- **Fields quan trọng**:
  - `staff_user_id`: Staff
  - `contract_id`: Hợp đồng
  - `shift_date`: Ngày ca
  - `shift_hours`: Số giờ làm việc
  - `approved_by`, `approved_at`: Duyệt chấm công

### 2. Bảng tổng hợp (Aggregation Tables)

#### `monthly_work_hours` - Giờ làm việc hàng tháng
- **Mục đích**: Tổng hợp giờ làm việc trong tháng để tính overtime
- **Fields quan trọng**:
  - `staff_user_id`: Staff/Teacher
  - `contract_id`: Hợp đồng
  - `year`, `month`: Tháng/năm
  - `total_hours`: Tổng số giờ làm
  - `teaching_hours`: Số giờ dạy (từ sessions, cho giáo viên)
  - `regular_hours`: Số giờ làm thường (từ shift_attendance, cho staff)
  - `overtime_hours`: Số giờ làm thêm = `total_hours - minimum_monthly_hours` (nếu > 0)
  - `teaching_sessions`: Số buổi dạy (cho giáo viên)
  - `is_locked`: Đã khóa chưa (sau khi dùng để tính lương)

### 3. Bảng Payroll (Payroll Tables)

#### `payroll_runs` - Kỳ lương
- **Mục đích**: Đại diện cho một kỳ tính lương (thường là hàng tháng)
- **Fields quan trọng**:
  - `period_start`, `period_end`: Khoảng thời gian kỳ lương
  - `branch_id`: Chi nhánh
  - `status`: DRAFT/APPROVED/PAID
  - `approved_by`, `paid_at`: Duyệt và thanh toán

#### `payroll_lines` - Chi tiết lương
- **Mục đích**: Chi tiết từng thành phần lương của mỗi nhân viên
- **Fields quan trọng**:
  - `payroll_run_id`: Kỳ lương
  - `staff_user_id`: Staff/Teacher
  - `component_type`: TEACHING/TA/CLUB/WORKSHOP/BASE/OVERTIME/ALLOWANCE/DEDUCTION
  - `source_id`: ID nguồn dữ liệu:
    - `session_roles.id` cho TEACHING/TA/CLUB/WORKSHOP
    - `contracts.id` cho BASE
    - `monthly_work_hours.id` cho OVERTIME
    - `null` hoặc expense_id cho ALLOWANCE/DEDUCTION
  - `amount`: Số tiền
  - `description`: Mô tả
  - `is_paid`, `paid_at`: Trạng thái thanh toán

#### `payroll_payments` - Thanh toán lương
- **Mục đích**: Ghi nhận thanh toán lương cho nhân viên
- **Fields quan trọng**:
  - `payroll_run_id`: Kỳ lương
  - `staff_user_id`: Staff/Teacher
  - `amount`: Số tiền thanh toán
  - `method`: BANK_TRANSFER/CASH
  - `paid_at`: Thời gian thanh toán
  - `cashbook_entry_id`: Liên kết với sổ quỹ

### 4. Bảng Finance (Finance Tables)

#### `cashbook_entries` - Sổ quỹ
- **Mục đích**: Ghi nhận tất cả giao dịch thu/chi
- **Fields quan trọng**:
  - `type`: CASH_IN/CASH_OUT
  - `related_type`: INVOICE/PAYROLL/EXPENSE/ADJUSTMENT
  - `related_id`: ID của bảng liên quan (payroll_payment_id khi related_type=PAYROLL)
  - `amount`: Số tiền
  - `entry_date`: Ngày giao dịch

---

## 🔄 Flow Tính Lương Chi Tiết

### Phase 1: Thu thập dữ liệu (Data Collection)

#### 1.1. Cho Giáo viên (Teachers)

**Bước 1**: Khi buổi học hoàn thành (`sessions.status = COMPLETED`)
- Tạo hoặc cập nhật `session_roles` cho mỗi người tham gia:
  - `staff_user_id`: Giáo viên
  - `role`: MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP
  - `payable_unit_price`: Đơn giá (có thể lấy từ contract hoặc set thủ công)
  - `payable_allowance`: Phụ cấp (nếu có)

**Bước 2**: Tổng hợp giờ làm hàng tháng
- Tạo/update `monthly_work_hours`:
  - Tính `teaching_hours`: Tổng hợp từ `sessions` (đã COMPLETED) thông qua `session_roles` trong tháng
  - Tính `total_hours = teaching_hours`
  - Tính `overtime_hours = total_hours - minimum_monthly_hours` (nếu > 0, ngược lại = 0)

#### 1.2. Cho Staff

**Bước 1**: Chấm công hàng ngày
- Tạo `shift_attendance`:
  - `staff_user_id`: Staff
  - `shift_date`: Ngày làm việc
  - `shift_hours`: Số giờ làm
  - `approved_by`: Người duyệt

**Bước 2**: Tổng hợp giờ làm hàng tháng
- Tạo/update `monthly_work_hours`:
  - Tính `regular_hours`: Tổng hợp từ `shift_attendance` (đã được approved) trong tháng
  - Tính `total_hours = regular_hours`
  - Tính `overtime_hours = total_hours - minimum_monthly_hours` (nếu > 0, ngược lại = 0)

**Bước 3**: Lock monthly_work_hours
- Set `is_locked = true` khi đã xác nhận đúng, không cho sửa nữa

---

### Phase 2: Tạo kỳ lương (Create Payroll Run)

**Bước 1**: Tạo `payroll_runs`
- Tạo record mới với:
  - `period_start`: Ngày đầu tháng (ví dụ: 2024-01-01)
  - `period_end`: Ngày cuối tháng (ví dụ: 2024-01-31)
  - `branch_id`: Chi nhánh
  - `status`: DRAFT

**Bước 2**: Tính lương cho từng nhân viên

#### 2.1. Tính lương cho Giáo viên

**a) Lương từ giờ dạy (TEACHING/TA/CLUB/WORKSHOP)**
- Tạo `payroll_lines` từ `session_roles`:
  - Lấy tất cả `session_roles` của giáo viên trong kỳ lương (sessions đã COMPLETED)
  - Map `role` sang `component_type`:
    - MAIN_TEACHER → TEACHING
    - ASSISTANT → TA
    - CLUB → CLUB
    - WORKSHOP → WORKSHOP
  - `source_id` = `session_roles.id`
  - `amount` = `payable_unit_price + payable_allowance`
  - `description` = Mô tả buổi học và vai trò

**b) Lương overtime (OVERTIME)**
- Tạo `payroll_line` từ `monthly_work_hours`:
  - Lấy `monthly_work_hours` của giáo viên trong tháng (đã locked, có overtime_hours > 0)
  - `component_type` = OVERTIME
  - `source_id` = `monthly_work_hours.id`
  - `amount` = `overtime_hours × hourly_rate × overtime_rate_multiplier` (từ contract)
  - `description` = Mô tả số giờ overtime

**c) Phụ cấp cố định (ALLOWANCE)**
- Tạo `payroll_line` từ `contracts`:
  - Lấy `allowance_fixed` từ contract (nếu > 0 và contract đang active)
  - `component_type` = ALLOWANCE
  - `source_id` = `contracts.id`
  - `amount` = `allowance_fixed`
  - `description` = "Phụ cấp cố định"

#### 2.2. Tính lương cho Staff

**a) Lương cơ bản (BASE)**
- Tạo `payroll_line` từ `contracts`:
  - Nếu có `base_salary`: `amount` = `base_salary`
  - Nếu không có `base_salary`: `amount` = `regular_hours × hourly_rate` (từ monthly_work_hours)
  - `component_type` = BASE
  - `source_id` = `contracts.id`
  - `description` = "Lương cơ bản" hoặc "Lương theo giờ: X giờ"

**b) Lương overtime (OVERTIME)**
- Tương tự như giáo viên:
  - Lấy `monthly_work_hours` (đã locked, có overtime_hours > 0)
  - `amount` = `overtime_hours × hourly_rate × overtime_rate_multiplier`

**c) Phụ cấp cố định (ALLOWANCE)**
- Tương tự như giáo viên: Lấy từ `contracts.allowance_fixed`

**d) Khấu trừ (DEDUCTION)**
- Nếu có khấu trừ (ví dụ: nghỉ không phép, vi phạm):
  - `component_type` = DEDUCTION
  - `source_id` = NULL hoặc expense_id nếu có
  - `amount` = Số tiền khấu trừ
  - `description` = Lý do khấu trừ

---

### Phase 3: Duyệt kỳ lương (Approve Payroll Run)

**Bước 1**: Review và approve
- Cập nhật `payroll_runs`:
  - `status` = APPROVED
  - `approved_by` = ID người duyệt

**Lưu ý**: Sau khi approve, không nên sửa `payroll_lines` nữa để đảm bảo tính nhất quán.

---

### Phase 4: Thanh toán lương (Pay Salary)

**Bước 1**: Tạo `payroll_payments`
- Tính tổng lương cho mỗi nhân viên: SUM(`amount`) từ `payroll_lines` trong kỳ lương
- Tạo `payroll_payment`:
  - `payroll_run_id`: Kỳ lương
  - `staff_user_id`: Nhân viên
  - `amount`: Tổng lương
  - `method`: BANK_TRANSFER hoặc CASH
  - `paid_at`: Thời gian thanh toán

**Bước 2**: Ghi vào sổ quỹ (`cashbook_entries`)
- Tạo `cashbook_entry`:
  - `type` = CASH_OUT (chi trả lương)
  - `amount` = Tổng lương
  - `related_type` = PAYROLL
  - `related_id` = `payroll_payment_id`
  - `description` = Mô tả trả lương cho nhân viên và kỳ lương
  - `entry_date` = Ngày thanh toán

**Bước 3**: Cập nhật `payroll_payments` với `cashbook_entry_id`
- Link `payroll_payment` với `cashbook_entry` vừa tạo

**Bước 4**: Cập nhật `payroll_lines` là đã thanh toán
- Đánh dấu tất cả `payroll_lines` của nhân viên:
  - `is_paid` = true
  - `paid_at` = Thời gian thanh toán

**Bước 5**: Đánh dấu kỳ lương đã thanh toán
- Cập nhật `payroll_runs`:
  - `status` = PAID
  - `paid_at` = Thời gian thanh toán

---

## 📊 Sơ đồ Flow

```
┌─────────────────┐
│   contracts      │ ────┐
│  (Hợp đồng)      │     │
└─────────────────┘     │
                        │
┌─────────────────┐     │
│   sessions      │     │
│  (Buổi học)     │     │
└─────────────────┘     │
        │                │
        ▼                │
┌─────────────────┐     │
│ session_roles  │     │
│ (Vai trò dạy)   │     │
└─────────────────┘     │
        │                │
        │                │
┌─────────────────┐     │
│shift_attendance│     │
│ (Chấm công)     │     │
└─────────────────┘     │
        │                │
        │                │
        ▼                │
┌─────────────────┐     │
│monthly_work_    │     │
│hours            │     │
│ (Tổng hợp giờ)  │     │
└─────────────────┘     │
        │                │
        │                │
        ▼                │
┌─────────────────┐     │
│ payroll_runs    │◄────┘
│ (Kỳ lương)      │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│ payroll_lines   │
│ (Chi tiết lương)│
└─────────────────┘
        │
        ▼
┌─────────────────┐
│payroll_payments │
│ (Thanh toán)    │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│cashbook_entries │
│ (Sổ quỹ)        │
└─────────────────┘
```

---

## 🔍 Ví dụ cụ thể

### Ví dụ 1: Tính lương giáo viên tháng 1/2024

**Thông tin**:
- Giáo viên: Nguyễn Văn A
- Contract: `hourly_rate = 200,000 VND`, `minimum_monthly_hours = 80`, `overtime_rate_multiplier = 1.5`
- Tháng 1/2024: Dạy 20 buổi, mỗi buổi 1.5 giờ = 30 giờ
- Có 5 buổi dạy với vai trò MAIN_TEACHER, `payable_unit_price = 300,000 VND/buổi`

**Tính toán**:

1. **Teaching hours**: 30 giờ
2. **Teaching pay** (từ session_roles):
   - 5 buổi × 300,000 = 1,500,000 VND
   - 15 buổi khác × 200,000 = 3,000,000 VND
   - Tổng: 4,500,000 VND
3. **Overtime**: Không có (30 < 80)
4. **Allowance**: 500,000 VND (từ contract)
5. **Tổng lương**: 4,500,000 + 500,000 = 5,000,000 VND

**Payroll Lines**:
- Line 1: `component_type = TEACHING`, `amount = 4,500,000`
- Line 2: `component_type = ALLOWANCE`, `amount = 500,000`

### Ví dụ 2: Tính lương Staff có overtime

**Thông tin**:
- Staff: Trần Thị B
- Contract: `base_salary = 10,000,000 VND`, `minimum_monthly_hours = 160`, `overtime_rate_multiplier = 1.5`
- Tháng 1/2024: Làm 180 giờ (từ shift_attendance)

**Tính toán**:

1. **Regular hours**: 160 giờ (minimum)
2. **Overtime hours**: 180 - 160 = 20 giờ
3. **Base salary**: 10,000,000 VND
4. **Overtime pay**: 
   - Cần tính `hourly_rate` từ base_salary: `hourly_rate = 10,000,000 / 160 = 62,500 VND/giờ`
   - Overtime: 20 × 62,500 × 1.5 = 1,875,000 VND
5. **Tổng lương**: 10,000,000 + 1,875,000 = 11,875,000 VND

**Payroll Lines**:
- Line 1: `component_type = BASE`, `amount = 10,000,000`
- Line 2: `component_type = OVERTIME`, `amount = 1,875,000`, `source_id = monthly_work_hours.id`

---

## ⚠️ Lưu ý quan trọng

1. **Lock mechanism**: `monthly_work_hours.is_locked = true` sau khi dùng để tính lương, đảm bảo dữ liệu không bị thay đổi
2. **Audit trail**: `payroll_lines.source_id` giúp trace lại nguồn gốc của từng khoản lương
3. **Approval workflow**: `payroll_runs.status` phải qua DRAFT → APPROVED → PAID
4. **Cashbook integration**: Mỗi `payroll_payment` phải có `cashbook_entry` tương ứng để đảm bảo sổ quỹ chính xác
5. **Overtime calculation**: Chỉ tính khi `total_hours > minimum_monthly_hours`
6. **Component types**: 
   - TEACHING/TA/CLUB/WORKSHOP: Từ `session_roles`
   - BASE: Từ `contracts` (base_salary hoặc hourly_rate × regular_hours)
   - OVERTIME: Từ `monthly_work_hours`
   - ALLOWANCE/DEDUCTION: Manual hoặc từ các nguồn khác

---

## 🔗 Foreign Key Relationships

```
contracts
  ├── staff_user_id → users.id
  └── branch_id → branches.id

sessions
  └── class_id → classes.id

session_roles
  ├── session_id → sessions.id
  └── staff_user_id → users.id

shift_attendance
  ├── staff_user_id → users.id
  └── contract_id → contracts.id

monthly_work_hours
  ├── staff_user_id → users.id
  ├── contract_id → contracts.id
  └── branch_id → branches.id

payroll_runs
  └── branch_id → branches.id

payroll_lines
  ├── payroll_run_id → payroll_runs.id
  ├── staff_user_id → users.id
  └── source_id → session_roles.id | contracts.id | monthly_work_hours.id

payroll_payments
  ├── payroll_run_id → payroll_runs.id
  ├── staff_user_id → users.id
  └── cashbook_entry_id → cashbook_entries.id

cashbook_entries
  ├── branch_id → branches.id
  └── related_id → payroll_payments.id (khi related_type = PAYROLL)
```

---

## 📝 Summary

Flow tính lương trong KidzGo:

1. **Thu thập dữ liệu**: Từ `sessions`/`session_roles` (giáo viên) hoặc `shift_attendance` (staff)
2. **Tổng hợp**: Vào `monthly_work_hours` để tính overtime
3. **Tạo kỳ lương**: `payroll_runs` (DRAFT)
4. **Tính chi tiết**: `payroll_lines` cho từng thành phần lương
5. **Duyệt**: `payroll_runs` (APPROVED)
6. **Thanh toán**: `payroll_payments` → `cashbook_entries`
7. **Hoàn tất**: `payroll_runs` (PAID)

Tất cả đều có audit trail qua `source_id` và liên kết với `cashbook_entries` để đảm bảo tính minh bạch và chính xác.

