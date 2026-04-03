# Program -> Placement Test -> Registration Flow Guide

> Update 2026-04-03:
> Placement test recommendation contract da duoc doi sang `ProgramRecommendationId` va
> `SecondaryProgramRecommendationId`.
> Khong con su dung `IsSecondaryProgramSupplementary`; supplementary duoc suy ra tu `Program.IsSupplementary`.

## Business rule

### Phạm vi tài liệu

Tài liệu này mô tả luồng backend cho các thay đổi sau:

- Học viên có thể học `1 chương trình chính + 1 chương trình phụ` trong cùng một registration.
- Chương trình phụ có thể là:
  - Một chương trình trong chuỗi chính nhưng dùng cho kỹ năng trội hơn.
  - Một chương trình `supplementary` để dạy kèm thêm.
- Hai lớp khác nhau vẫn dùng chung `1 tuition plan / 1 package session`.
- Tài liệu này chưa bao gồm luồng invoice / finance.

### Quy tắc nghiệp vụ chính

1. Một `Registration` luôn đại diện cho một gói học chung.
2. `ProgramId` là chương trình chính, bắt buộc.
3. `SecondaryProgramId` là chương trình phụ, không bắt buộc.
4. `TuitionPlanId` chỉ chọn theo `ProgramId` của chương trình chính.
5. Không tạo tuition plan riêng cho `SecondaryProgramId`.
6. Một học viên không được có registration active trùng program với `ProgramId` hoặc `SecondaryProgramId`.
7. `SecondaryProgramId` phải khác `ProgramId`.
8. Placement test có thể lưu đồng thời:
   - `ProgramRecommendation`
   - `SecondaryProgramRecommendation`
   - `IsSecondaryProgramSupplementary`
   - `SecondaryProgramSkillFocus`
9. Trường hợp supplementary program không cần placement test thì có thể bỏ qua placement test và tạo registration trực tiếp.
10. Hai lớp thuộc 2 track khác nhau vẫn phải trỏ về cùng một `RegistrationId` trong `ClassEnrollment`.
11. Khi điểm danh `Present`, số buổi đã dùng và số buổi còn lại sẽ bị trừ trên registration chung.
12. Luồng `ConvertLeadToEnrolled` hiện chỉ đổi trạng thái lead/lead child và link student profile, chưa tự động tạo registration thường.
13. Giá trị `ProgramRecommendation` và `SecondaryProgramRecommendation` hiện đang lưu theo `Program.Name`, chưa phải `ProgramId`.
14. Luồng retake placement test có auto-create registration mới, nhưng chỉ dùng recommendation chính.

### Các trường dữ liệu mới / thay đổi

#### Program

- `IsSupplementary: bool`

#### PlacementTest

- `SecondaryProgramRecommendation: string?`
- `IsSecondaryProgramSupplementary: bool`
- `SecondaryProgramSkillFocus: string?`

#### Registration

- `SecondaryProgramId: Guid?`
- `SecondaryClassId: Guid?`
- `SecondaryClassAssignedDate: DateTime?`
- `SecondaryEntryType: EntryType?`
- `SecondaryProgramSkillFocus: string?`

#### ClassEnrollment

- `RegistrationId: Guid?`

### Luồng nghiệp vụ chuẩn

#### Case 1. Học 1 chương trình bình thường

1. Tạo hoặc chọn program chính.
2. Nếu có placement test thì nhập recommendation chính.
3. Convert lead sang enrolled nếu đi từ CRM.
4. Tạo registration với `ProgramId` + `TuitionPlanId`.
5. Suggest class và assign class cho `track = primary`.
6. Attendance sẽ trừ buổi vào registration chung.

#### Case 2. Học 2 chương trình theo kỹ năng trội

1. Tạo hoặc chọn program chính và program phụ.
2. Nhập kết quả placement test:
   - `ProgramRecommendation = Starter`
   - `SecondaryProgramRecommendation = Mover`
   - `SecondaryProgramSkillFocus = Speaking`
3. Convert lead sang enrolled nếu đi từ CRM.
4. Tạo registration:
   - `ProgramId = Starter`
   - `TuitionPlanId = plan của Starter`
   - `SecondaryProgramId = Mover`
   - `SecondaryProgramSkillFocus = Speaking`
5. Assign class cho `track = primary` và `track = secondary`.
6. Hai lớp dùng chung một package session.

#### Case 3. Học chương trình supplementary không qua placement test

1. Tạo program với `IsSupplementary = true`.
2. Tạo registration trực tiếp:
   - `ProgramId = chương trình chính`
   - `SecondaryProgramId = supplementary program`
3. Assign class cho từng track.
4. Attendance vẫn trừ buổi vào registration chung.

## Mỗi role được xem dữ liệu gì

| Role | Dữ liệu được xem |
| --- | --- |
| `Admin` | Toàn bộ program, placement test, registration trong hệ thống |
| `ManagementStaff` | Toàn bộ program, placement test, registration trong hệ thống |
| `AccountantStaff` | Chỉ xem danh sách và chi tiết placement test |
| `Anonymous` | Chỉ xem `GET /api/programs/active` và `GET /api/programs/{id}` |

## Phạm vi dữ liệu (own / department / all)

| Role | Phạm vi dữ liệu hiện tại |
| --- | --- |
| `Admin` | `all` |
| `ManagementStaff` | `all` |
| `AccountantStaff` | `all` nhưng chỉ cho API placement test read |
| `Anonymous` | `public` |

Ghi chú:

- Hiện backend chưa áp dụng rule `own` hoặc `department` cho các API trong tài liệu này.
- Các role được phép sẽ xem toàn bộ dữ liệu phù hợp với filter đầu vào.

## Các hành động được phép (view, create, edit, approve, delete…)

| Module | Admin | ManagementStaff | AccountantStaff | Anonymous |
| --- | --- | --- | --- | --- |
| Program | view, create, edit, toggle, delete | view, create, edit, toggle | không có | view active, view detail |
| Placement test | view, create, edit, update result, cancel, no-show, note, convert, retake | view, create, edit, update result, cancel, no-show, note, convert, retake | view | không có |
| Registration | view, create, edit, cancel, suggest, assign class, waiting list, transfer, upgrade | view, create, edit, cancel, suggest, assign class, waiting list, transfer, upgrade | không có | không có |

## Danh sách API

### Response success format chung

Tất cả API dùng `MatchOk()` hoặc `MatchCreated()` sẽ trả theo envelope:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Với API `POST` tạo mới, HTTP status là `201 Created`.

### Response error format chung

Khi lỗi, API trả `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Registration.SecondaryProgramDuplicated",
  "status": 400,
  "detail": "Secondary program must be different from primary program"
}
```

Nếu là validation error nhiều field:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "FieldName": [
      "Error message"
    ]
  }
}
```

### 1. Program APIs

#### `POST /api/programs`

- Mô tả API dùng để tạo program mới, bao gồm cả supplementary program.
- Role: `Admin`, `ManagementStaff`

Body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `branchId` | `Guid` | Có | Chi nhánh của program |
| `name` | `string` | Có | Tên program |
| `code` | `string` | Có | Mã program |
| `isMakeup` | `bool` | Có | Program bù |
| `isSupplementary` | `bool` | Có | Program phụ |

Response success:

- `201 Created`
- `data` gồm các field chính: `id`, `branchId`, `name`, `code`, `isMakeup`, `isSupplementary`, `isActive`

Response error:

| Status | Title / Code | Message |
| --- | --- | --- |
| `400` | validation error | `A program cannot be both makeup and supplementary.` |
| `400` | validation error | thiếu `branchId`, `name`, `code` |

#### `GET /api/programs`

- Mô tả API dùng để xem danh sách program.
- Role: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required |
| --- | --- | --- |
| `branchId` | `Guid?` | Không |
| `searchTerm` | `string?` | Không |
| `isActive` | `bool?` | Không |
| `isMakeup` | `bool?` | Không |
| `pageNumber` | `int` | Không |
| `pageSize` | `int` | Không |

Response success:

- `200 OK`
- `data.programs.items[]` có `isSupplementary`

#### `GET /api/programs/active`

- Mô tả API public để lấy program active.
- Role: `Anonymous`

#### `GET /api/programs/{id}`

- Mô tả API lấy chi tiết 1 program.
- Role: `Anonymous`

#### `PUT /api/programs/{id}`

- Mô tả API cập nhật program, bao gồm bật/tắt supplementary.
- Role: `Admin`, `ManagementStaff`

Body giống `POST /api/programs`.

#### `PATCH /api/programs/{id}/toggle-status`

- Mô tả API bật/tắt active status của program.
- Role: `Admin`, `ManagementStaff`

#### `DELETE /api/programs/{id}`

- Mô tả API xóa mềm program.
- Role: `Admin`

### 2. Placement test APIs

#### `POST /api/placement-tests`

- Mô tả API schedule placement test cho lead / lead child.
- Role: `Admin`, `ManagementStaff`

Body:

| Field | Type | Required |
| --- | --- | --- |
| `leadId` | `Guid?` | Không |
| `leadChildId` | `Guid?` | Không |
| `scheduledAt` | `DateTime?` | Không |
| `room` | `string?` | Không |
| `invigilatorUserId` | `Guid?` | Không |

Response success:

- `200 OK`
- `data` gồm thông tin placement test vừa tạo / schedule

#### `GET /api/placement-tests`

- Mô tả API xem danh sách placement test.
- Role: `Admin`, `ManagementStaff`, `AccountantStaff`

Query params:

| Field | Type | Required |
| --- | --- | --- |
| `leadId` | `Guid?` | Không |
| `studentProfileId` | `Guid?` | Không |
| `status` | `PlacementTestStatus?` | Không |
| `fromDate` | `DateTime?` | Không |
| `toDate` | `DateTime?` | Không |
| `sortBy` | `string?` | Không |
| `sortOrder` | `SortOrder` | Không |
| `page` | `int` | Không |
| `pageSize` | `int` | Không |

Response success:

- `200 OK`
- `data.items[]` có `programRecommendation`, `secondaryProgramRecommendation`, `secondaryProgramSkillFocus`

#### `GET /api/placement-tests/{id}`

- Mô tả API xem chi tiết placement test.
- Role: `Admin`, `ManagementStaff`, `AccountantStaff`

Response success:

- `200 OK`
- `data` có:
  - `programRecommendation`
  - `secondaryProgramRecommendation`
  - `isSecondaryProgramSupplementary`
  - `secondaryProgramSkillFocus`

#### `PUT /api/placement-tests/{id}/results`

- Mô tả API nhập điểm 4 kỹ năng và recommendation cho program chính + program phụ.
- Role: `Admin`, `ManagementStaff`

Body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `listeningScore` | `decimal?` | Không | Điểm kỹ năng |
| `speakingScore` | `decimal?` | Không | Điểm kỹ năng |
| `readingScore` | `decimal?` | Không | Điểm kỹ năng |
| `writingScore` | `decimal?` | Không | Điểm kỹ năng |
| `resultScore` | `decimal?` | Không | Điểm tổng |
| `programRecommendation` | `string?` | Không | Tên program chính |
| `secondaryProgramRecommendation` | `string?` | Không | Tên program phụ |
| `isSecondaryProgramSupplementary` | `bool?` | Không | Program phụ có phải supplementary không |
| `secondaryProgramSkillFocus` | `string?` | Không | Ví dụ `Speaking` |
| `attachmentUrl` | `string?` | Không | File đính kèm |

Response success:

- `200 OK`
- `data` có:
  - `id`
  - `listeningScore`, `speakingScore`, `readingScore`, `writingScore`, `resultScore`
  - `programRecommendation`
  - `secondaryProgramRecommendation`
  - `isSecondaryProgramSupplementary`
  - `secondaryProgramSkillFocus`
  - `status`
  - `newRegistrationId`

Response error:

| Status | Title / Code | Message |
| --- | --- | --- |
| `404` | placement test not found | Không tìm thấy placement test |
| `400` | validation error | Dữ liệu điểm hoặc input không hợp lệ |

#### `POST /api/placement-tests/{id}/convert-to-enrolled`

- Mô tả API convert lead / lead child thành enrolled và link student profile.
- Role: `Admin`, `ManagementStaff`
- Lưu ý: API này chưa tự tạo registration thường.

Body:

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `Guid?` | Không |

Response success:

- `200 OK`
- `data` có `leadId`, `leadStatus`, `placementTestId`, `placementTestStatus`, `studentProfileId`, `leadChildId`

#### `POST /api/placement-tests/{id}/retake`

- Mô tả API tạo placement test retake.
- Role: `Admin`, `ManagementStaff`
- Lưu ý: auto-create registration sau khi chấm retake hiện chỉ dùng recommendation chính.

### 3. Registration APIs

#### `POST /api/registrations`

- Mô tả API tạo registration chung cho 1 package, hỗ trợ thêm secondary program.
- Role: `Admin`, `ManagementStaff`

Body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Có | Học viên |
| `branchId` | `Guid` | Có | Chi nhánh |
| `programId` | `Guid` | Có | Program chính |
| `tuitionPlanId` | `Guid` | Có | Chỉ thuộc program chính |
| `secondaryProgramId` | `Guid?` | Không | Program phụ |
| `secondaryProgramSkillFocus` | `string?` | Không | Ví dụ `Speaking` |
| `expectedStartDate` | `DateTime?` | Không | Ngày dự kiến học |
| `preferredSchedule` | `string?` | Không | Mong muốn lịch học |
| `note` | `string?` | Không | Ghi chú |

Response success:

- `201 Created`
- `data` có:
  - `id`
  - `programId`, `programName`
  - `secondaryProgramId`, `secondaryProgramName`, `secondaryProgramSkillFocus`
  - `tuitionPlanId`, `tuitionPlanName`
  - `status`

Response error:

| Status | Title / Code | Message |
| --- | --- | --- |
| `404` | `Registration.StudentNotFound` | Student không tồn tại |
| `404` | `Registration.BranchNotFound` | Branch không tồn tại |
| `404` | `Registration.ProgramNotFound` | Program không tồn tại hoặc không active |
| `404` | `Registration.TuitionPlanNotFound` | Tuition plan không thuộc program chính |
| `400` | `Registration.SecondaryProgramDuplicated` | Secondary program phải khác primary |
| `409` | `Registration.AlreadyExists` | Học viên đã có active registration trùng program |

#### `PUT /api/registrations/{id}`

- Mô tả API cập nhật registration, bao gồm thêm, sửa, xóa secondary program trước khi assign secondary class.
- Role: `Admin`, `ManagementStaff`

Body:

| Field | Type | Required |
| --- | --- | --- |
| `expectedStartDate` | `DateTime?` | Không |
| `preferredSchedule` | `string?` | Không |
| `note` | `string?` | Không |
| `tuitionPlanId` | `Guid?` | Không |
| `secondaryProgramId` | `Guid?` | Không |
| `secondaryProgramSkillFocus` | `string?` | Không |
| `removeSecondaryProgram` | `bool` | Không |

Response error:

| Status | Title / Code | Message |
| --- | --- | --- |
| `400` | `Registration.SecondaryClassAssigned` | Không được đổi hoặc xóa secondary program khi đã có secondary class |
| `400` | `Registration.SecondaryProgramMissing` | Skill focus chỉ được set khi có secondary program |
| `400` | `Registration.ClassAlreadyAssigned` | Không được đổi tuition plan khi đã có class ở bất kỳ track nào |

#### `GET /api/registrations`

- Mô tả API xem danh sách registration.
- Role: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `Guid?` | Không |
| `branchId` | `Guid?` | Không |
| `programId` | `Guid?` | Không |
| `status` | `string?` | Không |
| `classId` | `Guid?` | Không |
| `pageNumber` | `int` | Không |
| `pageSize` | `int` | Không |

Response success:

- `200 OK`
- `data.page.items[]` có `secondaryProgramId`, `secondaryProgramName`, `secondaryClassId`, `secondaryClassName`, `remainingSessions`

#### `GET /api/registrations/{id}`

- Mô tả API xem chi tiết registration.
- Role: `Admin`, `ManagementStaff`

Response success:

- `200 OK`
- `data` có đầy đủ primary track và secondary track

#### `GET /api/registrations/{id}/suggest-classes`

- Mô tả API gợi ý lớp cho cả primary program và secondary program.
- Role: `Admin`, `ManagementStaff`

Response success:

- `200 OK`
- `data` có:
  - `suggestedClasses`, `alternativeClasses`
  - `secondarySuggestedClasses`, `secondaryAlternativeClasses`
  - `secondaryProgramSkillFocus`

#### `POST /api/registrations/{id}/assign-class`

- Mô tả API xếp lớp cho từng track của registration.
- Role: `Admin`, `ManagementStaff`

Body:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `classId` | `Guid?` | Không | Bắt buộc nếu `entryType != wait` |
| `entryType` | `string` | Có | `immediate`, `makeup`, `wait` |
| `track` | `string` | Có | `primary`, `secondary` |

Response success:

- `200 OK`
- `data` có `registrationId`, `registrationStatus`, `classId`, `classCode`, `classTitle`, `track`, `entryType`

Response error:

| Status | Title / Code | Message |
| --- | --- | --- |
| `400` | `Registration.SecondaryProgramMissing` | Track secondary nhưng registration chưa có secondary program |
| `400` | `Registration.ClassIdRequired` | Thiếu classId khi không phải wait |
| `400` | `Registration.ClassAlreadyAssigned` | Track đã có class, phải transfer thay vì assign lại |
| `400` | `ClassNotAvailable` | Class bị completed, cancelled hoặc suspended |
| `400` | `Registration.ClassNotMatchingProgram` | Class không thuộc đúng program của track |
| `409` | `AlreadyEnrolled` | Học viên đã ở trong class đó |

#### `GET /api/registrations/waiting-list`

- Mô tả API danh sách chờ xếp lớp, hỗ trợ lọc theo track.
- Role: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required |
| --- | --- | --- |
| `branchId` | `Guid?` | Không |
| `programId` | `Guid?` | Không |
| `track` | `string?` | Không |
| `pageNumber` | `int` | Không |
| `pageSize` | `int` | Không |

Response success:

- `200 OK`
- `data.items[]` có `track`, `programId`, `programName`, `isSupplementaryProgram`, `programSkillFocus`

#### `POST /api/registrations/{id}/transfer-class`

- Mô tả API chuyển lớp cho primary hoặc secondary track.
- Role: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required |
| --- | --- | --- |
| `newClassId` | `Guid` | Có |
| `track` | `string` | Không |
| `effectiveDate` | `DateTime?` | Không |

Response error:

| Status | Title / Code | Message |
| --- | --- | --- |
| `400` | `Registration.SecondaryProgramMissing` | Track secondary nhưng chưa có secondary program |
| `400` | `NoClassAssigned` | Track chưa có class để transfer |
| `400` | `Registration.CannotTransferToSameClass` | Không được transfer sang cùng class |
| `400` | `Registration.ClassNotMatchingProgram` | Class mới không đúng program của track |
| `400` | `Registration.ClassFull` | Class mới đã đầy |

#### `POST /api/registrations/{id}/upgrade`

- Mô tả API nâng gói học nhưng vẫn giữ toàn bộ dữ liệu primary / secondary track.
- Role: `Admin`, `ManagementStaff`

Body: không có body.

Query params:

| Field | Type | Required |
| --- | --- | --- |
| `newTuitionPlanId` | `Guid` | Có |

## Status definition

### Danh sách status

#### Program

Program hiện không dùng enum status riêng, mà dùng:

- `IsActive = true`: đang dùng được
- `IsActive = false`: ngưng dùng
- `IsDeleted = true`: xóa mềm

#### PlacementTestStatus

| Status | Ý nghĩa |
| --- | --- |
| `Scheduled` | Đã lên lịch placement test |
| `NoShow` | Học viên không đến test |
| `Completed` | Đã chấm điểm và hoàn tất |
| `Cancelled` | Đã hủy lịch test |

#### RegistrationStatus

| Status | Ý nghĩa |
| --- | --- |
| `New` | Vừa tạo registration, chưa xác định trạng thái học |
| `WaitingForClass` | Còn thiếu class ở primary track hoặc secondary track |
| `ClassAssigned` | Đã xếp lớp nhưng chưa vào học ngay |
| `Studying` | Có ít nhất một track đang vào học ngay |
| `Paused` | Bảo lưu |
| `Completed` | Hết buổi hoặc hoàn tất |
| `Cancelled` | Hủy đăng ký |

#### EntryType

| EntryType | Ý nghĩa |
| --- | --- |
| `Immediate` | Vào học ngay |
| `Makeup` | Học bù trước rồi mới vào lớp chính thức |
| `Wait` | Chờ lớp mới |
| `Retake` | Dùng cho luồng thi lại / lên lớp cao hơn |

#### LeadStatus / LeadChildStatus liên quan placement test

| Status | Ý nghĩa |
| --- | --- |
| `BookedTest` | Đã đặt lịch test |
| `TestDone` | Đã test xong |
| `Enrolled` | Đã convert sang enrolled |

### Luồng chuyển trạng thái

#### Placement test

- `Scheduled -> Completed`
- `Scheduled -> NoShow`
- `Scheduled -> Cancelled`

#### Lead / LeadChild

- `BookedTest -> TestDone`
- `TestDone -> Enrolled`

#### Registration

- `New -> WaitingForClass`
- `New -> ClassAssigned`
- `New -> Studying`
- `WaitingForClass -> ClassAssigned`
- `WaitingForClass -> Studying`
- `ClassAssigned -> Studying`
- `Studying -> Completed`
- `New / WaitingForClass / ClassAssigned / Studying -> Cancelled`

Ghi chú:

- Logic resolve status hiện dựa trên cả `primary` và `secondary` track.
- Nếu có `SecondaryProgramId` mà `SecondaryClassId` chưa có thì registration vẫn là `WaitingForClass`.
- Nếu có ít nhất một track `Immediate` thì registration có thể lên `Studying`.

## Permission matrix theo role

| API / Action | Admin | ManagementStaff | AccountantStaff | Anonymous |
| --- | --- | --- | --- | --- |
| Tạo / sửa / toggle / xóa program | Có | Có, trừ delete | Không | Không |
| Xem program detail | Có | Có | Không | Có |
| Xem active programs | Có | Có | Có | Có |
| Schedule placement test | Có | Có | Không | Không |
| Xem placement test list / detail | Có | Có | Có | Không |
| Update placement test result | Có | Có | Không | Không |
| Convert lead to enrolled | Có | Có | Không | Không |
| Retake placement test | Có | Có | Không | Không |
| Tạo / sửa / xem / hủy registration | Có | Có | Không | Không |
| Suggest class / waiting list / assign / transfer / upgrade registration | Có | Có | Không | Không |

## Validation rule

### Rule kiểm tra dữ liệu

#### Program

1. `BranchId` bắt buộc.
2. `Name` bắt buộc, tối đa 255 ký tự.
3. `Code` bắt buộc, tối đa 10 ký tự.
4. Không được vừa `IsMakeup = true` vừa `IsSupplementary = true`.

#### Placement test

1. `ProgramRecommendation` và `SecondaryProgramRecommendation` hiện nhận theo tên program.
2. Nếu xóa `SecondaryProgramRecommendation` thì backend sẽ tự clear:
   - `IsSecondaryProgramSupplementary`
   - `SecondaryProgramSkillFocus`
3. `SecondaryProgramSkillFocus` chỉ có ý nghĩa khi có `SecondaryProgramRecommendation`.
4. Placement test chỉ tự chuyển `Completed` khi đủ 4 kỹ năng và `ResultScore`.

#### Registration create / update

1. Student phải tồn tại và là `ProfileType.Student`.
2. Branch phải active.
3. Primary program phải active và chưa deleted.
4. Tuition plan phải thuộc primary program.
5. Secondary program phải active và chưa deleted.
6. `SecondaryProgramId` phải khác `ProgramId`.
7. Không được tạo registration mới nếu học viên đã có active registration trùng primary hoặc secondary program.
8. Không được đổi tuition plan sau khi đã assign class ở bất kỳ track nào.
9. Không được xóa hoặc đổi secondary program nếu secondary class đã được assign.
10. `SecondaryProgramSkillFocus` chỉ được set khi có secondary program.

#### Assign class

1. `track` được normalize thành `primary` hoặc `secondary`.
2. Track `secondary` chỉ hợp lệ khi registration có `SecondaryProgramId`.
3. Nếu `entryType != wait` thì `classId` là bắt buộc.
4. Class phải thuộc đúng program của track.
5. Class không được ở trạng thái `Completed`, `Cancelled`, `Suspended`.
6. Class không được full.
7. Học viên không được active trong cùng class đó.
8. Không được assign lại class cho track đã có class, phải dùng transfer.

#### Transfer class

1. Track phải đang có class cũ.
2. Class mới phải khác class cũ.
3. Class mới phải thuộc đúng program của track.
4. Class mới phải còn chỗ và đang ở trạng thái `Active` hoặc `Recruiting`.

### Các trường hợp trả lỗi

| Nhóm | HTTP | Code / Title | Mô tả |
| --- | --- | --- | --- |
| Program | `400` | validation error | Thiếu field bắt buộc hoặc vừa makeup vừa supplementary |
| Placement test | `404` | not found | Không tìm thấy placement test |
| Placement test | `400` | validation error | Input điểm / recommendation không hợp lệ |
| Registration | `404` | `Registration.StudentNotFound` | Không tìm thấy học viên |
| Registration | `404` | `Registration.BranchNotFound` | Không tìm thấy branch |
| Registration | `404` | `Registration.ProgramNotFound` | Không tìm thấy program |
| Registration | `404` | `Registration.TuitionPlanNotFound` | Không tìm thấy tuition plan hợp lệ |
| Registration | `400` | `Registration.SecondaryProgramDuplicated` | Secondary program trùng primary |
| Registration | `409` | `Registration.AlreadyExists` | Trùng registration active |
| Registration update | `400` | `Registration.SecondaryClassAssigned` | Đã assign secondary class nên không được đổi secondary program |
| Registration update | `400` | `Registration.SecondaryProgramMissing` | Set skill focus nhưng chưa có secondary program |
| Assign class | `400` | `Registration.ClassIdRequired` | Thiếu classId khi không phải wait |
| Assign class | `400` | `Registration.ClassNotMatchingProgram` | Class không thuộc program tương ứng |
| Assign class | `400` | `Registration.ClassFull` | Class đã đầy |
| Assign class | `409` | `AlreadyEnrolled` | Học viên đã có trong class |
| Transfer class | `400` | `NoClassAssigned` | Track chưa có class để transfer |
| Transfer class | `400` | `Registration.CannotTransferToSameClass` | Transfer sang cùng class |
| Auth | `401` | `Unauthorized` | Thiếu hoặc sai token |
| Auth | `403` | `Forbidden` | Không có quyền theo role |
| Server | `500` | `Server failure` | Lỗi ngoài dự kiến |

## Ghi chú triển khai frontend / backoffice

1. FE nên hiển thị riêng:
   - Program chính
   - Program phụ
   - Skill focus của program phụ
   - Track khi assign class
2. FE không nên tự tạo 2 registration cho case học 2 chương trình.
3. FE nên chọn `TuitionPlanId` theo program chính duy nhất.
4. Sau placement test, FE/backoffice phải map recommendation theo `Program.Name` sang `ProgramId` trước khi gọi create registration.
5. Với waiting list, FE nên cho filter theo `track = primary | secondary`.
