# Luong Quan Ly Dang Ky Hoc - Registration Flow Guide

## Tong quan

Tai lieu nay huong dan cach su dung he thong quan ly dang ky hoc theo luong moi. Luong moi tach hai phan:

1. **Phan thiet lap**: Chuong trinh -> Goi hoc -> Lop hoc -> Lich hoc / Phong / Giao vien
2. **Phan nghiep vu van hanh**: Dang ky hoc vien -> Go y lop phu hop -> Xep lop -> Hoc vu phat sinh

---

## Cac thanh phan chinh

### 1. Chuong trinh (Program)
La noi dung dao tao, vi du: Starter, Movers, Flyers

### 2. Goi hoc (TuitionPlan)
La goi ban cho hoc sinh, vi du:
- Starter 3 thang
- Starter 6 thang
- Starter 12 thang

Goi hoc chua:
- Chuong trinh
- Thoi luong
- Hoc phi
- So buoi chuan

### 3. Lop hoc (Class)
La noi hoc that, vi du: Starter A, Starter B, Starter Weekend

Lop hoc gan voi:
- Chuong trinh
- Giao vien
- Chi nhanh
- Lich hoc
- Phong hoc
- Suc chua
- Trang thai hoat dong

### 4. Dang ky hoc (Registration)
La ban ghi ghi nhan:
- Hoc vien dang ky chuong trinh nao
- Dang ky goi nao
- Ngay dang ky
- Ngay bat dau hoc du kien
- Nhu cau ca hoc
- Chi nhanh
- Trang thai xep lop

---

## Trang thai Dang ky (RegistrationStatus)

| Gia tri | Mo ta | Mo ta chi tiet |
|---------|--------|----------------|
| 0 - New | Moi tao | Dang ky moi, chua xep lop |
| 1 - WaitingForClass | Cho xep lop | Khong co lop phu hop, cho admin xep |
| 2 - Studying | Dang hoc | Dang theo hoc tich cuc |
| 4 - Paused | Bao luu | Tam nguong hoc |
| 5 - Completed | Hoan thanh | Da het buoi hoc |
| 6 - Cancelled | Huy | Dang ky bi huy |

---

## Trang thai Lop hoc (ClassStatus)

| Gia tri | Mo ta | Mo ta chi tiet |
|---------|--------|----------------|
| 0 - Planned | Sap khai giang | Lop da len ke hoach |
| 1 - Recruiting | Dang tuyen sinh | Dang nhan hoc vien |
| 2 - Active | Dang hoc | Dang trong qua trinh giang day |
| 3 - Full | Da day | Khong nhan them hoc vien |
| 4 - Completed | Ket thuc | Lop da hoan thanh khoa hoc |
| 5 - Suspended | Tam ngung | Lop tam ngung hoat dong |
| 6 - Cancelled | Huy | Lop bi huy |

---

## EntryType - Cach nhap hoc

| Gia tri | Mo ta | Khi nao dung |
|---------|--------|--------------|
| `immediate` | Vao hoc ngay | Hoc vien nhap hoc tu dau hoac giua chung, tham gia cac buoi con lai |

| `wait` | Cho lop moi | Khong co lop phu hop, cho admin xep lop thu cong |

---

## Luong hoat dong

### Buoc 1: Setup ban dau (Admin)

Admin tao:
1. Chuong trinh (Program)
2. Cac goi hoc (TuitionPlan) cho tung chuong trinh
3. Lop hoc (Class) theo chuong trinh
4. Giao vien, phong, lich hoc

### Buoc 2: Hoc vien dang ky

Tao dang ky voi:
- Hoc vien
- Chi nhanh
- Chuong trinh
- Goi hoc
- Ngay bat dau du kien
- Nhu cau lich hoc

**API**: `POST /api/registrations`
- Status mac dinh: **New**

### Buoc 3: Go y lop phu hop

He thong goi y cac lop:
- Cung chuong trinh
- Cung chi nhanh
- Con cho
- Phu hop ca hoc (theo preferredSchedule)

**API**: `GET /api/registrations/{id}/suggest-classes`

### Buoc 4: Xep lop

Admin chon lop phu hop.

**API**: `POST /api/registrations/{id}/assign-class`

- **entryType = "immediate"** -> Tao Enrollment, Status = `Studying`

- **entryType = "wait"** -> Khong tao Enrollment, Status = `WaitingForClass`

### Buoc 5: Van hanh hoc vu

Trong qua trinh hoc co the phat sinh:
- Vao giua chuong trinh
- Hoc bu
- Chuyen lop
- Bao luu
- Nang goi
- Gia han
- Chuyen chi nhanh

---

## Tu dong hoa (Auto Status)

### Auto Full
- Khi enrollment count = capacity -> Lop tu dong chuyen sang **Full**
- Trigger: Sau khi them enrollment moi trong assign-class

### Auto Completed
- Khi `RemainingSessions = 0` -> Registration tu dong chuyen sang **Completed**
- Khi tat ca Registration trong lop completed -> Class tu dong chuyen sang **Completed**
- Trigger: Sau khi diem danh Present trong mark-attendance

### Auto Active
- Khi den ngay khai giang -> Lop tu dong chuyen tu **Planned** sang **Active**
- Trigger: Job `SyncPlannedToActualSessionsJob`

---

## Cac truong hop su dung (Use Cases)

### UC-001: Hoc sinh moi dang ky va co lop phu hop

**Flow:**
```
1. POST /api/registrations
   -> Tao Registration (status: New)

2. GET /api/registrations/{id}/suggest-classes
   -> Xem cac lop goi y

3. POST /api/registrations/{id}/assign-class
   -> Xep lop voi entryType="immediate"
   -> Ket qua: Enrollment duoc tao, Status = Studying
```

### UC-002: Hoc sinh dang ky giua chung khi lop dang hoc

**Flow:**
```
1. POST /api/registrations
   -> Tao Registration (status: New)

2. GET /api/registrations/{id}/suggest-classes
   -> Xem cac lop dang Active

3. POST /api/registrations/{id}/assign-class
   -> entryType="immediate"
   -> Ket qua: Enrollment duoc tao
```

### UC-003: Dang ky nhung chua co lop phu hop

**Flow:**
```
1. POST /api/registrations
   -> Tao Registration (status: New)

2. GET /api/registrations/{id}/suggest-classes
   -> Khong co lop phu hop (hoac lop da Full)

3. POST /api/registrations/{id}/assign-class
   -> entryType="wait"
   -> classId: null (khong can)
   -> Ket qua: KHONG tao Enrollment, Status = WaitingForClass

4. GET /api/registrations/waiting-list
   -> Xem danh sach cho xep lop

5. Khi co lop moi, admin xep lop thu cong
   -> POST /api/registrations/{id}/assign-class
   -> entryType="immediate" voi classId cua lop moi
```

### UC-004: Nang cap goi hoc

**Flow:**
```
POST /api/registrations/{id}/upgrade?newTuitionPlanId=uuid
-> Tao Registration moi voi goi cao hon
-> Registration cu duoc danh dau OperationType = Upgrade
```

### UC-005: Chuyen lop

**Flow:**
```
POST /api/registrations/{id}/transfer-class?newClassId=uuid&effectiveDate=2024-02-01T00:00:00Z
-> Tao Registration moi cho lop moi
-> Registration cu duoc danh dau OperationType = Transfer
```

### UC-006: Hoc sinh xin bao luu

**Flow:**
```
POST /api/enrollments/{id}/pause
-> Enrollment chuyen sang status Paused
-> Registration van giu nguyen (Studying)
```

---

## API Endpoints Chi Tiet

### 1. Tao dang ky hoc

**Endpoint:** `POST /api/registrations`

**Authorization:** Admin, ManagementStaff

**Request:**
```json
{
    "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
    "branchId": "550e8400-e29b-41d4-a716-446655440002",
    "programId": "550e8400-e29b-41d4-a716-446655440003",
    "tuitionPlanId": "550e8400-e29b-41d4-a716-446655440004",
    "expectedStartDate": "2024-01-15T00:00:00Z",
    "preferredSchedule": "T7",
    "note": "Hoc vao thu 7 buoi chieu"
}
```

**Response (201 Created):**
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
    "branchId": "550e8400-e29b-41d4-a716-446655440002",
    "branchName": "Chi nhanh 1",
    "programId": "550e8400-e29b-41d4-a716-446655440003",
    "programName": "Starter",
    "tuitionPlanId": "550e8400-e29b-41d4-a716-446655440004",
    "tuitionPlanName": "Starter 3 thang",
    "registrationDate": "2024-01-10T10:30:00Z",
    "expectedStartDate": "2024-01-15T00:00:00Z",
    "preferredSchedule": "T7",
    "note": "Hoc vao thu 7 buoi chieu",
    "status": "New",
    "classId": null,
    "className": null,
    "createdAt": "2024-01-10T10:30:00Z"
}
```

**Errors:**

| HTTP | Error Code | Mo ta |
|------|------------|-------|
| 400 | ValidationError | Du lieu khong hop le |
| 404 | StudentNotFound | Hoc vien khong ton tai |
| 404 | BranchNotFound | Chi nhanh khong ton tai |
| 404 | ProgramNotFound | Chuong trinh khong ton tai |
| 404 | TuitionPlanNotFound | Goi hoc khong ton tai |
| 409 | AlreadyExists | Hoc vien da co dang ky active cho chuong trinh nay |

---

### 2. Lay danh sach dang ky

**Endpoint:** `GET /api/registrations`

**Authorization:** Admin, ManagementStaff

**Query Parameters:**

| Parameter | Type | Mo ta |
|-----------|------|--------|
| studentProfileId | Guid | Loc theo hoc vien |
| branchId | Guid | Loc theo chi nhanh |
| programId | Guid | Loc theo chuong trinh |
| status | string | Loc theo trang thai (New, Studying, Completed...) |
| classId | Guid | Loc theo lop |
| pageNumber | int | So trang (default: 1) |
| pageSize | int | Kich thuoc trang (default: 10) |

**Response (200 OK):**
```json
{
    "items": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440010",
            "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
            "studentName": "Nguyen Van A",
            "branchId": "550e8400-e29b-41d4-a716-446655440002",
            "branchName": "Chi nhanh 1",
            "programId": "550e8400-e29b-41d4-a716-446655440003",
            "programName": "Starter",
            "classId": "550e8400-e29b-41d4-a716-446655440005",
            "className": "Starter A",
            "status": "Studying",
            "totalSessions": 12,
            "usedSessions": 5,
            "remainingSessions": 7,
            "registrationDate": "2024-01-10T10:30:00Z"
        }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10
}
```

---

### 3. Lay chi tiet dang ky

**Endpoint:** `GET /api/registrations/{id}`

**Authorization:** Admin, ManagementStaff

**Response (200 OK):**
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
    "studentName": "Nguyen Van A",
    "branchId": "550e8400-e29b-41d4-a716-446655440002",
    "branchName": "Chi nhanh 1",
    "programId": "550e8400-e29b-41d4-a716-446655440003",
    "programName": "Starter",
    "tuitionPlanId": "550e8400-e29b-41d4-a716-446655440004",
    "tuitionPlanName": "Starter 3 thang",
    "classId": "550e8400-e29b-41d4-a716-446655440005",
    "className": "Starter A",
    "registrationDate": "2024-01-10T10:30:00Z",
    "expectedStartDate": "2024-01-15T00:00:00Z",
    "actualStartDate": "2024-01-15T08:00:00Z",
    "preferredSchedule": "T7",
    "note": "Hoc vao thu 7 buoi chieu",
    "status": "Studying",
    "entryType": "Immediate",
    "totalSessions": 12,
    "usedSessions": 5,
    "remainingSessions": 7,
    "expiryDate": "2024-04-15T00:00:00Z",
    "createdAt": "2024-01-10T10:30:00Z",
    "updatedAt": "2024-02-01T10:00:00Z"
}
```

**Errors:**

| HTTP | Error Code | Mo ta |
|------|------------|-------|
| 404 | RegistrationNotFound | Dang ky khong ton tai |

---

### 4. Cap nhat dang ky

**Endpoint:** `PUT /api/registrations/{id}`

**Authorization:** Admin, ManagementStaff

**Request:**
```json
{
    "expectedStartDate": "2024-02-01T00:00:00Z",
    "preferredSchedule": "CN",
    "note": "Cap nhat ghi chu"
}
```

**Luu y:** Khong the cap nhat cac truong da xep lop (classId, status...)

**Errors:**

| HTTP | Error Code | Mo ta |
|------|------------|-------|
| 400 | InvalidStatus | Trang thai khong cho phep cap nhat |
| 404 | TuitionPlanNotFound | Goi hoc khong ton tai (khi doi goi) |

---

### 5. Go y lop phu hop

**Endpoint:** `GET /api/registrations/{id}/suggest-classes`

**Authorization:** Admin, ManagementStaff

**Logic loc:**
- Cung chuong trinh (ProgramId)
- Cung chi nhanh (BranchId)
- Con cho (Capacity > CurrentEnrollment)
- Con tuyen sinh (Status: Planned, Recruiting, Active)
- **Phu hop ca hoc** (theo preferredSchedule vs schedulePattern)

**Ho tro preferredSchedule:**
- Sang/chieu/toi: "sang", "chieu", "toi"
- Cuoi tuan: "cuoi tuan", "thu 7", "cn"
- Trong tuan: "trong tuan"
- Thu cu the: "thu 2", "thu 3"...

**Response (200 OK):**
```json
{
    "registrationId": "550e8400-e29b-41d4-a716-446655440010",
    "programName": "Starter",
    "branchName": "Chi nhanh 1",
    "preferredSchedule": "T7",
    "suggestedClasses": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440005",
            "code": "STA001",
            "title": "Starter A",
            "status": "Recruiting",
            "capacity": 15,
            "currentEnrollment": 10,
            "remainingSlots": 5,
            "startDate": "2024-01-20",
            "endDate": "2024-04-20",
            "schedulePattern": "FREQ=WEEKLY;BYDAY=SA;BYHOUR=14;BYMINUTE=30",
            "mainTeacherName": "Nguyen Van A",
            "classroomName": "Phong 101",
            "isClassStarted": false
        }
    ],
    "alternativeClasses": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440006",
            "code": "STA002",
            "title": "Starter B",
            "status": "Planned",
            "capacity": 15,
            "currentEnrollment": 0,
            "remainingSlots": 15,
            "startDate": "2024-02-01",
            "endDate": "2024-05-01",
            "schedulePattern": "FREQ=WEEKLY;BYDAY=SU;BYHOUR=9;BYMINUTE=0",
            "mainTeacherName": "Tran Thi B",
            "classroomName": "Phong 102",
            "isClassStarted": false
        }
    ]
}
```

**Giai thich:**
- `suggestedClasses`: Lop bat dau trong 7 ngay toi
- `alternativeClasses`: Lop bat dau sau 7 ngay
- `isClassStarted: true`: Lop da khai giang

**Errors:**

| HTTP | Error Code | Mo ta |
|------|------------|-------|
| 404 | RegistrationNotFound | Dang ky khong ton tai |

---

### 6. Xep lop cho hoc vien

**Endpoint:** `POST /api/registrations/{id}/assign-class`

**Authorization:** Admin, ManagementStaff

**Request:**
```json
{
    "classId": "550e8400-e29b-41d4-a716-446655440005",
    "entryType": "immediate"
}
```

**Request (entryType = wait - khong can classId):**
```json
{
    "classId": null,
    "entryType": "wait"
}
```

**Response (200 OK) - immediate:**
```json
{
    "registrationId": "550e8400-e29b-41d4-a716-446655440010",
    "registrationStatus": "Studying",
    "classId": "550e8400-e29b-41d4-a716-446655440005",
    "classCode": "STA001",
    "classTitle": "Starter A",
    "entryType": "Immediate",
    "classAssignedDate": "2024-01-15T08:00:00Z",
    "warningMessage": null
}
```

**Response (200 OK) - immediate voi lop da Active:**
```json
{
    "registrationId": "550e8400-e29b-41d4-a716-446655440010",
    "registrationStatus": "Studying",
    "classId": "550e8400-e29b-41d4-a716-446655440005",
    "classCode": "STA001",
    "classTitle": "Starter A",
    "entryType": "Immediate",
    "classAssignedDate": "2024-01-15T08:00:00Z",
    "warningMessage": "Lop da bat dau. Hoc vien se tham gia giua chung."
}
```



**Response (200 OK) - wait:**
```json
{
    "registrationId": "550e8400-e29b-41d4-a716-446655440010",
    "registrationStatus": "WaitingForClass",
    "classId": "00000000-0000-0000-0000-000000000000",
    "classCode": null,
    "classTitle": null,
    "entryType": "Wait",
    "classAssignedDate": "2024-01-15T08:00:00Z",
    "warningMessage": "Hoc vien da duoc them vao danh sach cho lop moi."
}
```

**Auto Status:**
- Khi enrollment count = capacity -> Class tu dong chuyen sang **Full**

**Errors:**

| HTTP | Error Code | Mo ta |
|------|------------|-------|
| 404 | RegistrationNotFound | Dang ky khong ton tai |
| 404 | ClassNotFound | Lop khong ton tai |
| 400 | ClassNotMatchingProgram | Lop khong thuoc chuong trinh cua dang ky |
| 400 | ClassNotAvailable | Lop da Completed/Cancelled/Suspended |
| 400 | ClassFull | Lop da day |
| 409 | AlreadyEnrolled | Hoc vien da co enrollment trong lop nay |
| 400 | InvalidStatus | Dang ky da Completed/Cancelled |

---

### 7. Danh sach cho xep lop

**Endpoint:** `GET /api/registrations/waiting-list`

**Authorization:** Admin, ManagementStaff

**Query Parameters:**

| Parameter | Type | Mo ta |
|-----------|------|--------|
| branchId | Guid | Loc theo chi nhanh |
| programId | Guid | Loc theo chuong trinh |
| pageNumber | int | So trang (default: 1) |
| pageSize | int | Kich thuoc trang (default: 10) |

**Response (200 OK):**
```json
{
    "items": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440010",
            "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
            "studentName": "Nguyen Van A",
            "branchId": "550e8400-e29b-41d4-a716-446655440002",
            "branchName": "Chi nhanh 1",
            "programId": "550e8400-e29b-41d4-a716-446655440003",
            "programName": "Starter",
            "tuitionPlanName": "Starter 3 thang",
            "preferredSchedule": "T7",
            "registrationDate": "2024-01-10T10:30:00Z",
            "expectedStartDate": "2024-01-15T00:00:00Z",
            "note": "Hoc vao thu 7 buoi chieu",
            "daysWaiting": 5
        }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10
}
```

---

### 8. Huy dang ky

**Endpoint:** `PATCH /api/registrations/{id}/cancel`

**Authorization:** Admin, ManagementStaff

**Query Parameters:**

| Parameter | Type | Mo ta |
|-----------|------|--------|
| reason | string | Ly do huy |

**Response (200 OK):**
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "status": "Cancelled",
    "cancelledAt": "2024-01-20T10:00:00Z"
}
```

**Errors:**

| HTTP | Error Code | Mo ta |
|------|------------|-------|
| 400 | InvalidStatus | Dang ky da Completed/Cancelled |
| 400 | InvalidStatus | Dang ky dang Studying, can huy enrollment truoc |

---

### 9. Chuyen lop

**Endpoint:** `POST /api/registrations/{id}/transfer-class`

**Authorization:** Admin, ManagementStaff

**Query Parameters:**

| Parameter | Type | Mo ta |
|-----------|------|--------|
| newClassId | Guid | Lop moi |
| effectiveDate | DateTime | Ngay hieu luc |

**Response (200 OK):**
```json
{
    "registrationId": "550e8400-e29b-41d4-a716-446655440010",
    "newClassId": "550e8400-e29b-41d4-a716-446655440006",
    "newClassName": "Starter B",
    "effectiveDate": "2024-02-01T00:00:00Z",
    "status": "Studying"
}
```

**Errors:**

| HTTP | Error Code | Mo ta |
|------|------------|-------|
| 400 | InvalidStatus | Dang ky khong o trang thai Studying |
| 404 | ClassNotFound | Lop moi khong ton tai |
| 400 | ClassNotAvailable | Lop moi khong Active/Recruiting |
| 400 | ClassFull | Lop moi da day |

---

### 10. Nang cap goi hoc

**Endpoint:** `POST /api/registrations/{id}/upgrade`

**Authorization:** Admin, ManagementStaff

**Query Parameters:**

| Parameter | Type | Mo ta |
|-----------|------|--------|
| newTuitionPlanId | Guid | Goi hoc moi |

**Response (201 Created):**
```json
{
    "newRegistrationId": "550e8400-e29b-41d4-a716-446655440020",
    "originalRegistrationId": "550e8400-e29b-41d4-a716-446655440010",
    "newTuitionPlanId": "550e8400-e29b-41d4-a716-446655440005",
    "newTuitionPlanName": "Starter 6 thang",
    "totalSessions": 24,
    "remainingSessions": 24,
    "status": "Studying"
}
```

**Logic:**
- Giu nguyen Enrollment hien tai
- Tao Registration moi voi goi cao hon
- Danh dau Registration cu voi OperationType = Upgrade

---

## Diem danh va Tu dong hoa

### Diem danh hoc sinh

**Endpoint:** `POST /api/attendances/mark`

**Request:**
```json
{
    "sessionId": "550e8400-e29b-41d4-a716-446655440030",
    "attendances": [
        {
            "studentProfileId": "550e8400-e29b-41d4-a716-446655440001",
            "attendanceStatus": "Present"
        },
        {
            "studentProfileId": "550e8400-e29b-41d4-a716-446655440002",
            "attendanceStatus": "Absent",
            "note": "Xin nghi truoc 24h"
        }
    ]
}
```

**AttendanceStatus:**

| Gia tri | Mo ta | Anh huong UsedSessions |
|---------|--------|------------------------|
| Present | Co mat | +1 |
| Absent | Vang | 0 |
| Excused | Vang co phep | 0 |
| Late | Di muon | +1 |

### Tu dong cap nhat khi diem danh

Khi hoc sinh co mat (Present):
1. `UsedSessions++`
2. `RemainingSessions--`
3. Neu `RemainingSessions == 0` -> Registration.Status = **Completed**
4. Neu tat ca Registration completed -> Class.Status = **Completed**

---

## Business Rules

### BR-001: Dang ky qua he thong
Hoc vien dang ky goi hoc, khong dang ky truc tiep vao lop

### BR-002: Lop thuoc chuong trinh
Lop hoc thuoc chuong trinh, khong thuoc goi 3/6/9/12 thang

### BR-003: Mot chuong trinh co nhieu goi hoc
Hoc sinh chon goi phu hop voi nhu cau

### BR-004: Mot lop co the chua hoc vien thuoc cac goi khac nhau
Lop quan ly chuong trinh va lich hoc, khong phu thuoc goi hoc

### BR-005: Tach ngay dang ky va ngay vao hoc
- RegistrationDate: Ngay dang ky
- ExpectedStartDate: Ngay bat dau du kien
- ActualStartDate: Ngay vao hoc thuc te (sau khi xep lop voi immediate)

### BR-006: Xep lop la buoc sau dang ky
Dang ky co the ton tai khi chua tim duoc lop phu hop (status: New hoac WaitingForClass)

### BR-007: Phan biet cach nhap hoc giua chung
- immediate: Vao hoc ngay, tham gia cac buoi con lai
- wait: Cho lop moi

### BR-008: Nghiep vu khong cau truc lop
Chuyen lop, bao luu, nang goi khong can tao lop moi

---

## So sanh Luong cu va Luong moi

### Luong cu
Khoa hoc -> Lop hoc -> Phong hoc -> Lich hoc

**Uu diem:**
- Don gian
- De hinh dung
- De CRUD nhanh

**Nhuoc diem:**
- Sai trong tam nghiep vu
- Kho xu ly dang ky giua chung
- De nhau khoa/chuong trinh/goi
- Kho nang goi, bao luu, chuyen lop
- De nay sinh so luong khoa/lop

### Luong moi
Chuong trinh -> Goi hoc -> Lop hoc -> Dang ky hoc vien -> Xep lop -> Hoc vu phat sinh

**Uu diem:**
- Sat nghiep vu trung tam
- Xu ly duoc hau het tinh huong thuc te
- Linh hoat cho sale va hoc vu
- De mo rong dai han
- Tranh nay sinh du lieu khong can thiet

**Nhuoc diem:**
- Kho thiet ke hon ban dau
- Can nhieu rule hon
- UI/DB phuc tap hon luc dau

---

## Luu y quan trong

1. **Enrollment (ClassEnrollment)**: Van ton tai nhung duoc tao dong tu Registration khi xep lop
2. **PauseEnrollmentRequest**: Su dung cho nghiep vu bao luu (da co san)
3. **RemainingSessions**: Duoc tu dong cap nhat khi diem danh

---

## Error Codes Reference

| Error Code | Mo ta |
|------------|-------|
| RegistrationNotFound | Dang ky khong ton tai |
| StudentNotFound | Hoc vien khong ton tai |
| BranchNotFound | Chi nhanh khong ton tai |
| ProgramNotFound | Chuong trinh khong ton tai |
| TuitionPlanNotFound | Goi hoc khong ton tai |
| ClassNotFound | Lop khong ton tai |
| ClassNotMatchingProgram | Lop khong thuoc chuong trinh cua dang ky |
| ClassNotAvailable | Lop da ket thuc/huy/tam ngung |
| ClassFull | Lop da day |
| AlreadyExists | Dang ky da ton tai cho hoc vien/chuong trinh |
| AlreadyEnrolled | Hoc vien da co enrollment trong lop nay |
| InvalidStatus | Trang thai khong cho phep thao tac |

---

## Version

- Ngay tao: 18/03/2026
- Phien ban: 2.0
- Cap nhat: Them EntryType, Auto Status, chi tiet API
