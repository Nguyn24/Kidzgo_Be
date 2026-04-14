# FE API Contract - Parent Finance and Enrollment Confirmation PDF

Ngay cap nhat: `2026-04-14`

Tai lieu nay ghi lai thay doi moi cho FE:

- `GET /api/parent/overview` bo sung du lieu hoc phi/cong no va tien do goi hoc.
- `POST /api/registrations/{id}/enrollment-confirmation-pdf` xuat phieu xac nhan nhap hoc PDF sau khi hoc vien da duoc xep lop.

## 1. Role va pham vi du lieu

| Role | Du lieu duoc xem | Pham vi | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Parent | Dashboard cua hoc sinh da lien ket voi parent profile hien tai | `own`, theo `studentProfileId` trong query hoac `studentId` trong token | `view` parent overview |
| Admin | Registration, enrollment, PDF xac nhan nhap hoc | `all` theo code hien tai | `view`, `create`, `edit`, `cancel`, `assign`, `transfer`, `upgrade`, `generatePdf`, `regeneratePdf` |
| ManagementStaff | Registration, enrollment, PDF xac nhan nhap hoc | `all` theo code hien tai; chua enforce branch/department filter trong endpoint nay | `view`, `create`, `edit`, `cancel`, `assign`, `transfer`, `upgrade`, `generatePdf`, `regeneratePdf` |
| AccountantStaff | Khong duoc goi endpoint PDF theo attribute hien tai | `none` voi endpoint PDF | Khong co quyen tren endpoint PDF |
| Teacher | Khong duoc goi cac endpoint trong tai lieu nay | `none` | Khong co quyen |
| Student | Khong duoc goi cac endpoint trong tai lieu nay | `none` | Khong co quyen |

Luu y:

- `GET /api/parent/overview` hien chi dung `[Authorize]`, nhung handler bat buoc user hien tai phai co `ProfileType.Parent` active va hoc sinh phai nam trong `ParentStudentLinks`.
- `POST /api/registrations/{id}/enrollment-confirmation-pdf` dung `[Authorize(Roles = "Admin,ManagementStaff")]`.
- Neu can dung pham vi `department/branch` cho `ManagementStaff`, BE can bo sung filter branch trong handler/controller sau. Hien tai endpoint PDF chua enforce dieu nay.

## 2. Permission matrix

| API | Parent | Admin | ManagementStaff | AccountantStaff | Teacher | Student |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/parent/overview` | `view own` | Khong phai flow chinh | Khong phai flow chinh | Khong phai flow chinh | No | No |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | No | `generate/regenerate all` | `generate/regenerate all` | No | No | No |

## 3. Danh sach API

### 3.1. `GET /api/parent/overview`

Muc dich:

- Tra dashboard tong hop cho parent theo hoc sinh da chon.
- FE dung de hien:
  - ten chuong trinh/goi hoc
  - tong so buoi, da hoc, con lai
  - tong cong no chuan tu BE
  - han dong gan nhat va so ngay con lai/qua han

Thay doi moi:

- Them `programName`
- Them `packageName`
- Them `totalSessions`
- Them `usedSessions`
- Them `remainingSessions`
- Them `outstandingAmount`
- Them `nextDueDate`
- Them `daysUntilDue`
- `tuitionDue` cu duoc map ve cung gia tri voi `outstandingAmount` de FE cu khong bi lech.

Quyen:

- Authenticated user.
- User phai co parent profile active.
- Hoc sinh phai linked voi parent profile hien tai.

Pham vi du lieu:

- `own`: chi tra du lieu cua hoc sinh selected trong token hoac `studentProfileId` query.
- Neu parent truyen `studentProfileId` khong linked voi minh, API tra loi `404`.

Params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | Hoc sinh can xem. Neu khong truyen, BE dung `studentId` trong token. |
| `classId` | `Guid?` | No | Loc theo lop active cua hoc sinh. Dong thoi dung de chon registration/class lien quan neu hop le. |
| `sessionId` | `Guid?` | No | Loc upcoming session theo session cu the. |
| `fromDate` | `DateTime?` | No | Ngay bat dau cho cac thong ke theo khoang thoi gian. Mac dinh `now - 1 month`. |
| `toDate` | `DateTime?` | No | Ngay ket thuc cho cac thong ke theo khoang thoi gian. Mac dinh `now + 1 month`. |

Business logic moi:

- So buoi khong suy tu lich hoc hay attendance.
- BE lay `totalSessions`, `usedSessions`, `remainingSessions` tu `Registration`.
- Registration hien hanh duoc chon theo cac status:
  - `Studying`
  - `ClassAssigned`
  - `Paused`
- Neu co nhieu registration hop le, BE uu tien `Studying`, sau do registration co `ActualStartDate`, `ExpectedStartDate`, `RegistrationDate` moi nhat.
- `outstandingAmount` tinh tu invoice status `Pending` va `Overdue`, tru di tong payment da ghi nhan tren invoice.
- `nextDueDate` chi lay tu invoice con no thuc su sau khi tru payment.
- `daysUntilDue = nextDueDate - today` theo ngay Viet Nam. Gia tri am nghia la da qua han.

Response success:

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "statistics": {},
    "studentProfiles": [],
    "classes": [],
    "upcomingSessions": [],
    "recentAttendances": [],
    "makeupCredits": [],
    "pendingHomeworks": [],
    "recentExams": [],
    "reports": [],
    "pendingInvoices": [],
    "activeMissions": [],
    "openTickets": [],
    "studentInfo": {
      "id": "00000000-0000-0000-0000-000000000000",
      "displayName": "Student name",
      "level": 1,
      "totalStars": 20,
      "xp": 100
    },
    "classInfo": {
      "id": "00000000-0000-0000-0000-000000000000",
      "code": "KID-001",
      "title": "Kidz Program",
      "studentProfileId": "00000000-0000-0000-0000-000000000000",
      "status": "Active"
    },
    "attendanceRate": 90.0,
    "homeworkCompletion": 80.0,
    "xp": 100,
    "level": 1,
    "streak": 3,
    "stars": 20,
    "nextClasses": [],
    "pendingApprovals": [],
    "tuitionDue": 1500000,
    "programName": "English Foundation",
    "packageName": "24 sessions",
    "totalSessions": 24,
    "usedSessions": 10,
    "remainingSessions": 14,
    "outstandingAmount": 1500000,
    "nextDueDate": "2026-04-30T00:00:00",
    "daysUntilDue": 16,
    "unreadNotifications": 2
  }
}
```

Response error:

HTTP `401 Unauthorized`

- Token thieu hoac khong hop le.

HTTP `404 Not Found`

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "ParentProfile",
  "status": 404,
  "detail": "not found"
}
```

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Student",
  "status": 404,
  "detail": "Student not linked to this parent"
}
```

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "StudentId",
  "status": 404,
  "detail": "No student selected in token"
}
```

Validation rule:

- `studentProfileId` neu truyen phai la GUID hop le.
- `classId` neu truyen phai la GUID hop le.
- `sessionId` neu truyen phai la GUID hop le.
- `fromDate` va `toDate` phai parse duoc thanh `DateTime`.
- Parent chi duoc xem hoc sinh linked voi parent profile hien tai.

### 3.2. `POST /api/registrations/{id}/enrollment-confirmation-pdf`

Muc dich:

- Xuat phieu xac nhan nhap hoc PDF cho registration da duoc xep lop.
- FE dung `pdfUrl` de hien link tai/xem phieu.
- Neu PDF da ton tai va `regenerate = false`, BE tra lai file cu, khong tao lai.
- Neu `regenerate = true`, BE tao lai PDF va cap nhat `ClassEnrollment.EnrollmentConfirmationPdfUrl`.

Quyen:

- `Admin`
- `ManagementStaff`

Pham vi du lieu:

- `all` theo code hien tai.
- Chua enforce branch/department scope trong endpoint nay.

Endpoint:

```http
POST /api/registrations/{id}/enrollment-confirmation-pdf?track=primary&regenerate=false
```

Params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `id` | `Guid` | Yes | N/A | Registration ID. |
| `track` | `string` | No | `primary` | Track can xuat PDF. Gia tri hop le theo helper hien tai: `primary`, `secondary`. Gia tri khac se fallback ve `primary`. |
| `regenerate` | `bool` | No | `false` | `false`: dung PDF cu neu da co. `true`: tao lai PDF. |

Body:

- Khong co request body.

Business logic:

- BE tim registration theo `{id}`.
- BE normalize `track`:
  - `secondary` -> `RegistrationTrackType.Secondary`
  - cac gia tri khac -> `RegistrationTrackType.Primary`
- BE tim active enrollment theo:
  - `RegistrationId == id`
  - `Track == trackType`
  - `Status == EnrollmentStatus.Active`
- Neu enrollment da co `EnrollmentConfirmationPdfUrl` va `regenerate = false`, BE tra response voi `reusedExistingPdf = true`.
- Neu can tao moi:
  - lay thong tin hoc sinh, phu huynh, chi nhanh, lop, chuong trinh, goi hoc
  - lay ngay hoc dau tien tu `StudentSessionAssignments` status `Assigned`
  - generate PDF bang `IEnrollmentConfirmationPdfGenerator`
  - luu `EnrollmentConfirmationPdfUrl`, `EnrollmentConfirmationPdfGeneratedAt`, `EnrollmentConfirmationPdfGeneratedBy`
  - tra URL download qua `IFileStorageService.GetDownloadUrl`

Response success:

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "00000000-0000-0000-0000-000000000000",
    "enrollmentId": "00000000-0000-0000-0000-000000000001",
    "track": "primary",
    "pdfUrl": "https://cdn.example.com/enrollment-confirmation.pdf",
    "pdfGeneratedAt": "2026-04-14T07:00:00Z",
    "reusedExistingPdf": false,
    "enrollDate": "2026-04-14",
    "firstStudyDate": "2026-04-16",
    "studentName": "Student name",
    "classCode": "KID-001",
    "classTitle": "Kidz Program",
    "programName": "English Foundation",
    "tuitionPlanName": "24 sessions",
    "tuitionAmount": 4800000,
    "currency": "VND"
  }
}
```

Response success khi dung lai PDF cu:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "00000000-0000-0000-0000-000000000000",
    "enrollmentId": "00000000-0000-0000-0000-000000000001",
    "track": "primary",
    "pdfUrl": "https://cdn.example.com/existing.pdf",
    "pdfGeneratedAt": "2026-04-10T07:00:00Z",
    "reusedExistingPdf": true,
    "enrollDate": "2026-04-10",
    "firstStudyDate": "2026-04-12",
    "studentName": "Student name",
    "classCode": "KID-001",
    "classTitle": "Kidz Program",
    "programName": "English Foundation",
    "tuitionPlanName": "24 sessions",
    "tuitionAmount": 4800000,
    "currency": "VND"
  }
}
```

Response error:

HTTP `401 Unauthorized`

- Token thieu hoac khong hop le.

HTTP `403 Forbidden`

- User khong co role `Admin` hoac `ManagementStaff`.

HTTP `404 Not Found`

Registration khong ton tai:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Registration.NotFound",
  "status": 404,
  "detail": "Registration with ID {id} not found"
}
```

Khong co active enrollment cho registration/track:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Registration.EnrollmentNotFound",
  "status": 404,
  "detail": "No active enrollment was found for registration {id} and track 'primary'."
}
```

HTTP `500 Internal Server Error`

Loi generate PDF/file storage:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Server failure",
  "status": 500,
  "detail": "An unexpected error occurred"
}
```

Validation rule:

- `id` phai la GUID hop le.
- `track` nen gui `primary` hoac `secondary`.
- Neu FE gui `track` khac `secondary`, BE hien tai fallback ve `primary`, khong tra validation error.
- `regenerate` phai parse duoc thanh boolean.
- Registration phai ton tai.
- Registration phai co active enrollment dung track.
- Enrollment phai active; enrollment `Paused` hoac `Dropped` khong duoc xuat PDF theo endpoint hien tai.

## 4. Status definition

### 4.1. `RegistrationStatus`

| Status | Y nghia |
| --- | --- |
| `New` | Registration moi tao, chua xep lop. |
| `WaitingForClass` | Dang cho xep lop hoac con track chua co lop. |
| `ClassAssigned` | Da xep lop nhung entry type khong phai vao hoc ngay, vi du `Makeup` hoac `Retake`. |
| `Studying` | Da xep lop va dang hoc. |
| `Paused` | Registration dang bao luu/tam dung. |
| `Completed` | Hoan thanh goi hoc/khoa hoc. |
| `Cancelled` | Da huy registration. |

Luong chuyen trang thai chinh:

```text
New
  -> WaitingForClass
  -> ClassAssigned
  -> Studying
  -> Completed

New/WaitingForClass/ClassAssigned/Studying
  -> Cancelled

Studying
  -> Paused
  -> Studying
```

Luu y theo code hien tai:

- `AssignClass` dung entry type de resolve status:
  - `Immediate` -> `Studying`
  - `Makeup` -> `ClassAssigned`
  - `Retake` -> `ClassAssigned`
  - `Wait` -> `WaitingForClass`
- `MarkAttendance` co the set `Completed` khi `RemainingSessions == 0`.
- `CancelRegistration` set registration ve `Cancelled` va drop active enrollment lien quan.

### 4.2. `EnrollmentStatus`

| Status | Y nghia |
| --- | --- |
| `Active` | Hoc vien dang active trong lop. |
| `Paused` | Enrollment dang tam dung/bao luu. |
| `Dropped` | Hoc vien da roi lop/drop enrollment. |

Luong chuyen trang thai chinh:

```text
Active -> Paused -> Active
Active -> Dropped
Paused -> Dropped
```

Rang buoc voi PDF:

- API enrollment confirmation PDF chi chap nhan enrollment `Active`.

### 4.3. `InvoiceStatus`

| Status | Y nghia |
| --- | --- |
| `Pending` | Hoa don dang cho thanh toan. |
| `Paid` | Hoa don da thanh toan. |
| `Overdue` | Hoa don da qua han. |
| `Cancelled` | Hoa don da huy. |

Lien quan `GET /api/parent/overview`:

- `outstandingAmount` chi tinh invoice `Pending` va `Overdue`.
- `Paid` va `Cancelled` khong tinh vao cong no.
- Invoice `Pending/Overdue` nhung da du payment thi khong tinh vao `nextDueDate`.

### 4.4. `RegistrationTrackType`

| API value | Domain value | Y nghia |
| --- | --- | --- |
| `primary` | `Primary` | Track chuong trinh chinh. |
| `secondary` | `Secondary` | Track chuong trinh phu/secondary program. |

## 5. Cac truong hop tra loi loi

| API | Case | HTTP | Code/Title |
| --- | --- | --- | --- |
| `GET /api/parent/overview` | Chua login/token sai | `401` | Standard auth error |
| `GET /api/parent/overview` | User khong co parent profile active | `404` | `ParentProfile` |
| `GET /api/parent/overview` | Khong co selected student | `404` | `StudentId` |
| `GET /api/parent/overview` | Student khong linked voi parent | `404` | `Student` |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Chua login/token sai | `401` | Standard auth error |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Khong co role `Admin,ManagementStaff` | `403` | Standard authorization error |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Registration khong ton tai | `404` | `Registration.NotFound` |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Khong co active enrollment theo track | `404` | `Registration.EnrollmentNotFound` |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Loi generate PDF/file storage | `500` | `Server failure` |

## 6. Ghi chu cho FE

- FE khong can tu tinh `remainingSessions` tu attendance nua; dung truc tiep `data.remainingSessions`.
- FE nen dung `data.outstandingAmount` cho tong no; `data.tuitionDue` chi la alias/backward compatibility.
- FE nen dung `data.nextDueDate` va `data.daysUntilDue` thay vi tu doan invoice nao la ky dong chinh.
- Neu `daysUntilDue < 0`, co the hien thi qua han.
- Neu `nextDueDate = null`, hien tai khong co invoice con no co due date.
- Khi goi PDF, neu FE muon ep tao lai file moi thi truyen `regenerate=true`.
- Neu registration co secondary track, FE can truyen `track=secondary` de xuat dung phieu cua enrollment secondary.
