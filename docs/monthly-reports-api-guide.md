# Hướng dẫn sử dụng API Monthly Reports

## Tổng quan

API Monthly Reports cho phép tạo, quản lý và publish báo cáo học tập hàng tháng cho học sinh. Workflow bao gồm:
1. **Tạo Job** → 2. **Aggregate Data** → 3. **Teacher viết draft** → 4. **Submit** → 5. **Review/Approve** → 6. **Publish**

---

## Base URL

```
/api/monthly-reports
```

**Lưu ý**: Tất cả endpoints đều yêu cầu authentication (Bearer token)

---

## 1. Tạo Monthly Report Job

**UC-174**: Tạo job để tạo reports cho tất cả học sinh trong branch

### Endpoint
```
POST /api/monthly-reports/jobs
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Request Body
```json
{
  "month": 12,
  "year": 2024,
  "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Request Parameters
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `month` | int | Yes | Tháng (1-12) |
| `year` | int | Yes | Năm (2000-2100) |
| `branchId` | Guid | Yes | ID của branch |

### Response (201 Created)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "month": 12,
  "year": 2024,
  "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2024-12-01T10:00:00Z"
}
```

### Response Headers
```
Location: /api/monthly-reports/jobs/{jobId}
```

### Example (cURL)
```bash
curl -X POST "https://api.kidzgo.com/api/monthly-reports/jobs" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "month": 12,
    "year": 2024,
    "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

### Error Responses
- **400 Bad Request**: Month/year không hợp lệ
- **404 Not Found**: Branch không tồn tại
- **401 Unauthorized**: Chưa đăng nhập hoặc không có quyền

---

## 2. Xem danh sách Monthly Report Jobs

**UC-177**: Xem danh sách các jobs đã tạo

### Endpoint
```
GET /api/monthly-reports/jobs
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `branchId` | Guid | No | Filter theo branch |
| `month` | int | No | Filter theo tháng (1-12) |
| `year` | int | No | Filter theo năm |
| `status` | string | No | Filter theo status: `Pending`, `Generating`, `Done`, `Failed` |

### Response (200 OK)
```json
{
  "jobs": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "month": 12,
      "year": 2024,
      "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "branchName": "Chi nhánh Quận 1",
      "status": "Done",
      "startedAt": "2024-12-01T10:00:00Z",
      "finishedAt": "2024-12-01T10:05:00Z",
      "errorMessage": null,
      "retryCount": 0,
      "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "createdByName": "Nguyễn Văn A",
      "createdAt": "2024-12-01T10:00:00Z",
      "reportCount": 25
    }
  ]
}
```

### Example (cURL)
```bash
curl -X GET "https://api.kidzgo.com/api/monthly-reports/jobs?branchId={branchId}&year=2024&month=12" \
  -H "Authorization: Bearer {token}"
```

---

## 3. Xem chi tiết Monthly Report Job

**UC-178**: Xem thông tin chi tiết và danh sách reports của một job

### Endpoint
```
GET /api/monthly-reports/jobs/{jobId}
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `jobId` | Guid | Yes | ID của job |

### Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "month": 12,
  "year": 2024,
  "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "branchName": "Chi nhánh Quận 1",
  "status": "Done",
  "startedAt": "2024-12-01T10:00:00Z",
  "finishedAt": "2024-12-01T10:05:00Z",
  "errorMessage": null,
  "retryCount": 0,
  "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdByName": "Nguyễn Văn A",
  "createdAt": "2024-12-01T10:00:00Z",
  "updatedAt": "2024-12-01T10:05:00Z",
  "reports": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "studentName": "Nguyễn Văn B",
      "classId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "className": "Lớp A1",
      "status": "Draft",
      "createdAt": "2024-12-01T10:01:00Z"
    }
  ]
}
```

### Example (cURL)
```bash
curl -X GET "https://api.kidzgo.com/api/monthly-reports/jobs/{jobId}" \
  -H "Authorization: Bearer {token}"
```

---

## 4. Aggregate Data cho Monthly Reports

**UC-175**: Gom dữ liệu từ các nguồn (attendance, homework, test, mission, session reports) cho tất cả học sinh trong job

### Endpoint
```
POST /api/monthly-reports/jobs/{jobId}/aggregate
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `jobId` | Guid | Yes | ID của job |

### Response (200 OK)
```json
{
  "totalReportsCreated": 20,
  "totalReportsUpdated": 5,
  "reportIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "3fa85f64-5717-4562-b3fc-2c963f66afa7"
  ]
}
```

### Lưu ý
- Endpoint này sẽ:
  - Tạo `StudentMonthlyReport` cho tất cả học sinh active trong branch
  - Aggregate dữ liệu từ: SessionReports, Attendance, Homework, ExamResults, Missions, Stars/XP
  - Lưu vào `MonthlyReportData` (JSON format)
  - Đánh dấu SessionReports đã được compiled (`IsMonthlyCompiled = true`)
  - Update job status thành `Done`

### Example (cURL)
```bash
curl -X POST "https://api.kidzgo.com/api/monthly-reports/jobs/{jobId}/aggregate" \
  -H "Authorization: Bearer {token}"
```

---

## 5. Xem Monthly Report

**UC-179**: Teacher xem draft report  
**UC-186**: Parent/Student xem published report

### Endpoint
```
GET /api/monthly-reports/{reportId}
```

### Authorization
- Roles: `Teacher`, `Admin`, `ManagementStaff`, `Parent`, `Student`
- **Teacher**: Chỉ xem reports của lớp mình
- **Parent/Student**: Chỉ xem reports của mình/con mình, và chỉ khi status = `Published`
- **Admin/ManagementStaff**: Xem tất cả

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reportId` | Guid | Yes | ID của report |

### Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "studentName": "Nguyễn Văn B",
  "classId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "className": "Lớp A1",
  "jobId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "month": 12,
  "year": 2024,
  "draftContent": "<html>...</html>",
  "finalContent": null,
  "status": "Draft",
  "pdfUrl": null,
  "pdfGeneratedAt": null,
  "submittedBy": null,
  "submittedByName": null,
  "reviewedBy": null,
  "reviewedByName": null,
  "reviewedAt": null,
  "publishedAt": null,
  "createdAt": "2024-12-01T10:01:00Z",
  "updatedAt": "2024-12-01T10:01:00Z",
  "data": {
    "attendanceData": "{\"total\":20,\"present\":18,\"absent\":2,\"makeup\":0,\"notMarked\":0,\"percentage\":90.0}",
    "homeworkData": "{\"total\":10,\"completed\":8,\"submitted\":1,\"pending\":1,\"late\":0,\"missing\":0,\"average\":8.5,\"completionRate\":90.0}",
    "testData": "{\"total\":2,\"tests\":[{\"examId\":\"...\",\"type\":\"Progress\",\"score\":85,\"maxScore\":100,\"date\":\"2024-12-15\",\"comment\":\"Good work\"}]}",
    "missionData": "{\"completed\":5,\"total\":6,\"inProgress\":1,\"stars\":150,\"xp\":500,\"currentLevel\":\"5\",\"currentXp\":1200}",
    "notesData": "{\"total\":20,\"sessionReports\":[{\"sessionId\":\"...\",\"sessionDate\":\"2024-12-01\",\"reportDate\":\"2024-12-01\",\"teacherName\":\"Nguyễn Văn C\",\"feedback\":\"Student is making good progress\",\"aiGeneratedSummary\":null}]}"
  },
  "comments": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "commenterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "commenterName": "Nguyễn Văn D",
      "content": "Please add more details about speaking skills",
      "createdAt": "2024-12-02T10:00:00Z"
    }
  ]
}
```

### Data Format

#### AttendanceData (JSON)
```json
{
  "total": 20,
  "present": 18,
  "absent": 2,
  "makeup": 0,
  "notMarked": 0,
  "percentage": 90.0
}
```

#### HomeworkData (JSON)
```json
{
  "total": 10,
  "completed": 8,
  "submitted": 1,
  "pending": 1,
  "late": 0,
  "missing": 0,
  "average": 8.5,
  "completionRate": 90.0
}
```

#### TestData (JSON)
```json
{
  "total": 2,
  "tests": [
    {
      "examId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "type": "Progress",
      "score": 85,
      "maxScore": 100,
      "date": "2024-12-15",
      "comment": "Good work"
    }
  ]
}
```

#### MissionData (JSON)
```json
{
  "completed": 5,
  "total": 6,
  "inProgress": 1,
  "stars": 150,
  "xp": 500,
  "currentLevel": "5",
  "currentXp": 1200
}
```

#### NotesData (JSON)
```json
{
  "total": 20,
  "sessionReports": [
    {
      "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "sessionDate": "2024-12-01",
      "reportDate": "2024-12-01",
      "teacherName": "Nguyễn Văn C",
      "feedback": "Student is making good progress",
      "aiGeneratedSummary": null
    }
  ]
}
```

### Example (cURL)
```bash
curl -X GET "https://api.kidzgo.com/api/monthly-reports/{reportId}" \
  -H "Authorization: Bearer {token}"
```

---

## 6. Chỉnh sửa Draft Monthly Report

**UC-180**: Teacher chỉnh sửa nội dung draft report

### Endpoint
```
PUT /api/monthly-reports/{reportId}/draft
```

### Authorization
- Roles: `Teacher`
- Chỉ có thể edit khi status = `Draft` hoặc `Review`
- Chỉ có thể edit reports của lớp mình

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reportId` | Guid | Yes | ID của report |

### Request Body
```json
{
  "draftContent": "<html><body><h1>Monthly Report December 2024</h1><p>Student has shown excellent progress...</p></body></html>"
}
```

### Request Parameters
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `draftContent` | string | Yes | Nội dung draft (HTML hoặc markdown) |

### Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "draftContent": "<html>...</html>",
  "updatedAt": "2024-12-02T10:00:00Z"
}
```

### Error Responses
- **400 Bad Request**: Status không phải Draft hoặc Review
- **401 Unauthorized**: Không phải teacher của lớp này
- **404 Not Found**: Report không tồn tại

### Example (cURL)
```bash
curl -X PUT "https://api.kidzgo.com/api/monthly-reports/{reportId}/draft" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "draftContent": "<html><body><h1>Monthly Report</h1></body></html>"
  }'
```

---

## 7. Submit Monthly Report

**UC-181**: Teacher submit report để review

### Endpoint
```
POST /api/monthly-reports/{reportId}/submit
```

### Authorization
- Roles: `Teacher`

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reportId` | Guid | Yes | ID của report |

### Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Review",
  "submittedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "updatedAt": "2024-12-02T10:00:00Z"
}
```

### Lưu ý
- Status sẽ chuyển từ `Draft` → `Review`
- Chỉ có thể submit khi status = `Draft`

### Example (cURL)
```bash
curl -X POST "https://api.kidzgo.com/api/monthly-reports/{reportId}/submit" \
  -H "Authorization: Bearer {token}"
```

---

## 8. Thêm Comment vào Monthly Report

**UC-182**: Staff/Admin thêm comment để feedback

### Endpoint
```
POST /api/monthly-reports/{reportId}/comments
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reportId` | Guid | Yes | ID của report |

### Request Body
```json
{
  "content": "Please add more details about speaking skills improvement"
}
```

### Request Parameters
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `content` | string | Yes | Nội dung comment |

### Response (201 Created)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reportId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "commenterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "content": "Please add more details about speaking skills improvement",
  "createdAt": "2024-12-02T10:00:00Z"
}
```

### Response Headers
```
Location: /api/monthly-reports/{reportId}/comments/{commentId}
```

### Example (cURL)
```bash
curl -X POST "https://api.kidzgo.com/api/monthly-reports/{reportId}/comments" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Please add more details about speaking skills"
  }'
```

---

## 9. Approve Monthly Report

**UC-183**: Staff/Admin approve report

### Endpoint
```
POST /api/monthly-reports/{reportId}/approve
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reportId` | Guid | Yes | ID của report |

### Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Approved",
  "reviewedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reviewedAt": "2024-12-02T11:00:00Z",
  "updatedAt": "2024-12-02T11:00:00Z"
}
```

### Lưu ý
- Status sẽ chuyển từ `Review` → `Approved`
- Chỉ có thể approve khi status = `Review`

### Example (cURL)
```bash
curl -X POST "https://api.kidzgo.com/api/monthly-reports/{reportId}/approve" \
  -H "Authorization: Bearer {token}"
```

---

## 10. Reject Monthly Report

**UC-184**: Staff/Admin reject report

### Endpoint
```
POST /api/monthly-reports/{reportId}/reject
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reportId` | Guid | Yes | ID của report |

### Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Rejected",
  "reviewedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reviewedAt": "2024-12-02T11:00:00Z",
  "updatedAt": "2024-12-02T11:00:00Z"
}
```

### Lưu ý
- Status sẽ chuyển từ `Review` → `Rejected`
- Teacher có thể edit lại và submit lại

### Example (cURL)
```bash
curl -X POST "https://api.kidzgo.com/api/monthly-reports/{reportId}/reject" \
  -H "Authorization: Bearer {token}"
```

---

## 11. Publish Monthly Report

**UC-185**: Publish report để Parent/Student có thể xem

### Endpoint
```
POST /api/monthly-reports/{reportId}/publish
```

### Authorization
- Roles: `Admin`, `ManagementStaff`

### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reportId` | Guid | Yes | ID của report |

### Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Published",
  "publishedAt": "2024-12-02T12:00:00Z",
  "updatedAt": "2024-12-02T12:00:00Z"
}
```

### Lưu ý
- Status sẽ chuyển từ `Approved` → `Published`
- Chỉ có thể publish khi status = `Approved`
- `DraftContent` sẽ được copy sang `FinalContent` (nếu FinalContent chưa có)
- **Tự động gửi notification** cho Parent/Student (UC-187)

### Example (cURL)
```bash
curl -X POST "https://api.kidzgo.com/api/monthly-reports/{reportId}/publish" \
  -H "Authorization: Bearer {token}"
```

---

## Workflow hoàn chỉnh

### Luồng 1: Tạo và Publish Report (Không dùng AI)

```
1. Admin tạo Job
   POST /api/monthly-reports/jobs
   → Job status: Pending

2. Admin aggregate data
   POST /api/monthly-reports/jobs/{jobId}/aggregate
   → Tạo reports cho tất cả students
   → Reports status: Draft
   → Job status: Done

3. Teacher xem và chỉnh sửa draft
   GET /api/monthly-reports/{reportId}
   PUT /api/monthly-reports/{reportId}/draft
   → Status: Draft

4. Teacher submit
   POST /api/monthly-reports/{reportId}/submit
   → Status: Review

5. Staff/Admin review và comment (optional)
   POST /api/monthly-reports/{reportId}/comments

6. Staff/Admin approve
   POST /api/monthly-reports/{reportId}/approve
   → Status: Approved

7. Staff/Admin publish
   POST /api/monthly-reports/{reportId}/publish
   → Status: Published
   → Tự động gửi notification cho Parent/Student
```

### Luồng 2: Reject và chỉnh sửa lại

```
1. Staff/Admin reject
   POST /api/monthly-reports/{reportId}/reject
   → Status: Rejected

2. Teacher chỉnh sửa lại
   PUT /api/monthly-reports/{reportId}/draft
   → Status: Rejected (vẫn có thể edit)

3. Teacher submit lại
   POST /api/monthly-reports/{reportId}/submit
   → Status: Review
```

---

## Report Status Flow

```
Draft → Review → Approved → Published
         ↓
      Rejected (có thể edit và submit lại)
```

### Status Descriptions

| Status | Description | Có thể edit? | Có thể xem? |
|--------|-------------|--------------|-------------|
| `Draft` | Teacher đang viết | ✅ Teacher | ✅ Teacher, Admin, Staff |
| `Review` | Đang chờ review | ✅ Teacher | ✅ Teacher, Admin, Staff |
| `Approved` | Đã được approve | ❌ | ✅ Teacher, Admin, Staff |
| `Rejected` | Bị reject, cần chỉnh sửa | ✅ Teacher | ✅ Teacher, Admin, Staff |
| `Published` | Đã publish, Parent/Student có thể xem | ❌ | ✅ Tất cả (theo authorization) |

---

## Authorization Rules

### Admin / ManagementStaff
- ✅ Tạo job
- ✅ Aggregate data
- ✅ Xem tất cả reports
- ✅ Comment reports
- ✅ Approve/Reject reports
- ✅ Publish reports

### Teacher
- ✅ Xem reports của lớp mình
- ✅ Edit draft/review reports của lớp mình
- ✅ Submit reports của lớp mình

### Parent
- ✅ Xem reports của con mình (chỉ khi Published)
- ❌ Không thể edit/comment

### Student
- ✅ Xem reports của mình (chỉ khi Published)
- ❌ Không thể edit/comment

---

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "code": "MonthlyReport.InvalidStatus",
  "description": "Cannot edit Monthly Report with status 'Published'"
}
```

#### 401 Unauthorized
```json
{
  "code": "MonthlyReport.Unauthorized",
  "description": "You can only view reports of your classes"
}
```

#### 404 Not Found
```json
{
  "code": "MonthlyReport.NotFound",
  "description": "Monthly Report with Id = '{reportId}' was not found"
}
```

#### 409 Conflict
```json
{
  "code": "MonthlyReport.AlreadyExists",
  "description": "Monthly Report for student '{studentProfileId}' in 12/2024 already exists"
}
```

---

## Data Aggregation Details

Khi gọi `POST /api/monthly-reports/jobs/{jobId}/aggregate`, hệ thống sẽ:

1. **Lấy danh sách học sinh**: Tất cả học sinh có `EnrollmentStatus = Active` trong branch
2. **Aggregate Session Reports**: Tất cả session reports trong tháng, đánh dấu `IsMonthlyCompiled = true`
3. **Aggregate Attendance**: Tính tổng số buổi, present, absent, makeup, percentage
4. **Aggregate Homework**: Tính tổng số bài tập, completed, pending, average score, completion rate
5. **Aggregate Test Results**: Lấy tất cả exam results trong tháng
6. **Aggregate Missions**: Tính số mission completed, stars, XP earned
7. **Lưu vào MonthlyReportData**: Tất cả dữ liệu được lưu dưới dạng JSON trong `MonthlyReportData`

---

## Best Practices

1. **Tạo Job trước khi aggregate**: Luôn tạo job trước, sau đó mới aggregate
2. **Kiểm tra job status**: Kiểm tra job status = `Done` trước khi teacher bắt đầu viết
3. **Validate data**: Teacher nên xem `data` field để đảm bảo dữ liệu đầy đủ trước khi viết
4. **Draft content format**: Có thể dùng HTML hoặc Markdown cho `draftContent`
5. **Review process**: Nên comment trước khi approve/reject để teacher biết cần sửa gì
6. **Publish timing**: Chỉ publish khi đã approve và chắc chắn nội dung đúng

---

## Example: Complete Workflow

### Step 1: Admin tạo job cho tháng 12/2024
```bash
POST /api/monthly-reports/jobs
{
  "month": 12,
  "year": 2024,
  "branchId": "branch-id-123"
}
# Response: jobId = "job-id-456"
```

### Step 2: Admin aggregate data
```bash
POST /api/monthly-reports/jobs/job-id-456/aggregate
# Response: { "totalReportsCreated": 25, ... }
```

### Step 3: Teacher xem danh sách reports của lớp mình
```bash
GET /api/monthly-reports/jobs/job-id-456
# Xem danh sách reports trong job
```

### Step 4: Teacher xem một report và data
```bash
GET /api/monthly-reports/report-id-789
# Xem draftContent (null), data (có đầy đủ)
```

### Step 5: Teacher viết draft
```bash
PUT /api/monthly-reports/report-id-789/draft
{
  "draftContent": "<html><body><h1>Báo cáo tháng 12/2024</h1><p>Học sinh đã có tiến bộ tốt...</p></body></html>"
}
```

### Step 6: Teacher submit
```bash
POST /api/monthly-reports/report-id-789/submit
# Status: Review
```

### Step 7: Staff comment (optional)
```bash
POST /api/monthly-reports/report-id-789/comments
{
  "content": "Vui lòng thêm chi tiết về kỹ năng speaking"
}
```

### Step 8: Staff approve
```bash
POST /api/monthly-reports/report-id-789/approve
# Status: Approved
```

### Step 9: Staff publish
```bash
POST /api/monthly-reports/report-id-789/publish
# Status: Published
# Tự động gửi notification cho Parent/Student
```

### Step 10: Parent xem report
```bash
GET /api/monthly-reports/report-id-789
# Xem finalContent (đã publish)
```

---

## Notes

- Tất cả timestamps đều ở UTC format (ISO 8601)
- `draftContent` và `finalContent` có thể là HTML hoặc Markdown
- `data` fields là JSON strings, cần parse khi sử dụng
- Notification sẽ được gửi tự động khi publish (UC-187)
- Session Reports sẽ được đánh dấu `IsMonthlyCompiled = true` sau khi aggregate

