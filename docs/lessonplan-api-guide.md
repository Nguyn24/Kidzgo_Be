
## 2. LessonPlanTemplate API

### 2.1. Tạo LessonPlanTemplate mới

**Endpoint:**
```
POST /api/lesson-plan-templates
```

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "programId": "guid-required",
  "level": "1",
  "sessionIndex": 1,
  "attachment": "url-to-attachment-file"
}
```

**Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| programId | Guid | ✅ | ID của chương trình học |
| level | string | ✅ | Cấp độ (ví dụ: "1", "2", "3") |
| sessionIndex | int | ✅ | Số thứ tự buổi học trong chương trình |
| attachment | string | ❌ | URL file đính kèm (tài liệu mẫu) |

**Response (201 Created):**
```json
{
  "id": "guid",
  "programId": "guid",
  "programName": "Toán Tiểu Học",
  "level": "1",
  "sessionIndex": 1,
  "attachment": "url-to-attachment",
  "isActive": true,
  "createdBy": "guid",
  "createdByName": "Nguyễn Văn A",
  "createdAt": "2025-02-01T00:00:00Z",
  "usedCount": 0
}
```

---

### 2.2. Lấy danh sách LessonPlanTemplates

**Endpoint:**
```
GET /api/lesson-plan-templates
```

**Query Parameters:**

| Parameter | Type | Required | Mô tả |
|-----------|------|----------|-------|
| programId | Guid | ❌ | Lọc theo chương trình |
| level | string | ❌ | Lọc theo cấp độ |
| isActive | bool | ❌ | Lọc theo trạng thái hoạt động |
| includeDeleted | bool | ❌ | Bao gồm các bản đã xóa (mặc định: false) |
| pageNumber | int | ❌ | Số trang (mặc định: 1) |
| pageSize | int | ❌ | Số item/trang (mặc định: 10) |

**Example Request:**
```http
GET /api/lesson-plan-templates?programId=123e4567-e89b-12d3-a456-426614174000&level=1&isActive=true&pageNumber=1&pageSize=20
```

**Response (200 OK):**
```json
{
  "templates": {
    "items": [
      {
        "id": "guid",
        "programId": "guid",
        "programName": "Toán Tiểu Học",
        "level": "1",
        "sessionIndex": 1,
        "attachment": "url",
        "isActive": true,
        "createdBy": "guid",
        "createdByName": "Nguyễn Văn A",
        "createdAt": "2025-02-01T00:00:00Z",
        "usedCount": 5
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 2.3. Lấy LessonPlanTemplate theo ID

**Endpoint:**
```
GET /api/lesson-plan-templates/{id}
```

**Example Request:**
```http
GET /api/lesson-plan-templates/123e4567-e89b-12d3-a456-426614174000
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "programId": "guid",
  "programName": "Toán Tiểu Học",
  "level": "1",
  "sessionIndex": 1,
  "attachment": "url",
  "isActive": true,
  "createdBy": "guid",
  "createdByName": "Nguyễn Văn A",
  "createdAt": "2025-02-01T00:00:00Z",
  "usedCount": 5
}
```

---

### 2.4. Cập nhật LessonPlanTemplate

**Endpoint:**
```
PUT /api/lesson-plan-templates/{id}
```

**Request Body:**
```json
{
  "level": "2",
  "sessionIndex": 1,
  "attachment": "new-url",
  "isActive": false
}
```

**Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| level | string | ❌ | Cấp độ mới |
| sessionIndex | int | ❌ | Số thứ tự buổi học mới |
| attachment | string | ❌ | URL file đính kèm mới |
| isActive | bool | ❌ | Trạng thái hoạt động |

**Response (200 OK):**
```json
{
  "id": "guid",
  "programId": "guid",
  "programName": "Toán Tiểu Học",
  "level": "2",
  "sessionIndex": 1,
  "attachment": "new-url",
  "isActive": false,
  "createdBy": "guid",
  "createdByName": "Nguyễn Văn A",
  "createdAt": "2025-02-01T00:00:00Z",
  "usedCount": 5
}
```

---

### 2.5. Xóa LessonPlanTemplate (Soft Delete)

**Endpoint:**
```
DELETE /api/lesson-plan-templates/{id}
```

**Response (204 No Content):** Thành công không có nội dung trả về

---

## 3. LessonPlan API

### 3.1. Tạo LessonPlan mới

**Endpoint:**
```
POST /api/lesson-plans
```

**Request Body:**
```json
{
  "sessionId": "guid-required",
  "templateId": "guid-optional",
  "plannedContent": "Nội dung giáo án dự kiến",
  "actualContent": "Nội dung đã dạy thực tế",
  "actualHomework": "Bài tập về nhà",
  "teacherNotes": "Ghi chú của giáo viên"
}
```

**Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sessionId | Guid | ✅ | ID của buổi học (Session) |
| templateId | Guid | ❌ | ID của template (LessonPlanTemplate) |
| plannedContent | string | ❌ | Nội dung dự kiến sẽ dạy |
| actualContent | string | ❌ | Nội dung đã dạy thực tế |
| actualHomework | string | ❌ | Bài tập về nhà |
| teacherNotes | string | ❌ | Ghi chú của giáo viên |

**Response (201 Created):**
```json
{
  "id": "guid",
  "sessionId": "guid",
  "sessionTitle": "Toán - Buổi 1",
  "sessionDate": "2025-02-01",
  "classId": "guid",
  "classCode": "LOP001",
  "templateId": "guid",
  "templateLevel": "1",
  "templateSessionIndex": 1,
  "plannedContent": "Nội dung giáo án dự kiến",
  "actualContent": "Nội dung đã dạy thực tế",
  "actualHomework": "Bài tập về nhà",
  "teacherNotes": "Ghi chú của giáo viên",
  "submittedBy": "guid",
  "submittedByName": "Nguyễn Văn A",
  "submittedAt": "2025-02-01T10:00:00Z",
  "createdAt": "2025-02-01T08:00:00Z"
}
```

---

### 3.2. Lấy danh sách LessonPlans

**Endpoint:**
```
GET /api/lesson-plans
```

**Query Parameters:**

| Parameter | Type | Required | Mô tả |
|-----------|------|----------|-------|
| sessionId | Guid | ❌ | Lọc theo buổi học |
| classId | Guid | ❌ | Lọc theo lớp |
| templateId | Guid | ❌ | Lọc theo template |
| submittedBy | Guid | ❌ | Lọc theo người tạo |
| fromDate | DateTime | ❌ | Từ ngày |
| toDate | DateTime | ❌ | Đến ngày |
| includeDeleted | bool | ❌ | Bao gồm đã xóa |
| pageNumber | int | ❌ | Số trang |
| pageSize | int | ❌ | Số item/trang |

**Example Request:**
```http
GET /api/lesson-plans?classId=123e4567-e89b-12d3-a456-426614174000&fromDate=2025-02-01&toDate=2025-02-28
```

**Response (200 OK):**
```json
{
  "lessonPlans": {
    "items": [
      {
        "id": "guid",
        "sessionId": "guid",
        "sessionTitle": "Toán - Buổi 1",
        "sessionDate": "2025-02-01",
        "classId": "guid",
        "classCode": "LOP001",
        "templateId": "guid",
        "templateLevel": "1",
        "templateSessionIndex": 1,
        "plannedContent": "Nội dung giáo án dự kiến",
        "actualContent": "Nội dung đã dạy thực tế",
        "actualHomework": "Bài tập về nhà",
        "submittedBy": "guid",
        "submittedByName": "Nguyễn Văn A",
        "submittedAt": "2025-02-01T10:00:00Z",
        "createdAt": "2025-02-01T08:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 3.3. Lấy LessonPlan theo ID

**Endpoint:**
```
GET /api/lesson-plans/{id}
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "sessionId": "guid",
  "sessionTitle": "Toán - Buổi 1",
  "sessionDate": "2025-02-01",
  "templateId": "guid",
  "templateLevel": "1",
  "templateSessionIndex": 1,
  "plannedContent": "Nội dung giáo án dự kiến",
  "actualContent": "Nội dung đã dạy thực tế",
  "actualHomework": "Bài tập về nhà",
  "teacherNotes": "Ghi chú của giáo viên",
  "submittedBy": "guid",
  "submittedByName": "Nguyễn Văn A",
  "submittedAt": "2025-02-01T10:00:00Z",
  "createdAt": "2025-02-01T08:00:00Z"
}
```

---

### 3.4. Cập nhật LessonPlan

**Endpoint:**
```
PUT /api/lesson-plans/{id}
```

**Request Body:**
```json
{
  "templateId": "guid",
  "plannedContent": "Nội dung giáo án đã cập nhật",
  "actualContent": "Nội dung đã dạy thực tế",
  "actualHomework": "Bài tập về nhà đã cập nhật",
  "teacherNotes": "Ghi chú mới"
}
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "sessionId": "guid",
  ...
}
```

---

### 3.5. Cập nhật nội dung thực tế (PATCH)

**Endpoint:**
```
PATCH /api/lesson-plans/{id}/actual
```

**Đặc điểm:** API này chỉ cập nhật các trường Actual (nội dung đã dạy thực tế)

**Request Body:**
```json
{
  "actualContent": "Nội dung đã dạy thực tế",
  "actualHomework": "Bài tập về nhà",
  "teacherNotes": "Ghi chú của giáo viên"
}
```

---

### 3.6. Lấy Template từ LessonPlan

**Endpoint:**
```
GET /api/lesson-plans/{id}/template
```

**Mục đích:** Lấy giáo án khung (template) và planned content từ một lesson plan cụ thể - chỉ đọc (read-only)

**Response (200 OK):**
```json
{
  "lessonPlanId": "guid",
  "templateId": "guid",
  "templateLevel": "1",
  "templateSessionIndex": 1,
  "templateStructureJson": "{...}",
  "plannedContent": "Nội dung giáo án dự kiến",
  "isReadOnly": true
}
```

---

### 3.7. Xóa LessonPlan

**Endpoint:**
```
DELETE /api/lesson-plans/{id}
```

**Response (204 No Content):** Thành công

