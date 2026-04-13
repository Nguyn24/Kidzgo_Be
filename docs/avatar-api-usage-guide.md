# Avatar API Usage Guide

Ngay cap nhat: `2026-04-13`

Muc dich tai lieu:

- Tong hop cac API lien quan den upload avatar
- Mo ta cac API GET dang tra `avatarUrl` hoac `studentAvatarUrl`
- Ghi ro request, response, error message de FE tich hop
- Lam ro endpoint nao doc `User.AvatarUrl`, endpoint nao doc `Profile.AvatarUrl`

Luu y chung:

- Phan lon API qua mediator tra success theo format:

```json
{
  "isSuccess": true,
  "data": {}
}
```

- Cac API upload trong `FileUploadController` tra response raw, khong wrap `isSuccess`:

```json
{
  "url": "https://cdn.example.com/avatars/abc.jpg",
  "fileName": "avatar.jpg",
  "size": 123456
}
```

- Error tu domain/handler thuong ra `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "File.ParentProfileSelectionRequired",
  "status": 400,
  "detail": "Parent account must select a student profile before updating avatar",
  "traceId": "..."
}
```

## 1. Avatar source map

Bang nay rat quan trong cho FE vi khong phai endpoint nao cung doc avatar tu cung 1 nguon.

| API | JSON path | Nguon avatar |
| --- | --- | --- |
| `GET /api/me` | `data.avatarUrl` | `User.AvatarUrl` |
| `GET /api/teacher/profile` | `data.basicInfo.avatarUrl` | `User.AvatarUrl` |
| `GET /api/student/profile` | `data.studentInfo.avatarUrl` | `Profile.AvatarUrl` |
| `GET /api/parent/account` | `data.user.avatarUrl` | `User.AvatarUrl` |
| `GET /api/parent/account` | `data.parentProfile.avatarUrl` | `Profile.AvatarUrl` cua parent profile |
| `GET /api/classes/{id}/students` | `data.students.items[].avatarUrl` | `Profile.AvatarUrl ?? User.AvatarUrl` |
| `GET /api/attendance/{sessionId}` | `data.attendances[].studentAvatarUrl` | `Profile.AvatarUrl ?? User.AvatarUrl` |
| `GET /api/students/timetable` | `data.sessions[].studentAvatarUrl` | `Profile.AvatarUrl` |
| `GET /api/parent/timetable` | `data.sessions[].studentAvatarUrl` | `Profile.AvatarUrl` |

FE note:

- `POST /api/files/avatar` da tu update avatar trong DB, khong can goi them API khac neu chi muon upload avatar.
- `GET /api/students/timetable` va `GET /api/parent/timetable` hien doc `Profile.AvatarUrl` truc tiep, khong fallback sang `User.AvatarUrl`.
- `GET /api/classes/{id}/students` va `GET /api/attendance/{sessionId}` co fallback `Profile.AvatarUrl ?? User.AvatarUrl`.

## 2. Upload avatar

### 2.1. `POST /api/files/avatar`

Muc dich:

- Upload 1 file avatar image
- Backend upload file len storage
- Backend tu dong update avatar vao user/profile tuy context hien tai

Role:

- Bat ky user da dang nhap

Auth:

- `Bearer token` bat buoc

Content-Type:

- `multipart/form-data`

Form-data:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `file` | `file` | Yes | Anh avatar |

Request example:

```bash
curl -X POST "/api/files/avatar" \
  -H "Authorization: Bearer <token>" \
  -F "file=@avatar.png"
```

Success response:

```json
{
  "url": "https://cdn.example.com/avatars/user-id/avatar.png",
  "fileName": "avatar.png",
  "size": 248392
}
```

Error cho frontend:

| Code/Title | HTTP | Message |
| --- | --- | --- |
| raw bad request | 400 | `No file provided` |
| `File.SizeExceedsLimit` | 400 | `File size exceeds maximum allowed size of 10MB for image` |
| `File.InvalidFileType` | 400 | `File type is not allowed for image. Allowed types: .jpg, .jpeg, .png, .gif, .webp, .bmp, .svg` |
| `File.Unauthorized` | 400 | `Unauthorized to perform this operation` |
| `File.ParentProfileSelectionRequired` | 400 | `Parent account must select a student profile before updating avatar` |
| `File.UploadFailed` | 500 | `Failed to upload file: ...` |

Behavior note:

- Folder upload hien tai la `avatars/{currentUserId}`.
- `ResourceType` la `image`, nen gioi han dung rule file image.
- Voi `Parent`, backend co the yeu cau `student profile` duoc chon trong token/context. Neu chua co, API se fail voi `File.ParentProfileSelectionRequired`.

### 2.2. `PUT /api/me`

Muc dich:

- Update thong tin current user
- Co the upload avatar trong cung request `multipart/form-data`
- Co the update cung luc `fullName`, `email`, `phoneNumber`, `profiles[].displayName`

Role:

- Bat ky user da dang nhap

Auth:

- `Bearer token` bat buoc

Content-Type:

- `multipart/form-data`

Form-data fields:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `fullName` | `string` | No | Chi update khi co gia tri |
| `email` | `string` | No | Chi update khi co gia tri |
| `phoneNumber` | `string` | No | Chi update khi co gia tri |
| `avatarUrl` | `string` | No | Set truc tiep URL neu khong upload file |
| `avatar` | `file` | No | Neu co thi backend upload va update avatar |
| `profiles[i].id` | `guid` | No | Profile can sua |
| `profiles[i].displayName` | `string` | No | Ten hien thi moi |

Request example:

```bash
curl -X PUT "/api/me" \
  -H "Authorization: Bearer <token>" \
  -F "fullName=Nguyen Van A" \
  -F "phoneNumber=0901234567" \
  -F "avatar=@avatar.png" \
  -F "profiles[0].id=11111111-1111-1111-1111-111111111111" \
  -F "profiles[0].displayName=Be Na"
```

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
    "avatarUrl": "https://cdn.example.com/avatars/user-id/avatar.png",
    "isActive": true,
    "profiles": [
      {
        "id": "guid",
        "displayName": "Be Na",
        "profileType": "Student"
      }
    ]
  }
}
```

Error cho frontend:

| Code/Title | HTTP | Message |
| --- | --- | --- |
| raw bad request | 400 | `Avatar file is empty` |
| `File.SizeExceedsLimit` | 400 | `File size exceeds maximum allowed size of 10MB for image` |
| `File.InvalidFileType` | 400 | `File type is not allowed for image. Allowed types: .jpg, .jpeg, .png, .gif, .webp, .bmp, .svg` |
| `File.ParentProfileSelectionRequired` | 400 | `Parent account must select a student profile before updating avatar` |
| `Users.NotFound` | 404 | `The user with the Id = '...' was not found` |
| `User.EmailAlreadyExists` | 409 | `Email '...' is already in use.` |
| `Users.PhoneNumberNotUnique` | 409 | `The provided phone number is already in use` |

FE note:

- Neu FE chi muon upload avatar rieng le, uu tien dung `POST /api/files/avatar`.
- Neu FE muon update profile + avatar trong cung 1 submit, dung `PUT /api/me`.
- Trong flow nay, backend upload avatar truoc, sau do moi update thong tin user.
- Voi `Parent`, avatar upload co the update `Profile.AvatarUrl` nhieu hon `User.AvatarUrl`; neu UI can avatar parent profile thi nen refresh `GET /api/parent/account`.

## 3. GET APIs tra avatar url

### 3.1. `GET /api/me`

Muc dich:

- Lay thong tin user hien tai
- Tra `data.avatarUrl` de render avatar user

Role:

- Bat ky user da dang nhap

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
    "phoneNumber": "0909999999",
    "role": "Teacher",
    "branchId": "guid",
    "avatarUrl": "https://cdn.example.com/avatars/user-id/avatar.png",
    "profiles": [
      {
        "id": "guid",
        "displayName": "Tran Thi B",
        "profileType": "Teacher"
      }
    ]
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Users.NotFound` | 404 | `The user with the Id = '...' was not found` |

### 3.2. `GET /api/teacher/profile`

Muc dich:

- Lay profile page cho teacher
- Avatar nam o `data.basicInfo.avatarUrl`

Role:

- `Teacher`

Auth:

- `Bearer token` bat buoc

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "basicInfo": {
      "id": "guid",
      "fullName": "Tran Thi B",
      "email": "teacher@example.com",
      "phoneNumber": "0909999999",
      "avatarUrl": "https://cdn.example.com/avatars/user-id/avatar.png",
      "branchId": "guid",
      "branchName": "CN 1"
    },
    "bio": null,
    "skills": [],
    "certificates": [],
    "teachingStats": {}
  }
}
```

Error cho frontend:

- Co the fail theo `GET /api/me` neu current user khong ton tai
- Co the fail theo `GetTeacherOverview` neu khong lay duoc dashboard data

### 3.3. `GET /api/student/profile`

Muc dich:

- Lay profile page cho student
- Avatar nam o `data.studentInfo.avatarUrl`

Role:

- `Student`

Auth:

- `Bearer token` bat buoc

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "studentInfo": {
      "id": "guid",
      "displayName": "Be Na",
      "name": "Nguyen Ngoc Na",
      "dateOfBirth": "2018-01-15",
      "avatarUrl": "https://cdn.example.com/avatars/student-profile/avatar.png",
      "email": "parent@example.com",
      "phone": "0901234567"
    },
    "attendancePercent": 95.5,
    "scores": [],
    "courseHistory": [],
    "certificates": []
  }
}
```

Error cho frontend:

| Code/Title | HTTP | Message |
| --- | --- | --- |
| `StudentProfile` | 404 | `Student profile not found` |

### 3.4. `GET /api/parent/account`

Muc dich:

- Lay account page cho parent
- Co 2 avatar co the dung:
  - `data.user.avatarUrl` tu user
  - `data.parentProfile.avatarUrl` tu parent profile

Role:

- `Parent`

Auth:

- `Bearer token` bat buoc

Success response mau:

```json
{
  "isSuccess": true,
  "data": {
    "user": {
      "id": "guid",
      "fullName": "Phu huynh A",
      "avatarUrl": "https://cdn.example.com/avatars/user/avatar.png"
    },
    "parentProfile": {
      "id": "guid",
      "displayName": "Me cua Be Na",
      "avatarUrl": "https://cdn.example.com/avatars/profile/avatar.png"
    }
  }
}
```

Error cho frontend:

- Co the fail theo `GET /api/me` neu user khong ton tai

### 3.5. `GET /api/classes/{id}/students`

Muc dich:

- Lay danh sach hoc sinh trong lop
- Tra avatar de render class roster

Role:

- `Admin`, `ManagementStaff`, `Teacher`

Auth:

- `Bearer token` bat buoc

Query params:

| Param | Type | Required |
| --- | --- | --- |
| `pageNumber` | `int` | No |
| `pageSize` | `int` | No |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "students": {
      "items": [
        {
          "studentProfileId": "guid",
          "fullName": "Be Na",
          "avatarUrl": "https://cdn.example.com/avatars/student/avatar.png",
          "email": "parent@example.com",
          "phone": "0901234567",
          "enrollDate": "2026-03-01",
          "status": "Active",
          "attendanceRate": 95.5,
          "progressPercent": 88.0,
          "stars": 12,
          "lastActiveAt": "2026-04-13T02:10:00Z"
        }
      ],
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 1
    }
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Class.NotFound` | 404 | `Class with Id = '...' was not found` |

Avatar note:

- API nay fallback `profile.avatarUrl ?? user.avatarUrl`.

### 3.6. `GET /api/attendance/{sessionId}`

Muc dich:

- Lay danh sach diem danh cua session
- Tra `studentAvatarUrl` cho tung hoc sinh

Role:

- `Admin`, `Teacher`

Auth:

- `Bearer token` bat buoc

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "guid",
    "sessionName": "KID A1",
    "date": "2026-04-13",
    "startTime": "18:00:00",
    "endTime": "19:30:00",
    "summary": {
      "totalStudents": 2,
      "presentCount": 2,
      "absentCount": 0,
      "makeupCount": 0,
      "notMarkedCount": 0
    },
    "attendances": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "studentName": "Be Na",
        "studentAvatarUrl": "https://cdn.example.com/avatars/student/avatar.png",
        "registrationId": "guid",
        "track": "Main",
        "isMakeup": false,
        "attendanceStatus": "Present",
        "absenceType": null,
        "hasMakeupCredit": false,
        "note": null,
        "markedAt": "2026-04-13T11:02:00Z"
      }
    ]
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Session.NotFound` | 404 | `Session with Id = '...' was not found` |

Avatar note:

- API nay fallback `profile.avatarUrl ?? user.avatarUrl`.

### 3.7. `GET /api/students/timetable`

Muc dich:

- Lay timetable cua current student
- Moi session item co `studentAvatarUrl`

Role:

- `Student`

Auth:

- `Bearer token` bat buoc

Query params:

| Param | Type | Required |
| --- | --- | --- |
| `from` | `datetime` | No |
| `to` | `datetime` | No |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "studentDisplayName": "Be Na",
        "studentAvatarUrl": "https://cdn.example.com/avatars/student/avatar.png",
        "color": "#F7B267",
        "classId": "guid",
        "classCode": "KID-A1",
        "classTitle": "KID A1",
        "plannedDatetime": "2026-04-13T11:00:00Z",
        "durationMinutes": 90,
        "participationType": "Main",
        "status": "Scheduled",
        "plannedTeacherName": "Teacher A",
        "plannedAssistantName": "TA B",
        "track": "Main",
        "isMakeup": false,
        "attendanceStatus": "Present"
      }
    ]
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Profile.StudentNotFound` | 404/400 | `Student profile not found.` |
| `Profile.NotFound` | 404 | `The profile with the Id = '...' was not found` |

Avatar note:

- API nay doc `Profile.AvatarUrl` truc tiep, khong fallback `User.AvatarUrl`.

### 3.8. `GET /api/parent/timetable`

Muc dich:

- Lay timetable cua tat ca hoc sinh linked voi parent hien tai
- Moi session item co thong tin student + `studentAvatarUrl`

Role:

- `Parent`

Auth:

- `Bearer token` bat buoc

Query params:

| Param | Type | Required |
| --- | --- | --- |
| `from` | `datetime` | No |
| `to` | `datetime` | No |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "studentDisplayName": "Be Na",
        "studentAvatarUrl": "https://cdn.example.com/avatars/student/avatar.png",
        "color": "#F7B267",
        "classId": "guid",
        "classCode": "KID-A1",
        "classTitle": "KID A1",
        "plannedDatetime": "2026-04-13T11:00:00Z",
        "durationMinutes": 90,
        "track": "Main",
        "isMakeup": false,
        "attendanceStatus": "Present"
      }
    ]
  }
}
```

Error cho frontend:

| Code/Title | HTTP | Message |
| --- | --- | --- |
| `ParentProfile` | 404 | `Parent profile not found` |

Avatar note:

- API nay doc `StudentProfile.AvatarUrl` truc tiep, khong fallback `User.AvatarUrl`.
- FE co the group theo `studentProfileId` neu parent co nhieu be.

## 4. Khuyen nghi tich hop cho frontend

### 4.1. Neu chi can upload avatar

- Goi `POST /api/files/avatar`
- Sau khi success, co the refresh 1 trong cac GET phu hop:
  - `GET /api/me` neu UI dang hien avatar user
  - `GET /api/teacher/profile` neu la man teacher profile
  - `GET /api/student/profile` neu la man student profile
  - `GET /api/parent/timetable` hoac `GET /api/students/timetable` neu UI dang hien timetable co avatar hoc sinh

### 4.2. Neu can update profile + avatar trong 1 lan submit

- Goi `PUT /api/me` voi `multipart/form-data`
- Dung `avatar` cho file
- Dung them `fullName`, `phoneNumber`, `profiles[i].displayName` neu can

### 4.3. Cac diem can tranh nham

- `GET /api/me` chi tra `User.AvatarUrl`, khong tra avatar cua tung profile.
- `GET /api/student/profile` dung avatar o `studentInfo.avatarUrl`, khong nam o root.
- `GET /api/parent/account` co the co ca `user.avatarUrl` va `parentProfile.avatarUrl`; FE can chon dung field theo man hinh.
- `GET /api/students/timetable` va `GET /api/parent/timetable` hien khong fallback sang `User.AvatarUrl`.

## 5. Checklist test nhanh cho FE

1. Upload avatar bang `POST /api/files/avatar` voi file `.png` hop le.
2. Refresh `GET /api/me` hoac GET profile tuong ung va kiem tra field avatar da doi.
3. Kiem tra danh sach hoc sinh `GET /api/classes/{id}/students` render dung `avatarUrl`.
4. Kiem tra attendance `GET /api/attendance/{sessionId}` render dung `studentAvatarUrl`.
5. Kiem tra timetable student/parent render dung `studentAvatarUrl` va khong bi null neu profile da co avatar.
