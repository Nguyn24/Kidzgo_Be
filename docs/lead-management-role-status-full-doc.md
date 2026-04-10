# Lead Management Role / Permission / Status Full Doc

Last updated: `2026-04-10`

## 1. Scope

Tài liệu này mô tả trạng thái hiện tại của module Lead Management trong backend:

- Role nào được xem / thao tác dữ liệu lead
- Phạm vi dữ liệu thực tế đang áp dụng trong code
- Danh sách Lead APIs
- Contract request / response chính
- Status definition và luồng chuyển trạng thái
- Validation rule và các trường hợp trả lỗi
- Các implementation note quan trọng để FE / QA / BE bám sát khi thay đổi

Tài liệu này bám theo implementation hiện tại trong:

- `Kidzgo.API/Controllers/LeadController.cs`
- `Kidzgo.Application/Leads/*`
- `Kidzgo.Domain/CRM/*`

## 2. Role Data Scope

### 2.1. Role được xem dữ liệu gì

| Role | Dữ liệu được xem | Scope hiện tại | Ghi chú |
| --- | --- | --- | --- |
| `Anonymous` | Chỉ được tạo lead public | `none` | Chỉ dùng `POST /api/leads/public` |
| `Admin` | Toàn bộ lead, lead child, lead activity, SLA | `all` | Không có filter theo own/department ở backend |
| `ManagementStaff` | Toàn bộ lead, lead child, lead activity, SLA | `all` | Có thêm `self-assign` |
| `AccountantStaff` | Toàn bộ lead, lead child, lead activity, SLA | `all` | Không có `assign` và `self-assign` |
| `Teacher` | Không có quyền vào Lead APIs | `none` | Nhận `403` nếu gọi endpoint protected |
| `Student` | Không có quyền vào Lead APIs | `none` | Nhận `403` nếu gọi endpoint protected |
| `Parent` | Không có quyền vào Lead APIs | `none` | Nhận `403` nếu gọi endpoint protected |

### 2.2. Phạm vi dữ liệu thực tế

Backend hiện chưa implement row-level permission kiểu:

- `own`
- `department`
- `branch-only`

Vì vậy với các role được phép (`Admin`, `ManagementStaff`, `AccountantStaff`), phạm vi dữ liệu thực tế hiện là:

- `all`

`ownerStaffId` hiện chỉ là field dữ liệu và có thể dùng làm filter query; nó chưa phải permission boundary.

## 3. Allowed Actions By Role

| Role | View | Create | Edit | Assign | Self-assign | Update Status | Add Activity / Note | View Activity | Child CRUD | View SLA | Delete |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Anonymous` | No | Public create only | No | No | No | No | No | No | No | No | No |
| `Admin` | Yes | Yes | Yes | Yes | No | Yes | Yes | Yes | Yes | Yes | Child delete only |
| `ManagementStaff` | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Child delete only |
| `AccountantStaff` | Yes | Yes | Yes | No | No | Yes | Yes | Yes | Yes | Yes | Child delete only |
| `Teacher` | No | No | No | No | No | No | No | No | No | No | No |
| `Student` | No | No | No | No | No | No | No | No | No | No | No |
| `Parent` | No | No | No | No | No | No | No | No | No | No | No |

## 4. API Catalog

| Method | Endpoint | Roles | Scope | Main Action | Purpose |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/api/leads/public` | `Anonymous` | `none` | `create` | Tạo lead từ web form public |
| `POST` | `/api/leads` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `create` | Tạo lead nội bộ |
| `GET` | `/api/leads` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `view` | Xem danh sách lead |
| `GET` | `/api/leads/{id}` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `view` | Xem chi tiết lead |
| `PUT` | `/api/leads/{id}` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `edit` | Cập nhật thông tin lead |
| `POST` | `/api/leads/{id}/assign` | `Admin`, `ManagementStaff` | `all` | `assign` | Gán lead cho staff |
| `POST` | `/api/leads/{id}/self-assign` | `ManagementStaff` | `all` | `assign` | Management staff tự nhận lead |
| `PATCH` | `/api/leads/{id}/status` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `edit` | Cập nhật trạng thái lead |
| `POST` | `/api/leads/{id}/notes` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `create` | Ghi nhận activity / note / contact lead |
| `GET` | `/api/leads/{id}/activities` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `view` | Xem timeline activity của lead |
| `GET` | `/api/leads/{id}/sla` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `view` | Xem SLA first response |
| `GET` | `/api/leads/statuses` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `view` | Lấy danh sách `LeadStatus` |
| `GET` | `/api/leads/{leadId}/children` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `view` | Xem children của lead |
| `POST` | `/api/leads/{leadId}/children` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `create` | Thêm child cho lead |
| `PUT` | `/api/leads/{leadId}/children/{childId}` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `edit` | Cập nhật child của lead |
| `DELETE` | `/api/leads/{leadId}/children/{childId}` | `Admin`, `ManagementStaff`, `AccountantStaff` | `all` | `delete` | Xóa child khỏi lead |

## 5. Success / Error Envelope

### 5.1. Response success

Lead APIs đang dùng wrapper:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Ví dụ response object thực tế sẽ thay đổi theo endpoint.

### 5.2. Response error

Lead APIs đang trả lỗi theo `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Lead.NotFound",
  "status": 404,
  "detail": "The lead with the Id = 'guid' was not found"
}
```

Validation error thường có dạng:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Lead.InvalidStatusTransition",
  "status": 400,
  "detail": "Invalid status transition. Cannot change from current status to target status"
}
```

Auth / permission error thường là:

- `401 Unauthorized`: chưa đăng nhập
- `403 Forbidden`: có token nhưng sai role

## 6. API Details

## 6.1. POST `/api/leads/public`

### Mô tả

Tạo lead từ landing page / web form public.

### Roles / Scope / Action

- Role: `Anonymous`
- Scope: `none`
- Action: `create`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `contactName` | `string` | No | Backend chấp nhận rỗng nếu có field contact khác |
| `phone` | `string?` | No | Dùng để chống duplicate |
| `zaloId` | `string?` | No | Dùng để chống duplicate |
| `email` | `string?` | No | Dùng để chống duplicate |
| `branchPreference` | `Guid?` | No | Phải tồn tại nếu truyền |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "source": "Landing",
    "campaign": null,
    "contactName": "Nguyen Van A",
    "phone": "0912345678",
    "zaloId": "zalo_123",
    "email": "parent@example.com",
    "company": null,
    "subject": null,
    "branchPreference": "guid-or-null",
    "notes": null,
    "status": "New",
    "ownerStaffId": null,
    "createdAt": "2026-04-10T09:00:00Z"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | `Lead.InvalidContactInfo` | Không có đủ thông tin contact tối thiểu |
| `404` | `Lead.BranchNotFound` | `branchPreference` không tồn tại |
| `409` | `Lead.DuplicateLead` | Trùng phone / email / zaloId |

## 6.2. POST `/api/leads`

### Mô tả

Tạo lead nội bộ từ các nguồn `Zalo`, `Referral`, `Offline`, `Landing`.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `create`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `source` | `LeadSource` | Yes | `Landing`, `Zalo`, `Referral`, `Offline` |
| `campaign` | `string?` | No | |
| `contactName` | `string` | No | Code cho phép rỗng nếu còn phone/email/zaloId |
| `phone` | `string?` | No | |
| `zaloId` | `string?` | No | |
| `email` | `string?` | No | |
| `company` | `string?` | No | |
| `subject` | `string?` | No | |
| `branchPreference` | `Guid?` | No | Phải tồn tại nếu truyền |
| `notes` | `string?` | No | |
| `ownerStaffId` | `Guid?` | No | Nếu truyền phải là `ManagementStaff` hoặc `AccountantStaff` |
| `children` | `ChildDto[]?` | No | Danh sách child tạo cùng lead |

### `children[]`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `childName` | `string` | Yes | |
| `dob` | `DateOnly?` | No | |
| `gender` | `Gender` | Yes | `Male`, `Female` |
| `programInterest` | `string?` | No | |
| `notes` | `string?` | No | |

### Success Response

Success format giống `POST /api/leads/public`, nhưng:

- `source` lấy từ request
- `ownerStaffId` có thể có giá trị
- `children` không nằm trong response create lead hiện tại

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | `Lead.InvalidContactInfo` | Không có contact tối thiểu |
| `400` | `Lead.OwnerNotStaff` | `ownerStaffId` không phải `ManagementStaff` / `AccountantStaff` |
| `404` | `Lead.OwnerNotFound` | `ownerStaffId` không tồn tại |
| `404` | `Lead.BranchNotFound` | Branch không tồn tại |
| `409` | `Lead.DuplicateLead` | Trùng phone / email / zaloId |

## 6.3. GET `/api/leads`

### Mô tả

Lấy danh sách lead có filter và phân trang.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `view`

### Query Params

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `status` | `string?` | No | `New`, `Contacted`, `BookedTest`, `TestDone`, `Enrolled`, `Lost` |
| `source` | `string?` | No | `Landing`, `Zalo`, `Referral`, `Offline` |
| `ownerStaffId` | `Guid?` | No | Filter theo owner |
| `branchPreference` | `Guid?` | No | Filter theo branch |
| `searchTerm` | `string?` | No | Tìm theo contactName / phone / email |
| `page` | `int` | No | Default `1` |
| `pageSize` | `int` | No | Default `20` |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "leads": [
      {
        "id": "guid",
        "source": "Landing",
        "campaign": null,
        "contactName": "Nguyen Van A",
        "phone": "0912345678",
        "zaloId": "zalo_123",
        "email": "parent@example.com",
        "company": null,
        "subject": null,
        "branchPreference": "guid-or-null",
        "branchPreferenceName": "Branch 1",
        "status": "Contacted",
        "ownerStaffId": "guid-or-null",
        "ownerStaffName": "Staff A",
        "firstResponseAt": "2026-04-10T10:00:00Z",
        "touchCount": 3,
        "nextActionAt": "2026-04-11T08:00:00Z",
        "createdAt": "2026-04-10T09:00:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

### Error Response

Hiện handler không có custom validation riêng cho query params ngoài parse enum ở controller.

## 6.4. GET `/api/leads/{id}`

### Mô tả

Lấy chi tiết lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `view`

### Path Params

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "source": "Landing",
    "campaign": null,
    "contactName": "Nguyen Van A",
    "phone": "0912345678",
    "zaloId": "zalo_123",
    "email": "parent@example.com",
    "company": null,
    "subject": null,
    "branchPreference": "guid-or-null",
    "branchPreferenceName": "Branch 1",
    "notes": "Interested in trial",
    "status": "Contacted",
    "ownerStaffId": "guid-or-null",
    "ownerStaffName": "Staff A",
    "firstResponseAt": "2026-04-10T10:00:00Z",
    "touchCount": 3,
    "nextActionAt": "2026-04-11T08:00:00Z",
    "createdAt": "2026-04-10T09:00:00Z",
    "updatedAt": "2026-04-10T10:00:00Z"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 6.5. PUT `/api/leads/{id}`

### Mô tả

Cập nhật thông tin tổng của lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `edit`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `contactName` | `string?` | No | Nếu có giá trị khác rỗng thì update |
| `phone` | `string?` | No | Có thể clear bằng empty string |
| `zaloId` | `string?` | No | Có thể clear bằng empty string |
| `email` | `string?` | No | Có thể clear bằng empty string |
| `company` | `string?` | No | Có thể clear bằng empty string |
| `subject` | `string?` | No | Có thể clear bằng empty string |
| `branchPreference` | `Guid?` | No | Nếu truyền phải tồn tại |
| `notes` | `string?` | No | Có thể clear bằng empty string |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "contactName": "Nguyen Van A",
    "phone": "0912345678",
    "zaloId": null,
    "email": "parent@example.com",
    "company": null,
    "subject": null,
    "branchPreference": "guid-or-null",
    "notes": "Updated note",
    "updatedAt": "2026-04-10T10:30:00Z"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | `Lead.CannotUpdateConvertedLead` | Lead đã `Enrolled`, không cho update |
| `404` | `Lead.NotFound` | Lead không tồn tại |
| `404` | `Lead.BranchNotFound` | Branch không tồn tại |

## 6.6. POST `/api/leads/{id}/assign`

### Mô tả

Gán owner cho lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`
- Scope: `all`
- Action: `assign`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `ownerStaffId` | `Guid` | Yes | Phải là `ManagementStaff` hoặc `AccountantStaff` |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "leadId": "guid",
    "ownerStaffId": "guid",
    "ownerStaffName": "Staff A"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | `Lead.OwnerNotStaff` | Owner role không hợp lệ |
| `404` | `Lead.NotFound` | Lead không tồn tại |
| `404` | `Lead.OwnerNotFound` | User không tồn tại |

## 6.7. POST `/api/leads/{id}/self-assign`

### Mô tả

Management staff tự nhận lead về mình.

### Roles / Scope / Action

- Role: `ManagementStaff`
- Scope: `all`
- Action: `assign`

### Path Params

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "leadId": "guid",
    "ownerStaffId": "guid",
    "ownerStaffName": "Staff A"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | `Lead.OwnerNotStaff` | User hiện tại không phải `ManagementStaff` |
| `404` | `Lead.NotFound` | Lead không tồn tại |
| `404` | `Lead.OwnerNotFound` | User hiện tại không tồn tại trong DB |

## 6.8. PATCH `/api/leads/{id}/status`

### Mô tả

Cập nhật trạng thái lead thủ công.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `edit`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `status` | `LeadStatus` | Yes | `New`, `Contacted`, `BookedTest`, `TestDone`, `Enrolled`, `Lost` |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "leadId": "guid",
    "status": "Contacted",
    "firstResponseAt": "2026-04-10T10:00:00Z"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | `Lead.InvalidStatusTransition` | Hiện tại rule cứng là không cho rời khỏi `Enrolled` |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 6.9. POST `/api/leads/{id}/notes`

### Mô tả

Ghi nhận `LeadActivity` cho lead. Endpoint này dùng cho:

- `Call`
- `Zalo`
- `Sms`
- `Email`
- `Note`

Ngoài việc lưu activity, endpoint này còn:

- tăng `touchCount`
- update `nextActionAt` nếu có truyền
- set `firstResponseAt` ở lần contact đầu tiên
- tự động chuyển `New -> Contacted` nếu `activityType` là `Call`, `Zalo`, `Sms`, `Email`

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `create`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `content` | `string` | Yes | Nội dung activity |
| `activityType` | `ActivityType` | No | Default `Note`; valid: `Call`, `Zalo`, `Sms`, `Email`, `Note` |
| `nextActionAt` | `DateTime?` | No | Không được ở quá khứ |
| `clearNextAction` | `bool?` | No | Nếu `true` thì xóa `Lead.NextActionAt`; không được đi cùng `nextActionAt` |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "activityId": "guid",
    "leadId": "guid",
    "activityType": "Call",
    "content": "Da goi, phu huynh hen goi lai",
    "leadStatus": "Contacted",
    "firstResponseAt": "2026-04-10T10:00:00Z",
    "nextActionAt": "2026-04-11T08:00:00Z",
    "leadNextActionAt": "2026-04-11T08:00:00Z",
    "createdAt": "2026-04-10T10:00:00Z"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | Validation | `content` rỗng, `nextActionAt` ở quá khứ, hoặc truyền đồng thời `nextActionAt` và `clearNextAction = true` |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 6.10. GET `/api/leads/{id}/activities`

### Mô tả

Lấy timeline activity của một lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `view`

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "leadId": "guid",
    "activities": [
      {
        "id": "guid",
        "activityType": "Call",
        "content": "Da goi, phu huynh hen goi lai",
        "nextActionAt": "2026-04-11T08:00:00Z",
        "createdBy": "guid-or-null",
        "createdByName": "Staff A",
        "createdAt": "2026-04-10T10:00:00Z"
      }
    ]
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 6.11. GET `/api/leads/{id}/sla`

### Mô tả

Xem SLA first response của lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `view`

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "leadId": "guid",
    "createdAt": "2026-04-10T09:00:00Z",
    "firstResponseAt": "2026-04-10T10:00:00Z",
    "timeToFirstResponse": "01:00:00",
    "slaTargetHours": 24,
    "isSLACompliant": true,
    "isSLAOverdue": false
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 6.12. GET `/api/leads/statuses`

### Mô tả

Trả về danh sách `LeadStatus`.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `view`

### Success Response

```json
{
  "statuses": [
    "New",
    "Contacted",
    "BookedTest",
    "TestDone",
    "Enrolled",
    "Lost"
  ]
}
```

## 6.13. GET `/api/leads/{leadId}/children`

### Mô tả

Lấy danh sách child của lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `view`

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "children": [
      {
        "id": "guid",
        "leadId": "guid",
        "childName": "Be A",
        "dob": "2018-01-01",
        "gender": "Male",
        "programInterest": "English",
        "notes": null,
        "status": "New",
        "convertedStudentProfileId": null,
        "createdAt": "2026-04-10T09:00:00Z",
        "updatedAt": "2026-04-10T09:00:00Z"
      }
    ]
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 6.14. POST `/api/leads/{leadId}/children`

### Mô tả

Thêm child mới cho lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `create`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `childName` | `string` | Yes | |
| `dob` | `DateOnly?` | No | |
| `gender` | `Gender` | Yes | `Male`, `Female` |
| `programInterest` | `string?` | No | |
| `notes` | `string?` | No | |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "leadId": "guid",
    "childName": "Be A",
    "dob": "2018-01-01",
    "gender": "Male",
    "programInterest": "English",
    "notes": null,
    "status": "New",
    "createdAt": "2026-04-10T09:00:00Z",
    "updatedAt": "2026-04-10T09:00:00Z"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | Validation | `childName` rỗng |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 6.15. PUT `/api/leads/{leadId}/children/{childId}`

### Mô tả

Cập nhật child của lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `edit`

### Request Body

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `childName` | `string?` | No | |
| `dob` | `DateTime?` | No | Contract hiện là `DateTime?` |
| `gender` | `Gender?` | No | Contract hiện có field nhưng handler chưa update thực tế |
| `programInterest` | `string?` | No | |
| `notes` | `string?` | No | |

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "leadId": "guid",
    "childName": "Be A",
    "dob": "2018-01-01",
    "gender": "Male",
    "programInterest": "English",
    "notes": null,
    "status": "New",
    "updatedAt": "2026-04-10T10:00:00Z"
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `404` | `Lead.NotFound` | Lead không tồn tại |
| `404` | `LeadChild` | Child không tồn tại trong lead |

## 6.16. DELETE `/api/leads/{leadId}/children/{childId}`

### Mô tả

Xóa child khỏi lead.

### Roles / Scope / Action

- Role: `Admin`, `ManagementStaff`, `AccountantStaff`
- Scope: `all`
- Action: `delete`

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "leadId": "guid",
    "success": true
  }
}
```

### Error Response

| Status | Code | Meaning |
| --- | --- | --- |
| `400` | Validation | Child đã có placement test |
| `400` | Validation | Child đang `Enrolled` |
| `404` | `Lead.NotFound` | Lead không tồn tại |
| `404` | `LeadChild` | Child không tồn tại trong lead |

## 7. Status Definition

## 7.1. Lead Status

| Status | Meaning |
| --- | --- |
| `New` | Lead mới tạo, chưa ghi nhận contact thực tế với phụ huynh |
| `Contacted` | Đã có contact thực tế hoặc staff set status thủ công |
| `BookedTest` | Đã đặt lịch placement test |
| `TestDone` | Đã hoàn tất placement test |
| `Enrolled` | Đã convert thành đăng ký / enrolled |
| `Lost` | Lead không theo tiếp |

## 7.2. Lead Child Status

| Status | Meaning |
| --- | --- |
| `New` | Child mới tạo |
| `BookedTest` | Child đã đặt lịch test |
| `TestDone` | Child đã test xong |
| `Enrolled` | Child đã convert thành student |
| `Lost` | Child không theo tiếp |

## 7.3. Activity Type

| ActivityType | Meaning |
| --- | --- |
| `Call` | Gọi điện |
| `Zalo` | Nhắn / trao đổi qua Zalo |
| `Sms` | Nhắn SMS |
| `Email` | Gửi email |
| `Note` | Ghi chú nội bộ |

## 8. Status Transition Flow

## 8.1. Lead Status Flow

### Tạo lead

- `POST /api/leads/public` -> `Lead.Status = New`
- `POST /api/leads` -> `Lead.Status = New`

### Contact lead

- `POST /api/leads/{id}/notes`
- Nếu `activityType` là `Call`, `Zalo`, `Sms`, `Email` và current status là `New`
- Backend tự động:
  - set `FirstResponseAt` nếu chưa có
  - chuyển `Lead.Status` từ `New` sang `Contacted`

### Manual status update

- `PATCH /api/leads/{id}/status` có thể đổi status thủ công
- Rule cứng hiện tại:
  - nếu current status là `Enrolled`
  - và target status khác `Enrolled`
  - trả `400 Lead.InvalidStatusTransition`

### Placement test / enrollment related transitions

Ngoài `LeadController`, một số flow khác trong hệ thống cũng có thể cập nhật `Lead.Status`:

- Schedule placement test -> có thể set `BookedTest`
- Update placement test results -> có thể set `TestDone`
- Convert lead to enrolled -> set `Enrolled`

Các flow này nằm ở module Placement Test.

## 8.2. Lead Child Status Flow

Hiện child được tạo mới với:

- `LeadChild.Status = New`

Các transition child-level chủ yếu đi qua flow placement test / convert enrollment.

## 9. Permission Matrix By Role

| Endpoint | Anonymous | Admin | ManagementStaff | AccountantStaff | Teacher | Student | Parent |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST /api/leads/public` | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| `POST /api/leads` | No | Yes | Yes | Yes | No | No | No |
| `GET /api/leads` | No | Yes | Yes | Yes | No | No | No |
| `GET /api/leads/{id}` | No | Yes | Yes | Yes | No | No | No |
| `PUT /api/leads/{id}` | No | Yes | Yes | Yes | No | No | No |
| `POST /api/leads/{id}/assign` | No | Yes | Yes | No | No | No | No |
| `POST /api/leads/{id}/self-assign` | No | No | Yes | No | No | No | No |
| `PATCH /api/leads/{id}/status` | No | Yes | Yes | Yes | No | No | No |
| `POST /api/leads/{id}/notes` | No | Yes | Yes | Yes | No | No | No |
| `GET /api/leads/{id}/activities` | No | Yes | Yes | Yes | No | No | No |
| `GET /api/leads/{id}/sla` | No | Yes | Yes | Yes | No | No | No |
| `GET /api/leads/statuses` | No | Yes | Yes | Yes | No | No | No |
| `GET /api/leads/{leadId}/children` | No | Yes | Yes | Yes | No | No | No |
| `POST /api/leads/{leadId}/children` | No | Yes | Yes | Yes | No | No | No |
| `PUT /api/leads/{leadId}/children/{childId}` | No | Yes | Yes | Yes | No | No | No |
| `DELETE /api/leads/{leadId}/children/{childId}` | No | Yes | Yes | Yes | No | No | No |

## 10. Validation Rules

## 10.1. Lead Create Rules

- Ít nhất một trong các field contact phải có dữ liệu thực:
  - `contactName`
  - `phone`
  - `email`
  - `zaloId`
- Không cho tạo lead trùng:
  - `phone`
  - `email`
  - `zaloId`
- `branchPreference` nếu có phải tồn tại
- `ownerStaffId` nếu có phải tồn tại
- `ownerStaffId` nếu có phải là:
  - `ManagementStaff`
  - hoặc `AccountantStaff`

## 10.2. Lead Update Rules

- Không cho update lead đang `Enrolled`
- `branchPreference` nếu có phải tồn tại
- Một số string field có thể clear bằng empty string

## 10.3. Status Rules

- Không cho chuyển từ `Enrolled` sang status khác qua `PATCH /status`
- `PATCH /status` sang `Contacted` sẽ set `firstResponseAt` nếu chưa có
- `POST /notes` với `Call/Zalo/Sms/Email` cũng sẽ set `firstResponseAt` nếu chưa có

## 10.4. Lead Activity / Note Rules

- `content` bắt buộc
- `nextActionAt` nếu có phải `>= now`
- không được truyền đồng thời `nextActionAt` và `clearNextAction = true`
- `activityType = Note` không auto đổi status
- `activityType = Call/Zalo/Sms/Email` có thể auto `New -> Contacted`

## 10.5. Lead Child Rules

- Tạo child mới:
  - `childName` bắt buộc
- Xóa child:
  - không được xóa nếu child đã có placement test
  - không được xóa nếu child đang `Enrolled`

## 11. Error Cases

## 11.1. Lead-related errors

| HTTP | Code | Detail |
| --- | --- | --- |
| `404` | `Lead.NotFound` | Lead không tồn tại |
| `400` | `Lead.InvalidContactInfo` | Thiếu contact tối thiểu khi create |
| `404` | `Lead.OwnerNotFound` | Owner không tồn tại |
| `400` | `Lead.OwnerNotStaff` | Owner không phải staff hợp lệ |
| `404` | `Lead.BranchNotFound` | Branch không tồn tại |
| `409` | `Lead.DuplicateLead` | Lead trùng phone / email / zaloId |
| `400` | `Lead.CannotUpdateConvertedLead` | Không được update lead đã `Enrolled` |
| `400` | `Lead.InvalidStatusTransition` | Transition status không hợp lệ |

## 11.2. Activity / note errors

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | Validation | `content` rỗng |
| `400` | Validation | `nextActionAt` ở quá khứ |
| `400` | Validation | Không được set `nextActionAt` và `clearNextAction` cùng lúc |
| `404` | `Lead.NotFound` | Lead không tồn tại |

## 11.3. Lead child errors

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | Validation | `ChildName` rỗng khi create |
| `404` | `LeadChild` | Child không tồn tại trong lead |
| `400` | Validation | Không thể xóa child đã có placement test |
| `400` | Validation | Không thể xóa child đang `Enrolled` |

## 11.4. Auth errors

| HTTP | Case |
| --- | --- |
| `401` | Chưa đăng nhập hoặc token không hợp lệ |
| `403` | Có token nhưng role không đủ quyền |

## 12. Implementation Notes

### 12.1. Scope note

Hiện chưa có logic phân quyền `own` / `department`. Tất cả role lead nội bộ đang thấy toàn bộ dữ liệu lead.

### 12.2. LeadActivity note

`POST /api/leads/{id}/notes` không chỉ là note text. Đây là endpoint ghi nhận `LeadActivity` cho toàn bộ tương tác lead.

### 12.3. Auto contact note

Thay đổi mới đã áp dụng:

- Nếu dùng `POST /api/leads/{id}/notes`
- và `activityType` là `Call`, `Zalo`, `Sms`, `Email`
- thì lead `New` sẽ tự chuyển sang `Contacted`

Điều này giúp FE không cần gọi thêm `PATCH /status` chỉ để đánh dấu đã contact.

### 12.4. Next action note

`nextActionAt` hiện được lưu ở:

- `LeadActivity.NextActionAt`
- `Lead.NextActionAt`

Mục đích là để lưu lịch follow-up tiếp theo. Hiện backend chưa có reminder job hoặc auto overdue handling. Từ thay đổi mới này, FE có thể gửi `clearNextAction = true` qua `POST /api/leads/{id}/notes` để xóa lịch follow-up hiện tại mà không cần tạo API mới.

### 12.5. Known gap: Update Lead Child

Implementation hiện tại của `PUT /api/leads/{leadId}/children/{childId}` có dấu hiệu chưa hoàn thiện:

- field `dob`
- field `gender`

đã có trong request contract nhưng phần update trong handler đang bị comment / chưa xử lý đầy đủ. FE nên test kỹ trước khi dựa hoàn toàn vào endpoint này cho 2 field đó.
