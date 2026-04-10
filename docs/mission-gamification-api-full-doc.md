# Mission & Gamification API Full Doc

Tài liệu này tổng hợp đầy đủ API hiện có trong:

- `Kidzgo.API/Controllers/MissionController.cs`
- `Kidzgo.API/Controllers/GamificationController.cs`

Mục tiêu của tài liệu là mô tả đúng contract backend hiện tại để FE, BE và QA có thể cùng nhìn trên một mặt bằng thống nhất: role nào xem được gì, scope dữ liệu thực tế là gì, body/query cần truyền gì, response trả về ra sao, status nào đang được dùng và những điểm lệch giữa quyền nghiệp vụ với implementation hiện tại.

## 1. Mục tiêu và phạm vi

- Tài liệu này mô tả theo implementation hiện tại trong controller + handler + DTO + error class.
- Khi có khác biệt giữa “ý nghĩa nghiệp vụ” và “scope thực tế đang chạy”, tài liệu sẽ ghi rõ.
- Tài liệu bao phủ 2 nhóm API:
  - `Mission APIs`
  - `Gamification APIs`
- Tài liệu không mô tả UI flow phía FE, chỉ mô tả BE contract.

## 2. Vai trò, dữ liệu được xem, scope và hành động

| Role / Context | Dữ liệu xem được | Scope thực tế hiện tại | Hành động được phép | Ghi chú |
| --- | --- | --- | --- | --- |
| `Admin` | Toàn bộ mission, mission progress, star/xp, streak, reward store, reward redemption, settings | `all` | `view`, `create`, `edit`, `delete`, `approve`, `cancel`, `deliver`, `update settings` | Quyền rộng nhất |
| `ManagementStaff` | Tương tự `Admin` ở module này | `all` | `view`, `create`, `edit`, `delete`, `approve`, `cancel`, `deliver`, `update settings` | Không thấy filter theo branch trong các handler |
| `Teacher` | Mission, mission progress, manual stars/xp, star transactions, balance/level, attendance streak, reward redemption list | Pha trộn giữa `department/intended` và `all thực tế` | `view`, `create mission`, `edit mission`, `add/deduct stars`, `add/deduct xp` | Nhiều API chưa chặn theo lớp/học sinh giáo viên phụ trách |
| `Parent` | Mission list/detail/progress, active reward store items, các API “me” nếu token có `StudentId` | Chủ yếu là `selected student in token`, nhưng có vài API mission/reward redemption chi tiết đang rộng hơn | `view` | `GetMission*` và `GetRewardRedemptionById` hiện chưa chặn theo child linked |
| `Student` | Mission của mình, star balance, level, attendance streak, active reward store items, reward redemption của mình | `own` hoặc `selected student in token` | `view`, `request reward`, `confirm received`, `check-in` | Một số endpoint “me” thực chất dựa vào `StudentId` claim chứ không ép role `Student` |
| `Authenticated user có StudentId trong token` | Các API “me” và một số action self-service | `own selected student in token` | `view`, `check-in`, `request reward`, `confirm received` | Thực tế controller cho phép rộng hơn role nghiệp vụ |

Quy ước scope trong tài liệu:

- `own`: dữ liệu của chính student hiện tại.
- `selected student in token`: dữ liệu của `StudentId` claim đang có trong token.
- `all`: không có filter theo branch, lớp, người tạo hoặc chủ sở hữu trong handler.

## 3. Cấu trúc response chung

### 3.1. Envelope thành công

Các API dùng `MatchOk()` hoặc `MatchCreated()` đều trả `ApiResult<T>`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 3.2. Response phân trang

Các API trả `Page<T>` có dạng:

```json
{
  "isSuccess": true,
  "data": {
    "items": [],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 0
  }
}
```

Một số API không dùng `Page<T>` mà trả cấu trúc custom như:

```json
{
  "isSuccess": true,
  "data": {
    "transactions": [],
    "totalCount": 0,
    "page": 1,
    "pageSize": 20,
    "totalPages": 0
  }
}
```

### 3.3. Format lỗi

Khi handler trả `Result.Failure(...)`, API dùng `Results.Problem(...)` theo dạng:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Mission.InvalidScope",
  "status": 400,
  "detail": "Invalid mission scope. For Class scope, TargetClassId is required. For Student scope, TargetStudentId is required. For Group scope, TargetGroup is required."
}
```

Nếu lỗi là `ValidationError`, response có thể kèm thêm `errors`.

### 3.4. HTTP status chung

| HTTP status | Khi nào |
| --- | --- |
| `200 OK` | Query / update thành công |
| `201 Created` | Create thành công |
| `400 Bad Request` | Validation error, invalid transition, input không hợp lệ |
| `401 Unauthorized` | Thiếu hoặc hết hạn token |
| `403 Forbidden` | Có token nhưng không đủ role theo `[Authorize(Roles = ...)]` |
| `404 Not Found` | Không tìm thấy entity hoặc thiếu `StudentId` / profile phù hợp |
| `409 Conflict` | Xung đột nghiệp vụ như thiếu sao, thiếu số lượng, mission đang được dùng |
| `500 Internal Server Error` | Lỗi hệ thống hoặc null reference không được map thủ công |

## 4. Bảng tổng hợp endpoint

| Endpoint | Method | Role chính | Scope thực tế | Ghi chú |
| --- | --- | --- | --- | --- |
| `/api/missions` | `POST` | Admin, ManagementStaff, Teacher | `all` với Admin/Management; `teacher-scoped targets` khi tạo | Teacher chỉ bị chặn ở target class/student/group |
| `/api/missions` | `GET` | Admin, ManagementStaff, Teacher, Parent, Student | `all` | Chưa filter theo owner / linked student |
| `/api/missions/{id}` | `GET` | Admin, ManagementStaff, Teacher, Parent, Student | `all` | Chỉ cần biết `id` |
| `/api/missions/{id}` | `PUT` | Admin, ManagementStaff, Teacher | `all` với Admin/Management; Teacher bị chặn theo target mới | Chưa check teacher có phải creator không |
| `/api/missions/{id}` | `DELETE` | Admin, ManagementStaff | `all` | Xóa cứng, nhưng dễ bị chặn bởi `MissionInUse` |
| `/api/missions/{id}/progress` | `GET` | Admin, ManagementStaff, Teacher, Parent, Student | `all` | Chưa filter theo owner / linked student |
| `/api/missions/me/progress` | `GET` | Context có `StudentId` trong token | `selected student in token` | Tên route là `me` nhưng phụ thuộc `StudentId` claim |
| `/api/gamification/stars/add` | `POST` | Admin, ManagementStaff, Teacher | `all` | Không có teacher scope guard |
| `/api/gamification/stars/deduct` | `POST` | Admin, ManagementStaff, Teacher | `all` | Không có teacher scope guard |
| `/api/gamification/xp/add` | `POST` | Admin, ManagementStaff, Teacher | `all` | Không có teacher scope guard |
| `/api/gamification/xp/deduct` | `POST` | Admin, ManagementStaff, Teacher | `all` | Không có teacher scope guard |
| `/api/gamification/stars/transactions` | `GET` | Admin, ManagementStaff, Teacher | `all` | Chưa check student profile tồn tại |
| `/api/gamification/stars/balance` | `GET` | Admin, ManagementStaff, Teacher | `all` | Có thể trả `0` cho profile không có transaction |
| `/api/gamification/level` | `GET` | Admin, ManagementStaff, Teacher | `all` | Có thể trả `Level 1` / `0 XP` nếu chưa có dữ liệu |
| `/api/gamification/stars/balance/me` | `GET` | Context có `StudentId` trong token | `selected student in token` | Không ép role `Student` |
| `/api/gamification/level/me` | `GET` | Context có `StudentId` trong token | `selected student in token` | Không ép role `Student` |
| `/api/gamification/attendance-streak` | `GET` | Admin, ManagementStaff, Teacher | `all` | Không check profile tồn tại |
| `/api/gamification/attendance-streak/check-in` | `POST` | Context có `StudentId` trong token | `selected student in token` | Không ép role `Student` |
| `/api/gamification/attendance-streak/me` | `GET` | Context có `StudentId` trong token | `selected student in token` | Không ép role `Student` |
| `/api/gamification/reward-store/items` | `POST` | Admin, ManagementStaff | `all` | Tạo item mới |
| `/api/gamification/reward-store/items` | `GET` | Admin, ManagementStaff | `all` | List item chưa xóa mềm |
| `/api/gamification/reward-store/items/{id}` | `GET` | Admin, ManagementStaff | `all` | Detail item chưa xóa mềm |
| `/api/gamification/reward-store/items/active` | `GET` | Authenticated | `all active non-deleted` | Intended cho Student/Parent nhưng controller đang mở rộng hơn |
| `/api/gamification/reward-store/items/{id}` | `PUT` | Admin, ManagementStaff | `all` | Update item |
| `/api/gamification/reward-store/items/{id}` | `DELETE` | Admin, ManagementStaff | `all` | Soft delete |
| `/api/gamification/reward-store/items/{id}/toggle-status` | `PATCH` | Admin, ManagementStaff | `all` | Toggle `IsActive` |
| `/api/gamification/reward-redemptions` | `POST` | Context có `StudentId` trong token | `selected student in token` | Không ép role `Student` |
| `/api/gamification/reward-redemptions` | `GET` | Admin, ManagementStaff, Teacher | `all` | Teacher đang xem được toàn bộ |
| `/api/gamification/reward-redemptions/{id}` | `GET` | Authenticated | `all` | Chưa check owner hoặc role |
| `/api/gamification/reward-redemptions/me` | `GET` | Context có `StudentId` trong token | `selected student in token` | Không ép role `Student` |
| `/api/gamification/reward-redemptions/{id}/approve` | `PATCH` | Admin, ManagementStaff | `all` | Requested -> Approved |
| `/api/gamification/reward-redemptions/{id}/cancel` | `PATCH` | Admin, ManagementStaff | `all` | Requested/Approved -> Cancelled |
| `/api/gamification/reward-redemptions/{id}/mark-delivered` | `PATCH` | Admin, ManagementStaff | `all` | Approved -> Delivered |
| `/api/gamification/reward-redemptions/{id}/confirm-received` | `PATCH` | Context có `StudentId` trong token | `own selected student in token` | Delivered -> Received |
| `/api/gamification/reward-redemptions/batch-deliver` | `PATCH` | Admin, ManagementStaff | `all` | Batch Approved -> Delivered |
| `/api/gamification/settings` | `GET` | Admin, ManagementStaff | `all` | Settings toàn cục |
| `/api/gamification/settings` | `PUT` | Admin, ManagementStaff | `all` | Update settings toàn cục |

## 5. API Mission

### 5.1. Tạo mission

- Endpoint: `POST /api/missions`
- Role / scope / action:
  - `Admin`, `ManagementStaff`: `all` / `create`
  - `Teacher`: `teacher-scoped targets` / `create`
- Mô tả: tạo mission mới theo scope `Class`, `Student` hoặc `Group`, đồng thời auto-generate `MissionProgress` cho các student target và gửi in-app notification cho student nhận mission.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `title` | `string` | Có | Validator yêu cầu không rỗng |
| `description` | `string?` | Không | Mô tả mission |
| `scope` | `MissionScope` | Có | `Class`, `Student`, `Group` |
| `targetClassId` | `Guid?` | Điều kiện | Bắt buộc khi `scope = Class` |
| `targetStudentId` | `Guid?` | Điều kiện | Bắt buộc khi `scope = Student` |
| `targetGroup` | `Guid[]?` | Điều kiện | Bắt buộc khi `scope = Group` và phải có ít nhất 1 phần tử |
| `missionType` | `MissionType` | Có | `HomeworkStreak`, `ReadingStreak`, `NoUnexcusedAbsence`, `Custom` |
| `startAt` | `DateTime?` | Không | Nếu có phải >= hiện tại |
| `endAt` | `DateTime?` | Không | Nếu có phải > `startAt` theo handler; validator chặn `>= startAt` |
| `rewardStars` | `int?` | Không | Không có validator bắt buộc > 0 |
| `rewardExp` | `int?` | Không | Không có validator bắt buộc > 0 |
| `totalRequired` | `int?` | Không | Số mục tiêu cần đạt cho streak/custom mission |

Response success:

- `201 Created`
- `data`:
  - `id`
  - `title`
  - `description`
  - `scope`
  - `targetClassId`
  - `targetStudentId`
  - `targetGroup`
  - `missionType`
  - `startAt`
  - `endAt`
  - `rewardStars`
  - `rewardExp`
  - `totalRequired`
  - `createdBy`
  - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | `title` rỗng, `startAt` trong quá khứ, `endAt` trong quá khứ |
| `400` | `Mission.InvalidScope` | Thiếu target tương ứng với `scope` |
| `400` | `Mission.InvalidDateRange` | `endAt <= startAt` ở handler |
| `400` | `Mission.TeacherCannotTargetClass` | Teacher target class ngoài lớp mình phụ trách |
| `400` | `Mission.TeacherCannotTargetStudent` | Teacher target student ngoài lớp mình dạy |
| `400` | `Mission.TeacherCannotTargetSomeStudents` | Teacher target group có student ngoài phạm vi được dạy |
| `404` | `Mission.ClassNotFound` | `targetClassId` không tồn tại |
| `404` | `Mission.StudentNotFound` | `targetStudentId` không tồn tại |
| `404` | `Mission.SomeStudentsNotFound` | Một phần `targetGroup` không tồn tại |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc role được phép |

Validation và lưu ý:

- `CreateMissionCommandValidator` dùng `DateTime.UtcNow` tại thời điểm validate.
- Handler convert `startAt`, `endAt` sang UTC trước khi lưu.
- Khi tạo thành công, hệ thống auto tạo `MissionProgress` với trạng thái `Assigned` cho toàn bộ target students.
- Với `scope = Class`, target students được lấy từ `ClassEnrollments` có `Status = Active`.
- Nếu lớp không có học sinh active thì mission vẫn được tạo, nhưng không có `MissionProgress`.
- Mission tạo xong có thể rất khó xóa, vì có `MissionProgress` thì `DELETE` sẽ bị chặn bởi `Mission.MissionInUse`.

### 5.2. Danh sách mission

- Endpoint: `GET /api/missions`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student` / thực tế `all` / `view`
- Mô tả: lấy danh sách mission có hỗ trợ filter cơ bản và phân trang.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `scope` | `MissionScope?` | Không | Lọc theo scope |
| `targetClassId` | `Guid?` | Không | Lọc theo lớp target |
| `targetStudentId` | `Guid?` | Không | Lọc theo student target |
| `missionType` | `MissionType?` | Không | Lọc theo loại mission |
| `searchTerm` | `string?` | Không | Tìm trong `title` và `description` |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

Response success:

- `200 OK`
- `data.missions`
  - `items[]`
    - `id`
    - `title`
    - `description`
    - `scope`
    - `targetClassId`
    - `targetClassCode`
    - `targetClassTitle`
    - `targetStudentId`
    - `targetGroup`
    - `missionType`
    - `startAt`
    - `endAt`
    - `rewardStars`
    - `rewardExp`
    - `createdBy`
    - `createdByName`
    - `createdAt`
  - `pageNumber`
  - `totalPages`
  - `totalCount`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc role được phép |

Validation và lưu ý:

- Handler hiện không lọc theo user hiện tại, branch, linked student hay owner.
- `Parent` và `Student` về mặt implementation có thể query ra mission ngoài phạm vi của mình nếu biết filter phù hợp.
- Response list hiện không trả `totalRequired`.
- Chưa có validation cho `pageNumber`, `pageSize`; giá trị <= 0 có thể gây hành vi không mong muốn ở `Skip/Take`.

### 5.3. Chi tiết mission

- Endpoint: `GET /api/missions/{id}`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student` / thực tế `all` / `view`
- Mô tả: lấy chi tiết một mission theo `id`.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Mission id |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `title`
  - `description`
  - `scope`
  - `targetClassId`
  - `targetClassCode`
  - `targetClassTitle`
  - `targetStudentId`
  - `targetGroup`
  - `missionType`
  - `startAt`
  - `endAt`
  - `rewardStars`
  - `rewardExp`
  - `createdBy`
  - `createdByName`
  - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Mission.NotFound` | Không tìm thấy mission |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không thuộc role được phép |

Validation và lưu ý:

- Handler hiện không check quyền sở hữu hoặc linked child.
- Response detail cũng không trả `totalRequired`.

### 5.4. Cập nhật mission

- Endpoint: `PUT /api/missions/{id}`
- Role / scope / action:
  - `Admin`, `ManagementStaff`: `all` / `edit`
  - `Teacher`: target mới phải thuộc phạm vi được dạy / `edit`
- Mô tả: cập nhật thông tin mission.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Mission id |

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `title` | `string` | Có trên request DTO | Validator không cho rỗng khi có giá trị |
| `description` | `string?` | Không | Mô tả mission |
| `scope` | `MissionScope` | Có | `Class`, `Student`, `Group` |
| `targetClassId` | `Guid?` | Điều kiện | Bắt buộc khi `scope = Class` |
| `targetStudentId` | `Guid?` | Điều kiện | Bắt buộc khi `scope = Student` |
| `targetGroup` | `Guid[]?` | Điều kiện | Bắt buộc khi `scope = Group` |
| `missionType` | `MissionType` | Có | Loại mission |
| `startAt` | `DateTime?` | Không | Nếu có phải >= hiện tại |
| `endAt` | `DateTime?` | Không | Nếu có phải > `startAt` theo handler |
| `rewardStars` | `int?` | Không | Không có validator > 0 |
| `rewardExp` | `int?` | Không | Không có validator > 0 |
| `totalRequired` | `int?` | Không | Target value |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `title`
  - `description`
  - `scope`
  - `targetClassId`
  - `targetStudentId`
  - `targetGroup`
  - `missionType`
  - `startAt`
  - `endAt`
  - `rewardStars`
  - `rewardExp`
  - `totalRequired`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `Validation.General` | `id` rỗng, `startAt` / `endAt` không hợp lệ |
| `400` | `Mission.InvalidScope` | Thiếu target theo `scope` |
| `400` | `Mission.InvalidDateRange` | `endAt <= startAt` |
| `400` | `Mission.TeacherCannotTargetClass` | Teacher target class không thuộc lớp đang dạy |
| `400` | `Mission.TeacherCannotTargetStudent` | Teacher target student ngoài phạm vi |
| `400` | `Mission.TeacherCannotTargetSomeStudents` | Group có student ngoài phạm vi teacher |
| `404` | `Mission.NotFound` | Mission không tồn tại |
| `404` | `Mission.ClassNotFound` | `targetClassId` không tồn tại |
| `404` | `Mission.StudentNotFound` | `targetStudentId` không tồn tại |
| `404` | `Mission.SomeStudentsNotFound` | Một phần `targetGroup` không tồn tại |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Update không tự regenerate lại `MissionProgress` cho target mới.
- Teacher hiện không bị check “có phải người tạo mission hay không”.
- Teacher chỉ bị chặn theo target mới trong request; đây là khác biệt đáng lưu ý so với nghiệp vụ chặt hơn.

### 5.5. Xóa mission

- Endpoint: `DELETE /api/missions/{id}`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `delete`
- Mô tả: xóa mission khỏi DB.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Mission id |

Response success:

- `200 OK`
- `data = null`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Mission.NotFound` | Mission không tồn tại |
| `409` | `Mission.MissionInUse` | Mission đã có `MissionProgress` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Đây là xóa cứng, không phải soft delete.
- Vì mission create thường auto sinh `MissionProgress`, phần lớn mission có target student sẽ khó xóa được.

### 5.6. Xem progress của mission

- Endpoint: `GET /api/missions/{id}/progress`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student` / thực tế `all` / `view`
- Mô tả: xem progress bar và danh sách progress record của mission.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Mission id |

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không | Lọc 1 student cụ thể |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

Response success:

- `200 OK`
- `data.mission`
  - `id`
  - `title`
- `data.progresses`
  - `items[]`
    - `id`
    - `missionId`
    - `studentProfileId`
    - `studentName`
    - `status`
    - `progressValue`
    - `progressPercentage`
    - `completedAt`
    - `verifiedBy`
    - `verifiedByName`
  - `pageNumber`
  - `totalPages`
  - `totalCount`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Mission.NotFound` | Không tìm thấy mission |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- `progressPercentage` ở endpoint này được tính như thể `progressValue` đã là phần trăm `0..100` và bị clamp về `[0,100]`.
- Công thức này khác với endpoint `GET /api/missions/me/progress`.
- Handler không lọc theo user/linked child, nên Parent/Student có thể xem progress của mission khác nếu biết `id`.

### 5.7. Xem mission của “tôi”

- Endpoint: `GET /api/missions/me/progress`
- Role / scope / action: context có `StudentId` trong token / `selected student in token` / `view`
- Mô tả: lấy danh sách mission progress của student hiện tại trong token.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `missions`
    - `items[]`
      - `id`
      - `missionId`
      - `title`
      - `description`
      - `missionType`
      - `status`
      - `progressValue`
      - `totalRequired`
      - `progressPercentage`
      - `rewardStars`
      - `rewardExp`
      - `startAt`
      - `endAt`
      - `completedAt`
    - `pageNumber`
    - `totalPages`
    - `totalCount`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Xp.ProfileNotFound` | Token không có `StudentId` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role nếu token không qua authorize class-level |

Validation và lưu ý:

- Route tên là `me` nhưng controller không ép role `Student`; chỉ cần token có `StudentId` là gọi được.
- `progressPercentage` tại endpoint này tính theo:
  - nếu có `totalRequired > 0`: `progressValue * 100 / totalRequired`
  - nếu không có `totalRequired`: `progressValue` có giá trị thì `100`, ngược lại `0`

## 6. API Gamification: Stars và XP

### 6.1. Cộng stars thủ công

- Endpoint: `POST /api/gamification/stars/add`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all student profiles` / `edit`
- Mô tả: cộng stars thủ công cho student và tạo `StarTransaction`.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile cần cộng stars |
| `amount` | `int` | Có | Hiện không có validator bắt buộc > 0 |
| `reason` | `string?` | Không | Lý do điều chỉnh |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `amount`
  - `newBalance`
  - `transactionId`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Star.ProfileNotFound` | Student profile không tồn tại hoặc không phải `Student` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Không có validator chặn `amount <= 0`.
- Nếu truyền `amount` âm, endpoint “add” vẫn chạy và thực tế có thể làm giảm balance.
- `SourceType` được lưu là `Manual`.
- Hiện chưa có guard chặn teacher cộng stars cho student ngoài lớp mình dạy.

### 6.2. Trừ stars thủ công

- Endpoint: `POST /api/gamification/stars/deduct`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all student profiles` / `edit`
- Mô tả: trừ stars thủ công cho student và tạo `StarTransaction`.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile cần trừ stars |
| `amount` | `int` | Có | Hiện không có validator bắt buộc > 0 |
| `reason` | `string?` | Không | Lý do điều chỉnh |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `amount`
  - `newBalance`
  - `transactionId`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `Star.InsufficientBalance` | Balance hiện tại nhỏ hơn `amount` |
| `404` | `Star.ProfileNotFound` | Student profile không tồn tại hoặc không phải `Student` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Không có validator chặn `amount <= 0`.
- Nếu truyền `amount` âm, endpoint “deduct” có thể làm tăng balance do công thức hiện tại.
- `SourceType` được lưu là `Adjustment`.

### 6.3. Cộng XP thủ công

- Endpoint: `POST /api/gamification/xp/add`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all student profiles` / `edit`
- Mô tả: cộng XP thủ công và cập nhật `StudentLevel`.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile |
| `amount` | `int` | Có | Hiện không có validator bắt buộc > 0 |
| `reason` | `string?` | Không | Có trong request nhưng handler không lưu transaction riêng cho XP |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `amount`
  - `newXp`
  - `newLevel`
  - `levelUp`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Xp.ProfileNotFound` | Student profile không tồn tại hoặc không phải `Student` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Nếu chưa có `StudentLevel`, handler sẽ tạo mới.
- Không có validator chặn `amount <= 0`.
- Nếu truyền `amount` âm, endpoint “add” có thể làm giảm XP.

### 6.4. Trừ XP thủ công

- Endpoint: `POST /api/gamification/xp/deduct`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all student profiles` / `edit`
- Mô tả: trừ XP thủ công và cập nhật `StudentLevel`.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile |
| `amount` | `int` | Có | Hiện không có validator bắt buộc > 0 |
| `reason` | `string?` | Không | Lý do |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `amount`
  - `newXp`
  - `newLevel`
  - `levelDown`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Xp.ProfileNotFound` | Student profile không tồn tại hoặc không phải `Student` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- XP không bao giờ âm, handler dùng `Math.Max(0, oldXp - amount)`.
- Không có validator chặn `amount <= 0`; amount âm có thể làm tăng XP.

### 6.5. Lịch sử star transactions

- Endpoint: `GET /api/gamification/stars/transactions`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all` / `view`
- Mô tả: lấy lịch sử transaction stars của một student.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile |
| `page` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `20` |

Response success:

- `200 OK`
- `data`:
  - `transactions[]`
    - `id`
    - `studentProfileId`
    - `amount`
    - `reason`
    - `sourceType`
    - `sourceId`
    - `balanceAfter`
    - `createdBy`
    - `createdByName`
    - `createdAt`
  - `totalCount`
  - `page`
  - `pageSize`
  - `totalPages`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Handler không kiểm tra student profile có tồn tại hay không.
- Nếu không có transaction, API trả list rỗng.

### 6.6. Star balance hiện tại của một student

- Endpoint: `GET /api/gamification/stars/balance`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all` / `view`
- Mô tả: lấy star balance hiện tại dựa trên transaction mới nhất.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `balance`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Nếu student chưa có transaction nào, `balance = 0`.
- Handler không check student profile có tồn tại hay không.

### 6.7. Level và XP của một student

- Endpoint: `GET /api/gamification/level`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all` / `view`
- Mô tả: lấy level và XP hiện tại của một student.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `level`
  - `xp`
  - `xpRequiredForNextLevel`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Nếu chưa có bản ghi `StudentLevel`, handler suy ra level từ `0 XP`.
- Handler không check student profile có tồn tại hay không.

### 6.8. Star balance của “tôi”

- Endpoint: `GET /api/gamification/stars/balance/me`
- Role / scope / action: context có `StudentId` trong token / `selected student in token` / `view`
- Mô tả: lấy star balance của student hiện tại trong token.

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `balance`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Star.ProfileNotFound` | Token không có `StudentId` |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- Controller không ép role `Student`.
- Nếu có `StudentId` nhưng chưa có transaction, `balance = 0`.

### 6.9. Level và XP của “tôi”

- Endpoint: `GET /api/gamification/level/me`
- Role / scope / action: context có `StudentId` trong token / `selected student in token` / `view`
- Mô tả: lấy level, XP của student hiện tại trong token.

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `level`
  - `xp`
  - `xpRequiredForNextLevel`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Xp.ProfileNotFound` | Token không có `StudentId` |
| `401` | Unauthorized | Chưa login |

## 7. API Gamification: Attendance Streak

### 7.1. Xem attendance streak của một student

- Endpoint: `GET /api/gamification/attendance-streak`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all` / `view`
- Mô tả: xem current streak, max streak và lịch sử streak gần đây.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Student profile |

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `currentStreak`
  - `maxStreak`
  - `lastAttendanceDate`
  - `recentStreaks[]`
    - `id`
    - `attendanceDate`
    - `currentStreak`
    - `rewardStars`
    - `rewardExp`
    - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Nếu chưa có streak record nào, API trả `currentStreak = 0`, `maxStreak = 0`, `recentStreaks = []`.
- Handler không check profile có tồn tại hay không.

### 7.2. Check-in attendance streak

- Endpoint: `POST /api/gamification/attendance-streak/check-in`
- Role / scope / action: context có `StudentId` trong token / `selected student in token` / `create`
- Mô tả: student tự check-in hằng ngày, nhận stars + XP theo settings và đồng thời cập nhật progress cho mission `NoUnexcusedAbsence` đang active.

Request body: không có.

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `attendanceDate`
  - `currentStreak`
  - `maxStreak`
  - `rewardStars`
  - `rewardExp`
  - `isNewStreak`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Star.ProfileNotFound` | Token không có `StudentId` hoặc profile không tồn tại / không phải Student |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- Nếu đã check-in trong ngày, API không tạo record mới mà trả lại record cũ với `isNewStreak = false`.
- Nếu hôm qua không có streak, hệ thống reset streak về `1`.
- Check-in sẽ tạo `AttendanceStreak`, cộng stars, cộng XP, cập nhật mission `NoUnexcusedAbsence`, và auto thưởng mission nếu hoàn thành.
- Controller không ép role `Student`; chỉ cần token có `StudentId`.

### 7.3. Xem attendance streak của “tôi”

- Endpoint: `GET /api/gamification/attendance-streak/me`
- Role / scope / action: context có `StudentId` trong token / `selected student in token` / `view`
- Mô tả: xem attendance streak của student hiện tại trong token.

Response success:

- `200 OK`
- `data`:
  - `studentProfileId`
  - `currentStreak`
  - `maxStreak`
  - `lastAttendanceDate`
  - `recentStreaks[]`
    - `id`
    - `attendanceDate`
    - `currentStreak`
    - `rewardStars`
    - `rewardExp`
    - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `Star.ProfileNotFound` | Token không có `StudentId` |
| `401` | Unauthorized | Chưa login |

## 8. API Gamification: Reward Store

### 8.1. Tạo reward store item

- Endpoint: `POST /api/gamification/reward-store/items`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `create`
- Mô tả: tạo item mới trong cửa hàng đổi quà.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `title` | `string` | Có | Request DTO là non-nullable |
| `description` | `string?` | Không | Mô tả |
| `imageUrl` | `string?` | Không | Ảnh |
| `costStars` | `int` | Có | Phải > 0 |
| `quantity` | `int` | Có | Phải >= 0 |
| `isActive` | `bool` | Không | Mặc định `true` |

Response success:

- `201 Created`
- `data`:
  - `id`
  - `title`
  - `description`
  - `imageUrl`
  - `costStars`
  - `quantity`
  - `isActive`
  - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `RewardStore.InvalidCostStars` | `costStars <= 0` |
| `400` | `RewardStore.InvalidQuantity` | `quantity < 0` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Handler trim `title`, `description`, `imageUrl`.
- Chưa có validator riêng cho `title` rỗng sau khi trim.

### 8.2. Danh sách reward store items cho admin/management

- Endpoint: `GET /api/gamification/reward-store/items`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `view`
- Mô tả: lấy danh sách item chưa bị xóa mềm.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `isActive` | `bool?` | Không | Lọc active/inactive |
| `page` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `20` |

Response success:

- `200 OK`
- `data.items`
  - `items[]`
    - `id`
    - `title`
    - `description`
    - `imageUrl`
    - `costStars`
    - `quantity`
    - `isActive`
    - `createdAt`
  - `pageNumber`
  - `totalPages`
  - `totalCount`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

### 8.3. Chi tiết reward store item

- Endpoint: `GET /api/gamification/reward-store/items/{id}`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `view`
- Mô tả: lấy chi tiết một reward store item.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Item id |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `title`
  - `description`
  - `imageUrl`
  - `costStars`
  - `quantity`
  - `isActive`
  - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `RewardStore.NotFound` | Item không tồn tại hoặc đã soft-delete |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

### 8.4. Danh sách reward store item đang active

- Endpoint: `GET /api/gamification/reward-store/items/active`
- Role / scope / action: authenticated user / `all active non-deleted` / `view`
- Mô tả: lấy danh sách item đang active để student/parent đổi quà.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `page` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `20` |

Response success:

- `200 OK`
- `data.items`
  - `items[]`
    - `id`
    - `title`
    - `description`
    - `imageUrl`
    - `costStars`
    - `quantity`
    - `isActive`
    - `createdAt`
  - `pageNumber`
  - `totalPages`
  - `totalCount`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- Do controller có `[Authorize]` ở class level và method không có `Roles`, endpoint này hiện mở cho mọi authenticated role, không chỉ Student/Parent.

### 8.5. Cập nhật reward store item

- Endpoint: `PUT /api/gamification/reward-store/items/{id}`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `edit`
- Mô tả: cập nhật thông tin item.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Item id |

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `title` | `string?` | Không | Nếu truyền chuỗi rỗng thì giữ title cũ |
| `description` | `string?` | Không | Nếu truyền chuỗi rỗng sẽ set `null` |
| `imageUrl` | `string?` | Không | Nếu truyền chuỗi rỗng sẽ set `null` |
| `costStars` | `int?` | Không | Nếu có phải > 0 |
| `quantity` | `int?` | Không | Nếu có phải >= 0 |
| `isActive` | `bool?` | Không | Bật/tắt item |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `title`
  - `description`
  - `imageUrl`
  - `costStars`
  - `quantity`
  - `isActive`
  - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `RewardStore.InvalidCostStars` | `costStars <= 0` |
| `400` | `RewardStore.InvalidQuantity` | `quantity < 0` |
| `404` | `RewardStore.NotFound` | Item không tồn tại hoặc đã soft-delete |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

### 8.6. Xóa reward store item

- Endpoint: `DELETE /api/gamification/reward-store/items/{id}`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `delete`
- Mô tả: soft delete reward store item.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Item id |

Response success:

- `200 OK`
- `data`:
  - `id`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `RewardStore.NotFound` | Item không tồn tại hoặc đã soft-delete |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

### 8.7. Toggle trạng thái reward store item

- Endpoint: `PATCH /api/gamification/reward-store/items/{id}/toggle-status`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `edit`
- Mô tả: đổi trạng thái `IsActive` của item.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Item id |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `isActive`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `RewardStore.NotFound` | Item không tồn tại hoặc đã soft-delete |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

## 9. API Gamification: Reward Redemption

### 9.1. Request đổi quà

- Endpoint: `POST /api/gamification/reward-redemptions`
- Role / scope / action: context có `StudentId` trong token / `selected student in token` / `create`
- Mô tả: student gửi yêu cầu đổi quà, hệ thống trừ stars ngay tại thời điểm request và trừ tồn kho item.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `itemId` | `Guid` | Có | Reward store item |
| `quantity` | `int` | Không | Mặc định `1`, phải > 0 theo nghiệp vụ nhưng handler map lỗi kiểu khác |

Response success:

- `201 Created`
- `data`:
  - `id`
  - `itemId`
  - `itemName`
  - `quantity`
  - `studentProfileId`
  - `status`
  - `starsDeducted`
  - `remainingStars`
  - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `RewardRedemption.StudentProfileNotFound` | Token không có `StudentId` hoặc profile student không tồn tại |
| `404` | `RewardRedemption.ItemNotFound` | Item không tồn tại hoặc đã soft-delete |
| `400` | `RewardRedemption.ItemNotActive` | Item đang inactive |
| `409` | `RewardRedemption.InsufficientQuantity` | `quantity <= 0` hoặc tồn kho không đủ |
| `409` | `RewardRedemption.InsufficientStars` | Student không đủ sao |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- Handler đang dùng `RewardRedemption.InsufficientQuantity` cho cả case `quantity <= 0`.
- Hệ thống trừ stars trước, sau đó mới tạo `RewardRedemption`.
- Item quantity bị giảm ngay khi request thành công.
- `status` ban đầu là `Requested`.
- Controller không ép role `Student`; nếu token khác nhưng có `StudentId`, endpoint vẫn có thể dùng.

### 9.2. Danh sách reward redemption

- Endpoint: `GET /api/gamification/reward-redemptions`
- Role / scope / action: `Admin`, `ManagementStaff`, `Teacher` / thực tế `all` / `view`
- Mô tả: lấy danh sách redemption có filter theo student, item và status.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | Không | Lọc theo student |
| `itemId` | `Guid?` | Không | Lọc theo item |
| `status` | `string?` | Không | Parse sang `RedemptionStatus`; invalid string sẽ bị bỏ qua |
| `page` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `20` |

Response success:

- `200 OK`
- `data.redemptions`
  - `items[]`
    - `id`
    - `itemId`
    - `itemName`
    - `quantity`
    - `starsDeducted`
    - `studentProfileId`
    - `studentName`
    - `branchName`
    - `status`
    - `cancelReason`
    - `handledBy`
    - `handledByName`
    - `handledAt`
    - `deliveredAt`
    - `receivedAt`
    - `createdAt`
  - `pageNumber`
  - `totalPages`
  - `totalCount`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Teacher hiện có thể xem toàn bộ redemption, không bị chặn theo lớp hoặc branch.
- `branchName` được suy ra từ active enrollment gần nhất của student.

### 9.3. Chi tiết reward redemption

- Endpoint: `GET /api/gamification/reward-redemptions/{id}`
- Role / scope / action: authenticated user / thực tế `all` / `view`
- Mô tả: lấy chi tiết một redemption theo `id`.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Redemption id |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `itemId`
  - `itemName`
  - `quantity`
  - `starsDeducted`
  - `studentProfileId`
  - `studentName`
  - `branchName`
  - `status`
  - `cancelReason`
  - `handledBy`
  - `handledByName`
  - `handledAt`
  - `deliveredAt`
  - `receivedAt`
  - `createdAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `RewardRedemption.NotFound` | Không tìm thấy redemption |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- Đây là một auth gap lớn: controller không giới hạn role và handler không check owner.
- Bất kỳ authenticated user nào biết `id` đều có thể lấy detail redemption.

### 9.4. Danh sách reward redemption của “tôi”

- Endpoint: `GET /api/gamification/reward-redemptions/me`
- Role / scope / action: context có `StudentId` trong token / `selected student in token` / `view`
- Mô tả: lấy danh sách redemption của student hiện tại trong token.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `status` | `string?` | Không | Parse sang `RedemptionStatus`; invalid string bị bỏ qua |
| `page` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `20` |

Response success:

- `200 OK`
- `data.redemptions`
  - `items[]`
    - `id`
    - `itemId`
    - `itemName`
    - `quantity`
    - `starsDeducted`
    - `studentProfileId`
    - `branchName`
    - `status`
    - `cancelReason`
    - `handledBy`
    - `handledByName`
    - `handledAt`
    - `deliveredAt`
    - `receivedAt`
    - `createdAt`
  - `pageNumber`
  - `totalPages`
  - `totalCount`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `404` | `RewardRedemption.NotFound` | Token không có `StudentId` trong handler hiện tại |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- Khi không có `StudentId`, handler trả `RewardRedemption.NotFound(Guid.Empty)` thay vì lỗi profile rõ nghĩa.

### 9.5. Duyệt reward redemption

- Endpoint: `PATCH /api/gamification/reward-redemptions/{id}/approve`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `approve`
- Mô tả: duyệt redemption từ `Requested` sang `Approved`.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Redemption id |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `status`
  - `handledBy`
  - `handledAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `RewardRedemption.InvalidStatusTransition` | Status hiện tại không phải `Requested` |
| `404` | `RewardRedemption.NotFound` | Redemption không tồn tại |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

### 9.6. Hủy / từ chối reward redemption

- Endpoint: `PATCH /api/gamification/reward-redemptions/{id}/cancel`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `cancel`
- Mô tả: hủy redemption, hoàn lại quantity item và refund stars cho student.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Redemption id |

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `reason` | `string?` | Không | Lý do hủy |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `status`
  - `cancelReason`
  - `handledBy`
  - `handledAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `RewardRedemption.InvalidStatusTransition` | Status hiện tại không phải `Requested` hoặc `Approved` |
| `404` | `RewardRedemption.NotFound` | Redemption không tồn tại |
| `404` | `RewardRedemption.ItemNotFound` | Không load được item của redemption |
| `404` | `Star.ProfileNotFound` | Student profile dùng để refund stars không tồn tại |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Handler refund stars bằng cách gọi lại `AddStarsCommand`.
- `status` sau khi hủy là `Cancelled`.

### 9.7. Đánh dấu đã giao quà

- Endpoint: `PATCH /api/gamification/reward-redemptions/{id}/mark-delivered`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `deliver`
- Mô tả: chuyển redemption từ `Approved` sang `Delivered`.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Redemption id |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `status`
  - `deliveredAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `RewardRedemption.InvalidStatusTransition` | Status hiện tại không phải `Approved` |
| `404` | `RewardRedemption.NotFound` | Redemption không tồn tại |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

### 9.8. Xác nhận đã nhận quà

- Endpoint: `PATCH /api/gamification/reward-redemptions/{id}/confirm-received`
- Role / scope / action: context có `StudentId` trong token / `own selected student in token` / `confirm`
- Mô tả: student xác nhận đã nhận quà từ `Delivered` sang `Received`.

Path params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `id` | `Guid` | Có | Redemption id |

Response success:

- `200 OK`
- `data`:
  - `id`
  - `status`
  - `receivedAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `RewardRedemption.InvalidStatusTransition` | Status hiện tại không phải `Delivered` |
| `404` | `RewardRedemption.StudentProfileNotFound` | Token không có `StudentId` |
| `404` | `RewardRedemption.NotFound` | Redemption không tồn tại hoặc không thuộc student hiện tại |
| `401` | Unauthorized | Chưa login |

Validation và lưu ý:

- Handler cố tình trả `NotFound` nếu redemption không thuộc student hiện tại, để không lộ sự tồn tại của record.

### 9.9. Batch deliver reward redemptions

- Endpoint: `PATCH /api/gamification/reward-redemptions/batch-deliver`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `deliver`
- Mô tả: đánh dấu hàng loạt redemption đang `Approved` thành `Delivered`, có thể lọc theo tháng/năm dựa trên `HandledAt`.

Query params:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `year` | `int?` | Không | Nếu có phải trong khoảng `2000..2100` |
| `month` | `int?` | Không | Nếu có phải trong khoảng `1..12` |

Response success:

- `200 OK`
- `data`:
  - `deliveredCount`
  - `deliveredRedemptionIds`
  - `deliveredAt`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `400` | `Month` | `month < 1` hoặc `month > 12` |
| `400` | `Year` | `year < 2000`, `year > 2100`, hoặc có `month` nhưng thiếu `year` |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Nếu không tìm thấy redemption phù hợp, API vẫn thành công với `deliveredCount = 0`.
- Handler chỉ lấy record đang `Approved`.

## 10. API Gamification: Settings

### 10.1. Xem gamification settings

- Endpoint: `GET /api/gamification/settings`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `view`
- Mô tả: lấy settings toàn cục cho reward check-in.

Response success:

- `200 OK`
- `data`:
  - `checkInRewardStars`
  - `checkInRewardExp`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Nếu chưa có bản ghi settings trong DB, API trả default:
  - `checkInRewardStars = 1`
  - `checkInRewardExp = 5`

### 10.2. Cập nhật gamification settings

- Endpoint: `PUT /api/gamification/settings`
- Role / scope / action: `Admin`, `ManagementStaff` / `all` / `edit`
- Mô tả: cập nhật settings toàn cục cho reward check-in.

Request body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `checkInRewardStars` | `int` | Có | Không có validator chặn số âm |
| `checkInRewardExp` | `int` | Có | Không có validator chặn số âm |

Response success:

- `200 OK`
- `data`:
  - `checkInRewardStars`
  - `checkInRewardExp`

Response lỗi:

| HTTP | Code / title | Khi nào |
| --- | --- | --- |
| `401` | Unauthorized | Chưa login |
| `403` | Forbidden | Không đủ role |

Validation và lưu ý:

- Nếu chưa có settings, handler sẽ tạo mới record `Id = 1`.
- Hiện không có validation cho giá trị âm hoặc quá lớn.

## 11. Định nghĩa status

### 11.1. MissionScope

| Status | Ý nghĩa |
| --- | --- |
| `Class` | Mission áp dụng cho toàn bộ học sinh active trong một lớp |
| `Student` | Mission áp dụng cho một student cụ thể |
| `Group` | Mission áp dụng cho một nhóm student cụ thể |

### 11.2. MissionType

| Status | Ý nghĩa |
| --- | --- |
| `HomeworkStreak` | Nhiệm vụ theo chuỗi hoàn thành bài tập |
| `ReadingStreak` | Nhiệm vụ theo chuỗi đọc |
| `NoUnexcusedAbsence` | Nhiệm vụ không nghỉ học không phép |
| `Custom` | Nhiệm vụ tùy chỉnh |

### 11.3. MissionProgressStatus

| Status | Ý nghĩa |
| --- | --- |
| `Assigned` | Đã giao mission cho student |
| `InProgress` | Student đang thực hiện |
| `Completed` | Student đã hoàn thành mission |
| `Expired` | Mission đã hết hạn |

Luồng chuyển trạng thái hiện thấy trong code:

| From | To | Khi nào |
| --- | --- | --- |
| `Assigned` | `InProgress` | Khi mission bắt đầu được tiến độ hóa, ví dụ check-in cho mission `NoUnexcusedAbsence` |
| `InProgress` | `Completed` | Khi `progressValue >= totalRequired` |

Lưu ý:

- Enum có `Expired`, nhưng 2 controller này không expose API chuyển trạng thái này.
- Không có API manual để teacher/admin đổi status mission progress trong 2 controller hiện tại.

### 11.4. RedemptionStatus

| Status | Ý nghĩa |
| --- | --- |
| `Requested` | Student vừa request đổi quà |
| `Approved` | Admin/Management đã duyệt |
| `Delivered` | Quà đã được giao |
| `Received` | Student đã xác nhận nhận quà |
| `Cancelled` | Yêu cầu bị hủy / từ chối |

Luồng chuyển trạng thái:

| From | To | Khi nào |
| --- | --- | --- |
| `Requested` | `Approved` | Gọi API approve |
| `Requested` | `Cancelled` | Gọi API cancel |
| `Approved` | `Cancelled` | Gọi API cancel |
| `Approved` | `Delivered` | Gọi API mark-delivered hoặc batch-deliver |
| `Delivered` | `Received` | Gọi API confirm-received |

### 11.5. Reward store item state

| Field | Ý nghĩa |
| --- | --- |
| `isActive = true` | Item đang mở cho đổi quà |
| `isActive = false` | Item tạm ngưng |
| `isDeleted = true` | Item đã soft-delete |

### 11.6. StarSourceType

| Status | Ý nghĩa |
| --- | --- |
| `Mission` | Stars phát sinh từ mission |
| `Manual` | Stars cộng tay |
| `Homework` | Stars phát sinh từ homework |
| `Test` | Stars phát sinh từ test |
| `Adjustment` | Điều chỉnh stars |

## 12. Ma trận phân quyền theo role

| API group | Admin | ManagementStaff | Teacher | Parent | Student | Authenticated có `StudentId` |
| --- | --- | --- | --- | --- | --- | --- |
| Mission create | Yes | Yes | Yes | No | No | No |
| Mission list/detail/progress | Yes | Yes | Yes | Yes | Yes | Nếu có role phù hợp controller |
| Mission me/progress | Yes về mặt authorize class, nhưng phụ thuộc `StudentId` | Yes về mặt authorize class, nhưng phụ thuộc `StudentId` | Yes về mặt authorize class, nhưng phụ thuộc `StudentId` | Có thể | Có thể | Yes |
| Manual add/deduct stars | Yes | Yes | Yes | No | No | No |
| Manual add/deduct xp | Yes | Yes | Yes | No | No | No |
| Star transactions / balance / level theo studentProfileId | Yes | Yes | Yes | No | No | No |
| My star balance / my level | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể | Có thể | Yes |
| Attendance streak theo studentProfileId | Yes | Yes | Yes | No | No | No |
| Check-in / my attendance streak | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể | Có thể | Yes |
| Reward store admin CRUD | Yes | Yes | No | No | No | No |
| Reward store active list | Yes | Yes | Yes | Yes | Yes | Yes |
| Request reward redemption | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể | Có thể | Yes |
| Reward redemption list | Yes | Yes | Yes | No | No | No |
| Reward redemption detail by id | Yes | Yes | Yes | Yes | Yes | Yes |
| Approve / cancel / deliver redemption | Yes | Yes | No | No | No | No |
| Confirm received | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể nếu token có `StudentId` | Có thể | Có thể | Yes |
| Gamification settings | Yes | Yes | No | No | No | No |

Lưu ý:

- Bảng trên phản ánh quyền thực tế từ controller + handler, không chỉ quyền nghiệp vụ mong muốn.
- Nhiều endpoint “self-service” hiện đang dựa trên `StudentId` claim thay vì khóa chặt theo role `Student`.

## 13. Validation rule và các trường hợp trả lỗi

### 13.1. Rule tổng hợp

| Rule | API | Lỗi trả về |
| --- | --- | --- |
| `title` mission không được rỗng | POST/PUT missions | `400 Validation.General` |
| `startAt` không được ở quá khứ | POST/PUT missions | `400 Validation.General` |
| `endAt` không được ở quá khứ và phải sau `startAt` | POST/PUT missions | `400 Validation.General` hoặc `400 Mission.InvalidDateRange` |
| `scope = Class` phải có `targetClassId` | POST/PUT missions | `400 Mission.InvalidScope` |
| `scope = Student` phải có `targetStudentId` | POST/PUT missions | `400 Mission.InvalidScope` |
| `scope = Group` phải có `targetGroup` | POST/PUT missions | `400 Mission.InvalidScope` |
| Teacher chỉ được target lớp / học sinh thuộc phạm vi dạy | POST/PUT missions | `400 Mission.TeacherCannotTargetClass`, `Mission.TeacherCannotTargetStudent`, `Mission.TeacherCannotTargetSomeStudents` |
| `costStars > 0` | POST/PUT reward store item | `400 RewardStore.InvalidCostStars` |
| `quantity >= 0` | POST/PUT reward store item | `400 RewardStore.InvalidQuantity` |
| `quantity request đổi quà > 0` theo nghiệp vụ | POST reward-redemptions | Hiện đang trả `409 RewardRedemption.InsufficientQuantity` |
| Item đổi quà phải active | POST reward-redemptions | `400 RewardRedemption.ItemNotActive` |
| Quantity item phải đủ | POST reward-redemptions | `409 RewardRedemption.InsufficientQuantity` |
| Student phải đủ stars | POST reward-redemptions | `409 RewardRedemption.InsufficientStars` |
| Redemption chỉ approve khi đang `Requested` | PATCH approve | `400 RewardRedemption.InvalidStatusTransition` |
| Redemption chỉ cancel khi đang `Requested` hoặc `Approved` | PATCH cancel | `400 RewardRedemption.InvalidStatusTransition` |
| Redemption chỉ mark-delivered khi đang `Approved` | PATCH mark-delivered | `400 RewardRedemption.InvalidStatusTransition` |
| Redemption chỉ confirm-received khi đang `Delivered` | PATCH confirm-received | `400 RewardRedemption.InvalidStatusTransition` |
| `month` phải nằm trong `1..12` | PATCH batch-deliver | `400 Month` |
| `year` phải nằm trong `2000..2100` | PATCH batch-deliver | `400 Year` |
| Nếu có `month` thì phải có `year` | PATCH batch-deliver | `400 Year` |

### 13.2. Những điểm chưa có validation chặt

- `AddStars`, `DeductStars`, `AddXp`, `DeductXp` hiện không có validator chặn `amount <= 0`.
- `GetMissions`, `GetMissionProgress`, `GetRewardStoreItems`, `GetRewardRedemptions`, `GetMyRewardRedemptions`, `GetStarTransactions` chưa có validator cho `page/pageSize`.
- `UpdateGamificationSettings` chưa chặn giá trị âm.
- `GetRewardRedemptionById` chưa kiểm tra owner / linked student.
- `GetMissions`, `GetMissionById`, `GetMissionProgress` chưa lọc theo owner / linked student / branch.

## 14. Known gaps và implementation notes

### 14.1. Auth / scope gaps

- `GET /api/missions`
  - Parent/Student hiện xem được toàn bộ mission theo filter, không bị giới hạn vào mission của child/current student.
- `GET /api/missions/{id}`
  - Parent/Student chỉ cần biết `id` là xem được detail.
- `GET /api/missions/{id}/progress`
  - Parent/Student hiện xem được progress của mọi mission nếu biết `id`.
- `PUT /api/missions/{id}`
  - Teacher không bị check owner của mission, chỉ check target mới.
- `POST /api/gamification/stars/add`, `stars/deduct`, `xp/add`, `xp/deduct`
  - Teacher chưa bị chặn theo lớp/học sinh mình phụ trách.
- `GET /api/gamification/reward-redemptions`
  - Teacher đang xem được toàn bộ redemption.
- `GET /api/gamification/reward-redemptions/{id}`
  - Bất kỳ authenticated user nào cũng có thể xem detail nếu biết `id`.
- Các endpoint self-service:
  - `/api/gamification/stars/balance/me`
  - `/api/gamification/level/me`
  - `/api/gamification/attendance-streak/check-in`
  - `/api/gamification/attendance-streak/me`
  - `POST /api/gamification/reward-redemptions`
  - `/api/gamification/reward-redemptions/me`
  - `/api/gamification/reward-redemptions/{id}/confirm-received`
  đều phụ thuộc vào `StudentId` trong token hơn là role `Student`.

### 14.2. Response / contract gaps

- `GetMissions` và `GetMissionById` hiện không trả `totalRequired`, dù field này tồn tại ở entity và create/update response.
- `GetMissionProgress` chỉ trả `mission.id`, `mission.title`, không trả reward hay totalRequired.
- `GetMissionProgress.progressPercentage` và `GetMyMissions.progressPercentage` đang dùng 2 công thức khác nhau.
- `DeleteMission` trả `data = null`, không có payload chi tiết.

### 14.3. Validation gaps

- Manual stars/xp APIs thiếu validation cho số âm.
- `RequestRewardRedemption` dùng error `InsufficientQuantity` cho cả case `quantity <= 0`, khiến semantic lỗi chưa thật chuẩn.
- `UpdateGamificationSettings` cho phép lưu giá trị âm.

### 14.4. Hành vi nghiệp vụ đáng lưu ý

- Tạo mission sẽ auto tạo `MissionProgress` cho toàn bộ target students active.
- Vì vậy mission vừa tạo thường sẽ không xóa được nữa bằng `DELETE`, do `Mission.MissionInUse`.
- Check-in attendance streak không chỉ tạo streak record mà còn cộng stars, cộng XP, cập nhật mission `NoUnexcusedAbsence`, và auto thưởng mission nếu hoàn thành.
- Request reward redemption trừ stars và giảm tồn kho ngay từ bước request, không chờ approve.
- Cancel reward redemption hoàn stars và hoàn quantity item.

## 15. Nguồn code chính

- `Kidzgo.API/Controllers/MissionController.cs`
- `Kidzgo.API/Controllers/GamificationController.cs`
- `Kidzgo.API/Requests/CreateMissionRequest.cs`
- `Kidzgo.API/Requests/UpdateMissionRequest.cs`
- `Kidzgo.API/Requests/AddStarsRequest.cs`
- `Kidzgo.API/Requests/DeductStarsRequest.cs`
- `Kidzgo.API/Requests/AddXpRequest.cs`
- `Kidzgo.API/Requests/DeductXpRequest.cs`
- `Kidzgo.API/Requests/CreateRewardStoreItemRequest.cs`
- `Kidzgo.API/Requests/UpdateRewardStoreItemRequest.cs`
- `Kidzgo.API/Requests/RequestRewardRedemptionRequest.cs`
- `Kidzgo.API/Requests/CancelRewardRedemptionRequest.cs`
- `Kidzgo.API/Requests/UpdateGamificationSettingsRequest.cs`
- `Kidzgo.Application/Missions/*`
- `Kidzgo.Application/Gamification/*`
- `Kidzgo.Domain/Gamification/*`
