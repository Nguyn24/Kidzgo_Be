# Tài liệu chức năng chương trình bù (Make-up)

Ngày cập nhật: 2026-03-25  
Phạm vi: tài liệu này mô tả chức năng make-up theo code hiện tại trong `Kidzgo_Be`, bao gồm nghiệp vụ, quyền theo role, API, status, validation và các trường hợp lỗi.

## 1. Tổng quan nghiệp vụ

### 1.1. Các khái niệm chính

- `LeaveRequest`: đơn xin nghỉ của học sinh cho một ngày học trong một lớp.
- `MakeupCredit`: quyền học bù gắn với `studentProfileId` và `sourceSessionId` (buổi gốc bị nghỉ).
- `MakeupAllocation`: bản ghi phân bổ một `MakeupCredit` vào `targetSessionId` (buổi học bù).
- `Attendance`: điểm danh của học sinh theo từng buổi học.

### 1.2. Luồng nghiệp vụ chính

1. Học sinh/phụ huynh tạo đơn nghỉ.
2. Hệ thống tính `NoticeHours`.
3. Nếu báo trước `>= 24h`, đơn được `Approved` ngay khi tạo.
4. Khi đơn nghỉ được `Approved`, hệ thống tạo `MakeupCredit` cho các session trong phạm vi nghỉ.
5. Hệ thống có thể tự gợi ý hoặc tự xếp buổi bù vào cuối tuần.
6. Phụ huynh/học sinh có thể tự dùng `MakeupCredit` để chọn buổi bù.
7. Giáo viên/Admin điểm danh buổi học; nếu vắng có đơn nghỉ `>= 24h` thì có thể phát sinh credit nếu chưa có.

### 1.3. Business rules đang có trong code

- Một `MakeupCredit` luôn gắn với đúng 1 học sinh và 1 buổi gốc bị nghỉ.
- `CreateLeaveRequest` tạo 1 `LeaveRequest` cho mỗi ngày có session trong khoảng `SessionDate -> EndDate`.
- Giới hạn số ngày nghỉ theo tháng lấy từ `ProgramLeavePolicy.MaxLeavesPerMonth`; mặc định là `2` nếu chưa cấu hình.
- Khi tạo đơn nghỉ:
  - Nếu `noticeHours >= 24` thì auto `Approved`.
  - Nếu `< 24` thì để `Pending`.
- Khi `LeaveRequest` được `Approved`, hệ thống luôn tạo `MakeupCredit` cho session tương ứng, không còn ràng buộc `> 24h` ở bước approve thủ công.
- Khi điểm danh `Absent` và hệ thống resolve được `AbsenceType.WithNotice24H`, hệ thống tạo `MakeupCredit` nếu chưa tồn tại.
- Auto schedule buổi bù trong luồng `CreateLeaveRequest`/`ApproveLeaveRequest` chỉ tìm session:
  - cùng branch,
  - khác lớp gốc,
  - cuối tuần,
  - class active,
  - program có `IsMakeup = true`.
- `UseMakeupCredit` chỉ cho phép Parent hoặc Student thực hiện ở tầng handler.
- `UseMakeupCredit` chỉ cho phép chọn buổi bù:
  - không ở quá khứ,
  - vào thứ 7/chủ nhật,
  - thuộc tuần sau tuần nghỉ,
  - thuộc đúng `ClassId` gửi lên request.
- Nếu credit đang `Used` nhưng `UsedSessionId` vẫn là buổi tương lai, phụ huynh/học sinh được phép đổi sang buổi khác.
- Khi đổi buổi cho credit đang `Used`, buổi mới phải:
  - cùng program với buổi bù hiện tại,
  - còn slot,
  - và allocation cũ sẽ bị chuyển sang `Cancelled`.
- `UseMakeupCredit` chỉ cập nhật `MakeupCredit` + tạo `MakeupAllocation`; không tự tạo/cập nhật `Attendance`.
- `UpdateAttendance` khi sửa sang `Absent` sẽ set `AbsenceType = NoNotice`; không tự tạo makeup credit.

### 1.4. Tiền điều kiện cấu hình

- Muốn auto schedule học bù, hệ thống cần có class/session make-up cuối tuần.
- Program có thể được đánh dấu:
  - `IsMakeup = true`
  - `DefaultMakeupClassId`
- Nếu không có slot phù hợp, credit vẫn được tạo nhưng có thể chưa được phân bổ tự động.

## 2. Role và phạm vi dữ liệu

### 2.1. Các role liên quan

- `Admin`
- `ManagementStaff`
- `AccountantStaff`
- `Teacher`
- `Parent`
- `Student`

Ghi chú:

- `Student` được dùng ở nhiều controller qua `[Authorize(Roles = "...Student")]`.
- Riêng `MakeupController` và một phần `LeaveRequestController` chỉ dùng `[Authorize]`, nên phạm vi truy cập thực tế chủ yếu phụ thuộc vào handler chứ không chỉ role annotation.

### 2.2. Dữ liệu mỗi role có thể xem theo code hiện tại

| Role | Dữ liệu có thể xem | Scope thực tế |
| --- | --- | --- |
| Admin | Toàn bộ leave request, makeup credit, allocation, session attendance, lịch sử attendance | `all` |
| ManagementStaff | Toàn bộ leave request, makeup credit, allocation | `all` |
| AccountantStaff | Toàn bộ makeup credit, allocation, leave request qua các endpoint chỉ cần `[Authorize]` | `all` |
| Teacher | Session attendance; đồng thời có thể gọi nhiều API makeup/leave chỉ cần `[Authorize]` | `all` |
| Parent | Có endpoint riêng theo học sinh đã chọn trong token; nhưng nhiều endpoint generic vẫn nhận `studentProfileId` và hiện chưa khóa ownership đầy đủ | `own(selected student)` ở endpoint `/api/parent/*`; nhiều endpoint generic là `all-if-knows-id` |
| Student | Xem attendance history theo `StudentId` trong token; với endpoint generic của makeup/leave hiện không có khóa role chặt | `own` ở attendance history; nhiều endpoint generic là `all-if-knows-id` |

### 2.3. Hành động được phép theo role

| Hành động | Admin | ManagementStaff | AccountantStaff | Teacher | Parent | Student |
| --- | --- | --- | --- | --- | --- | --- |
| Tạo leave request | Có | Có | Có | Có | Có | Có |
| Xem danh sách leave request | Có | Có | Có | Có | Có | Có |
| Xem chi tiết leave request | Có | Có | Có | Có | Có | Có |
| Approve leave request | Có | Có | Không | Không | Không | Không |
| Reject leave request | Có | Có | Không | Không | Không | Không |
| Bulk approve leave request | Có | Có | Không | Không | Không | Không |
| Điểm danh session | Có | Không | Không | Có | Không | Không |
| Sửa điểm danh | Có | Không | Không | Có | Không | Không |
| Xem attendance theo session | Có | Không | Không | Có | Không | Không |
| Xem attendance history theo token | Có | Không | Không | Có | Có | Có |
| Xem makeup credits | Có | Có | Có | Có | Có | Có |
| Xem tất cả makeup credits | Có | Có | Có | Có | Có | Có |
| Xem chi tiết makeup credit | Có | Có | Có | Có | Có | Có |
| Use makeup credit | Không ở handler | Không ở handler | Không ở handler | Không ở handler | Có | Có |
| Expire makeup credit | Có | Có | Có | Có | Có | Có |
| Xem suggest available sessions | Có | Có | Có | Có | Có | Có |
| Xem allocations | Có | Có | Có | Có | Có | Có |
| Xem danh sách học viên có makeup/leave | Có | Có | Có | Có | Có qua endpoint parent riêng | Không có endpoint riêng |

Ghi chú quan trọng:

- `Use makeup credit` bị chặn ở handler nếu user không phải Parent hoặc Student.
- `Expire makeup credit` hiện không có giới hạn role riêng ở controller/handler.
- Nhiều endpoint read/list của module make-up hiện chưa enforce `own/department` ở tầng handler.

### 2.4. Phạm vi dữ liệu theo mức own / department / all

| Mức | Ý nghĩa trong code hiện tại |
| --- | --- |
| `own` | Dữ liệu bị ràng buộc bởi `StudentId` hoặc `ParentId` trong token |
| `department` | Hiện chưa có enforcement rõ ràng ở module make-up; branch chỉ là query filter tự nguyện |
| `all` | Không có chặn ownership ở handler, hoặc controller chỉ yêu cầu authenticated |

Kết luận:

- `department` hiện chủ yếu là filter theo `branchId`, không phải permission boundary thật sự.
- `all` là phạm vi thực tế của nhiều API read/list trong module make-up hiện tại.

## 3. Status definition

### 3.1. LeaveRequestStatus

| Status | Ý nghĩa |
| --- | --- |
| `Pending` | Đơn nghỉ đã tạo nhưng chưa duyệt |
| `Approved` | Đơn nghỉ đã duyệt; có thể phát sinh makeup credit |
| `Rejected` | Đơn nghỉ bị từ chối |
| `Cancelled` | Đơn nghỉ bị hủy |

### Luồng chuyển trạng thái thực tế

- `CreateLeaveRequest`
  - `Approved` ngay nếu `noticeHours >= 24`
  - `Pending` nếu `< 24`
- `ApproveLeaveRequest`
  - cho phép chuyển sang `Approved` nếu chưa `Approved`
- `RejectLeaveRequest`
  - cho phép chuyển sang `Rejected` nếu chưa `Rejected`
- `CancelLeaveRequest` handler có tồn tại
  - cho phép chuyển sang `Cancelled` nếu chưa `Cancelled` và session chưa qua ngày
  - hiện chưa thấy endpoint HTTP expose trong controller

Ghi chú:

- Code hiện tại không khóa chặt toàn bộ transition; ví dụ handler `Reject` không chặn trường hợp từ `Approved` sang `Rejected`.

### 3.2. MakeupCreditStatus

| Status | Ý nghĩa |
| --- | --- |
| `Available` | Credit còn hiệu lực, chưa dùng |
| `Used` | Credit đã được gán vào một buổi bù (`UsedSessionId`) |
| `Expired` | Credit bị hết hạn thủ công |

### Luồng chuyển trạng thái thực tế

- Khi tạo credit: `Available`
- Khi auto schedule thành công trong approve/create leave flow: có thể chuyển ngay sang `Used`
- Khi parent/student gọi `use`: `Available -> Used`
- Khi gọi `expire`: mọi trạng thái hiện tại đều có thể bị set thành `Expired`

Ghi chú:

- `Expire` handler không kiểm tra credit đang `Used` hay `Available`.
- Khi expire, `UsedSessionId` bị xóa về `null`.

### 3.3. MakeupAllocationStatus

| Status | Ý nghĩa |
| --- | --- |
| `Pending` | Đã tạo allocation nhưng chưa có bước confirm riêng |
| `Confirmed` | Enum có định nghĩa nhưng hiện chưa thấy flow sử dụng |
| `Cancelled` | Enum có định nghĩa nhưng hiện chưa thấy flow sử dụng |

### Luồng chuyển trạng thái thực tế

- Tạo allocation: `Pending`
- Chưa có API/handler public nào chuyển allocation sang `Confirmed` hoặc `Cancelled`

### 3.4. AttendanceStatus

| Status | Ý nghĩa |
| --- | --- |
| `Present` | Có mặt |
| `Absent` | Vắng mặt |
| `Makeup` | Học bù |
| `NotMarked` | Chưa điểm danh |

### 3.5. AbsenceType

| Status | Ý nghĩa |
| --- | --- |
| `WithNotice24H` | Nghỉ có báo trước >= 24h |
| `Under24H` | Nghỉ có báo trước nhưng < 24h |
| `NoNotice` | Nghỉ không báo trước |
| `LongTerm` | Có enum nhưng hiện không thấy được set trong flow attendance hiện tại |

## 4. Permission matrix theo API

| API | Admin | ManagementStaff | AccountantStaff | Teacher | Parent | Student | Scope thực tế |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST /api/leave-requests` | Có | Có | Có | Có | Có | Có | request-driven |
| `GET /api/leave-requests` | Có | Có | Có | Có | Có | Có | `all` hoặc theo `studentId` nếu không truyền query |
| `GET /api/leave-requests/{id}` | Có | Có | Có | Có | Có | Có | `all` |
| `PUT /api/leave-requests/{id}/approve` | Có | Có | Không | Không | Không | Không | `all` |
| `PUT /api/leave-requests/approve-bulk` | Có | Có | Không | Không | Không | Không | `all` |
| `PUT /api/leave-requests/{id}/reject` | Có | Có | Không | Không | Không | Không | `all` |
| `POST /api/attendance/{sessionId}` | Có | Không | Không | Có | Không | Không | session-level |
| `GET /api/attendance/{sessionId}` | Có | Không | Không | Có | Không | Không | session-level |
| `PUT /api/attendance/{sessionId}/students/{studentProfileId}` | Có | Không | Không | Có | Không | Không | session-level |
| `GET /api/attendance/students` | Có | Không | Không | Có | Có | Có | `own` qua token |
| `GET /api/makeup-credits` | Có | Có | Có | Có | Có | Có | `all-if-knows-studentProfileId` |
| `GET /api/makeup-credits/all` | Có | Có | Có | Có | Có | Có | `all`, hoặc `own` nếu không truyền `studentProfileId` và token có `StudentId` |
| `GET /api/makeup-credits/{id}` | Có | Có | Có | Có | Có | Có | `all` |
| `POST /api/makeup-credits/{id}/use` | Không ở handler | Không ở handler | Không ở handler | Không ở handler | Có | Có | `own` |
| `POST /api/makeup-credits/{id}/expire` | Có | Có | Có | Có | Có | Có | `all` |
| `GET /api/makeup-credits/{id}/parent/get-available-sessions` | Có | Có | Có | Có | Có | Có | `all` |
| `GET /api/makeup-credits/allocations` | Có | Có | Có | Có | Có | Có | `all-if-knows-studentProfileId` |
| `GET /api/makeup-credits/students` | Có | Có | Có | Có | Có | Có | `all` |
| `GET /api/parent/students-with-makeup-or-leave` | Không | Không | Không | Không | Có | Không | `own(selected student)` |

## 5. API catalog

### 5.1. Quy ước response chung

#### Success format

```json
{
  "isSuccess": true,
  "data": {}
}
```

#### Error format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "MakeupCredit.NotAvailable",
  "status": 400,
  "detail": "Makeup credit '...' is not available for allocation."
}
```

#### Validation error format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "GreaterThanOrEqualValidator",
      "description": "Session date cannot be in the past",
      "type": 2
    }
  ]
}
```

#### Auth error format

`401 Unauthorized`

```json
{
  "status": 401,
  "type": "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "detail": "Authentication required to access this resource."
}
```

`403 Forbidden`

```json
{
  "status": 403,
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "detail": "You do not have permission to access this resource."
}
```

#### Mapping status code chung

| Loại lỗi | HTTP status |
| --- | --- |
| Validation | `400` |
| NotFound | `404` |
| Conflict | `409` |
| Unauthorized/Forbidden | `401` / `403` |
| Failure khác | `500` |

### 5.2. Leave Request APIs

#### 5.2.1. Tạo đơn nghỉ

- Endpoint: `POST /api/leave-requests`
- Mục đích: tạo đơn nghỉ cho học sinh; nếu báo trước đủ 24h thì auto approve và có thể auto tạo credit + auto schedule buổi bù.
- Roles: mọi user đã authenticate

##### Body

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `guid` | Có | Học sinh xin nghỉ |
| `classId` | `guid` | Có | Lớp đang học |
| `sessionDate` | `date` | Có | Ngày bắt đầu nghỉ |
| `endDate` | `date?` | Không | Ngày kết thúc nghỉ |
| `reason` | `string?` | Không | Lý do nghỉ |

##### Success response

```json
{
  "isSuccess": true,
  "data": {
    "leaveRequests": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "classId": "guid",
        "sessionDate": "2026-03-28",
        "endDate": null,
        "reason": "string",
        "noticeHours": 30,
        "status": "Approved",
        "requestedAt": "2026-03-25T10:00:00Z",
        "approvedAt": "2026-03-25T10:00:00Z"
      }
    ]
  }
}
```

##### Validation rules

- `studentProfileId` bắt buộc
- `classId` bắt buộc
- `sessionDate >= today`
- `endDate >= sessionDate` nếu có
- `endDate >= today` nếu có

##### Business rules

- Học sinh phải có enrollment `Active` trong class.
- Class phải tồn tại.
- Trong khoảng nghỉ phải có session thực tế của class.
- Số ngày nghỉ hợp lệ trong tháng không vượt `ProgramLeavePolicy.MaxLeavesPerMonth`, mặc định `2`.
- Mỗi ngày có session trong range sẽ sinh ra 1 leave request riêng.

##### Business errors thường gặp

| HTTP | Code | Message |
| --- | --- | --- |
| `400` | `LeaveRequest.NotEnrolled` | Student profile '{studentProfileId}' is not enrolled in class '{classId}'. |
| `400` | `LeaveRequest.ExceededMonthlyLeaveLimit` | Student has exceeded the maximum of X leaves per month. |
| `404` | `LeaveRequest.NotFound` | The leave request with Id = '{id}' was not found. |
| `404` | `LeaveRequest.ClassNotFound` | The class with Id = '{classId}' was not found. |
| `404` | `LeaveRequest.SessionNotFound` | No session found for class '{classId}' on date '{sessionDate}'. |

#### 5.2.2. Danh sách đơn nghỉ

- Endpoint: `GET /api/leave-requests`
- Mục đích: lấy danh sách đơn nghỉ, có filter theo học sinh, lớp, status, branch.
- Roles: mọi user đã authenticate

##### Query params

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `guid?` | Không | Nếu không truyền, handler dùng `StudentId` từ token nếu có |
| `classId` | `guid?` | Không | Filter theo lớp |
| `status` | `LeaveRequestStatus?` | Không | `Pending/Approved/Rejected/Cancelled` |
| `branchId` | `guid?` | Không | Filter theo branch |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

##### Success response

`data` là `Page<GetLeaveRequestsResponse>`

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "classId": "guid",
        "sessionDate": "2026-03-28",
        "endDate": null,
        "reason": "string",
        "noticeHours": 10,
        "status": "Pending",
        "requestedAt": "2026-03-25T10:00:00Z",
        "approvedAt": null
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

#### 5.2.3. Chi tiết đơn nghỉ

- Endpoint: `GET /api/leave-requests/{id}`
- Mục đích: lấy chi tiết một đơn nghỉ.
- Roles: mọi user đã authenticate

##### Path params

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Có |

##### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "studentProfileId": "guid",
    "classId": "guid",
    "sessionDate": "2026-03-28",
    "endDate": null,
    "reason": "string",
    "noticeHours": 10,
    "status": "Pending",
    "requestedAt": "2026-03-25T10:00:00Z",
    "approvedBy": "guid",
    "approvedAt": null
  }
}
```

##### Error

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `LeaveRequest.NotFound` | The leave request with Id = '{id}' was not found. |

#### 5.2.4. Approve đơn nghỉ

- Endpoint: `PUT /api/leave-requests/{id}/approve`
- Mục đích: duyệt đơn nghỉ; tạo makeup credit và có thể auto xếp buổi bù.
- Roles: `Admin`, `ManagementStaff`

##### Path params

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Có |

##### Success response

```json
{
  "isSuccess": true,
  "data": null
}
```

##### Business rules

- Đơn phải tồn tại.
- Không được approve lại đơn đã `Approved`.
- Với từng session nằm trong khoảng nghỉ:
  - nếu chưa có credit cùng `studentProfileId + sourceSessionId + CreatedReason.ApprovedLeave24H` thì tạo mới.
- Handler sẽ thử auto assign vào session cuối tuần cùng tuần nghỉ; nếu tìm được slot thì:
  - tạo `MakeupAllocation`
  - set `MakeupCredit.Status = Used`
  - set `UsedSessionId`

##### Error

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `LeaveRequest.NotFound` | The leave request with Id = '{id}' was not found. |
| `404` | `LeaveRequest.SessionNotFound` | No session found for class '{classId}' on date '{sessionDate}'. |
| `409` | `LeaveRequest.AlreadyApproved` | Leave request is already approved. |

#### 5.2.5. Bulk approve đơn nghỉ

- Endpoint: `PUT /api/leave-requests/approve-bulk`
- Mục đích: duyệt nhiều đơn nghỉ cùng lúc.
- Roles: `Admin`, `ManagementStaff`

##### Body

| Field | Type | Required |
| --- | --- | --- |
| `ids` | `guid[]` | Có |

##### Success response

```json
{
  "isSuccess": true,
  "data": {
    "approvedIds": ["guid"],
    "errors": [
      {
        "id": "guid",
        "code": "LeaveRequest.AlreadyApproved",
        "message": "Leave request is already approved."
      }
    ]
  }
}
```

#### 5.2.6. Reject đơn nghỉ

- Endpoint: `PUT /api/leave-requests/{id}/reject`
- Mục đích: từ chối đơn nghỉ.
- Roles: `Admin`, `ManagementStaff`

##### Success response

```json
{
  "isSuccess": true,
  "data": null
}
```

##### Error

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `LeaveRequest.NotFound` | The leave request with Id = '{id}' was not found. |
| `409` | `LeaveRequest.AlreadyRejected` | Leave request is already rejected. |

#### 5.2.7. Cancel đơn nghỉ

- Trạng thái hiện tại: handler có tồn tại nhưng chưa thấy endpoint public trong `LeaveRequestController`.
- Ý nghĩa nghiệp vụ trong code:
  - chỉ cancel được nếu session chưa qua ngày,
  - nếu đơn từng được approve thì hệ thống xóa các `MakeupCredit` và `MakeupAllocation` liên quan.

### 5.3. Attendance APIs liên quan make-up

#### 5.3.1. Điểm danh session

- Endpoint: `POST /api/attendance/{sessionId}`
- Mục đích: điểm danh nhiều học sinh trong một session; có thể làm phát sinh makeup credit khi vắng có báo trước >= 24h.
- Roles: `Admin`, `Teacher`

##### Path params

| Field | Type | Required |
| --- | --- | --- |
| `sessionId` | `guid` | Có |

##### Body

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `attendances` | `array` | Có | Danh sách học sinh cần điểm danh |
| `attendances[].studentProfileId` | `guid` | Có | Học sinh |
| `attendances[].attendanceStatus` | `AttendanceStatus` | Có | `Present/Absent/Makeup/NotMarked` |
| `attendances[].note` | `string?` | Không | Ghi chú |

##### Success response

```json
{
  "isSuccess": true,
  "data": {
    "results": [
      {
        "id": "guid",
        "sessionId": "guid",
        "studentProfileId": "guid",
        "attendanceStatus": "Absent",
        "absenceType": "WithNotice24H",
        "markedAt": "2026-03-25T10:00:00Z",
        "note": "Xin nghỉ"
      }
    ]
  }
}
```

##### Business rules

- Session phải tồn tại.
- Nếu `AttendanceStatus = Absent`, handler resolve `AbsenceType` từ leave request đã `Approved`.
- Nếu resolve ra `WithNotice24H` và chưa có credit cùng session, hệ thống tạo `MakeupCredit`.
- Nếu `AttendanceStatus = Present`, handler cập nhật `UsedSessions/RemainingSessions` của registration.

##### Error

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `Attendance.NotFound` | The attendance record with Id = '{id}' was not found. |

Ghi chú:

- Handler hiện trả `AttendanceErrors.NotFound(sessionId)` khi session không tồn tại, nên `title` vẫn là `Attendance.NotFound`.

#### 5.3.2. Xem attendance của session

- Endpoint: `GET /api/attendance/{sessionId}`
- Mục đích: xem toàn bộ attendance của session, bao gồm summary và cờ `hasMakeupCredit`.
- Roles: `Admin`, `Teacher`

##### Success response

```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "guid",
    "sessionName": "Class ABC",
    "date": "2026-03-28",
    "startTime": "09:00:00",
    "endTime": "10:30:00",
    "summary": {
      "totalStudents": 10,
      "presentCount": 8,
      "absentCount": 1,
      "makeupCount": 1,
      "notMarkedCount": 0
    },
    "attendances": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "studentName": "Nguyen Van A",
        "attendanceStatus": "Absent",
        "absenceType": "WithNotice24H",
        "hasMakeupCredit": true,
        "note": "Xin nghỉ",
        "markedAt": "2026-03-25T10:00:00Z"
      }
    ]
  }
}
```

#### 5.3.3. Sửa attendance

- Endpoint: `PUT /api/attendance/{sessionId}/students/{studentProfileId}`
- Mục đích: sửa điểm danh một học sinh trong session.
- Roles: `Admin`, `Teacher`

##### Body

| Field | Type | Required |
| --- | --- | --- |
| `attendanceStatus` | `AttendanceStatus` | Có |
| `note` | `string?` | Không |

##### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "sessionId": "guid",
    "studentProfileId": "guid",
    "attendanceStatus": "Absent",
    "absenceType": "NoNotice",
    "note": "string",
    "updatedAt": null
  }
}
```

##### Validation rules

- `sessionId` bắt buộc
- `studentProfileId` bắt buộc
- `attendanceStatus` phải là enum hợp lệ

##### Business rules

- Attendance record phải tồn tại trước đó.
- Teacher chỉ sửa được trong vòng 24h sau khi session kết thúc.
- Admin không bị chặn bởi cửa sổ 24h.
- Khi sửa sang `Absent`, `AbsenceType` luôn bị set `NoNotice`.

##### Error

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `Attendance.NotFound` | Attendance not found for session '{sessionId}' and student '{studentProfileId}'. |
| `400` | `Attendance.UpdateWindowClosed` | Attendance for session '{sessionId}' can only be updated within 24 hours after it ends. |

#### 5.3.4. Attendance history của học sinh

- Endpoint: `GET /api/attendance/students`
- Mục đích: xem lịch sử attendance của học sinh từ token hiện tại.
- Roles: `Admin`, `Teacher`, `Parent`, `Student`

##### Query params

| Field | Type | Required |
| --- | --- | --- |
| `pageNumber` | `int` | Không |
| `pageSize` | `int` | Không |

##### Business rules

- Handler bắt buộc phải có `StudentId` trong token.
- Profile phải tồn tại và thuộc `userContext.UserId`.

### 5.4. Makeup Credit APIs

#### 5.4.1. Danh sách credit theo học sinh

- Endpoint: `GET /api/makeup-credits`
- Mục đích: lấy danh sách makeup credit theo `studentProfileId`.
- Roles: mọi user đã authenticate

##### Query params

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `guid` | Có |

##### Success response

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "studentProfileId": "guid",
      "sourceSessionId": "guid",
      "status": "Available",
      "createdReason": "ApprovedLeave24H",
      "expiresAt": null,
      "usedSessionId": null,
      "createdAt": "2026-03-25T10:00:00Z"
    }
  ]
}
```

#### 5.4.2. Danh sách tất cả credit

- Endpoint: `GET /api/makeup-credits/all`
- Mục đích: list credit có filter theo học sinh, status, branch, paging.
- Roles: mọi user đã authenticate

##### Query params

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `guid?` | Không | Nếu null thì handler thử dùng `StudentId` từ token |
| `status` | `string?` | Không | Parse về `MakeupCreditStatus` |
| `branchId` | `guid?` | Không | Filter qua class enrollment của student |
| `pageNumber` | `int` | Không | Mặc định `1` |
| `pageSize` | `int` | Không | Mặc định `10` |

##### Success response

`data` là `Page<MakeupCreditResponse>`

#### 5.4.3. Chi tiết credit

- Endpoint: `GET /api/makeup-credits/{id}`
- Mục đích: lấy chi tiết một makeup credit.
- Roles: mọi user đã authenticate

##### Error

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `MakeupCredit.NotFound` | The makeup credit with Id = '{id}' was not found. |

#### 5.4.4. Dùng credit để chọn buổi bù

- Endpoint: `POST /api/makeup-credits/{id}/use`
- Mục đích: parent/student chọn buổi học bù cho một credit.
- Roles tại controller: mọi user đã authenticate
- Roles thực tế tại handler: chỉ `Parent` hoặc `Student`

##### Path params

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Có |

##### Body

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `studentProfileId` | `guid?` | Parent: Có, Student: Không bắt buộc | Parent phải truyền và phải linked với parent hiện tại |
| `classId` | `guid` | Có | Class chứa `targetSessionId` |
| `targetSessionId` | `guid` | Có | Session được chọn để học bù |

##### Success response

```json
{
  "isSuccess": true,
  "data": null
}
```

##### Validation/business rules

- Parent phải truyền `studentProfileId`.
- `studentProfileId` phải linked với parent đang đăng nhập.
- Student dùng `StudentId` từ token.
- Credit phải tồn tại.
- Credit phải thuộc đúng student đang thao tác.
- Nếu credit đã có `UsedSessionId` và buổi đó là hôm nay hoặc quá khứ thì không cho đổi.
- Credit `Available` được dùng bình thường.
- Credit `Used` vẫn được đổi nếu buổi đã xếp còn ở tương lai.
- Nếu `ExpiresAt <= now` thì bị coi là hết hạn.
- `sourceSession` và `targetSession` phải tồn tại.
- `targetSession` không được ở quá khứ.
- `targetSession` phải rơi vào `Saturday` hoặc `Sunday`.
- `targetSession` phải thuộc tuần sau tuần nghỉ, không được cùng tuần hoặc sớm hơn.
- `targetSession.ClassId` phải bằng `classId` trong request.
- Nếu đang đổi từ một allocation cũ, `targetSession` phải thuộc cùng program với buổi bù hiện tại.
- `targetSession` phải còn slot, tính theo `active enrollments + non-cancelled makeup allocations < class capacity`.

##### Business errors

| HTTP | Code | Message |
| --- | --- | --- |
| `400` | `Profile.UserMustBeParentOrStudent` | User must be a parent or student |
| `400` | `MakeupCredit.ParentMustProvideStudentProfileId` | Parent must provide StudentProfileId. |
| `400` | `MakeupCredit.NotBelongToStudent` | This makeup credit does not belong to the specified student. |
| `400` | `MakeupCredit.NotAvailable` | Makeup credit '{id}' is not available for allocation. |
| `400` | `MakeupCredit.Expired` | Makeup credit '{id}' is expired. |
| `400` | `MakeupCredit.CannotUsePastDate` | Cannot use makeup credit for past dates. |
| `400` | `MakeupCredit.MustBeWeekend` | Makeup session must be on Saturday or Sunday. |
| `400` | `MakeupCredit.MustBeFutureWeek` | Makeup session must be in the weeks after the missed week. |
| `400` | `MakeupCredit.CannotChangeAllocatedPastSession` | Cannot change makeup session because the allocated session is today or has already passed. |
| `400` | `MakeupCredit.StudentNotBelongToParent` | Student does not belong to this parent. |
| `400` | `MakeupCredit.SessionNotBelongToClass` | Target session does not belong to the specified class. |
| `400` | `MakeupCredit.MustStayInCurrentMakeupProgram` | Target session must belong to the same makeup program as the current allocation. |
| `409` | `MakeupCredit.TargetSessionFull` | Target makeup session has no available slot. |
| `404` | `MakeupCredit.NotFound` | The makeup credit with Id = '{id}' was not found. |

#### 5.4.5. Expire credit

- Endpoint: `POST /api/makeup-credits/{id}/expire`
- Mục đích: đánh dấu credit hết hạn thủ công.
- Roles: mọi user đã authenticate

##### Body

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `expiresAt` | `datetime?` | Không | Nếu null thì handler set `DateTime.UtcNow` |

##### Success response

```json
{
  "isSuccess": true,
  "data": null
}
```

##### Validation rules

- `makeupCreditId` bắt buộc
- nếu truyền `expiresAt` thì phải `<= now`

##### Business rules

- Credit phải tồn tại.
- Handler set:
  - `Status = Expired`
  - `ExpiresAt = request.ExpiresAt ?? now`
  - `UsedSessionId = null`

#### 5.4.6. Gợi ý buổi bù khả dụng

- Endpoint: `GET /api/makeup-credits/{id}/parent/get-available-sessions`
- Mục đích: lấy danh sách session gợi ý để dùng credit.
- Roles: mọi user đã authenticate

##### Query params

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `fromDate` | `date?` | Không | Mặc định hôm nay |
| `toDate` | `date?` | Không | Mặc định `fromDate + 7 ngày` |
| `timeOfDay` | `string?` | Không | `morning`, `afternoon`, `evening` |

##### Rule lọc chính

- bỏ session nguồn,
- chỉ `SessionStatus = Scheduled`,
- chỉ session tương lai,
- chỉ thứ 7/chủ nhật,
- cùng branch với session nguồn,
- khác class với session nguồn,
- lọc theo khung giờ nếu có,
- loại các session trùng giờ hoặc cách session học hiện tại của học sinh < 2 giờ.

##### Success response

```json
{
  "isSuccess": true,
  "data": [
    {
      "sessionId": "guid",
      "classId": "guid",
      "classCode": "MK-01",
      "classTitle": "Makeup Weekend",
      "programName": "Program A",
      "programCode": "PA",
      "branchId": "guid",
      "plannedDatetime": "2026-03-29T09:00:00Z",
      "plannedEndDatetime": "2026-03-29T10:30:00Z"
    }
  ]
}
```

#### 5.4.7. Danh sách allocations theo học sinh

- Endpoint: `GET /api/makeup-credits/allocations`
- Mục đích: lấy lịch sử phân bổ buổi bù theo học sinh.
- Roles: mọi user đã authenticate

##### Query params

| Field | Type | Required |
| --- | --- | --- |
| `studentProfileId` | `guid` | Có |

##### Success response

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "makeupCreditId": "guid",
      "targetSessionId": "guid",
      "assignedBy": "guid",
      "assignedAt": "2026-03-25T10:00:00Z"
    }
  ]
}
```

Ghi chú:

- Response hiện không trả `status` của allocation dù entity có field này.

#### 5.4.8. Danh sách học viên có makeup credit hoặc leave request

- Endpoint: `GET /api/makeup-credits/students`
- Mục đích: lấy danh sách học viên có leave request `Pending/Approved` hoặc makeup credit `Available`.
- Roles: mọi user đã authenticate

##### Query params

| Field | Type | Required |
| --- | --- | --- |
| `searchTerm` | `string?` | Không |
| `branchId` | `guid?` | Không |
| `pageNumber` | `int` | Không |
| `pageSize` | `int` | Không |

##### Success response

`data` là `Page<StudentWithMakeupOrLeaveResponse>`

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userId": "guid",
        "displayName": "Nguyen Van A",
        "userEmail": "a@example.com",
        "hasLeaveRequest": false,
        "hasMakeupCredit": true,
        "leaveRequestCount": 0,
        "makeupCreditCount": 2
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1
  }
}
```

### 5.5. Parent helper API

#### 5.5.1. Danh sách học sinh hiện tại của parent có makeup/leave

- Endpoint: `GET /api/parent/students-with-makeup-or-leave`
- Mục đích: lấy dữ liệu make-up/leave cho học sinh đã chọn trong token của phụ huynh.
- Roles: Parent

##### Query params

| Field | Type | Required |
| --- | --- | --- |
| `searchTerm` | `string?` | Không |
| `pageNumber` | `int` | Không |
| `pageSize` | `int` | Không |

##### Business rules

- Phải tìm được parent profile từ `userContext.UserId`.
- Token phải có `StudentId`.
- `StudentId` phải linked với parent.
- Chỉ trả về học sinh đã chọn trong token, không trả toàn bộ danh sách con.

##### Error

| HTTP | Code | Message |
| --- | --- | --- |
| `404` | `ParentProfile` | Parent profile not found for current user |
| `404` | `StudentId` | No student selected in token |
| `404` | `Student` | Student not linked to this parent |

## 6. Validation rule tổng hợp

### 6.1. Leave request

- `SessionDate` không được ở quá khứ.
- `EndDate` không được nhỏ hơn `SessionDate`.
- Học sinh phải enrolled active trong class.
- Trong range phải có session thực.
- Không vượt số ngày nghỉ tối đa theo tháng.

### 6.2. Makeup credit use

- Parent phải truyền `studentProfileId`.
- Student phải có `StudentId` trong token.
- Credit phải thuộc student đang thao tác.
- Credit phải `Available`.
- Credit chưa hết hạn.
- Target session phải ở tương lai.
- Target session phải là cuối tuần.
- Target session phải ở tuần sau tuần nghỉ.
- Target session phải thuộc class gửi lên.

### 6.3. Expire credit

- `ExpiresAt` nếu truyền phải ở quá khứ hoặc hiện tại.

### 6.4. Attendance update

- Teacher chỉ sửa được trong 24 giờ sau khi session kết thúc.
- Admin sửa không bị chặn bởi cửa sổ thời gian.

## 7. Danh sách lỗi nghiệp vụ chính

### 7.1. Leave request

| Code | Ý nghĩa |
| --- | --- |
| `LeaveRequest.NotFound` | Không tìm thấy đơn nghỉ |
| `LeaveRequest.AlreadyApproved` | Đơn đã được duyệt |
| `LeaveRequest.AlreadyRejected` | Đơn đã bị từ chối |
| `LeaveRequest.NotEnrolled` | Học sinh không active trong lớp |
| `LeaveRequest.SessionNotFound` | Không có session phù hợp với ngày nghỉ |
| `LeaveRequest.ClassNotFound` | Không tìm thấy class |
| `LeaveRequest.ExceededMonthlyLeaveLimit` | Vượt giới hạn ngày nghỉ trong tháng |
| `LeaveRequest.CannotCancelPastSession` | Không thể hủy đơn của buổi đã qua |
| `LeaveRequest.AlreadyCancelled` | Đơn đã bị hủy |

### 7.2. Makeup credit

| Code | Ý nghĩa |
| --- | --- |
| `MakeupCredit.NotFound` | Không tìm thấy credit |
| `MakeupCredit.NotAvailable` | Credit không ở trạng thái khả dụng |
| `MakeupCredit.Expired` | Credit đã hết hạn |
| `MakeupCredit.NotBelongToStudent` | Credit không thuộc học sinh đang thao tác |
| `MakeupCredit.MustBeWeekend` | Buổi bù phải là cuối tuần |
| `MakeupCredit.MustBeFutureWeek` | Buổi bù phải thuộc tuần sau tuần nghỉ |
| `MakeupCredit.CannotUsePastDate` | Không được chọn ngày quá khứ |
| `MakeupCredit.CannotChangeAllocatedPastSession` | Không được đổi nếu buổi bù cũ là hôm nay hoặc đã qua |
| `MakeupCredit.ParentMustProvideStudentProfileId` | Parent chưa truyền studentProfileId |
| `MakeupCredit.StudentNotBelongToParent` | Học sinh không linked với parent |
| `MakeupCredit.SessionNotBelongToClass` | Session không thuộc class request |

### 7.3. Attendance

| Code | Ý nghĩa |
| --- | --- |
| `Attendance.NotFound` | Không tìm thấy attendance record |
| `Attendance.UpdateWindowClosed` | Quá thời gian cho phép sửa điểm danh |

### 7.4. Profile / auth liên quan

| Code | Ý nghĩa |
| --- | --- |
| `Profile.StudentNotFound` | Không tìm thấy student profile từ token/context |
| `Profile.UserMustBeParentOrStudent` | Chỉ parent/student được dùng flow này |

## 8. Các lưu ý quan trọng theo code hiện tại

1. `POST /api/makeup-credits/{id}/use` yêu cầu buổi bù phải ở tuần sau tuần nghỉ, nhưng flow auto schedule trong `CreateLeaveRequest` và `ApproveLeaveRequest` lại đang tìm session cuối tuần của cùng tuần nghỉ.
2. `MakeupController` hiện chỉ dùng `[Authorize]`, nên nhiều endpoint read/list/expire không giới hạn role cụ thể.
3. `LeaveRequestController` cũng có nhiều endpoint chỉ cần authenticated, chưa enforce ownership tại handler cho Parent/Student.
4. `AccountantStaff` hiện có thể truy cập nhiều API make-up read/write hơn mong đợi nghiệp vụ vì controller chưa chặn role.
5. `Expire makeup credit` có thể expire cả credit đã `Used`, đồng thời xóa `UsedSessionId`.
6. `MakeupAllocationStatus.Confirmed` và `Cancelled` mới chỉ tồn tại ở enum; chưa có API/handler public để sử dụng.
7. Manual create makeup credit có command + validator nhưng endpoint HTTP đang bị comment out trong `MakeupController`.
8. `UseMakeupCredit` không tự động set attendance `Makeup`; attendance là flow riêng.

## 9. Đề xuất cách đọc tài liệu

- Nếu cần hiểu lifecycle: đọc mục `1`, `3`, `8`.
- Nếu cần phân quyền theo role: đọc mục `2`, `4`.
- Nếu cần tích hợp API: đọc mục `5`, `6`, `7`.
