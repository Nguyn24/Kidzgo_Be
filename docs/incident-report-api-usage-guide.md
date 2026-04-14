# Incident Report API Usage Guide

## 1. Tổng quan

Flow này là API mới riêng cho chức năng `Báo cáo sự cố`, dùng entity `Ticket` làm storage nhưng **không dùng lại API ticket cũ**.

Base route:

```text
/api/incident-reports
```

Yêu cầu chung:

- Cần `Authorization: Bearer <access_token>`
- JSON enum đang serialize/deserialize dạng `string`
- JSON response đang dùng `camelCase`

Role được phép dùng flow này:

- `Teacher`
- `ManagementStaff`
- `AccountantStaff`
- `Admin`

Role theo quyền:

- `Teacher`, `ManagementStaff`, `AccountantStaff`: tạo, xem danh sách của chính mình, xem chi tiết của chính mình, thêm comment cho incident của chính mình
- `Admin`: xem tất cả, assign, update status, xem statistics

## 2. Enum dùng cho FE

### 2.1 IncidentReportCategory

```json
[
  "Classroom",
  "Student",
  "TeachingMaterial",
  "TeachingSchedule",
  "Equipment",
  "System",
  "Academic",
  "Finance",
  "Operations",
  "ParentStudentFeedback",
  "Other"
]
```

### 2.2 IncidentReportStatus

```json
[
  "Open",
  "InProgress",
  "Resolved",
  "Closed",
  "Rejected"
]
```

### 2.3 IncidentReportCommentType

```json
[
  "AdditionalInfo",
  "Evidence",
  "ProcessingNote"
]
```

## 3. Chuẩn response

### 3.1 Success response

API success đang trả theo format:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 3.2 Error response

Business error và validation error đang trả theo `ProblemDetails`.

Ví dụ:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "IncidentReport.NotFound",
  "status": 404,
  "detail": "Incident report with Id = '6f2cbe57-d0a2-4fc1-bff8-6a1f8a22e8b1' was not found"
}
```

Validation error:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "Subject",
      "description": "'Subject' must not be empty.",
      "type": 1
    }
  ]
}
```

Lưu ý quan trọng:

- `401/403` có thể đến từ `[Authorize]` của ASP.NET nếu user chưa login hoặc không đúng role controller
- Một số lỗi permission trong handler đang map thành `500` vì backend đang dùng `ErrorType.Failure`, FE nên đọc thêm `title` và `detail`, không chỉ dựa vào status code

## 4. DTO response

### 4.1 IncidentReportDto

```json
{
  "id": "guid",
  "openedByUserId": "guid",
  "openedByUserName": "string",
  "branchId": "guid",
  "branchName": "string",
  "classId": "guid | null",
  "classCode": "string | null",
  "classTitle": "string | null",
  "category": "Classroom",
  "subject": "string",
  "message": "string",
  "status": "Open",
  "assignedToUserId": "guid | null",
  "assignedToUserName": "string | null",
  "evidenceUrl": "string | null",
  "createdAt": "2026-04-14T14:00:00+07:00",
  "updatedAt": "2026-04-14T14:00:00+07:00",
  "commentCount": 0
}
```

### 4.2 IncidentReportCommentDto

```json
{
  "id": "guid",
  "commenterUserId": "guid",
  "commenterUserName": "string",
  "message": "string",
  "attachmentUrl": "string | null",
  "commentType": "AdditionalInfo",
  "createdAt": "2026-04-14T14:00:00+07:00"
}
```

### 4.3 IncidentReportDetailDto

Giống `IncidentReportDto` và thêm:

```json
{
  "comments": [
    {
      "id": "guid",
      "commenterUserId": "guid",
      "commenterUserName": "string",
      "message": "string",
      "attachmentUrl": "string | null",
      "commentType": "Evidence",
      "createdAt": "2026-04-14T14:10:00+07:00"
    }
  ]
}
```

## 5. API chi tiết

## 5.1 Tạo báo cáo sự cố

### Endpoint

```http
POST /api/incident-reports
```

### Roles

- `Teacher`
- `ManagementStaff`
- `AccountantStaff`
- `Admin`

### Request body

```json
{
  "branchId": "0a76da07-cb7d-4b78-bef7-f1c7b0ab8c6b",
  "classId": "d5a5fbbf-d3f0-4ef8-98db-a54f3a0dbce9",
  "category": "System",
  "subject": "Không mở được màn hình điểm danh",
  "message": "Teacher vào lớp lúc 18h30 nhưng màn hình điểm danh load trắng.",
  "evidenceUrl": "https://cdn.example.com/evidence/incident-01.png"
}
```

### Validate

- `branchId`: bắt buộc
- `category`: bắt buộc, đúng enum
- `subject`: bắt buộc, tối đa `200`
- `message`: bắt buộc, tối đa `2000`
- `classId`: optional

### Success response

Status: `201 Created`

```json
{
  "isSuccess": true,
  "data": {
    "id": "96d0fd38-d39b-4c17-a69f-5f4a28f86877",
    "openedByUserId": "2fbb5da5-3b57-40bf-b90e-5b55196950d2",
    "openedByUserName": "Nguyen Van A",
    "branchId": "0a76da07-cb7d-4b78-bef7-f1c7b0ab8c6b",
    "branchName": "CN Quan 7",
    "classId": "d5a5fbbf-d3f0-4ef8-98db-a54f3a0dbce9",
    "classCode": "IE-KID-01",
    "classTitle": "Kid English 01",
    "category": "System",
    "subject": "Không mở được màn hình điểm danh",
    "message": "Teacher vào lớp lúc 18h30 nhưng màn hình điểm danh load trắng.",
    "status": "Open",
    "assignedToUserId": null,
    "assignedToUserName": null,
    "evidenceUrl": "https://cdn.example.com/evidence/incident-01.png",
    "createdAt": "2026-04-14T14:00:00+07:00",
    "updatedAt": "2026-04-14T14:00:00+07:00",
    "commentCount": 0,
    "comments": []
  }
}
```

### Error cases

- `400 Validation.General`
- `404 Ticket.UserNotFound`
- `404 Ticket.BranchNotFound`
- `404 Ticket.ClassNotFound`
- `500 IncidentReport.InvalidRole`

## 5.2 Lấy danh sách báo cáo sự cố

### Endpoint

```http
GET /api/incident-reports
```

### Roles

- `Teacher`
- `ManagementStaff`
- `AccountantStaff`
- `Admin`

### Query params

- `branchId`: `guid`, optional
- `openedByUserId`: `guid`, optional
- `assignedToUserId`: `guid`, optional
- `classId`: `guid`, optional
- `category`: `IncidentReportCategory`, optional
- `status`: `IncidentReportStatus`, optional
- `keyword`: `string`, optional
- `createdFrom`: `datetime`, optional
- `createdTo`: `datetime`, optional
- `pageNumber`: `int`, default `1`
- `pageSize`: `int`, default `10`

### Permission behavior

- `Admin`: thấy tất cả incident
- Role còn lại: backend tự giới hạn chỉ thấy incident do chính user đó tạo

### Sample request

```http
GET /api/incident-reports?status=Open&category=System&pageNumber=1&pageSize=10
```

### Success response

Status: `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "incidentReports": {
      "items": [
        {
          "id": "96d0fd38-d39b-4c17-a69f-5f4a28f86877",
          "openedByUserId": "2fbb5da5-3b57-40bf-b90e-5b55196950d2",
          "openedByUserName": "Nguyen Van A",
          "branchId": "0a76da07-cb7d-4b78-bef7-f1c7b0ab8c6b",
          "branchName": "CN Quan 7",
          "classId": null,
          "classCode": null,
          "classTitle": null,
          "category": "System",
          "subject": "Không mở được màn hình điểm danh",
          "message": "Teacher vào lớp lúc 18h30 nhưng màn hình điểm danh load trắng.",
          "status": "Open",
          "assignedToUserId": null,
          "assignedToUserName": null,
          "evidenceUrl": "https://cdn.example.com/evidence/incident-01.png",
          "createdAt": "2026-04-14T14:00:00+07:00",
          "updatedAt": "2026-04-14T14:00:00+07:00",
          "commentCount": 0
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

### Error cases

- `404 Ticket.UserNotFound`
- `500 IncidentReport.InvalidRole`

## 5.3 Lấy chi tiết 1 báo cáo sự cố

### Endpoint

```http
GET /api/incident-reports/{id}
```

### Roles

- `Teacher`
- `ManagementStaff`
- `AccountantStaff`
- `Admin`

### Permission behavior

- `Admin`: xem mọi incident
- Role còn lại: chỉ xem incident do chính mình tạo

### Success response

Status: `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "id": "96d0fd38-d39b-4c17-a69f-5f4a28f86877",
    "openedByUserId": "2fbb5da5-3b57-40bf-b90e-5b55196950d2",
    "openedByUserName": "Nguyen Van A",
    "branchId": "0a76da07-cb7d-4b78-bef7-f1c7b0ab8c6b",
    "branchName": "CN Quan 7",
    "classId": null,
    "classCode": null,
    "classTitle": null,
    "category": "System",
    "subject": "Không mở được màn hình điểm danh",
    "message": "Teacher vào lớp lúc 18h30 nhưng màn hình điểm danh load trắng.",
    "status": "InProgress",
    "assignedToUserId": "f4b855d7-70b1-48b7-a11e-f26728aa15dc",
    "assignedToUserName": "Tran Thi B",
    "evidenceUrl": "https://cdn.example.com/evidence/incident-01.png",
    "createdAt": "2026-04-14T14:00:00+07:00",
    "updatedAt": "2026-04-14T14:30:00+07:00",
    "commentCount": 2,
    "comments": [
      {
        "id": "a6c9c2df-3db0-4a03-8450-169eaa3de88f",
        "commenterUserId": "2fbb5da5-3b57-40bf-b90e-5b55196950d2",
        "commenterUserName": "Nguyen Van A",
        "message": "Đính kèm thêm ảnh lỗi.",
        "attachmentUrl": "https://cdn.example.com/evidence/incident-02.png",
        "commentType": "Evidence",
        "createdAt": "2026-04-14T14:05:00+07:00"
      },
      {
        "id": "2c48f040-4cfd-45a2-8ca5-484b85254fd1",
        "commenterUserId": "f4b855d7-70b1-48b7-a11e-f26728aa15dc",
        "commenterUserName": "Tran Thi B",
        "message": "Đã tiếp nhận và đang kiểm tra.",
        "attachmentUrl": null,
        "commentType": "ProcessingNote",
        "createdAt": "2026-04-14T14:20:00+07:00"
      }
    ]
  }
}
```

### Error cases

- `404 Ticket.UserNotFound`
- `404 IncidentReport.NotFound`
- `500 IncidentReport.Unauthorized`

## 5.4 Thêm comment / bổ sung minh chứng

### Endpoint

```http
POST /api/incident-reports/{id}/comments
```

### Roles

- `Teacher`
- `ManagementStaff`
- `AccountantStaff`
- `Admin`

### Permission behavior

- `Admin`: comment mọi incident
- Role còn lại: chỉ comment incident do chính mình tạo

### Request body

```json
{
  "message": "Em bổ sung thêm hình ảnh lỗi ở máy tính phòng A2.",
  "attachmentUrl": "https://cdn.example.com/evidence/incident-03.png",
  "commentType": "Evidence"
}
```

### Validate

- `message`: bắt buộc, tối đa `2000`
- `commentType`: đúng enum

### Success response

Status: `201 Created`

```json
{
  "isSuccess": true,
  "data": {
    "id": "a6c9c2df-3db0-4a03-8450-169eaa3de88f",
    "commenterUserId": "2fbb5da5-3b57-40bf-b90e-5b55196950d2",
    "commenterUserName": "Nguyen Van A",
    "message": "Em bổ sung thêm hình ảnh lỗi ở máy tính phòng A2.",
    "attachmentUrl": "https://cdn.example.com/evidence/incident-03.png",
    "commentType": "Evidence",
    "createdAt": "2026-04-14T14:05:00+07:00"
  }
}
```

### Error cases

- `400 Validation.General`
- `404 Ticket.UserNotFound`
- `404 IncidentReport.NotFound`
- `500 IncidentReport.Unauthorized`

## 5.5 Assign người xử lý

### Endpoint

```http
PATCH /api/incident-reports/{id}/assign
```

### Roles

- `Admin`

### Request body

```json
{
  "assignedToUserId": "f4b855d7-70b1-48b7-a11e-f26728aa15dc"
}
```

### Business behavior

- Có thể assign cho `Admin`, `ManagementStaff`, `AccountantStaff`, `Teacher`
- Nếu assignee không phải `Admin` thì phải cùng `branch` với incident
- Nếu incident đang `Open`, sau khi assign backend tự chuyển sang `InProgress`

### Success response

Status: `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "id": "96d0fd38-d39b-4c17-a69f-5f4a28f86877",
    "openedByUserId": "2fbb5da5-3b57-40bf-b90e-5b55196950d2",
    "openedByUserName": "Nguyen Van A",
    "branchId": "0a76da07-cb7d-4b78-bef7-f1c7b0ab8c6b",
    "branchName": "CN Quan 7",
    "classId": null,
    "classCode": null,
    "classTitle": null,
    "category": "System",
    "subject": "Không mở được màn hình điểm danh",
    "message": "Teacher vào lớp lúc 18h30 nhưng màn hình điểm danh load trắng.",
    "status": "InProgress",
    "assignedToUserId": "f4b855d7-70b1-48b7-a11e-f26728aa15dc",
    "assignedToUserName": "Tran Thi B",
    "evidenceUrl": "https://cdn.example.com/evidence/incident-01.png",
    "createdAt": "2026-04-14T14:00:00+07:00",
    "updatedAt": "2026-04-14T14:30:00+07:00",
    "commentCount": 2
  }
}
```

### Error cases

- `400 Validation.General`
- `404 Ticket.UserNotFound`
- `404 IncidentReport.NotFound`
- `404 IncidentReport.AssignedUserNotFound`
- `409 IncidentReport.AssignedUserBranchMismatch`
- `403` từ authorize nếu không phải `Admin`
- `500 IncidentReport.AssignAdminOnly`

## 5.6 Cập nhật trạng thái

### Endpoint

```http
PATCH /api/incident-reports/{id}/status
```

### Roles

- `Admin`

### Request body

```json
{
  "status": "Resolved"
}
```

### Luồng chuyển trạng thái hợp lệ

- `Open` -> `InProgress`, `Resolved`, `Closed`, `Rejected`
- `InProgress` -> `Resolved`, `Closed`, `Rejected`
- `Resolved` -> `Closed`, `Rejected`
- `Closed` -> không chuyển tiếp
- `Rejected` -> không chuyển tiếp
- Không cho update sang chính trạng thái hiện tại

### Success response

Status: `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "id": "96d0fd38-d39b-4c17-a69f-5f4a28f86877",
    "openedByUserId": "2fbb5da5-3b57-40bf-b90e-5b55196950d2",
    "openedByUserName": "Nguyen Van A",
    "branchId": "0a76da07-cb7d-4b78-bef7-f1c7b0ab8c6b",
    "branchName": "CN Quan 7",
    "classId": null,
    "classCode": null,
    "classTitle": null,
    "category": "System",
    "subject": "Không mở được màn hình điểm danh",
    "message": "Teacher vào lớp lúc 18h30 nhưng màn hình điểm danh load trắng.",
    "status": "Resolved",
    "assignedToUserId": "f4b855d7-70b1-48b7-a11e-f26728aa15dc",
    "assignedToUserName": "Tran Thi B",
    "evidenceUrl": "https://cdn.example.com/evidence/incident-01.png",
    "createdAt": "2026-04-14T14:00:00+07:00",
    "updatedAt": "2026-04-14T15:00:00+07:00",
    "commentCount": 2
  }
}
```

### Error cases

- `400 Validation.General`
- `404 Ticket.UserNotFound`
- `404 IncidentReport.NotFound`
- `409 IncidentReport.InvalidStatusTransition`
- `403` từ authorize nếu không phải `Admin`
- `500 IncidentReport.UpdateStatusAdminOnly`

## 5.7 Thống kê incident

### Endpoint

```http
GET /api/incident-reports/statistics
```

### Roles

- `Admin`

### Query params

- `branchId`: `guid`, optional
- `openedByUserId`: `guid`, optional
- `assignedToUserId`: `guid`, optional
- `classId`: `guid`, optional
- `category`: `IncidentReportCategory`, optional
- `status`: `IncidentReportStatus`, optional
- `keyword`: `string`, optional
- `createdFrom`: `datetime`, optional
- `createdTo`: `datetime`, optional

### Success response

Status: `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "total": 12,
    "open": 3,
    "inProgress": 4,
    "resolved": 2,
    "closed": 2,
    "rejected": 1,
    "unassigned": 5,
    "byStatus": [
      {
        "status": "Closed",
        "count": 2
      },
      {
        "status": "InProgress",
        "count": 4
      },
      {
        "status": "Open",
        "count": 3
      },
      {
        "status": "Rejected",
        "count": 1
      },
      {
        "status": "Resolved",
        "count": 2
      }
    ],
    "byCategory": [
      {
        "category": "System",
        "count": 5
      },
      {
        "category": "Operations",
        "count": 3
      },
      {
        "category": "Equipment",
        "count": 2
      }
    ]
  }
}
```

### Error cases

- `404 Ticket.UserNotFound`
- `403` từ authorize nếu không phải `Admin`
- `500 IncidentReport.Unauthorized`

## 6. Error code reference

### Auth / permission

- `IncidentReport.InvalidRole`: `Your role cannot use incident reports`
- `IncidentReport.Unauthorized`: `You do not have permission to access this incident report`
- `IncidentReport.AssignAdminOnly`: `Only admins can assign incident reports`
- `IncidentReport.UpdateStatusAdminOnly`: `Only admins can update incident report status`

### Not found

- `Ticket.UserNotFound`: `User not found`
- `Ticket.BranchNotFound`: `Branch not found or inactive`
- `Ticket.ClassNotFound`: `Class not found`
- `IncidentReport.NotFound`: `Incident report with Id = '{id}' was not found`
- `IncidentReport.AssignedUserNotFound`: `Assigned handler not found`

### Conflict

- `IncidentReport.AssignedUserBranchMismatch`: `Assigned handler must belong to the same branch as the incident`
- `IncidentReport.InvalidStatusTransition`: `Cannot change incident status from {current} to {target}`

### Validation

- `Validation.General`: `One or more validation errors occurred`

## 7. FE implementation notes

- Khi gửi enum, dùng đúng string enum backend đang expose, ví dụ `System`, `InProgress`, `Evidence`
- Với user không phải `Admin`, FE không nên hiển thị filter `openedByUserId` như quyền admin, vì backend vẫn tự khóa dữ liệu về incident của chính user
- `PATCH /assign` sẽ tự đổi status từ `Open` sang `InProgress`, FE nên cập nhật UI theo response trả về thay vì tự suy luận
- Các lỗi permission nghiệp vụ trong handler hiện có thể ra `500`, FE nên đọc `title` và `detail` để hiển thị message đúng
- `createdFrom` và `createdTo` nên gửi ISO datetime

## 8. Suggested FE flow

### Teacher / Staff

1. Tạo incident bằng `POST /api/incident-reports`
2. Load list của mình bằng `GET /api/incident-reports`
3. Xem chi tiết bằng `GET /api/incident-reports/{id}`
4. Bổ sung thông tin hoặc minh chứng bằng `POST /api/incident-reports/{id}/comments`

### Admin

1. Load toàn bộ list bằng `GET /api/incident-reports`
2. Xem detail bằng `GET /api/incident-reports/{id}`
3. Assign handler bằng `PATCH /api/incident-reports/{id}/assign`
4. Update status bằng `PATCH /api/incident-reports/{id}/status`
5. Lấy dashboard numbers bằng `GET /api/incident-reports/statistics`
