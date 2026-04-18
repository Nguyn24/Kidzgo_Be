# Tài Liệu API FE - Placement Result & Registration Assign Class - 2026-04-18

Tài liệu này mô tả 3 API FE đang cần dùng:

- `PUT /api/placement-tests/{id}/results`: cập nhật kết quả placement test.
- `GET /api/registrations/{id}`: xem chi tiết ghi danh.
- `POST /api/registrations/{id}/assign-class`: xếp lớp cho ghi danh.

## Tổng quan role và phạm vi dữ liệu

Tất cả API trong tài liệu này yêu cầu user đã đăng nhập.

| Role | Dữ liệu được xem/chỉnh | Phạm vi dữ liệu | Hành động được phép |
| --- | --- | --- | --- |
| Admin | Placement test, registration, class assignment | `all` | `view`, `edit_result`, `assign_class` |
| ManagementStaff | Placement test, registration, class assignment | `all` | `view`, `edit_result`, `assign_class` |
| Teacher | Không được truy cập các API này | `none` | `none` |
| AccountantStaff | Không được truy cập các API này | `none` | `none` |
| Parent | Không được truy cập các API này | `none` | `none` |
| Student | Không được truy cập các API này | `none` | `none` |
| Anonymous | Không được truy cập | `none` | `none` |

Ghi chú:

- Hiện tại các API này đang dùng scope `all` cho `Admin` và `ManagementStaff`.
- Chưa có filter theo `own` hoặc `department` trong 3 endpoint này.
- `Teacher` không được cập nhật kết quả placement test, không xem registration detail, và không assign class qua các API này.

## Định dạng response chung

Success từ `MatchOk()` được bọc trong `ApiResult<T>`:

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
  "title": "Registration.NotFound",
  "status": 404,
  "detail": "Registration was not found"
}
```

Một số lỗi request/model binding có thể trả `400 Bad Request` theo format mặc định của ASP.NET Core.

## Danh sách API

### 1. PUT `/api/placement-tests/{id}/results`

Dùng để cập nhật điểm/kết quả của placement test, chương trình đề xuất và file/link đính kèm kết quả.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Hành động: `edit_result`

Route params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Id của placement test cần cập nhật kết quả. |

Body JSON:

```json
{
  "listeningScore": 8.5,
  "speakingScore": 8,
  "readingScore": 7.5,
  "writingScore": 7,
  "resultScore": 31,
  "programRecommendationId": "11111111-1111-1111-1111-111111111111",
  "secondaryProgramRecommendationId": "22222222-2222-2222-2222-222222222222",
  "secondaryProgramSkillFocus": "Speaking",
  "attachmentUrl": [
    "https://cdn.example.com/result-1.pdf",
    "https://cdn.example.com/result-2.pdf"
  ]
}
```

Các field trong body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `listeningScore` | `decimal?` | No | Điểm nghe. |
| `speakingScore` | `decimal?` | No | Điểm nói. |
| `readingScore` | `decimal?` | No | Điểm đọc. |
| `writingScore` | `decimal?` | No | Điểm viết. |
| `resultScore` | `decimal?` | No | Tổng điểm/kết quả cuối. |
| `programRecommendationId` | `Guid?` | No | Chương trình chính được đề xuất. Gửi `00000000-0000-0000-0000-000000000000` để xóa đề xuất chính. |
| `secondaryProgramRecommendationId` | `Guid?` | No | Chương trình phụ được đề xuất. Gửi `00000000-0000-0000-0000-000000000000` để xóa đề xuất phụ và `secondaryProgramSkillFocus`. |
| `secondaryProgramSkillFocus` | `string?` | No | Kỹ năng tập trung của chương trình phụ. |
| `attachmentUrl` | `string` hoặc `array<string>?` | No | Link đính kèm kết quả. FE có thể gửi 1 string hoặc mảng nhiều URL. |

Ví dụ gửi 1 link:

```json
{
  "resultScore": 31,
  "attachmentUrl": "https://cdn.example.com/result.pdf"
}
```

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "listeningScore": 8.5,
    "speakingScore": 8,
    "readingScore": 7.5,
    "writingScore": 7,
    "resultScore": 31,
    "programRecommendationId": "11111111-1111-1111-1111-111111111111",
    "programRecommendationName": "Kidzgo Starter",
    "secondaryProgramRecommendationId": "22222222-2222-2222-2222-222222222222",
    "secondaryProgramRecommendationName": "Kidzgo Speaking",
    "secondaryProgramSkillFocus": "Speaking",
    "attachmentUrl": "https://cdn.example.com/result-1.pdf",
    "attachmentUrls": [
      "https://cdn.example.com/result-1.pdf",
      "https://cdn.example.com/result-2.pdf"
    ],
    "status": "Completed",
    "updatedAt": "2026-04-18T08:00:00Z",
    "newRegistrationId": null
  }
}
```

Ghi chú response:

- `attachmentUrl` là URL đầu tiên để tương thích FE cũ.
- `attachmentUrls` là danh sách đầy đủ các URL.
- `newRegistrationId` chỉ có giá trị trong một số luồng retake placement test khi BE tự tạo registration mới.

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | Model binding error | `attachmentUrl` không phải string hoặc array string. |
| 400 | `PlacementTest.SecondaryProgramDuplicated` | Chương trình chính và chương trình phụ đề xuất trùng nhau. |
| 404 | `PlacementTest.NotFound` | Không tìm thấy placement test theo `id`. |
| 404 | `PlacementTest.ProgramRecommendationNotFound` | `programRecommendationId` không tồn tại. |
| 404 | `PlacementTest.SecondaryProgramRecommendationNotFound` | `secondaryProgramRecommendationId` không tồn tại. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

Luồng xử lý chính:

1. FE cập nhật điểm và các thông tin đề xuất.
2. Nếu đủ 5 điểm `listeningScore`, `speakingScore`, `readingScore`, `writingScore`, `resultScore`, BE chuyển placement test sang `Completed`.
3. Nếu placement test liên kết lead/lead child đang ở trạng thái booked test, BE cập nhật tiến trình sang đã làm test.
4. Với placement test retake, nếu đủ điều kiện, BE có thể tạo registration mới và trả về `newRegistrationId`.

### 2. GET `/api/registrations/{id}`

Dùng để xem chi tiết một ghi danh, bao gồm thông tin học sinh, chương trình, lớp đã xếp, số buổi, lịch học thực tế và buổi học đầu tiên dự kiến.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Hành động: `view`

Route params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Id của registration cần xem chi tiết. |

Response thành công:

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
    "programName": "Kidzgo Starter",
    "secondaryProgramId": null,
    "secondaryProgramName": null,
    "secondaryProgramSkillFocus": null,
    "tuitionPlanId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
    "tuitionPlanName": "48 sessions",
    "registrationDate": "2026-04-17T03:00:00Z",
    "expectedStartDate": "2026-04-20T00:00:00Z",
    "actualStartDate": "2026-04-20T10:00:00Z",
    "preferredSchedule": "Mon/Wed 17:00",
    "note": "Student prefers evening class",
    "status": "Studying",
    "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
    "className": "ST01",
    "entryType": "Immediate",
    "secondaryClassId": null,
    "secondaryClassName": null,
    "secondaryEntryType": null,
    "totalSessions": 48,
    "usedSessions": 2,
    "remainingSessions": 46,
    "originalRegistrationId": null,
    "operationType": "Initial",
    "firstStudySession": {
      "sessionId": "ffffffff-ffff-ffff-ffff-ffffffffffff",
      "classEnrollmentId": "99999999-9999-9999-9999-999999999999",
      "track": "primary",
      "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
      "className": "ST01",
      "plannedDatetime": "2026-04-20T10:00:00Z",
      "studyDate": "2026-04-20"
    },
    "actualStudySchedules": [
      {
        "track": "primary",
        "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
        "className": "ST01",
        "programId": "11111111-1111-1111-1111-111111111111",
        "programName": "Kidzgo Starter",
        "usesClassDefaultSchedule": true,
        "classSchedulePattern": "Mon,Wed",
        "sessionSelectionPattern": null,
        "effectiveSchedulePattern": "Mon,Wed",
        "studyDayCodes": ["Mon", "Wed"],
        "studyDays": ["Thứ 2", "Thứ 4"],
        "studyDaySummary": "Thứ 2, Thứ 4",
        "scheduleSegments": []
      }
    ],
    "createdAt": "2026-04-17T03:00:00Z",
    "updatedAt": "2026-04-18T08:00:00Z"
  }
}
```

Field quan trọng cho FE:

| Field | Type | Mô tả |
| --- | --- | --- |
| `status` | `string` | Trạng thái registration hiện tại. |
| `classId`, `className`, `entryType` | nullable | Lớp chính đã xếp và kiểu vào lớp. |
| `secondaryClassId`, `secondaryClassName`, `secondaryEntryType` | nullable | Lớp phụ nếu registration có chương trình phụ. |
| `totalSessions`, `usedSessions`, `remainingSessions` | `int` | Tổng buổi, đã học, còn lại. |
| `firstStudySession` | object? | Buổi học đầu tiên bé cần đi học theo lịch đã assign. |
| `actualStudySchedules` | array | Lịch học thực tế theo từng track `primary`/`secondary`. |

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Không tìm thấy registration theo `id`. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

Luồng xử lý chính:

- API chỉ đọc dữ liệu, không làm thay đổi trạng thái.
- `firstStudySession` được lấy từ session assignment sớm nhất còn active của registration.
- Nếu chưa assign class hoặc chưa có session assignment hợp lệ, `firstStudySession` là `null`.

### 3. POST `/api/registrations/{id}/assign-class`

Dùng để xếp lớp cho registration. API hỗ trợ xếp lớp chính/phụ, vào học ngay, học bù/retake hoặc đưa vào trạng thái chờ lớp.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Hành động: `assign_class`

Route params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Id của registration cần xếp lớp. |

Body JSON:

```json
{
  "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "entryType": "immediate",
  "track": "primary",
  "firstStudyDate": "2026-04-27",
  "sessionSelectionPattern": "Mon,Wed"
}
```

Các field trong body:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `classId` | `Guid?` | Required nếu không phải `wait` | null | Lớp cần assign. Với `entryType = wait`, có thể không gửi. |
| `entryType` | `string` | No | `immediate` | `immediate`, `makeup`, `retake`, `wait`. |
| `track` | `string` | No | `primary` | `primary` hoặc `secondary`. |
| `firstStudyDate` | `date?` | No | null | Ngày học đầu tiên phụ huynh/học sinh muốn bắt đầu. Không dùng với `wait`. |
| `sessionSelectionPattern` | `string?` | No | null | Pattern chọn buổi học trong tuần, ví dụ `Mon,Wed`. Dùng khi cần chọn một phần lịch lớp hoặc chương trình phụ có lịch riêng. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "registrationStatus": "Studying",
    "classId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
    "classCode": "ST01",
    "classTitle": "Starter 01",
    "track": "primary",
    "entryType": "Immediate",
    "classAssignedDate": "2026-04-18T08:00:00Z",
    "firstStudyDate": "2026-04-27",
    "firstStudySessionAt": "2026-04-27T10:00:00Z",
    "warningMessage": null
  }
}
```

Ví dụ assign vào trạng thái chờ lớp:

```json
{
  "entryType": "wait",
  "track": "primary"
}
```

Response khi chờ lớp:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "registrationStatus": "WaitingForClass",
    "classId": "00000000-0000-0000-0000-000000000000",
    "classCode": "",
    "classTitle": "",
    "track": "primary",
    "entryType": "Wait",
    "classAssignedDate": "2026-04-18T08:00:00Z",
    "firstStudyDate": null,
    "firstStudySessionAt": null,
    "warningMessage": null
  }
}
```

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `Registration.InvalidStatus` | Registration đã `Completed`/`Cancelled`, hoặc thao tác assign không hợp lệ với trạng thái hiện tại. |
| 400 | `Registration.SecondaryProgramMissing` | `track = secondary` nhưng registration không có chương trình phụ. |
| 400 | `Registration.ClassAlreadyAssigned` | Track đã có lớp, cần dùng API transfer class thay vì assign mới. |
| 400 | `Registration.FirstStudyDateNotAllowed` | Gửi `firstStudyDate` khi `entryType = wait`. |
| 400 | `Registration.ClassIdRequired` | Không gửi `classId` khi entry type cần lớp. |
| 400 | `Registration.ClassNotMatchingProgram` | Lớp không thuộc chương trình cần assign của registration. |
| 400 | `Enrollment.SessionSelectionPatternInvalid` | `sessionSelectionPattern` sai format. |
| 400 | `Enrollment.SessionSelectionPatternEmpty` | Pattern không có ngày học hợp lệ. |
| 400 | `Enrollment.SessionSelectionPatternMismatch` | Pattern không khớp lịch lớp. |
| 400 | `Registration.FirstStudyDateInPast` | `firstStudyDate` nhỏ hơn ngày hiện tại theo giờ Việt Nam. |
| 400 | `Registration.FirstStudyDateBeforeClassStart` | `firstStudyDate` trước ngày bắt đầu lớp. |
| 400 | `Registration.FirstStudyDateAfterClassEnd` | `firstStudyDate` sau ngày kết thúc lớp. |
| 400 | `Registration.FirstStudyDateNoSession` | Ngày mong muốn không có buổi học hợp lệ trong lớp/pattern đã chọn. |
| 400 | `ClassNotAvailable` | Lớp đã `Completed`, `Cancelled` hoặc `Suspended`. |
| 400 | `Registration.ClassFull` | Lớp đã đủ số lượng học sinh. |
| 404 | `Registration.NotFound` | Không tìm thấy registration. |
| 404 | `Registration.ClassNotFound` | Không tìm thấy lớp theo `classId`. |
| 409 | `AlreadyEnrolled` | Học sinh đã active trong lớp đó. |
| 409 | `Enrollment.StudentScheduleConflict` | Lịch học bị trùng với lớp/lịch học khác của học sinh. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

Luồng xử lý chính:

1. FE chọn registration và gọi assign class.
2. Nếu `entryType = wait`, BE chuyển registration về `WaitingForClass`, không tạo enrollment và không nhận `firstStudyDate`.
3. Nếu assign vào lớp, BE kiểm tra lớp có tồn tại, đúng chương trình, còn hoạt động, còn chỗ, và không trùng lịch học sinh.
4. Nếu có `firstStudyDate`, BE chỉ cho phép ngày đó nếu lớp có session hợp lệ đúng ngày này.
5. BE tạo `ClassEnrollment` và sync `StudentSessionAssignments` từ ngày bắt đầu hợp lệ.
6. Với `entryType = immediate`, registration chuyển sang `Studying`.
7. Với `entryType = makeup` hoặc `retake`, registration chuyển sang `ClassAssigned`.

## Status definition

### PlacementTestStatus

| Status | Ý nghĩa |
| --- | --- |
| `Scheduled` | Đã đặt lịch placement test. |
| `NoShow` | Học sinh không đến làm test. |
| `Completed` | Đã có kết quả placement test. |
| `Cancelled` | Placement test đã bị hủy. |

Luồng chuyển trạng thái liên quan API result:

| Từ status | Điều kiện | Sang status |
| --- | --- | --- |
| `Scheduled`/`NoShow` | Cập nhật đủ 5 điểm: listening, speaking, reading, writing, result | `Completed` |
| `Completed` | Cập nhật lại điểm/kết quả | `Completed` |
| `Cancelled` | Không có transition riêng trong API result | Giữ nguyên theo validation/business hiện tại |

### RegistrationStatus

| Status | Ý nghĩa |
| --- | --- |
| `New` | Registration mới tạo, chưa xếp lớp. |
| `WaitingForClass` | Đang chờ lớp phù hợp. |
| `ClassAssigned` | Đã xếp lớp nhưng chưa chuyển sang đang học. |
| `Studying` | Học sinh đang học. |
| `Paused` | Ghi danh/học tập đang được bảo lưu. |
| `Completed` | Đã hoàn tất khóa/chương trình. |
| `Cancelled` | Registration đã hủy. |

Luồng chuyển trạng thái liên quan assign class:

| Từ status | Hành động | Sang status |
| --- | --- | --- |
| `New`/`WaitingForClass` | `entryType = wait` | `WaitingForClass` |
| `New`/`WaitingForClass` | `entryType = immediate` | `Studying` |
| `New`/`WaitingForClass` | `entryType = makeup` hoặc `retake` | `ClassAssigned` |
| `Completed`/`Cancelled` | Assign class | Không cho phép, trả `Registration.InvalidStatus` |

### EntryType

| EntryType | Ý nghĩa |
| --- | --- |
| `Immediate` | Vào học ngay theo lịch lớp. |
| `Makeup` | Vào lớp theo dạng học bù/bổ sung. |
| `Retake` | Vào lớp theo luồng học lại/retake. |
| `Wait` | Chưa xếp lớp, đưa về chờ lớp. |

### EnrollmentStatus

| Status | Ý nghĩa |
| --- | --- |
| `Active` | Enrollment đang hoạt động. |
| `Paused` | Enrollment đang bảo lưu/tạm ngưng. |
| `Dropped` | Enrollment đã dừng/rút khỏi lớp. |

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
| `id` phải tồn tại | Tất cả | 404 theo domain tương ứng |
| `attachmentUrl` phải là string hoặc array string | Placement result | 400 model binding error |
| Chương trình đề xuất phải tồn tại | Placement result | 404 `PlacementTest.ProgramRecommendationNotFound` |
| Chương trình phụ đề xuất phải tồn tại | Placement result | 404 `PlacementTest.SecondaryProgramRecommendationNotFound` |
| Chương trình chính và phụ không được trùng nhau | Placement result | 400 `PlacementTest.SecondaryProgramDuplicated` |
| Registration không được `Completed` hoặc `Cancelled` khi assign | Assign class | 400 `Registration.InvalidStatus` |
| `track = secondary` cần registration có chương trình phụ | Assign class | 400 `Registration.SecondaryProgramMissing` |
| Track đã có lớp thì không assign lại bằng API này | Assign class | 400 `Registration.ClassAlreadyAssigned` |
| `entryType = wait` không được gửi `firstStudyDate` | Assign class | 400 `Registration.FirstStudyDateNotAllowed` |
| Assign vào lớp cần `classId` | Assign class | 400 `Registration.ClassIdRequired` |
| Lớp phải tồn tại | Assign class | 404 `Registration.ClassNotFound` |
| Lớp phải đúng chương trình của registration/track | Assign class | 400 `Registration.ClassNotMatchingProgram` |
| Lớp phải còn hoạt động | Assign class | 400 `ClassNotAvailable` |
| Lớp phải còn slot | Assign class | 400 `Registration.ClassFull` |
| Học sinh chưa active trong lớp đó | Assign class | 409 `AlreadyEnrolled` |
| Lịch học sinh không được trùng lịch khác | Assign class | 409 `Enrollment.StudentScheduleConflict` |
| `sessionSelectionPattern` phải hợp lệ và khớp lịch lớp | Assign class | 400 `Enrollment.SessionSelectionPatternInvalid`/`Empty`/`Mismatch` |
| `firstStudyDate` không được là ngày quá khứ | Assign class | 400 `Registration.FirstStudyDateInPast` |
| `firstStudyDate` phải nằm trong thời gian lớp | Assign class | 400 `Registration.FirstStudyDateBeforeClassStart` hoặc `Registration.FirstStudyDateAfterClassEnd` |
| `firstStudyDate` phải đúng ngày có session hợp lệ | Assign class | 400 `Registration.FirstStudyDateNoSession` |

## Lưu ý cho FE

- Với placement result, FE nên đọc `attachmentUrls` để hiển thị nhiều file. `attachmentUrl` chỉ là field tương thích ngược.
- Với registration detail, nếu cần hiển thị "ngày đầu tiên bé đi học", dùng `firstStudySession.studyDate` hoặc `firstStudySession.plannedDatetime`.
- Với assign class, nếu phụ huynh chọn ngày bắt đầu cụ thể, gửi `firstStudyDate`. BE sẽ chỉ nhận nếu đúng ngày có buổi học trong lớp đã chọn.
- Nếu FE chỉ muốn xếp lớp gần nhất theo lịch lớp, không cần gửi `firstStudyDate`.
- Nếu API assign báo `Registration.ClassAlreadyAssigned`, FE nên chuyển sang luồng transfer class thay vì gọi assign lại.
