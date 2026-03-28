# Homework Full Doc

Tai lieu nay mo ta module homework theo code backend hien tai.

## 1. Overview

### 1.1. Core entities
- `HomeworkAssignment`: bai tap giao cho lop, co the gan voi `Session`
- `HomeworkStudent`: ban ghi homework cua tung hoc sinh
- `HomeworkQuestion`: cau hoi cua bai quiz
- `QuestionBankItem`: cau hoi trong question bank

### 1.2. Business rules
- Homework duoc giao theo `ClassId`, `SessionId` la optional.
- Neu co `SessionId`, session phai ton tai va thuoc dung class.
- Khi tao homework, he thong auto tao `HomeworkStudent` cho tat ca hoc sinh dang `EnrollmentStatus.Active` trong class.
- Neu class khong co hoc sinh active, tao homework bi fail.
- Submission type hien co:
  - `File`
  - `Image`
  - `Text`
  - `Link`
  - `Quiz`
- API map `Quiz` thanh `MULTIPLE_CHOICE`.
- Homework thuong:
  - student submit -> `Submitted` hoac `Late`
  - teacher/TA/staff/admin grade tay -> `Graded`
- Homework quiz:
  - student submit xong auto-grade ngay -> `Graded`
- Quiz co the co `TimeLimitMinutes`, `StartedAt`, `AllowResubmit`.
- Job `MarkOverdueHomeworkSubmissionsJob` quet homework qua han:
  - `Assigned -> Missing`
  - set `Score = 0`
  - set `GradedAt`
  - them feedback mac dinh neu chua co
- On-time submit co the duoc cong `RewardStars`.
- Homework thuong nop dung han co the tang progress mission `HomeworkStreak`.

## 2. Role Scope

### 2.1. Role summary
| Role | Xem du lieu gi | Pham vi thuc te |
| --- | --- | --- |
| `Student` | homework cua minh, detail, feedback, ket qua quiz | `own` |
| `TeachingAssistant` | grade, mark late/missing, student history | controller cho phep, nhung nhieu handler chua enforce own-class |
| `Teacher` | CRUD homework, submissions, grade, question bank | mot phan `own class`, mot phan handler chua enforce |
| `ManagementStaff` | full homework | `all` |
| `Admin` | full homework | `all` |
| `Parent` | khong co API homework rieng | none |

### 2.2. Scope note quan trong
- Scope duoc enforce tot:
  - `GET /api/homework/submissions`
  - `GET /api/homework/submissions/{homeworkStudentId}`
  - tat ca student APIs
- Scope chua enforce dong deu cho `Teacher` / `TeachingAssistant`:
  - create/update/delete/link reward
  - grade
  - mark late/missing
  - student history
  - list/detail homework assignment

## 3. Status Definition

| Status | Y nghia |
| --- | --- |
| `Assigned` | da giao, chua nop |
| `Submitted` | da nop, cho cham tay |
| `Graded` | da cham xong |
| `Late` | nop tre |
| `Missing` | qua han khong nop |

### 3.1. Main transitions
- `Assigned -> Submitted`: student nop homework thuong dung han
- `Assigned -> Late`: student nop homework thuong tre han
- `Assigned -> Missing`: job overdue hoac teacher/staff mark
- `Submitted -> Graded`: teacher/TA/staff/admin cham
- `Graded -> Graded`: re-grade homework thuong
- `Late -> Missing`: teacher/TA/staff/admin mark
- `Assigned -> Graded`: student submit quiz, auto-grade

### 3.2. Transition notes
- Homework thuong da auto-grade 0 vi overdue thi khong duoc submit lai qua endpoint submit thuong.
- Quiz submit xong khong qua `Submitted`, ma di thang sang `Graded`.

## 4. Validation Rules

### 4.1. Homework create/update
- `ClassId` required khi create
- `Title` khong duoc rong / whitespace
- `DueAt` neu co phai >= hien tai
- `MaxScore > 0`
- `RewardStars >= 0`
- `TimeLimitMinutes > 0`
- `MissionId` neu co phai ton tai
- `SessionId` neu co phai ton tai va thuoc class
- Update bi chan neu da co submission `Submitted` hoac `Graded`

### 4.2. Submit homework thuong
- Chi owner cua `HomeworkStudent` duoc submit
- Khong duoc submit neu da `Submitted` hoac `Graded`
- Khong duoc submit neu record da bi auto-grade 0
- Payload phai phu hop submission type:
  - `File/Image` -> can `AttachmentUrls`
  - `Text` -> can `TextAnswer`
  - `Link` -> can `LinkUrl`

### 4.3. Submit quiz
- Chi owner duoc submit
- Homework phai la `Quiz`
- Phai co `Answers`
- Moi `QuestionId` phai thuoc homework
- Neu co `TimeLimitMinutes`, qua gio se fail
- Neu da `Submitted/Graded`, chi nop lai duoc khi `AllowResubmit = true`
- Neu `Missing`, quiz submit bi chan

### 4.4. Grade / mark
- Grade:
  - `Score >= 0`
  - `Score <= MaxScore` neu co `MaxScore`
  - chi grade duoc khi status la `Submitted` hoac `Graded`
- Mark:
  - chi nhan `Late` hoac `Missing`
  - hop le tu `Assigned`
  - `Late -> Missing` cung hop le

## 5. Response Format

### 5.1. Success
```json
{
  "isSuccess": true,
  "data": {}
}
```

### 5.2. Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Homework.InvalidDueDate",
  "status": 400,
  "detail": "Due date must be in the future"
}
```

### 5.3. Common error codes
- `Homework.NotFound`
- `Homework.ClassNotFound`
- `Homework.SessionNotFound`
- `Homework.MissionNotFound`
- `Homework.InvalidDueDate`
- `Homework.CannotUpdate`
- `Homework.ClassHasNoActiveStudents`
- `Homework.InvalidTitle`
- `Homework.InvalidMaxScore`
- `Homework.InvalidRewardStars`
- `Homework.InvalidTimeLimitMinutes`
- `Homework.InvalidSubmissionType`
- `Homework.InvalidStatusForMarking`
- `Homework.NoQuestionsProvided`
- `Homework.InvalidQuestionText`
- `Homework.InsufficientOptions`
- `Homework.InvalidCorrectAnswer`
- `Homework.InvalidPoints`
- `Homework.ProgramNotFound`
- `Homework.InvalidQuestionDistribution`
- `Homework.InsufficientQuestionsInBank`
- `Homework.UnsupportedQuestionBankFileType`
- `Homework.InvalidQuestionBankFile`
- `Homework.InvalidQuestionBankRow`
- `HomeworkSubmission.NotFound`
- `HomeworkSubmission.InvalidScore`
- `HomeworkSubmission.ScoreExceedsMax`
- `HomeworkSubmission.InvalidStatus`
- `HomeworkSubmission.InvalidStatusTransition`
- `HomeworkSubmission.Unauthorized`
- `HomeworkSubmission.AlreadySubmitted`
- `HomeworkSubmission.AlreadyAutoGraded`
- `HomeworkSubmission.InvalidData`
- `HomeworkSubmission.CannotSubmitMissing`
- `HomeworkSubmission.TimeExpired`
- `HomeworkSubmission.NotSubmitted`
- `HomeworkSubmission.CannotSubmitMultipleChoice`
- `HomeworkSubmission.NoAnswersProvided`
- `HomeworkSubmission.QuestionNotFound`

## 6. Permission Matrix

| Action | Student | Parent | TA | Teacher | ManagementStaff | Admin |
| --- | --- | --- | --- | --- | --- | --- |
| View own homework list/detail/feedback | Yes | No direct API | No | No | No | No |
| Submit homework | Yes | No | No | No | No | No |
| Submit quiz | Yes | No | No | No | No | No |
| Create homework | No | No | No | Yes | Yes | Yes |
| Create quiz manual | No | No | No | Yes | Yes | Yes |
| Create quiz from bank | No | No | No | Yes | Yes | Yes |
| List/detail homework assignment | No | No | No | Yes | Yes | Yes |
| Update/delete homework | No | No | No | Yes | Yes | Yes |
| Link mission / set reward stars | No | No | No | Yes | Yes | Yes |
| List/detail submissions | No | No | No | Yes | Yes | Yes |
| Grade submission | No | No | Yes | Yes | Yes | Yes |
| Mark late/missing | No | No | Yes | Yes | Yes | Yes |
| View student history | No | No | Yes | Yes | Yes | Yes |
| Question bank APIs | No | No | No | Yes | Yes | Yes |

## 7. API Catalog

## 7.1. Teacher / Staff / Admin - Homework

| Method | Endpoint | Roles | Dung de lam gi |
| --- | --- | --- | --- |
| `POST` | `/api/homework` | `Teacher,ManagementStaff,Admin` | Tao homework thuong |
| `POST` | `/api/homework/multiple-choice` | `Teacher,ManagementStaff,Admin` | Tao quiz thu cong |
| `POST` | `/api/homework/multiple-choice/from-bank` | `Teacher,ManagementStaff,Admin` | Tao quiz tu question bank |
| `GET` | `/api/homework` | `Teacher,ManagementStaff,Admin` | List homework assignments |
| `GET` | `/api/homework/{id}` | `Teacher,ManagementStaff,Admin` | Detail homework assignment |
| `PUT` | `/api/homework/{id}` | `Teacher,ManagementStaff,Admin` | Update homework |
| `DELETE` | `/api/homework/{id}` | `Teacher,ManagementStaff,Admin` | Hard delete homework |
| `POST` | `/api/homework/{id}/link-mission` | `Teacher,ManagementStaff,Admin` | Gan mission cho homework |
| `PUT` | `/api/homework/{id}/reward-stars` | `Teacher,ManagementStaff,Admin` | Set reward stars |
| `GET` | `/api/homework/submissions` | `Teacher,ManagementStaff,Admin` | List submissions |
| `GET` | `/api/homework/submissions/{homeworkStudentId}` | `Teacher,ManagementStaff,Admin` | Detail submission |
| `POST` | `/api/homework/submissions/{homeworkStudentId}/grade` | `Teacher,TeachingAssistant,ManagementStaff,Admin` | Cham bai |
| `PUT` | `/api/homework/submissions/{homeworkStudentId}/mark-status` | `Teacher,TeachingAssistant,ManagementStaff,Admin` | Mark late/missing |
| `GET` | `/api/homework/students/{studentProfileId}/history` | `Teacher,TeachingAssistant,ManagementStaff,Admin` | Xem lich su homework cua hoc sinh |

### 7.1.1. `POST /api/homework`
Body:
- `classId: Guid` required
- `sessionId: Guid?`
- `title: string` required
- `description: string?`
- `dueAt: DateTime?`
- `book: string?`
- `pages: string?`
- `skills: string?`
- `submissionType: string` required
- `maxScore: decimal?`
- `rewardStars: int?`
- `timeLimitMinutes: int?`
- `allowResubmit: bool?`
- `missionId: Guid?`
- `instructions: string?`
- `expectedAnswer: string?`
- `rubric: string?`
- `attachment: string?`

Success `201`:
- `id`
- `classId`
- `sessionId`
- `title`
- `description`
- `dueAt`
- `submissionType`
- `maxScore`
- `rewardStars`
- `timeLimitMinutes`
- `allowResubmit`
- `missionId`
- `instructions`
- `expectedAnswer`
- `rubric`
- `attachmentUrl`
- `createdAt`
- `assignedStudentsCount`

### 7.1.2. `POST /api/homework/multiple-choice`
Body:
- `classId: Guid` required
- `sessionId: Guid?`
- `title: string` required
- `description: string?`
- `dueAt: DateTime?`
- `rewardStars: int?`
- `timeLimitMinutes: int?`
- `allowResubmit: bool?`
- `missionId: Guid?`
- `instructions: string?`
- `questions[]` required
  - `questionText: string`
  - `questionType: MultipleChoice|TextInput`
  - `options: string[]`
  - `correctAnswer: string`
  - `points: int`
  - `explanation: string?`

Success `201`:
- `id`
- `classId`
- `sessionId`
- `title`
- `dueAt`
- `rewardStars`
- `timeLimitMinutes`
- `allowResubmit`
- `missionId`
- `instructions`
- `createdAt`
- `assignedStudentsCount`
- `questions[]`

### 7.1.3. `POST /api/homework/multiple-choice/from-bank`
Body:
- `classId: Guid` required
- `programId: Guid` required
- `sessionId: Guid?`
- `title: string` required
- `description: string?`
- `dueAt: DateTime?`
- `rewardStars: int?`
- `timeLimitMinutes: int?`
- `allowResubmit: bool?`
- `missionId: Guid?`
- `instructions: string?`
- `distribution[]`
  - `level: Easy|Medium|Hard`
  - `count: int`

Success `201`:
- giong create quiz manual

### 7.1.4. `GET /api/homework`
Query:
- `classId`
- `sessionId`
- `skill`
- `submissionType`
- `branchId`
- `fromDate`
- `toDate`
- `pageNumber`
- `pageSize`

Success `200`:
- `data.homeworkAssignments.items[]`
  - `id`
  - `classId`
  - `classCode`
  - `classTitle`
  - `sessionId`
  - `title`
  - `description`
  - `dueAt`
  - `book`
  - `pages`
  - `skills`
  - `submissionType`
  - `maxScore`
  - `rewardStars`
  - `timeLimitMinutes`
  - `allowResubmit`
  - `missionId`
  - `createdAt`
  - `totalStudents`
  - `submittedCount`
  - `gradedCount`
  - `lateCount`
  - `missingCount`

### 7.1.5. `GET /api/homework/{id}`
Success `200`:
- homework assignment detail
- `students[]`
  - `id`
  - `studentProfileId`
  - `studentName`
  - `status`
  - `submittedAt`
  - `gradedAt`
  - `score`
  - `teacherFeedback`

### 7.1.6. `PUT /api/homework/{id}`
Body:
- tat ca field optional, giong create tru `attachment`

Success `200`:
- `id`
- `title`
- `dueAt`

### 7.1.7. `DELETE /api/homework/{id}`
Success `200`:
- `data = null`

### 7.1.8. `POST /api/homework/{id}/link-mission`
Body:
- `missionId: Guid`

Success `200`:
- `homeworkId`
- `missionId`
- `missionTitle`

### 7.1.9. `PUT /api/homework/{id}/reward-stars`
Body:
- `rewardStars: int`

Success `200`:
- `homeworkId`
- `rewardStars`

## 7.2. Teacher / Staff / Admin - Submission APIs

### 7.2.1. `GET /api/homework/submissions`
Query:
- `classId`
- `status`
- `pageNumber`
- `pageSize`

Success `200`:
- `data.submissions.items[]`
  - `id`
  - `homeworkAssignmentId`
  - `homeworkTitle`
  - `studentProfileId`
  - `studentName`
  - `status`
  - `submittedAt`
  - `gradedAt`
  - `score`
  - `teacherFeedback`
  - `dueAt`
  - `createdAt`

Enforced scope:
- Teacher chi xem duoc classes ma minh la `MainTeacher` hoac `AssistantTeacher`

### 7.2.2. `GET /api/homework/submissions/{homeworkStudentId}`
Success `200`:
- `id`
- `assignmentId`
- `assignmentTitle`
- `assignmentDescription`
- `assignmentAttachmentUrl`
- `instructions`
- `classId`
- `classCode`
- `classTitle`
- `dueAt`
- `book`
- `pages`
- `skills`
- `submissionType`
- `maxScore`
- `status`
- `submittedAt`
- `gradedAt`
- `score`
- `teacherFeedback`
- `aiFeedback`
- `attachmentUrls`
- `textAnswer`
- `linkUrl`
- `isLate`
- `isOverdue`
- `studentProfileId`
- `studentName`

### 7.2.3. `POST /api/homework/submissions/{homeworkStudentId}/grade`
Body:
- `homeworkStudentId: Guid` (field trong body hien du thua, backend dung path param)
- `score: decimal`
- `teacherFeedback: string?`

Success `200`:
- `id`
- `assignmentId`
- `status`
- `score`
- `teacherFeedback`
- `gradedAt`

### 7.2.4. `PUT /api/homework/submissions/{homeworkStudentId}/mark-status`
Body:
- `homeworkStudentId: Guid` (field trong body hien du thua)
- `status: Late|Missing`

Success `200`:
- `id`
- `status`

### 7.2.5. `GET /api/homework/students/{studentProfileId}/history`
Query:
- `classId`
- `pageNumber`
- `pageSize`

Success `200`:
- `data.homeworks.items[]`
  - `id`
  - `assignmentId`
  - `assignmentTitle`
  - `classId`
  - `classCode`
  - `classTitle`
  - `dueAt`
  - `status`
  - `submittedAt`
  - `gradedAt`
  - `score`
  - `maxScore`
  - `teacherFeedback`
  - `isLate`

## 7.3. Student APIs

| Method | Endpoint | Dung de lam gi |
| --- | --- | --- |
| `GET` | `/api/students/homework/my` | List homework cua minh |
| `GET` | `/api/students/homework/submitted` | List bai da nop |
| `GET` | `/api/students/homework/{homeworkStudentId}` | Detail homework / ket qua cham / quiz review |
| `POST` | `/api/students/homework/submit` | Nop homework thuong |
| `POST` | `/api/students/homework/multiple-choice/submit` | Submit quiz va auto-grade |
| `GET` | `/api/students/homework/feedback/my` | List feedback cua minh |

### 7.3.1. `GET /api/students/homework/my`
Query:
- `status`
- `classId`
- `pageNumber`
- `pageSize`

Success `200`:
- `data.homeworks.items[]`
  - `id`
  - `assignmentId`
  - `assignmentTitle`
  - `assignmentDescription`
  - `assignmentAttachmentUrl`
  - `classId`
  - `classCode`
  - `classTitle`
  - `dueAt`
  - `book`
  - `pages`
  - `skills`
  - `submissionType`
  - `maxScore`
  - `timeLimitMinutes`
  - `status`
  - `submittedAt`
  - `gradedAt`
  - `score`
  - `isLate`
  - `isOverdue`

Note:
- endpoint nay khong tra `teacherFeedback`

### 7.3.2. `GET /api/students/homework/submitted`
Query:
- `classId`
- `pageNumber`
- `pageSize`

Success:
- giong `GET /api/students/homework/my`
- chi giu status `Submitted` va `Graded`

### 7.3.3. `GET /api/students/homework/{homeworkStudentId}`
Success `200`:
- `id`
- `homeworkStudentId`
- `assignmentId`
- `assignmentTitle`
- `assignmentDescription`
- `assignmentAttachmentUrl`
- `instructions`
- `classId`
- `classCode`
- `classTitle`
- `dueAt`
- `book`
- `pages`
- `skills`
- `submissionType`
- `maxScore`
- `rewardStars`
- `timeLimitMinutes`
- `allowResubmit`
- `status`
- `startedAt`
- `submittedAt`
- `gradedAt`
- `score`
- `teacherFeedback`
- `aiFeedback`
- `attachmentUrls`
- `textAnswer`
- `linkUrl`
- `isLate`
- `isOverdue`
- `questions[]`
- `review`
- `showReview`
- `showCorrectAnswer`
- `showExplanation`

Quiz note:
- GET detail quiz lan dau co the set `StartedAt`
- quiz da `Graded` se tra review

### 7.3.4. `POST /api/students/homework/submit`
Body:
- `homeworkStudentId: Guid`
- `textAnswer: string?`
- `attachmentUrls: string[]?`
- `linkUrl: string?`

Success `200`:
- `id`
- `assignmentId`
- `status`
- `submittedAt`

### 7.3.5. `POST /api/students/homework/multiple-choice/submit`
Body:
- `homeworkStudentId: Guid`
- `answers[]`
  - `questionId: Guid`
  - `selectedOptionId: Guid?`

Success `200`:
- `id`
- `assignmentId`
- `status`
- `submittedAt`
- `gradedAt`
- `maxScore`
- `score`
- `rewardStars`
- `correctCount`
- `wrongCount`
- `skippedCount`
- `totalCount`
- `totalPoints`
- `earnedPoints`
- `answerResults[]`

### 7.3.6. `GET /api/students/homework/feedback/my`
Query:
- `classId`
- `pageNumber`
- `pageSize`

Success `200`:
- `data.feedbacks.items[]`
  - `id`
  - `assignmentId`
  - `assignmentTitle`
  - `classId`
  - `classCode`
  - `classTitle`
  - `dueAt`
  - `status`
  - `submittedAt`
  - `gradedAt`
  - `score`
  - `maxScore`
  - `teacherFeedback`
  - `aiFeedback`
  - `isLate`

## 7.4. Related Question Bank APIs

| Method | Endpoint | Roles | Muc dich |
| --- | --- | --- | --- |
| `POST` | `/api/question-bank` | `Teacher,ManagementStaff,Admin` | Tao cau hoi thu cong |
| `GET` | `/api/question-bank` | `Teacher,ManagementStaff,Admin` | List question bank theo program |
| `POST` | `/api/question-bank/import?programId={guid}` | `Teacher,ManagementStaff,Admin` | Import question bank tu file |

Body / params chinh:
- `POST /api/question-bank`
  - `programId`
  - `items[] { questionText, questionType, options, correctAnswer, points, explanation, level }`
- `GET /api/question-bank`
  - `programId` required
  - `level`
  - `pageNumber`
  - `pageSize`
- `POST /api/question-bank/import`
  - form-data `file`
  - support `.csv .xlsx .xls .docx .pdf`

## 8. Known Gaps

### 8.1. FE gaps ma backend da co
- Student xem feedback giao vien:
  - backend da co `GET /api/students/homework/feedback/my`
  - detail student da co `teacherFeedback`, `aiFeedback`, `score`
- Student xem ket qua cham bai sau khi nop file:
  - backend da co `GET /api/students/homework/{homeworkStudentId}`
- Tao bai multiple choice bang question bank:
  - backend da co `POST /api/homework/multiple-choice/from-bank`
  - backend da co full question bank APIs
  - neu man hinh chua co thi day la gap FE

### 8.2. Backend gaps / mismatches
- `CreateHomeworkAssignmentRequest` co `attachment`, nhung create handler hien khong map `AttachmentUrl` vao entity
- `UpdateHomeworkAssignmentRequest` hien khong co field update attachment
- `GetHomeworkSubmissionDetailResponse` co `LinkUrl`, nhung handler hien khong set
- Nhieu teacher/TA handlers chua enforce own-class scope
- `GET /api/students/homework/submitted` dang filter sau query, khong paging DB that su

## 9. Recommended FE Flow

### 9.1. Student
1. `GET /api/students/homework/my`
2. Mo detail:
   - homework thuong: `GET /api/students/homework/{homeworkStudentId}`
   - quiz: `GET /api/students/homework/{homeworkStudentId}`
3. Submit:
   - thuong: `POST /api/students/homework/submit`
   - quiz: `POST /api/students/homework/multiple-choice/submit`
4. Tab feedback:
   - `GET /api/students/homework/feedback/my`

### 9.2. Teacher
1. Tao homework / tao quiz / tao quiz from bank
2. Theo doi submissions:
   - `GET /api/homework/submissions`
   - `GET /api/homework/submissions/{homeworkStudentId}`
3. Cham bai:
   - `POST /api/homework/submissions/{homeworkStudentId}/grade`
4. Mark late/missing neu can:
   - `PUT /api/homework/submissions/{homeworkStudentId}/mark-status`
