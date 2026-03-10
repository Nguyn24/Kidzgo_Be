# Makeup Credit Flow 
---

## 1. Giới hạn nghỉ tối đa 2 buổi/tháng

### API: `POST /api/leave-requests`

**Mô tả**: Khi tạo leave request, hệ thống sẽ kiểm tra tổng số buổi nghỉ trong tháng (bao gồm cả pending và đã approve). Nếu vượt quá 2 buổi, trả về lỗi.

**Request**:
```json
{
  "studentProfileId": "guid",
  "classId": "guid",
  "sessionDate": "2025-01-15",
  "endDate": "2025-01-15",
  "reason": "string"
}
```

**Response (thành công)**:
```json
{
  "id": "guid",
  "studentProfileId": "guid",
  "classId": "guid",
  "sessionDate": "2025-01-15",
  "endDate": "2025-01-15",
  "reason": "string",
  "noticeHours": 48,
  "status": "Approved",
  "requestedAt": "2025-01-14T10:00:00Z",
  "approvedAt": "2025-01-14T10:00:00Z"
}
```

**Response (lỗi - vượt quá 2 buổi/tháng)**:
```json
{
  "type": "Validation",
  "code": "LeaveRequest.ExceededMonthlyLeaveLimit",
  "description": "Student has exceeded the maximum of 2 leaves per month."
}
```

---

## 2. Tự động xếp lịch bù vào T7/CN

### API: `POST /api/leave-requests` (Auto Approve)

**Mô tả**: Khi leave request được auto approve (notice >= 24h) hoặc được staff approve, hệ thống sẽ tự động tạo makeup credit và xếp lịch bù vào T7/CN của tuần student nghỉ.

**Logic**:
1. Tìm thứ 7 và chủ nhật của tuần student nghỉ
2. Tìm các session T7/CN cùng tuần, cùng level, cùng branch, khác class
3. Random chọn 1 session còn slot (enrolled count < capacity)
4. Tạo `MakeupCredit` và `MakeupAllocation`

### API: `PUT /api/leave-requests/{id}/approve`

**Mô tả**: Staff/Admin duyệt leave request. Sau khi approve, hệ thống sẽ tự động xếp lịch bù.

**Request**: Không cần body

**Response**: `200 OK` nếu thành công

---

## 3. API gợi ý buổi bù

### API: `GET /api/makeup-credits/{id}/suggestions`

**Mô tả**: Gợi ý các buổi học bù phù hợp cho makeup credit.

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| makeupDate | DateOnly | Yes | Ngày mong muốn học bù |
| timeOfDay | string | No | Buổi trong ngày: "morning", "afternoon", "evening" |

hoặc có thể để null hết

**Response**:
```json
[
  {
    "sessionId": "guid",
    "classId": "guid",
    "classCode": "string",
    "classTitle": "string",
    "programName": "string",
    "programLevel": 1,
    "plannedDatetime": "2025-01-18T09:00:00Z",
    "plannedEndDatetime": "2025-01-18T10:30:00Z",
    "branchId": "guid"
  }
]
```

---

## 4. API sử dụng makeup credit

### API: `POST /api/makeup-credits/{id}/use`

**Mô tả**: Student hoặc Parent sử dụng makeup credit để đăng ký vào buổi bù. API này cũng dùng để đổi buổi bù đã được tự động xếp lịch sang ngày/buổi khác.

**Validation**:
- Target session phải là T7 hoặc CN
- Target session phải thuộc các tuần SAU tuần nghỉ (không phải cùng tuần)
- Target session không được là ngày trong quá khứ
- **Nếu makeup credit đã được xếp lịch (đã có MakeupAllocation)**: Kiểm tra nếu ngày học bù đã qua thì không thể đổi được

**Request** (cho Student):
```json
{
  "studentProfileId": null,
  "classId": "guid",
  "targetSessionId": "guid"
}
```

**Request** (cho Parent - bắt buộc truyền studentProfileId):
```json
{
  "studentProfileId": "guid-cua-con",
  "classId": "guid",
  "targetSessionId": "guid"
}
```

**Response**: `200 OK` nếu thành công

**Các lỗi có thể xảy ra**:

| Error Code | Description |
|------------|-------------|
| `MakeupCredit.NotFound` | Makeup credit không tồn tại |
| `MakeupCredit.NotAvailable` | Makeup credit không còn khả dụng |
| `MakeupCredit.Expired` | Makeup credit đã hết hạn |
| `MakeupCredit.NotBelongToStudent` | Makeup credit không thuộc về học sinh được chỉ định |
| `MakeupCredit.ParentMustProvideStudentProfileId` | Parent phải truyền studentProfileId |
| `MakeupCredit.StudentNotBelongToParent` | Học sinh không thuộc về parent này |
| `MakeupCredit.MustBeWeekend` | Buổi bù phải là thứ 7 hoặc chủ nhật |
| `MakeupCredit.MustBeFutureWeek` | Buổi bù phải thuộc các tuần sau tuần nghỉ |
| `MakeupCredit.CannotUsePastDate` | Không thể sử dụng cho ngày trong quá khứ |
| `MakeupCredit.CannotChangeAllocatedPastSession` | Không thể đổi buổi bù vì buổi bù đã được xếp lịch đã qua |
| `MakeupCredit.SessionNotBelongToClass` | Session không thuộc về class được chỉ định |



## 7. Ví dụ sử dụng

### Student đăng ký nghỉ và tự động được xếp lịch bù

1. **Tạo leave request**:
```bash
POST /api/leave-requests
{
  "studentProfileId": "student-uuid",
  "classId": "class-uuid", 
  "sessionDate": "2025-01-15",
  "reason": "Bệnh"
}
```

2. **Hệ thống tự động**:
   - Tạo MakeupCredit cho buổi nghỉ
   - Xếp lịch bù vào T7/CN của tuần đó (nếu có session còn slot)

### Student tự đổi lịch bù

1. **Xem các buổi bù gợi ý**:


2. **Đăng ký vào buổi bù mới**:
```bash
POST /api/makeup-credits/{makeupCreditId}/use
{
  "studentProfileId": null,
  "classId": "class-uuid",
  "targetSessionId": "session-uuid"
}
```

### Parent đăng ký thay con

1. **Parent xem danh sách makeup credit của con**:
```bash
GET /api/makeup-credits?studentProfileId={studentId}
```

2. **Parent đăng ký buổi bù cho con**:
```bash
POST /api/makeup-credits/{makeupCreditId}/use
{
  "studentProfileId": "student-uuid-cua-con",
  "classId": "class-uuid",
  "targetSessionId": "session-uuid"
}
```l
