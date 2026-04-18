# Tài Liệu API FE - Placement Result Và Registration Detail/Assign Class - 2026-04-18

Tài liệu này mô tả 3 API FE đang cần dùng:

- `PUT /api/placement-tests/{id}/results`
- `GET /api/registrations/{id}`
- `POST /api/registrations/{id}/assign-class`

Các API đều trả success qua `MatchOk()`, được bọc trong `ApiResult<T>`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error từ domain result trả về dạng ProblemDetails:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Registration.FirstStudyDateNoSession",
  "status": 400,
  "detail": "FirstStudyDate must match an available class session for the selected class and schedule pattern."
}
```

## Tổng quan role và phạm vi dữ liệu

Các endpoint trong tài liệu này đều yêu cầu user đăng nhập và role hợp lệ.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động được phép |
| --- | --- | --- | --- |
| Admin | Placement test result, registration detail, class assignment | `all` | `view`, `edit`, `assign_class`, `complete_result` |
| ManagementStaff | Placement test result, registration detail, class assignment | `all` | `view`, `edit`, `assign_class`, `complete_result` |
| Teacher | Không được truy cập các API này | `none` | `none` |
| AccountantStaff | Không được truy cập các API này | `none` | `none` |
| Parent | Không được truy cập các API này | `none` | `none` |
| Student | Không được truy cập các API này | `none` | `none` |
| Anonymous | Không được truy cập | `none` | `none` |

Ghi chú:

- Hiện tại các API này chưa enforce scope `own` hoặc `department`.
- `Admin` và `ManagementStaff` đang thao tác trên phạm vi `all`.
- `GET /api/registrations/{id}` trả thông tin học vụ theo registration, gồm lớp chính/phụ nếu có.
- `PUT /api/placement-tests/{id}/results` có thể làm placement test chuyển sang `Completed` khi đủ điểm.
- `POST /api/registrations/{id}/assign-class` tạo `ClassEnrollment` cho track được chọn và sync các `StudentSessionAssignment` tương ứng.

## Danh sách API

### 1. PUT `/api/placement-tests/{id}/results`

Dùng để cập nhật kết quả placement test, điểm từng kỹ năng, chương trình đề xuất và file đính kèm kết quả.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Hành động: `edit`, `complete_result`

Path params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | ID của placement test cần cập nhật kết quả. |

Body JSON:

```json
{
  "listeningScore": 8,
  "speakingScore": 7.5,
  "readingScore": 8,
  "writingScore": 7,
  "resultScore": 7.6,
  "programRecommendationId": "11111111-1111-1111-1111-111111111111",
  "secondaryProgramRecommendationId": "22222222-2222-2222-2222-222222222222",
  "secondaryProgramSkillFocus": "Speaking",
  "attachmentUrl": [
    "https://cdn.example.com/placement/result-1.pdf",
    "https://cdn.example.com/placement/result-2.jpg"
  ]
}
```

Body fields:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `listeningScore` | `decimal?` | No | Điểm Listening. Nếu không gửi thì giữ nguyên. |
| `speakingScore` | `decimal?` | No | Điểm Speaking. Nếu không gửi thì giữ nguyên. |
| `readingScore` | `decimal?` | No | Điểm Reading. Nếu không gửi thì giữ nguyên. |
| `writingScore` | `decimal?` | No | Điểm Writing. Nếu không gửi thì giữ nguyên. |
| `resultScore` | `decimal?` | No | Điểm tổng. Nếu không gửi thì giữ nguyên. |
| `programRecommendationId` | `Guid?` | No | Chương trình chính được đề xuất. Gửi `Guid.Empty` để clear. |
| `secondaryProgramRecommendationId` | `Guid?` | No | Chương trình phụ được đề xuất. Gửi `Guid.Empty` để clear cả chương trình phụ và `secondaryProgramSkillFocus`. |
| `secondaryProgramSkillFocus` | `string?` | No | Skill focus cho chương trình phụ. Chỉ update khi có secondary recommendation hoặc placement test đã có secondary recommendation. |
| `attachmentUrl` | `string` hoặc `array<string>` | No | Link file kết quả. Hỗ trợ gửi 1 string hoặc nhiều URL. Gửi `[]` để clear attachment. Nếu không gửi thì giữ nguyên. |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "listeningScore": 8,
    "speakingScore": 7.5,
    "readingScore": 8,
    "writingScore": 7,
    "resultScore": 7.6,
    "programRecommendationId": "11111111-1111-1111-1111-111111111111",
    "programRecommendationName": "Kidz English Starter",
    "secondaryProgramRecommendationId": "22222222-2222-2222-2222-222222222222",
    "secondaryProgramRecommendationName": "Speaking Booster",
    "secondaryProgramSkillFocus": "Speaking",
    "attachmentUrl": "https://cdn.example.com/placement/result-1.pdf",
    "attachmentUrls": [
      "https://cdn.example.com/placement/result-1.pdf",
      "https://cdn.example.com/placement/result-2.jpg"
    ],
    "status": "Completed",
    "updatedAt": "2026-04-18T03:30:00Z",
    "newRegistrationId": null
  }
}
```

Ghi chú response:

- `attachmentUrl` là URL đầu tiên để backward-compatible.
- `attachmentUrls` là danh sách URL đầy đủ FE nên dùng.
- `newRegistrationId` chỉ có giá trị khi đây là retake placement test và BE tự tạo registration mới theo logic retake.
- Khi đủ `listeningScore`, `speakingScore`, `readingScore`, `writingScore`, `resultScore`, placement test sẽ chuyển sang `Completed`.

Response error:

| HTTP | Code/title | Khi nào |
| --- | --- | --- |
| 400 | `PlacementTest.SecondaryProgramDuplicated` | Program chính và program phụ giống nhau. |
| 404 | `PlacementTest.NotFound` | Không tìm thấy placement test theo `id`. |
| 404 | `PlacementTest.ProgramRecommendationNotFound` | `programRecommendationId` không tồn tại hoặc program không active. |
| 404 | `PlacementTest.SecondaryProgramRecommendationNotFound` | `secondaryProgramRecommendationId` không tồn tại hoặc program không active. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

### 2. GET `/api/registrations/{id}`

Dùng để lấy chi tiết registration, thông tin lớp đã xếp, chương trình chính/phụ, lịch học thực tế theo track và buổi học đầu tiên đã được assign cho bé.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Hành động: `view`

Path params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | ID của registration. |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "studentProfileId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "studentName": "Nguyen Van A",
    "branchId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
    "branchName": "HCM",
    "programId": "11111111-1111-1111-1111-111111111111",
    "programName": "Kidz English Starter",
    "secondaryProgramId": null,
    "secondaryProgramName": null,
    "secondaryProgramSkillFocus": null,
    "tuitionPlanId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
    "tuitionPlanName": "48 buoi",
    "registrationDate": "2026-04-17T08:00:00Z",
    "expectedStartDate": "2026-04-27T00:00:00Z",
    "actualStartDate": "2026-04-27T11:00:00Z",
    "preferredSchedule": "Thu 2 18:00",
    "note": "Phu huynh muon bat dau tu 27/04",
    "status": "Studying",
    "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
    "className": "KIDZ-A1-MON",
    "entryType": "Immediate",
    "secondaryClassId": null,
    "secondaryClassName": null,
    "secondaryEntryType": null,
    "totalSessions": 48,
    "usedSessions": 0,
    "remainingSessions": 48,
    "originalRegistrationId": null,
    "operationType": "Initial",
    "firstStudySession": {
      "sessionId": "99999999-9999-9999-9999-999999999999",
      "classEnrollmentId": "88888888-8888-8888-8888-888888888888",
      "track": "primary",
      "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
      "className": "KIDZ-A1-MON",
      "plannedDatetime": "2026-04-27T11:00:00Z",
      "studyDate": "2026-04-27"
    },
    "actualStudySchedules": [
      {
        "track": "primary",
        "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
        "className": "KIDZ-A1-MON",
        "programId": "11111111-1111-1111-1111-111111111111",
        "programName": "Kidz English Starter",
        "usesClassDefaultSchedule": true,
        "classSchedulePattern": "FREQ=WEEKLY;BYDAY=MO;BYHOUR=18;BYMINUTE=0",
        "sessionSelectionPattern": null,
        "effectiveSchedulePattern": "FREQ=WEEKLY;BYDAY=MO;BYHOUR=18;BYMINUTE=0",
        "studyDayCodes": ["MO"],
        "studyDays": ["Thu 2"],
        "studyDaySummary": "Thu 2",
        "scheduleSegments": []
      }
    ],
    "createdAt": "2026-04-17T08:00:00Z",
    "updatedAt": "2026-04-18T03:30:00Z"
  }
}
```

Response fields quan trọng:

| Field | Type | Mô tả |
| --- | --- | --- |
| `firstStudySession` | `object?` | Buổi học đầu tiên thực tế đã được assign cho bé trong registration này. Null nếu chưa xếp lớp/chưa có assignment. |
| `firstStudySession.studyDate` | `DateOnly` | Ngày đầu tiên bé phải đi học theo session assignment. |
| `firstStudySession.plannedDatetime` | `DateTime` | Thời gian bắt đầu buổi học đầu tiên. |
| `actualStudySchedules` | `array` | Lịch học thực tế theo active class enrollment, group theo track `primary`/`secondary`. |
| `actualStudySchedules[].scheduleSegments` | `array` | Các segment lịch riêng cho enrollment, thường dùng với chương trình phụ hoặc lịch thay đổi theo giai đoạn. |

Response error:

| HTTP | Code/title | Khi nào |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Không tìm thấy registration theo `id`. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

### 3. POST `/api/registrations/{id}/assign-class`

Dùng để xếp lớp cho registration theo track `primary` hoặc `secondary`. API này tạo `ClassEnrollment`, sync các buổi học cho học viên trong `StudentSessionAssignments`, và có thể cho FE chọn ngày đầu tiên bé muốn bắt đầu học bằng `firstStudyDate`.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Hành động: `assign_class`, `create_enrollment`

Path params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | ID của registration cần xếp lớp. |

Body JSON:

```json
{
  "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "entryType": "immediate",
  "track": "primary",
  "firstStudyDate": "2026-04-27",
  "sessionSelectionPattern": null
}
```

Body fields:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `classId` | `Guid?` | Required khi `entryType` khác `wait` | null | Lớp cần xếp. Với `wait`, có thể null. |
| `entryType` | `string` | No | `immediate` | `immediate`, `makeup`, `wait`, `retake`. |
| `track` | `string` | No | `primary` | `primary` hoặc `secondary`. |
| `firstStudyDate` | `DateOnly?` | No | null | Ngày đầu tiên bé muốn học lớp này. Nếu gửi, ngày phải có session hợp lệ của lớp và match `sessionSelectionPattern`. Các session trước ngày này sẽ không được assign. |
| `sessionSelectionPattern` | `string?` | No | null | RRULE subset của lịch lớp cho học viên này. Nếu null thì học tất cả buổi theo lịch lớp. |

Ví dụ chọn bé bắt đầu học từ ngày `27/04` thay vì buổi gần nhất `20/04`:

```json
{
  "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "entryType": "immediate",
  "track": "primary",
  "firstStudyDate": "2026-04-27"
}
```

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "registrationStatus": "Studying",
    "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
    "classCode": "KIDZ-A1-MON",
    "classTitle": "KIDZ A1 Monday",
    "track": "primary",
    "entryType": "Immediate",
    "classAssignedDate": "2026-04-18T03:30:00Z",
    "firstStudyDate": "2026-04-27",
    "firstStudySessionAt": "2026-04-27T11:00:00Z",
    "warningMessage": "Lop da bat dau. Hoc vien se tham gia giua chung."
  }
}
```

Business behavior:

- `entryType = immediate`: tạo active enrollment, registration thường chuyển `Studying`.
- `entryType = makeup`: tạo active enrollment, registration thường chuyển `ClassAssigned`.
- `entryType = retake`: tạo active enrollment cho luồng retake, registration thường chuyển `ClassAssigned`.
- `entryType = wait`: không tạo enrollment, registration chuyển/giữ `WaitingForClass`.
- `firstStudyDate` nếu được gửi sẽ trở thành `ClassEnrollment.EnrollDate`.
- BE sync assignment chỉ cho session có `sessionDate >= EnrollDate`.
- Với chương trình phụ (`Program.IsSupplementary = true`), BE tạo thêm `ClassEnrollmentScheduleSegment` với `EffectiveFrom = EnrollDate`.

Response error:

| HTTP | Code/title | Khi nào |
| --- | --- | --- |
| 400 | `Registration.InvalidStatus` | Registration đã `Completed`/`Cancelled`, hoặc đổi track đã assign về `wait`. |
| 400 | `Registration.SecondaryProgramMissing` | `track = secondary` nhưng registration không có secondary program. |
| 400 | `Registration.ClassAlreadyAssigned` | Track đã có lớp, cần dùng transfer-class thay vì assign-class. |
| 400 | `Registration.FirstStudyDateNotAllowed` | Gửi `firstStudyDate` khi `entryType = wait`. |
| 400 | `Registration.ClassIdRequired` | `entryType` khác `wait` nhưng không gửi `classId`. |
| 404 | `Registration.ClassNotFound` | Không tìm thấy lớp theo `classId`. |
| 400 | `Registration.ClassNotMatchingProgram` | Lớp không thuộc program của track đang assign. |
| 400 | `Enrollment.SessionSelectionPatternInvalid` | `sessionSelectionPattern` không parse được. |
| 400 | `Enrollment.SessionSelectionPatternEmpty` | `sessionSelectionPattern` không match slot nào trong khoảng validate. |
| 400 | `Enrollment.SessionSelectionPatternMismatch` | `sessionSelectionPattern` không phải subset của lịch lớp. |
| 400 | `Registration.FirstStudyDateInPast` | `firstStudyDate` nhỏ hơn ngày hiện tại theo giờ Việt Nam. |
| 400 | `Registration.FirstStudyDateBeforeClassStart` | `firstStudyDate` trước ngày bắt đầu lớp. |
| 400 | `Registration.FirstStudyDateAfterClassEnd` | `firstStudyDate` sau ngày kết thúc lớp. |
| 400 | `Registration.FirstStudyDateNoSession` | Ngày chọn không có session hợp lệ của lớp hoặc không match `sessionSelectionPattern`. |
| 400 | `ClassNotAvailable` | Lớp ở trạng thái `Completed`, `Cancelled`, `Suspended`. |
| 400 | `Registration.ClassFull` | Lớp đã đủ capacity. |
| 409 | `AlreadyEnrolled` | Học viên đã có enrollment trong lớp này. |
| 409 | `Enrollment.StudentScheduleConflict` | Học viên đã có buổi học khác trùng hoặc cách dưới 15 phút. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

## Status definition

### PlacementTestStatus

| Status | Ý nghĩa | Chuyển trạng thái liên quan |
| --- | --- | --- |
| `Scheduled` | Đã đặt lịch placement test. | Có thể cập nhật kết quả. |
| `NoShow` | Học viên không đến test. | Có thể được cập nhật kết quả nếu luồng nghiệp vụ cho phép nhập điểm sau đó. |
| `Completed` | Đã hoàn tất placement test. | `PUT /results` tự chuyển sang status này khi đủ 5 điểm: listening, speaking, reading, writing, result. |
| `Cancelled` | Placement test đã hủy. | Không có logic chuyển riêng trong API `PUT /results`. |

Luồng chuyển trạng thái chính trong `PUT /api/placement-tests/{id}/results`:

1. Staff nhập điểm từng kỹ năng và điểm tổng.
2. Nếu placement test có đủ `listeningScore`, `speakingScore`, `readingScore`, `writingScore`, `resultScore`, BE set `Status = Completed`.
3. Nếu placement test gắn với lead/lead child đang `BookedTest`, BE cập nhật lead/child sang `TestDone`.
4. Nếu đây là retake placement test và đủ điều kiện, BE có thể tự tạo registration mới và trả `newRegistrationId`.

### RegistrationStatus

| Status | Ý nghĩa | API liên quan |
| --- | --- | --- |
| `New` | Registration mới tạo. | Có thể xếp lớp. |
| `WaitingForClass` | Đang chờ lớp. | Có thể xếp lớp bằng assign-class. |
| `ClassAssigned` | Đã xếp lớp nhưng chưa vào trạng thái học chính thức hoặc cần makeup/retake. | Có thể xem detail. |
| `Studying` | Đang học. | Có thể xem detail. |
| `Paused` | Bảo lưu. | Không phải luồng chính của 3 API trong tài liệu này. |
| `Completed` | Hoàn thành. | Không được assign-class. |
| `Cancelled` | Đã hủy. | Không được assign-class. |

Luồng chuyển trạng thái chính trong `POST /api/registrations/{id}/assign-class`:

1. `entryType = wait` -> registration ở `WaitingForClass`, không tạo enrollment.
2. `entryType = immediate` -> tạo enrollment, sync assignments, registration thường thành `Studying`.
3. `entryType = makeup` hoặc `retake` -> tạo enrollment, sync assignments, registration thường thành `ClassAssigned`.
4. Nếu registration có cả primary và secondary track, status cuối cùng được resolve dựa trên trạng thái assign của cả hai track.

### EntryType

| EntryType | Ý nghĩa |
| --- | --- |
| `Immediate` | Vào học ngay theo các session được assign. |
| `Makeup` | Có lớp nhưng cần học bù/bổ trợ trước khi vào lớp chính thức. |
| `Wait` | Chờ lớp, chưa tạo enrollment. |
| `Retake` | Xếp lớp sau thi lại/lên chương trình khác. |

### EnrollmentStatus

| Status | Ý nghĩa |
| --- | --- |
| `Active` | Enrollment đang hoạt động, được dùng để sync session assignments. |
| `Paused` | Enrollment đang bảo lưu. |
| `Dropped` | Enrollment đã drop khỏi lớp. |

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | AccountantStaff | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PUT /api/placement-tests/{id}/results` | Yes | Yes | No | No | No | No | No |
| `GET /api/registrations/{id}` | Yes | Yes | No | No | No | No | No |
| `POST /api/registrations/{id}/assign-class` | Yes | Yes | No | No | No | No | No |

## Validation rule tổng hợp

| Rule | API áp dụng | Kết quả khi sai |
| --- | --- | --- |
| User phải đăng nhập | Tất cả | 401 |
| Role phải là `Admin` hoặc `ManagementStaff` | Tất cả | 403 |
| Placement test phải tồn tại | PUT results | 404 `PlacementTest.NotFound` |
| Program recommendation phải active và chưa xóa | PUT results | 404 |
| Primary và secondary recommendation không được giống nhau | PUT results | 400 `PlacementTest.SecondaryProgramDuplicated` |
| `attachmentUrl` nhận string hoặc array string | PUT results | Request invalid nếu JSON không đúng kiểu. |
| Registration phải tồn tại | GET registration, assign-class | 404 `Registration.NotFound` |
| Registration không được `Completed`/`Cancelled` khi assign | assign-class | 400 `Registration.InvalidStatus` |
| `track = secondary` cần registration có secondary program | assign-class | 400 `Registration.SecondaryProgramMissing` |
| Track đã có lớp thì không assign lại | assign-class | 400 `Registration.ClassAlreadyAssigned` |
| `classId` bắt buộc khi entry type không phải `wait` | assign-class | 400 `Registration.ClassIdRequired` |
| Class phải tồn tại và thuộc đúng program | assign-class | 404 hoặc 400 |
| Class phải còn nhận học viên | assign-class | 400 `ClassNotAvailable` hoặc `Registration.ClassFull` |
| Học viên chưa được enroll trong lớp đó | assign-class | 409 `AlreadyEnrolled` |
| `sessionSelectionPattern` phải là subset của lịch lớp | assign-class | 400 |
| `firstStudyDate` chỉ dùng khi assign lớp thật | assign-class | 400 `Registration.FirstStudyDateNotAllowed` |
| `firstStudyDate` không được ở quá khứ | assign-class | 400 `Registration.FirstStudyDateInPast` |
| `firstStudyDate` phải nằm trong thời gian lớp | assign-class | 400 |
| `firstStudyDate` phải có session hợp lệ | assign-class | 400 `Registration.FirstStudyDateNoSession` |
| Lịch mới không được trùng lịch học khác của bé | assign-class | 409 `Enrollment.StudentScheduleConflict` |

## Lưu ý cho FE

- Với placement result, FE nên dùng `attachmentUrls` trong response. `attachmentUrl` chỉ là URL đầu tiên để tương thích cũ.
- Với registration detail, FE dùng `firstStudySession.studyDate` để hiển thị ngày đầu tiên bé phải đi học.
- Với assign-class, nếu phụ huynh muốn bé bắt đầu từ `27/04` thay vì buổi gần nhất `20/04`, FE gửi `firstStudyDate = "2026-04-27"`.
- Nếu FE gửi `firstStudyDate` không phải ngày có session của lớp, BE trả `Registration.FirstStudyDateNoSession`.
- `ExpectedStartDate` của registration không tự quyết định assignment. Ngày quyết định assignment trong assign-class là `firstStudyDate`, fallback về ngày hiện tại nếu không gửi.
