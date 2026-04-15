# Tài Liệu API FE - Pause Enrollment Request - 2026-04-15

Tài liệu này mô tả các API trong `PauseEnrollmentRequestController.cs`.

Luồng chính: tạo yêu cầu bảo lưu, staff approve/reject, hệ thống pause các enrollment active có session trong khoảng bảo lưu, sau đó staff cập nhật outcome và có thể reassign sang lớp tương đương.

## Role và phạm vi dữ liệu

Controller có `[Authorize]`, tất cả API yêu cầu đăng nhập.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động |
| --- | --- | --- | --- |
| Admin | Tất cả pause enrollment requests | `all` | `view`, `create`, `approve`, `reject`, `cancel`, `update_outcome`, `reassign` |
| ManagementStaff | Tất cả pause enrollment requests | `all` | `view`, `create`, `approve`, `reject`, `cancel`, `update_outcome`, `reassign` |
| Teacher | Theo code hiện tại có thể gọi create/list/detail/cancel | `all` nếu truyền filter; không có role-specific guard | `view`, `create`, `cancel` |
| Parent | Nếu không truyền `studentProfileId`, list auto scope về `userContext.StudentId`; create/detail/cancel hiện chưa check ownership trong handler | `own` mặc định list, nhưng cần lưu ý explicit id | `view`, `create`, `cancel` |
| Student | Nếu không truyền `studentProfileId`, list auto scope về `userContext.StudentId`; create/detail/cancel hiện chưa check ownership trong handler | `own` mặc định list, nhưng cần lưu ý explicit id | `view`, `create`, `cancel` |
| Anonymous | Không được truy cập | `none` | `none` |

Ghi chú: `GET list` chỉ auto-scope Parent/Student khi không gửi `studentProfileId`. Nếu FE gửi explicit `studentProfileId`, handler hiện không validate ownership.

## Response format

Success:

```json
{ "isSuccess": true, "data": {} }
```

Error:

```json
{
  "title": "PauseEnrollmentRequest.DuplicateActiveRequest",
  "status": 409,
  "detail": "A pending or approved pause request already exists for this student in the selected date range"
}
```

## Enum/status

| Enum | Values |
| --- | --- |
| `PauseEnrollmentRequestStatus` | `Pending`, `Approved`, `Rejected`, `Cancelled` |
| `PauseEnrollmentOutcome` | `ContinueSameClass`, `ReassignEquivalentClass`, `ContinueWithTutoring` |
| `EnrollmentStatus` liên quan | `Active`, `Paused`, `Dropped` |

## Danh sách API

| Method | Endpoint | Roles | Mô tả |
| --- | --- | --- | --- |
| POST | `/api/pause-enrollment-requests` | Logged in | Tạo yêu cầu bảo lưu. |
| GET | `/api/pause-enrollment-requests` | Logged in | Lấy danh sách yêu cầu. |
| GET | `/api/pause-enrollment-requests/{id}` | Logged in | Xem chi tiết yêu cầu. |
| PUT | `/api/pause-enrollment-requests/{id}/approve` | Admin, ManagementStaff | Duyệt yêu cầu. |
| PUT | `/api/pause-enrollment-requests/approve-bulk` | Admin, ManagementStaff | Duyệt nhiều yêu cầu. |
| PUT | `/api/pause-enrollment-requests/{id}/reject` | Admin, ManagementStaff | Từ chối yêu cầu. |
| PUT | `/api/pause-enrollment-requests/{id}/cancel` | Logged in | Hủy yêu cầu. |
| PUT | `/api/pause-enrollment-requests/{id}/outcome` | Admin, ManagementStaff | Cập nhật hướng xử lý sau bảo lưu. |
| POST | `/api/pause-enrollment-requests/{id}/reassign-equivalent-class` | Admin, ManagementStaff | Chuyển học sinh sang lớp tương đương sau bảo lưu. |

### POST `/api/pause-enrollment-requests`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Học sinh cần bảo lưu. |
| `pauseFrom` | `DateOnly` | Yes | Không được ở quá khứ. |
| `pauseTo` | `DateOnly` | Yes | Phải >= `pauseFrom`. |
| `reason` | `string?` | No | Lý do bảo lưu. |

Success `201`: `CreatePauseEnrollmentRequestResponse` gồm `id`, `studentProfileId`, `pauseFrom`, `pauseTo`, `reason`, `status`, `requestedAt`, `classes`.

`classes[]`: các lớp active của học sinh có session nằm trong khoảng bảo lưu.

Errors: `400 validation`, `404 PauseEnrollmentRequest.StudentNotFound`, `409 PauseEnrollmentRequest.NoEnrollmentsInRange`, `409 PauseEnrollmentRequest.DuplicateActiveRequest`, `401`.

### GET `/api/pause-enrollment-requests`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | null |
| `classId` | `Guid?` | No | null |
| `status` | `PauseEnrollmentRequestStatus?` | No | null |
| `branchId` | `Guid?` | No | null |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: `Page<PauseEnrollmentRequestResponse>`.

Response item fields: `id`, `studentProfileId`, `classId`, `pauseFrom`, `pauseTo`, `reason`, `status`, `requestedAt`, `approvedBy`, `approvedAt`, `cancelledBy`, `cancelledAt`, `outcome`, `outcomeNote`, `outcomeBy`, `outcomeAt`, `reassignedClassId`, `reassignedEnrollmentId`, `outcomeCompletedBy`, `outcomeCompletedAt`, `classes`.

Errors: `401`; enum query invalid có thể `400`.

### GET `/api/pause-enrollment-requests/{id}`

Path: `id: Guid`.

Success: `PauseEnrollmentRequestResponse`.

Errors: `404 PauseEnrollmentRequest.NotFound`, `401`.

### PUT `/api/pause-enrollment-requests/{id}/approve`

Duyệt yêu cầu. Khi approve, BE:

- Set request status `Approved`.
- Tìm enrollment active của học sinh có session trong khoảng `pauseFrom` tới `pauseTo`.
- Set các enrollment đó sang `Paused`.
- Hủy student session assignments trong khoảng bảo lưu.
- Reconcile leave/makeup bị superseded bởi pause.
- Ghi `PauseEnrollmentRequestHistory`.
- Gửi notification theo helper.

Success: `{ isSuccess: true, data: null }` hoặc response empty success.

Errors: `404 PauseEnrollmentRequest.NotFound`, `409 PauseEnrollmentRequest.AlreadyApproved`, `409 PauseEnrollmentRequest.AlreadyRejected`, `409 PauseEnrollmentRequest.AlreadyCancelled`, `409 PauseEnrollmentRequest.NoEnrollmentsInRange`, `409 PauseEnrollmentRequest.NotEnrolled`, `401`, `403`.

### PUT `/api/pause-enrollment-requests/approve-bulk`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `ids` | `array<Guid>` | Yes | Danh sách request cần approve. |

Success: `{ approvedIds, errors }`.

`errors[]`: `id`, `code`, `message`.

Errors: `400 Ids are required`, `401`, `403`. Lỗi từng item được trả trong `errors[]`.

### PUT `/api/pause-enrollment-requests/{id}/reject`

Từ chối yêu cầu.

Success: empty success.

Errors: `404 PauseEnrollmentRequest.NotFound`, `409 PauseEnrollmentRequest.AlreadyApproved`, `409 PauseEnrollmentRequest.AlreadyRejected`, `409 PauseEnrollmentRequest.AlreadyCancelled`, `401`, `403`.

### PUT `/api/pause-enrollment-requests/{id}/cancel`

Hủy yêu cầu. Theo domain error hiện có, cancel có thể bị chặn nếu quá cửa sổ cancel.

Success: empty success.

Errors: `404 PauseEnrollmentRequest.NotFound`, `409 PauseEnrollmentRequest.AlreadyCancelled`, `409 PauseEnrollmentRequest.CancelWindowExpired`, `401`.

### PUT `/api/pause-enrollment-requests/{id}/outcome`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `outcome` | `PauseEnrollmentOutcome` | Yes | `ContinueSameClass`, `ReassignEquivalentClass`, `ContinueWithTutoring`. |
| `outcomeNote` | `string?` | No | Ghi chú tư vấn. |

Success: empty success.

Logic:

- Chỉ set outcome cho request đã `Approved`.
- Nếu outcome là `ReassignEquivalentClass`, BE hủy assignments sau ngày bảo lưu để chuẩn bị xếp lớp mới.
- Nếu outcome là `ReassignEquivalentClass` hoặc `ContinueWithTutoring`, BE tạo notification/follow-up cho staff.

Errors: `404 PauseEnrollmentRequest.NotFound`, `409 PauseEnrollmentRequest.OutcomeNotAllowed`, `409 PauseEnrollmentRequest.OutcomeAlreadyCompleted`, `401`, `403`.

### POST `/api/pause-enrollment-requests/{id}/reassign-equivalent-class`

Dùng khi outcome đã là `ReassignEquivalentClass`.

Body:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `registrationId` | `Guid` | Yes | - | Registration của học sinh. |
| `newClassId` | `Guid` | Yes | - | Lớp tương đương mới. |
| `track` | `string` | No | `primary` | `primary` hoặc `secondary`. |
| `sessionSelectionPattern` | `string?` | No | null | RRULE subset cho chương trình phụ/lịch chọn một phần. |
| `effectiveDate` | `DateTime?` | No | Sau `pauseTo` hoặc hôm nay | Ngày hiệu lực vào lớp mới. |

Success: `ReassignEquivalentClassResponse` gồm `pauseEnrollmentRequestId`, `registrationId`, `oldClassId`, `oldClassName`, `newClassId`, `newClassName`, `droppedEnrollmentId`, `newEnrollmentId`, `track`, `effectiveDate`, `registrationStatus`, `outcomeCompletedAt`.

Logic:

- Validate request đã `Approved` và outcome là `ReassignEquivalentClass`.
- Validate registration thuộc đúng student của pause request.
- Tìm enrollment `Paused` từ history của pause request theo track.
- Drop enrollment cũ và hủy future assignments từ effective date.
- Tạo enrollment mới ở class mới, giữ tuition plan/registration/track.
- Nếu program là supplementary thì tạo `ClassEnrollmentScheduleSegment` cho enrollment mới.
- Sync assignments cho enrollment mới.
- Cập nhật class trong registration và resolve lại registration status.
- Mark outcome completed.

Errors: `404 PauseEnrollmentRequest.NotFound`, `404 Registration.NotFound`, `404 Registration.ClassNotFound`, `400 PauseEnrollmentRequest.OutcomeMustBeReassignEquivalentClass`, `409 PauseEnrollmentRequest.NoPausedEnrollmentToReassign`, `400 PauseEnrollmentRequest.EffectiveDateBeforePauseEnd`, `400 PauseEnrollmentRequest.RegistrationStudentMismatch`, `400/409 Registration.InvalidStatus`, `409 Registration.CannotTransferToSameClass`, `409 AlreadyEnrolled`, `409 Registration.ClassFull`, `400 ClassNotAvailable`, schedule conflict errors, `401`, `403`.

## Status definition

### PauseEnrollmentRequestStatus

| Status | Ý nghĩa |
| --- | --- |
| `Pending` | Yêu cầu mới tạo, chờ staff xử lý. |
| `Approved` | Staff duyệt, enrollment liên quan được pause. |
| `Rejected` | Staff từ chối. |
| `Cancelled` | Người dùng hủy yêu cầu. |

Luồng chuyển:

```text
Pending -- approve --> Approved
Pending -- reject --> Rejected
Pending -- cancel --> Cancelled
Approved -- outcome --> Approved + outcome set
Approved + outcome ReassignEquivalentClass -- reassign-equivalent-class --> outcomeCompletedAt set
```

### PauseEnrollmentOutcome

| Outcome | Ý nghĩa |
| --- | --- |
| `ContinueSameClass` | Sau bảo lưu, học sinh tiếp tục lớp cũ. |
| `ReassignEquivalentClass` | Sau bảo lưu, học sinh được xếp lớp tương đương mới. |
| `ContinueWithTutoring` | Sau tư vấn, tiếp tục theo hướng học kèm/chương trình phụ; hiện code chủ yếu tạo follow-up, không tự tạo registration mới. |

## Permission matrix

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/pause-enrollment-requests` | Yes | Yes | Yes | Yes | Yes | No |
| `GET /api/pause-enrollment-requests` | Yes | Yes | Yes | Yes | Yes | No |
| `GET /api/pause-enrollment-requests/{id}` | Yes | Yes | Yes | Yes | Yes | No |
| `PUT /approve` | Yes | Yes | No | No | No | No |
| `PUT /approve-bulk` | Yes | Yes | No | No | No | No |
| `PUT /reject` | Yes | Yes | No | No | No | No |
| `PUT /cancel` | Yes | Yes | Yes | Yes | Yes | No |
| `PUT /outcome` | Yes | Yes | No | No | No | No |
| `POST /reassign-equivalent-class` | Yes | Yes | No | No | No | No |

## Validation rules

| Rule | API áp dụng | Lỗi |
| --- | --- | --- |
| Đăng nhập bắt buộc | Tất cả | 401 |
| Role đúng | Approve/reject/outcome/reassign | 403 |
| `studentProfileId` bắt buộc và tồn tại | Create | 404 |
| `pauseFrom` không ở quá khứ | Create | 400 validation |
| `pauseTo >= pauseFrom` | Create | 400 validation |
| Học sinh phải có active enrollment có session trong khoảng bảo lưu | Create, approve | 409 `NoEnrollmentsInRange` |
| Không có pending/approved request trùng khoảng ngày | Create | 409 `DuplicateActiveRequest` |
| Chỉ request pending mới nên approve/reject/cancel | Approve/reject/cancel | 409 already status errors |
| Outcome chỉ được set khi request `Approved` | Outcome, reassign | 409 `OutcomeNotAllowed` |
| Không set/reassign lại nếu outcome đã completed | Outcome, reassign | 409 `OutcomeAlreadyCompleted` |
| Reassign yêu cầu outcome là `ReassignEquivalentClass` | Reassign | 400 |
| Registration phải thuộc đúng student bảo lưu | Reassign | 400 |
| Effective date phải sau pause end | Reassign | 400 |
| Lớp mới phải cùng program, còn slot, status phù hợp | Reassign | 400/409 |
| Không được reassign vào cùng lớp cũ | Reassign | 409 |
| Lịch mới không được conflict với lịch học khác của student | Reassign | 409 schedule conflict |

