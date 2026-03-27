# Tài liệu Placement Test full flow theo code hiện tại

Ngày cập nhật: 2026-03-27

## 1. Phạm vi

Tài liệu này mô tả flow Placement Test theo implementation hiện tại trong code, gồm:

- đặt lịch placement test
- xem danh sách / chi tiết
- cập nhật lịch test
- hủy test
- đánh dấu no-show
- nhập kết quả
- thêm note
- convert lead sang enrolled
- tạo placement test retake
- auto tạo registration mới khi retake hoàn tất

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

### 2.1. Role và data scope

- `Admin`, `ManagementStaff`: full flow Placement Test
- `AccountantStaff`: chỉ có quyền list/detail
- `Teacher`, `Parent`, `Student`: không có endpoint Placement Test trong controller này
- Scope dữ liệu hiện tại là `all`
- Không có `own` / `department` scope ở các handler Placement Test

### 2.2. Entity chính

`PlacementTest` hiện có các nhóm field chính:

- liên kết CRM:
  - `LeadId`
  - `LeadChildId`
- liên kết vận hành:
  - `StudentProfileId`
  - `ClassId`
  - `ScheduledAt`
  - `Room`
  - `InvigilatorUserId`
- liên kết retake:
  - `OriginalPlacementTestId`
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

Ghi chú:

- `LevelRecommendation` có trong entity và response list/detail, nhưng hiện không có API public nào cập nhật field này.
- `OriginalPlacementTestId` chỉ được set ở flow retake.

### 2.3. Rule khi đặt lịch

- Request hiện chỉ nhận:
  - `LeadId`
  - `LeadChildId`
  - `ScheduledAt`
  - `Room`
  - `InvigilatorUserId`
- Không còn nhận `StudentProfileId` hoặc `ClassId` ở API schedule.
- Nếu có `LeadChildId`:
  - hệ thống ưu tiên `LeadChildId`
  - load `Lead` qua `LeadChild`
  - nếu request có thêm `LeadId` nhưng không khớp `LeadChild.LeadId` thì trả lỗi validation
- Nếu chỉ có `LeadId`:
  - hệ thống load `Lead`
  - nếu lead chưa có child nào thì tự tạo default `LeadChild` để giữ backward compatibility
- `ScheduledAt` là bắt buộc và bị validator chặn nếu ở quá khứ
- Nếu có `InvigilatorUserId`:
  - user phải tồn tại
  - code hiện tại không kiểm tra role của invigilator
- Khi tạo mới:
  - `PlacementTest.Status = Scheduled`
  - `LeadChild.Status = BookedTest` nếu chưa là `BookedTest`
  - `Lead.Status = BookedTest` nếu chưa là `BookedTest`
- Response schedule có field `StudentProfileId` và `ClassId`, nhưng handler không set nên thực tế đang trả `null`

### 2.4. Rule khi update Placement Test

- Có thể update:
  - `ScheduledAt`
  - `Room`
  - `InvigilatorUserId`
  - `StudentProfileId`
  - `ClassId`
- Không được update nếu `Status = Completed`
- Nếu truyền `StudentProfileId`:
  - profile phải tồn tại
  - profile phải có `ProfileType.Student`
- Nếu truyền `ClassId`:
  - class phải tồn tại
- Nếu truyền `InvigilatorUserId`:
  - user phải tồn tại
- Nếu truyền `Room = ""` hoặc whitespace thì hệ thống set `null`
- `ScheduledAt` nếu có truyền sẽ được convert về UTC

### 2.5. Rule khi nhập kết quả

- API hiện cho cập nhật:
  - `ListeningScore`
  - `SpeakingScore`
  - `ReadingScore`
  - `WritingScore`
  - `ResultScore`
  - `ProgramRecommendation`
  - `AttachmentUrl`
- API hiện không nhận `LevelRecommendation`
- Không có validator range cho score ở application layer
- Placement Test sẽ tự chuyển `Completed` khi đủ cả 5 score:
  - `ListeningScore`
  - `SpeakingScore`
  - `ReadingScore`
  - `WritingScore`
  - `ResultScore`
- Khi auto complete:
  - nếu có `LeadChild` và child đang `BookedTest` thì chuyển `LeadChild.Status = TestDone` và tạo `LeadActivity`
  - nếu `Lead` đang `BookedTest` thì chuyển `Lead.Status = TestDone`
  - nếu test này là retake (`OriginalPlacementTestId != null`) thì handler sẽ thử auto tạo registration mới

### 2.6. Rule auto tạo registration sau retake

Logic này nằm trong `UpdatePlacementTestResultsCommandHandler`, chỉ chạy khi:

- placement test là retake (`OriginalPlacementTestId != null`)
- placement test đã có đủ 5 score để complete
- `StudentProfileId` khác `null`
- `ProgramRecommendation` khác `null` hoặc whitespace

Handler sẽ:

- tìm active registration hiện tại của student
- tìm `Program` theo `ProgramRecommendation` bằng `Program.Name`
- tìm tuition plan active rẻ nhất của program đích
- set registration cũ sang `Completed`
- tạo registration mới:
  - `Status = WaitingForClass`
  - `EntryType = Retake`
  - `OperationType = Retake`
  - `OriginalRegistrationId = registration cũ`
  - `TotalSessions = RemainingSessions cũ`
  - `UsedSessions = 0`
  - `RemainingSessions = RemainingSessions cũ`
- tạo `LeadActivity`

Nếu thiếu bất kỳ điều kiện nào ở trên thì Placement Test vẫn complete bình thường, nhưng `NewRegistrationId` trong response sẽ là `null`.

### 2.7. Rule khi cancel / no-show

- `Cancel` chỉ chặn khi test đã `Completed`
- `NoShow` chỉ chặn khi test đã `Completed`
- Vì vậy flow trạng thái hiện tại vẫn khá mở:
  - `Scheduled -> Cancelled`
  - `Scheduled -> NoShow`
  - `Cancelled -> NoShow`
  - `NoShow -> Cancelled`
  - `Cancelled -> Cancelled`
  - `NoShow -> NoShow`
- Nếu cancel có `reason`:
  - append vào `Notes` với prefix `Cancelled: `

### 2.8. Rule khi thêm note

- Note được `Trim()`
- Nếu đã có note cũ thì append bằng newline

### 2.9. Rule khi convert lead sang enrolled

- Placement Test phải tồn tại
- `Lead` phải còn tồn tại
- Nếu là legacy flow không có `LeadChild`:
  - nếu `Lead.Status = Enrolled` thì chặn `LeadAlreadyEnrolled`
- Nếu request có `StudentProfileId`:
  - profile phải tồn tại và là student
  - nếu profile đã gán cho `LeadChild` khác thì trả conflict
  - profile sẽ được set vào `PlacementTest.StudentProfileId`
- Nếu là child-based flow:
  - nếu child đã `Enrolled` thì trả validation
  - nếu request có `StudentProfileId` thì set `LeadChild.ConvertedStudentProfileId`
  - set `LeadChild.Status = Enrolled`
  - nếu lead có ít nhất một child `Enrolled` thì set `Lead.Status = Enrolled`
  - tạo `LeadActivity`
- Nếu là legacy flow:
  - set trực tiếp `Lead.Status = Enrolled`
  - tạo `LeadActivity`
- Nếu `PlacementTest.Status != Completed` thì handler tự set `Completed`

### 2.10. Rule khi tạo retake

API mới: `POST /api/placement-tests/{id}/retake`

- `id` là `OriginalPlacementTestId`
- Request bắt buộc:
  - `StudentProfileId`
  - `NewProgramId`
  - `NewTuitionPlanId`
  - `BranchId`
- Request optional:
  - `ScheduledAt`
  - `Room`
  - `InvigilatorUserId`
  - `Note`
- Handler validate:
  - original placement test phải tồn tại
  - student profile phải tồn tại và là student
  - `originalPlacementTest.StudentProfileId` phải khớp `StudentProfileId` request
  - student không được có placement test khác đang `Scheduled` hoặc `Completed` ngoài original test
  - branch phải tồn tại và active
  - invigilator phải tồn tại nếu có truyền
  - `NewProgramId` phải tồn tại, active, không deleted
  - `NewTuitionPlanId` phải tồn tại, active và thuộc đúng `NewProgramId`
- Handler không có validator kiểm tra `ScheduledAt` quá khứ
- Handler không bắt buộc phải có active registration để tạo retake
- Retake placement test mới sẽ:
  - copy `LeadId`, `LeadChildId` từ original placement test
  - set `StudentProfileId` từ request
  - set `ClassId = null`
  - set `OriginalPlacementTestId = id` gốc
  - reset toàn bộ score về `null`
  - set `ProgramRecommendation = newProgram.Name`
  - set `Notes = request.Note` hoặc note mặc định
  - `Status = Scheduled`
- Nếu có active registration hiện tại:
  - response sẽ trả thêm `OriginalProgramName`, `OriginalTuitionPlanName`, `OriginalRemainingSessions`
- Nếu không có active registration:
  - retake vẫn được tạo
  - các field gốc trong response có thể là `null` / `0`

## 3. Status definition

### 3.1. `PlacementTestStatus`

| Status | Ý nghĩa |
| --- | --- |
| `Scheduled` | Đã tạo và đang chờ thi / thi lại |
| `NoShow` | Không tham dự |
| `Completed` | Đã hoàn tất kết quả hoặc bị set completed khi convert |
| `Cancelled` | Đã hủy |

### 3.2. Status flow thực tế trong code

| Từ | Sang | Trigger |
| --- | --- | --- |
| `null` | `Scheduled` | schedule hoặc retake |
| `Scheduled` | `Cancelled` | cancel |
| `Scheduled` | `NoShow` | no-show |
| `Scheduled` | `Completed` | update results đủ 5 score hoặc convert |
| `Cancelled` | `NoShow` | no-show |
| `Cancelled` | `Completed` | update results đủ 5 score hoặc convert |
| `NoShow` | `Cancelled` | cancel |
| `NoShow` | `Completed` | update results đủ 5 score hoặc convert |

Ghi chú:

- Đây là flow thực tế theo code hiện tại, không phải state machine bị khóa chặt.
- Flow retake tạo một record placement test mới với `Status = Scheduled`, không reset record cũ.

### 3.3. Status liên quan `Lead` / `LeadChild`

| Thời điểm | LeadChild | Lead |
| --- | --- | --- |
| Schedule | `BookedTest` | `BookedTest` |
| Update results đủ 5 score | `TestDone` nếu đang `BookedTest` | `TestDone` nếu đang `BookedTest` |
| Convert enrolled | `Enrolled` | `Enrolled` |

## 4. Permission matrix

| Chức năng | Admin | ManagementStaff | AccountantStaff | Teacher | Parent | Student |
| --- | --- | --- | --- | --- | --- | --- |
| Schedule placement test | Yes | Yes | No | No | No | No |
| Xem danh sách placement tests | Yes | Yes | Yes | No | No | No |
| Xem chi tiết placement test | Yes | Yes | Yes | No | No | No |
| Cập nhật placement test | Yes | Yes | No | No | No | No |
| Hủy placement test | Yes | Yes | No | No | No | No |
| Đánh dấu no-show | Yes | Yes | No | No | No | No |
| Nhập kết quả | Yes | Yes | No | No | No | No |
| Thêm note | Yes | Yes | No | No | No | No |
| Convert lead to enrolled | Yes | Yes | No | No | No | No |
| Tạo retake placement test | Yes | Yes | No | No | No | No |

## 5. Response format

### 5.1. Success

Controller đang dùng `result.MatchOk()`, nên response success theo wrapper API của project có dạng tổng quát:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 5.2. Error

Error thường ra theo `ProblemDetails` / error wrapper của project, ví dụ:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "PlacementTest.NotFound",
  "status": 404,
  "detail": "The placement test with the Id = '...' was not found"
}
```

### 5.3. Mapping status code

| HTTP code | Ý nghĩa |
| --- | --- |
| `400` | validation |
| `401` | thiếu token / token sai |
| `403` | sai role |
| `404` | không tìm thấy entity |
| `409` | conflict dữ liệu |
| `500` | lỗi hệ thống |

## 6. Validation rules và lỗi tiêu biểu

### 6.1. Schedule

| Rule | Nơi kiểm tra | Lỗi |
| --- | --- | --- |
| `scheduledAt` bắt buộc | request + validator | `400 Validation.General` |
| `scheduledAt` không ở quá khứ | validator | `400 Validation.General` |
| phải có `leadId` hoặc `leadChildId` | handler | `400 LeadId` |
| `leadChildId` phải tồn tại nếu truyền | handler | `404 LeadChild` |
| nếu có cả `leadId` và `leadChildId` thì phải khớp nhau | handler | `400 LeadId` |
| `leadId` phải tồn tại nếu dùng legacy flow | handler | `404 PlacementTest.LeadNotFound` |
| `invigilatorUserId` phải tồn tại nếu truyền | handler | `404 PlacementTest.InvigilatorNotFound` |

### 6.2. Update

| Rule | Nơi kiểm tra | Lỗi |
| --- | --- | --- |
| `placementTestId` bắt buộc | validator | `400 Validation.General` |
| `scheduledAt` không ở quá khứ nếu truyền | validator | `400 Validation.General` |
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| completed test không được update | handler | `400 PlacementTest.CannotUpdateCompletedTest` |
| `studentProfileId` phải là student hợp lệ nếu truyền | handler | `404 PlacementTest.StudentProfileNotFound` |
| `classId` phải tồn tại nếu truyền | handler | `404 PlacementTest.ClassNotFound` |
| `invigilatorUserId` phải tồn tại nếu truyền | handler | `404 PlacementTest.InvigilatorNotFound` |

### 6.3. Cancel / No-show

| Rule | Nơi kiểm tra | Lỗi |
| --- | --- | --- |
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| completed test không được cancel | handler | `400 PlacementTest.CannotCancelCompletedTest` |
| completed test không được no-show | handler | `400 PlacementTest.CannotMarkNoShowCompletedTest` |

### 6.4. Update results

| Rule | Nơi kiểm tra | Lỗi |
| --- | --- | --- |
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| score không có validator range | hiện chưa có | chưa có app-side error riêng |
| `LevelRecommendation` không có input ở API | contract hiện tại | không thể update qua endpoint |

### 6.5. Add note

| Rule | Nơi kiểm tra | Lỗi |
| --- | --- | --- |
| `note` bắt buộc | request annotation | thường `400` model validation |
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |

### 6.6. Convert to enrolled

| Rule | Nơi kiểm tra | Lỗi |
| --- | --- | --- |
| placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| lead phải tồn tại | handler | `404 PlacementTest.LeadNotFound` |
| legacy lead đã enrolled thì không convert lại | handler | `409 PlacementTest.LeadAlreadyEnrolled` |
| `studentProfileId` phải là student hợp lệ nếu truyền | handler | `404 PlacementTest.StudentProfileNotFound` |
| student profile không được gán cho child khác | handler | `409 PlacementTest.StudentProfileAlreadyAssigned` |
| child đã enrolled thì không convert lại | handler | `400 LeadChild` |

### 6.7. Retake

| Rule | Nơi kiểm tra | Lỗi |
| --- | --- | --- |
| original placement test phải tồn tại | handler | `404 PlacementTest.NotFound` |
| `studentProfileId` phải là student hợp lệ | handler | `404 PlacementTest.StudentProfileNotFound` |
| `studentProfileId` phải khớp `originalPlacementTest.StudentProfileId` | handler | `404 PlacementTest.StudentProfileNotFound` |
| student không được có placement test khác đang `Scheduled` hoặc `Completed` | handler | `409 PlacementTest.RetakeAlreadyScheduled` |
| `branchId` phải tồn tại và active | handler | `404 Registration.BranchNotFound` |
| `invigilatorUserId` phải tồn tại nếu truyền | handler | `404 PlacementTest.InvigilatorNotFound` |
| `newProgramId` phải tồn tại, active, không deleted | handler | `404 Registration.ProgramNotFound` |
| `newTuitionPlanId` phải thuộc đúng program mới và active | handler | `404 Registration.TuitionPlanNotFound` |
| `scheduledAt` hiện không có validator quá khứ | hiện chưa có | chưa có app-side error riêng |

## 7. Danh sách API

### 7.1. `POST /api/placement-tests`

- Role: `Admin`, `ManagementStaff`
- Mục đích: đặt lịch placement test

#### Body

| Field | Type | Required |
| --- | --- | --- |
| `leadId` | `guid?` | Conditional |
| `leadChildId` | `guid?` | Conditional |
| `scheduledAt` | `datetime` | Yes |
| `room` | `string?` | No |
| `invigilatorUserId` | `guid?` | No |

#### Success data

| Field | Type |
| --- | --- |
| `id` | `guid` |
| `leadId` | `guid?` |
| `leadChildId` | `guid?` |
| `studentProfileId` | `guid?` |
| `classId` | `guid?` |
| `scheduledAt` | `datetime?` |
| `status` | `string` |
| `room` | `string?` |
| `invigilatorUserId` | `guid?` |
| `createdAt` | `datetime` |

#### Error

- `400 Validation.General`
- `400 LeadId`
- `404 LeadChild`
- `404 PlacementTest.LeadNotFound`
- `404 PlacementTest.InvigilatorNotFound`

### 7.2. `GET /api/placement-tests`

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Mục đích: xem danh sách placement test

#### Query

| Param | Type | Required |
| --- | --- | --- |
| `leadId` | `guid?` | No |
| `studentProfileId` | `guid?` | No |
| `status` | `PlacementTestStatus?` | No |
| `fromDate` | `datetime?` | No |
| `toDate` | `datetime?` | No |
| `page` | `int`, default `1` | No |
| `pageSize` | `int`, default `20` | No |

#### Success data

`data` là:

```json
{
  "placementTests": [
    {
      "id": "guid",
      "leadId": "guid?",
      "leadChildId": "guid?",
      "leadContactName": "string?",
      "childName": "string?",
      "studentProfileId": "guid?",
      "studentName": "string?",
      "classId": "guid?",
      "className": "string?",
      "scheduledAt": "datetime?",
      "status": "Scheduled|NoShow|Completed|Cancelled",
      "room": "string?",
      "invigilatorUserId": "guid?",
      "invigilatorName": "string?",
      "resultScore": "decimal?",
      "listeningScore": "decimal?",
      "speakingScore": "decimal?",
      "readingScore": "decimal?",
      "writingScore": "decimal?",
      "levelRecommendation": "string?",
      "programRecommendation": "string?",
      "notes": "string?",
      "attachmentUrl": "string?",
      "createdAt": "datetime",
      "updatedAt": "datetime"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20,
  "totalPages": 0
}
```

### 7.3. `GET /api/placement-tests/{id}`

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Mục đích: xem chi tiết placement test
- Path: `id: guid`

#### Success data

Cùng shape với một item trong list.

#### Error

- `404 PlacementTest.NotFound`

### 7.4. `PUT /api/placement-tests/{id}`

- Role: `Admin`, `ManagementStaff`
- Mục đích: cập nhật lịch / phòng / giám thị / student / class

#### Body

| Field | Type | Required |
| --- | --- | --- |
| `scheduledAt` | `datetime?` | No |
| `room` | `string?` | No |
| `invigilatorUserId` | `guid?` | No |
| `studentProfileId` | `guid?` | No |
| `classId` | `guid?` | No |

#### Success data

| Field | Type |
| --- | --- |
| `id` | `guid` |
| `leadId` | `guid?` |
| `studentProfileId` | `guid?` |
| `classId` | `guid?` |
| `scheduledAt` | `datetime?` |
| `status` | `string` |
| `room` | `string?` |
| `invigilatorUserId` | `guid?` |
| `updatedAt` | `datetime` |

#### Error

- `400 Validation.General`
- `404 PlacementTest.NotFound`
- `400 PlacementTest.CannotUpdateCompletedTest`
- `404 PlacementTest.StudentProfileNotFound`
- `404 PlacementTest.ClassNotFound`
- `404 PlacementTest.InvigilatorNotFound`

### 7.5. `POST /api/placement-tests/{id}/cancel`

- Role: `Admin`, `ManagementStaff`
- Mục đích: hủy placement test

#### Body

| Field | Type | Required |
| --- | --- | --- |
| `reason` | `string?` | No |

#### Success data

| Field | Type |
| --- | --- |
| `id` | `guid` |
| `status` | `string` |
| `updatedAt` | `datetime` |

#### Error

- `404 PlacementTest.NotFound`
- `400 PlacementTest.CannotCancelCompletedTest`

### 7.6. `POST /api/placement-tests/{id}/no-show`

- Role: `Admin`, `ManagementStaff`
- Mục đích: đánh dấu no-show

#### Success data

| Field | Type |
| --- | --- |
| `id` | `guid` |
| `status` | `string` |
| `updatedAt` | `datetime` |

#### Error

- `404 PlacementTest.NotFound`
- `400 PlacementTest.CannotMarkNoShowCompletedTest`

### 7.7. `PUT /api/placement-tests/{id}/results`

- Role: `Admin`, `ManagementStaff`
- Mục đích: nhập điểm và recommendation đang được expose bởi API

#### Body

| Field | Type | Required |
| --- | --- | --- |
| `listeningScore` | `decimal?` | No |
| `speakingScore` | `decimal?` | No |
| `readingScore` | `decimal?` | No |
| `writingScore` | `decimal?` | No |
| `resultScore` | `decimal?` | No |
| `programRecommendation` | `string?` | No |
| `attachmentUrl` | `string?` | No |

#### Success data

| Field | Type |
| --- | --- |
| `id` | `guid` |
| `listeningScore` | `decimal?` |
| `speakingScore` | `decimal?` |
| `readingScore` | `decimal?` |
| `writingScore` | `decimal?` |
| `resultScore` | `decimal?` |
| `programRecommendation` | `string?` |
| `attachmentUrl` | `string?` |
| `status` | `string` |
| `updatedAt` | `datetime` |
| `newRegistrationId` | `guid?` |

#### Business effect

- nếu đủ 5 score thì `status` tự thành `Completed`
- có thể update `LeadChild.Status` và `Lead.Status` sang `TestDone`
- nếu đây là retake và đủ điều kiện thì `newRegistrationId` sẽ có giá trị

#### Error

- `404 PlacementTest.NotFound`

### 7.8. `POST /api/placement-tests/{id}/notes`

- Role: `Admin`, `ManagementStaff`
- Mục đích: thêm note

#### Body

| Field | Type | Required |
| --- | --- | --- |
| `note` | `string` | Yes |

#### Success data

| Field | Type |
| --- | --- |
| `id` | `guid` |
| `notes` | `string?` |
| `updatedAt` | `datetime` |

#### Error

- `400` nếu request thiếu `note`
- `404 PlacementTest.NotFound`

### 7.9. `POST /api/placement-tests/{id}/convert-to-enrolled`

- Role: `Admin`, `ManagementStaff`
- Mục đích: convert lead/child sang enrolled sau placement test

#### Body

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `guid?` | No |

#### Success data

| Field | Type |
| --- | --- |
| `leadId` | `guid` |
| `leadStatus` | `string` |
| `placementTestId` | `guid` |
| `placementTestStatus` | `string` |
| `studentProfileId` | `guid?` |
| `leadChildId` | `guid?` |

#### Error

- `404 PlacementTest.NotFound`
- `404 PlacementTest.LeadNotFound`
- `409 PlacementTest.LeadAlreadyEnrolled`
- `404 PlacementTest.StudentProfileNotFound`
- `409 PlacementTest.StudentProfileAlreadyAssigned`
- `400 LeadChild`

### 7.10. `POST /api/placement-tests/{id}/retake`

- Role: `Admin`, `ManagementStaff`
- Mục đích: tạo placement test retake cho student đang học hoặc cần thi lại để chuyển chương trình

#### Body

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `guid` | Yes |
| `newProgramId` | `guid` | Yes |
| `newTuitionPlanId` | `guid` | Yes |
| `branchId` | `guid` | Yes |
| `scheduledAt` | `datetime?` | No |
| `room` | `string?` | No |
| `invigilatorUserId` | `guid?` | No |
| `note` | `string?` | No |

#### Success data

| Field | Type |
| --- | --- |
| `newPlacementTestId` | `guid` |
| `originalPlacementTestId` | `guid` |
| `studentProfileId` | `guid` |
| `originalProgramName` | `string?` |
| `newProgramName` | `string?` |
| `originalTuitionPlanName` | `string?` |
| `newTuitionPlanName` | `string?` |
| `originalRemainingSessions` | `int` |
| `placementTestStatus` | `string` |
| `scheduledAt` | `datetime?` |
| `room` | `string?` |
| `invigilatorUserId` | `guid?` |
| `createdAt` | `datetime` |

#### Side effect

- tạo một placement test mới với `OriginalPlacementTestId = id` gốc
- copy `LeadId` và `LeadChildId` từ original test
- preset `ProgramRecommendation = newProgram.Name`
- tạo `LeadActivity` nếu placement test gốc có `LeadId`

#### Error

- `404 PlacementTest.NotFound`
- `404 PlacementTest.StudentProfileNotFound`
- `409 PlacementTest.RetakeAlreadyScheduled`
- `404 Registration.BranchNotFound`
- `404 PlacementTest.InvigilatorNotFound`
- `404 Registration.ProgramNotFound`
- `404 Registration.TuitionPlanNotFound`

## 8. Status và field quan trọng ở DB

| Field | Kiểu | Ràng buộc đáng chú ý |
| --- | --- | --- |
| `Status` | enum string | max length `20`, required |
| `Room` | string | max length `100` |
| `LevelRecommendation` | string | max length `100` |
| `ProgramRecommendation` | string | max length `100` |
| `OriginalPlacementTestId` | guid? | field dùng cho flow retake |
| `ResultScore` | numeric | nullable |
| `ListeningScore` | numeric | nullable |
| `SpeakingScore` | numeric | nullable |
| `ReadingScore` | numeric | nullable |
| `WritingScore` | numeric | nullable |

## 9. Các trường hợp trả lỗi quan trọng

- không truyền cả `leadId` và `leadChildId` khi schedule
- truyền cả `leadId` và `leadChildId` nhưng không khớp nhau
- schedule/update với `scheduledAt` ở quá khứ
- invigilator không tồn tại
- update/cancel/no-show đối với test đã `Completed`
- convert với student profile đã gán cho child khác
- convert child đã `Enrolled`
- retake cho student không match `originalPlacementTest.StudentProfileId`
- retake khi student đã có placement test khác ở trạng thái `Scheduled` hoặc `Completed`
- retake với branch/program/tuition plan không hợp lệ
- results đủ 5 score nhưng không tạo được registration mới vì thiếu active registration hoặc không resolve được program / tuition plan đích

## 10. Kết luận implementation hiện tại

- Placement Test hiện là flow nội bộ cho `Admin`, `ManagementStaff`, và read-only cho `AccountantStaff`
- Schedule API hiện tối giản, chỉ nhận thông tin lead/leadChild + lịch thi + phòng + invigilator
- `LevelRecommendation` tồn tại ở entity/response nhưng chưa có input API để cập nhật
- Flow `retake` đã được thêm ở controller và gắn qua `OriginalPlacementTestId`
- Auto tạo registration mới không diễn ra ở endpoint `retake`, mà diễn ra ở endpoint `results` khi retake placement test đủ điều kiện để complete
- State machine vẫn chưa khóa chặt giữa `Scheduled / NoShow / Cancelled / Completed`
