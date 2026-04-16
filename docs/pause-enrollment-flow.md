# Pause Enrollment (Bao Luu) API Flow

Tai lieu nay mo ta chi tiet flow bao luu (pause enrollment) de FE tich hop.

## Tong quan
- Bao luu la yeu cau tam dung hoc theo khoang ngay `pause_from` -> `pause_to`.
- Khi tao request, he thong tu quet cac lop/buoi hoc cua hoc sinh trong khoang bao luu.
- Khi duoc APPROVED, he thong tu dong tam dung cac ghi danh ACTIVE co buoi hoc nam trong khoang bao luu.
- Ket qua sau bao luu:
  - `ContinueSameClass`: he thong tu dong re-activate lai enrollment cu va restore assignment tu sau `pauseTo`.
  - `ReassignEquivalentClass`: he thong khong tu dong xep lop moi. Enrollment cu bi drop tu `pauseFrom` tro di, assignment lop cu tu `pauseFrom` den tuong lai bi huy, staff phai them hoc sinh vao lop khac thu cong.
  - `ContinueWithTutoring`: chi ghi nhan outcome va huy assignment lop thuong tu sau `pauseTo`, staff phai tu van/goi hoc rieng thu cong.

## Dinh dang ngay
- `PauseFrom`/`PauseTo` dung `DateOnly` theo format `YYYY-MM-DD`.
- Kiem tra ngay su dung `DateTime.UtcNow.Date` (UTC date).

## Enum
- `PauseEnrollmentRequestStatus`: `Pending`, `Approved`, `Rejected`, `Cancelled`
- `PauseEnrollmentOutcome`: `ContinueSameClass`, `ReassignEquivalentClass`, `ContinueWithTutoring`

## Base URL
`/api/pause-enrollment-requests`

## Authorization
- Tao / xem / huy: yeu cau da dang nhap.
- Approve / Reject / Outcome / Approve-bulk: `Admin` hoac `ManagementStaff`.

## Response model (PauseEnrollmentRequestResponse)
```
{
  "id": "uuid",
  "studentProfileId": "uuid",
  "classId": "uuid | null",
  "pauseFrom": "YYYY-MM-DD",
  "pauseTo": "YYYY-MM-DD",
  "reason": "string | null",
  "status": "Pending|Approved|Rejected|Cancelled",
  "requestedAt": "2026-03-17T10:00:00Z",
  "approvedBy": "uuid | null",
  "approvedAt": "2026-03-17T10:05:00Z | null",
  "cancelledBy": "uuid | null",
  "cancelledAt": "2026-03-17T10:05:00Z | null",
  "outcome": "ContinueSameClass|ReassignEquivalentClass|ContinueWithTutoring | null",
  "outcomeNote": "string | null",
  "outcomeBy": "uuid | null",
  "outcomeAt": "2026-03-17T10:05:00Z | null",
  "classes": [
    {
      "id": "uuid",
      "code": "ENG-L1-2024-01",
      "title": "English Level 1",
      "programId": "uuid",
      "programName": "English Level 1",
      "branchId": "uuid",
      "branchName": "Hanoi",
      "startDate": "2026-01-01",
      "endDate": "2026-06-01",
      "status": "Active"
    }
  ]
}
```

## Response model (CreatePauseEnrollmentRequestResponse - POST only)
```
{
  "id": "uuid",
  "studentProfileId": "uuid",
  "pauseFrom": "YYYY-MM-DD",
  "pauseTo": "YYYY-MM-DD",
  "reason": "string | null",
  "status": "Pending",
  "requestedAt": "2026-03-17T10:00:00Z",
  "classes": [
    {
      "id": "uuid",
      "code": "ENG-L1-2024-01",
      "title": "English Level 1",
      "programId": "uuid",
      "programName": "English Level 1",
      "branchId": "uuid",
      "branchName": "Hanoi",
      "startDate": "2026-01-01",
      "endDate": "2026-06-01",
      "status": "Active"
    }
  ]
}
```

## Endpoints

### 1. Tao yeu cau bao luu
`POST /api/pause-enrollment-requests`

Body:
```
{
  "studentProfileId": "uuid",
  "pauseFrom": "2026-04-01",
  "pauseTo": "2026-04-30",
  "reason": "string | null"
}
```

Response:
- `201 Created` + `CreatePauseEnrollmentRequestResponse`

Validation rules:
- `pauseFrom` >= UTC today
- `pauseTo` >= `pauseFrom`
- Hoc sinh phai co ghi danh ACTIVE va co buoi hoc trong khoang bao luu
- Khong duoc trung khi da co request PENDING/APPROVED bi chong lap thoi gian voi cung hoc sinh

### 2. Danh sach yeu cau
`GET /api/pause-enrollment-requests`

Query:
- `studentProfileId` (optional)
- `classId` (optional)
- `status` (optional)
- `branchId` (optional)
- `pageNumber` (default 1)
- `pageSize` (default 10)

Response:
```
{
  "items": [ ...PauseEnrollmentRequestResponse ],
  "pageNumber": 1,
  "totalPages": 3,
  "totalCount": 25
}
```

### 3. Chi tiet
`GET /api/pause-enrollment-requests/{id}`

Response:
- `200 OK` + `PauseEnrollmentRequestResponse`

### 4. Duyet (approve)
`PUT /api/pause-enrollment-requests/{id}/approve`

Response:
- `200 OK` + `PauseEnrollmentRequestResponse`

Behavior:
- `status = Approved`, set `approvedBy/approvedAt`
- Tu dong pause cac ghi danh ACTIVE co buoi hoc nam trong khoang bao luu
- Neu request co `classId` (legacy), se luon bao gom lop khoi nguon du khoang bao luu khong co session
- Ghi lich su vao `pause_enrollment_request_histories` cho moi ghi danh bi anh huong

### 5. Duyet hang loat
`PUT /api/pause-enrollment-requests/approve-bulk`

Body:
```
{
  "ids": ["uuid1", "uuid2"]
}
```

Response:
```
{
  "approvedIds": ["uuid1"],
  "errors": [
    { "id": "uuid2", "code": "PauseEnrollmentRequest.AlreadyApproved", "message": "..." }
  ]
}
```

### 6. Tu choi (reject)
`PUT /api/pause-enrollment-requests/{id}/reject`

Response:
- `200 OK` + `PauseEnrollmentRequestResponse`

### 7. Huy yeu cau
`PUT /api/pause-enrollment-requests/{id}/cancel`

Response:
- `200 OK` + `PauseEnrollmentRequestResponse`

Rules:
- Chi huy khi `status = Pending` va ngay hien tai (UTC) < `pauseFrom`

### 8. Cap nhat ket qua sau bao luu
`PUT /api/pause-enrollment-requests/{id}/outcome`

Body:
```
{
  "outcome": "ContinueSameClass",
  "outcomeNote": "string | null"
}
```

Response:
- `200 OK` + `PauseEnrollmentRequestResponse`

Rules:
- Chi cho phep khi `status = Approved`
- Neu `outcome = ContinueSameClass`:
  - He thong doi cac enrollment bi pause boi request nay tu `Paused` -> `Active`
  - Ghi them lich su `Paused -> Active` vao `pause_enrollment_request_histories`
  - Restore `student_session_assignments` tu ngay lon hon giua `pauseTo + 1 ngay` va `UTC today`

## Error codes thuong gap
- `PauseEnrollmentRequest.NotFound`
- `PauseEnrollmentRequest.StudentNotFound`
- `PauseEnrollmentRequest.ClassNotFound`
- `PauseEnrollmentRequest.NotEnrolled`
- `PauseEnrollmentRequest.EnrollmentNotActive`
- `PauseEnrollmentRequest.DuplicateActiveRequest`
- `PauseEnrollmentRequest.NoEnrollmentsInRange`
- `PauseEnrollmentRequest.AlreadyApproved`
- `PauseEnrollmentRequest.AlreadyRejected`
- `PauseEnrollmentRequest.AlreadyCancelled`
- `PauseEnrollmentRequest.CancelWindowExpired`
- `PauseEnrollmentRequest.OutcomeNotAllowed`

## Flow goi y cho FE

1. Parent tao yeu cau → status `Pending`
2. Staff approve → status `Approved`, he thong pause ghi danh + ghi lich su
3. Sau khi hoc sinh quay lai:
   - Staff cap nhat outcome (ContinueSameClass / ReassignEquivalentClass / ContinueWithTutoring)
