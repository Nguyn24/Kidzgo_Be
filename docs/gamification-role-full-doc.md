# Tai lieu Gamification theo Role

## 1. Muc dich

Tai lieu nay mo ta implementation gamification hien tai trong codebase sau khi da pull code moi nhat.
Noi dung duoc cap nhat theo code that, khong dua tren mock flow cu.

Pham vi:

- Mission
- Mission Progress
- Stars
- XP / Level
- Attendance Streak
- Reward Store
- Reward Redemption
- Gamification Settings
- Diem lien ket giua Homework va Mission

Nguon doi chieu chinh:

- `Kidzgo.API/Controllers/MissionController.cs`
- `Kidzgo.API/Controllers/GamificationController.cs`
- `Kidzgo.API/Controllers/HomeworkController.cs`
- `Kidzgo.Application/Missions/*`
- `Kidzgo.Application/Gamification/*`
- `Kidzgo.Application/Homework/SubmitHomework/SubmitHomeworkCommandHandler.cs`
- `Kidzgo.Domain/Gamification/*`
- `Kidzgo.Infrastructure/BackgroundJobs/AutoConfirmRewardRedemptionJob.cs`

## 2. Tong quan mo hinh du lieu

### 2.1. Mission

Entity `Mission` hien tai gom cac field nghiep vu chinh:

- `Id`
- `Title`
- `Description`
- `Scope`: `Class`, `Student`, `Group`
- `TargetClassId`
- `TargetStudentId`
- `TargetGroup`: danh sach `StudentProfileId`
- `MissionType`: `HomeworkStreak`, `ReadingStreak`, `NoUnexcusedAbsence`, `Custom`
- `StartAt`
- `EndAt`
- `RewardStars`
- `RewardExp`
- `TotalRequired`
- `CreatedBy`
- `CreatedAt`

Ghi chu:

- `TargetStudentId` chi dung cho `Scope = Student`.
- `TotalRequired` dang la target value cho cac mission dang streak/so lan.
- Mission hien tai khong co status enum rieng.

### 2.2. MissionProgress

Entity `MissionProgress` gom:

- `Id`
- `MissionId`
- `StudentProfileId`
- `Status`: `Assigned`, `InProgress`, `Completed`, `Expired`
- `ProgressValue`
- `CompletedAt`
- `VerifiedBy`

Ghi chu:

- Mission progress duoc auto tao khi tao mission.
- Chua co public API de edit progress thu cong.

### 2.3. StarTransaction

Stars duoc luu theo dang ledger, khong luu balance tong truc tiep.

Field chinh:

- `StudentProfileId`
- `Amount`
- `Reason`
- `SourceType`: `Mission`, `Manual`, `Homework`, `Test`, `Adjustment`
- `SourceId`
- `BalanceAfter`
- `CreatedBy`
- `CreatedAt`

Balance hien tai luon lay tu `BalanceAfter` cua transaction moi nhat.

### 2.4. StudentLevel

XP va level duoc luu trong `StudentLevel`:

- `StudentProfileId`
- `CurrentLevel`
- `CurrentXp`
- `UpdatedAt`

Cong thuc level dang dung:

- `Level = floor(totalXp / 100) + 1`
- `0-99 XP = Level 1`
- `100-199 XP = Level 2`
- `200-299 XP = Level 3`

XP can de len level tiep theo:

- `100 - (currentXp % 100)`
- Neu chia het cho `100` thi tra `100`

### 2.5. AttendanceStreak

Moi lan check-in tao 1 record:

- `StudentProfileId`
- `AttendanceDate`
- `CurrentStreak`
- `RewardStars`
- `RewardExp`
- `CreatedAt`

### 2.6. RewardStoreItem

Field chinh:

- `Title`
- `Description`
- `ImageUrl`
- `CostStars`
- `Quantity`
- `IsActive`
- `IsDeleted`
- `CreatedAt`
- `UpdatedAt`

Delete dang la soft delete bang `IsDeleted = true`.

### 2.7. RewardRedemption

Field chinh:

- `ItemId`
- `ItemName`
- `Quantity`
- `StudentProfileId`
- `Status`: `Requested`, `Approved`, `Delivered`, `Received`, `Cancelled`
- `HandledBy`
- `HandledAt`
- `DeliveredAt`
- `ReceivedAt`
- `CreatedAt`

`ItemName` la snapshot ten qua tai thoi diem doi qua.

### 2.8. GamificationSettings

Bang settings hien tai moi quan ly reward cho check-in:

- `CheckInRewardStars`
- `CheckInRewardExp`
- `CreatedAt`
- `UpdatedAt`

Neu chua co record settings, system fallback:

- `CheckInRewardStars = 1`
- `CheckInRewardExp = 5`

## 3. Business rule hien tai

### 3.1. Access model

- API dung `User.Role` trong JWT de authorize.
- Cac API learner dang dung student-context qua `StudentId` trong token.
- `StudentId` thuong duoc nap sau khi chon profile student.
- Cac endpoint `me`, `check-in`, `request reward`, `confirm received` phu thuoc truc tiep vao `StudentId`.

### 3.2. Mission

- `Scope = Class` bat buoc co `TargetClassId`.
- `Scope = Student` bat buoc co `TargetStudentId`.
- `Scope = Group` bat buoc co `TargetGroup` va danh sach khong rong.
- `EndAt` phai sau `StartAt`.
- `StartAt` va `EndAt` neu co se duoc chuyen sang UTC trong handler.
- Khi tao mission, backend auto tao `MissionProgress` cho tung hoc sinh target.

Rule target student:

- `Class`: lay hoc sinh dang `EnrollmentStatus.Active` trong lop.
- `Student`: tao progress cho 1 hoc sinh.
- `Group`: tao progress cho danh sach hoc sinh trong `TargetGroup`.

Xoa mission:

- Chi xoa duoc khi mission chua co `MissionProgress`.
- Neu da co progress thi tra `MissionInUse`.

### 3.3. Mission progress

Enum co san:

- `Assigned`
- `InProgress`
- `Completed`
- `Expired`

Trang thai `Expired` da ton tai trong domain nhung hien chua co flow public/update job ro rang de set tu dong trong cac handler da ra soat.

### 3.4. Homework va Mission

Mission co the duoc gan vao homework theo 3 cach:

- Gan ngay luc tao homework thuong
- Gan luc tao homework multiple choice
- Dung endpoint `POST /api/homework/{id}/link-mission`

Auto tracking dang co:

- `HomeworkStreak`
  - Khi hoc sinh nop bai dung han, tat ca mission `HomeworkStreak` dang active cua hoc sinh se duoc tang `ProgressValue + 1`
  - Neu dang `Assigned` thi chuyen sang `InProgress`
  - Khi dat `TotalRequired` thi chuyen `Completed` va cong reward mission
- `NoUnexcusedAbsence`
  - Khi check-in hang ngay, tat ca mission dang active loai nay cua hoc sinh duoc tang `ProgressValue + 1`
  - Neu dat `TotalRequired` thi chuyen `Completed` va cong reward mission

Hien chua thay implementation auto tracking cho:

- `ReadingStreak`
- `Custom`

### 3.5. Stars

- Stars luu theo ledger trong `StarTransactions`.
- `AddStars` tao transaction moi voi `Amount > 0`, `SourceType = Manual`.
- `DeductStars` tao transaction moi voi `Amount < 0`, `SourceType = Adjustment`.
- `DeductStars` se chan neu balance hien tai khong du.
- `GetStarBalance` va `GetMyStarBalance` lay balance tu transaction moi nhat.

### 3.6. XP / Level

- `AddXp` se tao hoac cap nhat `StudentLevel`.
- `DeductXp` khong cho XP am, gia tri toi thieu la `0`.
- Level duoc tinh lai moi lan add/deduct XP.

### 3.7. Attendance streak

- Moi hoc sinh chi check-in 1 lan/ngay theo `UTC date`.
- Neu hom nay da check-in roi, API tra lai record cu va `IsNewStreak = false`.
- Neu hom qua co check-in, `CurrentStreak = streak hom qua + 1`.
- Neu hom qua khong co check-in, streak reset ve `1`.
- Moi lan check-in moi nhan reward theo `GamificationSettings`.
- `GetAttendanceStreak` va `GetMyAttendanceStreak` tra:
  - `CurrentStreak`
  - `MaxStreak`
  - `LastAttendanceDate`
  - `RecentStreaks` toi da 30 ban ghi gan nhat

### 3.8. Reward store

- `CostStars` phai `> 0`
- `Quantity` phai `>= 0`
- Delete la soft delete
- Toggle status chi dao `IsActive`
- API active items cho learner chi loc `IsActive = true`, van bo qua item soft-deleted

### 3.9. Reward redemption

Khi learner request doi qua:

- Xac thuc student profile trong token
- Item phai ton tai va khong bi soft delete
- Item phai `IsActive = true`
- `Quantity` request phai `> 0`
- Ton kho phai du
- Student phai du stars
- He thong tru stars ngay
- He thong giam ton kho ngay
- Tao `RewardRedemption` voi status `Requested`
- Snapshot `ItemName` tai thoi diem doi qua

Khi staff cancel:

- Chi cancel duoc tu `Requested` hoac `Approved`
- He thong hoan ton kho
- He thong refund stars
- Cap nhat redemption thanh `Cancelled`

Khi staff approve:

- Chi cho `Requested -> Approved`

Khi staff mark delivered:

- Chi cho `Approved -> Delivered`

Khi learner confirm received:

- Chi cho hoc sinh so huu redemption do
- Chi cho `Delivered -> Received`

Auto confirm:

- Quartz job `AutoConfirmRewardRedemptionJob`
- Quet redemption `Delivered` ma chua `Received`
- Neu `DeliveredAt` da qua N ngay thi auto set `Received`
- So ngay cho duoc doc tu `Quartz:Schedules:AutoConfirmRewardRedemptionJob_Days`
- Fallback hien tai la `3` ngay

### 3.10. Gamification settings

- `GET /api/gamification/settings` tra settings hien tai
- `PUT /api/gamification/settings` cap nhat reward cho daily check-in
- Neu chua co row settings, update API se tao moi row dau tien voi `Id = 1`

## 4. Role va pham vi du lieu

| Role / Context | Data scope thuc te | Ghi chu |
|---|---|---|
| `Admin` | all | full mission, stars, xp, reward store, reward redemption, settings |
| `ManagementStaff` | all | gan nhu Admin trong gamification |
| `Teacher` | all tren cac API duoc mo | co create/update mission, add/deduct star/xp, xem redemption list, xem streak |
| `Parent` co `StudentId` | own student context | dung cac API `me`, request redemption, confirm received, check-in neu token mang `StudentId` |
| `Student` co `StudentId` | own | dung cac API `me`, request redemption, confirm received, check-in |
| `TeachingAssistant` | rat han che | khong co API gamification rieng; co tham gia grade homework o module homework |

Luu y quan trong:

- Mot so API learner hien khong check role rieng tai action, ma dua vao authenticated + `StudentId`.
- `GET /api/missions`, `GET /api/missions/{id}`, `GET /api/missions/{id}/progress` hien chua thay ownership filter trong handler.
- `GET /api/gamification/reward-redemptions/{id}` hien la authenticated-only, chua co check ownership trong handler.

## 5. Permission matrix

| Chuc nang | Admin | ManagementStaff | Teacher | Parent / Student context |
|---|---|---|---|---|
| Xem mission list | Yes | Yes | Yes | Yes |
| Xem mission detail | Yes | Yes | Yes | Yes |
| Tao mission | Yes | Yes | Yes | No |
| Cap nhat mission | Yes | Yes | Yes | No |
| Xoa mission | Yes | Yes | No | No |
| Xem mission progress theo mission | Yes | Yes | Yes | Yes |
| Xem missions cua chinh minh (`me/progress`) | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| Add stars | Yes | Yes | Yes | No |
| Deduct stars | Yes | Yes | Yes | No |
| Xem star transactions theo student | Yes | Yes | Yes | No |
| Xem star balance theo student | Yes | Yes | Yes | No |
| Xem star balance `/me` | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| Add XP | Yes | Yes | Yes | No |
| Deduct XP | Yes | Yes | Yes | No |
| Xem level theo student | Yes | Yes | Yes | No |
| Xem level `/me` | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| Xem attendance streak theo student | Yes | Yes | Yes | No |
| Check-in streak | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| Xem attendance streak `/me` | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| CRUD reward store | Yes | Yes | No | No |
| Xem reward store active | Yes | Yes | Yes neu chi authenticated | Yes |
| Request reward redemption | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| Xem reward redemption list | Yes | Yes | Yes | No |
| Xem reward redemption by id | Yes | Yes | Yes | Yes, nhung hien chua co ownership check |
| Xem reward redemptions `/me` | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| Approve / Cancel / Deliver redemption | Yes | Yes | No | No |
| Batch deliver redemptions | Yes | Yes | No | No |
| Confirm received redemption | Neu co `StudentId` | Neu co `StudentId` | Neu co `StudentId` | Yes |
| Xem / sua gamification settings | Yes | Yes | No | No |
| Link homework voi mission | Yes | Yes | Yes | No |

## 6. Status va state transition

### 6.1. MissionProgressStatus

| Status | Y nghia |
|---|---|
| `Assigned` | da duoc giao cho hoc sinh |
| `InProgress` | hoc sinh da bat dau tich luy tien do |
| `Completed` | da dat muc tieu `TotalRequired` hoac logic tuong ung |
| `Expired` | co enum trong domain, nhung chua thay flow public/update tu dong ro rang |

### 6.2. RedemptionStatus

| Status | Y nghia |
|---|---|
| `Requested` | learner vua doi qua, da bi tru stars va giu ton |
| `Approved` | staff da duyet |
| `Delivered` | staff da giao qua |
| `Received` | learner hoac system da xac nhan nhan qua |
| `Cancelled` | request bi huy, da refund stars va hoan ton |

### 6.3. State transition dang implement

| Tu | Sang | Trigger |
|---|---|---|
| `Requested` | `Approved` | approve API |
| `Requested` | `Cancelled` | cancel API |
| `Approved` | `Cancelled` | cancel API |
| `Approved` | `Delivered` | mark-delivered API |
| `Delivered` | `Received` | confirm-received API |
| `Delivered` | `Received` | auto-confirm Quartz job |

## 7. Danh sach API hien tai

### 7.1. Missions

| Method | Route | Role |
|---|---|---|
| `POST` | `/api/missions` | `Admin,ManagementStaff,Teacher` |
| `GET` | `/api/missions` | `Admin,ManagementStaff,Teacher,Parent,Student` |
| `GET` | `/api/missions/{id}` | `Admin,ManagementStaff,Teacher,Parent,Student` |
| `PUT` | `/api/missions/{id}` | `Admin,ManagementStaff,Teacher` |
| `DELETE` | `/api/missions/{id}` | `Admin,ManagementStaff` |
| `GET` | `/api/missions/{id}/progress` | `Admin,ManagementStaff,Teacher,Parent,Student` |
| `GET` | `/api/missions/me/progress` | `Admin,ManagementStaff,Teacher,Parent,Student` |

Filter/query hien co cho mission list:

- `scope`
- `targetClassId`
- `targetStudentId`
- `missionType`
- `searchTerm`
- `pageNumber`
- `pageSize`

Luu y:

- `GetMissionsQuery` co field `TargetGroup`, nhung handler hien chua filter theo field nay.

### 7.2. Stars va XP

| Method | Route | Role |
|---|---|---|
| `POST` | `/api/gamification/stars/add` | `Admin,ManagementStaff,Teacher` |
| `POST` | `/api/gamification/stars/deduct` | `Admin,ManagementStaff,Teacher` |
| `POST` | `/api/gamification/xp/add` | `Admin,ManagementStaff,Teacher` |
| `POST` | `/api/gamification/xp/deduct` | `Admin,ManagementStaff,Teacher` |
| `GET` | `/api/gamification/stars/transactions` | `Admin,ManagementStaff,Teacher` |
| `GET` | `/api/gamification/stars/balance` | `Admin,ManagementStaff,Teacher` |
| `GET` | `/api/gamification/level` | `Admin,ManagementStaff,Teacher` |
| `GET` | `/api/gamification/stars/balance/me` | authenticated |
| `GET` | `/api/gamification/level/me` | authenticated |

### 7.3. Attendance streak

| Method | Route | Role |
|---|---|---|
| `GET` | `/api/gamification/attendance-streak` | `Admin,ManagementStaff,Teacher` |
| `POST` | `/api/gamification/attendance-streak/check-in` | authenticated |
| `GET` | `/api/gamification/attendance-streak/me` | authenticated |

### 7.4. Reward store

| Method | Route | Role |
|---|---|---|
| `POST` | `/api/gamification/reward-store/items` | `Admin,ManagementStaff` |
| `GET` | `/api/gamification/reward-store/items` | `Admin,ManagementStaff` |
| `GET` | `/api/gamification/reward-store/items/{id}` | `Admin,ManagementStaff` |
| `GET` | `/api/gamification/reward-store/items/active` | authenticated |
| `PUT` | `/api/gamification/reward-store/items/{id}` | `Admin,ManagementStaff` |
| `DELETE` | `/api/gamification/reward-store/items/{id}` | `Admin,ManagementStaff` |
| `PATCH` | `/api/gamification/reward-store/items/{id}/toggle-status` | `Admin,ManagementStaff` |

Filter list:

- `isActive`
- `page`
- `pageSize`

### 7.5. Reward redemption

| Method | Route | Role |
|---|---|---|
| `POST` | `/api/gamification/reward-redemptions` | authenticated |
| `GET` | `/api/gamification/reward-redemptions` | `Admin,ManagementStaff,Teacher` |
| `GET` | `/api/gamification/reward-redemptions/{id}` | authenticated |
| `GET` | `/api/gamification/reward-redemptions/me` | authenticated |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/approve` | `Admin,ManagementStaff` |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/cancel` | `Admin,ManagementStaff` |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/mark-delivered` | `Admin,ManagementStaff` |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/confirm-received` | authenticated |
| `PATCH` | `/api/gamification/reward-redemptions/batch-deliver` | `Admin,ManagementStaff` |

Filter list staff:

- `studentProfileId`
- `itemId`
- `status`
- `page`
- `pageSize`

Filter list learner:

- `status`
- `page`
- `pageSize`

### 7.6. Settings

| Method | Route | Role |
|---|---|---|
| `GET` | `/api/gamification/settings` | `Admin,ManagementStaff` |
| `PUT` | `/api/gamification/settings` | `Admin,ManagementStaff` |

### 7.7. Homework lien quan Mission

| Method | Route | Role |
|---|---|---|
| `POST` | `/api/homework` | `Teacher,ManagementStaff,Admin` |
| `POST` | `/api/homework/multiple-choice` | `Teacher,ManagementStaff,Admin` |
| `POST` | `/api/homework/multiple-choice/from-bank` | `Teacher,ManagementStaff,Admin` |
| `PUT` | `/api/homework/{id}` | `Teacher,ManagementStaff,Admin` |
| `POST` | `/api/homework/{id}/link-mission` | `Teacher,ManagementStaff,Admin` |

Ghi chu:

- Cac flow tao/sua homework deu co `MissionId`.
- Homework thuong hien co field teacher attachment trong create flow.

## 8. Response va status code

### 8.1. Success shape

He thong dang dung wrapper ket qua chung:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 8.2. Error shape

Loi thuong tra ve theo `ProblemDetails`, vi du:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": []
}
```

### 8.3. HTTP status code thuong gap

| Code | Y nghia |
|---|---|
| `400` | validation / transition khong hop le |
| `401` | thieu token / token sai |
| `403` | sai role |
| `404` | khong tim thay, hoac learner khong so huu resource va bi che bang not found |
| `409` | conflict du lieu |
| `500` | loi he thong |

## 9. Cac diem can luu y khi tich hop Frontend

### 9.1. Mission

- Frontend nen hien thi `TotalRequired` cho cac mission streak vi backend dang dung field nay de tinh complete.
- Khong nen gia dinh moi mission type deu tu dong tang progress. Hien moi chac chan co `HomeworkStreak` va `NoUnexcusedAbsence`.

### 9.2. Reward redemption

- Request doi qua se tru stars ngay, khong doi staff approve moi tru.
- Cancel boi staff se refund stars va tra lai quantity.
- Learner co the thay redemption detail theo id, nhung hien backend chua check ownership trong handler. Neu day la man learner, frontend nen goi uu tien `/me`.

### 9.3. Attendance

- Check-in dung ngay UTC, khong phai local branch time.
- Neu UI tinh streak theo gio dia phuong thi can canh bao sai lech quanh moc 00:00 UTC.

### 9.4. Balance va level

- Neu hoc sinh chua co transaction stars thi balance = `0`.
- Neu hoc sinh chua co `StudentLevel` thi level duoc tinh on-the-fly tu XP = `0`, tuc `Level 1`.

## 10. Known gaps / implementation notes

- `ReadingStreak` co enum nhung chua thay logic auto-track trong code da ra soat.
- `Custom` mission type chua thay generic executor rieng.
- `MissionProgressStatus.Expired` co enum nhung chua thay job/handler public cap nhat ro rang.
- `GET /api/missions*` hien chua thay ownership filter trong handler.
- `GET /api/gamification/reward-redemptions/{id}` hien chua check ownership trong handler.
- `GetMissionProgress` hien tinh `ProgressPercentage` bang cach clamp `ProgressValue` ve `0..100`, khong dua tren `TotalRequired`.
- `GetMyMissions` lai tinh `ProgressPercentage` dua tren `TotalRequired` neu field nay co gia tri.
- `GetMissions` co field query `TargetGroup` trong object query nhung controller va handler hien chua dung de filter.

## 11. Ket luan

Tinh den hien tai, gamification module da co day du nhom chuc nang sau:

- Quan ly mission va auto tao mission progress
- Cong/tru stars
- Cong/tru XP va tinh level
- Daily check-in va attendance streak
- Reward store
- Reward redemption day du status flow
- Settings cho reward check-in
- Auto-confirm reward redemption qua Quartz

Tuy nhien, mot so phan van can tiep tuc hoan thien neu muon siet nghiep vu san xuat:

- ownership filtering cho learner APIs
- auto expire mission progress
- implementation ro rang cho `ReadingStreak` va `Custom`
- thong nhat cong thuc `ProgressPercentage` giua cac API
