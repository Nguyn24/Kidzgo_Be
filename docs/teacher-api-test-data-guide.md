# Hướng dẫn tạo data test cho Teacher APIs qua API

## Mục tiêu
Tạo đủ data để test 2 API endpoints:
- `GET /api/teacher/classes` - Lấy danh sách lớp của Teacher
- `GET /api/teacher/timetable` - Lấy thời khóa biểu của Teacher

## Thứ tự thực hiện (theo dependency)

### Bước 1: Tạo Branch (nếu chưa có)
**API:** `POST /api/branches`  
**Role:** `Admin`  
**Request Body:**
```json
{
  "code": "BR-001",
  "name": "Test Branch",
  "address": "123 Test Street",
  "contactPhone": "0123456789",
  "contactEmail": "branch@test.com"
}
```
**Response:** `201 Created` với `Location: /api/branches/{branchId}`  
**Lưu lại:** `branchId` từ response header hoặc response body

---

### Bước 2: Gán Branch cho Teacher (nếu teacher chưa có branch_id)
**API:** `PATCH /api/admin/users/{teacherUserId}/assign-branch`  
**Role:** `Admin`  
**Request Body:**
```json
{
  "branchId": "<branchId từ bước 1>"
}
```
**Response:** `200 OK`

**Lưu ý:** 
- `teacherUserId` là `Id` của user `teacher1@gmail.com` (từ bảng `Users`)
- Nếu teacher đã có `branch_id` thì bỏ qua bước này

---

### Bước 3: Tạo Program
**API:** `POST /api/programs`  
**Role:** `Admin` hoặc `Staff`  
**Request Body:**
```json
{
  "branchId": "<branchId từ bước 1>",
  "name": "Timetable Test Program",
  "level": "Level 1",
  "totalSessions": 20,
  "defaultTuitionAmount": 0,
  "unitPriceSession": 0,
  "description": "Program for teacher timetable API test"
}
```
**Response:** `201 Created` với `Location: /api/programs/{programId}`  
**Lưu lại:** `programId` từ response header hoặc response body

---

### Bước 4: Tạo Classroom
**API:** `POST /api/classrooms`  
**Role:** `Admin` hoặc `Staff`  
**Request Body:**
```json
{
  "branchId": "<branchId từ bước 1>",
  "name": "Test Room 1",
  "capacity": 20,
  "note": "Room for timetable test"
}
```
**Response:** `201 Created` với `Location: /api/classrooms/{classroomId}`  
**Lưu lại:** `classroomId` từ response header hoặc response body

---

### Bước 5: Tạo Class và gán Teacher
**API:** `POST /api/classes`  
**Role:** `Admin` hoặc `Staff`  
**Request Body:**
```json
{
  "branchId": "<branchId từ bước 1>",
  "programId": "<programId từ bước 3>",
  "code": "TT-001",
  "title": "Timetable Test Class",
  "mainTeacherId": "<teacherUserId - Id của teacher1@gmail.com>",
  "assistantTeacherId": null,
  "startDate": "2025-01-01",
  "endDate": "2025-03-31",
  "capacity": 20,
  "schedulePattern": null
}
```
**Response:** `201 Created` với `Location: /api/classes/{classId}`  
**Lưu lại:** `classId` từ response header hoặc response body

**Lưu ý:** 
- `startDate` và `endDate` format: `YYYY-MM-DD`
- `mainTeacherId` phải là `Id` của user có role `Teacher`

---

### Bước 6: Tạo Sessions (tối thiểu 3 sessions trong tháng 01/2025)
**API:** `POST /api/sessions`  
**Role:** `Admin` hoặc `Staff`  
**Request Body cho Session 1:**
```json
{
  "classId": "<classId từ bước 5>",
  "plannedDatetime": "2025-01-05T09:00:00Z",
  "durationMinutes": 90,
  "plannedRoomId": "<classroomId từ bước 4>",
  "plannedTeacherId": "<teacherUserId>",
  "plannedAssistantId": null,
  "participationType": "Main"
}
```

**Request Body cho Session 2:**
```json
{
  "classId": "<classId từ bước 5>",
  "plannedDatetime": "2025-01-12T09:00:00Z",
  "durationMinutes": 90,
  "plannedRoomId": "<classroomId từ bước 4>",
  "plannedTeacherId": "<teacherUserId>",
  "plannedAssistantId": null,
  "participationType": "Main"
}
```

**Request Body cho Session 3:**
```json
{
  "classId": "<classId từ bước 5>",
  "plannedDatetime": "2025-01-19T09:00:00Z",
  "durationMinutes": 90,
  "plannedRoomId": "<classroomId từ bước 4>",
  "plannedTeacherId": "<teacherUserId>",
  "plannedAssistantId": null,
  "participationType": "Main"
}
```

**Response:** `201 Created` với `Location: /api/sessions/{sessionId}`

**Lưu ý:**
- `plannedDatetime` format: ISO 8601 UTC (`YYYY-MM-DDTHH:mm:ssZ`)
- `participationType` có thể là: `Main`, `Makeup`, `ExtraPaid`, `Free`, `Trial`
- Tạo ít nhất 3 sessions trong khoảng thời gian bạn muốn test (ví dụ: tháng 01/2025)

---

## Test APIs

### Test 1: GET /api/teacher/classes
**API:** `GET /api/teacher/classes?pageNumber=1&pageSize=10`  
**Role:** `Teacher` (đăng nhập bằng `teacher1@gmail.com`)  
**Expected:** Trả về danh sách classes mà teacher này là `mainTeacherId` hoặc `assistantTeacherId`

---

### Test 2: GET /api/teacher/timetable
**API:** `GET /api/teacher/timetable?from=2025-01-01T00:00:00Z&to=2025-01-31T23:59:59Z`  
**Role:** `Teacher` (đăng nhập bằng `teacher1@gmail.com`)  
**Expected:** Trả về danh sách sessions trong khoảng thời gian từ `from` đến `to`, mà teacher này là `plannedTeacherId` hoặc `actualTeacherId`

**Lưu ý:**
- `from` và `to` format: ISO 8601 UTC (`YYYY-MM-DDTHH:mm:ssZ`)
- Nếu không truyền `from` hoặc `to`, API sẽ trả về tất cả sessions của teacher

---

## Checklist

- [ ] Đã tạo Branch
- [ ] Đã gán Branch cho Teacher (nếu cần)
- [ ] Đã tạo Program
- [ ] Đã tạo Classroom
- [ ] Đã tạo Class và gán Teacher
- [ ] Đã tạo ít nhất 3 Sessions trong tháng test
- [ ] Đã test `GET /api/teacher/classes`
- [ ] Đã test `GET /api/teacher/timetable`

---

## Lưu ý quan trọng

1. **Authentication:** Tất cả các API đều yêu cầu authentication. Đảm bảo đã login và có token hợp lệ.

2. **Authorization:** 
   - Các API tạo data (Branch, Program, Classroom, Class, Session) yêu cầu role `Admin` hoặc `Staff`
   - API test (`/api/teacher/*`) yêu cầu role `Teacher`

3. **DateTime Format:** 
   - Tất cả `DateTime` trong request phải là UTC format: `YYYY-MM-DDTHH:mm:ssZ`
   - Ví dụ: `2025-01-05T09:00:00Z`

4. **Date Format:** 
   - `DateOnly` trong request format: `YYYY-MM-DD`
   - Ví dụ: `2025-01-01`

5. **Teacher User ID:** 
   - Cần lấy `Id` của user `teacher1@gmail.com` từ bảng `Users` hoặc từ response của `GET /api/admin/users?role=Teacher`

6. **Nếu teacher chưa có branch_id:** 
   - Phải gán branch trước khi tạo Program, Classroom, Class, Session
   - Vì các entities này đều yêu cầu `branchId`

