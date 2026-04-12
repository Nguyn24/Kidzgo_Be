# Update Nhung Chinh Sua API Ngay 12-4

Ngay cap nhat: `2026-04-12`

Muc dich tai lieu:

- Tong hop cac API da thay doi trong phien lam viec ngay `12/4`
- Mo ta impact cho frontend
- Chuan hoa cach goi API, request, response, error message
- Lam tai lieu append duoc cho cac API se cap nhat tiep theo

Luu y chung:

- Success response thuong theo format:

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
  "title": "Pin.NotSet",
  "status": 409,
  "detail": "PIN has not been set yet.",
  "traceId": "..."
}
```

## 1. Danh sach API da update

### Auth / Parent PIN

- `PUT /api/auth/change-pin`
- `POST /api/auth/profiles/request-pin-reset`
- `POST /api/auth/profiles/request-pin-reset-zalo-otp`
- `POST /api/auth/profiles/verify-pin-reset-zalo-otp`
- `POST /api/auth/reset-pin`
- `POST /api/auth/profiles/verify-parent-pin`

### Parent

- `GET /api/parent/timetable`
- `GET /api/parent/media`

### Media

- `GET /api/media`
- `GET /api/media/{id}`

### Homework

- `POST /api/homework/multiple-choice`
- `POST /api/homework/multiple-choice/from-bank`

### Exam

- `POST /api/exams/{examId}/questions/from-bank-matrix`

### Placement Test

- `POST /api/placement-tests/{id}/questions/from-bank-matrix`

## 2. Auth / Parent PIN

### 2.1. `PUT /api/auth/change-pin`

Muc dich:

- Parent doi PIN bang `currentPin`
- Runtime hien chi check `ParentProfile.PinHash`
- Khong con dung `User.PinHash`

Role:

- `Parent`

Auth:

- `Bearer token` bat buoc

Request body:

```json
{
  "currentPin": "1234",
  "newPin": "1111"
}
```

Field:

| Field | Type | Required |
| --- | --- | --- |
| `currentPin` | `string` | Yes |
| `newPin` | `string` | Yes |

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
| `Profile.Invalid` | 400/404 theo mapper | `Profile is invalid.` |
| `Pin.Invalid` | 400 | `PIN must be numeric and less than 10 digits.` |
| `Pin.NotSet` | 409 | `PIN has not been set yet.` |
| `Pin.Wrong` | 409 | `PIN is incorrect.` |

FE note:

- Neu user chua tung set PIN, frontend nen chuyen sang flow `verify-parent-pin` lan dau thay vi `change-pin`.

### 2.2. `POST /api/auth/profiles/verify-parent-pin`

Muc dich:

- Verify PIN cua parent
- Neu `profile.PinHash` chua co, API se xem day la flow set PIN lan dau va luu PIN do

Role:

- `Parent`

Auth:

- `Bearer token` bat buoc

Request body:

```json
{
  "profileId": "guid",
  "pin": "1234"
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
| `Profile.Invalid` | 400/404 | `Profile is invalid.` |
| `Pin.Invalid` | 400 | `PIN must be numeric and less than 10 digits.` |
| `Pin.Wrong` | 409 | `PIN is incorrect.` |

FE note:

- Day la endpoint co the dung cho `set PIN lan dau` hoac `verify PIN da co`.

### 2.3. `POST /api/auth/profiles/request-pin-reset`

Muc dich:

- Request forgot PIN qua email
- Khong can request body
- Backend tu resolve parent profile cua current user

Role:

- `Parent`

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

| Code | HTTP | Message |
| --- | --- | --- |
| `Profile.Invalid` | 400/404 | `Profile is invalid.` |
| `Profile.EmailNotSet` | 400 | `Email is required for PIN reset.` |

FE note:

- Flow thuc te la backend gui email reset link, frontend khong can truyen `profileId`.

### 2.4. `POST /api/auth/profiles/request-pin-reset-zalo-otp`

Muc dich:

- Request forgot PIN qua Zalo OTP
- Khong can request body

Role:

- `Parent`

Auth:

- `Bearer token` bat buoc

Request:

- Body rong

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "challengeId": "guid",
    "otpExpiresAt": "2026-04-12T10:10:00Z"
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Profile.Invalid` | 400/404 | `Profile is invalid.` |
| `Profile.ZaloIdNotSet` | 400 | `Zalo ID is required for Zalo OTP PIN reset.` |
| `Profile.ZaloOtpSendFailed` | 500/400 theo mapper | `Unable to send Zalo OTP for PIN reset.` |

FE note:

- Luu `challengeId` de verify OTP
- UI nen hien countdown theo `otpExpiresAt`

### 2.5. `POST /api/auth/profiles/verify-pin-reset-zalo-otp`

Muc dich:

- Verify OTP va lay `resetToken`

Role:

- `Anonymous`

Auth:

- Khong can token

Request body:

```json
{
  "challengeId": "guid",
  "otp": "123456"
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "resetToken": "token-string",
    "expiresAt": "2026-04-12T11:00:00Z"
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Pin.InvalidResetToken` | 400 | `PIN reset token is invalid or has expired.` |
| `Profile.Invalid` | 400/404 | `Profile is invalid.` |
| `Pin.InvalidOtp` | 400 | `OTP is invalid or has expired.` |

FE note:

- Sau khi verify thanh cong, frontend redirect sang man hinh `reset-pin` va dung `resetToken`.

### 2.6. `POST /api/auth/reset-pin`

Muc dich:

- Reset PIN bang token tu email hoac Zalo OTP flow

Role:

- `Anonymous`

Auth:

- Khong can token

Request body:

```json
{
  "token": "reset-token",
  "newPin": "1111"
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
| `Pin.Invalid` | 400 | `PIN must be numeric and less than 10 digits.` |
| `Pin.InvalidResetToken` | 400 | `PIN reset token is invalid or has expired.` |
| `Pin.OtpNotVerified` | 400 | `OTP has not been verified for this PIN reset request.` |
| `Profile.Invalid` | 400/404 | `Profile is invalid.` |

## 3. Parent

### 3.1. `GET /api/parent/timetable`

Muc dich:

- Lay timetable cua tat ca student linked voi parent hien tai
- Da sua de khong con chi tra 1 student context

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
        "studentDisplayName": "Be A",
        "studentAvatarUrl": "https://...",
        "color": "#F7B267",
        "classId": "guid",
        "classCode": "KID-A1",
        "classTitle": "KID A1",
        "plannedDatetime": "2026-04-12T09:00:00Z",
        "durationMinutes": 90,
        "track": "Main",
        "isMakeup": false,
        "attendanceStatus": "Present"
      }
    ]
  }
}
```

FE note:

- Frontend phai render theo `sessions[]`
- Moi item hien da co thong tin student, khong can group bang API khac nua
- Neu muon group theo tung be, FE group bang `studentProfileId`

## 4. Homework

### 4.1. `POST /api/homework/multiple-choice`

Muc dich:

- Tao homework multiple choice thu cong
- Da bo sung `skills` va `attachment`
- Neu la listening quiz, co the truyen audio URL trong `attachment`

Role:

- `Teacher`, `ManagementStaff`, `Admin`

Auth:

- `Bearer token` bat buoc

Request body:

```json
{
  "classId": "guid",
  "sessionId": "guid-or-null",
  "title": "Listening Quiz Unit 3",
  "description": "Listen and choose",
  "dueAt": "2026-04-14T17:00:00Z",
  "skills": "Listening",
  "topic": "Unit 3",
  "rewardStars": 5,
  "timeLimitMinutes": 15,
  "maxAttempts": 1,
  "allowResubmit": false,
  "instructions": "Nghe audio va chon dap an",
  "attachment": "https://cdn.example.com/audio/unit-3.mp3",
  "questions": [
    {
      "questionText": "What time does class start?",
      "questionType": "MultipleChoice",
      "options": ["7:00", "7:30", "8:00", "8:30"],
      "correctAnswer": "1",
      "points": 1,
      "explanation": "It starts at 7:30"
    }
  ]
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "title": "Listening Quiz Unit 3",
    "skills": "Listening",
    "attachmentUrl": "https://cdn.example.com/audio/unit-3.mp3",
    "isListeningQuiz": true,
    "questions": [
      {
        "id": "guid",
        "orderIndex": 0,
        "questionText": "What time does class start?",
        "questionType": "MultipleChoice",
        "options": ["7:00", "7:30", "8:00", "8:30"],
        "points": 1
      }
    ]
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Homework.InvalidTitle` | 400 | `Title cannot be empty or whitespace` |
| `Homework.InvalidDueDate` | 400 | `Due date must be in the future` |
| `Homework.InvalidTimeLimitMinutes` | 400 | `TimeLimitMinutes must be greater than 0` |
| `Homework.InvalidMaxAttempts` | 400 | `MaxAttempts must be greater than 0` |
| `Homework.NoQuestionsProvided` | 400 | `At least one question is required for multiple choice homework` |
| `Homework.InvalidCorrectAnswer` | 400 | `Question n has invalid correct answer index` |
| `Homework.ClassNotFound` | 404 | `Class not found or inactive` |

### 4.2. `POST /api/homework/multiple-choice/from-bank`

Muc dich:

- Tao homework multiple choice tu `QuestionBank`
- Da filter duoc theo `skills`, `topic`
- Dung distribution theo do kho

Role:

- `Teacher`, `ManagementStaff`, `Admin`

Auth:

- `Bearer token` bat buoc

Request body:

```json
{
  "classId": "guid",
  "programId": "guid",
  "sessionId": null,
  "title": "Unit 3 Bank Quiz",
  "description": "Auto generated from question bank",
  "dueAt": "2026-04-14T17:00:00Z",
  "skills": "Listening",
  "topic": "Unit 3",
  "attachment": "https://cdn.example.com/audio/unit-3.mp3",
  "distribution": [
    { "level": "Easy", "count": 4 },
    { "level": "Medium", "count": 4 },
    { "level": "Hard", "count": 2 }
  ]
}
```

Success response:

- Cung shape voi `POST /api/homework/multiple-choice`
- Khac o cho `questions[]` duoc tao tu `QuestionBank`

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Homework.ProgramNotFound` | 404 | `Program with Id = '...' was not found` |
| `Homework.InvalidQuestionDistribution` | 400 | `Question distribution must have at least one level with count > 0` |
| `Homework.InsufficientQuestionsInBank` | 400 | `Not enough questions in bank for level ...` |
| `Homework.ClassHasNoActiveStudents` | 400 | `Class with Id = '...' has no active enrolled students` |

FE note:

- `homework binh thuong` khong dung matrix
- Matrix hien chi ap dung cho `multiple choice homework`

## 5. Media

### 5.1. `GET /api/media`

Muc dich:

- Lay danh sach media
- Student/Parent nay da duoc sua de xem:
  - media ca nhan gan truc tiep cho student
  - media lop cua cac lop student dang `Active enrollment`
- Bo sung filter `studentProfileId` va `date`

Role:

- `Student`, `Parent`, `Teacher`, `ManagementStaff`, `Admin`

Auth:

- `Bearer token` bat buoc

Query params:

| Param | Type | Required |
| --- | --- | --- |
| `branchId` | `guid` | No |
| `classId` | `guid` | No |
| `studentProfileId` | `guid` | No |
| `monthTag` | `string` | No |
| `date` | `datetime` | No |
| `type` | `MediaType` | No |
| `contentType` | `MediaContentType` | No |
| `visibility` | `Visibility` | No |
| `approvalStatus` | `ApprovalStatus` | No |
| `isPublished` | `bool` | No |
| `pageNumber` | `int` | No |
| `pageSize` | `int` | No |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "media": {
      "items": [
        {
          "id": "guid",
          "classId": "guid",
          "className": "KID-A1",
          "studentProfileId": "guid-or-null",
          "studentName": "Be A",
          "monthTag": "2026-04",
          "type": "Photo",
          "contentType": "Image",
          "url": "https://cdn.example.com/media/a.jpg",
          "caption": "Session 1",
          "createdAt": "2026-04-12T09:30:00Z"
        }
      ],
      "pageNumber": 1,
      "pageSize": 20,
      "totalCount": 12
    }
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Profile.StudentNotFound` | 404/400 theo mapper | `Student profile not found.` |

FE note:

- `Student` khong can gui `studentProfileId`
- `Parent` co the gui `studentProfileId` de chon tung be
- `date` filter theo ngay tao media, backend match theo khoang `00:00:00 -> 23:59:59`
- Neu media la `class media`, item co the co `studentProfileId = null`

### 5.2. `GET /api/media/{id}`

Muc dich:

- Lay chi tiet media
- Da sua de detail endpoint dung cung rule access voi list:
  - student chi doc duoc media ca nhan cua minh hoac media lop minh dang hoc
  - parent chi doc duoc media cua cac child linked hoac media lop cua cac child do

Role:

- `Student`, `Parent`, `Teacher`, `ManagementStaff`, `Admin`

Auth:

- `Bearer token` bat buoc

Success response:

- Cung shape voi truoc day, chi thay doi phan kiem tra access

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `Media.NotFound` | 404 | `Media with Id = '...' was not found` |

FE note:

- Frontend khong nen cache assumption rang co id la doc duoc; detail nay gio se 404 neu media khong nam trong pham vi child/class cho phep

### 5.3. `GET /api/parent/media`

Muc dich:

- Parent lay album/items media cua student duoc linked
- Da sua de tra ca media ca nhan va media lop cua student do
- Bo sung filter `classId`, `monthTag`, `date`

Role:

- `Parent`

Auth:

- `Bearer token` bat buoc

Query params:

| Param | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `guid` | No |
| `classId` | `guid` | No |
| `monthTag` | `string` | No |
| `date` | `datetime` | No |
| `type` | `string` | No |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "albums": [
      {
        "albumId": "2026-04",
        "title": "2026-04",
        "type": "Photo",
        "date": "2026-04-12T09:30:00Z",
        "coverUrl": "https://cdn.example.com/media/a.jpg",
        "count": 8
      }
    ],
    "items": [
      {
        "id": "guid",
        "albumId": "2026-04",
        "title": "Session 1",
        "type": "Photo",
        "date": "2026-04-12T09:30:00Z",
        "coverUrl": "https://cdn.example.com/media/a.jpg",
        "count": 1,
        "url": "https://cdn.example.com/media/a.jpg"
      }
    ]
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `StudentProfile` | 404 | `Student profile not linked to current parent` |

FE note:

- Neu parent co nhieu child, frontend nen truyen `studentProfileId` ro rang de tranh lay nham child dang selected trong token
- `monthTag` va `date` co the dung dong thoi neu can loc album theo thang roi tiep tuc loc ngay

## 6. Exam

### 6.1. `POST /api/exams/{examId}/questions/from-bank-matrix`

Muc dich:

- Tao bo cau hoi exam tu `QuestionBank` theo matrix do kho
- Dong bo lai `Exam.MaxScore`
- Chan regenerate neu exam da co submissions

Role:

- `Teacher`, `ManagementStaff`, `Admin`

Auth:

- `Bearer token` bat buoc

Request body:

```json
{
  "totalQuestions": 10,
  "questionType": "MultipleChoice",
  "skill": "Listening",
  "topic": "Unit 3",
  "replaceExistingQuestions": true,
  "shuffleQuestions": true,
  "distribution": [
    { "level": "Easy", "count": 4 },
    { "level": "Medium", "count": 4 },
    { "level": "Hard", "count": 2 }
  ]
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "examId": "guid",
    "classId": "guid",
    "programId": "guid",
    "examType": "Progress",
    "questionType": "MultipleChoice",
    "skill": "Listening",
    "topic": "Unit 3",
    "requestedQuestionCount": 10,
    "createdQuestionCount": 10,
    "totalPoints": 10,
    "replacedExistingQuestions": true,
    "previousQuestionCount": 0,
    "distribution": [
      { "level": "Easy", "requestedCount": 4, "createdCount": 4 }
    ],
    "questions": [
      {
        "id": "guid",
        "sourceQuestionBankItemId": "guid",
        "orderIndex": 1,
        "questionText": "Question text",
        "questionType": "MultipleChoice",
        "level": "Easy",
        "points": 1,
        "options": ["A", "B", "C", "D"],
        "correctAnswer": "B",
        "explanation": "..."
      }
    ]
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `ExamQuestion.ExamNotFound` | 404 | `Exam with Id = '...' was not found` |
| `ExamQuestion.InvalidMatrixDistribution` | 400 | `Question distribution must have at least one level with count > 0` |
| `ExamQuestion.MatrixTotalMismatch` | 400 | `TotalQuestions = x does not match distributed total = y` |
| `ExamQuestion.UnsupportedQuestionBankType` | 400 | `Question bank matrix currently supports only MultipleChoice and Text question types` |
| `ExamQuestion.CannotRegenerateWhenSubmissionsExist` | 409 | `Cannot regenerate exam questions because this exam already has submissions` |
| `ExamQuestion.InsufficientQuestionsInBank` | 400 | `Not enough question bank items for level 'Easy'. Required 4, available 2` |

FE note:

- Neu exam da co bai lam, frontend nen disable nut regenerate
- `replaceExistingQuestions = false` co the dung de append them cau

## 7. Placement Test

### 7.1. `POST /api/placement-tests/{id}/questions/from-bank-matrix`

Muc dich:

- Tao question set cho placement test tu `QuestionBank`
- Resolve `ProgramId` theo thu tu:
  1. `request.programId`
  2. `placementTest.programRecommendationId`
  3. `placementTest.class.programId`

Role:

- `Admin`, `ManagementStaff`

Auth:

- `Bearer token` bat buoc

Request body:

```json
{
  "programId": "guid-or-null",
  "questionType": "MultipleChoice",
  "skill": "Listening",
  "topic": "Starter",
  "shuffleQuestions": true,
  "totalQuestions": 10,
  "distribution": [
    { "level": "Easy", "count": 4 },
    { "level": "Medium", "count": 4 },
    { "level": "Hard", "count": 2 }
  ]
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "placementTestId": "guid",
    "programId": "guid",
    "programSource": "Request",
    "questionType": "MultipleChoice",
    "skill": "Listening",
    "topic": "Starter",
    "requestedQuestionCount": 10,
    "createdQuestionCount": 10,
    "totalPoints": 10,
    "distribution": [
      { "level": "Easy", "requestedCount": 4, "createdCount": 4 }
    ],
    "questions": [
      {
        "sourceQuestionBankItemId": "guid",
        "orderIndex": 1,
        "questionText": "Question text",
        "questionType": "MultipleChoice",
        "level": "Easy",
        "points": 1,
        "options": ["A", "B", "C", "D"],
        "correctAnswer": "B",
        "explanation": "..."
      }
    ]
  }
}
```

Error cho frontend:

| Code | HTTP | Message |
| --- | --- | --- |
| `PlacementTest.NotFound` | 404 | `The placement test with the Id = '...' was not found` |
| `PlacementTest.ProgramNotResolved` | 400 | `Cannot resolve program for placement test '...'. Provide ProgramId explicitly or set ProgramRecommendation/Class first.` |
| `PlacementTest.InvalidQuestionMatrixDistribution` | 400 | `Question distribution must have at least one level with count > 0` |
| `PlacementTest.MatrixTotalMismatch` | 400 | `TotalQuestions = x does not match distributed total = y` |
| `PlacementTest.InsufficientQuestionsInBank` | 400 | `Not enough question bank items for level 'Easy'. Required 4, available 2` |

FE note:

- Flow nay hien tra `question set` de frontend/nhan vien dung trong placement test context
- Hien chua persist thanh exam rieng

## 8. Frontend checklist

### 8.1. PIN flow

- Dung `verify-parent-pin` cho lan dau set PIN
- Dung `change-pin` cho flow doi PIN khi da co PIN
- Dung `request-pin-reset` neu reset qua email
- Dung `request-pin-reset-zalo-otp` -> `verify-pin-reset-zalo-otp` -> `reset-pin` neu reset qua Zalo OTP

### 8.2. Timetable parent

- Render `sessions[]` cua tat ca child
- Neu can tab theo be, group bang `studentProfileId`

### 8.3. Homework listening quiz

- Neu `isListeningQuiz = true`, FE phai render audio player tu `attachmentUrl`
- `skills` va `attachment` da co ngay trong API create

### 8.4. Matrix flows

- `exam matrix` va `placement test matrix` deu co `distribution[]`
- Frontend can validate tong `count` truoc khi submit de tranh loi `MatrixTotalMismatch`

### 8.5. Media

- `Student` va `Parent` da xem duoc media lop theo `ClassEnrollment.Active`, khong chi media gan truc tiep vao `StudentProfileId`
- Neu parent co nhieu child, FE nen truyen `studentProfileId` khi goi `GET /api/parent/media`
- Dung `monthTag` de group theo thang, dung `date` neu can loc ngay cu the
- Khi mo `GET /api/media/{id}`, FE phai xu ly `404 Media.NotFound` nhu mot truong hop khong con quyen truy cap hop le

## 9. Mau them update API tiep theo

Khi co API moi sau ngay `12/4`, append theo format:

1. `## <nhom module>`
2. `### <METHOD> <endpoint>`
3. `Muc dich`
4. `Role`
5. `Request body` + JSON sample
6. `Success response` + JSON sample
7. `Error cho frontend` + bang `Code / HTTP / Message`
