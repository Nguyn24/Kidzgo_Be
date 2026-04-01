
# Flow Change - Mixed Program + Session-Level Assignment

## 1. Mục đích và phạm vi

Tài liệu này tổng hợp toàn bộ thay đổi BE đã được sửa cho flow:

- Program có thể được đánh dấu là `supplementary`
- Placement test có thể đề xuất thêm `secondary program`
- Registration hỗ trợ `1 package / 2 tracks`
- Học sinh có thể học 2 lớp nhưng chỉ học một phần lịch của mỗi lớp thông qua `session-level assignment`
- Timetable, attendance roster, leave/makeup, session report validation, session reminder đã chuyển sang đọc theo session assignment thật sự

Ngoài phạm vi đợt này:

- Không sửa invoice / finance / cashbook
- Không đổi flow thu tiền
- Không thêm UI mới trong BE, chỉ đổi contract và logic

---

## 2. Business rule

### BR-01. Program được chia thành 3 nhóm nghiệp vụ

- `Main program`: chương trình chính như Starter, Mover, Flyer
- `Supplementary program`: chương trình phụ để dạy kèm, không nằm trong chuỗi chính
- `Makeup program`: chương trình dùng cho học bù

Rule:

- Một `Program` không được vừa `IsMakeup = true` vừa `IsSupplementary = true`

### BR-02. Placement test có thể đề xuất 2 hướng học

- `ProgramRecommendation`: chương trình chính
- `SecondaryProgramRecommendation`: chương trình phụ / chương trình học theo kỹ năng trội hơn
- `IsSecondaryProgramSupplementary`: secondary program là supplementary hay không
- `SecondaryProgramSkillFocus`: kỹ năng được tách ra, ví dụ `Speaking`

### BR-03. 1 registration = 1 package học chung

Một `Registration` vẫn là gói học chung để quản lý:

- học viên
- branch
- primary program
- tuition plan
- tổng số buổi / đã học / còn lại

Registration có thể gắn thêm:

- `SecondaryProgramId`
- `SecondaryClassId`
- `SecondaryEntryType`
- `SecondaryProgramSkillFocus`

### BR-04. 1 registration có thể học 2 lớp

Nếu học sinh học 2 tracks thì:

- `primary` học một lớp
- `secondary` học một lớp khác
- cả hai vẫn dùng chung `RegistrationId`
- `RemainingSessions` được trừ trên 1 package chung

### BR-05. Không còn coi ClassEnrollment là đủ

Trước đây:

- học sinh có `ClassEnrollment` là được xem như học toàn bộ session của lớp

Hiện tại:

- `ClassEnrollment` chỉ cho biết học sinh thuộc lớp nào
- `StudentSessionAssignment` mới là nguồn sự thật để xác định học sinh học session nào

### BR-06. SessionSelectionPattern cho phép chọn một phần lịch lớp

Khi xếp lớp / chuyển lớp / tạo enrollment, có thể truyền:

- `SessionSelectionPattern`

Rule:

- Pattern phải là subset của `Class.SchedulePattern`
- Nếu không truyền thì học sinh học tất cả session hợp lệ của lớp

Ví dụ:

- Lớp có lịch `Tue + Thu`
- Học sinh chỉ học `Tue` cho lớp A
- Học sinh chỉ học `Thu` cho lớp B

### BR-07. Timetable của học sinh hiển thị theo session được assign

Timetable không còn lấy "toàn bộ session của mỗi class enrollment" nữa.

Timetable mới gồm:

- regular sessions từ `StudentSessionAssignments`
- fallback legacy sessions nếu session đó chưa có assignment rows
- makeup sessions từ `MakeupAllocations`

### BR-08. Attendance roster chỉ hiển thị học sinh thực sự thuộc session

Roster của session lấy theo:

- regular participants
- makeup participants

Không hiển thị học sinh của cả lớp nếu học sinh đó không được assign vào session đó.

### BR-09. Mark attendance chỉ hợp lệ cho participant thật sự

Teacher/Admin chỉ có thể mark attendance cho học sinh nằm trong participant list của session.

Rule:

- học sinh regular + `Present` -> trừ buổi trong registration
- học sinh makeup + `Present` -> không trừ buổi package regular

### BR-10. Leave request theo session thật sự

Leave request mới có thể tạo theo:

- 1 session cụ thể qua `SessionId`
- hoặc 1 khoảng ngày, nhưng backend chỉ tạo leave cho các session học sinh được assign thật sự

### BR-11. Makeup suggest/use phải tránh xung đột lịch thật sự

Khi gợi ý học bù hoặc chọn session học bù:

- backend check xung đột với regular sessions
- backend check xung đột với makeup sessions khác
- backend check minimum gap 2 giờ
- backend check sức chứa session

### BR-12. Session report chỉ được tạo cho học sinh thuộc session

Teacher chỉ tạo report được khi:

- teacher đó đang dạy session đó
- học sinh thuộc participant list của session đó

### BR-13. Session reminder gửi theo participant thật sự

Reminder session chỉ gửi cho học sinh có regular assignment của session đó.

Makeup reminder vẫn là luồng riêng.

### BR-14. Upgrade package giữ nguyên logic 1 package chung

Khi upgrade:

- tạo registration mới
- carry forward `RemainingSessions`
- cộng thêm số buổi của gói mới
- cập nhật active enrollments + future session assignments sang `RegistrationId` mới

Ví dụ:

- còn 2 buổi
- upgrade thêm gói 30 buổi
- registration mới có `NewTotalSessions = 32`

### BR-15. Legacy data vẫn được support

Nếu session cũ chưa có `StudentSessionAssignments`:

- timetable / roster / leave / makeup vẫn fallback sang logic `ClassEnrollment`

Mục đích:

- không vỡ dữ liệu cũ
- cho phép migrate dần

---

## 3. Dữ liệu và entity thay đổi

### 3.1 Program

Thêm field:

- `IsSupplementary: bool`

### 3.2 PlacementTest

Thêm field:

- `SecondaryProgramRecommendation: string?`
- `IsSecondaryProgramSupplementary: bool`
- `SecondaryProgramSkillFocus: string?`

### 3.3 Registration

Thêm / đã dùng các field liên quan 2 tracks:

- `SecondaryProgramId`
- `SecondaryClassId`
- `SecondaryEntryType`
- `SecondaryProgramSkillFocus`

### 3.4 ClassEnrollment

Thêm field:

- `RegistrationId`
- `Track: RegistrationTrackType`
- `SessionSelectionPattern: string?`

### 3.5 StudentSessionAssignment

Entity mới:

- `SessionId`
- `StudentProfileId`
- `ClassEnrollmentId`
- `RegistrationId`
- `Track`
- `Status`

### 3.6 LeaveRequest

Thêm field:

- `SessionId`

---

## 4. Mỗi role được xem dữ liệu gì

| Role | Dữ liệu xem được | Ghi chú |
|---|---|---|
| Admin | Tất cả program, placement test, registration, session, attendance, leave, makeup, session report | Full system scope |
| ManagementStaff | Tất cả dữ liệu vận hành liên quan program, placement test, registration, session, leave, session report | Hiện tại BE đang theo all scope, FE có thể filter theo branch |
| AccountantStaff | Xem placement test list/detail | Read-only, không sửa placement test |
| Teacher | Xem/tác động attendance session; tạo/xem/sửa/submit session report; xem report của lớp mình | Session report có check teacher phải thuộc session; attendance endpoint hiện tại mới check role |
| Parent | Xem report đã publish của con; dùng makeup theo con; tạo leave request qua API được phép | `Use makeup` có check parent-child; `leave create/list` hiện tại chưa khóa chặt ownership ở controller |
| Student | Xem timetable của chính mình; xem report đã publish của chính mình; dùng makeup của chính mình | Timetable lấy `StudentId` từ token |
| Anonymous | Xem `GET /api/programs/active`, `GET /api/programs/{id}` | Chỉ read program public |

---

## 5. Phạm vi dữ liệu (own / department / all)

| Chức năng | Admin | ManagementStaff | AccountantStaff | Teacher | Parent | Student |
|---|---|---|---|---|---|---|
| Program | all | all | none | none | none | none |
| Placement test list/detail | all | all | all | none | none | none |
| Placement test update results | all | all | none | none | none | none |
| Registration | all | all | none | none | none | none |
| Student timetable | none | none | none | none | none | own |
| Attendance roster / mark | all | none | none | current BE = role-based, chưa khóa theo own class | none | none |
| Leave request create/list/detail | current BE = all authenticated với query/body | current BE = all authenticated với query/body | current BE = all authenticated với query/body | current BE = all authenticated với query/body | own / child là mong muốn, nhưng create/list hiện tại chưa khóa chặt | own là mong muốn, list default theo token nếu không truyền studentProfileId |
| Makeup suggest/use | all read nếu gọi API; `use` thực tế chỉ cho parent/student | all read nếu gọi API; `use` bị handler chặn | all read nếu gọi API; `use` bị handler chặn | all read nếu gọi API; `use` bị handler chặn | own children | own |
| Session report list/detail | all | all | none | own classes | own children, chỉ Published | own, chỉ Published |
| Session reminder | system job | system job | system job | system job | system job | system job |

Ghi chú quan trọng:

- Bảng trên ghi theo hành vi BE hiện tại
- Có một số endpoint controller đang mở rộng hơn mong muốn về ownership, nhất là leave/makeup read endpoints
- FE nên ẩn UI theo role nghiệp vụ dự kiến, không nên dựa hoàn toàn vào route exposure

---

## 6. Các hành động được phép (view, create, edit, approve, delete)

| Chức năng | View | Create | Edit | Approve / Submit | Delete / Cancel |
|---|---|---|---|---|---|
| Program | Admin, ManagementStaff, Anonymous(active/by-id) | Admin, ManagementStaff | Admin, ManagementStaff | toggle-status: Admin, ManagementStaff | delete: Admin |
| Placement test | Admin, ManagementStaff, AccountantStaff | Admin, ManagementStaff | Admin, ManagementStaff | convert/retake: Admin, ManagementStaff | cancel/no-show: Admin, ManagementStaff |
| Registration | Admin, ManagementStaff | Admin, ManagementStaff | Admin, ManagementStaff | assign-class / transfer / upgrade: Admin, ManagementStaff | cancel: Admin, ManagementStaff |
| Student timetable | Student | none | none | none | none |
| Attendance roster | Admin, Teacher | none | none | mark/update: Admin, Teacher | none |
| Leave request | Authenticated | Authenticated | none | approve/reject/bulk-approve: Admin, ManagementStaff | cancel: theo command đã có, cần endpoint nếu FE dùng |
| Makeup credit | Authenticated read | none trong đợt này | use/expire theo endpoint hiện có | none | expire / cancel allocation qua flow nghiệp vụ |
| Session report | Teacher/Admin/ManagementStaff/Parent | Teacher | Teacher/Admin/ManagementStaff | submit: Teacher, approve/reject/publish: Admin/ManagementStaff | none |

---

## 7. Danh sách API

### 7.1 Success format chung

Tất cả endpoint dùng `MatchOk()` / `MatchCreated()` trả về wrapper:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Với `201 Created`, body vẫn là:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Kèm `Location header`.

### 7.2 Error format chung

Khi lỗi, API trả `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Error.Code",
  "status": 400,
  "detail": "Human readable message",
  "errors": {
    "FieldName": [
      "message 1"
    ]
  }
}
```

Mapping:

- `400`: validation / problem
- `404`: not found
- `409`: conflict

---

## 8. Chi tiết API theo module

## 8.1 Program APIs

### API: POST `/api/programs`

- Mục đích: tạo program, support thêm supplementary program
- Roles: `Admin`, `ManagementStaff`
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `branchId` | `Guid` | yes | Branch của program |
| `name` | `string` | yes | Tên program |
| `code` | `string` | yes | Mã program |
| `isMakeup` | `bool` | yes | Đánh dấu program học bù |
| `isSupplementary` | `bool` | yes | Đánh dấu program phụ |

- Success `201`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "guid",
    "name": "Starter Support",
    "code": "SUP01",
    "isMakeup": false,
    "isSupplementary": true
  }
}
```

- Error:
  - `400 Program cannot be both makeup and supplementary`
  - `404 Branch not found`
  - `409 Program code/name duplicated` nếu handler/DB có unique conflict

### API: PUT `/api/programs/{id}`

- Mục đích: cập nhật program, bao gồm `isSupplementary`
- Roles: `Admin`, `ManagementStaff`
- Body: giống `create`
- Success: trả program đã cập nhật
- Error: giống `create` + `404 ProgramNotFound`

### API: GET `/api/programs`

- Mục đích: list program cho màn hình admin/staff
- Roles: `Admin`, `ManagementStaff`
- Query:

| Field | Type | Required |
|---|---|---|
| `branchId` | `Guid?` | no |
| `searchTerm` | `string?` | no |
| `isActive` | `bool?` | no |
| `isMakeup` | `bool?` | no |
| `pageNumber` | `int` | no |
| `pageSize` | `int` | no |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "programs": {
      "items": [
        {
          "id": "guid",
          "name": "Starter",
          "isMakeup": false,
          "isSupplementary": false
        }
      ]
    }
  }
}
```

### API: GET `/api/programs/{id}` and GET `/api/programs/active`

- Mục đích: FE lấy thông tin program public / active
- Roles:
  - by id: `anonymous`
  - active: `anonymous`
- Response đã có thêm `isSupplementary`

---

## 8.2 Placement Test APIs

### API: PUT `/api/placement-tests/{id}/results`

- Mục đích: lưu điểm 4 kỹ năng và recommendation primary + secondary
- Roles: `Admin`, `ManagementStaff`
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `listeningScore` | `decimal?` | no | Điểm nghe |
| `speakingScore` | `decimal?` | no | Điểm nói |
| `readingScore` | `decimal?` | no | Điểm đọc |
| `writingScore` | `decimal?` | no | Điểm viết |
| `resultScore` | `decimal?` | no | Điểm tổng |
| `programRecommendation` | `string?` | no | Program chính |
| `secondaryProgramRecommendation` | `string?` | no | Program secondary |
| `isSecondaryProgramSupplementary` | `bool?` | no | Secondary có phải supplementary |
| `secondaryProgramSkillFocus` | `string?` | no | Ví dụ `Speaking` |
| `attachmentUrl` | `string?` | no | File test |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programRecommendation": "Starter",
    "secondaryProgramRecommendation": "Mover",
    "isSecondaryProgramSupplementary": false,
    "secondaryProgramSkillFocus": "Speaking",
    "status": "Completed",
    "newRegistrationId": null
  }
}
```

- Error:
  - `404 PlacementTestNotFound`

Rule quan trọng:

- Nếu set `secondaryProgramRecommendation = ""` hoặc whitespace thì BE clear cả 3 field secondary
- Retake auto-create registration hiện tại chỉ mới auto xử lý theo `ProgramRecommendation`, chưa auto mở rộng sang secondary recommendation

### API: GET `/api/placement-tests`
### API: GET `/api/placement-tests/{id}`

- Mục đích: list/detail placement test
- Roles:
  - list/detail: `Admin`, `ManagementStaff`, `AccountantStaff`
- Response đã có thêm:
  - `secondaryProgramRecommendation`
  - `isSecondaryProgramSupplementary`
  - `secondaryProgramSkillFocus`

### API: POST `/api/placement-tests`
### API: POST `/api/placement-tests/{id}/convert-to-enrolled`

- Mục đích: schedule test và convert lead sang enrolled
- Contract không đổi trong đợt này
- Lưu ý: convert-to-enrolled không tự động tạo registration mixed-track, staff vẫn cần tạo registration tương ứng theo recommendation

---

## 8.3 Registration APIs

### API: POST `/api/registrations`

- Mục đích: tạo registration 1 package, support secondary program
- Roles: `Admin`, `ManagementStaff`
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `studentProfileId` | `Guid` | yes | Học sinh |
| `branchId` | `Guid` | yes | Chi nhánh |
| `programId` | `Guid` | yes | Program chính |
| `tuitionPlanId` | `Guid` | yes | Gói học của program chính |
| `secondaryProgramId` | `Guid?` | no | Program secondary |
| `secondaryProgramSkillFocus` | `string?` | no | Kỹ năng focus |
| `expectedStartDate` | `DateTime?` | no | Ngày bắt đầu dự kiến |
| `preferredSchedule` | `string?` | no | Nhu cầu lịch học |
| `note` | `string?` | no | Ghi chú |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "programName": "Starter",
    "secondaryProgramId": "guid",
    "secondaryProgramName": "Mover",
    "secondaryProgramSkillFocus": "Speaking",
    "status": "New",
    "classId": null,
    "secondaryClassId": null
  }
}
```

- Error:
  - `404 StudentNotFound`
  - `404 BranchNotFound`
  - `404 ProgramNotFound`
  - `404 TuitionPlanNotFound`
  - `400 Registration.SecondaryProgramDuplicated`
  - `409 AlreadyExists`

### API: GET `/api/registrations`

- Mục đích: list registrations
- Roles: `Admin`, `ManagementStaff`
- Query:

| Field | Type | Required |
|---|---|---|
| `studentProfileId` | `Guid?` | no |
| `branchId` | `Guid?` | no |
| `programId` | `Guid?` | no |
| `status` | `string?` | no |
| `classId` | `Guid?` | no |
| `pageNumber` | `int` | no |
| `pageSize` | `int` | no |

- Success: mỗi item đã có thêm:
  - `secondaryProgramId`
  - `secondaryProgramName`
  - `secondaryProgramSkillFocus`
  - `secondaryClassId`
  - `secondaryClassName`

### API: GET `/api/registrations/{id}`

- Mục đích: chi tiết registration
- Roles: `Admin`, `ManagementStaff`
- Success đã có thêm:
  - `secondaryProgramId`
  - `secondaryProgramName`
  - `secondaryProgramSkillFocus`
  - `secondaryClassId`
  - `secondaryClassName`
  - `secondaryEntryType`

### API: PUT `/api/registrations/{id}`

- Mục đích: cập nhật registration, support secondary program
- Roles: `Admin`, `ManagementStaff`
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `expectedStartDate` | `DateTime?` | no | |
| `preferredSchedule` | `string?` | no | |
| `note` | `string?` | no | |
| `tuitionPlanId` | `Guid?` | no | Chỉ đổi được trước khi assign bất kỳ class nào |
| `secondaryProgramId` | `Guid?` | no | Đổi / thêm secondary program |
| `secondaryProgramSkillFocus` | `string?` | no | Skill focus |
| `removeSecondaryProgram` | `bool` | no | Xóa secondary program |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "tuitionPlanId": "guid",
    "secondaryProgramId": "guid",
    "secondaryProgramName": "Mover",
    "secondaryProgramSkillFocus": "Speaking"
  }
}
```

- Error:
  - `404 RegistrationNotFound`
  - `400 InvalidStatus`
  - `400 Registration.SecondaryProgramDuplicated`
  - `400 Registration.SecondaryProgramMissing`
  - `400 Registration.SecondaryClassAssigned`
  - `400 Registration.ClassAlreadyAssigned`
  - `400 DifferentProgram`

### API: GET `/api/registrations/{id}/suggest-classes`

- Mục đích: gợi ý lớp cho cả primary và secondary track
- Roles: `Admin`, `ManagementStaff`
- Params: path `id`
- Success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "programName": "Starter",
    "suggestedClasses": [],
    "alternativeClasses": [],
    "secondaryProgramId": "guid",
    "secondaryProgramName": "Mover",
    "secondaryProgramSkillFocus": "Speaking",
    "secondarySuggestedClasses": [],
    "secondaryAlternativeClasses": []
  }
}
```

- Error:
  - `404 RegistrationNotFound`

### API: POST `/api/registrations/{id}/assign-class`

- Mục đích: xếp lớp cho track `primary` hoặc `secondary`, đồng thời tạo session assignments
- Roles: `Admin`, `ManagementStaff`
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `classId` | `Guid?` | yes cho `immediate/makeup/retake`, no cho `wait` | Lớp cần xếp |
| `entryType` | `string` | yes | `immediate`, `makeup`, `wait`, `retake` |
| `track` | `string` | yes | `primary` hoặc `secondary` |
| `sessionSelectionPattern` | `string?` | no | RRULE subset của class schedule |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "registrationStatus": "Studying",
    "classId": "guid",
    "classCode": "STA01",
    "classTitle": "Starter Tue Thu",
    "track": "secondary",
    "entryType": "Immediate",
    "classAssignedDate": "2026-04-01T10:00:00Z",
    "warningMessage": null
  }
}
```

- Error:
  - `404 RegistrationNotFound`
  - `404 ClassNotFound`
  - `400 Registration.SecondaryProgramMissing`
  - `400 Registration.ClassIdRequired`
  - `400 Registration.ClassAlreadyAssigned`
  - `400 ClassNotMatchingProgram`
  - `400 ClassNotAvailable`
  - `409 AlreadyEnrolled`
  - `400 Enrollment.SessionSelectionPatternInvalid`
  - `400 Enrollment.SessionSelectionPatternEmpty`
  - `400 Enrollment.SessionSelectionPatternMismatch`

### API: GET `/api/registrations/waiting-list`

- Mục đích: waiting list theo track
- Roles: `Admin`, `ManagementStaff`
- Query:

| Field | Type | Required |
|---|---|---|
| `branchId` | `Guid?` | no |
| `programId` | `Guid?` | no |
| `track` | `string?` | no |
| `pageNumber` | `int` | no |
| `pageSize` | `int` | no |

- Success: mỗi item đã có thêm:
  - `track`
  - `isSupplementaryProgram`
  - `programSkillFocus`

### API: POST `/api/registrations/{id}/transfer-class`

- Mục đích: chuyển lớp theo từng track, đồng bộ lại future session assignments
- Roles: `Admin`, `ManagementStaff`
- Query:

| Field | Type | Required |
|---|---|---|
| `newClassId` | `Guid` | yes |
| `track` | `string` | no |
| `sessionSelectionPattern` | `string?` | no |
| `effectiveDate` | `DateTime?` | no |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "oldClassId": "guid",
    "newClassId": "guid",
    "track": "primary",
    "effectiveDate": "2026-04-05T00:00:00Z",
    "status": "Studying"
  }
}
```

- Error:
  - `404 RegistrationNotFound`
  - `404 ClassNotFound`
  - `400 NoClassAssigned`
  - `400 ClassNotMatchingProgram`
  - `400 ClassNotAvailable`
  - `400 ClassFull`
  - `400 CannotTransferToSameClass`
  - `400 Enrollment.SessionSelectionPatternInvalid/Mismatch`

### API: POST `/api/registrations/{id}/upgrade`

- Mục đích: gia hạn / upgrade package, carry forward remaining sessions
- Roles: `Admin`, `ManagementStaff`
- Query:

| Field | Type | Required |
|---|---|---|
| `newTuitionPlanId` | `Guid` | yes |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "originalRegistrationId": "guid",
    "newRegistrationId": "guid",
    "oldTuitionPlanName": "Starter 30",
    "newTuitionPlanName": "Starter 30",
    "oldTotalSessions": 30,
    "newTotalSessions": 32,
    "addedSessions": 30,
    "status": "Studying"
  }
}
```

- Error:
  - `404 RegistrationNotFound`
  - `404 TuitionPlanNotFound`
  - `400 NoActiveRegistrationForUpgrade`
  - `400 DifferentProgram`

### API: PATCH `/api/registrations/{id}/cancel`

- Mục đích: hủy registration và cancel future session assignments trên enrollments liên quan
- Roles: `Admin`, `ManagementStaff`
- Query:

| Field | Type | Required |
|---|---|---|
| `reason` | `string?` | no |

- Success: trả registration đã cancel
- Error:
  - `404 RegistrationNotFound`
  - `400 InvalidStatus`

---

## 8.4 Student timetable API

### API: GET `/api/students/timetable`

- Mục đích: trả timetable của học sinh theo session assignment thật sự
- Roles: học sinh đang login
- Scope: `own`
- Query:

| Field | Type | Required |
|---|---|---|
| `from` | `DateTime?` | no |
| `to` | `DateTime?` | no |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "STA01",
        "classTitle": "Starter Tue Thu",
        "plannedDatetime": "2026-04-08T09:00:00Z",
        "durationMinutes": 90,
        "registrationId": "guid",
        "track": "secondary",
        "isMakeup": false,
        "attendanceStatus": "Present",
        "absenceType": null
      }
    ]
  }
}
```

- Trường mới FE cần dùng:
  - `registrationId`
  - `track`
  - `isMakeup`

- Error:
  - `404 StudentNotFound`

---

## 8.5 Session APIs (logic changed, contract gần như giữ nguyên)

### APIs liên quan

- `POST /api/sessions/generate-from-pattern`
- `POST /api/sessions`
- `PUT /api/sessions/{id}`
- `PUT /api/sessions/by-class`
- `POST /api/sessions/{id}/cancel`

### Mục đích thay đổi

Contract request/response không thay đổi đáng kể, nhưng sau khi đợt này:

- create/generate/update/cancel session sẽ sync lại `StudentSessionAssignments`
- nếu học sinh chỉ học một phần lịch của lớp thì session mới / session sửa sẽ tự động cập nhật assignment đúng track và pattern

### Ghi chú FE/QA

- Không cần sửa body API session
- Cần test lại vì timetable/roster của học sinh sẽ thay đổi khi session thay đổi

---

## 8.6 Attendance APIs

### API: GET `/api/attendance/{sessionId}`

- Mục đích: lấy roster session theo participant thật sự
- Roles: `Admin`, `Teacher`
- Success:

```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "guid",
    "summary": {
      "totalStudents": 10,
      "presentCount": 6,
      "absentCount": 1,
      "makeupCount": 1,
      "notMarkedCount": 2
    },
    "attendances": [
      {
        "studentProfileId": "guid",
        "studentName": "A",
        "registrationId": "guid",
        "track": "secondary",
        "isMakeup": false,
        "attendanceStatus": "NotMarked",
        "hasMakeupCredit": false
      }
    ]
  }
}
```

- Trường mới FE cần dùng:
  - `registrationId`
  - `track`
  - `isMakeup`

- Error:
  - `404 SessionNotFound`

### API: POST `/api/attendance/{sessionId}`

- Mục đích: mark attendance
- Roles: `Admin`, `Teacher`
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `attendances` | `array` | yes | Danh sách học sinh cần mark |
| `attendances[].studentProfileId` | `Guid` | yes | Học sinh |
| `attendances[].attendanceStatus` | `string` | yes | `Present`, `Absent`, `Makeup`, `NotMarked` |
| `attendances[].note` | `string?` | no | Ghi chú |

- Success: trả danh sách attendance đã mark
- Error:
  - `404 AttendanceNotFound/SessionNotFound`
  - `400 Attendance.StudentNotAssigned`

Rule:

- Regular participant + `Present` -> trừ `RemainingSessions`
- Makeup participant + `Present` -> không trừ package regular
- `Absent` sẽ resolve `AbsenceType` dựa trên leave request giống session hoặc range ngày

---

## 8.7 Leave Request APIs

### API: POST `/api/leave-requests`

- Mục đích: tạo leave request theo session cụ thể hoặc theo range, nhưng BE chỉ tạo leave cho các session học sinh được assign thật sự
- Roles: authenticated
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `studentProfileId` | `Guid` | yes | Học sinh |
| `classId` | `Guid` | yes | Lớp |
| `sessionId` | `Guid?` | no | Nếu muốn xin nghỉ đúng 1 session cụ thể |
| `sessionDate` | `DateOnly` | yes | Ngày bắt đầu xin nghỉ |
| `endDate` | `DateOnly?` | no | Ngày kết thúc nếu xin nghỉ range |
| `reason` | `string?` | no | Lý do |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "leaveRequests": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "classId": "guid",
        "sessionId": "guid",
        "sessionDate": "2026-04-08",
        "status": "Approved"
      }
    ]
  }
}
```

- Error:
  - `404 LeaveRequestNotFound`
  - `404 ClassNotFound`
  - `404 SessionNotFound`
  - `400 LeaveRequest.NotEnrolled`
  - `400 Session.StudentNotAssigned`
  - `400 LeaveRequest.AlreadyExists`
  - `400 ExceededMonthlyLeaveLimit`

Rule:

- Nếu `sessionId` có giá trị -> session phải thuộc `classId` và học sinh phải thuộc session đó
- Nếu xin range -> chỉ sinh leave cho các session assigned thật sự
- `NoticeHours >= 24` -> auto `Approved`, tạo makeup credit
- `NoticeHours < 24` -> `Pending`

### API: GET `/api/leave-requests`
### API: GET `/api/leave-requests/{id}`

- Mục đích: list/detail leave request
- Roles: authenticated
- Response mới đã có thêm `sessionId`

### API: PUT `/api/leave-requests/{id}/approve`
### API: PUT `/api/leave-requests/approve-bulk`
### API: PUT `/api/leave-requests/{id}/reject`

- Mục đích: duyệt / từ chối leave
- Roles:
  - approve/reject/bulk approve: `Admin`, `ManagementStaff`
- Logic thay đổi:
  - nếu leave có `SessionId` -> xử lý đúng session đó
  - capacity weekend makeup được tính theo participant thật sự, không còn chỉ dựa vào full class enrollment

---

## 8.8 Makeup APIs

### API: GET `/api/makeup-credits/{id}/parent/get-available-sessions`

- Mục đích: gợi ý session học bù không xung đột lịch thật sự của học sinh
- Roles: authenticated
- Query:

| Field | Type | Required |
|---|---|---|
| `fromDate` | `DateOnly?` | no |
| `toDate` | `DateOnly?` | no |
| `timeOfDay` | `string?` | no |

- Success:

```json
{
  "isSuccess": true,
  "data": [
    {
      "sessionId": "guid",
      "classId": "guid",
      "classCode": "MUP01",
      "programName": "Weekend Makeup",
      "plannedDatetime": "2026-04-11T08:00:00Z",
      "plannedEndDatetime": "2026-04-11T09:30:00Z"
    }
  ]
}
```

- Error:
  - `404 MakeupCreditNotFound`

Rule:

- chỉ lấy session cuối tuần
- không lấy session quá khứ
- check capacity theo participant thật sự
- check xung đột với regular sessions + makeup sessions + minimum gap 2 giờ

### API: POST `/api/makeup-credits/{id}/use`

- Mục đích: chọn session học bù
- Roles: authenticated, nhưng handler thực tế chỉ cho:
  - `Parent`
  - `Student`
- Body:

| Field | Type | Required | Note |
|---|---|---|---|
| `studentProfileId` | `Guid?` | no/conditional | Parent phải truyền |
| `classId` | `Guid` | yes | Lớp makeup target |
| `targetSessionId` | `Guid` | yes | Session makeup target |

- Success:

```json
{
  "isSuccess": true,
  "data": null
}
```

- Error:
  - `404 MakeupCreditNotFound`
  - `400 ParentMustProvideStudentProfileId`
  - `400 StudentNotBelongToParent`
  - `400 MustBeWeekend`
  - `400 MustBeFutureWeek`
  - `400 SessionNotBelongToClass`
  - `400 MakeupCredit.StudentAlreadyInTargetSession`
  - `400 MakeupCredit.TargetSessionConflict`
  - `400 TargetSessionFull`

---

## 8.9 Session Report APIs

### API: POST `/api/session-reports`

- Mục đích: teacher tạo session report cho học sinh
- Roles: `Teacher`
- Body:

| Field | Type | Required |
|---|---|---|
| `sessionId` | `Guid` | yes |
| `studentProfileId` | `Guid` | yes |
| `reportDate` | `DateOnly` | yes |
| `feedback` | `string` | yes |

- Success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "sessionId": "guid",
    "studentProfileId": "guid",
    "teacherUserId": "guid",
    "status": "Draft"
  }
}
```

- Error:
  - `404 SessionNotFound`
  - `404 UserNotFound`
  - `400 Session.StudentNotAssigned`
  - `409 SessionReport.AlreadyExists`
  - `400 Session.UnauthorizedAccess`

Rule:

- teacher phải là planned/actual teacher của session
- học sinh phải nằm trong participant list của session

### API: GET `/api/session-reports`
### API: GET `/api/session-reports/{id}`

- Mục đích: list/detail session report
- Roles:
  - list/detail: `Teacher`, `Admin`, `ManagementStaff`, `Parent`
- Scope trong handler:
  - `Teacher`: chỉ report của các lớp mình đang dạy
  - `Parent`: chỉ report `Published` của con
  - `Student`: nếu có profile student, chỉ report `Published` của chính mình
  - `Admin/ManagementStaff`: all

### APIs status flow

- `POST /api/session-reports/{id}/submit` -> `Draft` -> `Review`
- `POST /api/session-reports/{id}/approve` -> `Review` -> `Approved`
- `POST /api/session-reports/{id}/reject` -> `Review` -> `Rejected`
- `POST /api/session-reports/{id}/publish` -> `Approved` -> `Published`

---

## 8.10 Internal / no-API changes

### Session reminder job

Không có endpoint mới.

Logic mới:

- job session reminder lấy học sinh từ `StudentSessionAssignmentService.GetRegularParticipantsAsync()`
- không gửi reminder cho học sinh không thuộc session đó
- makeup reminder vẫn ở luồng riêng

### Session generation / update sync assignments

Không đổi contract API session, nhưng:

- generate / create / update / bulk update / cancel session sẽ sync lại `StudentSessionAssignments`

---

## 9. Status definition

## 9.1 Program classification

| Field | Giá trị | Ý nghĩa |
|---|---|---|
| `isMakeup` | `true/false` | Program học bù |
| `isSupplementary` | `true/false` | Program phụ, dạy kèm |

Rule:

- không được vừa makeup vừa supplementary

## 9.2 PlacementTestStatus

| Status | Ý nghĩa |
|---|---|
| `Scheduled` | Đã đặt lịch test |
| `Completed` | Đã chấm điểm/lưu kết quả |
| `NoShow` | Vắng test |
| `Cancelled` | Hủy lịch test |

Flow:

- `Scheduled` -> `Completed`
- `Scheduled` -> `NoShow`
- `Scheduled` -> `Cancelled`

## 9.3 RegistrationStatus

| Status | Ý nghĩa |
|---|---|
| `New` | Mới tạo, chưa xếp lớp |
| `WaitingForClass` | Chờ xếp lớp cho ít nhất 1 track |
| `ClassAssigned` | Đã xếp lớp nhưng chưa học ngay |
| `Studying` | Đang học |
| `Paused` | Bảo lưu |
| `Completed` | Đã hoàn thành package / registration cũ sau upgrade |
| `Cancelled` | Đã hủy |

Flow tổng quát:

- `New` -> `WaitingForClass`
- `New` -> `ClassAssigned`
- `New` -> `Studying`
- `WaitingForClass` -> `ClassAssigned`
- `WaitingForClass` -> `Studying`
- `ClassAssigned` -> `Studying`
- `Studying` -> `Completed`
- `Studying` -> `Cancelled`
- `Studying` -> `Paused`

## 9.4 Registration track và entry type

| Field | Giá trị | Ý nghĩa |
|---|---|---|
| `track` | `primary` | Track chính |
| `track` | `secondary` | Track phụ |
| `entryType` | `Immediate` | Vào học ngay |
| `entryType` | `Makeup` | Cần học bù trước |
| `entryType` | `Wait` | Chờ lớp |
| `entryType` | `Retake` | Nghiệp vụ thi lại / xếp lại |

## 9.5 StudentSessionAssignmentStatus

| Status | Ý nghĩa |
|---|---|
| `Assigned` | Học sinh được học session |
| `Cancelled` | Session không còn thuộc học sinh nữa |

Flow:

- `Assigned` -> `Cancelled` khi drop/pause/transfer/cancel session/đổi pattern
- `Cancelled` -> `Assigned` khi sync lại pattern hợp lệ

## 9.6 SessionStatus

| Status | Ý nghĩa |
|---|---|
| `Scheduled` | Session sắp / đang chờ học |
| `Completed` | Session đã dạy xong |
| `Cancelled` | Session bị hủy |

## 9.7 AttendanceStatus và AbsenceType

| Field | Giá trị | Ý nghĩa |
|---|---|---|
| `attendanceStatus` | `Present` | Có mặt |
| `attendanceStatus` | `Absent` | Vắng |
| `attendanceStatus` | `Makeup` | Đi học bù |
| `attendanceStatus` | `NotMarked` | Chưa điểm danh |
| `absenceType` | `WithNotice24H` | Xin nghỉ trước 24h |
| `absenceType` | `Under24H` | Xin nghỉ dưới 24h |
| `absenceType` | `NoNotice` | Nghỉ không báo trước |
| `absenceType` | `LongTerm` | Bảo lưu / nghỉ dài hạn |

## 9.8 LeaveRequestStatus

| Status | Ý nghĩa |
|---|---|
| `Pending` | Chờ duyệt |
| `Approved` | Đã duyệt |
| `Rejected` | Bị từ chối |
| `Cancelled` | Bị hủy |

Flow:

- `Pending` -> `Approved`
- `Pending` -> `Rejected`
- `Pending` -> `Cancelled`
- `Approved` -> `Cancelled`

## 9.9 MakeupCreditStatus và MakeupAllocationStatus

| Field | Giá trị | Ý nghĩa |
|---|---|---|
| `MakeupCreditStatus` | `Available` | Có thể dùng |
| `MakeupCreditStatus` | `Used` | Đã chọn session học bù |
| `MakeupCreditStatus` | `Expired` | Hết hạn |
| `MakeupAllocationStatus` | `Pending` | Đã gắn session học bù, chờ xác nhận nghiệp vụ |
| `MakeupAllocationStatus` | `Confirmed` | Đã xác nhận |
| `MakeupAllocationStatus` | `Cancelled` | Bỏ allocation cũ |

## 9.10 ReportStatus

| Status | Ý nghĩa |
|---|---|
| `Draft` | Teacher vừa tạo |
| `Review` | Đã submit cho staff/admin duyệt |
| `Approved` | Đã duyệt |
| `Rejected` | Bị từ chối |
| `Published` | Đã công bố cho parent/student xem |

Flow:

- `Draft` -> `Review`
- `Review` -> `Approved`
- `Review` -> `Rejected`
- `Approved` -> `Published`

---

## 10. Permission matrix theo role

| Module | Admin | ManagementStaff | AccountantStaff | Teacher | Parent | Student |
|---|---|---|---|---|---|---|
| Program CRUD | Y | Y (không delete) | N | N | N | N |
| Placement test view | Y | Y | Y | N | N | N |
| Placement test update result | Y | Y | N | N | N | N |
| Registration CRUD | Y | Y | N | N | N | N |
| Suggest / assign / transfer / upgrade registration | Y | Y | N | N | N | N |
| Student timetable | N | N | N | N | N | Y own |
| Attendance roster / mark | Y | Y role endpoint, scope own session chưa khóa chặt | N | Y role endpoint | N | N |
| Leave request create/list/detail | Y | Y | Y | Y | Y | Y |
| Leave request approve/reject | Y | Y | N | N | N | N |
| Makeup suggest sessions | Y | Y | Y | Y | Y | Y |
| Makeup use | N/A theo UI nghiệp vụ | N/A theo UI nghiệp vụ | N/A | N/A | Y own child | Y own |
| Session report list/detail | Y all | Y all | N | Y own classes | Y own child published | Y own published |
| Session report create/update/submit | Y edit via admin/staff endpoints một phần | Y edit/approve/reject/publish | N | Y create/update/submit | N | N |

Ghi chú:

- Matrix này phản ánh hành vi BE hiện tại + nghiệp vụ mong muốn cho FE
- Cho leave/makeup read flows, BE hiện tại vẫn còn mở rộng hơn mong muốn; FE nên giới hạn màn hình theo role

---

## 11. Validation rule

## 11.1 Program

- `BranchId`, `Name`, `Code` bắt buộc
- `Name <= 255`, `Code <= 10`
- không được `IsMakeup = true` và `IsSupplementary = true` cùng lúc

## 11.2 Placement test

- placement test phải tồn tại trước khi update result
- nếu clear `secondaryProgramRecommendation` thì BE tự clear cả `isSecondaryProgramSupplementary` và `secondaryProgramSkillFocus`

## 11.3 Registration create/update

- student phải tồn tại và là profile `Student`
- branch phải active
- primary program phải active, không deleted
- tuition plan phải thuộc primary program
- secondary program nếu có phải active, không deleted
- secondary program không được giống primary program
- học sinh không được có registration active trùng primary/secondary program đó
- `secondaryProgramSkillFocus` chỉ hợp lệ khi secondary program tồn tại
- không được remove / change secondary program nếu secondary class đã được assign
- không được đổi tuition plan sau khi đã assign bất kỳ primary/secondary class nào
- tuition plan mới khi update phải thuộc cùng primary program

## 11.4 Assign class / transfer class

- registration phải tồn tại và không `Completed/Cancelled`
- nếu `track = secondary` thì registration phải có `SecondaryProgramId`
- `wait` không cần `classId`; các loại khác bắt buộc có `classId`
- class phải thuộc đúng program của track
- class không được `Completed/Cancelled/Suspended`
- class mới khi transfer phải `Active` hoặc `Recruiting`
- class phải còn sức chứa
- học sinh không được đang active trong class đó
- không được assign lại class cho track đã có class
- không được transfer sang cùng class
- `SessionSelectionPattern` nếu có:
  - phải parse được
  - phải sinh ra ít nhất 1 slot hợp lệ
  - phải là subset của class schedule

## 11.5 Student timetable

- lấy `StudentId` từ token
- profile phải tồn tại, active, không deleted, và đúng user hiện tại

## 11.6 Attendance

- session phải tồn tại
- học sinh được mark phải nằm trong participant list của session
- chỉ regular participant mới trừ buổi package khi `Present`

## 11.7 Leave request

- student phải tồn tại, active
- class phải tồn tại
- nếu có `SessionId`:
  - session phải thuộc class
  - học sinh phải được assign vào session
- nếu xin theo range:
  - học sinh phải đang active trong class
  - backend chỉ lấy các session assigned trong range
- không được tạo trùng leave trên cùng session
- tổng số leave trong tháng không được vượt `ProgramLeavePolicy.MaxLeavesPerMonth`

## 11.8 Makeup

- credit phải tồn tại và thuộc về học sinh đang thao tác
- parent phải truyền `studentProfileId` và phải đúng quan hệ parent-child
- target session phải tồn tại
- target session phải là cuối tuần
- target session phải nằm ở tuần sau source session
- target session phải thuộc `classId` gửi lên
- nếu reschedule credit đã dùng, phải giữ cùng makeup program hiện tại
- học sinh không được đã nằm sẵn trong target session
- target session không được đầy
- target session không được xung đột / quá sát (< 2h) với session khác của học sinh

## 11.9 Session report

- session phải tồn tại
- student profile phải tồn tại và là `Student`
- teacher hiện tại phải tồn tại và là `Teacher`
- teacher phải là planned/actual teacher của session
- học sinh phải được assign vào session
- mỗi cặp `sessionId + studentProfileId` chỉ được có 1 report

---

## 12. Các trường hợp trả lỗi thường gặp

| Code / Title | HTTP | Khi nào gặp |
|---|---|---|
| `Program.NotFound` | 404 | Program không tồn tại |
| `PlacementTest.NotFound` | 404 | Placement test không tồn tại |
| `Registration.NotFound` | 404 | Registration không tồn tại |
| `Registration.SecondaryProgramDuplicated` | 400 | Secondary giống primary |
| `Registration.SecondaryProgramMissing` | 400 | Thao tác trên track secondary khi chưa có secondary program |
| `Registration.SecondaryClassAssigned` | 400 | Thử đổi/xóa secondary program khi secondary class đã assigned |
| `Registration.ClassAlreadyAssigned` | 400 | Track đã có class, cần transfer thay vì assign lại |
| `Registration.ClassIdRequired` | 400 | Assign class mà không gửi classId |
| `ClassNotAvailable` | 400 | Lớp không ở trạng thái cho phép |
| `ClassFull` | 400/409 | Lớp đã đầy |
| `AlreadyEnrolled` | 409 | Học sinh đã có enrollment trong lớp |
| `Enrollment.SessionSelectionPatternInvalid` | 400 | RRULE sai format |
| `Enrollment.SessionSelectionPatternEmpty` | 400 | Pattern hợp lệ nhưng không match slot nào |
| `Enrollment.SessionSelectionPatternMismatch` | 400 | Pattern không nằm trong lịch lớp |
| `Attendance.StudentNotAssigned` | 400 | Học sinh không thuộc session khi mark attendance |
| `Session.StudentNotAssigned` | 400 | Học sinh không thuộc session trong leave/report |
| `LeaveRequest.AlreadyExists` | 400 | Đã có leave cho session đó |
| `LeaveRequest.ExceededMonthlyLeaveLimit` | 400 | Vượt mức leave/tháng |
| `MakeupCredit.StudentAlreadyInTargetSession` | 400 | Học sinh đã nằm sẵn trong session học bù target |
| `MakeupCredit.TargetSessionConflict` | 400 | Session học bù trùng / quá sát lịch khác |
| `MakeupCredit.TargetSessionFull` | 400 | Session học bù đã đầy |
| `SessionReport.AlreadyExists` | 409 | Đã có report cho session + student |
| `Session.UnauthorizedAccess` | 400 | Teacher không thuộc session đó |

---

## 13. FE impact checklist

### 13.1 Màn hình Program

- Thêm field `isSupplementary`
- Hiển thị rõ `main / supplementary / makeup`

### 13.2 Màn hình Placement Test

- Thêm 3 field:
  - `secondaryProgramRecommendation`
  - `isSecondaryProgramSupplementary`
  - `secondaryProgramSkillFocus`
- Detail/list placement test cần hiển thị secondary recommendation

### 13.3 Màn hình Registration

- Form tạo/sửa registration cần support:
  - `secondaryProgramId`
  - `secondaryProgramSkillFocus`
- Detail/list registration cần hiển thị:
  - primary program/class
  - secondary program/class
- Waiting list cần filter / show theo `track`

### 13.4 Màn hình Assign class / Transfer class

- Bắt buộc FE phải biết đang thao tác trên `primary` hay `secondary`
- Nếu cần chia lịch học 2 lớp, FE phải gửi thêm `sessionSelectionPattern`

### 13.5 Timetable học sinh

- Không được assume mỗi class enrollment = học tất cả session của lớp
- Cần hiển thị:
  - `track`
  - `isMakeup`
  - `registrationId`

### 13.6 Attendance roster

- FE chỉ render roster backend trả về, không tự tuyển học sinh theo class
- Có thể hiển thị badge:
  - `primary/secondary`
  - `makeup`

### 13.7 Leave / Makeup

- Form leave nên hỗ trợ chọn `sessionId`
- Màn hình makeup suggestion / choose session phải test conflict real timetable

### 13.8 Session report

- Form create report phải chỉ cho teacher tạo cho học sinh thuộc session
- FE nên lấy student từ roster / participant list, không từ list học sinh của cả class

---

## 14. QA / test checklist

1. Tạo `supplementary program` mới, verify list/detail trả về `isSupplementary = true`.
2. Chấm placement test có `secondaryProgramRecommendation`, verify detail/list placement test có đủ 3 field secondary.
3. Tạo registration có `secondaryProgramId`, verify create/list/detail registration trả đúng primary + secondary.
4. Gọi suggest-classes, verify có đủ:
   - `suggestedClasses`
   - `secondarySuggestedClasses`
5. Assign `primary` và `secondary` vào 2 lớp khác nhau, gửi `sessionSelectionPattern` tách lịch, verify timetable chỉ ra đúng các session được assign.
6. Mark attendance cho học sinh không thuộc session, verify trả `Attendance.StudentNotAssigned`.
7. Mark attendance cho học sinh regular `Present`, verify `RemainingSessions` giảm.
8. Mark attendance cho học sinh makeup `Present`, verify không giảm package regular.
9. Tạo leave theo `sessionId`, verify leave request có `sessionId`, makeup credit tạo đúng source session.
10. Gọi suggest makeup sessions, verify session xung đột lịch thật sự không được trả về.
11. Dùng makeup credit vào session đã có học sinh regular trong lịch trùng, verify bị chặn.
12. Teacher tạo session report cho học sinh không thuộc session, verify bị chặn `Session.StudentNotAssigned`.
13. Session reminder chỉ gửi cho học sinh có regular assignment của session, không gửi cho học sinh không thuộc session đó.

---

## 15. Ghi chú cuối cùng

- Đợt thay đổi này đã đổi logic xuống session-level assignment, đây là thay đổi lớn nhất của flow
- Finance / invoice chưa được sửa trong đợt này
- Legacy fallback vẫn tồn tại để tránh vỡ dữ liệu cũ, nhưng dữ liệu mới nên đi theo `StudentSessionAssignments`
