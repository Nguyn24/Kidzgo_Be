# Tài Liệu API FE - Gamification - 2026-04-15

Tài liệu này mô tả các API trong `GamificationController.cs`.

Phạm vi gồm Stars/XP, attendance streak, mission reward rules, reward store, reward redemptions và settings. API export báo cáo quà đã giao trong tháng trả file Excel.

## Role và phạm vi dữ liệu

Controller có `[Authorize]`, tất cả API yêu cầu đăng nhập.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động |
| --- | --- | --- | --- |
| Admin | Stars/XP, reward rules, reward store, redemptions, settings | `all` | `view`, `create`, `edit`, `delete`, `approve`, `cancel`, `deliver`, `export`, `settings` |
| ManagementStaff | Stars/XP, reward rules, reward store, redemptions, settings | `all` | `view`, `create`, `edit`, `delete`, `approve`, `cancel`, `deliver`, `export`, `settings` |
| Teacher | Stars/XP theo `studentProfileId`, attendance streak, reward rules readonly, redemptions list | `all` theo API hiện tại | `view`, `add_stars`, `deduct_stars`, `add_xp`, `deduct_xp` |
| Parent | Active reward store, own-like endpoints nếu token resolve được student | `own` với `/me`/redemption request | `view_active_store`, `request_redemption`, `confirm_received` |
| Student | Active reward store, balance/level/streak/redemptions của chính mình | `own` | `view`, `check_in`, `request_redemption`, `confirm_received` |
| Anonymous | Không được truy cập | `none` | `none` |

Ghi chú: `GET /reward-redemptions/{id}` không gắn role cụ thể ngoài `[Authorize]`; handler hiện không check ownership cho detail. Các endpoint `/me` và `confirm-received` dùng `userContext.StudentId`.

## Response format

Success thông thường:

```json
{ "isSuccess": true, "data": {} }
```

Export Excel:

```text
HTTP 200
Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
Content-Disposition: attachment; filename=...
```

Error:

```json
{
  "title": "RewardRedemption.InvalidStatusTransition",
  "status": 400,
  "detail": "Cannot transition from Requested to Delivered"
}
```

## Enum/status liên quan

| Enum | Values |
| --- | --- |
| `RedemptionStatus` | `Requested`, `Approved`, `Delivered`, `Received`, `Cancelled` |
| `StarSourceType` | `Mission`, `Manual`, `Homework`, `Test`, `Adjustment` |
| `MissionType` | `HomeworkStreak`, `ReadingStreak`, `NoUnexcusedAbsence`, `ClassAttendance`, `Custom` |
| `MissionProgressMode` | `Count`, `Streak` |

## Danh sách API

### Stars, XP, Level

| Method | Endpoint | Roles | Params/Body | Success | Errors |
| --- | --- | --- | --- | --- | --- |
| POST | `/api/gamification/stars/add` | Admin, ManagementStaff, Teacher | Body `studentProfileId: Guid`, `amount: int`, `reason: string?` | `{ studentProfileId, amount, newBalance, reason }` | `404 Star.ProfileNotFound`, `401`, `403` |
| POST | `/api/gamification/stars/deduct` | Admin, ManagementStaff, Teacher | Body `studentProfileId`, `amount`, `reason?` | `{ studentProfileId, amount, newBalance, reason }` | `400 Star.InsufficientBalance`, `404 Star.ProfileNotFound`, `401`, `403` |
| POST | `/api/gamification/xp/add` | Admin, ManagementStaff, Teacher | Body `studentProfileId`, `amount`, `reason?` | `{ studentProfileId, amount, oldXp, newXp }` | `404 Xp.ProfileNotFound`, `401`, `403` |
| POST | `/api/gamification/xp/deduct` | Admin, ManagementStaff, Teacher | Body `studentProfileId`, `amount`, `reason?` | `{ studentProfileId, amount, oldXp, newXp }` | `404 Xp.ProfileNotFound`, `401`, `403` |
| GET | `/api/gamification/stars/transactions` | Admin, ManagementStaff, Teacher | Query `studentProfileId: Guid`, `page=1`, `pageSize=20` | `{ transactions, totalCount, page, pageSize, totalPages }` | `401`, `403` |
| GET | `/api/gamification/stars/balance` | Admin, ManagementStaff, Teacher | Query `studentProfileId: Guid` | `{ studentProfileId, balance }` | `404 Star.ProfileNotFound`, `401`, `403` |
| GET | `/api/gamification/level` | Admin, ManagementStaff, Teacher | Query `studentProfileId: Guid` | `{ studentProfileId, level, xp, xpRequiredForNextLevel }` | `404 Xp.ProfileNotFound`, `401`, `403` |
| GET | `/api/gamification/stars/balance/me` | Logged in | None | Balance của student hiện tại | Lỗi nếu không resolve được student |
| GET | `/api/gamification/level/me` | Logged in | None | Level/XP của student hiện tại | Lỗi nếu không resolve được student |

`transactions[]`: `id`, `studentProfileId`, `amount`, `reason`, `sourceType`, `sourceId`, `balanceAfter`, `createdBy`, `createdByName`, `createdAt`.

### Attendance streak

| Method | Endpoint | Roles | Params/Body | Success | Errors |
| --- | --- | --- | --- | --- | --- |
| GET | `/api/gamification/attendance-streak` | Admin, ManagementStaff, Teacher | Query `studentProfileId: Guid` | `{ studentProfileId, currentStreak, maxStreak, lastAttendanceDate, recentStreaks }` | `404 Star.ProfileNotFound`, `401`, `403` |
| POST | `/api/gamification/attendance-streak/check-in` | Logged in | None | Check-in hôm nay, có thể cộng reward theo settings | Lỗi nếu không resolve được student hoặc đã check-in tùy handler |
| GET | `/api/gamification/attendance-streak/me` | Logged in | None | Streak của student hiện tại | Lỗi nếu không resolve được student |

`recentStreaks[]`: `id`, `attendanceDate`, `currentStreak`, `rewardStars`, `rewardExp`, `createdAt`.

### Mission reward rules

| Method | Endpoint | Roles | Params/Body | Success | Errors |
| --- | --- | --- | --- | --- | --- |
| POST | `/api/gamification/mission-reward-rules` | Admin, ManagementStaff | Body `missionType`, `progressMode`, `totalRequired`, `rewardStars`, `rewardExp`, `isActive` | `201 CreateMissionRewardRuleResponse` | `400 MissionRewardRule.InvalidTotalRequired`, `400 MissionRewardRule.InvalidReward`, `409 MissionRewardRule.Duplicate`, `401`, `403` |
| GET | `/api/gamification/mission-reward-rules` | Admin, ManagementStaff, Teacher | Query `missionType?`, `progressMode?`, `isActive?`, `page=1`, `pageSize=20` | `{ rules: Page<MissionRewardRuleDto> }` | `401`, `403` |
| GET | `/api/gamification/mission-reward-rules/{id}` | Admin, ManagementStaff, Teacher | Path `id` | Rule detail | `404 MissionRewardRule.NotFound`, `401`, `403` |
| PUT | `/api/gamification/mission-reward-rules/{id}` | Admin, ManagementStaff | Body nullable fields như create | Updated rule | `404 MissionRewardRule.NotFound`, validation errors, `401`, `403` |
| PATCH | `/api/gamification/mission-reward-rules/{id}/toggle-status` | Admin, ManagementStaff | Path `id` | Toggle `isActive` | `404 MissionRewardRule.NotFound`, `401`, `403` |

Rule fields: `id`, `missionType`, `progressMode`, `totalRequired`, `rewardStars`, `rewardExp`, `isActive`, `createdAt`, `updatedAt`.

### Reward store items

| Method | Endpoint | Roles | Params/Body | Success | Errors |
| --- | --- | --- | --- | --- | --- |
| POST | `/api/gamification/reward-store/items` | Admin, ManagementStaff | Body `title`, `description?`, `imageUrl?`, `costStars`, `isActive=true` | `201 CreateRewardStoreItemResponse` | `400 RewardStore.InvalidCostStars`, `401`, `403` |
| GET | `/api/gamification/reward-store/items` | Admin, ManagementStaff | Query `isActive?`, `page=1`, `pageSize=20` | `{ items: Page<RewardStoreItemDto> }` | `401`, `403` |
| GET | `/api/gamification/reward-store/items/{id}` | Admin, ManagementStaff | Path `id` | Item detail | `404 RewardStore.NotFound`, `401`, `403` |
| GET | `/api/gamification/reward-store/items/active` | Logged in | Query `page=1`, `pageSize=20` | Chỉ items active | `401` |
| PUT | `/api/gamification/reward-store/items/{id}` | Admin, ManagementStaff | Body `title?`, `description?`, `imageUrl?`, `costStars?`, `isActive?` | Updated item | `400 RewardStore.InvalidCostStars`, `404 RewardStore.NotFound`, `401`, `403` |
| DELETE | `/api/gamification/reward-store/items/{id}` | Admin, ManagementStaff | Path `id` | Soft delete | `404 RewardStore.NotFound`, `401`, `403` |
| PATCH | `/api/gamification/reward-store/items/{id}/toggle-status` | Admin, ManagementStaff | Path `id` | Toggle `isActive` | `404 RewardStore.NotFound`, `401`, `403` |

Item fields: `id`, `title`, `description`, `imageUrl`, `costStars`, `isActive`, `createdAt`.

### Reward redemptions

| Method | Endpoint | Roles | Params/Body | Success | Errors |
| --- | --- | --- | --- | --- | --- |
| POST | `/api/gamification/reward-redemptions` | Logged in, cần student context | Body `itemId: Guid`, `quantity: int = 1` | `201 RequestRewardRedemptionResponse` | `400 RewardRedemption.InvalidQuantity`, `400 RewardRedemption.ItemNotActive`, `409 RewardRedemption.InsufficientStars`, `404 RewardRedemption.ItemNotFound`, `404 RewardRedemption.StudentProfileNotFound` |
| GET | `/api/gamification/reward-redemptions` | Admin, ManagementStaff, Teacher | Query `studentProfileId?`, `itemId?`, `status?`, `page=1`, `pageSize=20` | `{ redemptions: Page<RewardRedemptionDto> }` | `401`, `403` |
| GET | `/api/gamification/reward-redemptions/export-delivered` | Admin, ManagementStaff | Query `year?`, `month?`, `branchId?`, `itemId?` | File Excel `.xlsx` | `400` validation, `401`, `403` |
| GET | `/api/gamification/reward-redemptions/{id}` | Logged in | Path `id` | Redemption detail | `404 RewardRedemption.NotFound`, `401` |
| GET | `/api/gamification/reward-redemptions/me` | Logged in, cần student context | Query `status?`, `page=1`, `pageSize=20` | Redemptions của student hiện tại | `404 RewardRedemption.StudentProfileNotFound`, `401` |
| PATCH | `/api/gamification/reward-redemptions/{id}/approve` | Admin, ManagementStaff | Path `id` | `Requested -> Approved` | `400 RewardRedemption.InvalidStatusTransition`, `404 RewardRedemption.NotFound`, `401`, `403` |
| PATCH | `/api/gamification/reward-redemptions/{id}/cancel` | Admin, ManagementStaff | Body `reason?` | `Requested/Approved -> Cancelled`, hoàn stars | `400 RewardRedemption.InvalidStatusTransition`, `404 RewardRedemption.NotFound`, `401`, `403` |
| PATCH | `/api/gamification/reward-redemptions/{id}/mark-delivered` | Admin, ManagementStaff | Path `id` | `Approved -> Delivered` | `400 RewardRedemption.InvalidStatusTransition`, `404 RewardRedemption.NotFound`, `401`, `403` |
| PATCH | `/api/gamification/reward-redemptions/{id}/confirm-received` | Logged in, owner student | Path `id` | `Delivered -> Received` | `400 RewardRedemption.InvalidStatusTransition`, `404 RewardRedemption.NotFound`, `404 RewardRedemption.StudentProfileNotFound` |
| PATCH | `/api/gamification/reward-redemptions/batch-deliver` | Admin, ManagementStaff | Query `year?`, `month?` | `{ deliveredCount, deliveredRedemptionIds, deliveredAt }` | `401`, `403` |

`RewardRedemptionDto`: `id`, `itemId`, `itemName`, `quantity`, `starsDeducted`, `studentProfileId`, `studentName`, `branchName`, `status`, `cancelReason`, `handledBy`, `handledByName`, `handledAt`, `deliveredAt`, `receivedAt`, `createdAt`.

Export delivered:

- Nếu không gửi `year/month`, BE dùng tháng hiện tại.
- Report gồm các redemption trạng thái `Delivered` và `Received` vì `Received` cũng đã giao.
- Có thể lọc thêm `branchId` và `itemId`.

### Settings

| Method | Endpoint | Roles | Params/Body | Success | Errors |
| --- | --- | --- | --- | --- | --- |
| GET | `/api/gamification/settings` | Admin, ManagementStaff | None | `{ checkInRewardStars, checkInRewardExp }` | `401`, `403` |
| PUT | `/api/gamification/settings` | Admin, ManagementStaff | Body `checkInRewardStars: int`, `checkInRewardExp: int` | Updated settings | `401`, `403` |

## Status definition

### Reward redemption status

| Status | Ý nghĩa |
| --- | --- |
| `Requested` | Học sinh đã đổi quà, stars đã bị trừ. |
| `Approved` | Staff duyệt đơn đổi quà. |
| `Delivered` | Staff đã giao quà. |
| `Received` | Học sinh xác nhận đã nhận quà. |
| `Cancelled` | Staff hủy/từ chối, stars được hoàn. |

Luồng chuyển:

```text
Requested -- approve --> Approved
Requested -- cancel --> Cancelled
Approved -- cancel --> Cancelled
Approved -- mark-delivered --> Delivered
Delivered -- confirm-received --> Received
```

### Reward store item status

| Field | Ý nghĩa |
| --- | --- |
| `isActive = true` | Học sinh có thể thấy trong `/active` và đặt đổi. |
| `isActive = false` | Ẩn khỏi store active, không cho request redemption. |
| `isDeleted = true` | Soft delete, không hiển thị trong list bình thường. |

### Mission reward rule status

| Field | Ý nghĩa |
| --- | --- |
| `isActive = true` | Rule được dùng để resolve reward cho mission. |
| `isActive = false` | Rule lưu trong hệ thống nhưng không dùng. |

## Permission matrix

| Nhóm API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| Stars/XP manual | Yes | Yes | Yes | No | No | No |
| Student balance/level by `studentProfileId` | Yes | Yes | Yes | No | No | No |
| `/me` balance/level/streak | Yes nếu có student context | Yes nếu có student context | Yes nếu có student context | Yes | Yes | No |
| Mission reward rules | CRUD | CRUD | View | No | No | No |
| Reward store management | CRUD | CRUD | No | No | No | No |
| Active reward store | Yes | Yes | Yes | Yes | Yes | No |
| Request redemption | Yes nếu có student context | Yes nếu có student context | Yes nếu có student context | Yes nếu có student context | Yes | No |
| Manage redemptions approve/cancel/deliver/export | Yes | Yes | No | No | No | No |
| Redemption list | Yes | Yes | Yes | No | No | No |
| Redemption `/me` và confirm received | Có student context | Có student context | Có student context | Yes | Yes | No |
| Settings | Yes | Yes | No | No | No | No |

## Validation rules

| Rule | API áp dụng | Lỗi |
| --- | --- | --- |
| Đăng nhập bắt buộc | Tất cả | 401 |
| Role đúng | Các API có `[Authorize(Roles=...)]` | 403 |
| `studentProfileId` phải tồn tại | Stars/XP/balance/level/streak | 404 |
| Deduct stars không được vượt balance | `stars/deduct` | 400 `Star.InsufficientBalance` |
| Reward rule `totalRequired > 0` | Create/update rule | 400 `MissionRewardRule.InvalidTotalRequired` |
| Reward rule stars/exp không âm và ít nhất một giá trị > 0 | Create/update rule | 400 `MissionRewardRule.InvalidReward` |
| Không duplicate reward rule theo `missionType/progressMode/totalRequired` | Create rule | 409 `MissionRewardRule.Duplicate` |
| Reward item `costStars > 0` | Create/update item | 400 `RewardStore.InvalidCostStars` |
| Redemption `quantity > 0` | Request redemption | 400 `RewardRedemption.InvalidQuantity` |
| Item phải tồn tại và active | Request redemption | 404/400 |
| Student phải đủ stars | Request redemption | 409 `RewardRedemption.InsufficientStars` |
| Approve chỉ từ `Requested` | Approve | 400 `RewardRedemption.InvalidStatusTransition` |
| Cancel chỉ từ `Requested` hoặc `Approved` | Cancel | 400 `RewardRedemption.InvalidStatusTransition` |
| Mark delivered chỉ từ `Approved` | Mark delivered | 400 `RewardRedemption.InvalidStatusTransition` |
| Confirm received chỉ từ `Delivered` và đúng owner student | Confirm received | 400/404 |

