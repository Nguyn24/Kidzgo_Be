# Tài Liệu API FE - Class - 2026-04-15

Tài liệu này mô tả các API trong `ClassController.cs`.

Ghi chú:

- Controller không có `[Authorize]` ở class level, nhưng từng endpoint hiện đều có `[Authorize(Roles=...)]`.
- `GET /api/classes` hiện cho `Admin`, `ManagementStaff`, `Parent`; không cho `Teacher`.
- `GET /api/classes/{id}/students` cho `Teacher`, nhưng handler chỉ cho teacher xem lớp mà mình là main/assistant teacher.
- API schedule segment chỉ dùng cho chương trình phụ (`Program.IsSupplementary = true`).

## Role và phạm vi dữ liệu

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động |
| --- | --- | --- | --- |
| Admin | Classes, students, capacity, schedule segments | `all` | `view`, `create`, `edit`, `delete`, `change_status`, `assign_teacher`, `update_color`, `add_schedule_segment` |
| ManagementStaff | Classes, students, capacity, schedule segments | `all` | `view`, `create`, `edit`, `change_status`, `assign_teacher`, `update_color`, `add_schedule_segment` |
| Teacher | Students trong lớp teacher dạy | `own` với `/students` | `view_students` |
| Parent | Danh sách lớp makeup theo filter hiện tại | `own-ish` theo query/filter; handler filter `Program.IsMakeup` khi có `ParentId` | `view` |
| Student | Không được truy cập controller này theo attributes hiện tại | `none` | `none` |
| Anonymous | Không được truy cập | `none` | `none` |

## Response format

Success:

```json
{ "isSuccess": true, "data": {} }
```

Riêng `PATCH /api/classes/{classId}/color` trả:

```json
{ "isSuccess": true }
```

Error:

```json
{
  "title": "Class.CodeExists",
  "status": 409,
  "detail": "Class code already exists"
}
```

## Enum/status

| Enum | Values |
| --- | --- |
| `ClassStatus` | `Planned`, `Recruiting`, `Active`, `Full`, `Closed`, `Completed`, `Suspended`, `Cancelled` |
| `EnrollmentStatus` liên quan | `Active`, `Paused`, `Dropped` |

## Danh sách API

| Method | Endpoint | Roles | Mô tả |
| --- | --- | --- | --- |
| POST | `/api/classes` | Admin, ManagementStaff | Tạo lớp. |
| GET | `/api/classes` | Admin, ManagementStaff, Parent | Lấy danh sách lớp. |
| GET | `/api/classes/{id}` | Admin, ManagementStaff | Xem chi tiết lớp. |
| POST | `/api/classes/{id}/schedule-segments` | Admin, ManagementStaff | Thêm segment lịch cho lớp chương trình phụ. |
| GET | `/api/classes/{id}/students` | Admin, ManagementStaff, Teacher | Xem học sinh trong lớp. |
| PUT | `/api/classes/{id}` | Admin, ManagementStaff | Cập nhật lớp. |
| PATCH | `/api/classes/{classId}/color` | Admin, ManagementStaff | Cập nhật màu lớp. |
| DELETE | `/api/classes/{id}` | Admin | Xóa mềm lớp. |
| PATCH | `/api/classes/{id}/status` | Admin, ManagementStaff | Đổi status lớp. |
| PATCH | `/api/classes/{id}/assign-teacher` | Admin, ManagementStaff | Gán main/assistant teacher. |
| GET | `/api/classes/{id}/capacity` | Admin, ManagementStaff | Check sĩ số lớp. |

### POST `/api/classes`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `branchId` | `Guid` | Yes | Branch active. |
| `programId` | `Guid` | Yes | Program active. |
| `code` | `string` | Yes | Mã lớp, max 50, unique. |
| `title` | `string?` | Required nếu không có `name` | Tên lớp. |
| `name` | `string?` | Required nếu không có `title` | Alias; controller ưu tiên `name`, rồi `title`, rồi `code`. |
| `roomId` | `Guid?` | No | Phòng học. |
| `mainTeacherId` | `Guid?` | No | Teacher chính. |
| `assistantTeacherId` | `Guid?` | No | Teacher phụ. |
| `startDate` | `DateOnly` | Yes | Không được ở quá khứ. |
| `endDate` | `DateOnly?` | Required nếu có `schedulePattern` | Phải >= `startDate`. |
| `capacity` | `int` | Yes | Phải > 0. |
| `schedulePattern` | `string?` | No | RRULE lịch học. |
| `description` | `string?` | No | Mô tả. |

Success `201`: `CreateClassResponse` gồm `id`, `branchId`, `programId`, `code`, `title`, `roomId`, teacher ids, `startDate`, `endDate`, `status`, `capacity`, `schedulePattern`, `description`, `name`, `scheduleText`.

Errors: validation errors, `404 Class.BranchNotFound`, `404 Class.ProgramNotFound`, `409 Class.CodeExists`, `404 Class.MainTeacherNotFound`, `409 Class.MainTeacherBranchMismatch`, `404 Class.AssistantTeacherNotFound`, `409 Class.AssistantTeacherBranchMismatch`, `409 Class.RoomConflict`, `409 Class.TeacherConflict`, `409 Class.AssistantConflict`, `400 SchedulePattern.Invalid`, `401`, `403`.

### GET `/api/classes`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `branchId` | `Guid?` | No | null |
| `programId` | `Guid?` | No | null |
| `teacherId` | `Guid?` | No | null |
| `studentId` | `Guid?` | No | null |
| `status` | `ClassStatus?` | No | null |
| `schedulePattern` | `string?` | No | null |
| `searchTerm` | `string?` | No | null |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: `{ classes: Page<ClassDto> }`.

`ClassDto`: `id`, `branchId`, `branchName`, `programId`, `programName`, `code`, `title`, teacher ids/names, `startDate`, `endDate`, `status`, `capacity`, `currentEnrollmentCount`, `schedulePattern`, `description`, `name`, `roomId`, `roomName`, `scheduleText`, `studentCount`, `totalSessions`, `completedSessions`, `progressPercent`.

Parent behavior: nếu `userContext.ParentId` có giá trị, handler filter thêm `Program.IsMakeup`.

Errors: `401`, `403`; enum query invalid có thể `400`.

### GET `/api/classes/{id}`

Path: `id: Guid`.

Success: `GetClassByIdResponse`, gồm field như `ClassDto` và thêm `createdAt`, `updatedAt`, `teacherIds`, `teacherNames`, `scheduleSegments`.

`scheduleSegments[]`: `id`, `effectiveFrom`, `effectiveTo`, `schedulePattern`.

Errors: `404 Class.NotFound`, `401`, `403`.

### POST `/api/classes/{id}/schedule-segments`

Body:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `effectiveFrom` | `DateOnly` | Yes | - | Ngày bắt đầu áp dụng lịch mới. |
| `effectiveTo` | `DateOnly?` | No | null | Ngày kết thúc segment. |
| `schedulePattern` | `string` | Yes | - | RRULE lịch mới. |
| `generateSessions` | `bool` | No | true | Có generate sessions lại không. |
| `onlyFutureSessions` | `bool` | No | true | Chỉ generate cho future sessions. |

Success: `{ id, classId, programId, effectiveFrom, effectiveTo, schedulePattern, generatedSessionsCount }`.

Errors: `404 Class.NotFound`, `400 Class.SupplementaryProgramRequired`, `400 SchedulePattern.Empty`, `400 SchedulePattern.Invalid`, `400 Class.ScheduleSegmentInvalidEffectiveDate`, `409 Class.ScheduleSegmentAlreadyExists`, `409 Class.FutureScheduleSegmentExists`, session generation/conflict errors, `401`, `403`.

### GET `/api/classes/{id}/students`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: `{ students: Page<ClassStudentDto> }`.

`ClassStudentDto`: `studentProfileId`, `fullName`, `avatarUrl`, `email`, `phone`, `enrollDate`, `status`, `attendanceRate`, `progressPercent`, `stars`, `lastActiveAt`.

Teacher scope: teacher chỉ xem được nếu class có `MainTeacherId` hoặc `AssistantTeacherId` bằng user hiện tại.

Errors: `404 Class.NotFound`, `401`, `403`.

### PUT `/api/classes/{id}`

Body giống create.

Success: `UpdateClassResponse`.

Errors: giống create, thêm `404 Class.NotFound`.

### PATCH `/api/classes/{classId}/color`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `color` | `string` | Yes | Format `#RRGGBB`. |

Success: `{ isSuccess: true }`.

Errors: `400 Color is required`, `400 Color must be in format #RRGGBB`, `404 Class.NotFound`, `401`, `403`.

### DELETE `/api/classes/{id}`

Success: delete/soft delete response.

Errors: `404 Class.NotFound`, `409 Class.HasActiveEnrollments`, `401`, `403`.

### PATCH `/api/classes/{id}/status`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `status` | `ClassStatus` | Yes | Status mới. |

Success: `{ id, status }`.

Errors: `404 Class.NotFound`, `400 Class.StatusUnchanged`, `400 Class.InvalidStatusTransition`, `401`, `403`.

### PATCH `/api/classes/{id}/assign-teacher`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `mainTeacherId` | `Guid?` | No | Teacher chính mới hoặc null. |
| `assistantTeacherId` | `Guid?` | No | Teacher phụ mới hoặc null. |

Success: `{ classId, mainTeacherId, mainTeacherName, assistantTeacherId, assistantTeacherName }`.

Errors: `404 Class.NotFound`, `404 Class.MainTeacherNotFound`, `409 Class.MainTeacherBranchMismatch`, `404 Class.AssistantTeacherNotFound`, `409 Class.AssistantTeacherBranchMismatch`, teacher conflict errors, `401`, `403`.

### GET `/api/classes/{id}/capacity`

Success: `{ classId, capacity, currentEnrollmentCount, availableSlots, hasAvailableSlots }`.

Errors: `404 Class.NotFound`, `401`, `403`.

## Status definition

| Status | Ý nghĩa |
| --- | --- |
| `Planned` | Lớp đã lên kế hoạch. |
| `Recruiting` | Đang tuyển sinh. |
| `Active` | Đang học. |
| `Full` | Đã đủ sĩ số. |
| `Closed` | Đã đóng/kết thúc. |
| `Completed` | Hoàn thành. |
| `Suspended` | Tạm ngưng. |
| `Cancelled` | Đã hủy. |

Luồng chuyển trạng thái hiện tại:

```text
PATCH /status cho phép đổi status linh hoạt
Nếu status hiện tại là Closed thì không được đổi về Planned
Nếu status mới bằng status hiện tại -> Class.StatusUnchanged
```

## Permission matrix

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/classes` | Yes | Yes | No | No | No | No |
| `GET /api/classes` | Yes | Yes | No | Yes | No | No |
| `GET /api/classes/{id}` | Yes | Yes | No | No | No | No |
| `POST /api/classes/{id}/schedule-segments` | Yes | Yes | No | No | No | No |
| `GET /api/classes/{id}/students` | Yes | Yes | Yes, lớp mình dạy | No | No | No |
| `PUT /api/classes/{id}` | Yes | Yes | No | No | No | No |
| `PATCH /api/classes/{classId}/color` | Yes | Yes | No | No | No | No |
| `DELETE /api/classes/{id}` | Yes | No | No | No | No | No |
| `PATCH /api/classes/{id}/status` | Yes | Yes | No | No | No | No |
| `PATCH /api/classes/{id}/assign-teacher` | Yes | Yes | No | No | No | No |
| `GET /api/classes/{id}/capacity` | Yes | Yes | No | No | No | No |

## Validation rules

| Rule | API áp dụng | Lỗi |
| --- | --- | --- |
| Role đúng | Tất cả | 403 |
| `branchId`, `programId`, `code`, `title/name`, `startDate`, `capacity` bắt buộc | Create/update | 400 validation |
| `code` max 50 và unique | Create/update | 400/409 |
| `title` max 255 | Create/update | 400 |
| `startDate` không ở quá khứ | Create/update | 400 |
| Nếu có `schedulePattern` thì cần `endDate` | Create/update | 400 |
| `endDate >= startDate` và không ở quá khứ | Create/update | 400 |
| `capacity > 0` | Create/update | 400 |
| Branch/program phải active | Create/update | 404 |
| Teacher phải tồn tại, đúng role, cùng branch | Create/update/assign-teacher | 404/409 |
| Lịch/phòng/teacher không được conflict | Create/update/assign-teacher | 409 |
| Schedule segment chỉ cho supplementary program | Add schedule segment | 400 |
| Segment effective date phải nằm trong range lớp | Add schedule segment | 400 |
| Không được thêm segment trùng `effectiveFrom` hoặc thêm trước future segment đã có | Add schedule segment | 409 |
| Không delete class có active enrollments | Delete | 409 |
| Color phải đúng format `#RRGGBB` | Update color | 400 |

