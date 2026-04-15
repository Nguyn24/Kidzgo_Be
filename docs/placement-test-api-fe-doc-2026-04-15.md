# Tài Liệu API FE - Placement Test - 2026-04-15

Tài liệu này mô tả các API trong `PlacementTestController.cs`.

Ghi chú cho FE:

- Request tạo/sửa/retake placement test dùng `roomId`; không cần gửi field `room`.
- BE tự lấy `Classroom.Name` theo `roomId` để set `roomName` và `room`.
- Placement test theo mô hình 1 bé, 1 phòng, 1 người phụ trách trong cùng khung giờ.
- Availability check xem người phụ trách có trùng placement test khác hoặc lịch dạy trong teacher timetable không; phòng cũng không được trùng placement test/session khác.
- `durationMinutes` default 60 nếu không gửi.

## Role và phạm vi dữ liệu

Controller có `[Authorize]`, tất cả API yêu cầu đăng nhập.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động |
| --- | --- | --- | --- |
| Admin | Placement tests, availability, result, question matrix | `all` | `view`, `create`, `edit`, `cancel`, `mark_no_show`, `update_result`, `generate_questions`, `add_note`, `convert`, `retake` |
| ManagementStaff | Placement tests, availability, result, question matrix | `all` | `view`, `create`, `edit`, `cancel`, `mark_no_show`, `update_result`, `generate_questions`, `add_note`, `convert`, `retake` |
| AccountantStaff | Placement tests | `all` | `view` |
| Teacher/Parent/Student/Anonymous | Không được truy cập | `none` | `none` |

## Response format

Success:

```json
{ "isSuccess": true, "data": {} }
```

Domain error:

```json
{
  "title": "PlacementTest.RoomUnavailable",
  "status": 400,
  "detail": "The room with Id = '...' is not available at the selected time"
}
```

Một số validation trực tiếp trong controller trả string `400`, ví dụ `"Invalid level: Expert"`.

## Danh sách API

| Method | Endpoint | Roles | Mô tả |
| --- | --- | --- | --- |
| POST | `/api/placement-tests` | Admin, ManagementStaff | Đặt lịch placement test cho lead/lead child. |
| GET | `/api/placement-tests` | Admin, ManagementStaff, AccountantStaff | Lấy danh sách placement test. |
| GET | `/api/placement-tests/availability` | Admin, ManagementStaff | Check invigilator và phòng rảnh trong khung giờ. |
| GET | `/api/placement-tests/available-invigilators` | Admin, ManagementStaff | Alias cũ, trả cùng format availability. |
| GET | `/api/placement-tests/{id}` | Admin, ManagementStaff, AccountantStaff | Xem chi tiết placement test. |
| PUT | `/api/placement-tests/{id}` | Admin, ManagementStaff | Cập nhật lịch/phòng/invigilator/student/class. |
| POST | `/api/placement-tests/{id}/cancel` | Admin, ManagementStaff | Hủy placement test. |
| POST | `/api/placement-tests/{id}/no-show` | Admin, ManagementStaff | Đánh dấu không đến test. |
| PUT | `/api/placement-tests/{id}/results` | Admin, ManagementStaff | Nhập/cập nhật kết quả test. |
| POST | `/api/placement-tests/{id}/questions/from-bank-matrix` | Admin, ManagementStaff | Tạo câu hỏi từ question bank theo ma trận level. |
| POST | `/api/placement-tests/{id}/notes` | Admin, ManagementStaff | Thêm note. |
| POST | `/api/placement-tests/{id}/convert-to-enrolled` | Admin, ManagementStaff | Convert lead/child sang enrolled flow. |
| POST | `/api/placement-tests/{id}/retake` | Admin, ManagementStaff | Tạo placement test thi lại. |

### POST `/api/placement-tests`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `leadId` | `Guid?` | Required nếu không có `leadChildId` | Lead cần đặt lịch. |
| `leadChildId` | `Guid?` | Required nếu không có `leadId` | Ưu tiên hơn `leadId`; nếu gửi cả 2 thì phải match. |
| `scheduledAt` | `DateTime` | Yes | Không được ở quá khứ. |
| `durationMinutes` | `int?` | No | Phải > 0, default 60. |
| `roomId` | `Guid?` | Yes | Phòng active, đúng branch nếu có branch context, không trùng lịch. |
| `invigilatorUserId` | `Guid?` | Yes | Staff/teacher phụ trách, không trùng lịch. |

Success `201`: `SchedulePlacementTestResponse` gồm `id`, `leadId`, `leadChildId`, `studentProfileId`, `classId`, `scheduledAt`, `durationMinutes`, `status`, `roomId`, `roomName`, `room`, `invigilatorUserId`, `createdAt`.

Errors: `400 LeadId`, `400 PlacementTest.InvalidDuration`, `400 PlacementTest.InvigilatorRequired`, `400 PlacementTest.RoomRequired`, `400 PlacementTest.InvigilatorInvalidRole`, `400 PlacementTest.InvigilatorUnavailable`, `400 PlacementTest.RoomBranchMismatch`, `400 PlacementTest.RoomUnavailable`, `404 Lead.NotFound`, `404 LeadChild`, `404 PlacementTest.RoomNotFound`, `404 PlacementTest.InvigilatorNotFound`, `401`, `403`.

### GET `/api/placement-tests`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `leadId` | `Guid?` | No | null |
| `studentProfileId` | `Guid?` | No | null |
| `status` | `PlacementTestStatus?` | No | null |
| `fromDate` | `DateTime?` | No | null |
| `toDate` | `DateTime?` | No | null |
| `sortBy` | `string?` | No | null |
| `sortOrder` | `string` | No | `Descending` |
| `page` | `int` | No | 1 |
| `pageSize` | `int` | No | 20 |

Success: `{ placementTests, totalCount, page, pageSize, totalPages }`. Mỗi item gồm thông tin lead/child/student/class, lịch, phòng, invigilator, điểm, recommendation, notes, attachment, flags `isAccountProfileCreated`, `isConvertedToEnrolled`, `createdAt`, `updatedAt`.

Errors: enum query invalid có thể `400`; `401`; `403`.

### GET `/api/placement-tests/availability`

Query:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `scheduledAt` | `DateTime` | Yes | - | Giờ bắt đầu. |
| `durationMinutes` | `int?` | No | 60 | Số phút cần giữ lịch. |
| `branchId` | `Guid?` | No | null | Lọc theo branch. |
| `excludePlacementTestId` | `Guid?` | No | null | Bỏ qua test hiện tại khi edit. |
| `includeUnavailable` | `bool` | No | false | Trả cả item bận kèm conflict. |

Success: `{ scheduledAt, endAt, durationMinutes, items, rooms }`.

`items[]`: `userId`, `name`, `email`, `role`, `branchId`, `isAvailable`, `conflicts`.

`rooms[]`: `roomId`, `name`, `branchId`, `capacity`, `isAvailable`, `conflicts`.

`conflicts[]`: `type`, `referenceId`, `title`, `startAt`, `endAt`.

Errors: `400 PlacementTest.InvalidDuration`, `401`, `403`.

### GET `/api/placement-tests/available-invigilators`

Query và response giống `/availability`.

### GET `/api/placement-tests/{id}`

Path: `id: Guid`.

Success: `GetPlacementTestByIdResponse`, gồm đầy đủ thông tin lịch, phòng, invigilator, điểm, recommendation chính/phụ, notes, attachment, created/updated flags.

Errors: `404 PlacementTest.NotFound`, `401`, `403`.

### PUT `/api/placement-tests/{id}`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `scheduledAt` | `DateTime?` | No | Không được ở quá khứ. |
| `durationMinutes` | `int?` | No | Phải > 0. |
| `roomId` | `Guid?` | No | Nếu đổi schedule/phòng/invigilator thì check conflict lại. |
| `invigilatorUserId` | `Guid?` | No | Người phụ trách mới. |
| `studentProfileId` | `Guid?` | No | Gắn student profile. |
| `classId` | `Guid?` | No | Gắn class. |

Success: `UpdatePlacementTestResponse`.

Errors: `400 PlacementTest.CannotUpdateCompletedTest`, `400 PlacementTest.InvalidDuration`, `400 PlacementTest.InvigilatorRequired`, `400 PlacementTest.RoomRequired`, `400 PlacementTest.InvigilatorUnavailable`, `400 PlacementTest.RoomUnavailable`, `404 PlacementTest.NotFound`, `404 PlacementTest.StudentProfileNotFound`, `404 PlacementTest.ClassNotFound`, `401`, `403`.

### POST `/api/placement-tests/{id}/cancel`

Body optional: `reason: string?`.

Success: `{ id, status: "Cancelled", updatedAt }`.

Errors: `400 PlacementTest.CannotCancelCompletedTest`, `404 PlacementTest.NotFound`, `401`, `403`.

### POST `/api/placement-tests/{id}/no-show`

Success: `{ id, status: "NoShow", updatedAt }`.

Errors: `400 PlacementTest.CannotMarkNoShowCompletedTest`, `404 PlacementTest.NotFound`, `401`, `403`.

### PUT `/api/placement-tests/{id}/results`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `listeningScore` | `decimal?` | No | Điểm listening. |
| `speakingScore` | `decimal?` | No | Điểm speaking. |
| `readingScore` | `decimal?` | No | Điểm reading. |
| `writingScore` | `decimal?` | No | Điểm writing. |
| `resultScore` | `decimal?` | No | Điểm tổng. |
| `programRecommendationId` | `Guid?` | No | Program chính; gửi `Guid.Empty` để clear. |
| `secondaryProgramRecommendationId` | `Guid?` | No | Program phụ; gửi `Guid.Empty` để clear. |
| `secondaryProgramSkillFocus` | `string?` | No | Skill focus chương trình phụ. |
| `attachmentUrl` | `string?` | No | File kết quả/minh chứng. |

Success: `UpdatePlacementTestResultsResponse`, có `newRegistrationId` nếu đây là retake và BE tự tạo registration mới.

Errors: `400 PlacementTest.SecondaryProgramDuplicated`, `404 PlacementTest.NotFound`, `404 PlacementTest.ProgramRecommendationNotFound`, `404 PlacementTest.SecondaryProgramRecommendationNotFound`, `401`, `403`.

### POST `/api/placement-tests/{id}/questions/from-bank-matrix`

Body:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `programId` | `Guid?` | No | null |
| `questionType` | `string?` | No | `MultipleChoice` |
| `skill` | `string?` | No | null |
| `topic` | `string?` | No | null |
| `shuffleQuestions` | `bool` | No | true |
| `totalQuestions` | `int` | Yes | - |
| `distribution` | `array` | Yes | [] |
| `distribution[].level` | `string` | Yes | - |
| `distribution[].count` | `int` | Yes | - |

Success: `{ placementTestId, programId, programSource, questionType, skill, topic, requestedQuestionCount, createdQuestionCount, totalPoints, distribution, questions }`.

Errors: `400 Invalid question type`, `400 Invalid level`, `400 PlacementTest.InvalidQuestionMatrixDistribution`, `400 PlacementTest.MatrixTotalMismatch`, `400 PlacementTest.InsufficientQuestionsInBank`, `400 PlacementTest.ProgramNotResolved`, `404 PlacementTest.NotFound`, `401`, `403`.

### POST `/api/placement-tests/{id}/notes`

Body: `note: string` required.

Success: `{ id, notes, updatedAt }`.

Errors: `400 validation`, `404 PlacementTest.NotFound`, `401`, `403`.

### POST `/api/placement-tests/{id}/convert-to-enrolled`

Body optional: `studentProfileId: Guid?`.

Success: `{ leadId, leadStatus, placementTestId, placementTestStatus, studentProfileId, leadChildId }`.

Errors: `404 PlacementTest.NotFound`, `404 PlacementTest.StudentProfileNotFound`, `409 PlacementTest.StudentProfileAlreadyAssigned`, `409 PlacementTest.LeadAlreadyEnrolled`, `401`, `403`.

### POST `/api/placement-tests/{id}/retake`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Student thi lại. |
| `newProgramId` | `Guid` | Yes | Program mục tiêu. |
| `newTuitionPlanId` | `Guid` | Yes | Tuition plan mục tiêu. |
| `branchId` | `Guid` | Yes | Branch dùng để check phòng/người phụ trách. |
| `scheduledAt` | `DateTime?` | No | Nếu có thì cần `roomId` và `invigilatorUserId`. |
| `durationMinutes` | `int?` | No | Default 60, phải > 0. |
| `roomId` | `Guid?` | No | Phòng thi lại. |
| `invigilatorUserId` | `Guid?` | No | Người phụ trách. |
| `note` | `string?` | No | Ghi chú. |

Success: `{ newPlacementTestId, originalPlacementTestId, studentProfileId, originalProgramName, newProgramName, originalTuitionPlanName, newTuitionPlanName, originalRemainingSessions, placementTestStatus, scheduledAt, durationMinutes, roomId, roomName, room, invigilatorUserId, createdAt }`.

Errors: `404 PlacementTest.NotFound`, `404 PlacementTest.StudentProfileNotFound`, `404 Registration.BranchNotFound`, `404 Registration.ProgramNotFound`, `404 Registration.TuitionPlanNotFound`, `409 PlacementTest.RetakeAlreadyScheduled`, `400 PlacementTest.InvalidDuration`, `400 PlacementTest.InvigilatorRequired`, `400 PlacementTest.RoomRequired`, `400 PlacementTest.InvigilatorUnavailable`, `400 PlacementTest.RoomUnavailable`, `401`, `403`.

## Status definition

| Status | Ý nghĩa |
| --- | --- |
| `Scheduled` | Đã đặt lịch. |
| `NoShow` | Học sinh không đến test. |
| `Completed` | Đã nhập đủ điểm và hoàn tất. |
| `Cancelled` | Đã hủy. |

Luồng chuyển trạng thái:

```text
Create / Retake -> Scheduled
Scheduled hoặc Cancelled -> NoShow qua POST /no-show
Scheduled hoặc NoShow -> Cancelled qua POST /cancel
Scheduled hoặc NoShow hoặc Cancelled -> Completed khi PUT /results có đủ 5 điểm
Completed -> không được update schedule, cancel, no-show
```

## Permission matrix

| API | Admin | ManagementStaff | AccountantStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Create/update/cancel/no-show/results/questions/notes/convert/retake | Yes | Yes | No | No | No | No | No |
| List/detail placement tests | Yes | Yes | Yes | No | No | No | No |
| Availability APIs | Yes | Yes | No | No | No | No | No |

## Validation rules

| Rule | API áp dụng | Lỗi |
| --- | --- | --- |
| Đăng nhập bắt buộc | Tất cả | 401 |
| Role đúng | Tất cả | 403 |
| Có `leadId` hoặc `leadChildId` khi tạo | Create | 400 |
| `scheduledAt` không ở quá khứ | Create, update | 400 validation |
| `durationMinutes > 0` | Create, update, availability, retake | 400 |
| `roomId` bắt buộc khi đặt lịch cụ thể | Create, update schedule, retake có lịch | 400 `PlacementTest.RoomRequired` |
| `invigilatorUserId` bắt buộc khi đặt lịch cụ thể | Create, update schedule, retake có lịch | 400 `PlacementTest.InvigilatorRequired` |
| Invigilator không được bận placement test/timetable | Create, update, retake | 400 `PlacementTest.InvigilatorUnavailable` |
| Phòng phải active, đúng branch, không trùng lịch | Create, update, retake | 400/404 |
| Completed không được update schedule/cancel/no-show | Update, cancel, no-show | 400 |
| Program recommendation phải active | Results | 404 |
| Program phụ không được trùng program chính | Results | 400 |
| Matrix distribution phải match tổng và đủ question bank | Questions from bank matrix | 400 |

