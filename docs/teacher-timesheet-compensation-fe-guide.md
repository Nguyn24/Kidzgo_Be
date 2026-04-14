# Teacher Timesheet Compensation FE Guide

Tai lieu nay mo ta thay doi backend cho phan cong gio/thu nhap cua teacher de frontend co the:

- doc dung du lieu timesheet moi
- quan ly muc cong mac dinh theo nhom teacher
- gan loai teacher de backend tinh dung rate

## 1. Muc tieu nghiep vu

Thu nhap teacher khong con lay tu `PayrollPayments` da chi tra.

Backend gio tinh `income` theo:

- buoi day thuc te
- thoi luong day cua tung buoi
- `SessionRole.PayableUnitPrice` neu buoi do da set rieng
- neu chua set rieng thi fallback sang muc mac dinh theo nhom teacher
- neu chua co muc mac dinh thi fallback tiep tu `Contract.HourlyRate`

## 2. Enum moi

Field moi: `teacherCompensationType`

Gia tri hop le:

- `VietnameseTeacher`
- `ForeignTeacher`
- `Assistant`

Ghi chu:

- day la field gan tren `User`
- neu user khong phai role `Teacher` thi backend se khong giu field nay
- voi session ma user dang vai tro assistant thi backend van uu tien rate `Assistant`

## 3. API timesheet teacher

### Endpoint

- `GET /api/teacher/timesheet?teacherUserId={guid?}&year={number?}`

### Behavior moi

- `income` la thu nhap tinh toan theo buoi day
- khong phai so tien da thanh toan
- response bo sung them thong tin settings de FE co the hien thi ro cach tinh

### Data response

```json
{
  "teacherCompensationType": "ForeignTeacher",
  "standardSessionDurationMinutes": 90,
  "defaultRates": {
    "foreignTeacher": 500000,
    "vietnameseTeacher": 300000,
    "assistant": 150000
  },
  "monthlyData": [
    {
      "month": "2026-04",
      "hours": 18,
      "income": 4200000,
      "rate": 500000,
      "classCount": 12,
      "status": "Open"
    }
  ],
  "yearlySummary": {
    "totalHours": 18,
    "totalIncome": 4200000,
    "averagePerMonth": 4200000,
    "totalClasses": 12
  }
}
```

### Y nghia field

- `teacherCompensationType`: loai teacher dang duoc backend dung de fallback rate
- `standardSessionDurationMinutes`: thoi luong chuan cua 1 buoi dung de prorate
- `defaultRates`: 3 muc cong mac dinh hien hanh
- `monthlyData[].rate`: average session rate cua thang do de FE hien thi tong quan
- `monthlyData[].income`: tong thu nhap da tinh cua thang
- `monthlyData[].hours`: uu tien `MonthlyWorkHours.TotalHours`; neu thang do chua co monthly work hours thi backend fallback tong gio tu session
- `monthlyData[].status`:
  - `Locked`
  - `Open`

## 4. API settings cong mac dinh

FE/Backoffice co the dung 2 API duoi day de xem va cap nhat muc cong mac dinh.

### 4.1 Get settings

- `GET /api/finance/payroll/teacher-compensation-settings`

Response data:

```json
{
  "standardSessionDurationMinutes": 90,
  "foreignTeacherDefaultSessionRate": 500000,
  "vietnameseTeacherDefaultSessionRate": 300000,
  "assistantDefaultSessionRate": 150000
}
```

### 4.2 Update settings

- `PUT /api/finance/payroll/teacher-compensation-settings`

Request body:

```json
{
  "standardSessionDurationMinutes": 90,
  "foreignTeacherDefaultSessionRate": 500000,
  "vietnameseTeacherDefaultSessionRate": 300000,
  "assistantDefaultSessionRate": 150000
}
```

Validation:

- tat ca rate phai `>= 0`
- `standardSessionDurationMinutes > 0`

## 5. API user admin thay doi

Backend da bo sung `teacherCompensationType` trong create/update/get user de FE co the gan loai teacher.

### 5.1 Create user

- `POST /api/admin/users`

Request body bo sung:

```json
{
  "username": "teacher01",
  "name": "Teacher A",
  "email": "teacher@example.com",
  "password": "123456",
  "role": "Teacher",
  "branchId": "guid",
  "phoneNumber": "0901234567",
  "teacherCompensationType": "ForeignTeacher"
}
```

### 5.2 Update user

- `PUT /api/admin/users/{id}`

Request body co the gui:

```json
{
  "teacherCompensationType": "VietnameseTeacher"
}
```

Neu muon clear field:

```json
{
  "teacherCompensationType": ""
}
```

### 5.3 Get user list / detail

Cac API sau gio tra them field:

- `GET /api/admin/users`
- `GET /api/admin/users/{id}`

Field moi:

```json
{
  "teacherCompensationType": "Assistant"
}
```

## 6. Thu tu uu tien tinh rate cua backend

Backend tinh theo thu tu:

1. `SessionRole.PayableUnitPrice` cua chinh session do
2. settings mac dinh theo nhom:
   - `ForeignTeacher`
   - `VietnameseTeacher`
   - `Assistant`
3. `Contract.HourlyRate` quy doi ve `standardSessionDurationMinutes`
4. neu khong co gi thi `0`

Cong thuc quy doi thu nhap:

```text
session_income = session_rate * session_duration_minutes / standard_session_duration_minutes + allowance
```

## 7. Luu y cho FE rollout

- migration moi da tao bang settings va seed san 1 dong mac dinh
- nhung 3 muc rate ban dau dang la `0`
- sau khi deploy, FE/backoffice nen:
  1. cap nhat settings cong mac dinh
  2. gan `teacherCompensationType` cho tung teacher hien co

Neu chua gan `teacherCompensationType` cho teacher cu:

- backend mac dinh coi la `VietnameseTeacher`

## 8. Goi y UI FE

- man hinh timesheet teacher:
  - hien `income` la thu nhap uoc tinh / da tinh theo session
  - khong doi label thanh "paid"
- man hinh cai dat payroll:
  - cho sua 3 muc rate mac dinh
  - cho sua `standardSessionDurationMinutes`
- man hinh admin user:
  - voi role `Teacher`, hien dropdown `teacherCompensationType`
  - voi role khac, an field nay

## 9. Checklist test FE

- teacher `ForeignTeacher`, khong set payable rieng, timesheet ra rate ngoai
- teacher `VietnameseTeacher`, khong set payable rieng, timesheet ra rate Viet
- session assistant, timesheet ra rate assistant
- session co `PayableUnitPrice` rieng, timesheet uu tien gia rieng
- doi settings xong reload timesheet thay doi dung
- teacher cu chua set type, timesheet fallback `VietnameseTeacher`
