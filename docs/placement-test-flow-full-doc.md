# Tài liệu Placement Test full flow theo implementation hiện tại

## 1. Phạm vi

Tài liệu này mô tả **as-is implementation trong code** cho toàn bộ flow Placement Test, gồm:

- đặt lịch
- xem danh sách / chi tiết
- cập nhật lịch / phòng / giám thị / student / class
- hủy test
- đánh dấu no-show
- nhập điểm và recommendation
- thêm note
- convert lead sang enrolled

Nguồn chính:

- `Kidzgo.API/Controllers/PlacementTestController.cs`
- `Kidzgo.Application/PlacementTests/*`
- `Kidzgo.Domain/CRM/PlacementTest.cs`
- `Kidzgo.Domain/CRM/PlacementTestStatus.cs`
- `Kidzgo.Domain/CRM/Errors/PlacementTestErrors.cs`
- `Kidzgo.Domain/CRM/LeadStatus.cs`
- `Kidzgo.Domain/CRM/LeadChildStatus.cs`
- `Kidzgo.Infrastructure/Configuration/PlacementTestConfiguration.cs`

## 2. Business rule tổng quát

### 2.1. Role và data scope hiện tại

- `Admin`, `ManagementStaff`: full CRUD flow Placement Test
- `AccountantStaff`: chỉ được xem list/detail
- `Teacher`, `Parent`, `Student`: không có quyền ở PlacementTestController
- Data scope hiện tại cho role được phép là `all`
- Không có `own` hoặc `department` scope trong handler Placement Test

### 2.2. Entity chính

`PlacementTest` có các nhóm dữ liệu:

- liên kết lead:
  - `LeadId`
  - `LeadChildId`
- liên kết vận hành:
  - `StudentProfileId`
  - `ClassId`
  - `ScheduledAt`
  - `Room`
  - `InvigilatorUserId`
- kết quả:
  - `ListeningScore`
  - `SpeakingScore`
  - `ReadingScore`
  - `WritingScore`
  - `ResultScore`
  - `LevelRecommendation`
  - `ProgramRecommendation`
  - `AttachmentUrl`
- ghi chú:
  - `Notes`

### 2.3. Rule khi đặt lịch

- Có thể truyền:
  - `LeadChildId`
  - hoặc chỉ `LeadId`
- Nếu có `LeadChildId`:
  - hệ thống ưu tiên `LeadChildId`
  - load `Lead` thông qua `LeadChild`
  - nếu request có `LeadId` mà không khớp `LeadChild.LeadId` thì trả lỗi validation
- Nếu chỉ có `LeadId`:
  - hệ thống lấy `Lead`
  - nếu lead chưa có `LeadChild` nào thì tự tạo **default LeadChild** để tương thích ngược
- `ScheduledAt` không được ở quá khứ
- Nếu có `StudentProfileId`:
  - phải tồn tại
  - phải là `ProfileType.Student`
- Nếu có `ClassId`:
  - class phải tồn tại
- Nếu có `InvigilatorUserId`:
  - user phải tồn tại
  - code hiện tại **không check role của invigilator**
- Khi tạo mới:
  - `PlacementTest.Status = Scheduled`
  - `LeadChild.Status = BookedTest` nếu chưa là `BookedTest`
  - `Lead.Status = BookedTest` nếu chưa là `BookedTest`

### 2.4. Rule khi cập nhật Placement Test

- Không được update nếu `Status = Completed`
- Có thể cập nhật:
  - `ScheduledAt`
  - `Room`
  - `InvigilatorUserId`
  - `StudentProfileId`
  - `ClassId`
- Nếu field optional không truyền thì giữ nguyên
- Nếu `Room` truyền rỗng/whitespace thì set `null`

### 2.5. Rule khi nhập kết quả

- Có thể cập nhật từng phần các score và recommendation
- Placement Test sẽ tự chuyển `Completed` khi đủ cả 5 trường:
  - `ListeningScore`
  - `SpeakingScore`
  - `ReadingScore`
  - `WritingScore`
  - `ResultScore`
- `LevelRecommendation`, `ProgramRecommendation`, `AttachmentUrl` không bắt buộc để complete
- Khi auto-complete:
  - nếu có `LeadChild` và `LeadChild.Status = BookedTest`:
    - chuyển `LeadChild.Status = TestDone`
    - tạo `LeadActivity`
  - nếu `Lead.Status = BookedTest`:
    - chuyển `Lead.Status = TestDone`
    - tạo `LeadActivity`

### 2.6. Rule khi cancel / no-show

- `Cancel` chỉ chặn khi test đã `Completed`
- `NoShow` chỉ chặn khi test đã `Completed`
- Vì vậy implementation hiện tại cho phép:
  - `Scheduled -> Cancelled`
  - `Scheduled -> NoShow`
  - `Cancelled -> NoShow`
  - `NoShow -> Cancelled`
  - `Cancelled -> Cancelled`
  - `NoShow -> NoShow`
- Nếu cancel có `reason`:
  - append vào `Notes` với prefix `Cancelled: `

### 2.7. Rule khi thêm note

- Note mới được `Trim()`
- Nếu đã có note cũ, hệ thống append thêm bằng newline

### 2.8. Rule khi convert lead sang enrolled

- Nếu `PlacementTest` không tồn tại: lỗi
- Nếu `Lead` không còn tồn tại: lỗi
- Với legacy flow không có `LeadChild`:
  - nếu `Lead.Status = Enrolled` thì chặn `LeadAlreadyEnrolled`
- Nếu request có `StudentProfileId`:
  - profile phải tồn tại và là student
  - nếu profile đã gán cho `LeadChild` khác thì lỗi conflict
  - profile sẽ được link vào `PlacementTest.StudentProfileId`
- Với child-based flow:
  - nếu child đã `Enrolled` thì lỗi validation
  - nếu request có `StudentProfileId` thì set `LeadChild.ConvertedStudentProfileId`
  - set `LeadChild.Status = Enrolled`
  - nếu có ít nhất một child enrolled thì `Lead.Status = Enrolled`
  - tạo `LeadActivity`
- Với legacy flow:
  - set trực tiếp `Lead.Status = Enrolled`
  - tạo `LeadActivity`
- Nếu `PlacementTest.Status != Completed` thì handler sẽ tự set `Completed`

## 3. Status definition

### 3.1. `PlacementTestStatus`

| Status | Ý nghĩa |
|---|---|
| `Scheduled` | Đã đặt lịch |
| `NoShow` | Không tham dự |
| `Completed` | Đã có kết quả hoàn chỉnh hoặc đã convert |
| `Cancelled` | Đã hủy |

### 3.2. Status flow thực tế trong code

| Từ | Sang | Trigger |
|---|---|---|
| `null` | `Scheduled` | schedule |
| `Scheduled` | `Cancelled` | cancel |
| `Scheduled` | `NoShow` | no-show |
| `Scheduled` | `Completed` | update results đủ điểm hoặc convert |
| `Cancelled` | `NoShow` | no-show |
| `Cancelled` | `Completed` | update results đủ điểm hoặc convert |
| `NoShow` | `Cancelled` | cancel |
| `NoShow` | `Completed` | update results đủ điểm hoặc convert |

Ghi chú:

- Đây là **luồng thực tế theo code hiện tại**, không phải state machine bị khóa chặt.

### 3.3. Status liên quan Lead / LeadChild

| Thời điểm | LeadChild | Lead |
|---|---|---|
| Schedule | `BookedTest` | `BookedTest` |
| Update results đủ điểm | `TestDone` nếu đang `BookedTest` | `TestDone` nếu đang `BookedTest` |
| Convert enrolled | `Enrolled` | `Enrolled` |

## 4. Response format

### 4.1. Success

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 4.2. Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": []
}
```

### 4.3. Mapping status code

| HTTP code | Ý nghĩa |
|---|---|
| `400` | validation |
| `401` | thiếu token / token sai |
| `403` | sai role |
| `404` | không tìm thấy entity liên quan |
| `409` | conflict dữ liệu |
| `500` | lỗi hệ thống |

## 5. Permission matrix

| Chức năng | Admin | ManagementStaff | AccountantStaff | Teacher | Parent | Student |
|---|---|---|---|---|---|---|
| Schedule placement test | Yes | Yes | No | No | No | No |
| Xem danh sách placement tests | Yes | Yes | Yes | No | No | No |
| Xem chi tiết placement test | Yes | Yes | Yes | No | No | No |
| Cập nhật placement test | Yes | Yes | No | No | No | No |
| Hủy placement test | Yes | Yes | No | No | No | No |
| Đánh dấu no-show | Yes | Yes | No | No | No | No |
| Nhập kết quả | Yes | Yes | No | No | No | No |
| Thêm note | Yes | Yes | No | No | No | No |
| Convert lead to enrolled | Yes | Yes | No | No | No | No |

## 6. Validation rule và lỗi tiêu biểu

### 6.1. Schedule

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| `scheduledAt` bắt buộc | validator + request | `400 Validation.General` |
| `scheduledAt` không ở quá khứ | validator | `400 Validation.General` |
| phải có `leadId` hoặc `leadChildId` | handler | `400 LeadId` |
| `leadChildId` phải tồn tại nếu truyền | handler | `404 LeadChild` |
| nếu có cả `leadId` và `leadChildId` thì phải khớp nhau | handler | `400 LeadId` |
| `leadId` phải tồn tại nếu dùng legacy flow | handler | `404 PlacementTest.LeadNotFound` |
| `studentProfileId` phải là student hợp lệ | handler | `404 PlacementTest.StudentProfileNotFound` |
| `classId` phải tồn tại | handler | `404 PlacementTest.ClassNotFound` |
| `invigilatorUserId` phải tồn tại | handler | `404 PlacementTest.InvigilatorNotFound` |

### 6.2. Update

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| `placementTestId` bắt buộc | validator | `400 Validation.General` |
| `scheduledAt` không ở quá khứ nếu truyền | validator | `400 Validation.General` |
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| không update test đã completed | handler | `400 PlacementTest.CannotUpdateCompletedTest` |
| `studentProfileId` phải là student hợp lệ nếu truyền | handler | `404 PlacementTest.StudentProfileNotFound` |
| `classId` phải tồn tại nếu truyền | handler | `404 PlacementTest.ClassNotFound` |
| `invigilatorUserId` phải tồn tại nếu truyền | handler | `404 PlacementTest.InvigilatorNotFound` |

### 6.3. Cancel / No-show

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| completed test không được cancel | handler | `400 PlacementTest.CannotCancelCompletedTest` |
| completed test không được no-show | handler | `400 PlacementTest.CannotMarkNoShowCompletedTest` |

### 6.4. Update results

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| không có validator range cho score | hiện chưa có | chưa có error app-side |

### 6.5. Add note

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| `note` bắt buộc | request annotation | thường `400` model validation |
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |

### 6.6. Convert to enrolled

| Rule | Nơi kiểm tra | Lỗi |
|---|---|---|
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| lead phải tồn tại | handler | `404 PlacementTest.LeadNotFound` |
| legacy lead đã enrolled thì không convert lại | handler | `409 PlacementTest.LeadAlreadyEnrolled` |
| `studentProfileId` phải là student hợp lệ nếu truyền | handler | `404 PlacementTest.StudentProfileNotFound` |
| student profile không được gán cho child khác | handler | `409 PlacementTest.StudentProfileAlreadyAssigned` |
| child đã enrolled thì không convert lại | handler | `400 LeadChild` |

## 7. Danh sách API

### `POST /api/placement-tests`

- Role: `Admin`, `ManagementStaff`
- Mục đích: đặt lịch placement test
- Body:

| Field | Type | Required |
|---|---|---|
| `leadId` | guid | Conditional |
| `leadChildId` | guid | Conditional |
| `studentProfileId` | guid | No |
| `classId` | guid | No |
| `scheduledAt` | datetime | Yes |
| `room` | string | No |
| `invigilatorUserId` | guid | No |

- Success data:
  - `id`, `leadId`, `leadChildId`, `studentProfileId`, `classId`, `scheduledAt`, `status`, `room`, `invigilatorUserId`, `createdAt`
- Error:
  - `400 Validation.General`
  - `400 LeadId`
  - `404 LeadChild`
  - `404 PlacementTest.LeadNotFound`
  - `404 PlacementTest.StudentProfileNotFound`
  - `404 PlacementTest.ClassNotFound`
  - `404 PlacementTest.InvigilatorNotFound`

### `GET /api/placement-tests`

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Mục đích: xem danh sách placement test
- Query:

| Param | Type | Required |
|---|---|---|
| `leadId` | guid | No |
| `studentProfileId` | guid | No |
| `status` | enum(`Scheduled`,`NoShow`,`Completed`,`Cancelled`) | No |
| `fromDate` | datetime | No |
| `toDate` | datetime | No |
| `page` | int, default `1` | No |
| `pageSize` | int, default `20` | No |

- Success data:
  - `placementTests[]` gồm `id`, `leadId`, `leadChildId`, `leadContactName`, `childName`, `studentProfileId`, `studentName`, `classId`, `className`, `scheduledAt`, `status`, `room`, `invigilatorUserId`, `invigilatorName`, `resultScore`, `listeningScore`, `speakingScore`, `readingScore`, `writingScore`, `levelRecommendation`, `programRecommendation`, `notes`, `attachmentUrl`, `createdAt`, `updatedAt`
  - `totalCount`, `page`, `pageSize`, `totalPages`

### `GET /api/placement-tests/{id}`

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Mục đích: xem chi tiết placement test
- Path: `id: guid`
- Success data:
  - cùng field với một item trong list
- Error:
  - `404 PlacementTest.NotFound`

### `PUT /api/placement-tests/{id}`

- Role: `Admin`, `ManagementStaff`
- Mục đích: cập nhật lịch / phòng / giám thị / student / class
- Path: `id: guid`
- Body:

| Field | Type | Required |
|---|---|---|
| `scheduledAt` | datetime | No |
| `room` | string | No |
| `invigilatorUserId` | guid | No |
| `studentProfileId` | guid | No |
| `classId` | guid | No |

- Success data:
  - `id`, `leadId`, `studentProfileId`, `classId`, `scheduledAt`, `status`, `room`, `invigilatorUserId`, `updatedAt`
- Error:
  - `400 Validation.General`
  - `404 PlacementTest.NotFound`
  - `400 PlacementTest.CannotUpdateCompletedTest`
  - `404 PlacementTest.StudentProfileNotFound`
  - `404 PlacementTest.ClassNotFound`
  - `404 PlacementTest.InvigilatorNotFound`

### `POST /api/placement-tests/{id}/cancel`

- Role: `Admin`, `ManagementStaff`
- Mục đích: hủy placement test
- Path: `id: guid`
- Body: `reason?: string`
- Success data:
  - `id`, `status`, `updatedAt`
- Side effect:
  - nếu có reason thì append vào `notes` với prefix `Cancelled: `
- Error:
  - `404 PlacementTest.NotFound`
  - `400 PlacementTest.CannotCancelCompletedTest`

### `POST /api/placement-tests/{id}/no-show`

- Role: `Admin`, `ManagementStaff`
- Mục đích: đánh dấu không tham dự
- Path: `id: guid`
- Body: none
- Success data:
  - `id`, `status`, `updatedAt`
- Error:
  - `404 PlacementTest.NotFound`
  - `400 PlacementTest.CannotMarkNoShowCompletedTest`

### `PUT /api/placement-tests/{id}/results`

- Role: `Admin`, `ManagementStaff`
- Mục đích: nhập điểm và recommendation
- Path: `id: guid`
- Body:

| Field | Type | Required |
|---|---|---|
| `listeningScore` | decimal | No |
| `speakingScore` | decimal | No |
| `readingScore` | decimal | No |
| `writingScore` | decimal | No |
| `resultScore` | decimal | No |
| `levelRecommendation` | string | No |
| `programRecommendation` | string | No |
| `attachmentUrl` | string | No |

- Success data:
  - `id`, `listeningScore`, `speakingScore`, `readingScore`, `writingScore`, `resultScore`, `levelRecommendation`, `programRecommendation`, `attachmentUrl`, `status`, `updatedAt`
- Business effect:
  - nếu đủ 5 score thì `status` tự thành `Completed`
  - có thể update `LeadChild.Status` và `Lead.Status` sang `TestDone`
- Error:
  - `404 PlacementTest.NotFound`

### `POST /api/placement-tests/{id}/notes`

- Role: `Admin`, `ManagementStaff`
- Mục đích: thêm note
- Path: `id: guid`
- Body:

| Field | Type | Required |
|---|---|---|
| `note` | string | Yes |

- Success data:
  - `id`, `notes`, `updatedAt`
- Error:
  - `400` nếu request thiếu `note`
  - `404 PlacementTest.NotFound`

### `POST /api/placement-tests/{id}/convert-to-enrolled`

- Role: `Admin`, `ManagementStaff`
- Mục đích: convert lead/child sang enrolled sau placement test
- Path: `id: guid`
- Body:

| Field | Type | Required |
|---|---|---|
| `studentProfileId` | guid | No |

- Success data:
  - `leadId`, `leadStatus`, `placementTestId`, `placementTestStatus`, `studentProfileId`, `leadChildId`
- Business effect:
  - có thể tự set `PlacementTest.Status = Completed`
  - có thể set `LeadChild.Status = Enrolled`
  - có thể set `Lead.Status = Enrolled`
  - tạo `LeadActivity`
- Error:
  - `404 PlacementTest.NotFound`
  - `404 PlacementTest.LeadNotFound`
  - `409 PlacementTest.LeadAlreadyEnrolled`
  - `404 PlacementTest.StudentProfileNotFound`
  - `409 PlacementTest.StudentProfileAlreadyAssigned`
  - `400 LeadChild`

## 8. Các field quan trọng và ràng buộc DB

| Field | Kiểu | Ràng buộc đáng chú ý |
|---|---|---|
| `Status` | enum string | max length `20` |
| `Room` | string | max length `100` |
| `LevelRecommendation` | string | max length `100` |
| `ProgramRecommendation` | string | max length `100` |
| `LeadChild.ChildName` | string | required, max length `255` |

## 9. Các trường hợp trả lỗi quan trọng

- không truyền `leadId` và `leadChildId` khi schedule
- truyền cả `leadId` và `leadChildId` nhưng không khớp nhau
- schedule/update với `scheduledAt` ở quá khứ
- student profile không tồn tại hoặc không phải student
- class không tồn tại
- invigilator user không tồn tại
- update/cancel/no-show đối với test đã `Completed`
- convert với student profile đã gán cho child khác
- convert child đã `Enrolled`

## 10. Kết luận implementation hiện tại

- Placement Test đang là flow nội bộ cho `Admin`, `ManagementStaff`, và read-only cho `AccountantStaff`
- Scope dữ liệu hiện tại là `all`
- Không có state machine chặt cho `Scheduled / NoShow / Cancelled / Completed`
- Convert-to-enrolled có quyền đẩy `PlacementTest` sang `Completed` kể cả khi chưa qua bước nhập đủ điểm
