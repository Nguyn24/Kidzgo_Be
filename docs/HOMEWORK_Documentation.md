# Homework Management Documentation

## Table of Contents
1. [Business Rules](#business-rules)
2. [Roles & Data Scope](#roles--data-scope)
3. [Permissions Matrix](#permissions-matrix)
4. [Status Definitions](#status-definitions)
5. [API Endpoints](#api-endpoints)
6. [Validation Rules](#validation-rules)
7. [Error Codes](#error-codes)

---

## 1. Business Rules

### 1.1 Core Business Rules

| Rule ID | Description |
|---------|-------------|
| BR-001 | Homework assignments can only be created by Teacher, ManagementStaff, or Admin roles |
| BR-002 | Homework is assigned to a specific Class |
| BR-003 | Each student in the class automatically receives a homework record when assignment is created |
| BR-004 | Students can only submit their own homework |
| BR-005 | Teachers can only view/grade homework from classes they teach |
| BR-006 | Homework submission must contain data matching the submission type (Text/File/Image/Link/MULTIPLE_CHOICE) |
| BR-007 | Score cannot exceed MaxScore defined in the assignment |
| BR-008 | Reward stars are granted to students upon successful submission |
| BR-009 | Late submissions are automatically marked by background job |
| BR-010 | Multiple choice homework auto-grades upon submission |
| BR-011 | AI Hint is only available when the homework assignment enables it |
| BR-012 | AI Recommend is only available when the homework assignment enables it |
| BR-013 | AI Speaking Analysis can run from submitted transcript/audio/video without changing homework status |
| BR-014 | AI Speaking Practice can run directly from uploaded audio/video without requiring homework submission |
| BR-015 | AI Quick Grade only persists score/feedback when `aiUsed=true` |

### 1.2 Submission Types

| Type | Description | Required Data |
|------|-------------|---------------|
| `File` | Submit as file attachment | AttachmentUrl |
| `Image` | Submit as image | AttachmentUrl |
| `Text` | Submit as text answer | TextAnswer |
| `Link` | Submit as URL | LinkUrl |
| `MULTIPLE_CHOICE` | Multiple choice quiz | Answers (JSON) |

---

## 2. Roles & Data Scope

### 2.1 Roles in System

| Role | Description |
|------|-------------|
| `Admin` | Full system access |
| `ManagementStaff` | Manage staff, can view all data |
| `Teacher` | Teach specific classes |
| `TeachingAssistant` | Assist teachers, can grade homework |
| `Parent` | View child's homework (via student endpoint) |
| `Student` | Submit and view own homework |

### 2.2 Data Scope by Role

| Role | Data Scope | Description |
|------|------------|-------------|
| **Admin** | All | View, create, edit, delete all homework across all branches |
| **ManagementStaff** | All | View, create, edit, delete all homework across all branches |
| **Teacher** | Own Classes | View, create, edit, delete homework for classes they teach |
| **TeachingAssistant** | Own Classes | View, grade homework for classes they assist |
| **Student** | Own | View own homework, submit homework |
| **Parent** | Child's | View child's homework (via student proxy) |

---

## 3. Permissions Matrix

### 3.1 Teacher/Management/Admin Actions

| Action | Admin | ManagementStaff | Teacher | TeachingAssistant |
|--------|-------|-----------------|---------|-------------------|
| Create Homework | ✓ | ✓ | ✓ | ✗ |
| View All Homework | ✓ | ✓ | Own Classes | Own Classes |
| View Homework Detail | ✓ | ✓ | Own Classes | Own Classes |
| Update Homework | ✓ | ✓ | Own Classes | ✗ |
| Delete Homework | ✓ | ✓ | Own Classes | ✗ |
| Link to Mission | ✓ | ✓ | Own Classes | ✗ |
| Set Reward Stars | ✓ | ✓ | Own Classes | ✗ |
| View Submissions | ✓ | ✓ | Own Classes | Own Classes |
| View Submission Detail | ✓ | ✓ | Own Classes | Own Classes |
| Grade Homework | ✓ | ✓ | Own Classes | ✓ |
| Mark Late/Missing | ✓ | ✓ | Own Classes | ✓ |
| View Student History | ✓ | ✓ | Own Classes | Own Classes |

### 3.2 Student Actions

| Action | Student | Description |
|--------|---------|-------------|
| View My Homework | ✓ | View assigned homework |
| View Submitted Homework | ✓ | View submitted/graded homework |
| View Homework Detail | ✓ | View submission details |
| Submit Homework | ✓ | Submit homework answer |
| Submit Multiple Choice | ✓ | Submit quiz answers |
| View My Feedback | ✓ | View grades and feedback |
| Get AI Hint | ✓ | Request AI hint for current homework |
| Get AI Recommendations | ✓ | Request follow-up practice suggestions |
| Get AI Speaking Analysis | ✓ | Analyze submitted speaking answer |
| Get AI Speaking Practice | ✓ | Upload audio/video directly for instant speaking feedback |

---

## 4. Status Definitions

### 4.1 Homework Status List

| Status | Code | Description |
|--------|------|-------------|
| `Assigned` | 0 | Homework assigned to student, not yet submitted |
| `Submitted` | 1 | Student submitted homework, pending grading |
| `Graded` | 2 | Teacher graded and returned homework |
| `Late` | 3 | Submitted after due date |
| `Missing` | 4 | Not submitted and past due date |

### 4.2 Status Transition Flow

```
Assigned → Submitted (when student submits)
Submitted → Graded (when teacher grades)
Submitted → Late (when past due and manually marked)
Assigned → Missing (when past due and manually marked)
Any → Graded (re-grade allowed)
```

*AI Hint, AI Recommend, AI Speaking Analysis, and AI Speaking Practice do not change homework status.*
*AI Quick Grade keeps status unchanged when `aiUsed=false`, and moves submission to `Graded` when `aiUsed=true`.*

### 4.3 Allowed Status Transitions

| From | To | Condition |
|------|----|-----------|
| Assigned | Submitted | Student submits |
| Assigned | Missing | Past due date, manually marked |
| Submitted | Graded | Teacher grades |
| Submitted | Late | Past due date, manually marked |
| Graded | Graded | Teacher re-grades |

---

## 5. API Endpoints

### 5.1 Homework Assignment Management

#### 5.1.1 Create Homework Assignment

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/homework` |
| **Method** | POST |
| **Roles** | Teacher, ManagementStaff, Admin |
| **Scope** | Own Classes |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `ClassId` | GUID | Yes | Target class ID |
| `SessionId` | GUID | No | Related session |
| `Title` | string | Yes | Homework title |
| `Description` | string | No | Homework description |
| `DueAt` | DateTime | No | Due date/time (UTC) |
| `Book` | string | No | Related book |
| `Pages` | string | No | Page numbers |
| `Skills` | string | No | Related skills |
| `SubmissionType` | string | Yes | File/Image/Text/Link/MULTIPLE_CHOICE |
| `MaxScore` | decimal | No | Maximum score |
| `RewardStars` | int | No | Stars reward (>=0) |
| `TimeLimitMinutes` | int | No | Quiz time limit (minutes) |
| `AllowResubmit` | bool | No | Allow student resubmit quiz |
| `MissionId` | GUID | No | Linked mission |
| `Instructions` | string | No | Instructions for students |
| `ExpectedAnswer` | string | No | Expected answer (for grading) |
| `Rubric` | string | No | Grading rubric |
| `Attachment` | string | No | Attachment URL |

**Response Success (201 Created):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "title": "string",
    "description": "string",
    "dueAt": "2025-01-15T23:59:00Z",
    "submissionType": "Text",
    "maxScore": 10.0,
    "rewardStars": 5,
    "timeLimitMinutes": null,
    "allowResubmit": false,
    "createdAt": "2025-01-10T10:00:00Z"
  },
  "message": "Homework assignment created successfully"
}
```

**Response Error (400):**

```json
{
  "success": false,
  "error": {
    "code": "Validation error",
    "message": "Title is required"
  }
}
```

---

#### 5.1.2 Create Multiple Choice Homework

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/homework/multiple-choice` |
| **Method** | POST |
| **Roles** | Teacher, ManagementStaff, Admin |
| **Scope** | Own Classes |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `ClassId` | GUID | Yes | Target class ID |
| `SessionId` | GUID | No | Related session |
| `Title` | string | Yes | Homework title |
| `Description` | string | No | Homework description |
| `DueAt` | DateTime | No | Due date/time |
| `RewardStars` | int | No | Stars reward |
| `TimeLimitMinutes` | int | No | Quiz time limit (minutes) |
| `AllowResubmit` | bool | No | Allow student resubmit quiz |
| `MissionId` | GUID | No | Linked mission |
| `Instructions` | string | No | Instructions |
| `Questions` | List | Yes | Questions array |

**Questions Array:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `QuestionText` | string | Yes | Question content |
| `QuestionType` | string | Yes | MultipleChoice/TextInput |
| `Options` | List<string> | Yes* | Options for MC (*if MultipleChoice) |
| `CorrectAnswer` | string | Yes | Correct answer |
| `Points` | int | Yes | Point value |
| `Explanation` | string | No | Answer explanation |

**Response Success (201):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "string",
    "questions": [
      {
        "id": "guid",
        "questionText": "string",
        "questionType": "MultipleChoice",
        "points": 10
      }
    ],
    "maxScore": 100,
    "timeLimitMinutes": 30,
    "allowResubmit": false,
    "createdAt": "2025-01-10T10:00:00Z"
  },
  "message": "Multiple choice homework created successfully"
}
```

---

#### 5.1.3 Get Homework Assignments List

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/homework` |
| **Method** | GET |
| **Roles** | Teacher, ManagementStaff, Admin |
| **Scope** | Own Classes / All |

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `classId` | GUID | No | Filter by class |
| `sessionId` | GUID | No | Filter by session |
| `skill` | string | No | Filter by skill |
| `submissionType` | string | No | Filter by type |
| `branchId` | GUID | No | Filter by branch (Admin/Management only) |
| `fromDate` | DateTime | No | Filter from date |
| `toDate` | DateTime | No | Filter to date |
| `pageNumber` | int | No | Page number (default: 1) |
| `pageSize` | int | No | Page size (default: 10) |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "classId": "guid",
        "className": "Class 1A",
        "title": "Math Homework 1",
        "description": "Chapter 1 exercises",
        "dueAt": "2025-01-15T23:59:00Z",
        "submissionType": "Text",
        "maxScore": 10.0,
        "rewardStars": 5,
        "createdAt": "2025-01-10T10:00:00Z",
        "submissionCount": 20,
        "gradedCount": 15
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5
  }
}
```

---

#### 5.1.4 Get Homework Assignment By ID

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/homework/{id}` |
| **Method** | GET |
| **Roles** | Teacher, ManagementStaff, Admin |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "className": "Class 1A",
    "sessionId": "guid",
    "sessionName": "Spring 2025",
    "title": "Math Homework 1",
    "description": "Chapter 1 exercises",
    "dueAt": "2025-01-15T23:59:00Z",
    "book": "Math Grade 1",
    "pages": "10-15",
    "skills": "Addition, Subtraction",
    "submissionType": "Text",
    "maxScore": 10.0,
    "rewardStars": 5,
    "missionId": "guid",
    "instructions": "Complete all exercises",
    "expectedAnswer": "1. 10\n2. 15",
    "rubric": "Each correct answer: 2 points",
    "attachmentUrl": "https://...",
    "createdBy": "guid",
    "createdAt": "2025-01-10T10:00:00Z",
    "questions": [],
    "submissionCount": 20,
    "gradedCount": 15,
    "averageScore": 8.5
  }
}
```

---

#### 5.1.5 Update Homework Assignment

| Item | Details |
|------|---------|
| **Endpoint** | `PUT /api/homework/{id}` |
| **Method** | PUT |
| **Roles** | Teacher, ManagementStaff, Admin |

**Request Body:** (Same as Create, all fields optional)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Title` | string | No | Homework title |
| `Description` | string | No | Description |
| `DueAt` | DateTime | No | Due date |
| `Book` | string | No | Book reference |
| `Pages` | string | No | Page numbers |
| `Skills` | string | No | Skills |
| `SubmissionType` | string | No | Submission type |
| `MaxScore` | decimal | No | Max score |
| `RewardStars` | int | No | Stars |
| `MissionId` | GUID | No | Mission ID |
| `Instructions` | string | No | Instructions |
| `ExpectedAnswer` | string | No | Expected answer |
| `Rubric` | string | No | Rubric |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "Updated Title",
    "updatedAt": "2025-01-12T10:00:00Z"
  },
  "message": "Homework assignment updated successfully"
}
```

**Response Error (400):**

```json
{
  "success": false,
  "error": {
    "code": "Homework.CannotUpdate",
    "message": "Cannot update homework assignment that has submitted or graded submissions"
  }
}
```

---

#### 5.1.6 Delete Homework Assignment

| Item | Details |
|------|---------|
| **Endpoint** | `DELETE /api/homework/{id}` |
| **Method** | DELETE |
| **Roles** | Teacher, ManagementStaff, Admin |

**Response Success (200):**

```json
{
  "success": true,
  "message": "Homework assignment deleted successfully"
}
```

---

#### 5.1.7 Link Homework to Mission

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/homework/{id}/link-mission` |
| **Method** | POST |
| **Roles** | Teacher, ManagementStaff, Admin |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `MissionId` | GUID | Yes | Mission ID to link |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "homeworkId": "guid",
    "missionId": "guid"
  },
  "message": "Homework linked to mission successfully"
}
```

---

#### 5.1.8 Set Homework Reward Stars

| Item | Details |
|------|---------|
| **Endpoint** | `PUT /api/homework/{id}/reward-stars` |
| **Method** | PUT |
| **Roles** | Teacher, ManagementStaff, Admin |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `RewardStars` | int | Yes | Number of stars (>=0) |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "homeworkId": "guid",
    "rewardStars": 10
  },
  "message": "Reward stars updated successfully"
}
```

---

### 5.2 Homework Submission Management (Teacher)

#### 5.2.1 Get Homework Submissions List

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/homework/submissions` |
| **Method** | GET |
| **Roles** | Teacher, ManagementStaff, Admin |
| **Scope** | Own Classes |

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `classId` | GUID | No | Filter by class |
| `status` | string | No | Assigned/Submitted/Graded/Late/Missing |
| `pageNumber` | int | No | Page number |
| `pageSize` | int | No | Page size |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "homeworkId": "guid",
        "homeworkTitle": "Math Homework 1",
        "studentProfileId": "guid",
        "studentName": "John Doe",
        "classId": "guid",
        "className": "Class 1A",
        "status": "Submitted",
        "submittedAt": "2025-01-14T10:00:00Z",
        "gradedAt": null,
        "score": null,
        "teacherFeedback": null
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5
  }
}
```

---

#### 5.2.2 Get Homework Submission Detail

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/homework/submissions/{homeworkStudentId}` |
| **Method** | GET |
| **Roles** | Teacher, ManagementStaff, Admin |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "homeworkId": "guid",
    "homeworkTitle": "Math Homework 1",
    "studentProfileId": "guid",
    "studentName": "John Doe",
    "studentEmail": "john@example.com",
    "classId": "guid",
    "className": "Class 1A",
    "status": "Submitted",
    "submittedAt": "2025-01-14T10:00:00Z",
    "gradedAt": null,
    "score": null,
    "maxScore": 10.0,
    "teacherFeedback": null,
    "aiFeedback": null,
    "textAnswer": "My answer text...",
    "attachmentUrls": ["https://..."],
    "linkUrl": null,
    "dueAt": "2025-01-15T23:59:00Z",
    "isLate": false
  }
}
```

---

#### 5.2.3 Grade Homework

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/homework/submissions/{homeworkStudentId}/grade` |
| **Method** | POST |
| **Roles** | Teacher, TeachingAssistant, ManagementStaff, Admin |
| **Scope** | Own Classes |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Score` | decimal | Yes | Score (>=0, <=maxScore) |
| `TeacherFeedback` | string | No | Feedback for student |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "homeworkStudentId": "guid",
    "status": "Graded",
    "score": 8.5,
    "teacherFeedback": "Great work!",
    "gradedAt": "2025-01-16T10:00:00Z"
  },
  "message": "Homework graded successfully"
}
```

---

#### 5.2.3a AI Quick Grade Homework

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/homework/submissions/{homeworkStudentId}/quick-grade` |
| **Method** | POST |
| **Roles** | Teacher, TeachingAssistant, ManagementStaff, Admin |
| **Scope** | Own Classes |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Language` | string | No | Default `vi` |
| `Instructions` | string | No | Override instructions sent to AI |
| `Rubric` | string | No | Override rubric sent to AI |
| `ExpectedAnswerText` | string | No | Override expected answer / expected speaking text |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "assignmentId": "guid",
    "isSpeakingAnalysis": true,
    "aiUsed": true,
    "persisted": true,
    "status": "Graded",
    "score": 8.5,
    "rawAiScore": 8.5,
    "rawAiMaxScore": 10,
    "summary": "Good pronunciation overall",
    "strengths": ["Clear ending sounds"],
    "issues": ["Word stress is inconsistent"],
    "suggestions": ["Slow down on long words"],
    "stars": 4,
    "pronunciationScore": 8.5,
    "fluencyScore": 7.5,
    "accuracyScore": 8.0,
    "mispronouncedWords": ["family"],
    "wordFeedback": [
      {
        "word": "family",
        "heardAs": "famly",
        "issue": "Missing middle vowel",
        "tip": "Say fa-mi-ly in three beats"
      }
    ],
    "practicePlan": ["Repeat target words 3 times"],
    "warnings": [],
    "gradedAt": "2025-01-16T10:00:00Z"
  }
}
```

**Notes:**

- If the assignment has `SpeakingMode`, the backend routes to A8 speaking analysis.
- Otherwise the backend routes to A3 grading.
- If `aiUsed=false`, the API still returns `200`, but score/status/aiFeedback are not persisted.

---

#### 5.2.4 Mark Homework Late or Missing

| Item | Details |
|------|---------|
| **Endpoint** | `PUT /api/homework/submissions/{homeworkStudentId}/mark-status` |
| **Method** | PUT |
| **Roles** | Teacher, TeachingAssistant, ManagementStaff, Admin |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Status` | string | Yes | Late or Missing |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "homeworkStudentId": "guid",
    "status": "Late",
    "updatedAt": "2025-01-16T10:00:00Z"
  },
  "message": "Homework status updated successfully"
}
```

---

#### 5.2.5 Get Student Homework History

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/homework/students/{studentProfileId}/history` |
| **Method** | GET |
| **Roles** | Teacher, TeachingAssistant, ManagementStaff, Admin |

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `classId` | GUID | No | Filter by class |
| `pageNumber` | int | No | Page number |
| `pageSize` | int | No | Page size |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "studentProfileId": "guid",
    "studentName": "John Doe",
    "items": [
      {
        "homeworkId": "guid",
        "homeworkTitle": "Math Homework 1",
        "className": "Class 1A",
        "status": "Graded",
        "dueAt": "2025-01-15T23:59:00Z",
        "submittedAt": "2025-01-14T10:00:00Z",
        "gradedAt": "2025-01-16T10:00:00Z",
        "score": 8.5,
        "maxScore": 10.0
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50
  }
}
```

---

### 5.3 Student Homework (Student Portal)

#### 5.3.1 Get My Homework List

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/students/homework/my` |
| **Method** | GET |
| **Roles** | Student |

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `status` | string | No | Filter by status |
| `classId` | GUID | No | Filter by class |
| `pageNumber` | int | No | Page number |
| `pageSize` | int | No | Page size |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "homeworkId": "guid",
        "homeworkTitle": "Math Homework 1",
        "classId": "guid",
        "className": "Class 1A",
        "status": "Assigned",
        "dueAt": "2025-01-15T23:59:00Z",
        "submissionType": "Text",
        "maxScore": 10.0,
        "rewardStars": 5,
        "instructions": "Complete all exercises",
        "attachmentUrl": null,
        "isLate": false
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 20,
    "totalPages": 2
  }
}
```

---

#### 5.3.2 Get My Submitted Homework

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/students/homework/submitted` |
| **Method** | GET |
| **Roles** | Student |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "homeworkId": "guid",
        "homeworkTitle": "Math Homework 1",
        "className": "Class 1A",
        "status": "Graded",
        "dueAt": "2025-01-15T23:59:00Z",
        "submittedAt": "2025-01-14T10:00:00Z",
        "gradedAt": "2025-01-16T10:00:00Z",
        "score": 8.5,
        "maxScore": 10.0,
        "teacherFeedback": "Great work!"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 15
  }
}
```

---

#### 5.3.3 Get My Homework Submission Detail

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/students/homework/{homeworkStudentId}` |
| **Method** | GET |
| **Roles** | Student |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "homeworkStudentId": "guid",
    "assignmentId": "guid",
    "assignmentTitle": "Math Homework 1",
    "classTitle": "Class 1A",
    "status": "Graded",
    "dueAt": "2025-01-15T23:59:00Z",
    "submissionType": "MULTIPLE_CHOICE",
    "aiHintEnabled": true,
    "aiRecommendEnabled": true,
    "speakingMode": "speaking",
    "targetWords": ["hello", "family"],
    "speakingExpectedText": "Hello, my name is Rex.",
    "timeLimitMinutes": 30,
    "allowResubmit": false,
    "startedAt": "2025-01-14T09:30:00Z",
    "submittedAt": "2025-01-14T10:00:00Z",
    "gradedAt": "2025-01-16T10:00:00Z",
    "score": 8.5,
    "maxScore": 10.0,
    "teacherFeedback": "Great work!",
    "aiFeedback": "Well done!",
    "textAnswer": "My answer...",
    "attachmentUrls": ["https://..."],
    "linkUrl": null,
    "questions": [
      {
        "id": "guid",
        "questionText": "2 + 2 = ?",
        "questionType": "MultipleChoice",
        "points": 1,
        "options": [
          { "id": "guid", "text": "1", "orderIndex": 0 },
          { "id": "guid", "text": "2", "orderIndex": 1 },
          { "id": "guid", "text": "3", "orderIndex": 2 },
          { "id": "guid", "text": "4", "orderIndex": 3 }
        ]
      }
    ],
    "review": {
      "answerResults": [
        {
          "questionId": "guid",
          "questionText": "2 + 2 = ?",
          "selectedOptionId": "guid",
          "selectedOptionText": "4",
          "correctOptionId": "guid",
          "correctOptionText": "4",
          "isCorrect": true,
          "earnedPoints": 1,
          "maxPoints": 1,
          "explanation": "Basic addition"
        }
      ]
    },
    "showReview": true,
    "showCorrectAnswer": true,
    "showExplanation": true,
    "isLate": false,
    "rewardStars": 5
  }
}
```

*Note: `questions` chi tra ve khi `submissionType = MULTIPLE_CHOICE` va khong bao gom `correctAnswer`.*
*Note: `startedAt` duoc set lan dau khi hoc sinh mo bai (GET detail). Neu co `timeLimitMinutes`, BE se reject khi nop qua han.*

---

#### 5.3.4 Submit Homework

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/students/homework/submit` |
| **Method** | POST |
| **Roles** | Student |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `HomeworkStudentId` | GUID | Yes | Homework student record ID |
| `TextAnswer` | string | No* | Text answer (*if Text type) |
| `AttachmentUrls` | List<string> | No* | Attachment URLs (*if File/Image type) |
| `LinkUrl` | string | No* | Link URL (*if Link type) |

*Note: Required data depends on SubmissionType*

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "assignmentId": "guid",
    "status": "Submitted",
    "submittedAt": "2025-01-14T10:00:00Z"
  },
  "message": "Homework submitted successfully"
}
```

*Note: reward stars duoc cong neu nop dung han, nhung khong tra ve trong response.*
*Note: Bai nop file/image can upload truoc qua `POST /api/files/upload` va dung URL tra ve trong `attachmentUrls`.*

---

#### 5.3.5 Submit Multiple Choice Homework

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/students/homework/multiple-choice/submit` |
| **Method** | POST |
| **Roles** | Student |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `HomeworkStudentId` | GUID | Yes | Homework student record ID |
| `Answers` | List | Yes | Answers array |

**Answers Array:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `QuestionId` | GUID | Yes | Question ID |
| `SelectedOptionId` | GUID | No | Selected option ID (null nếu bỏ qua) |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "assignmentId": "guid",
    "status": "Graded",
    "submittedAt": "2025-01-14T10:00:00Z",
    "gradedAt": "2025-01-14T10:00:00Z",
    "score": 8.5,
    "maxScore": 10.0,
    "rewardStars": 5,
    "correctCount": 8,
    "wrongCount": 1,
    "skippedCount": 1,
    "totalCount": 10,
    "totalPoints": 10,
    "earnedPoints": 8,
    "answerResults": [
      {
        "questionId": "guid",
        "questionText": "What is 2+2?",
        "selectedOptionId": "guid",
        "selectedOptionText": "4",
        "correctOptionId": "guid",
        "correctOptionText": "4",
        "isCorrect": true,
        "earnedPoints": 1,
        "maxPoints": 1,
        "explanation": "Basic addition"
      }
    ]
  },
  "message": "Multiple choice homework submitted and graded successfully"
}
```

*Note: Chua ho tro `timeLimitMinutes/startedAt/allowResubmit` cho quiz; can bo sung neu co yeu cau rang buoc lam bai.*

---

#### 5.3.6 Get My Homework Feedback

| Item | Details |
|------|---------|
| **Endpoint** | `GET /api/students/homework/feedback/my` |
| **Method** | GET |
| **Roles** | Student |

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `classId` | GUID | No | Filter by class |
| `pageNumber` | int | No | Page number |
| `pageSize` | int | No | Page size |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "homeworkStudentId": "guid",
        "homeworkTitle": "Math Homework 1",
        "className": "Class 1A",
        "score": 8.5,
        "maxScore": 10.0,
        "teacherFeedback": "Great work!",
        "gradedAt": "2025-01-16T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 15
  }
}
```

---

#### 5.3.7 Get AI Hint

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/students/homework/{homeworkStudentId}/hint` |
| **Method** | POST |
| **Roles** | Student |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `CurrentAnswerText` | string | No | Optional latest answer text from UI |
| `Language` | string | No | Default `vi` |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "aiUsed": true,
    "summary": "Focus on present simple structure",
    "hints": ["Check the verb after he/she/it"],
    "grammarFocus": ["Present simple"],
    "vocabularyFocus": ["family members"],
    "encouragement": "You are close. Try one more time.",
    "warnings": []
  }
}
```

**Notes:**

- This endpoint does not change homework status, score, or feedback in DB.
- If `CurrentAnswerText` is empty, backend falls back to stored `TextAnswer`.

---

#### 5.3.8 Get AI Recommendations

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/students/homework/{homeworkStudentId}/recommendations` |
| **Method** | POST |
| **Roles** | Student |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `CurrentAnswerText` | string | No | Optional latest answer text from UI |
| `Language` | string | No | Default `vi` |
| `MaxItems` | int | No | Recommended 1..10, default 5 |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "aiUsed": true,
    "summary": "Practice more present simple sentence building",
    "focusSkill": "grammar",
    "topics": ["family"],
    "grammarTags": ["present simple"],
    "vocabularyTags": ["family members"],
    "recommendedLevels": ["Easy", "Medium"],
    "practiceTypes": ["text_input"],
    "warnings": [],
    "items": [
      {
        "questionBankItemId": "guid",
        "questionText": "My mother ____ a doctor.",
        "questionType": "TextInput",
        "options": [],
        "topic": "family",
        "skill": "grammar",
        "grammarTags": ["present simple"],
        "vocabularyTags": ["jobs"],
        "level": "Easy",
        "points": 1,
        "reason": "Matches the same grammar pattern"
      }
    ]
  }
}
```

---

#### 5.3.9 Get AI Speaking Analysis

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/students/homework/{homeworkStudentId}/speaking-analysis` |
| **Method** | POST |
| **Roles** | Student |

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `CurrentTranscript` | string | No | Optional transcript from UI |
| `Language` | string | No | Default `vi` |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "aiUsed": true,
    "summary": "Clear overall reading with a few weak words",
    "transcript": "Hello my name is Rex",
    "overallScore": 8.2,
    "pronunciationScore": 8.4,
    "fluencyScore": 7.8,
    "accuracyScore": 8.3,
    "stars": 4,
    "strengths": ["Good pacing"],
    "issues": ["Word stress on 'family'"],
    "mispronouncedWords": ["family"],
    "wordFeedback": [
      {
        "word": "family",
        "heardAs": "famly",
        "issue": "Missing middle vowel",
        "tip": "Say fa-mi-ly in three beats"
      }
    ],
    "suggestions": ["Repeat difficult words slowly"],
    "practicePlan": ["Read the sentence 3 more times"],
    "confidence": {
      "overall": 0.88
    },
    "warnings": []
  }
}
```

**Notes:**

- If `CurrentTranscript` is provided, backend prioritizes transcript analysis.
- If transcript is empty but the submission already has an audio/video attachment, backend analyzes the attachment instead.
- This endpoint does not persist score or AI feedback.

---

#### 5.3.10 Get AI Speaking Practice (Instant, no submit required)

| Item | Details |
|------|---------|
| **Endpoint** | `POST /api/students/ai-speaking/analyze` |
| **Method** | POST |
| **Roles** | Student |
| **Content-Type** | `multipart/form-data` |

**Form Data:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `File` | binary | Yes | Audio/video file to analyze immediately |
| `HomeworkStudentId` | GUID | No | Optional homework context; does not submit the homework |
| `Language` | string | No | Default `vi` |
| `Mode` | string | No | Example: `speaking`, `phonics` |
| `ExpectedText` | string | No | Expected reading text |
| `TargetWords` | string | No | Comma-separated target words |
| `Instructions` | string | No | Extra speaking instructions |

**Response Success (200):**

```json
{
  "success": true,
  "data": {
    "aiUsed": true,
    "summary": "Good practice attempt with clear consonants",
    "transcript": "Hello my name is Rex",
    "overallScore": 8.0,
    "pronunciationScore": 8.1,
    "fluencyScore": 7.7,
    "accuracyScore": 8.0,
    "stars": 4,
    "strengths": ["Clear ending sounds"],
    "issues": ["Word stress needs work"],
    "mispronouncedWords": ["family"],
    "wordFeedback": [
      {
        "word": "family",
        "heardAs": "famly",
        "issue": "Missing middle vowel",
        "tip": "Say fa-mi-ly in three beats"
      }
    ],
    "suggestions": ["Read slower on long words"],
    "practicePlan": ["Repeat target words 3 times"],
    "warnings": []
  }
}
```

**Swagger / form-data example:**

| Field | Example |
|-------|---------|
| `File` | `student-reading.webm` |
| `HomeworkStudentId` | `7d7c38d4-7d8d-4f49-92f3-4d2b70f4ef11` |
| `Language` | `vi` |
| `Mode` | `speaking` |
| `ExpectedText` | `Hello, my name is Rex.` |
| `TargetWords` | `hello, name, rex` |
| `Instructions` | `Read slowly and clearly.` |

**cURL example:**

```bash
curl -X POST "https://your-domain/api/students/ai-speaking/analyze" \
  -H "Authorization: Bearer <token>" \
  -F "file=@student-reading.webm" \
  -F "language=vi" \
  -F "mode=speaking" \
  -F "expectedText=Hello, my name is Rex." \
  -F "targetWords=hello, name, rex" \
  -F "instructions=Read slowly and clearly."
```

**Notes:**

- This endpoint is for instant speaking practice and does not require `submit homework`.
- If `HomeworkStudentId` is provided, backend borrows homework context but still does not submit or grade the homework.
- If the uploaded file is not audio/video, backend may return `200` with `aiUsed=false` and `warnings[]`.

---

## 6. Validation Rules

### 6.1 Create Homework Assignment

| Field | Rule | Error Message |
|-------|------|---------------|
| `ClassId` | Required, must be valid GUID | Class ID is required |
| `Title` | Required, not empty | Title is required |
| `DueAt` | If provided, must be >= now | Due date should not be in the past |
| `SubmissionType` | Required, must be valid enum | Invalid submission type |
| `MaxScore` | If provided, must be > 0 | MaxScore must be greater than 0 |
| `RewardStars` | If provided, must be >= 0 | RewardStars must be greater than or equal to 0 |
| `TimeLimitMinutes` | If provided, must be > 0 | TimeLimitMinutes must be greater than 0 |

### 6.2 Submit Homework

| Field | Rule | Error Message |
|-------|------|---------------|
| `HomeworkStudentId` | Required | Homework student ID is required |
| `TextAnswer` | Required if SubmissionType=Text | Submission data is required for Text submission type |
| `AttachmentUrls` | Required if SubmissionType=File/Image | Submission data is required for File/Image submission type |
| `LinkUrl` | Required if SubmissionType=Link | Submission data is required for Link submission type |

### 6.3 Grade Homework

| Field | Rule | Error Message |
|-------|------|---------------|
| `Score` | Required, >= 0 | Score cannot be negative |
| `Score` | Must not exceed MaxScore | Score cannot exceed maximum score of {maxScore} |
| Status | Must be Submitted | Can only grade homework that has been submitted |

### 6.4 Mark Late/Missing

| Field | Rule | Error Message |
|-------|------|---------------|
| `Status` | Must be "Late" or "Missing" | Status must be either 'LATE' or 'MISSING' |

### 6.5 Multiple Choice Homework

| Field | Rule | Error Message |
|-------|------|---------------|
| `Questions` | Required, at least 1 | At least one question is required |
| `QuestionText` | Required | Question {n} text cannot be empty |
| `Options` | Required for MC, at least 2 | Question {n} must have at least 2 options |
| `CorrectAnswer` | Must be valid index/text | Question {n} has invalid correct answer index |
| `Points` | Must be > 0 | Question {n} points must be greater than 0 |

### 6.6 AI Homework Endpoints

| Endpoint / Field | Rule | Error Message |
|------------------|------|---------------|
| `POST /api/students/homework/{homeworkStudentId}/hint` | Homework must enable AI Hint | AI hint is not enabled for this homework |
| `POST /api/students/homework/{homeworkStudentId}/recommendations` | Homework must enable AI Recommend | AI recommendation is not enabled for this homework |
| `POST /api/students/homework/{homeworkStudentId}/speaking-analysis` | Homework must have `SpeakingMode` | AI speaking analysis is not available for this homework |
| `POST /api/students/homework/{homeworkStudentId}/speaking-analysis` | Must have transcript or attachment | Submission data is required for speaking analysis |
| `POST /api/students/ai-speaking/analyze` | `File` is required | An audio or video file is required for instant AI speaking analysis |
| `POST /api/homework/submissions/{homeworkStudentId}/quick-grade` | Submission must be `Submitted` or `Graded` | Can only grade homework that has been submitted |
| `POST /api/homework/submissions/{homeworkStudentId}/quick-grade` | Submission must have text or attachment | Submission data is required for grading |

---

## 7. Error Codes

### 7.1 Homework Assignment Errors

| Code | Message | HTTP Status |
|------|---------|--------------|
| `Homework.NotFound` | Homework assignment not found | 404 |
| `Homework.ClassNotFound` | Class not found or inactive | 404 |
| `Homework.SessionNotFound` | Session not found | 404 |
| `Homework.MissionNotFound` | Mission not found | 404 |
| `Homework.InvalidDueDate` | Due date must be in the future | 400 |
| `Homework.CannotUpdate` | Cannot update with submissions | 400 |
| `Homework.Unauthorized` | No permission | 403 |
| `Homework.ClassHasNoActiveStudents` | Class has no students | 400 |
| `Homework.InvalidTitle` | Title cannot be empty | 400 |
| `Homework.InvalidMaxScore` | MaxScore must be > 0 | 400 |
| `Homework.InvalidRewardStars` | RewardStars must be >= 0 | 400 |
| `Homework.InvalidTimeLimitMinutes` | TimeLimitMinutes must be > 0 | 400 |
| `Homework.InvalidSubmissionType` | Invalid submission type | 400 |
| `Homework.InvalidStatusForMarking` | Status must be LATE or MISSING | 400 |

### 7.2 Homework Submission Errors

| Code | Message | HTTP Status |
|------|---------|--------------|
| `HomeworkSubmission.NotFound` | Submission not found | 404 |
| `HomeworkSubmission.InvalidScore` | Score cannot be negative | 400 |
| `HomeworkSubmission.ScoreExceedsMax` | Score exceeds max | 400 |
| `HomeworkSubmission.InvalidStatus` | Invalid status | 400 |
| `HomeworkSubmission.InvalidStatusTransition` | Invalid transition | 400 |
| `HomeworkSubmission.Unauthorized` | No permission | 403 |
| `HomeworkSubmission.AlreadySubmitted` | Already submitted | 400 |
| `HomeworkSubmission.InvalidData` | Invalid submission data | 400 |
| `HomeworkSubmission.CannotSubmitMissing` | Cannot submit MISSING | 400 |
| `HomeworkSubmission.NotSubmitted` | Not yet submitted | 400 |
| `HomeworkSubmission.CannotSubmitMultipleChoice` | Wrong endpoint | 400 |
| `HomeworkSubmission.NoAnswersProvided` | No answers | 400 |
| `HomeworkSubmission.TimeExpired` | Time limit expired | 400 |
| `HomeworkSubmission.QuestionNotFound` | Question not found | 404 |

### 7.3 AI Homework Errors

| Code | Message | HTTP Status |
|------|---------|--------------|
| `Homework.AiHintNotEnabled` | AI hint is not enabled for this homework | 400 |
| `Homework.AiRecommendNotEnabled` | AI recommendation is not enabled for this homework | 400 |
| `Homework.AiSpeakingNotAvailable` | AI speaking analysis is not available for this homework | 400 |
| `Homework.AiSpeakingPracticeFileRequired` | Audio/video file is required for instant practice | 400 |

### 7.4 Multiple Choice Errors

| Code | Message | HTTP Status |
|------|---------|--------------|
| `Homework.NoQuestionsProvided` | No questions | 400 |
| `Homework.InvalidQuestionText` | Question text empty | 400 |
| `Homework.InsufficientOptions` | Not enough options | 400 |
| `Homework.InvalidCorrectAnswer` | Invalid answer | 400 |
| `Homework.InvalidPoints` | Invalid points | 400 |

---

## 8. Appendix

### 8.1 Data Models

#### HomeworkAssignment
```
- Id: GUID
- ClassId: GUID
- SessionId: GUID?
- Title: string
- Description: string?
- DueAt: DateTime?
- Book: string?
- Pages: string?
- Skills: string?
- SubmissionType: SubmissionType (enum)
- MaxScore: decimal?
- RewardStars: int?
- MissionId: GUID?
- Instructions: string?
- ExpectedAnswer: string?
- Rubric: string?
- AttachmentUrl: string?
- CreatedBy: GUID?
- CreatedAt: DateTime
```

#### HomeworkStudent
```
- Id: GUID
- AssignmentId: GUID
- StudentProfileId: GUID
- Status: HomeworkStatus (enum)
- SubmittedAt: DateTime?
- GradedAt: DateTime?
- Score: decimal?
- TeacherFeedback: string?
- AiFeedback: string?
- TextAnswer: string?
- AttachmentUrl: string?
```

#### HomeworkQuestion
```
- Id: GUID
- HomeworkAssignmentId: GUID
- OrderIndex: int
- QuestionText: string
- QuestionType: HomeworkQuestionType (enum)
- Options: string? (JSON)
- CorrectAnswer: string?
- Points: int
- Explanation: string?
```

### 8.2 Enums

#### SubmissionType
- `File`
- `Image`
- `Text`
- `Link`
- `MULTIPLE_CHOICE`

#### HomeworkStatus
- `Assigned` = 0
- `Submitted` = 1
- `Graded` = 2
- `Late` = 3
- `Missing` = 4

#### HomeworkQuestionType
- `MultipleChoice`
- `TextInput`

#### UserRole
- `Admin`
- `ManagementStaff`
- `AccountantStaff`
- `Teacher`
- `Parent`

---

*Document Version: 1.0*
*Last Updated: 2025-02-10*

