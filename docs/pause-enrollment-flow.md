# Pause Enrollment (Bao Luu) API Flow

Tai lieu nay mo ta chi tiet flow bao luu (pause enrollment) de FE tich hop.

## Tong quan
- Bao luu la yeu cau tam dung hoc theo khoang ngay `pause_from` -> `pause_to`.
- Khi duoc APPROVED, he thong tu dong tam dung cac ghi danh ACTIVE co buoi hoc nam trong khoang bao luu.
- Ket qua sau bao luu (outcome) chi la ghi nhan, khong tu dong chuyen lop/thu hoc phi.

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
  "classId": "uuid",
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
  "outcomeAt": "2026-03-17T10:05:00Z | null"
}
```

## Endpoints

### 1. Tao yeu cau bao luu
`POST /api/pause-enrollment-requests`

Body:
```
{
  "studentProfileId": "uuid",
  "classId": "uuid",
  "pauseFrom": "2026-04-01",
  "pauseTo": "2026-04-30",
  "reason": "string | null"
}
```

Response:
- `201 Created` + `PauseEnrollmentRequestResponse`

Validation rules:
- `pauseFrom` >= UTC today
- `pauseTo` >= `pauseFrom`
- Hoc sinh phai co ghi danh ACTIVE o lop do
- Khong duoc trung khi da co request PENDING/APPROVED cho cung hoc sinh + lop

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
- Luon bao gom lop khoi nguon `classId` du khoang bao luu khong co session
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

## Error codes thuong gap
- `PauseEnrollmentRequest.NotFound`
- `PauseEnrollmentRequest.StudentNotFound`
- `PauseEnrollmentRequest.ClassNotFound`
- `PauseEnrollmentRequest.NotEnrolled`
- `PauseEnrollmentRequest.EnrollmentNotActive`
- `PauseEnrollmentRequest.DuplicateActiveRequest`
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

