# Tài liệu Gamification theo Role

## 1. Phạm vi

Tài liệu này mô tả **implementation hiện tại trong code** cho các chức năng liên quan Gamification:

- Mission
- Mission Progress
- Stars
- XP / Level
- Attendance Streak
- Reward Store
- Reward Redemption
- API gắn Homework với Mission

Nguồn chính:

- `Kidzgo.API/Controllers/MissionController.cs`
- `Kidzgo.API/Controllers/GamificationController.cs`
- `Kidzgo.API/Controllers/HomeworkController.cs`
- `Kidzgo.Application/Missions/*`
- `Kidzgo.Application/Gamification/*`
- `Kidzgo.Domain/Gamification/*`
- `Kidzgo.Infrastructure/BackgroundJobs/AutoConfirmRewardRedemptionJob.cs`

## 2. Business rule tổng quát

### 2.1. Access model

- Hệ thống check quyền bằng `User.Role` trong JWT.
- Nhiều API learner còn phụ thuộc `StudentId` trong token.
- `StudentId` được thêm sau khi gọi `POST /api/auth/profiles/select-student`.
- Vì vậy các API `me`, `check-in`, `request redemption`, `confirm received` thực tế chạy theo **student-context**.

### 2.2. Data scope hiện tại

- `department` scope chưa được implement riêng cho Gamification.
- Staff APIs hiện chủ yếu là `all`.
- Learner APIs dạng `me` là `own` theo `StudentId`.
- Một số API read chưa có ownership filter ở handler.

### 2.3. Mission

- `Scope = Class` bắt buộc có `TargetClassId`.
- `Scope = Group` bắt buộc có `TargetGroup`.
- `EndAt` phải sau `StartAt`.
- Xóa mission chỉ được khi chưa có `MissionProgress`.
- Mission không có status enum riêng.

### 2.4. Mission Progress

- Enum có sẵn: `Assigned`, `InProgress`, `Completed`, `Expired`.
- API hiện tại chỉ **đọc** progress.
- Không có public API hiện tại để tạo/cập nhật `MissionProgress`.

### 2.5. Stars

- Stars lưu dạng ledger trong `StarTransactions`.
- Balance lấy từ `BalanceAfter` của transaction mới nhất.
- Manual add dùng `SourceType = Manual`.
- Manual deduct dùng `SourceType = Adjustment`.

### 2.6. XP / Level

- XP lưu trong `StudentLevels`.
- Công thức level:
  - `Level = floor(XP / 100) + 1`
- `XpRequiredForNextLevel = 100 - (currentXp % 100)`; nếu chia hết thì trả `100`.

### 2.7. Attendance Streak

- Mỗi học sinh chỉ 1 check-in mỗi ngày theo `UTC date`.
- Nếu hôm qua có record thì streak +1.
- Nếu không có thì reset về `1`.
- Mỗi check-in mới:
  - `+1` star
  - `+5` XP

### 2.8. Reward Store

- `CostStars > 0`
- `Quantity >= 0`
- Delete là soft delete bằng `IsDeleted = true`

### 2.9. Reward Redemption

- Khi request:
  - validate item tồn tại / active / đủ tồn
  - validate đủ sao
  - trừ sao ngay
  - giảm tồn ngay
  - tạo redemption `Requested`
  - snapshot `ItemName`
- Khi cancel:
  - chỉ từ `Requested` hoặc `Approved`
  - hoàn tồn
  - refund sao
- Auto confirm:
  - `Delivered -> Received`
  - sau N ngày, fallback hiện tại `3`

## 3. Role và phạm vi dữ liệu

| Role / Context | Xem dữ liệu gì | Scope | Hành động |
|---|---|---|---|
| `Admin` | Toàn bộ gamification | `all` | full mission, star/xp, reward store, redemption, streak |
| `ManagementStaff` | Toàn bộ gamification | `all` | gần như Admin |
| `Teacher` | Mission, progress, star/xp, streak, redemption list | `all` | create/update mission, add/deduct star/xp, xem streak, xem redemption |
| `Parent` có `StudentId` | Dữ liệu của học sinh đang chọn | chủ yếu `own` | xem me APIs, request redemption, confirm received |
| `Student-context` có `StudentId` | Dữ liệu của chính học sinh | `own` | như Parent selected-student |
| `AccountantStaff` | Không có API gamification chuyên biệt | không rõ | không có nghiệp vụ gamification riêng |

### Ghi chú quan trọng

- `GET /api/missions`, `GET /api/missions/{id}`, `GET /api/missions/{id}/progress` hiện **không filter theo ownership ở handler**.
- `GET /api/gamification/reward-redemptions/{id}` hiện chỉ yêu cầu authenticated, không check ownership ở handler.
- Một số endpoint learner thực chất chỉ cần authenticated + có `StudentId`, không chặn thêm theo role ở action.

## 4. Permission matrix

| Chức năng | Admin | ManagementStaff | Teacher | Parent selected-student | Student-context |
|---|---|---|---|---|---|
| Xem mission list/detail/progress | Yes | Yes | Yes | Yes | phụ thuộc token/role thực tế |
| Tạo mission | Yes | Yes | Yes | No | No |
| Cập nhật mission | Yes | Yes | Yes | No | No |
| Xóa mission | Yes | Yes | No | No | No |
| Add/Deduct stars | Yes | Yes | Yes | No | No |
| Xem star transactions | Yes | Yes | Yes | No | No |
| Xem star balance theo student id | Yes | Yes | Yes | No | No |
| Xem star balance `/me` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes | Yes |
| Add/Deduct XP | Yes | Yes | Yes | No | No |
| Xem level theo student id | Yes | Yes | Yes | No | No |
| Xem level `/me` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes | Yes |
| Check-in streak | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes | Yes |
| Xem streak theo student id | Yes | Yes | Yes | No | No |
| Xem streak `/me` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes | Yes |
| Xem reward store active | Yes | Yes | Yes | Yes | Yes |
| CRUD reward store | Yes | Yes | No | No | No |
| Request redemption | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes | Yes |
| Xem redemption list | Yes | Yes | Yes | No | No |
| Xem redemption detail by id | Yes | Yes | Yes | Yes | Yes |
| Xem redemption `/me` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes | Yes |
| Approve/Cancel/Deliver redemption | Yes | Yes | No | No | No |
| Batch deliver | Yes | Yes | No | No | No |
| Confirm received | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes nếu có `StudentId` | Yes | Yes |
| Link homework to mission | Yes | Yes | Yes | No | No |

## 5. Status definition

### 5.1. `MissionProgressStatus`

| Status | Ý nghĩa |
|---|---|
| `Assigned` | đã gán |
| `InProgress` | đang thực hiện |
| `Completed` | đã hoàn thành |
| `Expired` | đã hết hạn |

Ghi chú:

- Enum có trong domain.
- Hiện chưa có public API để chuyển trạng thái progress.

### 5.2. `RedemptionStatus`

| Status | Ý nghĩa |
|---|---|
| `Requested` | learner request, sao đã trừ, tồn đã giữ |
| `Approved` | staff đã duyệt |
| `Delivered` | staff đã trao quà |
| `Received` | learner hoặc system xác nhận đã nhận |
| `Cancelled` | request bị hủy, hoàn tồn, refund sao |

### 5.3. Luồng chuyển trạng thái đang implement

| Từ | Sang | Trigger |
|---|---|---|
| `Requested` | `Approved` | approve API |
| `Requested` | `Cancelled` | cancel API |
| `Approved` | `Cancelled` | cancel API |
| `Approved` | `Delivered` | mark-delivered API |
| `Delivered` | `Received` | confirm-received API |
| `Delivered` | `Received` | auto-confirm Quartz job |

## 6. Response format

### 6.1. Success

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 6.2. Error

Hệ thống trả `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": []
}
```

### 6.3. Mapping status code

| HTTP code | Ý nghĩa |
|---|---|
| `400` | validation |
| `401` | thiếu token / token sai |
| `403` | sai role |
| `404` | không tìm thấy / thiếu `StudentId` / ownership bị che bằng not found |
| `409` | conflict dữ liệu |
| `500` | lỗi hệ thống |

## 7. Validation rule và lỗi tiêu biểu

### 7.1. Mission

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| `title` bắt buộc | validator | `400 Validation.General` |
| `startAt` không ở quá khứ | validator | `400 Validation.General` |
| `endAt` không ở quá khứ | validator | `400 Validation.General` |
| `endAt >= startAt` | validator | `400 Validation.General` |
| `endAt > startAt` | handler | `400 Mission.InvalidDateRange` |
| `scope=Class` cần `targetClassId` | handler | `400 Mission.InvalidScope` |
| `scope=Group` cần `targetGroup` | handler | `400 Mission.InvalidScope` |
| class đích phải tồn tại | handler | `404 Mission.ClassNotFound` |

### 7.2. Reward Store

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| `costStars > 0` | handler | `400 RewardStore.InvalidCostStars` |
| `quantity >= 0` | handler | `400 RewardStore.InvalidQuantity` |
| item phải tồn tại khi update/delete/toggle | handler | `404 RewardStore.NotFound` |

### 7.3. Reward Redemption

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| phải có `StudentId` trong token | handler | `404 RewardRedemption.StudentProfileNotFound` |
| student profile phải hợp lệ | handler | `404 RewardRedemption.StudentProfileNotFound` |
| item phải tồn tại | handler | `404 RewardRedemption.ItemNotFound` |
| item phải active | handler | `400 RewardRedemption.ItemNotActive` |
| `quantity > 0` | handler | `409 RewardRedemption.InsufficientQuantity` |
| tồn kho đủ | handler | `409 RewardRedemption.InsufficientQuantity` |
| sao đủ | handler | `409 RewardRedemption.InsufficientStars` |
| transition phải hợp lệ | handler | `400 RewardRedemption.InvalidStatusTransition` |

### 7.4. Stars / XP

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| student profile phải tồn tại | handler | `404 Star.ProfileNotFound` / `404 Xp.ProfileNotFound` |
| deduct stars không vượt balance | handler | `400 Star.InsufficientBalance` |
| XP không âm sau deduct | handler `Math.Max(0, ...)` | không trả lỗi, tự chặn về 0 |

Ghi chú:

- Hiện **không có validator riêng** cho `amount <= 0` ở add/deduct star/xp.

### 7.5. Attendance Streak

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| phải có `StudentId` trong token | handler | `404 Star.ProfileNotFound` |
| student profile phải tồn tại | handler | `404 Star.ProfileNotFound` |
| một ngày chỉ một record | handler + unique index | nếu đã check-in thì trả success với `isNewStreak = false` |

## 8. Danh sách API

## 8.1. Mission APIs

### `POST /api/missions`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Mục đích: tạo mission
- Body:

| Field | Type | Required |
|---|---|---|
| `title` | string | Yes |
| `description` | string | No |
| `scope` | enum(`Class`,`Student`,`Group`) | Yes |
| `targetClassId` | guid | Conditional |
| `targetGroup` | string | Conditional |
| `missionType` | enum(`HomeworkStreak`,`ReadingStreak`,`NoUnexcusedAbsence`,`Custom`) | Yes |
| `startAt` | datetime | No |
| `endAt` | datetime | No |
| `rewardStars` | int | No |
| `rewardExp` | int | No |

- Success data:
  - `id`, `title`, `description`, `scope`, `targetClassId`, `targetGroup`, `missionType`, `startAt`, `endAt`, `rewardStars`, `rewardExp`, `createdBy`, `createdAt`
- Error:
  - `400 Validation.General`
  - `400 Mission.InvalidScope`
  - `400 Mission.InvalidDateRange`
  - `404 Mission.ClassNotFound`

### `GET /api/missions`

- Role: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student` annotation
- Mục đích: lấy danh sách mission
- Query:

| Param | Type | Required |
|---|---|---|
| `scope` | enum | No |
| `targetClassId` | guid | No |
| `targetGroup` | string | No |
| `missionType` | enum | No |
| `searchTerm` | string | No |
| `pageNumber` | int, default `1` | No |
| `pageSize` | int, default `10` | No |

- Success data:
  - `missions.items[]` gồm `id`, `title`, `description`, `scope`, `targetClassId`, `targetClassCode`, `targetClassTitle`, `targetGroup`, `missionType`, `startAt`, `endAt`, `rewardStars`, `rewardExp`, `createdBy`, `createdByName`, `createdAt`
  - `missions.pageNumber`, `missions.totalPages`, `missions.totalCount`
- Error:
  - `401`, `403`, `500`

### `GET /api/missions/{id}`

- Role: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student` annotation
- Mục đích: xem chi tiết mission
- Path: `id: guid`
- Success data:
  - cùng field với một item mission
- Error:
  - `404 Mission.NotFound`

### `PUT /api/missions/{id}`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Mục đích: cập nhật mission
- Path: `id: guid`
- Body: giống create mission
- Success data:
  - `id`, `title`, `description`, `scope`, `targetClassId`, `targetGroup`, `missionType`, `startAt`, `endAt`, `rewardStars`, `rewardExp`
- Error:
  - `400 Validation.General`
  - `400 Mission.InvalidScope`
  - `400 Mission.InvalidDateRange`
  - `404 Mission.NotFound`
  - `404 Mission.ClassNotFound`

### `DELETE /api/missions/{id}`

- Role: `Admin`, `ManagementStaff`
- Mục đích: xóa mission nếu chưa có progress
- Path: `id: guid`
- Success data: `null`
- Error:
  - `404 Mission.NotFound`
  - `409 Mission.MissionInUse`

### `GET /api/missions/{id}/progress`

- Role: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student` annotation
- Mục đích: xem progress của mission
- Path: `id: guid`
- Query:

| Param | Type | Required |
|---|---|---|
| `studentProfileId` | guid | No |
| `pageNumber` | int, default `1` | No |
| `pageSize` | int, default `10` | No |

- Success data:
  - `mission.id`, `mission.title`
  - `progresses.items[]` gồm `id`, `missionId`, `studentProfileId`, `studentName`, `status`, `progressValue`, `progressPercentage`, `completedAt`, `verifiedBy`, `verifiedByName`
  - `progresses.pageNumber`, `progresses.totalPages`, `progresses.totalCount`
- Error:
  - `404 Mission.NotFound`

## 8.2. Stars, XP, Level, Attendance Streak

### `POST /api/gamification/stars/add`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Body: `studentProfileId: guid`, `amount: int`, `reason?: string`
- Success data: `studentProfileId`, `amount`, `newBalance`, `transactionId`
- Error:
  - `404 Star.ProfileNotFound`

### `POST /api/gamification/stars/deduct`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Body: `studentProfileId: guid`, `amount: int`, `reason?: string`
- Success data: `studentProfileId`, `amount`, `newBalance`, `transactionId`
- Error:
  - `404 Star.ProfileNotFound`
  - `400 Star.InsufficientBalance`

### `GET /api/gamification/stars/transactions`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Query: `studentProfileId`, `page=1`, `pageSize=20`
- Success data:
  - `transactions[]`: `id`, `studentProfileId`, `amount`, `reason`, `sourceType`, `sourceId`, `balanceAfter`, `createdBy`, `createdByName`, `createdAt`
  - `totalCount`, `page`, `pageSize`, `totalPages`

### `GET /api/gamification/stars/balance`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Query: `studentProfileId`
- Success data: `studentProfileId`, `balance`

### `GET /api/gamification/stars/balance/me`

- Auth: authenticated + cần `StudentId`
- Success data: `studentProfileId`, `balance`
- Error:
  - `404 Star.ProfileNotFound`

### `POST /api/gamification/xp/add`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Body: `studentProfileId: guid`, `amount: int`, `reason?: string`
- Success data: `studentProfileId`, `amount`, `newXp`, `newLevel`, `levelUp`
- Error:
  - `404 Xp.ProfileNotFound`

### `POST /api/gamification/xp/deduct`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Body: `studentProfileId: guid`, `amount: int`, `reason?: string`
- Success data: `studentProfileId`, `amount`, `newXp`, `newLevel`, `levelDown`
- Error:
  - `404 Xp.ProfileNotFound`

### `GET /api/gamification/level`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Query: `studentProfileId`
- Success data: `studentProfileId`, `level`, `xp`, `xpRequiredForNextLevel`

### `GET /api/gamification/level/me`

- Auth: authenticated + cần `StudentId`
- Success data: `studentProfileId`, `level`, `xp`, `xpRequiredForNextLevel`
- Error:
  - `404 Xp.ProfileNotFound`

### `GET /api/gamification/attendance-streak`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Query: `studentProfileId`
- Success data:
  - `studentProfileId`, `currentStreak`, `maxStreak`, `lastAttendanceDate`
  - `recentStreaks[]`: `id`, `attendanceDate`, `currentStreak`, `rewardStars`, `rewardExp`, `createdAt`

### `POST /api/gamification/attendance-streak/check-in`

- Auth: authenticated + cần `StudentId`
- Body: none
- Success data:
  - `studentProfileId`, `attendanceDate`, `currentStreak`, `maxStreak`, `rewardStars`, `rewardExp`, `isNewStreak`
- Error:
  - `404 Star.ProfileNotFound`

### `GET /api/gamification/attendance-streak/me`

- Auth: authenticated + cần `StudentId`
- Success data:
  - `studentProfileId`, `currentStreak`, `maxStreak`, `lastAttendanceDate`, `recentStreaks[]`
- Error:
  - `404 Star.ProfileNotFound`

## 8.3. Reward Store APIs

### `POST /api/gamification/reward-store/items`

- Role: `Admin`, `ManagementStaff`
- Body:
  - `title: string`
  - `description?: string`
  - `imageUrl?: string`
  - `costStars: int`
  - `quantity: int`
  - `isActive: bool`
- Success data:
  - `id`, `title`, `description`, `imageUrl`, `costStars`, `quantity`, `isActive`, `createdAt`
- Error:
  - `400 RewardStore.InvalidCostStars`
  - `400 RewardStore.InvalidQuantity`

### `GET /api/gamification/reward-store/items`

- Role: `Admin`, `ManagementStaff`
- Query: `isActive?`, `page=1`, `pageSize=20`
- Success data:
  - `items.items[]`: `id`, `title`, `description`, `imageUrl`, `costStars`, `quantity`, `isActive`, `createdAt`
  - `items.pageNumber`, `items.totalPages`, `items.totalCount`

### `GET /api/gamification/reward-store/items/{id}`

- Role: `Admin`, `ManagementStaff`
- Success data:
  - `id`, `title`, `description`, `imageUrl`, `costStars`, `quantity`, `isActive`, `createdAt`
- Error:
  - `404 RewardStore.NotFound`

### `GET /api/gamification/reward-store/items/active`

- Auth: mọi user đã đăng nhập
- Query: `page=1`, `pageSize=20`
- Success data: giống list reward store, chỉ lấy item active

### `PUT /api/gamification/reward-store/items/{id}`

- Role: `Admin`, `ManagementStaff`
- Body:
  - `title?: string`
  - `description?: string`
  - `imageUrl?: string`
  - `costStars?: int`
  - `quantity?: int`
  - `isActive?: bool`
- Success data:
  - `id`, `title`, `description`, `imageUrl`, `costStars`, `quantity`, `isActive`, `createdAt`
- Error:
  - `404 RewardStore.NotFound`
  - `400 RewardStore.InvalidCostStars`
  - `400 RewardStore.InvalidQuantity`

### `DELETE /api/gamification/reward-store/items/{id}`

- Role: `Admin`, `ManagementStaff`
- Success data: `id`
- Error:
  - `404 RewardStore.NotFound`

### `PATCH /api/gamification/reward-store/items/{id}/toggle-status`

- Role: `Admin`, `ManagementStaff`
- Success data: `id`, `isActive`
- Error:
  - `404 RewardStore.NotFound`

## 8.4. Reward Redemption APIs

### `POST /api/gamification/reward-redemptions`

- Auth: authenticated + cần `StudentId`
- Body: `itemId: guid`, `quantity: int = 1`
- Success data:
  - `id`, `itemId`, `itemName`, `quantity`, `studentProfileId`, `status`, `starsDeducted`, `remainingStars`, `createdAt`
- Error:
  - `404 RewardRedemption.StudentProfileNotFound`
  - `404 RewardRedemption.ItemNotFound`
  - `400 RewardRedemption.ItemNotActive`
  - `409 RewardRedemption.InsufficientQuantity`
  - `409 RewardRedemption.InsufficientStars`

### `GET /api/gamification/reward-redemptions`

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Query: `studentProfileId?`, `itemId?`, `status?`, `page=1`, `pageSize=20`
- Success data:
  - `redemptions.items[]`: `id`, `itemId`, `itemName`, `quantity`, `studentProfileId`, `studentName`, `branchName`, `status`, `handledBy`, `handledByName`, `handledAt`, `deliveredAt`, `receivedAt`, `createdAt`
  - `redemptions.pageNumber`, `redemptions.totalPages`, `redemptions.totalCount`

### `GET /api/gamification/reward-redemptions/{id}`

- Auth: mọi user đã đăng nhập
- Success data:
  - `id`, `itemId`, `itemName`, `quantity`, `studentProfileId`, `studentName`, `branchName`, `status`, `handledBy`, `handledByName`, `handledAt`, `deliveredAt`, `receivedAt`, `createdAt`
- Error:
  - `404 RewardRedemption.NotFound`

### `GET /api/gamification/reward-redemptions/me`

- Auth: authenticated + cần `StudentId`
- Query: `status?`, `page=1`, `pageSize=20`
- Success data:
  - `redemptions.items[]`: `id`, `itemId`, `itemName`, `quantity`, `studentProfileId`, `branchName`, `status`, `handledBy`, `handledByName`, `handledAt`, `deliveredAt`, `receivedAt`, `createdAt`
  - `redemptions.pageNumber`, `redemptions.totalPages`, `redemptions.totalCount`
- Error:
  - `404 RewardRedemption.NotFound` khi thiếu `StudentId`

### `PATCH /api/gamification/reward-redemptions/{id}/approve`

- Role: `Admin`, `ManagementStaff`
- Success data: `id`, `status`, `handledBy`, `handledAt`
- Error:
  - `404 RewardRedemption.NotFound`
  - `400 RewardRedemption.InvalidStatusTransition`

### `PATCH /api/gamification/reward-redemptions/{id}/cancel`

- Role: `Admin`, `ManagementStaff`
- Body: `reason?: string`
- Success data: `id`, `status`, `handledBy`, `handledAt`
- Side effects:
  - restore quantity
  - refund stars
- Error:
  - `404 RewardRedemption.NotFound`
  - `404 RewardRedemption.ItemNotFound`
  - `400 RewardRedemption.InvalidStatusTransition`

### `PATCH /api/gamification/reward-redemptions/{id}/mark-delivered`

- Role: `Admin`, `ManagementStaff`
- Success data: `id`, `status`, `deliveredAt`
- Error:
  - `404 RewardRedemption.NotFound`
  - `400 RewardRedemption.InvalidStatusTransition`

### `PATCH /api/gamification/reward-redemptions/{id}/confirm-received`

- Auth: authenticated + cần `StudentId`
- Success data: `id`, `status`, `receivedAt`
- Error:
  - `404 RewardRedemption.StudentProfileNotFound`
  - `404 RewardRedemption.NotFound`
  - `400 RewardRedemption.InvalidStatusTransition`

### `PATCH /api/gamification/reward-redemptions/batch-deliver`

- Role: `Admin`, `ManagementStaff`
- Query: `year?`, `month?`
- Validation:
  - `month` từ `1..12`
  - `year` từ `2000..2100`
  - có `month` thì bắt buộc có `year`
- Success data:
  - `deliveredCount`, `deliveredRedemptionIds[]`, `deliveredAt`
- Error:
  - `400 Month`
  - `400 Year`

## 8.5. API ngoài module nhưng liên quan Gamification

### `POST /api/homework/{id}/link-mission`

- Role: `Teacher`, `ManagementStaff`, `Admin`
- Mục đích: gắn homework hiện có vào mission
- Path: `id: guid`
- Body: `missionId: guid`
- Success data:
  - `homeworkId`, `missionId`, `missionTitle`
- Error:
  - `404 Homework.NotFound`
  - `404 Homework.MissionNotFound`

### MissionId trong create/update homework

Các flow sau có `MissionId` optional và sẽ validate mission tồn tại:

- `POST /api/homework`
- `POST /api/homework/multiple-choice`
- `POST /api/homework/multiple-choice/from-bank`
- `PUT /api/homework/{id}`

Nếu mission không tồn tại:

- `404 Homework.MissionNotFound`

## 9. Các trường hợp trả lỗi quan trọng

- tạo mission class nhưng thiếu `targetClassId`
- tạo mission group nhưng thiếu `targetGroup`
- ngày mission không hợp lệ
- xóa mission đã có progress
- deduct stars vượt balance
- token không có `StudentId` cho learner APIs
- item reward inactive / không tồn tại / không đủ tồn
- learner không đủ sao để redeem
- confirm/cancel/approve/deliver sai status transition
- learner confirm redemption không thuộc về mình sẽ nhận `404`

## 10. Kết luận về scope hiện tại

- `own` hiện rõ nhất ở nhóm API `me`, `check-in`, `request redemption`, `confirm received`
- `department` hiện chưa có cho Gamification
- `all` là scope thực tế của đa số API staff
- Có một số endpoint đang mở rộng hơn rule nghiệp vụ thường thấy:
  - mission read APIs không user-scoped ở handler
  - reward redemption detail không ownership-scoped ở handler
