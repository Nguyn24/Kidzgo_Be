# Attendance, Parent PIN, Profile Image, Session Color

Tài liệu này tổng hợp trạng thái hiện tại của backend cho 4 nhóm luồng:

1. Điểm danh và cập nhật điểm danh sau 24 giờ
2. Đổi mã PIN / quên mã PIN của parent
3. Profile chưa có hình ảnh / cập nhật avatar
4. Màu session trên lịch theo chương trình học

Phạm vi tài liệu:

- Mô tả theo code backend hiện tại trong repo
- Chỉ rõ phần nào đã có
- Chỉ rõ phần nào còn thiếu hoặc chưa đúng với business expectation

Ngày chốt tài liệu: `2026-04-12`

## 1. Tổng quan role và phạm vi dữ liệu

| Role | Phạm vi dữ liệu mặc định | Ghi chú |
| --- | --- | --- |
| `Admin` | `all` | Có quyền rộng nhất. Một số endpoint admin hiện chưa gắn `[Authorize]` đúng cách ở controller. |
| `ManagementStaff` | `department` hoặc `all` tùy endpoint | Với 4 luồng trong tài liệu này, nhiều chỗ chưa có enforcement branch/department thật sự. |
| `Teacher` | `own` hoặc `all` tùy endpoint | Một số API teacher hiện chưa chặn theo lớp được phân công. |
| `Parent` | `own` | Dựa vào `userId`, `parent profile`, hoặc `studentId` đã chọn trong token. |
| `Student` | `own` | Dựa vào `studentId` trong token. |
| `AccountantStaff` | không áp dụng chính cho 4 luồng này | Không có API chuyên biệt trong 4 luồng đang xét. |

## 2. Chuẩn response chung

### 2.1. Success format

Backend chuẩn hóa theo dạng:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Hoặc:

```json
{
  "isSuccess": true,
  "data": null
}
```

### 2.2. Error format

Khi lỗi, backend trả `ProblemDetails` hoặc object lỗi tương đương, chứa tối thiểu:

- `title`
- `status`
- `detail`
- `traceId`

Một số endpoint upload file đang trả lỗi dạng đơn giản:

```json
{
  "error": "No file provided"
}
```

## 3. Luồng điểm danh

### 3.1. Business rule

#### Current backend

- Teacher và Admin được phép tạo điểm danh hàng loạt cho session.
- Teacher không được điểm danh session trong tương lai.
- Admin được bypass rule session tương lai.
- Teacher được cập nhật điểm danh trong vòng `24 giờ` sau khi session kết thúc.
- Admin được cập nhật điểm danh sau `24 giờ`.
- Khi bulk mark vắng mặt (`Absent`), hệ thống tự suy ra `AbsenceType` từ leave request:
  - `WithNotice24H`
  - `Under24H`
  - `NoNotice`
- Nếu `AbsenceType = WithNotice24H`, backend có thể tạo `MakeupCredit`.

#### Gap / lưu ý

- Teacher hiện chưa bị giới hạn theo lớp mình phụ trách trong API điểm danh; nếu biết `sessionId` thì có thể thao tác.
- `UpdateAttendance` khi sửa 1 bản ghi sang `Absent` đang gán `AbsenceType = NoNotice`, không recompute lại theo leave request.
- Không có workflow approve riêng cho sửa điểm danh sau 24h; chỉ có rule admin bypass trực tiếp.

### 3.2. Mỗi role được xem gì

| Role | Xem dữ liệu gì | Scope |
| --- | --- | --- |
| `Admin` | Danh sách điểm danh session, lịch sử điểm danh học sinh đã chọn trong token | `all` cho session attendance, `own-selected-student` cho student history |
| `Teacher` | Danh sách điểm danh session, lịch sử điểm danh học sinh nếu token có `studentId` | `all` theo `sessionId` hiện tại, chưa khóa theo ownership |
| `Parent` | Lịch sử điểm danh của học sinh đã chọn | `own` |
| `Student` | Lịch sử điểm danh của chính mình | `own` |
| `ManagementStaff` | Không có endpoint attendance trực tiếp trong controller hiện tại | `none` |

### 3.3. Các hành động được phép

| Action | Admin | Teacher | Parent | Student | ManagementStaff |
| --- | --- | --- | --- | --- | --- |
| Bulk mark attendance | Yes | Yes | No | No | No |
| View session attendance list | Yes | Yes | No | No | No |
| View own student attendance history | Yes, nếu có token student | Yes, nếu có token student | Yes | Yes | No |
| Update one attendance record trong 24h | Yes | Yes | No | No | No |
| Update one attendance record sau 24h | Yes | No | No | No | No |

### 3.4. Danh sách API

#### 3.4.1. Bulk mark attendance

- Endpoint: `POST /api/attendance/{sessionId}`
- Mục đích: Điểm danh hàng loạt cho các học sinh trong session
- Auth: `Admin`, `Teacher`

Body:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `attendances` | `array` | Yes | Danh sách học sinh cần mark |
| `attendances[].studentProfileId` | `guid` | Yes | Học sinh thuộc session |
| `attendances[].attendanceStatus` | `enum` | Yes | `Present`, `Absent`, `Makeup` |
| `attendances[].note` | `string` | No | Ghi chú |

Success:

```json
{
  "isSuccess": true,
  "data": {
    "results": [
      {
        "id": "guid",
        "sessionId": "guid",
        "studentProfileId": "guid",
        "attendanceStatus": "Present",
        "absenceType": null,
        "markedAt": "2026-04-12T03:00:00Z",
        "note": "On time"
      }
    ]
  }
}
```

Error cases:

| Code | Message |
| --- | --- |
| `Attendance.NotFound` | Session attendance record/session không tồn tại |
| `Attendance.FutureSessionNotAllowed` | Teacher không được mark session trong tương lai |
| `Attendance.StudentNotAssigned` | Học sinh không thuộc session |

#### 3.4.2. Get session attendance

- Endpoint: `GET /api/attendance/{sessionId}`
- Mục đích: Lấy danh sách điểm danh của 1 session
- Auth: `Admin`, `Teacher`

Response data chính:

- `sessionId`
- `sessionName`
- `date`
- `startTime`
- `endTime`
- `summary`
- `attendances[]`

Trong từng item hiện có:

- `studentProfileId`
- `studentName`
- `studentAvatarUrl`
- `attendanceStatus`
- `absenceType`
- `hasMakeupCredit`
- `note`
- `markedAt`

#### 3.4.3. Get student attendance history

- Endpoint: `GET /api/attendance/students?pageNumber=1&pageSize=10`
- Mục đích: Lấy lịch sử điểm danh của học sinh đang được chọn trong token
- Auth: `Admin`, `Teacher`, `Parent`, `Student`

Rule:

- Phải có `studentId` trong token
- Student profile phải thuộc `userId` hiện tại

Error cases:

| Code | Message |
| --- | --- |
| `Profile.StudentNotFound` | Không có student context hoặc student không thuộc user |

#### 3.4.4. Update one attendance

- Endpoint: `PUT /api/attendance/{sessionId}/students/{studentProfileId}`
- Mục đích: Sửa 1 bản ghi điểm danh
- Auth: `Admin`, `Teacher`

Body:

| Field | Type | Required |
| --- | --- | --- |
| `attendanceStatus` | `enum` | Yes |
| `note` | `string` | No |

Error cases:

| Code | Message |
| --- | --- |
| `Attendance.NotFound` | Không có bản ghi attendance cho session + student |
| `Attendance.FutureSessionNotAllowed` | Teacher không được sửa session tương lai |
| `Attendance.UpdateWindowClosed` | Teacher không được sửa sau 24 giờ kể từ khi session kết thúc |

### 3.5. Status definition

#### AttendanceStatus

| Status | Ý nghĩa |
| --- | --- |
| `Present` | Có mặt |
| `Absent` | Vắng mặt |
| `Makeup` | Học bù |
| `NotMarked` | Chưa được điểm danh |

#### AbsenceType

| Status | Ý nghĩa |
| --- | --- |
| `WithNotice24H` | Xin nghỉ trước ít nhất 24 giờ |
| `Under24H` | Xin nghỉ dưới 24 giờ |
| `NoNotice` | Không báo trước |
| `LongTerm` | Trạng thái enum có khai báo nhưng không thấy dùng trong flow attendance hiện tại |

### 3.6. Luồng chuyển trạng thái

```text
NotMarked -> Present / Absent / Makeup
Present <-> Absent
Present <-> Makeup
Absent <-> Makeup
```

Rule cập nhật sau session:

- `Teacher`: chỉ trong 24h sau khi session kết thúc
- `Admin`: không bị khóa bởi 24h

### 3.7. Validation rule và các trường hợp trả lỗi

- `sessionId` phải tồn tại
- `studentProfileId` phải thuộc session khi bulk mark
- Teacher không được thao tác session tương lai
- Teacher không được sửa quá 24h sau giờ kết thúc
- Student history bắt buộc có `studentId` trong token

## 4. Luồng Parent PIN

### 4.1. Business rule

#### Current backend

- Chỉ `Parent profile` mới có `PinHash` dùng ở runtime.
- Một `user` được backend giả định chỉ có 1 `Parent profile` hợp lệ để dùng trong flow PIN.
- `verify-parent-pin`:
  - Nếu chưa có PIN thì set lần đầu
  - Nếu đã có PIN thì verify
- `change-pin`:
  - Chỉ đổi PIN nếu parent profile đã có PIN và `currentPin` đúng
- `request-pin-reset`:
  - Gửi email reset link
- `request-pin-reset-zalo-otp`:
  - Tạo OTP, gửi qua Zalo OA, trả `challengeId`
- `verify-pin-reset-zalo-otp`:
  - Verify OTP, trả `resetToken`
- `reset-pin`:
  - Đặt PIN mới bằng token email hoặc token sau OTP verify
- Admin có thể set lại PIN trực tiếp cho parent profile bằng API admin

#### Gap / lưu ý

- `PUT /api/auth/change-pin` được gắn `[Authorize]` chung, nhưng thực tế chỉ parent profile dùng được; role khác sẽ fail `Profile.Invalid`.
- `api/admin/users/{profileId}/change-pin` hiện nằm trong controller chưa gắn `[Authorize]` ở cấp controller. Về business intention đây là API admin, nhưng code hiện tại là security gap.
- Không có API public riêng cho “resend OTP”.

### 4.2. Mỗi role được xem gì

| Role | Xem / dùng dữ liệu gì | Scope |
| --- | --- | --- |
| `Parent` | Parent profile hiện tại, PIN state, forgot PIN challenge | `own` |
| `Admin` | Có thể đổi PIN parent theo `profileId` qua API admin | `all` |
| `Teacher` | Không có business scope cho parent PIN | `none` |
| `Student` | Không có business scope cho parent PIN | `none` |
| `ManagementStaff` | Chưa có API riêng | `none` |

### 4.3. Các hành động được phép

| Action | Parent | Admin | Teacher | Student | ManagementStaff |
| --- | --- | --- | --- | --- | --- |
| Set/verify PIN lần đầu | Yes | No | No | No | No |
| Change PIN bằng current PIN | Yes | No | No | No | No |
| Request forgot PIN via email | Yes | No | No | No | No |
| Request forgot PIN via Zalo OTP | Yes | No | No | No | No |
| Verify Zalo OTP | Public flow | Public flow | Public flow | Public flow | Public flow |
| Reset PIN bằng token | Public flow | Public flow | Public flow | Public flow | Public flow |
| Admin set parent PIN trực tiếp | No | Intended Yes | No | No | No |

### 4.4. Danh sách API

#### 4.4.1. Verify / set parent PIN

- Endpoint: `POST /api/auth/profiles/verify-parent-pin`
- Auth: `Authorize`
- Mục đích: Set PIN lần đầu hoặc verify PIN hiện có của parent profile

Body:

| Field | Type | Required |
| --- | --- | --- |
| `profileId` | `guid` | Yes |
| `pin` | `string` | Yes |

Error:

| Code | Message |
| --- | --- |
| `Profile.Invalid` | Profile không hợp lệ hoặc không phải parent |
| `Pin.Invalid` | PIN không phải số hoặc dài >= 10 |
| `Pin.Wrong` | PIN sai |

#### 4.4.2. Change PIN

- Endpoint: `PUT /api/auth/change-pin`
- Auth: `Authorize`
- Mục đích: Parent đổi PIN bằng `currentPin`

Body:

| Field | Type | Required |
| --- | --- | --- |
| `currentPin` | `string` | Yes |
| `newPin` | `string` | Yes |

Error:

| Code | Message |
| --- | --- |
| `Profile.Invalid` | Không resolve được parent profile hiện tại |
| `Pin.NotSet` | Parent profile chưa có PIN |
| `Pin.Wrong` | Current PIN sai |
| `Pin.Invalid` | New PIN không hợp lệ |

#### 4.4.3. Request reset PIN by email

- Endpoint: `POST /api/auth/profiles/request-pin-reset`
- Auth: `Authorize`
- Mục đích: Tạo reset token và gửi link qua email

Error:

| Code | Message |
| --- | --- |
| `Profile.Invalid` | Không tìm thấy parent profile hợp lệ |
| `Profile.EmailNotSet` | User chưa có email |

#### 4.4.4. Request reset PIN by Zalo OTP

- Endpoint: `POST /api/auth/profiles/request-pin-reset-zalo-otp`
- Auth: `Authorize`
- Mục đích: Gửi OTP qua Zalo OA

Success:

```json
{
  "isSuccess": true,
  "data": {
    "challengeId": "guid",
    "otpExpiresAt": "2026-04-12T03:40:00Z"
  }
}
```

Error:

| Code | Message |
| --- | --- |
| `Profile.Invalid` | Không tìm thấy parent profile |
| `Profile.ZaloIdNotSet` | Parent profile chưa có `ZaloId` |
| `Profile.ZaloOtpSendFailed` | Gửi Zalo thất bại |

#### 4.4.5. Verify Zalo OTP

- Endpoint: `POST /api/auth/profiles/verify-pin-reset-zalo-otp`
- Auth: `AllowAnonymous`
- Mục đích: Verify OTP và nhận `resetToken`

Body:

| Field | Type | Required |
| --- | --- | --- |
| `challengeId` | `guid` | Yes |
| `otp` | `string` | Yes |

Success:

```json
{
  "isSuccess": true,
  "data": {
    "resetToken": "secure_reset_token",
    "expiresAt": "2026-04-12T04:30:00Z"
  }
}
```

Error:

| Code | Message |
| --- | --- |
| `Pin.InvalidResetToken` | Challenge không hợp lệ, đã dùng, hoặc vượt max attempts |
| `Pin.InvalidOtp` | OTP sai hoặc hết hạn |
| `Profile.Invalid` | Profile gắn với token không còn hợp lệ |

#### 4.4.6. Reset PIN

- Endpoint: `POST /api/auth/reset-pin`
- Auth: `AllowAnonymous`
- Mục đích: Đặt PIN mới

Body:

| Field | Type | Required |
| --- | --- | --- |
| `token` | `string` | Yes |
| `newPin` | `string` | Yes |

Error:

| Code | Message |
| --- | --- |
| `Pin.InvalidResetToken` | Token không hợp lệ / hết hạn / đã dùng |
| `Pin.OtpNotVerified` | Token có OTP flow nhưng chưa verify OTP |
| `Pin.Invalid` | PIN không hợp lệ |
| `Profile.Invalid` | Parent profile không hợp lệ |

#### 4.4.7. Admin change parent PIN

- Endpoint: `PUT /api/admin/users/{profileId}/change-pin`
- Mục đích: Admin set lại PIN trực tiếp cho parent profile
- Business intention: `Admin only`
- Current code status: controller chưa khóa authorize đúng mức

Body:

| Field | Type | Required |
| --- | --- | --- |
| `newPin` | `string` | Yes |

### 4.5. Status definition

#### PIN state

| State | Ý nghĩa |
| --- | --- |
| `NotSet` | Parent profile chưa có `PinHash` |
| `Set` | Parent profile đã có `PinHash` |

#### Reset challenge state

| State | Điều kiện |
| --- | --- |
| `EmailResetIssued` | Có `ParentPinResetToken`, không có OTP |
| `OtpPending` | Có `OtpCodeHash`, chưa `OtpVerifiedAt`, chưa `UsedAt` |
| `OtpVerified` | Có `OtpVerifiedAt`, chưa `UsedAt` |
| `Used` | Có `UsedAt` |
| `Expired` | `UtcNow > ExpiresAt` hoặc `UtcNow > OtpExpiresAt` tùy bước |

### 4.6. Luồng chuyển trạng thái

```text
NotSet --verify-parent-pin--> Set
Set --change-pin--> Set
Set --request-pin-reset(email)--> EmailResetIssued --reset-pin--> Used
Set --request-pin-reset-zalo-otp--> OtpPending --verify-pin-reset-zalo-otp--> OtpVerified --reset-pin--> Used
```

### 4.7. Validation rule

- PIN phải là số
- PIN phải có độ dài `< 10`
- `verify-pin-reset-zalo-otp` yêu cầu OTP đúng `6` chữ số
- Parent forgot PIN bằng email yêu cầu `user.Email`
- Parent forgot PIN bằng Zalo yêu cầu `profile.ZaloId`

## 5. Luồng profile image / avatar

### 5.1. Business rule

#### Current backend

- Avatar là optional, có thể null.
- Có thể upload avatar qua:
  - `POST /api/files/avatar`
  - `PUT /api/me` với `multipart/form-data`
- Hệ thống hỗ trợ lưu avatar ở cả `User` và `Profile`.
- Với các danh sách attendance/class student, backend thường fallback:
  - `profile.AvatarUrl ?? user.AvatarUrl`

#### Gap / lưu ý

- `GetProfileByIdResponse` hiện không trả `AvatarUrl`.
- `GetCurrentUserResponse.Profiles` hiện không trả avatar cho từng profile.
- Chưa có API riêng để lấy/chỉnh avatar của một profile cụ thể theo `profileId`.
- Chưa có “status” chính thức cho profile image; chỉ có trạng thái ngầm là `null` hoặc có URL.

### 5.2. Mỗi role được xem gì

| Role | Xem / sửa dữ liệu gì | Scope |
| --- | --- | --- |
| `Parent` | Sửa account info và avatar account/profile hiện tại | `own` |
| `Teacher` | Sửa profile/account của chính mình | `own` |
| `Student` | Không có API edit avatar riêng trong controller hiện tại | `own` read-only tùy endpoint |
| `Admin` | Có thể xem user/profile ở API admin/profile chung, nhưng profile detail chưa có avatar field | `all` |

### 5.3. Các hành động được phép

| Action | Parent | Teacher | Student | Admin | ManagementStaff |
| --- | --- | --- | --- | --- | --- |
| Upload avatar generic | Yes | Yes | Yes nếu có token | Yes | Yes |
| Update own account + avatar | Yes | Yes | Không thấy endpoint riêng | Yes qua `/api/me` | Yes qua `/api/me` |
| View avatar trong attendance list | Gián tiếp | Gián tiếp | Gián tiếp | Gián tiếp | Gián tiếp |
| View avatar trong profile detail | Chưa hỗ trợ đầy đủ | Chưa hỗ trợ đầy đủ | Chưa hỗ trợ đầy đủ | Chưa hỗ trợ đầy đủ | Chưa hỗ trợ đầy đủ |

### 5.4. Danh sách API

#### 5.4.1. Upload avatar

- Endpoint: `POST /api/files/avatar`
- Auth: `Authorize`
- Mục đích: Upload file ảnh avatar

Request:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `file` | `multipart file` | Yes | Ảnh avatar |

Validation:

- Max size ảnh: `10MB`
- Extension hợp lệ: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`, `.svg`

Error:

| Code | Message |
| --- | --- |
| `File.NoFileProvided` hoặc raw bad request | Không có file |
| `File.SizeExceedsLimit` | Quá dung lượng |
| `File.InvalidFileType` | Sai định dạng |
| `File.ParentProfileSelectionRequired` | Được định nghĩa nhưng hiện khó chạm tới trong flow phổ biến |
| `File.UploadFailed` | Upload storage thất bại |

#### 5.4.2. Update current user by multipart form

- Endpoint: `PUT /api/me`
- Auth: `Authorize`
- Mục đích: Cập nhật thông tin user hiện tại, hỗ trợ upload avatar cùng request

Form fields:

| Field | Type | Required |
| --- | --- | --- |
| `fullName` | `string` | No |
| `email` | `string` | No |
| `phoneNumber` | `string` | No |
| `avatarUrl` | `string` | No |
| `avatar` | `file` | No |
| `profiles` | `list` | No |

Validation:

- Email phải đúng format
- Phone phải là số điện thoại Việt Nam hợp lệ
- Avatar file rỗng sẽ bị reject ngay tại controller

#### 5.4.3. Update parent account

- Endpoint: `PUT /api/parents/account`
- Auth: `Parent`
- Mục đích: Update account info của parent
- Note: hiện nhận `AvatarUrl`, không phải multipart upload

#### 5.4.4. Update teacher profile

- Endpoint: `PUT /api/teachers/profile`
- Auth: `Teacher`
- Mục đích: Update account info của teacher
- Note: hiện nhận `AvatarUrl`, không phải multipart upload

### 5.5. Status definition

Không có enum status riêng cho profile image.

Trạng thái business nên hiểu như sau:

| State | Ý nghĩa |
| --- | --- |
| `NoImage` | `AvatarUrl = null` |
| `HasImage` | `AvatarUrl` có giá trị |

### 5.6. Validation rule và lỗi

- Ảnh phải đúng extension được phép
- Ảnh không vượt size limit
- Email unique
- Phone number unique và đúng format Việt Nam

## 6. Luồng màu session trên lịch

### 6.1. Business rule

#### Current backend

- `Session` có trường `Color`.
- Khi tạo session thủ công hoặc generate từ pattern, màu được set bằng `SessionColorPalette.GetRandomColor()`.
- Các API timetable và session list/detail đều trả về `Color`.

#### Gap so với requirement của bạn

Requirement mong muốn:

- Session thuộc các chương trình học khác nhau phải có màu khác nhau để dễ nhận biết.

Current backend chưa đáp ứng đúng vì:

- Màu đang random theo từng `session`, không theo `program`
- Cùng một program có thể ra nhiều màu khác nhau
- Hai program khác nhau có thể trùng màu ngẫu nhiên
- `Program` hiện chưa có trường `Color`
- Chưa có API cấu hình màu theo program

### 6.2. Mỗi role được xem gì

| Role | Xem dữ liệu lịch gì | Scope |
| --- | --- | --- |
| `Admin` | Session list, session detail, teacher timetable | `all` |
| `ManagementStaff` | Session list, session detail, teacher timetable | `all` hoặc filter theo branch/class |
| `Teacher` | Teacher timetable của mình; nếu endpoint cho phép còn có thể xem teacher khác qua query | `own` hoặc `all-by-query` tùy role |
| `Parent` | Không có endpoint timetable riêng trong controller này; thường đi qua student context | `own-selected-student` |
| `Student` | Student timetable | `own` |

### 6.3. Các hành động được phép

| Action | Admin | ManagementStaff | Teacher | Parent | Student |
| --- | --- | --- | --- | --- | --- |
| Create session | Yes | Yes | No | No | No |
| Update session | Yes | Yes | No | No | No |
| View session list | Yes | Yes | No | No | No |
| View session detail | Yes | Yes | Yes | Yes | Yes |
| View teacher timetable | Yes | Yes | Yes | No | No |
| View student timetable | Gián tiếp nếu có student token | Gián tiếp nếu có student token | Gián tiếp nếu có student token | Yes qua selected student token | Yes |
| Configure color by program | No API | No API | No API | No API | No API |

### 6.4. Danh sách API

#### 6.4.1. Session list

- Endpoint: `GET /api/sessions`
- Auth: `Admin`, `ManagementStaff`
- Có trả `color`

#### 6.4.2. Session detail

- Endpoint: `GET /api/sessions/{sessionId}`
- Auth: mọi user đã đăng nhập
- Có trả `session.color`

#### 6.4.3. Teacher timetable

- Endpoint: `GET /api/teachers/timetable`
- Auth: `Teacher`, `Admin`, `ManagementStaff`
- Có trả `sessions[].color`

#### 6.4.4. Student timetable

- Endpoint: `GET /api/students/timetable`
- Auth: `Authorize`
- Rule: token phải có `studentId`, student phải thuộc user hiện tại
- Có trả `sessions[].color`

### 6.5. Status definition

#### SessionStatus

| Status | Ý nghĩa |
| --- | --- |
| `Scheduled` | Đã lên lịch |
| `Completed` | Đã hoàn thành |
| `Cancelled` | Đã hủy |

### 6.6. Luồng chuyển trạng thái

```text
Scheduled -> Completed
Scheduled -> Cancelled
Completed -> (không thấy flow reopen trong code hiện tại)
Cancelled -> (không thấy flow reopen trong code hiện tại)
```

### 6.7. Validation rule và lỗi

- Session list/timetable có filter `from`, `to`, `branchId`, `classId`
- Student timetable bắt buộc student trong token phải thuộc current user
- Chưa có validation nào cho “màu theo program” vì feature chưa tồn tại

## 7. Permission matrix tổng hợp theo role

| Flow | Admin | ManagementStaff | Teacher | Parent | Student |
| --- | --- | --- | --- | --- | --- |
| Mark attendance | Yes | No | Yes | No | No |
| Update attendance > 24h | Yes | No | No | No | No |
| View session attendance | Yes | No | Yes | No | No |
| View student attendance history | Conditional | No | Conditional | Yes | Yes |
| Set/verify parent PIN | No | No | No | Yes | No |
| Change parent PIN by current PIN | No | No | No | Yes | No |
| Forgot PIN by email | No | No | No | Yes | No |
| Forgot PIN by Zalo OTP | No | No | No | Yes | No |
| Admin override parent PIN | Intended Yes | No | No | No | No |
| Upload avatar | Yes | Yes | Yes | Yes | Conditional |
| Update own account avatar | Yes | Yes | Yes | Yes | Chưa có flow riêng |
| View calendar color | Yes | Yes | Yes | Conditional | Yes |
| Set color by program | Chưa có | Chưa có | Chưa có | Chưa có | Chưa có |

`Conditional` nghĩa là phải có đúng context trong token, thường là `studentId`.

## 8. Kết luận và backlog đề xuất

### 8.1. Đã có trong backend

- Rule teacher không được sửa attendance sau 24h
- Admin được sửa attendance sau 24h
- Parent change PIN
- Parent forgot PIN bằng email
- Parent forgot PIN bằng Zalo OTP
- Upload avatar cơ bản
- Session APIs có trả `Color`

### 8.2. Thiếu hoặc chưa đúng requirement

1. Attendance scope cho teacher chưa khóa theo ownership/class assignment.
2. Admin change parent PIN endpoint chưa được khóa authorize đúng mức ở controller.
3. `GetProfileById` và `GetCurrentUser.Profiles` chưa expose avatar/profile image.
4. Session color chưa theo program, mới random per session.
5. Chưa có API quản trị mapping `Program -> Color`.
6. Chưa có resend OTP / rate limit policy public rõ ràng cho Zalo OTP.
7. `UpdateAttendance` chưa recompute `AbsenceType` từ leave request khi đổi sang `Absent`.

### 8.3. Recommendation ngắn

1. Thêm `Program.Color`.
2. Khi tạo/generate session, copy màu từ `Program.Color` thay vì random.
3. Bổ sung `AvatarUrl` vào các DTO profile cần hiển thị.
4. Siết authorize cho `api/admin/users/{profileId}/change-pin`.
5. Siết teacher attendance theo class/session ownership.
6. Tách rõ rule correction sau 24h trong audit và UI.
