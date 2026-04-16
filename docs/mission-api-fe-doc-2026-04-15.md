# Tài Liệu API FE - Mission - 2026-04-15

Tài liệu này mô tả các API trong `MissionController.cs`.

Ghi chú:

- Teacher chỉ được xem, tạo, cập nhật mission cho lớp mình dạy hoặc học sinh trong lớp mình dạy.
- Teacher được tính là người dạy nếu là `MainTeacherId` hoặc `AssistantTeacherId`.
- Guard quyền đọc/tạo/sửa hiện đang áp dụng riêng cho Teacher. Parent/Student vẫn được attribute cho phép gọi list/detail/progress; endpoint own nên dùng `/me/progress`.

## Role và phạm vi dữ liệu

Controller có `[Authorize]`, tất cả API yêu cầu đăng nhập.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động |
| --- | --- | --- | --- |
| Admin | Missions và progress | `all` | `view`, `create`, `edit`, `delete`, `view_progress` |
| ManagementStaff | Missions và progress | `all` | `view`, `create`, `edit`, `delete`, `view_progress` |
| Teacher | Mission target lớp mình dạy hoặc học sinh trong lớp mình dạy | `own` | `view`, `create`, `edit`, `view_progress` |
| Parent | Theo code hiện tại có thể gọi list/detail/progress; own progress dùng `/me/progress` | `all` với list/detail, `own` với `/me/progress` | `view`, `view_own_progress` |
| Student | Theo code hiện tại có thể gọi list/detail/progress; own progress dùng `/me/progress` | `all` với list/detail, `own` với `/me/progress` | `view`, `view_own_progress` |
| Anonymous | Không được truy cập | `none` | `none` |

## Response format

Success:

```json
{ "isSuccess": true, "data": {} }
```

Error:

```json
{
  "title": "Mission.TeacherCannotTargetClass",
  "status": 400,
  "detail": "Teacher can only target classes they are assigned to."
}
```

## Enum

| Enum | Values |
| --- | --- |
| `MissionScope` | `Class`, `Student`, `Group` |
| `MissionType` | `HomeworkStreak`, `ReadingStreak`, `NoUnexcusedAbsence`, `ClassAttendance`, `Custom` |
| `MissionProgressMode` | `Count`, `Streak` |
| `MissionProgressStatus` | `Assigned`, `InProgress`, `Completed`, `Expired` |

## Danh sách API

| Method | Endpoint | Roles | Mô tả |
| --- | --- | --- | --- |
| POST | `/api/missions` | Admin, ManagementStaff, Teacher | Tạo mission và progress records cho target students. |
| GET | `/api/missions` | Admin, ManagementStaff, Teacher, Parent, Student | Lấy danh sách mission. |
| GET | `/api/missions/{id}` | Admin, ManagementStaff, Teacher, Parent, Student | Xem chi tiết mission. |
| PUT | `/api/missions/{id}` | Admin, ManagementStaff, Teacher | Cập nhật mission. |
| DELETE | `/api/missions/{id}` | Admin, ManagementStaff | Xóa mission nếu chưa có progress. |
| GET | `/api/missions/{id}/progress` | Admin, ManagementStaff, Teacher, Parent, Student | Xem progress của mission. |
| GET | `/api/missions/me/progress` | Admin, ManagementStaff, Teacher, Parent, Student | Xem progress của student hiện tại. |

### POST `/api/missions`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `title` | `string` | Yes | Tên mission. |
| `description` | `string?` | No | Mô tả. |
| `scope` | `MissionScope` | Yes | `Class`, `Student`, `Group`. |
| `targetClassId` | `Guid?` | Required khi `scope = Class` | Lớp target. |
| `targetStudentId` | `Guid?` | Required khi `scope = Student` | Học sinh target. |
| `targetGroup` | `array<Guid>?` | Required khi `scope = Group` | Danh sách học sinh target. |
| `missionType` | `MissionType` | Yes | Loại mission. |
| `progressMode` | `MissionProgressMode` | No | Default `Count`. |
| `startAt` | `DateTime?` | No | Thời gian bắt đầu. |
| `endAt` | `DateTime?` | No | Phải sau `startAt` nếu có cả hai. |
| `totalRequired` | `int?` | No | Tổng yêu cầu để hoàn thành, dùng resolve reward rule. |

Success `201`: `CreateMissionResponse` gồm `id`, title/description, scope/targets, `missionType`, `progressMode`, `startAt`, `endAt`, `rewardStars`, `rewardExp`, `totalRequired`, `createdBy`, `createdAt`.

Errors: `400 Mission.InvalidScope`, `400 Mission.InvalidDateRange`, `400 Mission.TeacherCannotTargetClass`, `400 Mission.TeacherCannotTargetStudent`, `400 Mission.TeacherCannotTargetSomeStudents`, `400 MissionRewardRule.NotConfigured`, `404 Mission.ClassNotFound`, `404 Mission.StudentNotFound`, `404 Mission.SomeStudentsNotFound`, `401`, `403`.

### GET `/api/missions`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `scope` | `MissionScope?` | No | null |
| `targetClassId` | `Guid?` | No | null |
| `targetStudentId` | `Guid?` | No | null |
| `missionType` | `MissionType?` | No | null |
| `searchTerm` | `string?` | No | null |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: `{ missions: Page<MissionDto> }`.

`MissionDto`: `id`, `title`, `description`, `scope`, `targetClassId`, `targetClassCode`, `targetClassTitle`, `targetStudentId`, `targetGroup`, `missionType`, `progressMode`, `startAt`, `endAt`, `rewardStars`, `rewardExp`, `totalRequired`, `createdBy`, `createdByName`, `createdAt`.

Errors: `401`, `403`; enum query invalid có thể `400`.

### GET `/api/missions/{id}`

Path: `id: Guid`.

Success: `GetMissionByIdResponse`, field giống `MissionDto`.

Errors: `400 Mission.TeacherCannotViewMission`, `404 Mission.NotFound`, `401`, `403`.

### PUT `/api/missions/{id}`

Body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `title` | `string` | Yes | Tên mission. |
| `description` | `string?` | No | Mô tả. |
| `scope` | `MissionScope` | Yes | `Class`, `Student`, `Group`. |
| `targetClassId` | `Guid?` | Required khi `scope = Class` | Lớp target. |
| `targetStudentId` | `Guid?` | Required khi `scope = Student` | Học sinh target. |
| `targetGroup` | `array<Guid>?` | Required khi `scope = Group` | Danh sách học sinh target. |
| `missionType` | `MissionType` | Yes | Loại mission. |
| `progressMode` | `MissionProgressMode?` | No | Mode mới. |
| `startAt` | `DateTime?` | No | Thời gian bắt đầu. |
| `endAt` | `DateTime?` | No | Thời gian kết thúc. |
| `totalRequired` | `int?` | No | Tổng yêu cầu. |

Success: `UpdateMissionResponse`, field tương tự create.

Errors: giống create, thêm `404 Mission.NotFound`.

### DELETE `/api/missions/{id}`

Success: response delete mission.

Errors: `404 Mission.NotFound`, `409 Mission.MissionInUse`, `401`, `403`.

### GET `/api/missions/{id}/progress`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | null |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: `{ mission, progresses }`.

`mission`: `id`, `title`, `missionType`, `progressMode`, `totalRequired`.

`progresses.items[]`: `id`, `missionId`, `studentProfileId`, `studentName`, `status`, `progressValue`, `totalRequired`, `progressPercentage`, `completedAt`, `verifiedBy`, `verifiedByName`.

Errors: `404 Mission.NotFound`, `401`, `403`.

### GET `/api/missions/me/progress`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success: progress của `userContext.StudentId`.

Errors: lỗi khi token không resolve được student profile, `401`, `403`.

## Status definition

Mission entity không có status lifecycle riêng. Status nằm ở `MissionProgressStatus`.

| Status | Ý nghĩa |
| --- | --- |
| `Assigned` | Mission đã được giao. |
| `InProgress` | Đang có tiến độ. |
| `Completed` | Hoàn thành, có thể cộng reward theo rule. |
| `Expired` | Quá hạn và chưa hoàn thành. |

Luồng chuyển trạng thái:

```text
Create mission -> tạo MissionProgress cho target students
Assigned -> InProgress khi tracker cập nhật progress
InProgress -> Completed khi đạt totalRequired
Assigned/InProgress -> Expired khi quá hạn
```

## Permission matrix

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/missions` | Yes | Yes | Yes, scope lớp/học sinh mình dạy | No | No | No |
| `GET /api/missions` | Yes | Yes | Yes, filter theo lớp/học sinh mình dạy | Yes | Yes | No |
| `GET /api/missions/{id}` | Yes | Yes | Yes, mission readable | Yes | Yes | No |
| `PUT /api/missions/{id}` | Yes | Yes | Yes, target mình quản lý | No | No | No |
| `DELETE /api/missions/{id}` | Yes | Yes | No | No | No | No |
| `GET /api/missions/{id}/progress` | Yes | Yes | Yes | Yes | Yes | No |
| `GET /api/missions/me/progress` | Yes | Yes | Yes | Yes | Yes | No |

## Validation rules

| Rule | API áp dụng | Lỗi |
| --- | --- | --- |
| Đăng nhập bắt buộc | Tất cả | 401 |
| Role đúng | Tất cả | 403 |
| `scope = Class` cần `targetClassId` | Create, update | 400 `Mission.InvalidScope` |
| `scope = Student` cần `targetStudentId` | Create, update | 400 `Mission.InvalidScope` |
| `scope = Group` cần `targetGroup` | Create, update | 400 `Mission.InvalidScope` |
| `endAt` phải sau `startAt` | Create, update | 400 `Mission.InvalidDateRange` |
| Target class/student phải tồn tại | Create, update | 404 |
| Teacher chỉ target lớp mình dạy | Create, update | 400 `Mission.TeacherCannotTargetClass` |
| Teacher chỉ target student trong lớp mình dạy | Create, update | 400 |
| Teacher chỉ đọc mission thuộc lớp/học sinh mình dạy | List, detail | Detail trả 400 `Mission.TeacherCannotViewMission` |
| Reward rule phải được cấu hình nếu mission cần reward | Create, update | 400 `MissionRewardRule.NotConfigured` |
| Không delete mission đã có progress | Delete | 409 `Mission.MissionInUse` |

