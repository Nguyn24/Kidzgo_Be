# Login API Usage Guide

Ngay cap nhat: `2026-04-13`

Muc dich tai lieu:

- Tong hop cac API trong flow login hien tai
- Mo ta request, response, error message de FE tich hop
- Lam ro flow sau login: lay current user, lay profile list, chon student context, refresh token, logout
- Ghi chu cac contract de nham nhat trong code hien tai

Pham vi tai lieu:

- `POST /api/auth/login`
- `POST /api/auth/login-phone`
- `POST /api/auth/refresh-token`
- `GET /api/me`
- `GET /api/auth/profiles`
- `POST /api/auth/profiles/select-student`
- `POST /api/me/logout`

Luu y chung:

- Success response tu cac API qua mediator thuong theo format:

```json
{
  "isSuccess": true,
  "data": {}
}
```

- Error response thuong theo `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Users.WrongPassword",
  "status": 409,
  "detail": "The passsword for this account is wrong",
  "traceId": "..."
}
```

- Validation error tu FluentValidation co format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "Email is required",
      "type": 1
    }
  ],
  "traceId": "..."
}
```

## 1. Login flow tong quan

Flow FE de xuat:

1. Goi `POST /api/auth/login` hoac `POST /api/auth/login-phone`.
2. Luu `accessToken` va `refreshToken`.
3. Goi `GET /api/me` de lay thong tin user hien tai.
4. Neu user co role lien quan profile context, goi `GET /api/auth/profiles`.
5. Neu la parent can chon be dang thao tac, FE tu luu `profileId` va co the goi `POST /api/auth/profiles/select-student` de validate profile da chon hop le.
6. Khi `accessToken` het han, goi `POST /api/auth/refresh-token`.
7. Khi logout, goi `POST /api/me/logout`, sau do FE xoa token local.

## 2. Login APIs

### 2.1. `POST /api/auth/login`

Muc dich:

- Dang nhap bang `email + password`
- Tao moi `accessToken`
- Tao moi `refreshToken`
- Xoa cac refresh token cu cua user, chi giu 1 refresh token moi

Auth:

- Khong can token

Request body:

```json
{
  "email": "teacher@example.com",
  "password": "123456"
}
```

Field:

| Field | Type | Required |
| --- | --- | --- |
| `email` | `string` | Yes |
| `password` | `string` | Yes |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "<jwt>",
    "refreshToken": "<refresh-token>",
    "role": "Teacher",
    "userId": "guid"
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Validation.General` | 400 | `One or more validation errors occurred` |
| `Users.NotFoundByEmail` | 404 | `The user with the specified email was not found` |
| `Users.WrongPassword` | 409 | `The passsword for this account is wrong` |
| `Users.InActive` | 409 | `The user is inactive` |

Validation notes:

- Neu thieu `email`: `Email is required`
- Neu thieu `password`: `Password is required`

FE note:

- Login thanh cong se update `LastLoginAt`, `LastSeenAt` cua user.
- Response login co `role` va `userId`, nhung FE van nen goi `GET /api/me` de dong bo full profile.

### 2.2. `POST /api/auth/login-phone`

Muc dich:

- Dang nhap bang so dien thoai

Auth:

- Khong can token

Request body:

```json
{
  "phoneNumber": "0901234567"
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "<jwt>",
    "refreshToken": "<refresh-token>",
    "role": "Parent",
    "userId": "guid"
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Validation.General` | 400 | `One or more validation errors occurred` |
| `Users.NotFoundByPhoneNumber` | 404 | `The user with the specified phone number was not found` |
| `Users.InActive` | 409 | `The user is inactive` |

Validation notes:

- Neu thieu `phoneNumber`: `Phone number is required`
- Neu sai format: `Phone number must be a valid Vietnamese phone number`

Contract note rat quan trong:

- Flow nay hien tai chi nhan `phoneNumber`.
- Backend hien khong verify password, OTP hoac PIN trong endpoint nay.
- FE chi nen dung flow nay neu day la behavior mong muon cua business hien tai.

### 2.3. `POST /api/auth/refresh-token`

Muc dich:

- Doi `refreshToken` lay cap token moi
- Backend rotate refresh token, token cu khong con dung

Auth:

- Khong can `Authorization` header

Request body:

API nay nhan raw JSON string, khong phai object.

```json
"<refresh-token>"
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "<jwt-moi>",
    "refreshToken": "<refresh-token-moi>",
    "role": 0,
    "userId": "00000000-0000-0000-0000-000000000000"
  }
}
```

Contract note:

- Theo code hien tai, handler `refresh-token` chi set `AccessToken` va `RefreshToken`.
- `Role` va `UserId` trong `TokenResponse` khong duoc gan lai o flow refresh, nen FE khong duoc phu thuoc vao 2 field nay khi refresh.
- Sau refresh, FE nen tiep tuc dung role/userId da luu truoc do hoac goi `GET /api/me`.

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Users.RefreshTokenInvalid` | 409 | `The refresh token is invalid` |
| `Users.InActive` | 409 | `The user is inactive` |

FE note:

- Neu refresh fail, FE nen clear session local va redirect ve man login.

## 3. APIs sau login

### 3.1. `GET /api/me`

Muc dich:

- Lay thong tin current user sau login
- Dung de build session client

Auth:

- `Bearer token` bat buoc

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "userName": "teacher01",
    "fullName": "Tran Thi B",
    "email": "teacher@example.com",
    "phoneNumber": "0901234567",
    "role": "Teacher",
    "branchId": "guid",
    "branch": {
      "id": "guid",
      "code": "CN1",
      "name": "Chi nhanh 1",
      "address": "..."
    },
    "profiles": [
      {
        "id": "guid",
        "displayName": "Tran Thi B",
        "profileType": "Teacher",
        "lastLoginAt": "2026-04-13T01:00:00Z",
        "lastSeenAt": "2026-04-13T01:05:00Z",
        "isOnline": true,
        "offlineDurationSeconds": 0
      }
    ],
    "selectedProfileId": null,
    "permissions": [],
    "isActive": true,
    "avatarUrl": "https://cdn.example.com/avatar.png",
    "lastLoginAt": "2026-04-13T01:00:00Z",
    "lastSeenAt": "2026-04-13T01:05:00Z",
    "isOnline": true,
    "offlineDurationSeconds": 0,
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-04-13T01:05:00Z"
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Users.NotFound` | 404 | `The user with the Id = '...' was not found` |

FE note:

- `selectedProfileId` hien tai dang hard-code `null` trong handler.
- Neu FE can student context, khong duoc trong cho field nay tu backend.

### 3.2. `GET /api/auth/profiles`

Muc dich:

- Lay danh sach profile active cua current user
- Hay dung sau login neu user co nhieu profile

Auth:

- `Bearer token` bat buoc

Query params:

| Param | Type | Required | Note |
| --- | --- | --- | --- |
| `profileType` | `string` | No | Controller nhan param nay nhung handler hien khong filter theo param |

Success response:

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "displayName": "Me cua Be Na",
      "profileType": "Parent",
      "lastLoginAt": "2026-04-13T01:00:00Z",
      "lastSeenAt": "2026-04-13T01:05:00Z",
      "isOnline": true,
      "offlineDurationSeconds": 0
    },
    {
      "id": "guid",
      "displayName": "Be Na",
      "profileType": "Student",
      "lastLoginAt": null,
      "lastSeenAt": null,
      "isOnline": false,
      "offlineDurationSeconds": null
    }
  ]
}
```

FE note:

- Handler hien lay tat ca profile active cua current user, bo qua `profileType` query param.
- Neu FE muon filter, nen filter them o client hoac yeu cau backend sua contract.

### 3.3. `POST /api/auth/profiles/select-student`

Muc dich:

- Validate student profile ma FE muon chon
- Huu ich cho parent co nhieu be

Auth:

- `Bearer token` bat buoc

Request body:

```json
{
  "profileId": "guid"
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": null
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Profile.Invalid` | 400 | `Profile is invalid.` |

Contract note rat quan trong:

- Handler hien tai chi validate `profileId` co thuoc user hien tai, la `Student`, active, khong deleted.
- Backend khong luu selected student context sau khi goi API nay.
- Comment trong code ghi ro: FE tu dung `profileId` de set context.

FE note:

- Xem endpoint nay nhu 1 buoc validate, khong phai save-state API.
- Neu FE can luu selected student, phai luu o local storage, state management hoac gui lai `profileId`/context theo flow rieng.

## 4. Logout

### 4.1. `POST /api/me/logout`

Muc dich:

- Xoa tat ca refresh token cua current user
- Ket thuc phien dang nhap tu phia backend

Auth:

- `Bearer token` bat buoc

Request:

- Body rong

Success response:

```json
{
  "isSuccess": true,
  "data": null
}
```

Error cho frontend:

- Hien tai khong co business error rieng; neu token hop le thi API se tra success ke ca khi user khong con refresh token nao.

FE note:

- Sau khi logout thanh cong, FE phai xoa:
  - `accessToken`
  - `refreshToken`
  - selected profile context local neu co

## 5. Error map nhanh

| Code | HTTP | Khi nao gap |
| --- | --- | --- |
| `Validation.General` | 400 | Thieu field bat buoc hoac sai format input |
| `Users.NotFoundByEmail` | 404 | Email login khong ton tai |
| `Users.NotFoundByPhoneNumber` | 404 | So dien thoai login khong ton tai |
| `Users.WrongPassword` | 409 | Sai mat khau khi login email |
| `Users.InActive` | 409 | User bi inactive hoac deleted |
| `Users.RefreshTokenInvalid` | 409 | Refresh token sai hoac het han |
| `Users.NotFound` | 404 | Goi `GET /api/me` nhung user trong token khong ton tai |
| `Profile.Invalid` | 400 | Chon `profileId` khong hop le trong flow select student |

## 6. Frontend checklist

### 6.1. Login bang email

1. Goi `POST /api/auth/login`.
2. Luu `accessToken`, `refreshToken`, `role`, `userId`.
3. Goi `GET /api/me`.
4. Neu can profile switch, goi `GET /api/auth/profiles`.

### 6.2. Login bang phone

1. Goi `POST /api/auth/login-phone`.
2. Luu token.
3. Goi `GET /api/me`.
4. Neu day khong phai flow business mong muon vi thieu OTP/password, can xac nhan lai voi backend truoc khi release.

### 6.3. Parent co nhieu be

1. Sau login, goi `GET /api/auth/profiles`.
2. Loc danh sach `profileType == "Student"`.
3. Khi user chon be, goi `POST /api/auth/profiles/select-student` de validate.
4. FE tu luu `profileId` dang chon.

### 6.4. Refresh token

1. Khi access token het han, goi `POST /api/auth/refresh-token` voi raw string body.
2. Neu success, thay token local.
3. Neu fail, clear session va ve login.

## 7. Luu y implementation cho FE

- `refresh-token` nhan body la raw string, khong phai:

```json
{
  "refreshToken": "..."
}
```

- `GET /api/auth/profiles` hien khong thuc su filter theo `profileType`.
- `POST /api/auth/profiles/select-student` khong persist selected context o backend.
- `POST /api/auth/login-phone` hien la flow login khong co challenge second-factor.
