# Luồng Quản lý Đăng ký Học - Registration Flow Guide

## Tổng quan

Tài liệu này hướng dẫn cách sử dụng hệ thống quản lý đăng ký học theo luồng mới. Luồng mới tách rõ hai phần:

1. **Phần thiết lập**: Chương trình → Gói học → Lớp học → Lịch học / Phòng / Giáo viên
2. **Phần nghiệp vụ vận hành**: Đăng ký học viên → Gợi ý lớp phù hợp → Xếp lớp → Học vụ phát sinh

---

## Các thành phần chính

### 1. Chương trình (Program)
Là nội dung đào tạo, ví dụ: Starter, Movers, Flyers

### 2. Gói học (TuitionPlan)
Là gói bán cho học sinh, ví dụ:
- Starter 3 tháng
- Starter 6 tháng
- Starter 12 tháng

Gói học chứa:
- Chương trình
- Thời lượng
- Học phí
- Số buổi chuẩn

### 3. Lớp học (Class)
Là nơi học thật, ví dụ: Starter A, Starter B, Starter Weekend

Lớp học gắn với:
- Chương trình
- Giáo viên
- Chi nhánh
- Lịch học
- Phòng học
- Sĩ số
- Trạng thái hoạt động

**Quan trọng**: Lớp thuộc chương trình, không thuộc gói 3/6/9/12 tháng.

### 4. Đăng ký học (Registration)
Là bản ghi ghi nhận:
- Học viên đăng ký chương trình nào
- Đăng ký gói nào
- Ngày đăng ký
- Ngày bắt đầu học dự kiến
- Nhu cầu ca học
- Chi nhánh
- Trạng thái xếp lớp

---

## Trạng thái Đăng ký (RegistrationStatus)

| Giá trị | Mô tả |
|---------|--------|
| 0 - New | Mới tạo |
| 1 - WaitingForClass | Chờ xếp lớp |
| 2 - ClassAssigned | Đã xếp lớp (chưa bắt đầu học) |
| 3 - Studying | Đang học |
| 4 - Paused | Bảo lưu |
| 5 - Completed | Hoàn thành |
| 6 - Cancelled | Hủy |

---

## Trạng thái Lớp học (ClassStatus)

| Giá trị | Mô tả |
|---------|--------|
| 0 - Planned | Sắp khai giảng |
| 1 - Recruiting | Đang tuyển sinh |
| 2 - Active | Đang học |
| 3 - Full | Đã đầy |
| 4 - Completed | Kết thúc |
| 5 - Suspended | Tạm ngưng |
| 6 - Cancelled | Hủy |

---

## Luồng hoạt động

### Bước 1: Setup ban đầu (Admin)

Admin tạo:
1. Chương trình (Program)
2. Các gói học (TuitionPlan) cho từng chương trình
3. Lớp học (Class) theo chương trình
4. Giáo viên, phòng, lịch học

### Bước 2: Học viên đăng ký

Tạo đăng ký với:
- Học viên
- Chi nhánh
- Chương trình
- Gói học
- Ngày bắt đầu dự kiến
- Nhu cầu lịch học

**API**: POST /api/registrations

### Bước 3: Hệ thống gợi ý lớp

Chỉ gợi ý các lớp:
- Cùng chương trình
- Cùng chi nhánh
- Còn chỗ
- Phù hợp ca học

**API**: GET /api/registrations/{id}/suggest-classes

### Bước 4: Xếp lớp

Admin chọn lớp phù hợp.

Nếu chưa có lớp phù hợp:
- Cho vào chờ xếp lớp
- Hoặc tạo lớp mới khi đủ số lượng

**API**: POST /api/registrations/{id}/assign-class

### Bước 5: Vận hành học vụ

Trong quá trình học có thể phát sinh:
- Vào giữa chừng
- Học bù
- Chuyển lớp
- Bảo lưu
- Nâng gói
- Gia hạn
- Chuyển chi nhánh

---

## API Endpoints

### 1. Tạo đăng ký học

`ash
POST /api/registrations
Content-Type: application/json

{
    \"studentProfileId\": \"uuid\",
    \"branchId\": \"uuid\",
    \"programId\": \"uuid\",
    \"tuitionPlanId\": \"uuid\",
    \"expectedStartDate\": \"2024-01-15T00:00:00Z\",
    \"preferredSchedule\": \"Tối thứ 7\",
    \"note\": \"Ghi chú thêm\"
}
`

### 2. Lấy danh sách đăng ký

`ash
GET /api/registrations?branchId=uuid&programId=uuid&status=New
`

### 3. Lấy chi tiết đăng ký

`ash
GET /api/registrations/{id}
`

### 4. Cập nhật đăng ký

`ash
PUT /api/registrations/{id}
Content-Type: application/json

{
    \"expectedStartDate\": \"2024-02-01T00:00:00Z\",
    \"preferredSchedule\": \"Cuối tuần\",
    \"note\": \"Cập nhật ghi chú\"
}
`

### 5. Hủy đăng ký

`ash
PATCH /api/registrations/{id}/cancel?reason=Lý do hủy
`

### 6. Gợi ý lớp phù hợp

`ash
GET /api/registrations/{id}/suggest-classes
`

Response:
`json
{
    \"registrationId\": \"uuid\",
    \"programName\": \"Starter\",
    \"branchName\": \"Chi nhánh 1\",
    \"preferredSchedule\": \"Tối thứ 7\",
    \"suggestedClasses\": [
        {
            \"id\": \"uuid\",
            \"code\": \"STA001\",
            \"title\": \"Starter A\",
            \"status\": \"Active\",
            \"capacity\": 15,
            \"currentEnrollment\": 10,
            \"remainingSlots\": 5,
            \"startDate\": \"2024-01-20\",
            \"schedulePattern\": \"Tối thứ 7\",
            \"mainTeacherName\": \"Nguyễn Văn A\",
            \"isClassStarted\": false
        }
    ],
    \"alternativeClasses\": []
}
`

### 7. Xếp lớp cho học viên

`ash
POST /api/registrations/{id}/assign-class
Content-Type: application/json

{
    \"classId\": \"uuid\",
    \"entryType\": \"immediate\" // \"immediate\" | \"makeup\" | \"wait\"
}
`

**EntryType**:
- immediate: Vào học ngay
- makeup: Học bù rồi vào lớp
- wait: Chờ lớp mới

### 8. Danh sách chờ xếp lớp

`ash
GET /api/registrations/waiting-list?branchId=uuid&programId=uuid
`

### 9. Chuyển lớp

`ash
POST /api/registrations/{id}/transfer-class?newClassId=uuid&effectiveDate=2024-02-01T00:00:00Z
`

### 10. Nâng gói học

`ash
POST /api/registrations/{id}/upgrade?newTuitionPlanId=uuid
`

---

## Các Case thực tế

### Case 1: Học sinh mới đăng ký và có lớp phù hợp

1. Tạo đăng ký: POST /api/registrations
2. Gợi ý lớp: GET /api/registrations/{id}/suggest-classes
3. Xếp lớp: POST /api/registrations/{id}/assign-class

### Case 2: Học sinh đăng ký giữa chừng khi lớp đang học

1. Tạo đăng ký
2. Gợi ý lớp
3. Xếp lớp với entryType: \"makeup\" nếu cần học bù trước khi vào lớp

### Case 3: Đăng ký nhưng chưa có lớp phù hợp

1. Tạo đăng ký (status: WaitingForClass)
2. Vào danh sách chờ: GET /api/registrations/waiting-list
3. Khi có lớp mới, xếp lớp cho học viên

### Case 4: Học sinh các gói khác nhau học chung một lớp

- Lớp chỉ quản lý chương trình và lịch học
- Từng học sinh giữ gói học riêng, ngày hết hạn riêng

### Case 5: Học sinh học 3 tháng rồi muốn nâng lên 6 tháng

`ash
POST /api/registrations/{id}/upgrade?newTuitionPlanId=uuid
`

- Không đổi lớp
- Không tạo lớp mới
- Tạo registration mới với tuitionplan cao hơn

### Case 6: Học sinh muốn chuyển lớp

`ash
POST /api/registrations/{id}/transfer-class?newClassId=uuid
`

### Case 7: Học sinh xin bảo lưu

Sử dụng PauseEnrollmentRequest (đã có sẵn trong hệ thống)

### Case 8: Lớp đầy nhưng vẫn có học viên đăng ký

1. Vẫn tạo đăng ký
2. Không cho xếp vượt sĩ số
3. Chuyển sang lớp khác hoặc danh sách chờ

---

## Business Rules

### Rule 1
Học viên đăng ký gói học, không đăng ký trực tiếp vào lớp

### Rule 2
Lớp học thuộc chương trình, không thuộc gói 3/6/9/12 tháng

### Rule 3
Một chương trình có nhiều gói học

### Rule 4
Một lớp có thể chứa học viên thuộc các gói khác nhau

### Rule 5
Phải tách ngày đăng ký và ngày vào học thực tế

### Rule 6
Xếp lớp là bước sau đăng ký. Đăng ký có thể tồn tại kể cả khi chưa tìm được lớp phù hợp.

### Rule 7
Khi vào giữa chừng phải có cách nhập học rõ ràng:
- Vào học ngay
- Học bù rồi vào
- Chờ lớp mới

### Rule 8
Các nghiệp vụ như chuyển lớp, bảo lưu, nâng gói không được phá cấu trúc lớp

---

## Tự động hóa

### Khi tạo đăng ký
Hệ thống tự kiểm tra:
- Có lớp cùng chương trình không
- Còn chỗ không
- Đúng chi nhánh không
- Đúng ca học không

### Khi xếp lớp
Hệ thống tự chặn:
- Sai chương trình
- Vượt sĩ số
- Lớp đã kết thúc
- Lớp tạm ngưng

### Khi vào giữa chừng
Hệ thống buộc chọn:
- Học ngay
- Học bù
- Chờ lớp mới

### Khi gần hết gói
Hệ thống cảnh báo:
- Còn ít buổi
- Gần hết hạn

### Khi không có lớp phù hợp
Hệ thống tự đưa sang:
- Danh sách chờ
- Hoặc gợi ý lớp gần nhất

---

## So sánh Luồng cũ và Luồng mới

### Luồng cũ
Khóa học → Lớp học → Phòng học → Lịch học

**Ưu**:
- Đơn giản
- Dễ hình dung
- Dễ CRUD nhanh

**Nhược**:
- Sai trọng tâm nghiệp vụ
- Khó xử lý đăng ký giữa chừng
- Dễ nhầm khóa/chương trình/gói
- Khó nâng gói, bảo lưu, chuyển lớp
- Dễ nổ số lượng khóa/lớp

### Luồng mới
Chương trình → Gói học → Lớp học → Đăng ký học viên → Xếp lớp → Học vụ phát sinh

**Ưu**:
- Sát nghiệp vụ trung tâm
- Xử lý được hầu hết tình huống thực tế
- Linh hoạt cho sale và học vụ
- Dễ mở rộng dài hạn
- Tránh nổ dữ liệu không cần thiết

**Nhược**:
- Khó thiết kế hơn ban đầu
- Cần nhiều rule hơn
- UI/DB phức tạp hơn lúc đầu

---

## Lưu ý quan trọng

1. **Enrollment (ClassEnrollment)**: Vẫn tồn tại nhưng được tự động tạo khi xếp lớp từ Registration
2. **PauseEnrollmentRequest**: Sử dụng cho nghiệp vụ bảo lưu (đã có sẵn)
3. **MakeupCredit**: Sử dụng cho nghiệp vụ học bù (đã có sẵn)

---

## Version

- Ngày tạo: 18/03/2026
- Phiên bản: 1.0
