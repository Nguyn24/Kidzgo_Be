# Student Schedule Conflict API Guide

Tai lieu nay liệt kê cac API co the tra ve loi conflict lich hoc cua student khi he thong phat hien:

- Session bi trung gio voi session khac ma student da duoc assign
- Hoac khoang cach giua 2 session nho hon 15 phut

## Error chung cho FE

- HTTP status: `409 Conflict`
- Problem `title`: `Enrollment.StudentScheduleConflict`
- Problem `detail`: message dong, vi du:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Enrollment.StudentScheduleConflict",
  "status": 409,
  "detail": "Student already has a scheduled session in class 'ENG-A1-01 - English A1' at 15/04/2026 17:30. Sessions must be at least 15 minutes apart."
}
```

Ghi chu:

- `detail` la message dong, se thay doi theo class/session dang bi conflict.
- FE nen check theo `title = Enrollment.StudentScheduleConflict` de hien thi UX on dinh.

## Cac API co the tra ve loi nay

### 1. Create Enrollment

- Endpoint: `POST /api/enrollments`
- Khi nao tra loi nay:
  - Staff ghi danh student vao class moi
  - Va cac session se duoc assign cho enrollment moi bi trung hoac qua sat voi lich hoc hien tai cua student

### 2. Reactivate Enrollment

- Endpoint: `PATCH /api/enrollments/{id}/reactivate`
- Khi nao tra loi nay:
  - Reactivate enrollment tu `Paused` ve `Active`
  - Va cac session duoc khoi phuc cho enrollment nay bi trung hoac qua sat voi lich hoc hien tai cua student

### 3. Update Enrollment

- Endpoint: `PUT /api/enrollments/{id}`
- Khi nao tra loi nay:
  - Update `enrollDate`
  - Hoac update `sessionSelectionPattern`
  - Va tap session sau khi cap nhat bi trung hoac qua sat voi lich hoc hien tai cua student

### 4. Assign Class cho Registration

- Endpoint: `POST /api/registrations/{id}/assign-class`
- Khi nao tra loi nay:
  - Xep class cho registration
  - Va cac session cua class duoc assign bi trung hoac qua sat voi lich hoc hien tai cua student

### 5. Transfer Class

- Endpoint: `POST /api/registrations/{id}/transfer-class`
- Khi nao tra loi nay:
  - Chuyen tu class hien tai sang class moi
  - Va cac session cua class moi, tinh tu `effectiveDate`, bi trung hoac qua sat voi lich hoc con lai cua student

### 6. Update Pause Enrollment Outcome

- Endpoint: `PUT /api/pause-enrollment-requests/{id}/outcome`
- Chi co the tra loi nay khi:
  - `outcome = ContinueSameClass`
  - He thong re-activate lai cac enrollment da pause
  - Va cac session duoc restore bi trung hoac qua sat voi lich hoc hien tai cua student

## Goi y xu ly FE

- Neu `title = Enrollment.StudentScheduleConflict`:
  - Hien thi thong bao conflict cho user
  - Khuyen nghi user chon class khac, doi `sessionSelectionPattern`, doi `enrollDate`, hoac doi `effectiveDate`
- Khong parse cung theo noi dung `detail`, vi day la message dong

