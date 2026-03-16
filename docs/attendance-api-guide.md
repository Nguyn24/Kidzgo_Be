# API Hướng Dẫn Sử Dụng - Điểm Danh (Attendance)

## Mục lục
- [Tổng quan](#tổng-quan)
- [Các API Endpoints](#các-api-endpoints)
  - [1. Điểm danh học sinh (UC-099)](#1-điểm-danh-học-sinh-uc-099)
  - [2. Lấy danh sách điểm danh của Session (UC-100)](#2-lấy-danh-sách-điểm-danh-của-session-uc-100)
  - [3. Lịch sử điểm danh học sinh (UC-101)](#3-lịch-sử-điểm-danh-học-sinh-uc-101)
  - [4. Cập nhật điểm danh (UC-104)](#4-cập-nhật-điểm-danh-uc-104)
- [Mã lỗi và thông báo](#mã-lỗi-và-thông-báo)
- [Luồng xử lý](#luồng-xử-lý)
- [Ví dụ sử dụng](#ví-dụ-sử-dụng)

---

## Tổng quan

API điểm danh cho phép giáo viên và nhân viên quản lý việc điểm danh học sinh theo từng buổi học (session). Hệ thống hỗ trợ các trạng thái điểm danh:
- **Present**: Có mặt
- **Absent**: Vắng mặt
- **Makeup**: Học bù
- **NotMarked**: Chưa điểm danh

### Base URL
```
https://api.kidzgo.com/api/attendance
```

### Authentication
Tất cả các API điểm danh đều yêu cầu xác thực bằng Bearer Token.

```bash
Authorization: Bearer <your_token>
```

---

## Các API Endpoints

### 1. Điểm danh học sinh (UC-099)

Điểm danh nhiều học sinh cùng lúc cho một buổi học.

**Endpoint:**
```http
POST /api/attendance/{sessionId}
```

**Path Parameters:**
| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| sessionId | GUID | ID của buổi học (session) |

**Request Body:**
```json
{
  "attendances": [
    {
      "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
      "attendanceStatus": 0,
      "note": "Đến muộn 5 phút"
    },
    {
      "studentProfileId": "550e8400-e29b-41d4-a716-446655440002",
      "attendanceStatus": 1,
      "note": "Xin nghỉ có phép"
    }
  ]
}
```

**Chi tiết Request:**
| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|-------|
| attendances | List | Có | Danh sách học sinh được điểm danh |
| studentProfileId | GUID | Có | ID hồ sơ học sinh |
| attendanceStatus | Enum | Có | Trạng thái điểm danh (0=Present, 1=Absent, 2=Makeup, 3=NotMarked) |
| note | String | Không | Ghi chú thêm |

**AttendanceStatus Values:**
| Giá trị | Tên | Mô tả |
|---------|-----|-------|
| 0 | Present | Có mặt |
| 1 | Absent | Vắng mặt |
| 2 | Makeup | Học bù |
| 3 | NotMarked | Chưa điểm danh |

**Response thành công (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "markedCount": 2,
    "attendances": [
      {
        "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
        "attendanceStatus": 0,
        "note": "Đến muộn 5 phút"
      }
    ]
  }
}
```

---

### 2. Lấy danh sách điểm danh của Session (UC-100)

Lấy toàn bộ danh sách điểm danh của một buổi học.

**Endpoint:**
```http
GET /api/attendance/{sessionId}
```

**Path Parameters:**
| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| sessionId | GUID | ID của buổi học (session) |

**Response thành công (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "sessionName": "Toán - Lớp 1A",
    "date": "2026-02-03",
    "startTime": "08:00",
    "endTime": "09:30",
    "totalStudents": 25,
    "presentCount": 20,
    "absentCount": 3,
    "makeupCount": 1,
    "notMarkedCount": 1,
    "attendances": [
      {
        "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
        "studentName": "Nguyễn Văn A",
        "attendanceStatus": 0,
        "note": null,
        "markedAt": "2026-02-03T08:15:00Z"
      }
    ]
  }
}
```

---

### 3. Lịch sử điểm danh học sinh (UC-101)

Lấy lịch sử điểm danh của học sinh hiện tại (lấy từ token).

**Endpoint:**
```http
GET /api/attendance/students
```

**Query Parameters:**
| Tham số | Kiểu | Mặc định | Mô tả |
|---------|------|----------|-------|
| pageNumber | Int | 1 | Số trang |
| pageSize | Int | 10 | Số lượng bản ghi/trang |

**Response thành công (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "sessionId": "550e8400-e29b-41d4-a716-446655440000",
        "sessionName": "Toán - Lớp 1A",
        "date": "2026-02-03",
        "startTime": "08:00",
        "className": "Lớp 1A",
        "attendanceStatus": 0,
        "note": null,
        "markedAt": "2026-02-03T08:15:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 45,
    "totalPages": 5
  }
}
```

---

### 4. Cập nhật điểm danh (UC-104)

Cập nhật trạng thái điểm danh của một học sinh trong buổi học.

**Endpoint:**
```http
PUT /api/attendance/{sessionId}/students/{studentProfileId}
```

**Path Parameters:**
| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| sessionId | GUID | ID của buổi học (session) |
| studentProfileId | GUID | ID hồ sơ học sinh |

**Request Body:**
```json
{
  "attendanceStatus": 2,
  "note": "Học bù buổi trước"
}
```

**Response thành công (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
    "attendanceStatus": 2,
    "note": "Học bù buổi trước",
    "updatedAt": "2026-02-03T10:30:00Z"
  }
}
```

---

## Mã lỗi và thông báo

| Mã lỗi | Mô tả |
|---------|-------|
| 400 | Dữ liệu request không hợp lệ |
| 401 | Chưa xác thực (token không hợp lệ) |
| 403 | Không có quyền thực hiện thao tác |
| 404 | Session hoặc học sinh không tồn tại |
| 500 | Lỗi server |

### Các lỗi nghiệp vụ (Business Errors):

| Error Code | Message | Mô tả |
|------------|---------|-------|
| SESSION_NOT_FOUND | Session not found | Buổi học không tồn tại |
| STUDENT_NOT_IN_SESSION | Student not enrolled in this session | Học sinh không đăng ký buổi học này |
| ATTENDANCE_ALREADY_MARKED | Attendance already marked | Điểm danh đã được thực hiện |
| CANNOT_UPDATE_FUTURE_SESSION | Cannot update attendance for future session | Không thể cập nhật điểm danh cho buổi học tương lai |
| INVALID_ATTENDANCE_STATUS | Invalid attendance status | Trạng thái điểm danh không hợp lệ |

---

## Luồng xử lý

### Luồng 1: Điểm danh hàng loạt

```
1. Giáo viên đăng nhập hệ thống
2. Chọn buổi học cần điểm danh
3. Hệ thống hiển thị danh sách học sinh đăng ký
4. Giáo viên đánh dấu trạng thái cho từng học sinh
5. Gửi request POST /api/attendance/{sessionId}
6. Hệ thống lưu điểm danh và trả về kết quả
7. (Tự động) Cập nhật streak điểm danh gamification
```

### Luồng 2: Cập nhật điểm danh

```
1. Phát hiện cần sửa điểm danh (sai trạng thái, thêm ghi chú...)
2. Gọi API PUT /api/attendance/{sessionId}/students/{studentProfileId}
3. Hệ thống xác thực và cập nhật
4. Trả về kết quả cập nhật
```

### Luồng 3: Phụ huynh xem lịch sử điểm danh

```
1. Phụ huynh đăng nhập app
2. Gọi API GET /api/attendance/students
3. Hệ thống lấy studentId từ token
4. Trả về lịch sử điểm danh của con
```

---

## Ví dụ sử dụng

### Curl - Điểm danh hàng loạt

```bash
curl -X POST "https://api.kidzgo.com/api/attendance/550e8400-e29b-41d4-a716-446655440000" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your_token>" \
  -d '{
    "attendances": [
      {
        "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
        "attendanceStatus": 0,
        "note": "Đến đúng giờ"
      },
      {
        "studentProfileId": "550e8400-e29b-41d4-a716-446655440002",
        "attendanceStatus": 1,
        "note": "Xin nghỉ có phép"
      }
    ]
  }'
```

### Curl - Lấy danh sách điểm danh

```bash
curl -X GET "https://api.kidzgo.com/api/attendance/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer <your_token>"
```

### Curl - Cập nhật điểm danh

```bash
curl -X PUT "https://api.kidzgo.com/api/attendance/550e8400-e29b-41d4-a716-446655440000/students/550e8400-e29b-41d4-a716-446655440001" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your_token>" \
  -d '{
    "attendanceStatus": 2,
    "note": "Học bù buổi trước"
  }'
```

### JavaScript (Fetch API)

```javascript
// Điểm danh hàng loạt
async function markAttendance(sessionId, attendances) {
  const response = await fetch(`/api/attendance/${sessionId}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ attendances })
  });
  return await response.json();
}

// Lấy danh sách điểm danh
async function getSessionAttendance(sessionId) {
  const response = await fetch(`/api/attendance/${sessionId}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
}

// Lấy lịch sử điểm danh của học sinh
async function getMyAttendanceHistory(pageNumber = 1, pageSize = 10) {
  const response = await fetch(`/api/attendance/students?pageNumber=${pageNumber}&pageSize=${pageSize}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
}

// Cập nhật điểm danh
async function updateAttendance(sessionId, studentProfileId, status, note) {
  const response = await fetch(`/api/attendance/${sessionId}/students/${studentProfileId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ 
      attendanceStatus: status,
      note: note 
    })
  });
  return await response.json();
}
```

---

## Lưu ý quan trọng

1. **Thời gian điểm danh**: Chỉ nên điểm danh khi buổi học đã bắt đầu hoặc sau khi kết thúc.
2. **Quyền hạn chế**: Chỉ Admin và giáo viên mới có quyền điểm danh và cập nhật.
3. **Gamification**: Khi điểm danh Present, hệ thống tự động cập nhật streak điểm danh (nếu có).
4. **Audit Log**: Tất cả thao tác điểm danh đều được ghi log để theo dõi.
5. **Concurrent**: Hệ thống hỗ trợ điểm danh đồng thời từ nhiều thiết bị.

---

## Liên hệ hỗ trợ

- Email: support@kidzgo.com
- Hotline: 1900 xxxx
- Slack: #api-support

