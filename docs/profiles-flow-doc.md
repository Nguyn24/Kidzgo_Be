# Tài liệu full flow bảng `profiles`

Ngày cập nhật: 2026-04-07  
Phạm vi: tài liệu này mô tả bảng `profiles` và toàn bộ flow liên quan theo code hiện tại của `Kidzgo_Be`.

## 1. Tổng quan

Bảng `profiles` dùng để quản lý hồ sơ nghiệp vụ gắn với `users`.

Một `User` có thể có nhiều `Profile`, nhưng hiện `ProfileType` chỉ có 2 loại:

- `Parent`
- `Student`

`profiles` là lớp dữ liệu trung gian giữa account đăng nhập (`users`) và các flow học vụ:

- parent/student portal
- parent PIN
- chọn student đang active trong token
- leave request, attendance, makeup, invoices, media, reports
- link quan hệ parent-student

## 2. Cấu trúc dữ liệu chính

## 2.1. Các cột quan trọng

| Cột | Kiểu | Ý nghĩa |
| --- | --- | --- |
| `Id` | `Guid` | PK của profile |
| `UserId` | `Guid` | FK về `users` |
| `ProfileType` | `enum` | `Parent` hoặc `Student` |
| `DisplayName` | `string` | Tên hiển thị chính |
| `Name` | `string?` | Họ tên |
| `Gender` | `enum?` | Giới tính |
| `DateOfBirth` | `DateOnly?` | Ngày sinh |
| `ZaloId` | `string?` | Zalo ID |
| `PinHash` | `string?` | PIN hash của profile, chủ yếu dùng cho Parent |
| `AvatarUrl` | `string?` | Avatar của profile |
| `IsApproved` | `bool` | Hồ sơ đã được duyệt hay chưa |
| `IsActive` | `bool` | Hồ sơ có đang active để sử dụng hay không |
| `IsDeleted` | `bool` | Soft delete |
| `LastLoginAt` | `DateTime?` | Lần login cuối của profile |
| `LastSeenAt` | `DateTime?` | Lần hoạt động cuối của profile |
| `CreatedAt` | `DateTime` | Thời điểm tạo |
| `UpdatedAt` | `DateTime` | Thời điểm cập nhật cuối |

## 2.2. Quan hệ chính

| Quan hệ | Loại |
| --- | --- |
| `Profile -> User` | N:1 |
| `ParentProfile -> ParentStudentLink -> StudentProfile` | N:N logic |
| `Profile -> ClassEnrollments` | 1:N |
| `Profile -> LeaveRequests` | 1:N |
| `Profile -> Attendances` | 1:N |
| `Profile -> MakeupCredits` | 1:N |
| `Profile -> Invoices` | 1:N |
| `Profile -> Notifications / Tickets / Media / Reports / Gamification...` | 1:N |

## 3. State definition của `profiles`

## 3.1. ProfileType

| Giá trị | Ý nghĩa |
| --- | --- |
| `Parent` | Hồ sơ phụ huynh |
| `Student` | Hồ sơ học sinh |

## 3.2. Trạng thái logic theo tổ hợp cờ

| `IsApproved` | `IsActive` | `IsDeleted` | Ý nghĩa thực tế |
| --- | --- | --- | --- |
| `false` | `false` | `false` | Hồ sơ mới tạo, chưa được duyệt |
| `true` | `false` | `false` | Hồ sơ đã duyệt nhưng chưa active sử dụng |
| `true/false` | `true` | `false` | Hồ sơ đang usable |
| `true/false` | `false` | `true` | Hồ sơ đã soft delete |

## 3.3. Luồng chuyển trạng thái hiện tại

1. `CreateProfile`
   - tạo profile với:
     - `IsApproved = false`
     - `IsActive = false`
     - `IsDeleted = false`
2. `ApproveProfile`
   - set `IsApproved = true`
   - chưa tự set `IsActive = true`
3. `ReactivateProfile`
   - set `IsDeleted = false`
   - set `IsActive = true`
   - áp dụng cho tất cả profile `IsApproved = true` cùng `UserId`
4. `DeleteProfile`
   - set `IsDeleted = true`
   - set `IsActive = false`
5. `UpdateProfile`
   - có thể chỉnh `DisplayName`
   - có thể chỉnh `IsActive`

## 4. Full flow nghiệp vụ

## 4.1. Flow tạo profile

1. Gọi `POST /api/profiles`
2. Hệ thống kiểm tra `UserId` có tồn tại
3. Trim `DisplayName`, `FullName`
4. Nếu có `PinHash` thì hash trước khi lưu
5. Tạo record mới ở trạng thái chưa duyệt, chưa active
6. Nếu là `Student`:
   - thử auto-fill `Name`, `Gender`, `DateOfBirth` từ `LeadChild`
7. Nếu là `Parent`:
   - thử auto-fill `Name`, `ZaloId` từ `Lead`
8. Tự động link với các profile cùng `UserId` khác type đang tồn tại
9. Lưu DB

## 4.2. Flow approve profile

1. Gọi `PUT /api/admin/users/approve`
2. Input là list `ProfileId`
3. Handler chia thành 3 nhóm:
   - `NotFound`
   - `AlreadyApproved`
   - `IdsToApprove`
4. Với profile cần duyệt:
   - set `IsApproved = true`
   - update `UpdatedAt`
5. Group theo `UserId`
6. Publish `ProfileCreatedDomainEvent`
7. Email dùng template `PROFILE_CREATED`
8. Email chứa link:
   - `/api/profiles/{profileId}/reactivate-and-update`

## 4.3. Flow reactivate từ email

1. User mở link `/api/profiles/{id}/reactivate-and-update`
2. Backend gọi `ReactivateProfileCommand`
3. Nếu lỗi khác `Profile.ProfileNotDeleted` thì trả lỗi
4. Nếu thành công hoặc profile vốn chưa deleted, backend redirect sang frontend:
   - `{FrontendUrl}/profile/update?profileId={id}`

Ghi chú:

- `ReactivateProfileCommandHandler` không chỉ reactivate một profile.
- Nó đang bật lại tất cả profile cùng `UserId` mà `IsApproved = true`.

## 4.4. Flow soft delete profile

1. Gọi `DELETE /api/profiles/{id}`
2. Nếu profile tồn tại:
   - `IsDeleted = true`
   - `IsActive = false`
   - cập nhật `UpdatedAt`

## 4.5. Flow link parent-student

1. Gọi `POST /api/profiles/link`
2. Validate parent profile:
   - type `Parent`
   - `!IsDeleted`
   - `IsActive`
3. Validate student profile:
   - type `Student`
   - `!IsDeleted`
   - `IsActive`
4. Kiểm tra link đã tồn tại chưa
5. Tạo `ParentStudentLink`

## 4.6. Flow unlink parent-student

1. Gọi `POST /api/profiles/unlink`
2. Tìm link theo `ParentProfileId + StudentProfileId`
3. Nếu có thì xóa link

## 4.7. Flow parent PIN

### Verify / set parent PIN

1. Gọi `POST /api/auth/profiles/verify-parent-pin`
2. Profile phải:
   - thuộc current user
   - là `Parent`
   - `!IsDeleted`
   - `IsActive`
3. PIN phải:
   - toàn số
   - độ dài < 10
4. Nếu `PinHash` đang null:
   - set PIN lần đầu
5. Nếu đã có `PinHash`:
   - verify PIN

### Request parent PIN reset

1. Gọi `POST /api/auth/profiles/request-pin-reset`
2. Profile phải hợp lệ như flow verify
3. User phải có email
4. Raise `ParentPinResetRequestDomainEvent`
5. Handler email:
   - lấy template `PARENT_PIN_RESET`
   - xóa token reset cũ chưa dùng
   - tạo token mới hết hạn sau 1 giờ
   - gửi mail reset link

### Admin đổi parent PIN

1. Gọi `PUT /api/admin/users/{profileId}/change-pin`
2. Profile phải tồn tại và là `Parent`
3. Profile không được deleted/inactive
4. PIN mới phải hợp lệ
5. Hệ thống hash lại PIN và lưu

## 4.8. Flow select student profile

1. Gọi `POST /api/auth/profiles/select-student`
2. Profile được chọn phải:
   - thuộc current user
   - là `Student`
   - `!IsDeleted`
   - `IsActive`
3. Hệ thống cập nhật:
   - `profile.LastLoginAt`
   - `profile.LastSeenAt`
   - `profile.UpdatedAt`
   - `user.LastSeenAt`
   - `user.UpdatedAt`
4. Sinh access token mới có thêm claim `StudentId`
5. Trả về `accessToken` mới + `studentId`

## 4.9. Flow lấy profile của current user

### `GET /api/auth/profiles`

- chỉ trả profile của `currentUser`
- chỉ lấy profile `!IsDeleted && IsActive`
- có thể filter `ProfileType`

### `GET /api/me`

- trả thông tin user hiện tại + danh sách profile đang active, không deleted
- `SelectedProfileId` hiện đang để `null` cố định

### `PUT /api/me`

- cho phép update user hiện tại
- phần profile trong request hiện chỉ map `Id` + `DisplayName`
- `IsActive` có trong form request nhưng controller hiện không truyền xuống command

## 5. Permission matrix theo API

Lưu ý: tài liệu này mô tả theo code hiện tại. Nhiều endpoint profile hiện chỉ dùng `[Authorize]`, chưa khóa role chi tiết.

| API | Auth yêu cầu | Scope thực tế |
| --- | --- | --- |
| `POST /api/profiles` | Authenticated | rộng, theo request |
| `GET /api/profiles` | Authenticated | rộng, lọc được nhiều user/profile |
| `GET /api/profiles/{id}` | Authenticated | rộng |
| `PUT /api/profiles/{id}` | Authenticated | rộng |
| `DELETE /api/profiles/{id}` | Authenticated | rộng |
| `PUT /api/profiles/{id}/reactivate` | Authenticated | rộng |
| `GET /api/profiles/{id}/reactivate-and-update` | Anonymous | public qua link mail |
| `POST /api/profiles/link` | Authenticated | rộng |
| `POST /api/profiles/unlink` | Authenticated | rộng |
| `PUT /api/admin/users/approve` | không thấy `[Authorize]` active | rất rộng |
| `PUT /api/admin/users/{profileId}/change-pin` | không thấy `[Authorize]` active | rất rộng |
| `GET /api/auth/profiles` | Authenticated | own |
| `POST /api/auth/profiles/verify-parent-pin` | Authenticated | own parent profile |
| `POST /api/auth/profiles/select-student` | Authenticated | own student profile |
| `POST /api/auth/profiles/request-pin-reset` | Authenticated | own parent profile |
| `GET /api/me` | Authenticated | own user |
| `PUT /api/me` | Authenticated | own user |

## 6. API catalog

## 6.1. Quy ước response chung

### Success format

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Profile.NotFound",
  "status": 404,
  "detail": "The profile with the Id = '...' was not found"
}
```

## 6.2. Profile CRUD APIs

### 6.2.1. Tạo profile

- Endpoint: `POST /api/profiles`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `userId` | `guid` | Có |
| `profileType` | `ProfileType` | Có |
| `displayName` | `string` | Có |
| `fullName` | `string?` | Không |
| `pinHash` | `string?` | Không |

- Validation:
  - `userId` bắt buộc
  - `displayName` bắt buộc, max 255
  - `fullName` max 255 nếu có
  - `profileType` phải là enum hợp lệ

- Success data:

```json
{
  "id": "guid",
  "userId": "guid",
  "profileType": "Parent",
  "displayName": "Mẹ bé A",
  "name": "Nguyễn Thị B",
  "isActive": false,
  "createdAt": "2026-04-07T10:00:00Z",
  "updatedAt": "2026-04-07T10:00:00Z"
}
```

### 6.2.2. Danh sách profile toàn hệ thống

- Endpoint: `GET /api/profiles`
- Query:

| Field | Type | Required |
| --- | --- | --- |
| `userId` | `guid?` | Không |
| `profileType` | `string?` | Không |
| `searchTerm` | `string?` | Không |
| `branchId` | `guid?` | Không |
| `isActive` | `bool?` | Không |
| `isDeleted` | `bool?` | Không |
| `isApproved` | `bool?` | Không |
| `pageNumber` | `int` | Không |
| `pageSize` | `int` | Không |

- Success data: `Page<GetAllProfilesResponse>`
- Trường trả về chính:
  - `id`, `userId`, `userEmail`
  - `profileType`, `displayName`, `name`, `gender`, `dateOfBirth`
  - `isActive`, `isDeleted`, `isApproved`
  - `lastLoginAt`, `lastSeenAt`, `isOnline`, `offlineDurationSeconds`
  - `createdAt`, `updatedAt`

### 6.2.3. Chi tiết profile

- Endpoint: `GET /api/profiles/{id}`
- Success data: `GetProfileByIdResponse`
- Trường trả về gần giống API list nhưng theo 1 record

### 6.2.4. Cập nhật profile

- Endpoint: `PUT /api/profiles/{id}`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `displayName` | `string?` | Không |
| `isActive` | `bool?` | Không |

- Validation:
  - `id` bắt buộc
  - `displayName` max 255 nếu có

- Success data:

```json
{
  "id": "guid",
  "userId": "guid",
  "profileType": 0,
  "displayName": "Tên mới",
  "isActive": true,
  "isDeleted": false,
  "createdAt": "2026-04-01T10:00:00Z",
  "updatedAt": "2026-04-07T10:00:00Z"
}
```

Ghi chú:

- `profileType` ở response này là enum raw, không phải string.

### 6.2.5. Xóa profile

- Endpoint: `DELETE /api/profiles/{id}`
- Hành vi:
  - soft delete
  - set `IsDeleted = true`
  - set `IsActive = false`

### 6.2.6. Reactivate profile

- Endpoint: `PUT /api/profiles/{id}/reactivate`
- Hành vi:
  - tìm profile theo `id`
  - bật lại tất cả profile `IsApproved = true` cùng `UserId`

### 6.2.7. Reactivate và redirect frontend

- Endpoint: `GET /api/profiles/{id}/reactivate-and-update`
- Auth: `AllowAnonymous`
- Hành vi:
  - gọi reactivate
  - redirect về frontend `/profile/update?profileId={id}`

## 6.3. Link APIs

### 6.3.1. Link parent-student

- Endpoint: `POST /api/profiles/link`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `parentProfileId` | `guid` | Có |
| `studentProfileId` | `guid` | Có |

- Validation:
  - cả 2 field bắt buộc

- Success data:

```json
{
  "id": "guid",
  "parentProfileId": "guid",
  "studentProfileId": "guid",
  "createdAt": "2026-04-07T10:00:00Z"
}
```

### 6.3.2. Unlink parent-student

- Endpoint: `POST /api/profiles/unlink`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `parentProfileId` | `guid` | Có |
| `studentProfileId` | `guid` | Có |

## 6.4. Approval và admin PIN APIs

### 6.4.1. Approve profile

- Endpoint: `PUT /api/admin/users/approve`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `profileId` | `guid[]?` | Không, nhưng nếu null/empty thì handler trả success rỗng |

- Success data:

```json
{
  "approvedCount": 2,
  "alreadyApproved": ["guid"],
  "notFound": ["guid"]
}
```

### 6.4.2. Admin đổi parent PIN

- Endpoint: `PUT /api/admin/users/{profileId}/change-pin`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `newPin` | `string` | Có |

- Rule:
  - profile phải là `Parent`
  - profile phải active và không deleted
  - PIN phải numeric, độ dài < 10

## 6.5. Auth profile APIs

### 6.5.1. Lấy profile của current user

- Endpoint: `GET /api/auth/profiles`
- Query:

| Field | Type | Required |
| --- | --- | --- |
| `profileType` | `string?` | Không |

- Rule:
  - chỉ trả profile `UserId == currentUser`
  - chỉ lấy `!IsDeleted && IsActive`

- Success data:
  - `id`, `displayName`, `name`, `gender`, `dateOfBirth`
  - `profileType`, `isApproved`
  - `lastLoginAt`, `lastSeenAt`, `isOnline`, `offlineDurationSeconds`

### 6.5.2. Verify parent PIN

- Endpoint: `POST /api/auth/profiles/verify-parent-pin`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `profileId` | `guid` | Có |
| `pin` | `string` | Có |

- Rule:
  - nếu chưa có `PinHash` thì set lần đầu
  - nếu đã có thì verify

### 6.5.3. Select student profile

- Endpoint: `POST /api/auth/profiles/select-student`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `profileId` | `guid` | Có |

- Success data:

```json
{
  "accessToken": "jwt",
  "studentId": "guid"
}
```

### 6.5.4. Request parent PIN reset

- Endpoint: `POST /api/auth/profiles/request-pin-reset`
- Body:

| Field | Type | Required |
| --- | --- | --- |
| `profileId` | `guid` | Có |

- Hành vi:
  - tạo token reset 1 giờ
  - gửi email theo template `PARENT_PIN_RESET`
  - xóa token cũ chưa dùng của cùng profile

## 6.6. Current user APIs có chứa profile data

### 6.6.1. Get current user

- Endpoint: `GET /api/me`
- Trả về user hiện tại + list profile đang active, không deleted

### 6.6.2. Update current user

- Endpoint: `PUT /api/me`
- Content type: `multipart/form-data`
- Có thể update:
  - `fullName`, `email`, `phoneNumber`, `avatarUrl`
  - avatar upload
  - `profiles[].displayName`

Ghi chú:

- `profiles[].isActive` có ở form request nhưng hiện không được map xuống command.

## 7. Validation rule tổng hợp

## 7.1. CreateProfile

- `UserId` bắt buộc
- `DisplayName` bắt buộc, max 255
- `FullName` max 255 nếu có
- `ProfileType` phải là `Parent` hoặc `Student`

## 7.2. UpdateProfile

- `Id` bắt buộc
- `DisplayName` max 255 nếu có

## 7.3. Link / Unlink

- `ParentProfileId` bắt buộc
- `StudentProfileId` bắt buộc

## 7.4. Parent PIN

- PIN phải là số
- Độ dài PIN phải nhỏ hơn 10 ký tự

## 8. Danh sách lỗi nghiệp vụ chính

| Code | HTTP | Ý nghĩa |
| --- | --- | --- |
| `Profile.Invalid` | `400` | Profile không hợp lệ với flow hiện tại |
| `Profile.NotFound` | `404` | Không tìm thấy profile |
| `Profile.UserNotFound` | `404` | Không tìm thấy user để tạo profile |
| `Profile.InvalidProfileType` | `400` | Profile type không phải Parent/Student |
| `Profile.StudentNotFound` | `404` | Không tìm thấy student profile |
| `Profile.ParentNotFound` | `404` | Không tìm thấy parent profile |
| `Profile.LinkAlreadyExists` | `409` | Link parent-student đã tồn tại |
| `Profile.LinkNotFound` | `404` | Không tìm thấy link parent-student |
| `Profile.EmailNotSet` | `400` | User/profile chưa có email để reset PIN |
| `Profile.StudentIdNotSelected` | `404` | Chưa có student được chọn trong token |
| `Profile.StudentNotLinkedToParent` | `404` | Student không linked với parent |
| `Profile.ProfileNotDeleted` | `400` | Profile chưa deleted nhưng code reactivate hiện không còn enforce lỗi này |
| `Profile.UserMustBeParentOrStudent` | `400` | Flow chỉ cho parent/student |
| `Profile.ProfileAlreadyApproved` | `409` | Profile đã approved |
| `Pin.Invalid` | `400` | PIN không hợp lệ |
| `Pin.Wrong` | `409` | PIN sai |
| `Pin.NotSet` | `409` | PIN chưa được set |

## 9. Response mẫu tổng hợp

## 9.1. `GET /api/profiles`

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userId": "guid",
        "userEmail": "user@example.com",
        "profileType": "Student",
        "displayName": "Bé A",
        "name": "Nguyễn Văn A",
        "gender": "Male",
        "dateOfBirth": "2018-01-01",
        "isActive": true,
        "isDeleted": false,
        "isApproved": true,
        "lastLoginAt": "2026-04-07T09:00:00Z",
        "lastSeenAt": "2026-04-07T10:00:00Z",
        "isOnline": true,
        "offlineDurationSeconds": 0,
        "createdAt": "2026-04-01T10:00:00Z",
        "updatedAt": "2026-04-07T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

## 9.2. `GET /api/auth/profiles`

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "displayName": "Bé A",
      "name": "Nguyễn Văn A",
      "gender": "Male",
      "dateOfBirth": "2018-01-01",
      "profileType": "Student",
      "isApproved": true,
      "lastLoginAt": "2026-04-07T09:00:00Z",
      "lastSeenAt": "2026-04-07T10:00:00Z",
      "isOnline": true,
      "offlineDurationSeconds": 0
    }
  ]
}
```

## 9.3. `GET /api/me`

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "userName": "parent01",
    "fullName": "Nguyễn Thị B",
    "email": "parent@example.com",
    "phoneNumber": "0900000000",
    "role": "Parent",
    "branchId": null,
    "profiles": [
      {
        "id": "guid",
        "displayName": "Mẹ bé A",
        "profileType": "Parent",
        "lastLoginAt": "2026-04-07T09:00:00Z",
        "lastSeenAt": "2026-04-07T10:00:00Z",
        "isOnline": true,
        "offlineDurationSeconds": 0
      }
    ],
    "selectedProfileId": null,
    "permissions": [],
    "isActive": true,
    "avatarUrl": null,
    "lastLoginAt": "2026-04-07T09:00:00Z",
    "lastSeenAt": "2026-04-07T10:00:00Z",
    "isOnline": true,
    "offlineDurationSeconds": 0,
    "createdAt": "2026-04-01T10:00:00Z",
    "updatedAt": "2026-04-07T10:00:00Z"
  }
}
```

## 10. Lưu ý quan trọng theo code hiện tại

1. `ProfileController` hiện chỉ có `[Authorize]`, chưa chặn role chi tiết cho CRUD/link/unlink.
2. `AdminUserController` đang comment dòng `[Authorize(Roles = "Admin")]`, nên các API approve/change-pin hiện chưa được khóa role ở controller.
3. `CreateProfile` tạo profile mới với `IsApproved = false` và `IsActive = false`; profile chưa usable ngay sau khi tạo.
4. `ApproveProfile` chỉ set `IsApproved = true`, không tự bật `IsActive`.
5. `ReactivateProfile` bật lại tất cả profile đã approved cùng `UserId`, không chỉ record được gọi.
6. `GET /api/profiles/{id}/reactivate-and-update` là public link, dùng cho email onboarding/reactivation.
7. `SelectStudentProfile` chỉ chấp nhận student profile thuộc chính current user; sau đó phát JWT mới có claim `StudentId`.
8. `GetCurrentUserResponse.SelectedProfileId` hiện luôn là `null`.
9. `PUT /api/me` chỉ update được `DisplayName` của profile con; `IsActive` tuy có trong request form nhưng bị bỏ qua ở controller.
10. Parent PIN reset đang dùng link hard-coded `https://kidzgo.app/parent/pin/reset?...` trong event handler.

