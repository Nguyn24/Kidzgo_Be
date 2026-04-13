# Gamification & Mission FE API Doc

Last updated: `2026-04-13`

Nguon doi chieu:

- `Kidzgo.API/Controllers/GamificationController.cs`
- `Kidzgo.API/Controllers/MissionController.cs`
- `Kidzgo.Application/Gamification/*`
- `Kidzgo.Application/Missions/*`
- `Kidzgo.Domain/Gamification/*`

Tai lieu nay mo ta contract backend hien tai de FE biet role nao duoc xem du lieu gi, scope du lieu thuc te, action duoc phep, danh sach API, response, status, permission matrix va validation/error.

## 1. Response Chung

Tat ca API thanh cong duoc wrap bang `ApiResult<T>`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

API tao moi dung `201 Created`; API con lai thuong dung `200 OK`.

Response phan trang dung `Page<T>`:

```json
{
  "isSuccess": true,
  "data": {
    "items": [],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 0,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

Mot so API cu nhu star transactions tra pagination custom:

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

Response loi dung `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Mission.InvalidScope",
  "status": 400,
  "detail": "Invalid mission scope. For Class scope, TargetClassId is required. For Student scope, TargetStudentId is required. For Group scope, TargetGroup is required."
}
```

HTTP status chung:

| HTTP status | Y nghia |
| --- | --- |
| `200 OK` | Xu ly thanh cong |
| `201 Created` | Tao moi thanh cong |
| `400 Bad Request` | Validation fail hoac invalid status transition |
| `401 Unauthorized` | Thieu token / token khong hop le |
| `403 Forbidden` | Co token nhung khong dung role theo `[Authorize(Roles = ...)]` |
| `404 Not Found` | Khong tim thay entity/profile hoac token khong co `StudentId` cho API `me` |
| `409 Conflict` | Xung dot nghiep vu: thieu sao, duplicate reward rule, mission dang co progress |
| `500 Internal Server Error` | Loi he thong chua duoc map thanh domain error |

## 2. Role, Scope Du Lieu, Action

| Role | Du lieu duoc xem | Scope du lieu thuc te hien tai | Action chinh |
| --- | --- | --- | --- |
| `Admin` | Mission, mission progress, stars, XP/level, attendance streak, mission reward rules, reward store, reward redemption, settings | `all` | `view`, `create`, `edit`, `approve`, `cancel`, `deliver`, `delete`, `update settings` |
| `ManagementStaff` | Tuong tu `Admin` trong module nay | `all` | `view`, `create`, `edit`, `approve`, `cancel`, `deliver`, `delete`, `update settings` |
| `Teacher` | Mission, mission progress, manual stars/xp, star transactions, balance/level, attendance streak, reward redemptions list, mission reward rules read | `all` voi nhieu API read; `teacher-scoped targets` khi tao/sua mission | `view`, `create mission`, `edit mission`, `add/deduct stars`, `add/deduct xp` |
| `Parent` | Mission list/detail/progress, active reward store items, mot so API `me` neu token co selected student | `selected student in token` voi API `me`; mot so API mission/detail hien dang `all` | `view` |
| `Student` | Star balance cua minh, level cua minh, attendance streak cua minh, mission progress cua minh, active reward store, reward redemption cua minh | `own` / `selected student in token` | `view`, `check-in`, `request reward`, `confirm received` |

Ghi chu quan trong cho FE/BE:

- `department` / `branch-only` chua duoc enforce trong cac handler cua module nay.
- `GET /api/missions`, `GET /api/missions/{id}`, `GET /api/missions/{id}/progress` cho Parent/Student ve route, nhung handler hien chua filter theo linked child / owner.
- `GET /api/gamification/reward-redemptions/{id}` cho moi authenticated user, handler hien chua enforce owner tru khi confirm received.
- `Teacher` bi guard target khi create/update mission: chi target class dang day hoac student trong class dang day.

## 3. Status Definition

### 3.1. MissionScope

| Status | Y nghia |
| --- | --- |
| `Class` | Mission ap dung cho hoc sinh active trong mot lop |
| `Student` | Mission ap dung cho mot `TargetStudentId` |
| `Group` | Mission ap dung cho danh sach `TargetGroup` |

### 3.2. MissionType

| Status | Y nghia |
| --- | --- |
| `HomeworkStreak` | Nhiem vu chuoi/so luong hoan thanh homework |
| `ReadingStreak` | Loai mission doc sach, hien co enum nhung chua thay tracker rieng trong controller nay |
| `NoUnexcusedAbsence` | Daily check-in / khong vang khong phep |
| `ClassAttendance` | Diem danh tren lop tu bang Attendance |
| `Custom` | Mission tuy chinh |

### 3.3. MissionProgressMode

| Status | Y nghia |
| --- | --- |
| `Count` | Cong don so lan dat dieu kien |
| `Streak` | Chuoi lien tiep; neu miss theo rule tracker thi progress co the reset |

### 3.4. MissionProgressStatus

| Status | Y nghia |
| --- | --- |
| `Assigned` | Da giao, progress = 0 hoac chua bat dau |
| `InProgress` | Dang tien hanh, progress > 0 nhung chua dat `TotalRequired` |
| `Completed` | Da dat `TotalRequired`, da cong reward mission neu co |
| `Expired` | Enum co san, hien chua thay job public trong controller nay auto set expired |

Luong chuyen trang thai mission progress:

- Tao mission: `Assigned`, `ProgressValue = 0`.
- Tracker cap nhat co progress: `Assigned -> InProgress`.
- Dat `TotalRequired`: `InProgress/Assigned -> Completed`, set `CompletedAt`, cong `RewardStars` va `RewardExp`.
- Streak bi miss: co the reset ve `Assigned`, `ProgressValue = 0`, `CompletedAt = null`.

### 3.5. RedemptionStatus

| Status | Y nghia |
| --- | --- |
| `Requested` | Hoc sinh da gui yeu cau doi qua; stars da bi tru |
| `Approved` | Admin/ManagementStaff da duyet yeu cau |
| `Delivered` | Trung tam da giao qua |
| `Received` | Hoc sinh xac nhan da nhan qua |
| `Cancelled` | Yeu cau bi huy/tu choi; stars duoc refund |

Luong chuyen trang thai reward redemption:

- `Requested -> Approved`: API approve.
- `Requested -> Cancelled`: API cancel, refund stars.
- `Approved -> Cancelled`: API cancel, refund stars.
- `Approved -> Delivered`: API mark delivered hoac batch deliver.
- `Delivered -> Received`: hoc sinh xac nhan da nhan.
- Khong cho `Delivered/Received/Cancelled -> Cancelled`.
- Khong cho `Requested -> Delivered` truc tiep.

### 3.6. RewardStoreItem / MissionRewardRule

| Field | Y nghia |
| --- | --- |
| `RewardStoreItem.IsActive` | Item dang hien thi/cho request doi qua |
| `RewardStoreItem.IsDeleted` | Xoa mem; API list/detail khong tra item da deleted |
| `MissionRewardRule.IsActive` | Rule dang duoc dung de resolve reward khi tao/sua mission |

## 4. Permission Matrix Theo Role

| Action | Admin | ManagementStaff | Teacher | Parent | Student |
| --- | --- | --- | --- | --- | --- |
| View mission list/detail/progress | Yes | Yes | Yes | Yes | Yes |
| Create mission | Yes | Yes | Yes, target bi gioi han | No | No |
| Update mission | Yes | Yes | Yes, target bi gioi han | No | No |
| Delete mission | Yes | Yes | No | No | No |
| View own mission progress | Neu token co StudentId | Neu token co StudentId | Neu token co StudentId | Neu token co StudentId | Yes |
| Add/deduct stars | Yes | Yes | Yes | No | No |
| Add/deduct XP | Yes | Yes | Yes | No | No |
| View star transactions by student | Yes | Yes | Yes | No | No |
| View star balance/level by student | Yes | Yes | Yes | No | No |
| View own balance/level/streak | Neu token co StudentId | Neu token co StudentId | Neu token co StudentId | Neu token co StudentId | Yes |
| Daily check-in | Authenticated + token co StudentId | Authenticated + token co StudentId | Authenticated + token co StudentId | Authenticated + token co StudentId | Yes |
| Manage mission reward rules | Yes | Yes | View only | No | No |
| Manage reward store items | Yes | Yes | No | No | No |
| View active reward store items | Yes | Yes | Yes | Yes | Yes |
| Request reward redemption | Authenticated + token co StudentId | Authenticated + token co StudentId | Authenticated + token co StudentId | Authenticated + token co StudentId | Yes |
| View reward redemptions list | Yes | Yes | Yes | No | No |
| View own reward redemptions | Authenticated + token co StudentId | Authenticated + token co StudentId | Authenticated + token co StudentId | Authenticated + token co StudentId | Yes |
| Approve/cancel/deliver reward redemption | Yes | Yes | No | No | No |
| Confirm received | Authenticated + owns redemption by StudentId | Authenticated + owns redemption by StudentId | Authenticated + owns redemption by StudentId | Authenticated + owns redemption by StudentId | Yes |
| View/update gamification settings | Yes | Yes | No | No | No |

## 5. API Catalog

### 5.1. Mission APIs

| Method | Endpoint | Roles | Scope thuc te | Mo ta |
| --- | --- | --- | --- | --- |
| `POST` | `/api/missions` | Admin, ManagementStaff, Teacher | Admin/Management: `all`; Teacher: target dang day | Tao mission va auto tao mission progress |
| `GET` | `/api/missions` | Admin, ManagementStaff, Teacher, Parent, Student | `all` | Lay danh sach mission co filter |
| `GET` | `/api/missions/{id}` | Admin, ManagementStaff, Teacher, Parent, Student | `all` | Lay chi tiet mission |
| `PUT` | `/api/missions/{id}` | Admin, ManagementStaff, Teacher | Admin/Management: `all`; Teacher: target dang day | Cap nhat mission |
| `DELETE` | `/api/missions/{id}` | Admin, ManagementStaff | `all` | Xoa mission neu chua co progress |
| `GET` | `/api/missions/{id}/progress` | Admin, ManagementStaff, Teacher, Parent, Student | `all` | Xem progress cua mission |
| `GET` | `/api/missions/me/progress` | Authenticated roles listed in controller | `selected student in token` | Xem mission progress cua student dang select |

### 5.2. Gamification APIs

| Method | Endpoint | Roles | Scope thuc te | Mo ta |
| --- | --- | --- | --- | --- |
| `POST` | `/api/gamification/stars/add` | Admin, ManagementStaff, Teacher | `all` | Cong stars thu cong |
| `POST` | `/api/gamification/stars/deduct` | Admin, ManagementStaff, Teacher | `all` | Tru stars thu cong |
| `POST` | `/api/gamification/xp/add` | Admin, ManagementStaff, Teacher | `all` | Cong XP thu cong |
| `POST` | `/api/gamification/xp/deduct` | Admin, ManagementStaff, Teacher | `all` | Tru XP thu cong |
| `GET` | `/api/gamification/stars/transactions` | Admin, ManagementStaff, Teacher | `all` | Lich su star transactions theo student |
| `GET` | `/api/gamification/stars/balance` | Admin, ManagementStaff, Teacher | `all` | Balance stars theo student |
| `GET` | `/api/gamification/level` | Admin, ManagementStaff, Teacher | `all` | Level/XP theo student |
| `GET` | `/api/gamification/stars/balance/me` | Authenticated | `selected student in token` | Balance stars cua student dang select |
| `GET` | `/api/gamification/level/me` | Authenticated | `selected student in token` | Level/XP cua student dang select |
| `GET` | `/api/gamification/attendance-streak` | Admin, ManagementStaff, Teacher | `all` | Attendance streak theo student |
| `POST` | `/api/gamification/attendance-streak/check-in` | Authenticated | `selected student in token` | Daily check-in |
| `GET` | `/api/gamification/attendance-streak/me` | Authenticated | `selected student in token` | Attendance streak cua student dang select |
| `POST` | `/api/gamification/mission-reward-rules` | Admin, ManagementStaff | `all` | Tao reward rule cho mission |
| `GET` | `/api/gamification/mission-reward-rules` | Admin, ManagementStaff, Teacher | `all` | List reward rules |
| `GET` | `/api/gamification/mission-reward-rules/{id}` | Admin, ManagementStaff, Teacher | `all` | Detail reward rule |
| `PUT` | `/api/gamification/mission-reward-rules/{id}` | Admin, ManagementStaff | `all` | Update reward rule |
| `PATCH` | `/api/gamification/mission-reward-rules/{id}/toggle-status` | Admin, ManagementStaff | `all` | Toggle active reward rule |
| `POST` | `/api/gamification/reward-store/items` | Admin, ManagementStaff | `all` | Tao item doi qua |
| `GET` | `/api/gamification/reward-store/items` | Admin, ManagementStaff | `all` | List item doi qua |
| `GET` | `/api/gamification/reward-store/items/{id}` | Admin, ManagementStaff | `all` | Detail item doi qua |
| `GET` | `/api/gamification/reward-store/items/active` | Authenticated | `all active` | List item active cho hoc sinh/phu huynh xem |
| `PUT` | `/api/gamification/reward-store/items/{id}` | Admin, ManagementStaff | `all` | Update item doi qua |
| `DELETE` | `/api/gamification/reward-store/items/{id}` | Admin, ManagementStaff | `all` | Xoa mem item doi qua |
| `PATCH` | `/api/gamification/reward-store/items/{id}/toggle-status` | Admin, ManagementStaff | `all` | Toggle active item |
| `POST` | `/api/gamification/reward-redemptions` | Authenticated | `selected student in token` | Hoc sinh request doi qua |
| `GET` | `/api/gamification/reward-redemptions` | Admin, ManagementStaff, Teacher | `all` | List reward redemptions |
| `GET` | `/api/gamification/reward-redemptions/{id}` | Authenticated | `all by id` | Detail redemption |
| `GET` | `/api/gamification/reward-redemptions/me` | Authenticated | `selected student in token` | Redemptions cua student dang select |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/approve` | Admin, ManagementStaff | `all` | Approve redemption |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/cancel` | Admin, ManagementStaff | `all` | Cancel/refund redemption |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/mark-delivered` | Admin, ManagementStaff | `all` | Mark delivered |
| `PATCH` | `/api/gamification/reward-redemptions/{id}/confirm-received` | Authenticated | `own by selected student` | Student confirm received |
| `PATCH` | `/api/gamification/reward-redemptions/batch-deliver` | Admin, ManagementStaff | `all approved filtered by handledAt` | Deliver hang loat |
| `GET` | `/api/gamification/settings` | Admin, ManagementStaff | `all` | Xem check-in rewards |
| `PUT` | `/api/gamification/settings` | Admin, ManagementStaff | `all` | Cap nhat check-in rewards |

## 6. Mission API Detail

### 6.1. `POST /api/missions`

Mo ta: Tao mission va auto tao `MissionProgress` cho target students. Neu scope class thi chi tao progress cho enrollment active va khong nam trong pause enrollment approved overlap voi khoang mission.

Body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `title` | `string` | Yes | Khong duoc rong |
| `description` | `string?` | No | Mo ta |
| `scope` | `MissionScope` | Yes | `Class`, `Student`, `Group` |
| `targetClassId` | `Guid?` | Required khi `scope=Class` | Lop target |
| `targetStudentId` | `Guid?` | Required khi `scope=Student` | Student target |
| `targetGroup` | `Guid[]?` | Required khi `scope=Group` | Danh sach student profile id |
| `missionType` | `MissionType` | Yes | Xem status definition |
| `progressMode` | `MissionProgressMode` | No | Default `Count` |
| `startAt` | `DateTime?` | No | Khong duoc trong qua khu neu co |
| `endAt` | `DateTime?` | No | Phai >= now va > `startAt` neu co |
| `totalRequired` | `int?` | Yes theo reward rule resolver | Phai > 0 va match active `MissionRewardRule` |

Success `201 Created`, `data`:

```json
{
  "id": "guid",
  "title": "string",
  "description": "string",
  "scope": "Class",
  "targetClassId": "guid",
  "targetStudentId": null,
  "targetGroup": null,
  "missionType": "NoUnexcusedAbsence",
  "progressMode": "Count",
  "startAt": "2026-04-13T08:00:00+07:00",
  "endAt": "2026-04-30T23:59:59+07:00",
  "rewardStars": 500,
  "rewardExp": 500,
  "totalRequired": 5,
  "createdBy": "guid",
  "createdAt": "2026-04-13T14:00:00+07:00"
}
```

Error chinh:

- `Mission.InvalidScope`
- `Mission.InvalidDateRange`
- `Mission.ClassNotFound`
- `Mission.StudentNotFound`
- `Mission.SomeStudentsNotFound`
- `Mission.TeacherCannotTargetClass`
- `Mission.TeacherCannotTargetStudent`
- `Mission.TeacherCannotTargetSomeStudents`
- `MissionRewardRule.InvalidTotalRequired`
- `MissionRewardRule.NotConfigured`
- FluentValidation: `Title is required`, `StartAt cannot be in the past`, `EndAt cannot be in the past`, `EndAt must be greater than or equal to StartAt`

### 6.2. `GET /api/missions`

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

Success `200 OK`, `data.missions` la `Page<MissionDto>`:

```json
{
  "missions": {
    "items": [
      {
        "id": "guid",
        "title": "string",
        "description": "string",
        "scope": "Class",
        "targetClassId": "guid",
        "targetClassCode": "KG-01",
        "targetClassTitle": "Class title",
        "targetStudentId": null,
        "targetGroup": null,
        "missionType": "NoUnexcusedAbsence",
        "progressMode": "Count",
        "startAt": null,
        "endAt": null,
        "rewardStars": 100,
        "rewardExp": 100,
        "totalRequired": 1,
        "createdBy": "guid",
        "createdByName": "Teacher name",
        "createdAt": "2026-04-13T14:00:00+07:00"
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1
  }
}
```

### 6.3. `GET /api/missions/{id}`

Route param: `id: Guid`.

Success `200 OK`, `data` fields same `MissionDto` detail.

Error:

- `Mission.NotFound`

### 6.4. `PUT /api/missions/{id}`

Route param: `id: Guid`.

Body: same `POST /api/missions`; `progressMode` nullable, neu null thi giu mode cu.

Success `200 OK`, `data`:

- `id`, `title`, `description`, `scope`, `targetClassId`, `targetStudentId`, `targetGroup`, `missionType`, `progressMode`, `startAt`, `endAt`, `rewardStars`, `rewardExp`, `totalRequired`

Error chinh:

- `Mission.NotFound`
- Cac error giong create mission
- `MissionRewardRule.NotConfigured` neu reward rule khong resolve duoc voi `missionType/progressMode/totalRequired`

### 6.5. `DELETE /api/missions/{id}`

Route param: `id: Guid`.

Success `200 OK`:

```json
{
  "isSuccess": true,
  "data": null
}
```

Error:

- `Mission.NotFound`
- `Mission.MissionInUse`: khong the xoa mission da co progress records

### 6.6. `GET /api/missions/{id}/progress`

Route param: `id: Guid`.

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | null |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success `200 OK`, `data`:

```json
{
  "mission": {
    "id": "guid",
    "title": "string",
    "missionType": "NoUnexcusedAbsence",
    "progressMode": "Count",
    "totalRequired": 5
  },
  "progresses": {
    "items": [
      {
        "id": "guid",
        "missionId": "guid",
        "studentProfileId": "guid",
        "studentName": "Student name",
        "status": "InProgress",
        "progressValue": 1,
        "totalRequired": 5,
        "progressPercentage": 20,
        "completedAt": null,
        "verifiedBy": null,
        "verifiedByName": null
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1
  }
}
```

Error:

- `Mission.NotFound`

### 6.7. `GET /api/missions/me/progress`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `pageNumber` | `int` | No | 1 |
| `pageSize` | `int` | No | 10 |

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "missions": {
    "items": [
      {
        "id": "missionProgressId",
        "missionId": "guid",
        "title": "string",
        "description": "string",
        "missionType": "NoUnexcusedAbsence",
        "progressMode": "Count",
        "status": "InProgress",
        "progressValue": 1,
        "totalRequired": 5,
        "progressPercentage": 20,
        "rewardStars": 500,
        "rewardExp": 500,
        "startAt": null,
        "endAt": "2026-04-30T23:59:59+07:00",
        "createdAt": "2026-04-13T14:00:00+07:00",
        "completedAt": null
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1
  }
}
```

Error:

- `Xp.ProfileNotFound` khi token khong co `StudentId`

## 7. Stars, XP, Level APIs

### 7.1. `POST /api/gamification/stars/add`

Body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Student profile |
| `amount` | `int` | Yes | FE nen enforce > 0; backend hien chua co validator rieng |
| `reason` | `string?` | No | Ly do |

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "amount": 10,
  "newBalance": 120,
  "transactionId": "guid"
}
```

Error:

- `Star.ProfileNotFound`

### 7.2. `POST /api/gamification/stars/deduct`

Body: same add stars.

Success `200 OK`, `data`:

- `studentProfileId`, `amount`, `newBalance`, `transactionId`

Error:

- `Star.ProfileNotFound`
- `Star.InsufficientBalance`

### 7.3. `POST /api/gamification/xp/add`

Body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Student profile |
| `amount` | `int` | Yes | FE nen enforce > 0; backend hien chua co validator rieng |
| `reason` | `string?` | No | Ly do |

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "amount": 10,
  "newXp": 210,
  "newLevel": "Level 3",
  "levelUp": true
}
```

Error:

- `Xp.ProfileNotFound`

### 7.4. `POST /api/gamification/xp/deduct`

Body: same add XP.

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "amount": 10,
  "newXp": 190,
  "newLevel": "Level 2",
  "levelDown": true
}
```

Error:

- `Xp.ProfileNotFound`

### 7.5. `GET /api/gamification/stars/transactions`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | - |
| `page` | `int` | No | 1 |
| `pageSize` | `int` | No | 20 |

Success `200 OK`, `data`:

```json
{
  "transactions": [
    {
      "id": "guid",
      "studentProfileId": "guid",
      "amount": 10,
      "reason": "Manual bonus",
      "sourceType": "Manual",
      "sourceId": null,
      "balanceAfter": 120,
      "createdBy": "guid",
      "createdByName": "Staff name",
      "createdAt": "2026-04-13T14:00:00+07:00"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

### 7.6. `GET /api/gamification/stars/balance`

Query:

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `Guid` | Yes |

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "balance": 120
}
```

### 7.7. `GET /api/gamification/level`

Query:

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `Guid` | Yes |

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "level": "Level 3",
  "xp": 210,
  "xpRequiredForNextLevel": 90
}
```

### 7.8. `GET /api/gamification/stars/balance/me`

Success `200 OK`, `data`:

- `studentProfileId`, `balance`

Error:

- `Star.ProfileNotFound` khi token khong co `StudentId`

### 7.9. `GET /api/gamification/level/me`

Success `200 OK`, `data`:

- `studentProfileId`, `level`, `xp`, `xpRequiredForNextLevel`

Error:

- `Xp.ProfileNotFound` khi token khong co `StudentId`

## 8. Attendance Streak APIs

### 8.1. `GET /api/gamification/attendance-streak`

Query:

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `Guid` | Yes |

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "currentStreak": 3,
  "maxStreak": 5,
  "lastAttendanceDate": "2026-04-13",
  "recentStreaks": [
    {
      "id": "guid",
      "attendanceDate": "2026-04-13",
      "currentStreak": 3,
      "rewardStars": 2,
      "rewardExp": 10,
      "createdAt": "2026-04-13T14:00:00+07:00"
    }
  ]
}
```

### 8.2. `POST /api/gamification/attendance-streak/check-in`

Body: none.

Success `200 OK`, `data`:

```json
{
  "studentProfileId": "guid",
  "attendanceDate": "2026-04-13",
  "currentStreak": 1,
  "maxStreak": 3,
  "rewardStars": 2,
  "rewardExp": 10,
  "isNewStreak": true
}
```

Behavior:

- Neu da check-in ngay hien tai: khong tao record moi, `isNewStreak=false`, van goi tracker de sync mission progress.
- Neu chua check-in: tao `AttendanceStreak`, cong stars/XP theo gamification settings, sync mission progress `NoUnexcusedAbsence`.

Error:

- `Star.ProfileNotFound` khi token khong co `StudentId` hoac profile khong phai student

### 8.3. `GET /api/gamification/attendance-streak/me`

Success `200 OK`, `data` same `GET /attendance-streak`.

Error:

- `Star.ProfileNotFound` khi token khong co `StudentId`

## 9. Mission Reward Rule APIs

Reward rule duoc admin setup truoc de can bang reward khi tao mission. Mission create/update khong nhap reward stars/exp truc tiep; backend resolve reward tu active rule theo `missionType + progressMode + totalRequired`.

Rule resolve hien tai:

- `totalRequired` mission phai > 0.
- Lay active rule cung `missionType` va `progressMode`.
- Rule hop le khi `missionTotalRequired % rule.TotalRequired == 0`.
- Neu co nhieu rule hop le, lay rule co `TotalRequired` lon nhat.
- Reward mission = reward rule * multiplier.

### 9.1. `POST /api/gamification/mission-reward-rules`

Body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `missionType` | `MissionType` | Yes | - |
| `progressMode` | `MissionProgressMode` | No | Default `Count` |
| `totalRequired` | `int` | Yes | > 0 |
| `rewardStars` | `int` | Yes | >= 0 |
| `rewardExp` | `int` | Yes | >= 0 |
| `isActive` | `bool` | No | Default true |

Success `201 Created`, `data`:

```json
{
  "id": "guid",
  "missionType": "NoUnexcusedAbsence",
  "progressMode": "Count",
  "totalRequired": 1,
  "rewardStars": 100,
  "rewardExp": 100,
  "isActive": true,
  "createdAt": "2026-04-13T14:00:00+07:00",
  "updatedAt": "2026-04-13T14:00:00+07:00"
}
```

Error:

- `MissionRewardRule.InvalidTotalRequired`
- `MissionRewardRule.InvalidReward`
- `MissionRewardRule.Duplicate`

### 9.2. `GET /api/gamification/mission-reward-rules`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `missionType` | `MissionType?` | No | null |
| `progressMode` | `MissionProgressMode?` | No | null |
| `isActive` | `bool?` | No | null |
| `page` | `int` | No | 1 |
| `pageSize` | `int` | No | 20 |

Success `200 OK`, `data.rules` la `Page<MissionRewardRuleDto>`.

### 9.3. `GET /api/gamification/mission-reward-rules/{id}`

Success `200 OK`, `data` same rule DTO.

Error:

- `MissionRewardRule.NotFound`

### 9.4. `PUT /api/gamification/mission-reward-rules/{id}`

Body nullable:

- `missionType?: MissionType`
- `progressMode?: MissionProgressMode`
- `totalRequired?: int`
- `rewardStars?: int`
- `rewardExp?: int`
- `isActive?: bool`

Success `200 OK`, `data` same rule DTO.

Error:

- `MissionRewardRule.NotFound`
- `MissionRewardRule.InvalidTotalRequired`
- `MissionRewardRule.InvalidReward`
- `MissionRewardRule.Duplicate`

### 9.5. `PATCH /api/gamification/mission-reward-rules/{id}/toggle-status`

Body: none.

Success `200 OK`, `data`:

```json
{
  "id": "guid",
  "isActive": false
}
```

Error:

- `MissionRewardRule.NotFound`

## 10. Reward Store Item APIs

Ghi chu: entity van con field `Quantity`, nhung create/update API hien khong nhan quantity va logic hien tai khong su dung quantity.

### 10.1. `POST /api/gamification/reward-store/items`

Body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `title` | `string` | Yes | Backend trim |
| `description` | `string?` | No | Backend trim |
| `imageUrl` | `string?` | No | Backend trim |
| `costStars` | `int` | Yes | > 0 |
| `isActive` | `bool` | No | Default true |

Success `201 Created`, `data`:

```json
{
  "id": "guid",
  "title": "string",
  "description": "string",
  "imageUrl": "https://...",
  "costStars": 100,
  "isActive": true,
  "createdAt": "2026-04-13T14:00:00+07:00"
}
```

Error:

- `RewardStore.InvalidCostStars`

### 10.2. `GET /api/gamification/reward-store/items`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `isActive` | `bool?` | No | null |
| `page` | `int` | No | 1 |
| `pageSize` | `int` | No | 20 |

Success `200 OK`, `data.items` la `Page<RewardStoreItemDto>`.

### 10.3. `GET /api/gamification/reward-store/items/{id}`

Success `200 OK`, `data` same item DTO.

Error:

- `RewardStore.NotFound`

### 10.4. `GET /api/gamification/reward-store/items/active`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `page` | `int` | No | 1 |
| `pageSize` | `int` | No | 20 |

Success `200 OK`, `data.items` la `Page<RewardStoreItemDto>` voi `isActive=true`.

### 10.5. `PUT /api/gamification/reward-store/items/{id}`

Body nullable:

- `title?: string`
- `description?: string`
- `imageUrl?: string`
- `costStars?: int`
- `isActive?: bool`

Success `200 OK`, `data` same item DTO.

Error:

- `RewardStore.NotFound`
- `RewardStore.InvalidCostStars`

### 10.6. `DELETE /api/gamification/reward-store/items/{id}`

Success `200 OK`, `data`:

```json
{
  "id": "guid"
}
```

Error:

- `RewardStore.NotFound`

### 10.7. `PATCH /api/gamification/reward-store/items/{id}/toggle-status`

Success `200 OK`, `data`:

```json
{
  "id": "guid",
  "isActive": false
}
```

Error:

- `RewardStore.NotFound`

## 11. Reward Redemption APIs

### 11.1. `POST /api/gamification/reward-redemptions`

Body:

| Field | Type | Required | Default | Ghi chu |
| --- | --- | --- | --- | --- |
| `itemId` | `Guid` | Yes | - | Reward store item |
| `quantity` | `int` | No | 1 | Phai > 0 |

Success `201 Created`, `data`:

```json
{
  "id": "guid",
  "itemId": "guid",
  "itemName": "Reward title",
  "quantity": 1,
  "studentProfileId": "guid",
  "status": "Requested",
  "starsDeducted": 100,
  "remainingStars": 20,
  "createdAt": "2026-04-13T14:00:00+07:00"
}
```

Behavior:

- Lay `StudentId` tu token.
- Item phai ton tai, chua deleted, dang active.
- Tru stars ngay khi request thanh cong.
- Luu snapshot `itemName` tai thoi diem request.

Error:

- `RewardRedemption.StudentProfileNotFound`
- `RewardRedemption.ItemNotFound`
- `RewardRedemption.ItemNotActive`
- `RewardRedemption.InvalidQuantity`
- `RewardRedemption.InsufficientStars`
- `Star.ProfileNotFound` hoac `Star.InsufficientBalance` neu buoc deduct stars fail

### 11.2. `GET /api/gamification/reward-redemptions`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | null |
| `itemId` | `Guid?` | No | null |
| `status` | `string?` | No | null |
| `page` | `int` | No | 1 |
| `pageSize` | `int` | No | 20 |

Success `200 OK`, `data.redemptions` la `Page<RewardRedemptionDto>`:

```json
{
  "redemptions": {
    "items": [
      {
        "id": "guid",
        "itemId": "guid",
        "itemName": "Reward title",
        "quantity": 1,
        "starsDeducted": 100,
        "studentProfileId": "guid",
        "studentName": "Student name",
        "branchName": "Branch name",
        "status": "Requested",
        "cancelReason": null,
        "handledBy": null,
        "handledByName": null,
        "handledAt": null,
        "deliveredAt": null,
        "receivedAt": null,
        "createdAt": "2026-04-13T14:00:00+07:00"
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1
  }
}
```

Ghi chu: Neu `status` string khong parse duoc enum thi backend bo qua filter status.

### 11.3. `GET /api/gamification/reward-redemptions/{id}`

Success `200 OK`, `data` same `RewardRedemptionDto` detail.

Error:

- `RewardRedemption.NotFound`

### 11.4. `GET /api/gamification/reward-redemptions/me`

Query:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `status` | `string?` | No | null |
| `page` | `int` | No | 1 |
| `pageSize` | `int` | No | 20 |

Success `200 OK`, `data.redemptions` la `Page<RewardRedemptionDto>` cua selected student.

Error:

- `RewardRedemption.NotFound` voi `Guid.Empty` khi token khong co `StudentId`

### 11.5. `PATCH /api/gamification/reward-redemptions/{id}/approve`

Body: none.

Success `200 OK`, `data`:

```json
{
  "id": "guid",
  "status": "Approved",
  "handledBy": "guid",
  "handledAt": "2026-04-13T14:00:00+07:00"
}
```

Error:

- `RewardRedemption.NotFound`
- `RewardRedemption.InvalidStatusTransition`: chi cho `Requested -> Approved`

### 11.6. `PATCH /api/gamification/reward-redemptions/{id}/cancel`

Body:

| Field | Type | Required |
| --- | --- | --- |
| `reason` | `string?` | No |

Success `200 OK`, `data`:

```json
{
  "id": "guid",
  "status": "Cancelled",
  "cancelReason": "string",
  "handledBy": "guid",
  "handledAt": "2026-04-13T14:00:00+07:00"
}
```

Behavior: Chi cancel tu `Requested` hoac `Approved`; refund stars ve student.

Error:

- `RewardRedemption.NotFound`
- `RewardRedemption.InvalidStatusTransition`
- `RewardRedemption.ItemNotFound`
- `Star.ProfileNotFound`

### 11.7. `PATCH /api/gamification/reward-redemptions/{id}/mark-delivered`

Body: none.

Success `200 OK`, `data`:

```json
{
  "id": "guid",
  "status": "Delivered",
  "deliveredAt": "2026-04-13T14:00:00+07:00"
}
```

Error:

- `RewardRedemption.NotFound`
- `RewardRedemption.InvalidStatusTransition`: chi cho `Approved -> Delivered`

### 11.8. `PATCH /api/gamification/reward-redemptions/{id}/confirm-received`

Body: none.

Success `200 OK`, `data`:

```json
{
  "id": "guid",
  "status": "Received",
  "receivedAt": "2026-04-13T14:00:00+07:00"
}
```

Behavior:

- Token phai co `StudentId`.
- Redemption phai thuoc selected student trong token.
- Chi cho `Delivered -> Received`.

Error:

- `RewardRedemption.StudentProfileNotFound`
- `RewardRedemption.NotFound`
- `RewardRedemption.InvalidStatusTransition`

### 11.9. `PATCH /api/gamification/reward-redemptions/batch-deliver`

Query:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `year` | `int?` | No | Neu co thi 2000..2100 |
| `month` | `int?` | No | 1..12; neu co `month` thi phai co `year` |

Success `200 OK`, `data`:

```json
{
  "deliveredCount": 2,
  "deliveredRedemptionIds": ["guid-1", "guid-2"],
  "deliveredAt": "2026-04-13T14:00:00+07:00"
}
```

Behavior:

- Chi deliver cac redemption dang `Approved`.
- Neu co `year/month`, filter theo `HandledAt` trong thang do.
- Neu khong co `month`, co `year`, filter ca nam.
- Neu khong co filter, deliver tat ca `Approved`.

Error:

- `Month`: `Month must be between 1 and 12`
- `Year`: `Year must be between 2000 and 2100`
- `Year`: `Year is required when Month is specified`

## 12. Gamification Settings APIs

### 12.1. `GET /api/gamification/settings`

Success `200 OK`, `data`:

```json
{
  "checkInRewardStars": 1,
  "checkInRewardExp": 5
}
```

### 12.2. `PUT /api/gamification/settings`

Body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `checkInRewardStars` | `int` | Yes | FE nen enforce >= 0; backend hien chua co validator rieng |
| `checkInRewardExp` | `int` | Yes | FE nen enforce >= 0; backend hien chua co validator rieng |

Success `200 OK`, `data`:

```json
{
  "checkInRewardStars": 2,
  "checkInRewardExp": 10
}
```

## 13. Validation Rule Tong Hop

### 13.1. Mission

- `title` bat buoc khi create/update.
- `scope=Class` bat buoc `targetClassId`.
- `scope=Student` bat buoc `targetStudentId`.
- `scope=Group` bat buoc `targetGroup` co item.
- `endAt` phai sau `startAt` trong handler.
- Validator yeu cau `startAt >= now` neu co.
- Validator yeu cau `endAt >= now` neu co.
- Khi tao/sua mission, `totalRequired` phai > 0 va resolve duoc active `MissionRewardRule`.
- Teacher chi target duoc class minh la main/assistant teacher, hoac student trong class active minh day.
- Class scope auto tao progress cho enrollment `Active`, loai hoc sinh dang pause enrollment approved overlap mission date range.

### 13.2. Mission Reward Rule

- `totalRequired > 0`.
- `rewardStars >= 0`.
- `rewardExp >= 0`.
- `rewardStars` va `rewardExp` khong duoc dong thoi bang 0.
- Khong duoc duplicate theo `missionType + progressMode + totalRequired`.

### 13.3. Stars / XP

- Student profile phai ton tai va `ProfileType=Student` khi add/deduct stars/xp.
- Deduct stars yeu cau balance hien tai >= amount.
- XP deduct khong de XP am, backend dung `Math.Max(0, oldXp - amount)`.
- Backend hien chua co validator amount > 0 cho add/deduct stars/xp, FE nen enforce > 0.

### 13.4. Attendance Check-In

- Token phai co `StudentId`.
- Profile phai ton tai va `ProfileType=Student`.
- Moi ngay chi tao mot `AttendanceStreak`; goi lai trong cung ngay tra record cu va `isNewStreak=false`.
- Check-in se sync mission type `NoUnexcusedAbsence`.

### 13.5. Reward Store

- `costStars > 0` khi create/update neu co.
- Delete la xoa mem `IsDeleted=true`.
- List/detail khong tra item da deleted.
- `quantity` khong nam trong request/response API hien tai.

### 13.6. Reward Redemption

- Token phai co `StudentId` khi request reward va confirm received.
- Item phai ton tai, chua deleted, dang active.
- `quantity > 0`.
- Balance stars phai du de request.
- Request reward tru stars ngay va tao redemption `Requested`.
- Cancel tu `Requested` hoac `Approved` se refund stars.
- Approve chi tu `Requested`.
- Mark delivered chi tu `Approved`.
- Confirm received chi tu `Delivered` va redemption phai thuoc selected student.
- Batch deliver chi update cac redemption dang `Approved`.

## 14. Error Code Tong Hop

| Code | HTTP | Khi nao |
| --- | --- | --- |
| `Mission.NotFound` | 404 | Khong tim thay mission |
| `Mission.ClassNotFound` | 404 | Target class khong ton tai |
| `Mission.StudentNotFound` | 404 | Target student khong ton tai |
| `Mission.SomeStudentsNotFound` | 404 | Mot so student trong group khong ton tai |
| `Mission.InvalidScope` | 400 | Scope khong du target field |
| `Mission.InvalidDateRange` | 400 | `endAt <= startAt` |
| `Mission.TeacherCannotTargetClass` | 400 | Teacher target class khong day |
| `Mission.TeacherCannotTargetStudent` | 400 | Teacher target student ngoai lop dang day |
| `Mission.TeacherCannotTargetSomeStudents` | 400 | Teacher target mot so student ngoai lop dang day |
| `Mission.MissionInUse` | 409 | Xoa mission da co progress |
| `MissionRewardRule.NotFound` | 404 | Khong tim thay reward rule |
| `MissionRewardRule.Duplicate` | 409 | Duplicate `missionType/progressMode/totalRequired` |
| `MissionRewardRule.NotConfigured` | 400 | Tao/sua mission khong resolve duoc active reward rule |
| `MissionRewardRule.InvalidTotalRequired` | 400 | `totalRequired <= 0` |
| `MissionRewardRule.InvalidReward` | 400 | Reward am hoac stars/exp deu bang 0 |
| `Star.ProfileNotFound` | 404 | Khong co selected student/profile student |
| `Star.InsufficientBalance` | 400 | Khong du stars de tru |
| `Xp.ProfileNotFound` | 404 | Khong co selected student/profile student |
| `RewardStore.NotFound` | 404 | Khong tim thay item hoac item da deleted |
| `RewardStore.InvalidCostStars` | 400 | `costStars <= 0` |
| `RewardRedemption.NotFound` | 404 | Khong tim thay redemption hoac khong duoc expose |
| `RewardRedemption.ItemNotFound` | 404 | Khong tim thay item |
| `RewardRedemption.ItemNotActive` | 400 | Item dang inactive |
| `RewardRedemption.InvalidQuantity` | 400 | `quantity <= 0` |
| `RewardRedemption.InsufficientStars` | 409 | Khong du stars de request reward |
| `RewardRedemption.InvalidStatusTransition` | 400 | Chuyen status sai luong |
| `RewardRedemption.StudentProfileNotFound` | 404 | Token khong co `StudentId` hoac profile khong ton tai |
| `Month` | 400 | Month ngoai 1..12 trong batch deliver |
| `Year` | 400 | Year ngoai 2000..2100 hoac thieu year khi co month |

## 15. FE Notes

- DateTime nen gui ISO 8601 co offset, vi API co converter normalize UTC, vi du `2026-04-30T23:59:59+07:00`.
- Khi tao mission ngay hien tai, can check `endAt` dung ngay/gio mong muon. Neu `endAt` da qua, mission progress tracker se khong tinh tiep.
- FE khong gui `rewardStars` / `rewardExp` khi create/update mission; backend resolve tu mission reward rule.
- FE nen tao reward rule truoc khi cho giao vien tao mission. Vi du rule `NoUnexcusedAbsence + Count + totalRequired=1 + 100 stars + 100 exp`, khi tao mission `totalRequired=5` backend co the resolve thanh `500 stars + 500 exp`.
- FE nen enforce cac validation ma backend chua enforce manh: amount stars/xp > 0, settings reward >= 0, page/pageSize hop ly.
- API reward item khong dung quantity trong request/response hien tai; quantity chi con o entity de co the mo lai sau.
