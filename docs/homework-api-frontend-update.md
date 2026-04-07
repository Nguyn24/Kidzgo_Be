# Homework API Frontend Update

Tài liệu này tổng hợp các thay đổi API trong flow homework để frontend cập nhật request, response parsing và xử lý lỗi.

## 1. Breaking/Behavior Changes

- `ResubmitPolicy` không còn dùng nữa.
- `MaxAttempts` là field chính để quy định số lần làm bài.
- `AllowResubmit` vẫn còn trong request để backward compatibility:
  - nếu FE gửi `MaxAttempts` thì backend ưu tiên `MaxAttempts`
  - nếu không gửi `MaxAttempts`:
    - `AllowResubmit = true` -> backend map thành `MaxAttempts = 2`
    - `AllowResubmit = false` hoặc không gửi -> backend map thành `MaxAttempts = 1`
- Trong response:
  - `AllowResubmit` vẫn được trả về để FE tiện check nhanh
  - `MaxAttempts` mới là source of truth
- Homework submission bây giờ có history theo từng lần nộp.

## 2. Request Changes

### 2.1 `POST /api/homework`

Request body thay đổi:

- Thêm `maxAttempts?: number`
- Bỏ `resubmitPolicy`
- `allowResubmit?: boolean` vẫn nhận nhưng chỉ là fallback cũ

Request body hiện tại:

```json
{
  "classId": "guid",
  "sessionId": "guid|null",
  "title": "Homework title",
  "description": "text",
  "dueAt": "2026-04-08T10:00:00Z",
  "book": "string",
  "pages": "string",
  "skills": "string",
  "topic": "string",
  "grammarTags": ["tag1"],
  "vocabularyTags": ["tag1"],
  "submissionType": "Text|File|Image|Video|Link|MULTIPLE_CHOICE",
  "maxScore": 10,
  "rewardStars": 5,
  "timeLimitMinutes": 20,
  "maxAttempts": 3,
  "allowResubmit": true,
  "aiHintEnabled": true,
  "aiRecommendEnabled": true,
  "instructions": "text",
  "expectedAnswer": "text",
  "rubric": "text",
  "speakingMode": "ReadAloud|FreeTalk",
  "targetWords": ["word1"],
  "speakingExpectedText": "text",
  "attachment": "url"
}
```

### 2.2 `POST /api/homework/multiple-choice`

Request body thay đổi:

- Thêm `maxAttempts?: number`
- Bỏ `resubmitPolicy`
- `allowResubmit?: boolean` vẫn là fallback cũ

```json
{
  "classId": "guid",
  "sessionId": "guid|null",
  "title": "Quiz 1",
  "description": "text",
  "dueAt": "2026-04-08T10:00:00Z",
  "topic": "Grammar",
  "grammarTags": ["present-simple"],
  "vocabularyTags": ["school"],
  "rewardStars": 5,
  "timeLimitMinutes": 15,
  "maxAttempts": 2,
  "allowResubmit": true,
  "aiHintEnabled": false,
  "aiRecommendEnabled": false,
  "instructions": "text",
  "questions": [
    {
      "questionText": "What is the capital of Vietnam?",
      "questionType": "MultipleChoice",
      "options": ["Hanoi", "Ho Chi Minh City", "Da Nang", "Hai Phong"],
      "correctAnswer": "Hanoi",
      "points": 10,
      "explanation": "Hanoi is the capital city of Vietnam"
    }
  ]
}
```

### 2.3 `POST /api/homework/multiple-choice/from-bank`

Request body thay đổi:

- Thêm `maxAttempts?: number`
- Bỏ `resubmitPolicy`
- `allowResubmit?: boolean` vẫn là fallback cũ

```json
{
  "classId": "guid",
  "programId": "guid",
  "sessionId": "guid|null",
  "title": "Quiz from bank",
  "description": "text",
  "dueAt": "2026-04-08T10:00:00Z",
  "topic": "Reading",
  "grammarTags": ["tag1"],
  "vocabularyTags": ["tag1"],
  "rewardStars": 5,
  "timeLimitMinutes": 20,
  "maxAttempts": 2,
  "allowResubmit": true,
  "instructions": "text",
  "distribution": [
    {
      "level": "Easy",
      "count": 5
    }
  ]
}
```

### 2.4 `PUT /api/homework/{id}`

Request body thay đổi:

- Thêm `maxAttempts?: number`
- Bỏ `resubmitPolicy`
- `allowResubmit?: boolean` vẫn là fallback cũ

Lưu ý:

- Không update được homework nếu đã có bài nộp `Submitted` hoặc `Graded`

## 3. Response Changes

## 3.1 Teacher APIs

### `GET /api/homework`

Mỗi item trong `data.homeworkAssignments.items[]` có thêm:

- `allowResubmit: boolean`
- `maxAttempts: number`

Ví dụ:

```json
{
  "id": "guid",
  "title": "Quiz 1",
  "submissionType": "MULTIPLE_CHOICE",
  "allowResubmit": true,
  "maxAttempts": 3
}
```

### `GET /api/homework/{id}`

Response có thêm:

- `allowResubmit: boolean`
- `maxAttempts: number`

Trong `students[]` có thêm:

- `attemptCount: number`

Ví dụ:

```json
{
  "id": "guid",
  "allowResubmit": true,
  "maxAttempts": 3,
  "students": [
    {
      "id": "homeworkStudentId",
      "studentProfileId": "guid",
      "studentName": "Student A",
      "status": "Graded",
      "attemptCount": 2
    }
  ]
}
```

### `GET /api/homework/students/{studentProfileId}/history`

Mỗi item trong `data.homeworks.items[]` có thêm:

- `allowResubmit: boolean`
- `maxAttempts: number`
- `attemptCount: number`
- `attempts: HomeworkSubmissionAttemptDto[]`

Shape của `attempts[]`:

```json
{
  "id": "guid",
  "attemptNumber": 2,
  "status": "Graded",
  "startedAt": "2026-04-07T08:00:00Z",
  "submittedAt": "2026-04-07T08:10:00Z",
  "gradedAt": "2026-04-07T08:11:00Z",
  "score": 8,
  "teacherFeedback": "Good",
  "aiFeedback": "text",
  "textAnswer": "text",
  "attachmentUrl": "url",
  "isLatest": true
}
```

### `GET /api/homework/submissions/{homeworkStudentId}`

Teacher xem chi tiết bài nộp có thêm:

- `allowResubmit`
- `maxAttempts`
- `attemptCount`
- `attempts[]`

Nếu là quiz và đã graded thì có:

- `questions[]`
- `review.answerResults[]`

## 3.2 Student APIs

### `GET /api/students/homework/my`

Mỗi item trong `data.homeworks.items[]` có thêm:

- `allowResubmit: boolean`
- `maxAttempts: number`
- `attemptCount: number`
- `attempts: HomeworkSubmissionAttemptDto[]`

### `GET /api/students/homework/submitted`

API này vẫn dùng shape của `GetStudentHomeworksResponse`, nhưng giờ mỗi item cũng có:

- `allowResubmit`
- `maxAttempts`
- `attemptCount`
- `attempts[]`

FE có thể dùng API này để render danh sách bài đã nộp theo từng lần làm.

### `GET /api/students/homework/{homeworkStudentId}`

Đây là API student xem chi tiết bài tập / bài đã nộp.

Response mới có thêm:

- `allowResubmit: boolean`
- `maxAttempts: number`
- `attemptId?: guid`
- `attemptNumber?: number`
- `attemptCount: number`
- `attempts: HomeworkSubmissionAttemptDto[]`
- `showReview: boolean`
- `showCorrectAnswer: boolean`
- `showExplanation: boolean`

Nếu là multiple choice:

- `questions[]` trả về options dạng object, không còn chỉ là string
- `review.answerResults[]` trả về đáp án mình chọn và đáp án đúng

`questions[]`:

```json
{
  "id": "questionId",
  "orderIndex": 0,
  "questionText": "What is the capital of Vietnam?",
  "questionType": "MultipleChoice",
  "options": [
    {
      "id": "optionId",
      "text": "Hanoi",
      "orderIndex": 0
    }
  ],
  "points": 10
}
```

`review.answerResults[]`:

```json
{
  "questionId": "guid",
  "questionText": "What is the capital of Vietnam?",
  "selectedOptionId": "guid",
  "selectedOptionText": "Ho Chi Minh City",
  "correctOptionId": "guid",
  "correctOptionText": "Hanoi",
  "isCorrect": false,
  "earnedPoints": 0,
  "maxPoints": 10,
  "explanation": "Hanoi is the capital city of Vietnam"
}
```

### `GET /api/students/homework/{homeworkStudentId}/attempts/{attemptNumber}`

API mới.

Mục đích:

- Xem chi tiết một lần nộp cụ thể
- Dùng cùng response shape với `GET /api/students/homework/{homeworkStudentId}`
- FE chỉ cần thay endpoint, không cần viết parser mới

Ví dụ use case:

- tab history nhiều lần submit
- click vào lần submit số 1, số 2, số 3 để xem lại bài làm từng lần

### `POST /api/students/homework/submit`

Response có thêm:

- `attemptId?: guid`
- `attemptNumber?: number`
- `attemptCount: number`

Ví dụ:

```json
{
  "id": "homeworkStudentId",
  "assignmentId": "guid",
  "status": "Submitted",
  "submittedAt": "2026-04-07T08:10:00Z",
  "attemptId": "guid",
  "attemptNumber": 2,
  "attemptCount": 2
}
```

### `POST /api/students/homework/multiple-choice/submit`

Response có thêm:

- `attemptId?: guid`
- `attemptNumber?: number`
- `attemptCount: number`
- `answerResults[]`

FE có thể dùng ngay `answerResults[]` để hiển thị kết quả bài quiz vừa nộp.

## 4. Multiple Choice Review Rules

Backend hiện hỗ trợ render review cho bài multiple choice theo từng attempt:

- câu đã chọn
- đáp án đúng
- đúng/sai
- điểm từng câu
- explanation

Lưu ý:

- `showReview = true` khi backend cho phép hiển thị review
- `showCorrectAnswer = true` khi FE được phép hiển thị đáp án đúng
- `showExplanation = true` khi FE được phép hiển thị giải thích

FE nên check các flag này thay vì assume luôn được show đáp án.

## 5. Error Messages FE Cần Handle

## 5.1 Error mới / quan trọng

### `Homework.InvalidMaxAttempts`

Message:

```text
MaxAttempts must be greater than 0
```

Khi:

- tạo homework với `maxAttempts <= 0`
- update homework với `maxAttempts <= 0`

### `HomeworkSubmission.AttemptLimitReached`

Message pattern:

```text
This homework can only be submitted {maxAttempts} time(s)
```

Khi:

- học sinh submit vượt quá số lần cho phép

FE nên chặn nút submit/resubmit nếu:

- `attemptCount >= maxAttempts`

### `HomeworkSubmission.AttemptNotFound`

Message pattern:

```text
Attempt {attemptNumber} for homework submission '{homeworkStudentId}' was not found
```

Khi:

- gọi `GET /api/students/homework/{homeworkStudentId}/attempts/{attemptNumber}` với attempt không tồn tại

## 5.2 Error cũ nhưng liên quan trực tiếp flow mới

### `Homework.CannotUpdate`

```text
Cannot update homework assignment that has submitted or graded submissions
```

### `HomeworkSubmission.CannotSubmitMultipleChoice`

```text
Wrong submission endpoint for this homework type
```

### `HomeworkSubmission.NoAnswersProvided`

```text
At least one answer must be provided
```

### `HomeworkSubmission.TimeExpired`

```text
Time limit has expired for this homework
```

### `HomeworkSubmission.CannotSubmitMissing`

```text
Cannot submit homework with MISSING status
```

## 6. FE Action Items

- Đổi form tạo/update homework sang gửi `maxAttempts`
- Không dùng `resubmitPolicy` nữa
- Có thể giữ `allowResubmit` tạm thời nếu cần compatibility, nhưng nên chuyển hẳn sang `maxAttempts`
- Ở list/detail homework, parse thêm:
  - `allowResubmit`
  - `maxAttempts`
  - `attemptCount`
  - `attempts[]`
- Ở student submission detail multiple choice:
  - update parser cho `questions[].options[]` dạng object
  - dùng `review.answerResults[]` để render đáp án của học sinh và đáp án đúng
- Thêm UI xem lịch sử nhiều lần nộp bằng `attempts[]`
- Khi click một attempt cụ thể, gọi:
  - `GET /api/students/homework/{homeworkStudentId}/attempts/{attemptNumber}`
- Trước khi cho submit lại, FE nên check:
  - `attemptCount < maxAttempts`

## 7. Gợi ý migrate FE an toàn

Thứ tự update FE nên làm:

1. Thay request tạo/update homework sang `maxAttempts`
2. Update parser response list/detail để đọc `maxAttempts` + `attempts[]`
3. Update màn student multiple choice review theo `review.answerResults[]`
4. Add UI attempt history
5. Chặn submit lại phía FE khi đã hết lượt
