# Lesson Plan Full Flow (API Usage)

Tai lieu nay gom day du flow va API usage cho LessonPlan va LessonPlanTemplate.

## 1. Overview
- LessonPlanTemplate: giao an khung theo Program + Level + SessionIndex.
- LessonPlan: giao an thuc te theo Session (co the gan Template).
- Media/Attachment: upload qua `api/files/*`, sau do luu URL vao `attachment`.

## 2. Roles
- LessonPlan: Teacher chi duoc tao/cap nhat/submit/xoa cho buoi day cua minh (PlannedTeacher/ActualTeacher). ManagementStaff, Admin duoc thao tac tat ca.
- LessonPlanTemplate: Teacher duoc tao/xem. Update/Delete chi danh cho ManagementStaff, Admin.

## 3. Upload Flow (Attachment)
Su dung upload truoc, lay URL tra ve de gan vao `attachment`.

### 3.1 Upload Lesson Plan Template
`POST /api/files/lesson-plan/template`

Form-data:
- `file`: file doc

Response (200)
```json
{
  "url": "https://...",
  "fileName": "template.docx",
  "size": 12345,
  "folder": "lesson-plan-templates",
  "resourceType": "document"
}
```

### 3.2 Upload Lesson Plan Materials
`POST /api/files/lesson-plan/materials`

Form-data:
- `file`: file tai lieu

Response (200)
```json
{
  "url": "https://...",
  "fileName": "materials.pdf",
  "size": 12345,
  "folder": "lesson-plan-materials",
  "resourceType": "document"
}
```

### 3.3 Upload Lesson Plan Media
`POST /api/files/lesson-plan/media?isVideo=false`

Form-data:
- `file`: image/video

Response (200)
```json
{
  "url": "https://...",
  "fileName": "image.png",
  "size": 12345,
  "folder": "lesson-plans/images",
  "resourceType": "image"
}
```

## 4. LessonPlanTemplate API

### 4.1 Create LessonPlanTemplate
`POST /api/lesson-plan-templates`

Request
```json
{
  "programId": "guid",
  "level": "1",
  "title": "Lesson Plan Template 1",
  "sessionIndex": 1,
  "attachment": "https://..."
}
```

Response (201)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "level": "1",
    "title": "Lesson Plan Template 1",
    "sessionIndex": 1,
    "attachment": "https://...",
    "isActive": true,
    "createdAt": "2025-02-01T00:00:00Z"
  }
}
```

### 4.2 List LessonPlanTemplates
`GET /api/lesson-plan-templates?programId=&level=&title=&isActive=&includeDeleted=&pageNumber=1&pageSize=10`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "templates": {
      "items": [
        {
          "id": "guid",
          "programId": "guid",
          "programName": "Program A",
          "title": "Lesson Plan Template 1",
          "level": "1",
          "sessionIndex": 1,
          "attachment": "https://...",
          "isActive": true,
          "createdBy": "guid",
          "createdByName": "Teacher A",
          "createdAt": "2025-02-01T00:00:00Z",
          "usedCount": 3
        }
      ],
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 1,
      "totalPages": 1
    }
  }
}
```

### 4.3 LessonPlanTemplate Detail
`GET /api/lesson-plan-templates/{id}`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "programName": "Program A",
    "level": "1",
    "title": "Lesson Plan Template 1",
    "sessionIndex": 1,
    "attachment": "https://...",
    "isActive": true,
    "createdBy": "guid",
    "createdByName": "Teacher A",
    "createdAt": "2025-02-01T00:00:00Z",
    "usedCount": 3
  }
}
```

### 4.4 Update LessonPlanTemplate
`PUT /api/lesson-plan-templates/{id}`

Request
```json
{
  "level": "2",
  "title": "Lesson Plan Template 1 (update)",
  "sessionIndex": 2,
  "attachment": "https://...",
  "isActive": false
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "title": "Lesson Plan Template 1 (update)",
    "level": "2",
    "sessionIndex": 2,
    "attachment": "https://...",
    "isActive": false
  }
}
```

### 4.5 Delete LessonPlanTemplate (soft delete)
`DELETE /api/lesson-plan-templates/{id}`

Response (200)
```json
{
  "isSuccess": true,
  "data": null
}
```

## 5. LessonPlan API

### 5.1 Create LessonPlan
`POST /api/lesson-plans`

Request
```json
{
  "classId": "guid",
  "sessionId": "guid",
  "templateId": "guid|null",
  "plannedContent": "Planned content",
  "actualContent": "Actual content",
  "actualHomework": "Homework",
  "teacherNotes": "Notes"
}
```

Response (201)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "sessionId": "guid",
    "templateId": "guid",
    "plannedContent": "Planned content",
    "actualContent": "Actual content",
    "actualHomework": "Homework",
    "teacherNotes": "Notes",
    "submittedBy": null,
    "submittedAt": null,
    "createdAt": "2025-02-01T08:00:00Z"
  }
}
```

### 5.2 List LessonPlans
`GET /api/lesson-plans?sessionId=&classId=&templateId=&submittedBy=&fromDate=&toDate=&includeDeleted=&pageNumber=1&pageSize=10`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "lessonPlans": {
      "items": [
        {
          "id": "guid",
          "sessionId": "guid",
          "sessionTitle": "Session 1",
          "sessionDate": "2025-02-01T00:00:00Z",
          "classId": "guid",
          "classCode": "CLASS001",
          "templateId": "guid",
          "templateLevel": "1",
          "templateSessionIndex": 1,
          "plannedContent": "Planned content",
          "actualContent": "Actual content",
          "actualHomework": "Homework",
          "teacherNotes": "Notes",
          "submittedBy": "guid",
          "submittedByName": "Teacher A",
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
}
```

### 5.3 LessonPlan Detail
`GET /api/lesson-plans/{id}`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "CLASS001",
    "sessionId": "guid",
    "sessionTitle": "Session 1",
    "sessionDate": "2025-02-01T00:00:00Z",
    "templateId": "guid",
    "templateLevel": "1",
    "templateSessionIndex": 1,
    "plannedContent": "Planned content",
    "actualContent": "Actual content",
    "actualHomework": "Homework",
    "teacherNotes": "Notes",
    "submittedBy": "guid",
    "submittedByName": "Teacher A",
    "submittedAt": "2025-02-01T10:00:00Z",
    "createdAt": "2025-02-01T08:00:00Z"
  }
}
```

### 5.4 Update LessonPlan
`PUT /api/lesson-plans/{id}`

Request
```json
{
  "templateId": "guid",
  "plannedContent": "Updated planned content",
  "actualContent": "Updated actual content",
  "actualHomework": "Updated homework",
  "teacherNotes": "Updated notes"
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "sessionId": "guid",
    "templateId": "guid",
    "plannedContent": "Updated planned content",
    "actualContent": "Updated actual content",
    "actualHomework": "Updated homework",
    "teacherNotes": "Updated notes"
  }
}
```

### 5.5 Update Actual Content (PATCH) - Submit
`PATCH /api/lesson-plans/{id}/actual`

Request
```json
{
  "actualContent": "Actual content",
  "actualHomework": "Homework",
  "teacherNotes": "Notes"
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "sessionId": "guid",
    "actualContent": "Actual content",
    "actualHomework": "Homework",
    "teacherNotes": "Notes",
    "submittedBy": "guid",
    "submittedByName": "Teacher A",
    "submittedAt": "2025-02-01T11:00:00Z",
    "updatedAt": "2025-02-01T11:00:00Z"
  }
}
```

### 5.6 Get Template From LessonPlan (read-only)
`GET /api/lesson-plans/{id}/template`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "lessonPlanId": "guid",
    "templateId": "guid",
    "templateLevel": "1",
    "templateSessionIndex": 1,
    "templateStructureJson": "{...}",
    "plannedContent": "Planned content",
    "isReadOnly": true
  }
}
```

### 5.7 Delete LessonPlan
`DELETE /api/lesson-plans/{id}`

Response (200)
```json
{
  "isSuccess": true,
  "data": null
}
```

## 6. Notes
- LessonPlanTemplate delete la soft delete.
- `includeDeleted=true` de lay ca ban ghi da xoa.
- Upload file qua `api/files/*` truoc, sau do gan URL vao `attachment`.
- `submittedAt/submittedBy` duoc set khi goi `PATCH /api/lesson-plans/{id}/actual`.
- 1 session = 1 lesson plan (unique theo `SessionId`).
- Neu role la Teacher, ket qua list/detail se chi gioi han theo session ma teacher dang day.
