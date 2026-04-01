# API Hướng Dẫn Sử Dụng - Điểm Danh (Attendance)

Ngày cập nhật: 2026-03-31  
Phạm vi: tài liệu này mô tả module Attendance theo code hiện tại trong `Kidzgo_Be`.

## Mục lục

- [Tổng quan](#tổng-quan)
- [Authentication và Authorization](#authentication-và-authorization)
- [Status và enum](#status-và-enum)
- [API Endpoints](#api-endpoints)
  - [1. Điểm danh học sinh (UC-099)](#1-điểm-danh-học-sinh-uc-099)
  - [2. Lấy danh sách điểm danh của session (UC-100)](#2-lấy-danh-sách-điểm-danh-của-session-uc-100)
  - [3. Lịch sử điểm danh học sinh (UC-101)](#3-lịch-sử-điểm-danh-học-sinh-uc-101)
  - [4. Cập nhật điểm danh (UC-104)](#4-cập-nhật-điểm-danh-uc-104)
- [Response format](#response-format)
- [Mã lỗi và business rule](#mã-lỗi-và-business-rule)
- [Ví dụ sử dụng](#ví-dụ-sử-dụng)
- [Lưu ý quan trọng](#lưu-ý-quan-trọng)

---

## Tổng quan

Module Attendance quản lý điểm danh học sinh theo từng buổi học (`session`).

Hệ thống đang hỗ trợ 4 trạng thái điểm danh:

- `Present`: có mặt
- `Absent`: vắng mặt
- `Makeup`: học bù
- `NotMarked`: chưa điểm danh

Module này còn liên quan trực tiếp tới make-up:

- Khi điểm danh `Absent`, hệ thống sẽ resolve `AbsenceType` dựa trên leave request đã `Approved`.
- Nếu `AbsenceType = WithNotice24H`, hệ thống tự tạo `MakeupCredit` nếu chưa có credit cho buổi đó.
- Khi điểm danh `Present`, hệ thống cập nhật `UsedSessions` và `RemainingSessions` của registration đang học.

### Base path

```http
/api/attendance
```

---

## Authentication và Authorization

Tất cả API đều yêu cầu Bearer Token.

```http
Authorization: Bearer <access_token>
```

### Quyền theo controller

| API | Admin | Teacher | Parent | Student |
| --- | --- | --- | --- | --- |
| `POST /api/attendance/{sessionId}` | Có | Có | Không | Không |
| `GET /api/attendance/{sessionId}` | Có | Có | Không | Không |
| `PUT /api/attendance/{sessionId}/students/{studentProfileId}` | Có | Có | Không | Không |
| `GET /api/attendance/students` | Có | Có | Có | Có |

### Lưu ý về quyền thực tế ở handler

- `GET /api/attendance/students` dù mở role cho `Admin/Teacher/Parent/Student`, nhưng handler vẫn yêu cầu:
  - token có `StudentId`
  - profile tương ứng phải có `ProfileType = Student`
  - profile phải có `UserId == currentUserId`
- Vì vậy endpoint này chỉ hoạt động khi token/current user đáp ứng đúng các điều kiện trên.

---

## Status và enum

### AttendanceStatus

| Value | Name | Ý nghĩa |
| --- | --- | --- |
| `0` | `Present` | Có mặt |
| `1` | `Absent` | Vắng mặt |
| `2` | `Makeup` | Học bù |
| `3` | `NotMarked` | Chưa điểm danh |

### AbsenceType

| Name | Ý nghĩa |
| --- | --- |
| `WithNotice24H` | Nghỉ có báo trước >= 24h |
| `Under24H` | Nghỉ có báo trước nhưng < 24h |
| `NoNotice` | Nghỉ không báo trước |
| `LongTerm` | Có enum trong domain nhưng hiện không được set trong attendance flow hiện tại |

---

## API Endpoints

### 1. Điểm danh học sinh (UC-099)

Điểm danh nhiều học sinh trong một session.

**Endpoint**

```http
POST /api/attendance/{sessionId}
```

**Roles**

- `Admin`
- `Teacher`

**Path params**

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `sessionId` | `guid` | Có | ID của session cần điểm danh |

**Request body**

```json
{
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
}
```

**Body fields**

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `attendances` | `array` | Có | Danh sách học sinh cần điểm danh |
| `attendances[].studentProfileId` | `guid` | Có | ID học sinh |
| `attendances[].attendanceStatus` | `AttendanceStatus` | Có | Trạng thái điểm danh |
| `attendances[].note` | `string?` | Không | Ghi chú |

**Success response**

```json
{
  "isSuccess": true,
  "data": {
    "results": [
      {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "sessionId": "550e8400-e29b-41d4-a716-446655440000",
        "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
        "attendanceStatus": "Present",
        "absenceType": null,
        "markedAt": "2026-03-31T08:15:00Z",
        "note": "Đến đúng giờ"
      },
      {
        "id": "660e8400-e29b-41d4-a716-446655440002",
        "sessionId": "550e8400-e29b-41d4-a716-446655440000",
        "studentProfileId": "550e8400-e29b-41d4-a716-446655440002",
        "attendanceStatus": "Absent",
        "absenceType": "WithNotice24H",
        "markedAt": "2026-03-31T08:15:00Z",
        "note": "Xin nghỉ có phép"
      }
    ]
  }
}
```

**Business rules**

- Session phải tồn tại.
- Nếu record attendance chưa có thì hệ thống tự tạo mới.
- Nếu đã có thì cập nhật trên record cũ.
- Nếu `AttendanceStatus = Absent`:
  - hệ thống resolve `AbsenceType` từ leave request đã `Approved`
  - nếu ra `WithNotice24H` thì tự tạo `MakeupCredit` nếu chưa có
- Nếu `AttendanceStatus = Present`:
  - hệ thống tăng `UsedSessions`
  - giảm `RemainingSessions`
  - có thể chuyển registration sang `Completed`
  - có thể chuyển class sang `Completed` nếu không còn active registration/enrollment phù hợp
- Nếu `AttendanceStatus = Makeup` hoặc `NotMarked` thì `AbsenceType` bị reset về `null`

**Error thường gặp**

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `Attendance.NotFound` | The attendance record with Id = '{sessionId}' was not found. |

---

### 2. Lấy danh sách điểm danh của session (UC-100)

Lấy danh sách điểm danh của toàn bộ học sinh đang `Active` trong class chứa session.

**Endpoint**

```http
GET /api/attendance/{sessionId}
```

**Roles**

- `Admin`
- `Teacher`

**Path params**

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `sessionId` | `guid` | Có | ID session |

**Success response**

```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "sessionName": "Toán - Lớp 1A",
    "date": "2026-03-31",
    "startTime": "08:00:00",
    "endTime": "09:30:00",
    "summary": {
      "totalStudents": 25,
      "presentCount": 20,
      "absentCount": 3,
      "makeupCount": 1,
      "notMarkedCount": 1
    },
    "attendances": [
      {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
        "studentName": "Nguyễn Văn A",
        "attendanceStatus": "Present",
        "absenceType": null,
        "hasMakeupCredit": false,
        "note": null,
        "markedAt": "2026-03-31T08:15:00Z"
      }
    ]
  }
}
```

**Business rules**

- Session phải tồn tại.
- Query lấy toàn bộ enrollment `Active` của class rồi join với attendance.
- Nếu học sinh chưa có attendance record:
  - `id = Guid.Empty`
  - `attendanceStatus = "NotMarked"`
- `hasMakeupCredit` là cờ kiểm tra học sinh có bất kỳ `MakeupCredit` nào đang `Available`, không chỉ riêng session hiện tại.

**Error thường gặp**

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `Session.NotFound` | Session with Id = '{sessionId}' was not found |

---

### 3. Lịch sử điểm danh học sinh (UC-101)

Lấy lịch sử điểm danh của học sinh từ `StudentId` trong token.

**Endpoint**

```http
GET /api/attendance/students
```

**Roles theo controller**

- `Admin`
- `Teacher`
- `Parent`
- `Student`

**Query params**

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `pageNumber` | `int` | Không | `1` | Số trang |
| `pageSize` | `int` | Không | `10` | Số bản ghi mỗi trang |

**Success response**

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "sessionId": "550e8400-e29b-41d4-a716-446655440000",
        "sessionDateTime": "2026-03-31T08:00:00Z",
        "attendanceStatus": "Present",
        "absenceType": null,
        "note": null
      }
    ],
    "pageNumber": 1,
    "totalPages": 5,
    "totalCount": 45,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**Business rules**

- Handler bắt buộc token có `StudentId`.
- Handler chỉ trả dữ liệu nếu profile:
  - tồn tại
  - có `ProfileType = Student`
  - có `UserId == currentUserId`
- Dữ liệu được sort giảm dần theo `Session.PlannedDatetime`.

**Error thường gặp**

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `Profile.StudentNotFound` | Student profile not found |

---

### 4. Cập nhật điểm danh (UC-104)

Cập nhật attendance của một học sinh trong một session.

**Endpoint**

```http
PUT /api/attendance/{sessionId}/students/{studentProfileId}
```

**Roles**

- `Admin`
- `Teacher`

**Path params**

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `sessionId` | `guid` | Có | ID session |
| `studentProfileId` | `guid` | Có | ID học sinh |

**Request body**

```json
{
  "attendanceStatus": 2,
  "note": "Học bù buổi trước"
}
```

**Body fields**

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `attendanceStatus` | `AttendanceStatus` | Có | Trạng thái mới |
| `note` | `string?` | Không | Ghi chú mới |

**Success response**

```json
{
  "isSuccess": true,
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
    "attendanceStatus": "Makeup",
    "absenceType": null,
    "note": "Học bù buổi trước",
    "updatedAt": "2026-03-31T08:15:00Z"
  }
}
```

**Validation rules**

- `sessionId` bắt buộc
- `studentProfileId` bắt buộc
- `attendanceStatus` phải là enum hợp lệ

**Business rules**

- Attendance record phải tồn tại sẵn.
- Với `Teacher`, chỉ được sửa trong vòng 24 giờ sau khi session kết thúc.
- Với `Admin`, không bị giới hạn cửa sổ 24 giờ.
- Khi update:
  - nếu `attendanceStatus = Absent` thì `absenceType` bị set thành `NoNotice`
  - nếu khác `Absent` thì `absenceType = null`
- Hệ thống tạo `AuditLog` cho thao tác update.
- Trường `updatedAt` hiện đang trả từ `attendance.MarkedAt`; handler không cập nhật lại `MarkedAt` trong flow update.

**Error thường gặp**

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `Attendance.NotFound` | Attendance not found for session '{sessionId}' and student '{studentProfileId}'. |
| `400` | `Attendance.UpdateWindowClosed` | Attendance for session '{sessionId}' can only be updated within 24 hours after it ends. |

---

## Response format

### Success

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Attendance.NotFound",
  "status": 404,
  "detail": "Attendance not found for session '...' and student '...'."
}
```

### Validation error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "'Session Id' must not be empty.",
      "type": 2
    }
  ]
}
```

### HTTP mapping

| Loại lỗi | HTTP status |
| --- | --- |
| Validation | `400` |
| NotFound | `404` |
| Conflict | `409` |
| Unauthorized | `401` |
| Forbidden | `403` |
| Failure khác | `500` |

---

## Mã lỗi và business rule

### Mã lỗi chính

| Code | HTTP | Ý nghĩa |
| --- | --- | --- |
| `Attendance.NotFound` | `404` | Không tìm thấy attendance record |
| `Attendance.UpdateWindowClosed` | `400` | Teacher sửa attendance quá 24 giờ sau khi session kết thúc |
| `Session.NotFound` | `404` | Không tìm thấy session |
| `Profile.StudentNotFound` | `404` | Không tìm thấy student profile phù hợp từ token/current user |

### Business rules tổng hợp

1. Attendance bulk mark có tính idempotent ở mức record:
   nếu record đã tồn tại thì cập nhật lại, không tạo trùng.
2. `Absent` có thể kéo theo `AbsenceType` và `MakeupCredit`.
3. `Present` có thể làm thay đổi registration/class status.
4. `UpdateAttendance` không tự tạo `MakeupCredit`.
5. `UpdateAttendance` luôn tạo audit log.
6. `GET /api/attendance/{sessionId}` hiển thị cả học sinh chưa có attendance record bằng status `NotMarked`.

---

## Ví dụ sử dụng

### Curl - điểm danh hàng loạt

```bash
curl -X POST "https://your-host/api/attendance/550e8400-e29b-41d4-a716-446655440000" \
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

### Curl - lấy attendance của session

```bash
curl -X GET "https://your-host/api/attendance/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer <your_token>"
```

### Curl - lấy lịch sử attendance

```bash
curl -X GET "https://your-host/api/attendance/students?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer <your_token>"
```

### Curl - cập nhật attendance

```bash
curl -X PUT "https://your-host/api/attendance/550e8400-e29b-41d4-a716-446655440000/students/550e8400-e29b-41d4-a716-446655440001" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your_token>" \
  -d '{
    "attendanceStatus": 2,
    "note": "Học bù buổi trước"
  }'
```

### JavaScript (Fetch API)

```javascript
async function markAttendance(sessionId, attendances, token) {
  const response = await fetch(`/api/attendance/${sessionId}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify({ attendances })
  });

  return response.json();
}

async function getSessionAttendance(sessionId, token) {
  const response = await fetch(`/api/attendance/${sessionId}`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.json();
}

async function getStudentAttendanceHistory(pageNumber, pageSize, token) {
  const response = await fetch(
    `/api/attendance/students?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  );

  return response.json();
}

async function updateAttendance(sessionId, studentProfileId, attendanceStatus, note, token) {
  const response = await fetch(
    `/api/attendance/${sessionId}/students/${studentProfileId}`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify({ attendanceStatus, note })
    }
  );

  return response.json();
}
```

---

## Lưu ý quan trọng

1. Tài liệu cũ của endpoint `GET /api/attendance/students` đã lệch shape response; shape đúng hiện tại là `id`, `sessionId`, `sessionDateTime`, `attendanceStatus`, `absenceType`, `note`.
2. `GET /api/attendance/students` mở role khá rộng ở controller nhưng vẫn phụ thuộc `StudentId` và `currentUserId` trong handler.
3. `GET /api/attendance/{sessionId}` trả cả học sinh chưa được điểm danh với `attendanceStatus = "NotMarked"`.
4. `hasMakeupCredit` trong response session attendance là cờ kiểm tra credit `Available` của học sinh, không chỉ credit của session đó.
5. `PUT /api/attendance/{sessionId}/students/{studentProfileId}` không cập nhật `MarkedAt`, nên `updatedAt` có thể phản ánh thời điểm mark cũ.
6. Flow attendance có liên kết trực tiếp với makeup credit, nhưng không tự set attendance sang `Makeup` khi dùng makeup credit ở module khác.
