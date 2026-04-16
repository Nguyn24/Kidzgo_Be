# Tài Liệu API FE - Enrollment - 2026-04-15

Tài liệu này mô tả các API trong `EnrollmentController.cs`.

Ghi chú quan trọng:

- Controller không có `[Authorize]` ở class level.
- `GET /api/enrollments` và `GET /api/enrollments/{id}` hiện đang comment `[Authorize]`, nên theo code hiện tại anonymous có thể gọi 2 API này.
- Các API tạo/sửa/pause/drop/reactivate/assign tuition plan/history/schedule segment yêu cầu `Admin` hoặc `ManagementStaff`.
- API schedule segment chỉ dùng cho enrollment thuộc chương trình phụ (`Program.IsSupplementary = true`).

## Role và phạm vi dữ liệu

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động |
| --- | --- | --- | --- |
| Admin | Enrollments và history | `all` | `view`, `create`, `edit`, `pause`, `drop`, `reactivate`, `assign_tuition_plan`, `add_schedule_segment` |
| ManagementStaff | Enrollments và history | `all` | `view`, `create`, `edit`, `pause`, `drop`, `reactivate`, `assign_tuition_plan`, `add_schedule_segment` |
| Teacher | Theo code hiện tại chỉ xem được nếu dùng 2 GET không authorize | `all` với 2 GET hiện tại | `view` |
| Parent | Theo code hiện tại chỉ xem được nếu dùng 2 GET không authorize | `all` với 2 GET hiện tại | `view` |
| Student | Theo code hiện tại chỉ xem được nếu dùng 2 GET không authorize | `all` với 2 GET hiện tại | `view` |
| Anonymous | `GET /api/enrollments`, `GET /api/enrollments/{id}` | `all` với 2 GET hiện tại | `view` |

## Response format

Success:

```json
{ "isSuccess": true, "data": {} }
```

Error:

```json
{
  "title": "Enrollment.ClassFull",
  "status": 409,
  "detail": "Class has reached its capacity"
}
```

## Enum/status

| Enum | Values |
| --- | --- |
| `EnrollmentStatus` | `Active`, `Paused`, `Dropped` |
| `RegistrationTrackType` | `Primary`, `Secondary` |

Track request nhận string, thường dùng `primary` hoặc `secondary`.

## Danh sách API

| Method | Endpoint | Roles | Mô tả |
| --- | --- | --- | --- |
| POST | `/api/enrollments` | Admin, ManagementStaff | Ghi danh học sinh vào lớp. |
| POST | `/api/enrollments/{id}/schedule-segments` | Admin, ManagementStaff | Thêm segment lịch học cho enrollment chương trình phụ. |
| GET | `/api/enrollments` | Không có authorize hiện tại | Lấy danh sách enrollments. |
| GET | `/api/enrollments/{id}` | Không có authorize hiện tại | Xem chi tiết enrollment. |
| PUT | `/api/enrollments/{id}` | Admin, ManagementStaff | Cập nhật enrollment. |
| PATCH | `/api/enrollments/{id}/pause` | Admin, ManagementStaff | Pause enrollment. |
| PATCH | `/api/enrollments/{id}/drop` | Admin, ManagementStaff | Drop enrollment. |
| PATCH | `/api/enrollments/{id}/reactivate` | Admin, ManagementStaff | Active lại enrollment paused. |
| PATCH | `/api/enrollments/{id}/assign-tuition-plan` | Admin, ManagementStaff | Gán tuition plan. |
| GET | `/api/enrollments/student/{studentProfileId}/history` | Admin, ManagementStaff | Xem lịch sử enrollment của học sinh. |

### POST `/api/enrollments`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `classId` | `Guid` | Yes | Lớp cần ghi danh. |
| `studentProfileId` | `Guid` | Yes | Student profile. |
| `enrollDate` | `DateOnly` | Yes | Ngày bắt đầu học. |
| `tuitionPlanId` | `Guid?` | No | Gói học phí. |
| `track` | `string?` | No | `primary` hoặc `secondary`; null dùng default helper. |
| `sessionSelectionPattern` | `string?` | No | RRULE subset nếu học một phần lịch lớp. |

Success `201`: `CreateEnrollmentResponse` gồm `id`, `classId`, `classCode`, `classTitle`, `studentProfileId`, `studentName`, `enrollDate`, `status`, `tuitionPlanId`, `tuitionPlanName`.

Logic:

- Class phải tồn tại và status `Active` hoặc `Planned`.
- Student profile phải là student.
- Không được active enrollment trùng class/student.
- Class chưa full.
- `sessionSelectionPattern` phải là subset hợp lệ của class schedule.
- Check conflict lịch học của student.
- Nếu program là supplementary, BE tạo `ClassEnrollmentScheduleSegment` ban đầu.
- BE sync assignments cho enrollment.

Errors: `404 Enrollment.ClassNotFound`, `404 Enrollment.StudentNotFound`, `409 Enrollment.ClassNotAvailable`, `409 Enrollment.AlreadyEnrolled`, `409 Enrollment.ClassFull`, `404 Enrollment.TuitionPlanNotFound`, `409 Enrollment.TuitionPlanNotAvailable`, `409 Enrollment.TuitionPlanProgramMismatch`, `409 Enrollment.StudentScheduleConflict`, `400 SchedulePattern.Invalid`, `401`, `403`.

### POST `/api/enrollments/{id}/schedule-segments`

Body:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `effectiveFrom` | `DateOnly` | Yes | - | Ngày bắt đầu áp dụng selection mới. |
| `effectiveTo` | `DateOnly?` | No | null | Ngày kết thúc segment. |
| `sessionSelectionPattern` | `string?` | No | null | RRULE subset mới. |
| `clearSessionSelectionPattern` | `bool` | No | false | Nếu true thì học toàn bộ lịch lớp từ segment này. |

Success: `{ id, enrollmentId, classId, programId, effectiveFrom, effectiveTo, sessionSelectionPattern, activeSessionSelectionPattern }`.

Errors: `404 Enrollment.NotFound`, `400 Enrollment.SupplementaryProgramRequired`, `409 Enrollment.AlreadyDropped`, `400 Enrollment.ScheduleSegmentInvalidEffectiveDate`, `409 Enrollment.ScheduleSegmentAlreadyExists`, `409 Enrollment.FutureScheduleSegmentExists`, `409 Enrollment.StudentScheduleConflict`, `400 SchedulePattern.Invalid`, `401`, `403`.

### GET `/api/enrollments`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `classId` | `Guid?` | No | null |
| `studentProfileId` | `Guid?` | No | null |
| `status` | `string?` | No | null |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: `{ enrollments: Page<EnrollmentDto> }`.

`EnrollmentDto`: `id`, `classId`, `classCode`, `classTitle`, `studentProfileId`, `studentName`, `enrollDate`, `status`, `tuitionPlanId`, `tuitionPlanName`.

Errors: invalid status string bị ignore nếu parse fail theo controller hiện tại; anonymous hiện có thể gọi.

### GET `/api/enrollments/{id}`

Path: `id: Guid`.

Success: `GetEnrollmentByIdResponse` gồm `id`, class/program/branch info, student info, `enrollDate`, `status`, tuition plan, `sessionSelectionPattern`, `scheduleSegments`, `createdAt`, `updatedAt`.

`scheduleSegments[]`: `id`, `effectiveFrom`, `effectiveTo`, `sessionSelectionPattern`.

Errors: `404 Enrollment.NotFound`.

### PUT `/api/enrollments/{id}`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `enrollDate` | `DateOnly?` | No | Ngày bắt đầu mới. |
| `tuitionPlanId` | `Guid?` | No | Gói học phí mới. |
| `track` | `string?` | No | `primary` hoặc `secondary`. |
| `sessionSelectionPattern` | `string?` | No | RRULE subset mới. |
| `clearSessionSelectionPattern` | `bool` | No | Nếu true clear selection pattern. |

Success: `UpdateEnrollmentResponse`.

Errors: `404 Enrollment.NotFound`, `404 Enrollment.TuitionPlanNotFound`, `409 Enrollment.TuitionPlanNotAvailable`, `409 Enrollment.TuitionPlanProgramMismatch`, `409 Enrollment.StudentScheduleConflict`, `400 SchedulePattern.Invalid`, `401`, `403`.

### PATCH `/api/enrollments/{id}/pause`

Success: enrollment response với `status = Paused`.

Logic: chỉ enrollment `Active` được pause; BE hủy future assignments cho enrollment.

Errors: `404 Enrollment.NotFound`, `409 Enrollment.InvalidStatus`, `401`, `403`.

### PATCH `/api/enrollments/{id}/drop`

Success: enrollment response với `status = Dropped`.

Logic: hủy future assignments và sync capacity status của class.

Errors: `404 Enrollment.NotFound`, `409 Enrollment.AlreadyDropped`, `401`, `403`.

### PATCH `/api/enrollments/{id}/reactivate`

Success: enrollment response với `status = Active`.

Logic:

- Không reactivate enrollment đã `Dropped`.
- Class phải còn available (`Active`, `Planned`, hoặc `Recruiting`).
- Class chưa full.
- Không conflict lịch student.
- Sync assignments và capacity status.

Errors: `404 Enrollment.NotFound`, `409 Enrollment.AlreadyActive`, `409 Enrollment.CannotReactivateDropped`, `409 Enrollment.ClassNotAvailable`, `409 Enrollment.ClassFull`, `409 Enrollment.StudentScheduleConflict`, `401`, `403`.

### PATCH `/api/enrollments/{id}/assign-tuition-plan`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `tuitionPlanId` | `Guid` | Yes | Tuition plan mới. |

Success: `AssignTuitionPlanResponse`.

Errors: `404 Enrollment.NotFound`, `404 Enrollment.TuitionPlanNotFound`, `409 Enrollment.TuitionPlanNotAvailable`, `409 Enrollment.TuitionPlanProgramMismatch`, `401`, `403`.

### GET `/api/enrollments/student/{studentProfileId}/history`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: `{ studentProfileId, studentName, enrollments: Page<EnrollmentHistoryDto> }`.

`EnrollmentHistoryDto`: `id`, `classId`, `classCode`, `classTitle`, `programId`, `programName`, `branchId`, `branchName`, `enrollDate`, `status`, `tuitionPlanId`, `tuitionPlanName`, `createdAt`, `updatedAt`.

Errors: `404 Enrollment.StudentNotFound`, `401`, `403`.

## Status definition

| Status | Ý nghĩa |
| --- | --- |
| `Active` | Học sinh đang học trong lớp. |
| `Paused` | Enrollment tạm dừng, thường do bảo lưu hoặc staff pause thủ công. |
| `Dropped` | Enrollment đã dừng hẳn/chuyển lớp, không reactivate được. |

Luồng chuyển:

```text
Create -> Active
Active -- pause --> Paused
Active hoặc Paused -- drop --> Dropped
Paused -- reactivate --> Active
Dropped -- không được reactivate
```

## Permission matrix

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/enrollments` | Yes | Yes | No | No | No | No |
| `POST /api/enrollments/{id}/schedule-segments` | Yes | Yes | No | No | No | No |
| `GET /api/enrollments` | Yes | Yes | Yes hiện tại | Yes hiện tại | Yes hiện tại | Yes hiện tại |
| `GET /api/enrollments/{id}` | Yes | Yes | Yes hiện tại | Yes hiện tại | Yes hiện tại | Yes hiện tại |
| `PUT /api/enrollments/{id}` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/pause` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/drop` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/reactivate` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/assign-tuition-plan` | Yes | Yes | No | No | No | No |
| `GET /api/enrollments/student/{studentProfileId}/history` | Yes | Yes | No | No | No | No |

## Validation rules

| Rule | API áp dụng | Lỗi |
| --- | --- | --- |
| Role đúng | Các API có `[Authorize]` | 403 |
| Class phải tồn tại | Create | 404 |
| Class status phải available | Create/reactivate | 409 |
| Student profile phải tồn tại và là student | Create/history | 404 |
| Không được active enrollment trùng class/student | Create | 409 |
| Class chưa full | Create/reactivate | 409 |
| Tuition plan phải tồn tại, active, cùng program với class | Create/update/assign | 404/409 |
| `sessionSelectionPattern` phải là subset hợp lệ của class schedule | Create/update/schedule segment | 400 |
| Lịch student không được conflict | Create/update/reactivate/schedule segment | 409 |
| Pause chỉ từ `Active` | Pause | 409 `Enrollment.InvalidStatus` |
| Dropped không được reactivate | Reactivate | 409 `Enrollment.CannotReactivateDropped` |
| Schedule segment chỉ cho supplementary program | Add schedule segment | 400 |
| Segment effective date phải nằm trong range enrollment/class | Add schedule segment | 400 |
| Không thêm segment trùng `effectiveFrom` hoặc trước future segment đã có | Add schedule segment | 409 |

