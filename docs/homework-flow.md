# Homework Full Flow (FE API Usage)

Tai lieu nay mo ta luong day du cua homework va huong dan su dung API cho front-end (request/response).

## 1. Tong quan
- Homework duoc giao theo lop (`ClassId`), co the gan theo buoi hoc (`SessionId`).
- Khi tao homework, he thong tu tao `HomeworkStudent` cho moi hoc sinh trong lop.
- Hoc sinh nop bai, giao vien/TA cham diem, cap nhat trang thai.

## 2. Trang thai
Trang thai homework theo hoc sinh:
- `Assigned`: da giao, chua nop
- `Submitted`: da nop, cho cham
- `Graded`: da cham
- `Late`: nop tre
- `Missing`: qua han, chua nop

## 3. Roles
- Admin/ManagementStaff: toan quyen
- Teacher/TeachingAssistant: tao/cham trong lop phu trach
- Student: xem va nop bai
- Parent: xem bai cua con

## 4. API (Teacher/Staff)

### 4.1 Create Homework
`POST /api/homework`

Request
```json
{
  "classId": "guid",
  "sessionId": "guid|null",
  "title": "Bai tap 1",
  "description": "Mo ta",
  "dueAt": "2026-03-20T16:00:00Z",
  "book": "Book 1",
  "pages": "10-15",
  "skills": "Reading",
  "submissionType": "Text",
  "maxScore": 10,
  "rewardStars": 5,
  "missionId": "guid|null",
  "instructions": "Lam bai trong 30 phut",
  "expectedAnswer": "A,B,C",
  "rubric": "Moi cau 2 diem",
  "attachment": "https://..."
}
```

Response (201)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "title": "Bai tap 1",
    "dueAt": "2026-03-20T16:00:00Z",
    "submissionType": "Text",
    "maxScore": 10,
    "rewardStars": 5,
    "createdAt": "2026-03-18T08:00:00Z"
  }
}
```

### 4.2 Create Multiple Choice Homework
`POST /api/homework/multiple-choice`

Request
```json
{
  "classId": "guid",
  "sessionId": "guid|null",
  "title": "Trac nghiem 1",
  "description": "Mo ta",
  "dueAt": "2026-03-20T16:00:00Z",
  "rewardStars": 5,
  "missionId": "guid|null",
  "instructions": "Chon dap an dung",
  "questions": [
    {
      "questionText": "2 + 2 = ?",
      "questionType": "MultipleChoice",
      "options": ["1","2","3","4"],
      "correctAnswer": "4",
      "points": 1,
      "explanation": "Cong co ban"
    }
  ]
}
```

Response (201)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "Trac nghiem 1",
    "maxScore": 10,
    "createdAt": "2026-03-18T08:00:00Z"
  }
}
```

### 4.3 List Homework
`GET /api/homework?classId=&sessionId=&submissionType=&branchId=&pageNumber=1&pageSize=10`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "classId": "guid",
        "className": "Class A",
        "title": "Bai tap 1",
        "dueAt": "2026-03-20T16:00:00Z",
        "submissionType": "Text",
        "maxScore": 10,
        "rewardStars": 5,
        "submissionCount": 20,
        "gradedCount": 10
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5
  }
}
```

### 4.4 Homework Detail
`GET /api/homework/{id}`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "className": "Class A",
    "sessionId": "guid|null",
    "title": "Bai tap 1",
    "description": "Mo ta",
    "dueAt": "2026-03-20T16:00:00Z",
    "submissionType": "Text",
    "maxScore": 10,
    "rewardStars": 5,
    "questions": [],
    "submissionCount": 20,
    "gradedCount": 10,
    "averageScore": 8.5
  }
}
```

### 4.5 Update Homework
`PUT /api/homework/{id}`

Request (fields optional)
```json
{
  "title": "Bai tap 1 (update)",
  "dueAt": "2026-03-22T16:00:00Z"
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "updatedAt": "2026-03-18T09:00:00Z"
  }
}
```

### 4.6 Delete Homework
`DELETE /api/homework/{id}`

Response (200)
```json
{
  "isSuccess": true,
  "message": "Homework assignment deleted successfully"
}
```

### 4.7 List Submissions
`GET /api/homework/submissions?classId=&status=&pageNumber=1&pageSize=10`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "homeworkId": "guid",
        "homeworkTitle": "Bai tap 1",
        "studentProfileId": "guid",
        "studentName": "Nguyen Van A",
        "status": "Submitted",
        "submittedAt": "2026-03-19T08:00:00Z",
        "gradedAt": null,
        "score": null
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5
  }
}
```

### 4.8 Submission Detail
`GET /api/homework/submissions/{homeworkStudentId}`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "homeworkId": "guid",
    "homeworkTitle": "Bai tap 1",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "status": "Submitted",
    "submittedAt": "2026-03-19T08:00:00Z",
    "gradedAt": null,
    "score": null,
    "textAnswer": "Noi dung",
    "attachmentUrls": ["https://..."],
    "linkUrl": null
  }
}
```

### 4.9 Grade Submission
`POST /api/homework/submissions/{homeworkStudentId}/grade`

Request
```json
{
  "score": 8.5,
  "teacherFeedback": "Tot"
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "homeworkStudentId": "guid",
    "status": "Graded",
    "score": 8.5,
    "gradedAt": "2026-03-19T09:00:00Z"
  }
}
```

### 4.10 Mark Late/Missing
`PUT /api/homework/submissions/{homeworkStudentId}/mark-status`

Request
```json
{
  "status": "Late"
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "homeworkStudentId": "guid",
    "status": "Late",
    "updatedAt": "2026-03-19T09:00:00Z"
  }
}
```

### 4.11 Student Homework History
`GET /api/homework/students/{studentProfileId}/history?classId=&pageNumber=1&pageSize=10`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "studentProfileId": "guid",
    "items": [
      {
        "homeworkId": "guid",
        "homeworkTitle": "Bai tap 1",
        "status": "Graded",
        "dueAt": "2026-03-20T16:00:00Z",
        "submittedAt": "2026-03-19T08:00:00Z",
        "gradedAt": "2026-03-19T09:00:00Z",
        "score": 8.5,
        "maxScore": 10
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50
  }
}
```

## 5. API (Student)

### 5.1 My Homework
`GET /api/students/homework/my?status=&classId=&pageNumber=1&pageSize=10`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "homeworkId": "guid",
        "homeworkTitle": "Bai tap 1",
        "classId": "guid",
        "className": "Class A",
        "status": "Assigned",
        "dueAt": "2026-03-20T16:00:00Z",
        "submissionType": "Text",
        "maxScore": 10,
        "rewardStars": 5
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 20,
    "totalPages": 2
  }
}
```

### 5.2 Do Multiple Choice (Get Questions)
`GET /api/students/homework/{homeworkStudentId}`

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "homeworkId": "guid",
    "homeworkTitle": "Trac nghiem 1",
    "classId": "guid",
    "className": "Class A",
    "status": "Assigned",
    "dueAt": "2026-03-20T16:00:00Z",
    "submissionType": "Quiz",
    "rewardStars": 5,
    "questions": [
      {
        "id": "guid",
        "orderIndex": 1,
        "questionText": "2 + 2 = ?",
        "questionType": "MultipleChoice",
        "options": ["1","2","3","4"],
        "points": 1
      }
    ]
  }
}
```

Luu y:
- Endpoint nay dung de render bai trac nghiem cho hoc sinh lam truc tiep.
- Khong tra `correctAnswer`/`explanation` trong luc lam bai; chi co trong response sau submit.

### 5.3 Submit Homework
`POST /api/students/homework/submit`

Request
```json
{
  "homeworkStudentId": "guid",
  "textAnswer": "Noi dung",
  "attachmentUrls": [],
  "linkUrl": null
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "assignmentId": "guid",
    "status": "Submitted",
    "submittedAt": "2026-03-19T08:00:00Z"
  }
}
```

Luu y:
- Reward stars duoc cong neu nop dung han, nhung khong tra ve trong response.

### 5.4 Submit Multiple Choice
`POST /api/students/homework/multiple-choice/submit`

Request
```json
{
  "homeworkStudentId": "guid",
  "answers": [
    { "questionId": "guid", "answer": "4" }
  ]
}
```

Response (200)
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "assignmentId": "guid",
    "status": "Graded",
    "submittedAt": "2026-03-19T08:00:00Z",
    "score": 8.5,
    "maxScore": 10,
    "rewardStars": 5,
    "correctCount": 8,
    "totalCount": 10,
    "totalPoints": 10,
    "earnedPoints": 8,
    "answerResults": [
      {
        "questionId": "guid",
        "questionText": "2 + 2 = ?",
        "studentAnswer": "4",
        "correctAnswer": "4",
        "isCorrect": true,
        "points": 1,
        "explanation": "Cong co ban"
      }
    ]
  }
}
```

Luu y:
- `answer` co the la option index (0-based) hoac text option.
- `rewardStars` chi tra ve gia tri duoc cong khi nop dung han.

### 5.5 My Submitted Homework
`GET /api/students/homework/submitted?pageNumber=1&pageSize=10`

### 5.6 My Homework Detail
`GET /api/students/homework/{homeworkStudentId}`

Luu y:
- Neu submissionType = `Quiz`, su dung response o muc 5.2 de render cau hoi khi lam bai.

### 5.7 My Feedback
`GET /api/students/homework/feedback/my?pageNumber=1&pageSize=10`

## 6. Notes
- Multiple choice co the auto-grade va set `Graded` ngay sau submit.
- Job he thong co the danh dau `Late`/`Missing` theo `DueAt`.
- Response fields co the khac tuy version, can doi chieu swagger khi integrate.
