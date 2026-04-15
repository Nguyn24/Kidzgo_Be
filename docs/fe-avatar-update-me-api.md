# FE API Usage: Avatar va Update Me

Tai lieu nay chi tap trung vao:

- request
- payload
- response
- error message

cho 3 API:

- `GET /api/me`
- `POST /api/files/avatar`
- `PUT /api/me`

## 1. `GET /api/me`

### Request

```http
GET /api/me
Authorization: Bearer <access_token>
```

### Payload

Khong co body.

### Success Response

```json
{
  "id": "3f5d62c0-0d7f-4a3d-b7e7-3d8d0a0f0001",
  "userName": "parent01",
  "fullName": "Nguyen Thi A",
  "email": "parent@example.com",
  "phoneNumber": "0900000000",
  "role": "Parent",
  "branchId": null,
  "avatarUrl": "https://cdn.example.com/avatars/user.jpg",
  "selectedProfileId": "c5fd59e0-8c57-48fd-a403-9b1ad6de0002",
  "isActive": true,
  "profiles": [
    {
      "id": "7e8c4f15-d417-46f0-83e1-2d8ef3d10001",
      "displayName": "Me cua Be B",
      "avatarUrl": "https://cdn.example.com/avatars/parent-profile.jpg",
      "profileType": "Parent",
      "lastLoginAt": "2026-04-15T01:00:00Z",
      "lastSeenAt": "2026-04-15T01:10:00Z",
      "isOnline": true,
      "offlineDurationSeconds": 0
    },
    {
      "id": "c5fd59e0-8c57-48fd-a403-9b1ad6de0002",
      "displayName": "Be B",
      "avatarUrl": "https://cdn.example.com/avatars/student.jpg",
      "profileType": "Student",
      "lastLoginAt": null,
      "lastSeenAt": null,
      "isOnline": false,
      "offlineDurationSeconds": null
    }
  ]
}
```

### FE can lay gi tu response

- `role`
- `avatarUrl`
- `selectedProfileId`
- `profiles`
- `parentProfileId`: profile co `profileType = "Parent"`
- `studentProfileId`: profile co `id = selectedProfileId`

### Error Response

Thuong gap:

```json
{
  "title": "Unauthorized",
  "status": 401
}
```

hoac:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "User was not found."
}
```

## 2. `POST /api/files/avatar`

Dung khi FE chi muon upload avatar rieng.

### Request

```http
POST /api/files/avatar?targetProfileId=<optional-guid>
Authorization: Bearer <access_token>
Content-Type: multipart/form-data
```

### Payload

`multipart/form-data`

Bat buoc:

- `file`: file anh

Optional:

- `targetProfileId`: query param

### Cach dung theo flow

#### Upload avatar cho Parent

Dung:

```http
POST /api/files/avatar?targetProfileId=<parentProfileId>
```

Ket qua:

- update `user.avatarUrl`
- update `parentProfile.avatarUrl`

#### Upload avatar cho Student

Dung:

```http
POST /api/files/avatar?targetProfileId=<studentProfileId>
```

Ket qua:

- update `studentProfile.avatarUrl`
- khong update `user.avatarUrl` cua parent

### Example Payload

```ts
const formData = new FormData();
formData.append("file", file);
```

### Success Response

```json
{
  "url": "https://cdn.example.com/avatars/new-avatar.jpg",
  "fileName": "avatar.jpg",
  "size": 245120
}
```

### Error Response

#### 400 No file provided

```json
{
  "error": "No file provided"
}
```

#### 400 File empty

Neu file length = 0, backend thuong se tra:

```json
{
  "error": "No file provided"
}
```

#### 400 Sai loai file

```json
{
  "error": "File type is not allowed for image. Allowed types: .jpg, .jpeg, .png, .gif, .webp, .bmp, .svg"
}
```

#### 400 Vuot dung luong

```json
{
  "error": "File size exceeds maximum allowed size of 10MB for image"
}
```

#### 400 Parent chua chon student ma muon update avatar profile

Co the gap khi token Parent khong co `selectedProfileId` va FE khong truyen `targetProfileId`:

```json
{
  "error": "Parent account must select a student profile before updating avatar"
}
```

#### 401 Unauthorized

```json
{
  "title": "Unauthorized",
  "status": 401
}
```

## 3. `PUT /api/me`

Dung khi FE muon update thong tin user hien tai, profile hien tai, va co the upload avatar cung luc.

### Request

```http
PUT /api/me
Authorization: Bearer <access_token>
Content-Type: multipart/form-data
```

### Payload

`multipart/form-data`

Field ho tro:

- `FullName`
- `Email`
- `PhoneNumber`
- `AvatarUrl`
- `TargetProfileId`
- `Avatar`
- `Profiles[0].Id`
- `Profiles[0].DisplayName`
- `Profiles[1].Id`
- `Profiles[1].DisplayName`

### Rule backend

#### Role khong co profile

Vi du:

- `Admin`
- `Teacher`
- `ManagementStaff`
- `AccountantStaff`

Backend chi update field cua `User`:

- `FullName`
- `Email`
- `PhoneNumber`
- `AvatarUrl`

#### Role `Parent`

Backend xu ly nhu sau:

- parent profile resolve theo `userId`
- student profile resolve theo `selectedProfileId` trong token
- neu gui `Avatar` + `TargetProfileId = parentProfileId`: update avatar parent
- neu gui `Avatar` + `TargetProfileId = studentProfileId`: update avatar student

### Payload Example 1: role khong co profile

```ts
const formData = new FormData();
formData.append("FullName", "Nguyen Van Staff");
formData.append("Email", "staff@example.com");
formData.append("PhoneNumber", "0900000000");
```

### Payload Example 2: Parent update parent profile + parent avatar

```ts
const formData = new FormData();

formData.append("FullName", "Nguyen Thi A");
formData.append("PhoneNumber", "0900000000");

formData.append("TargetProfileId", parentProfileId);
formData.append("Avatar", avatarFile);

formData.append("Profiles[0].Id", parentProfileId);
formData.append("Profiles[0].DisplayName", "Me cua Be B");
```

### Payload Example 3: Parent update student profile + student avatar

```ts
const formData = new FormData();

formData.append("TargetProfileId", selectedStudentProfileId);
formData.append("Avatar", avatarFile);

formData.append("Profiles[0].Id", selectedStudentProfileId);
formData.append("Profiles[0].DisplayName", "Be B");
```

### Payload Example 4: Parent update ca parent profile va student profile

```ts
const formData = new FormData();

formData.append("FullName", "Nguyen Thi A");
formData.append("PhoneNumber", "0900000000");

formData.append("Profiles[0].Id", parentProfileId);
formData.append("Profiles[0].DisplayName", "Me cua Be B");

formData.append("Profiles[1].Id", selectedStudentProfileId);
formData.append("Profiles[1].DisplayName", "Be B");
```

### Success Response

```json
{
  "id": "3f5d62c0-0d7f-4a3d-b7e7-3d8d0a0f0001",
  "userName": "parent01",
  "fullName": "Nguyen Thi A",
  "email": "parent@example.com",
  "phoneNumber": "0900000000",
  "role": "Parent",
  "branchId": null,
  "avatarUrl": "https://cdn.example.com/avatars/user.jpg",
  "isActive": true,
  "createdAt": "2026-04-01T00:00:00Z",
  "updatedAt": "2026-04-15T02:00:00Z",
  "profiles": [
    {
      "id": "7e8c4f15-d417-46f0-83e1-2d8ef3d10001",
      "displayName": "Me cua Be B",
      "avatarUrl": "https://cdn.example.com/avatars/parent-profile.jpg",
      "profileType": "Parent",
      "lastLoginAt": "2026-04-15T01:00:00Z",
      "lastSeenAt": "2026-04-15T01:10:00Z",
      "isOnline": true,
      "offlineDurationSeconds": 0
    },
    {
      "id": "c5fd59e0-8c57-48fd-a403-9b1ad6de0002",
      "displayName": "Be B",
      "avatarUrl": "https://cdn.example.com/avatars/student.jpg",
      "profileType": "Student",
      "lastLoginAt": null,
      "lastSeenAt": null,
      "isOnline": false,
      "offlineDurationSeconds": null
    }
  ]
}
```

### Error Response

#### 400 Avatar file is empty

```json
{
  "error": "Avatar file is empty"
}
```

#### 400 Email khong hop le

Response validation thuong dang:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "'Email' is not a valid email address."
    ]
  }
}
```

#### 400 PhoneNumber khong hop le

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PhoneNumber": [
      "Phone number must be a valid 10-digit Vietnamese phone number"
    ]
  }
}
```

#### 400 Email da ton tai

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Email 'parent@example.com' already exists."
}
```

#### 400 Phone number da ton tai

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Phone number must be unique."
}
```

#### 400 Error tu flow upload avatar ben trong `PUT /api/me`

Neu co gui `Avatar`, backend se upload avatar truoc. Neu upload fail thi response loi se den tu flow upload avatar.

Vi du:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Parent account must select a student profile before updating avatar"
}
```

#### 401 Unauthorized

```json
{
  "title": "Unauthorized",
  "status": 401
}
```

## 4. FE checklist

### Upload avatar parent

1. `GET /api/me`
2. tim `parentProfileId`
3. `POST /api/files/avatar?targetProfileId=<parentProfileId>`

### Upload avatar student

1. `GET /api/me`
2. lay `selectedProfileId`
3. `POST /api/files/avatar?targetProfileId=<selectedProfileId>`

### Update me cho Parent

1. `GET /api/me`
2. lay `parentProfileId`
3. lay `selectedProfileId`
4. build `FormData`
5. `PUT /api/me`

### Luu y

- Luon truyen `targetProfileId` ro rang khi upload avatar
- Luon gui `Profiles[i].Id` dung profile can sua
- Neu muon sua student khac, token phai duoc doi sang student do truoc
