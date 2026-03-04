# Luồng Chức năng Schedule (Lịch học) cho Phụ huynh

## 1. Tổng quan

Chức năng **Schedule** cho phụ huynh cho phép phụ huynh xem lịch học của học sinh thuộc tài khoản phụ huynh đó. Mỗi phụ huynh có thể quản lý nhiều học sinh (liên kết qua `ParentStudentLink`), và khi truy cập API, hệ thống sẽ xác định học sinh nào đang được chọn thông qua token (JWT).

## 2. Kiến trúc dữ liệu

### 2.1. Các Entity liên quan

| Entity | Mô tả |
|--------|-------|
| `Profile` | Thông tin người dùng (Parent, Student) |
| `ParentStudentLink` | Bảng liên kết giữa Parent và Student |
| `Class` | Lớp học |
| `ClassEnrollment` | Đăng ký học sinh vào lớp |
| `Session` | Buổi học |
| `Attendance` | Điểm danh |

### 2.2. Quan hệ

```
Parent (Profile)
    │
    └── ParentStudentLink (ParentProfileId)
            │
            └── Student (Profile)
                    │
                    └── ClassEnrollment (StudentProfileId)
                            │
                            └── Class (EnrollmentStatus = Active)
                                    │
                                    └── Sessions (Status != Cancelled)
```

## 3. API Endpoints

### 3.1. Lấy lịch học của học sinh

**Endpoint:**
```
GET /api/parent/timetable
```

**Query Parameters:**
| Parameter | Type | Required | Mô tả |
|-----------|------|----------|-------|
| from | DateTime | No | Ngày bắt đầu lọc (VD: 2025-01-01) |
| to | DateTime | No | Ngày kết thúc lọc (VD: 2025-01-31) |

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
```

**Ví dụ Request:**
```http
GET /api/parent/timetable?from=2025-02-01&to=2025-02-28
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3.2. Lấy overview của phụ huynh (bao gồm thông tin lịch học)

**Endpoint:**
```
GET /api/parent/overview
```

**Query Parameters:**
| Parameter | Type | Required | Mô tả |
|-----------|------|----------|-------|
| classId | Guid | No | Lọc theo lớp |
| sessionId | Guid | No | Lọc theo buổi học |
| fromDate | DateTime | No | Ngày bắt đầu |
| toDate | DateTime | No | Ngày kết thúc |

## 4. Luồng xử lý (Flow)

### 4.1. Sơ đồ luồng

```
┌─────────────────────────────────────────────────────────────┐
│                     REQUEST                                  │
│  GET /api/parent/timetable?from=...&to=...                  │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              AUTHENTICATION & AUTHORIZATION                  │
│  1. Verify JWT Token                                         │
│  2. Extract UserId từ token                                  │
│  3. Extract StudentId từ token (đã chọn student nào)       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                  VALIDATION                                  │
│  1. Kiểm tra StudentId có giá trị                            │
│  2. Kiểm tra Profile tồn tại và Active                       │
│  3. Kiểm tra ProfileType = Student                           │
│  4. Verify Student thuộc về Parent (ParentStudentLink)      │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                  QUERY SESSIONS                              │
│  1. Tìm các Class mà Student đăng ký (EnrollmentStatus=Active)│
│  2. Lọc Sessions theo:                                        │
│     - ClassId trong danh sách Class của Student               │
│     - Status != SessionStatus.Cancelled                      │
│     - PlannedDatetime >= query.From (nếu có)                 │
│     - PlannedDatetime <= query.To (nếu có)                  │
│  3. Sắp xếp theo PlannedDatetime                             │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                  RESPONSE                                    │
│  Trả về danh sách các buổi học với thông tin chi tiết        │
└─────────────────────────────────────────────────────────────┘
```

### 4.2. Chi tiết từng bước

#### B1: Authentication
ước - JWT Token được gửi kèm trong request header
- Token chứa thông tin: `UserId`, `StudentId` (học sinh đang được chọn)

#### Bước 2: Lấy thông tin Student từ Context
```csharp
var studentId = userContext.StudentId; // Lấy từ JWT token
var userId = userContext.UserId;
```

#### Bước 3: Validate Student
- Kiểm tra `studentId` có giá trị
- Kiểm tra Profile tồn tại
- Kiểm tra ProfileType = Student
- Kiểm tra Profile.IsActive = true và IsDeleted = false

#### Bước 4: Verify Parent-Student Relationship
```csharp
// Kiểm tra student thuộc về parent qua ParentStudentLink
var isLinked = await context.ParentStudentLinks
    .AnyAsync(link => link.ParentProfileId == parentProfile.Id && 
                     link.StudentProfileId == selectedStudentId.Valueước );
```

#### B5: Query Sessions
```csharp
var sessionsQuery = context.Sessions
    // Chỉ lấy các session của lớp mà student đang học (Active)
    ..Class.ClassEnrollments
        .Where(s => sAny(ce => ce.StudentProfileId == studentId.Value && 
                   ce.Status == EnrollmentStatus.Active))
    // Không lấy các session đã bị hủy
    .Where(s => s.Status != SessionStatus.Cancelled);
```

#### Bước 6: Filter by Date Range
```csharp
if (query.From.HasValue)
{
    sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime >= fromUtc);
}
if (query.To.HasValue)
{
    sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime <= toUtc);
}
```

#### Bước 7: Select và Return
```csharp
.Select(s => new TimetableItemDto
{
    Id = s.Id,
    ClassId = s.ClassId,
    ClassCode = s.Class.Code,
    ClassTitle = s.Class.Title,
    PlannedDatetime = s.PlannedDatetime,
    ActualDatetime = s.ActualDatetime,
    DurationMinutes = s.DurationMinutes,
    ParticipationType = s.ParticipationType,
    Status = s.Status,
    PlannedRoomName = s.PlannedRoom?.Name,
    ActualRoomName = s.ActualRoom?.Name,
    PlannedTeacherName = s.PlannedTeacher?.Name,
    ActualTeacherName = s.ActualTeacher?.Name,
    LessonPlanLink = s.LessonPlan != null ? $"/api/lesson-plans/{s.LessonPlan.Id}" : null
})
```

## 5. Response Model

### 5.1. TimetableResponse

```json
{
  "sessions": [
    {
      "id": "guid",
      "classId": "guid",
      "classCode": "LOP001",
      "classTitle": "Toán Lớp 1 - Buổi Sáng",
      "plannedDatetime": "2025-02-10T08:00:00Z",
      "actualDatetime": "2025-02-10T08:05:00Z",
      "durationMinutes": 90,
      "participationType": "Offline",
      "status": "Scheduled",
      "plannedRoomId": "guid",
      "plannedRoomName": "Phòng 101",
      "actualRoomId": "guid",
      "actualRoomName": "Phòng 101",
      "plannedTeacherId": "guid",
      "plannedTeacherName": "Nguyễn Văn A",
      "actualTeacherId": "guid",
      "actualTeacherName": "Nguyễn Văn A",
      "plannedAssistantId": null,
      "plannedAssistantName": null,
      "lessonPlanId": "guid",
      "lessonPlanLink": "/api/lesson-plans/guid"
    }
  ]
}
```

### 5.2. Error Responses

| Status Code | Mô tả | Ví dụ |
|-------------|-------|-------|
| 400 | Bad Request - Thiếu hoặc sai parameter | |
| 401 | Unauthorized - Token không hợp lệ | |
| 403 | Forbidden - Student không thuộc về Parent | |
| 404 | Not Found - Student không tồn tại | |

```json
// 403 Forbidden Example
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Forbidden",
  "status": 403,
  "errors": {
    "Student": ["Student not linked to this parent"]
  }
}
```

## 6. Bảo mật (Security)

### 6.1. Authorization Rules

1. **Parent chỉ xem được lịch học của các con mình:**
   - Phải có `ParentStudentLink` với Student
   - Student phải có `UserId` trùng với `UserId` trong token

2. **Student context trong token:**
   - Khi đăng nhập, parent có thể chọn 1 student để làm việc
   - `StudentId` được lưu trong JWT token
   - Tất cả API của parent đều dùng `StudentId` này để lọc dữ liệu

### 6.2. Ví dụ JWT Payload

```json
{
  "sub": "user-id-123",
  "userId": "user-id-123",
  "studentId": "student-id-456",  // Student đang được chọn
  "role": "Parent",
  "exp": 1706789100
}
```

## 7. Các trường hợp sử dụng (Use Cases)

| STT | Use Case | API Endpoint | Mô tả |
|-----|----------|--------------|-------|
| 1 | Xem lịch học tuần này | `GET /api/parent/timetable` | Không cần from/to, mặc định lấy tất cả |
| 2 | Xem lịch học trong khoảng thời gian | `GET /api/parent/timetable?from=2025-02-01&to=2025-02-28` | Lọc theo ngày |
| 3 | Xem lịch học chi tiết | `GET /api/sessions/{sessionId}` | Xem chi tiết 1 buổi học |
| 4 | Xem overview + lịch học | `GET /api/parent/overview` | Bao gồm thống kê và lịch học sắp tới |

## 8. Lưu ý quan trọng

1. **Một Parent có nhiều Student:** Khi parent đăng nhập, hệ thống sẽ yêu cầu chọn 1 student để xem dữ liệu. `StudentId` được lưu trong token.

2. **Enrollment Status:** Chỉ những lớp học có `EnrollmentStatus = Active` mới hiển thị trong lịch.

3. **Session Status:** Các buổi học có `Status = Cancelled` sẽ không hiển thị.

4. **Timezone:** Thời gian trả về là UTC. Client cần convert sang timezone local.

5. **Phân trang:** API timetable hiện tại không hỗ trợ phân trang, trả về tất cả các session trong khoảng thời gian.


