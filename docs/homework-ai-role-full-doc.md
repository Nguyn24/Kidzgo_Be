# Homework AI Role Full Doc

Ngày cập nhật: 2026-04-02

Màn hình:
- `Student Homework` trên Zalo Mini App
- `Teacher Homework` trên Web Portal
- `Question Bank` trên Web Portal

## 1. Phạm vi và nguồn kiểm tra

Tài liệu này mô tả các thay đổi và phần AI vừa được bổ sung cho luồng homework/question bank, đối chiếu trực tiếp từ code hiện tại ở các file chính:

- `Kidzgo.API/Controllers/StudentController.cs`
- `Kidzgo.API/Controllers/HomeworkController.cs`
- `Kidzgo.API/Controllers/QuestionBankController.cs`
- `Kidzgo.API/Requests/GetHomeworkHintRequest.cs`
- `Kidzgo.API/Requests/GetHomeworkRecommendationsRequest.cs`
- `Kidzgo.API/Requests/GetHomeworkSpeakingAnalysisRequest.cs`
- `Kidzgo.API/Requests/AiGradeHomeworkRequest.cs`
- `Kidzgo.API/Requests/GenerateQuestionBankItemsRequest.cs`
- `Kidzgo.Application/Homework/GetHomeworkHint/*`
- `Kidzgo.Application/Homework/GetHomeworkRecommendations/*`
- `Kidzgo.Application/Homework/GetHomeworkSpeakingAnalysis/*`
- `Kidzgo.Application/Homework/AiGradeHomework/*`
- `Kidzgo.Application/QuestionBank/GenerateAiQuestionBankItems*`
- `Kidzgo.Application/Abstraction/Homework/IAiHomeworkAssistant.cs`
- `Kidzgo.Infrastructure/AI/HttpAiHomeworkAssistant.cs`
- `Kidzgo.Domain/Homework/Errors/HomeworkErrors.cs`
- `AI-KidzGo/app/agents/a3_homework/*`
- `AI-KidzGo/app/agents/a8_speaking/*`

Ngoài phạm vi:

- UI/UX thực tế trên Zalo Mini App và Web Portal
- Các API homework CRUD có sẵn không liên quan đến AI
- AI Monthly Report / AI Feedback Enhancement

## 2. Tóm tắt luồng nghiệp vụ hiện tại

### 2.1. Student role

1. Học sinh vào chi tiết homework của mình.
2. Nếu bài có bật `AiHintEnabled`, học sinh có thể bấm `Gợi ý`.
3. Nếu bài có bật `AiRecommendEnabled`, học sinh có thể bấm `Luyện tập thêm`.
4. Nếu bài là bài speaking và assignment có `SpeakingMode`, học sinh có thể gửi transcript tạm hoặc nộp audio/video rồi yêu cầu `AI Speaking`.
5. Ngoài luồng homework, học sinh cũng có thể dùng `AI Speaking Practice` bất cứ lúc nào bằng cách thu âm rồi gọi endpoint `multipart/form-data`, không cần submit homework trước.
6. Các API student AI chỉ đọc dữ liệu submission hiện tại và trả về phân tích/gợi ý; không đổi `Score`, `Status`, `TeacherFeedback`.

### 2.2. Teacher / TA / Staff role

1. Giáo viên/TA/staff mở submission đã nộp.
2. Bấm `Chấm nhanh`.
3. Backend tự chọn A3 grading hoặc A8 speaking theo cấu hình assignment:
   - Nếu assignment có `SpeakingMode` -> đi A8.
   - Ngược lại -> đi A3.
4. Nếu AI chấm được (`aiUsed=true`) thì backend persist:
   - `Status = Graded`
   - `Score`
   - `AiFeedback`
   - `GradedAt`
5. Nếu AI không chấm được (`aiUsed=false`) thì trả kết quả có `warnings`, nhưng không đổi trạng thái/submission trong DB.

### 2.3. Teacher / Management / Admin role với AI Creator

1. Người dùng nhập `topic`, `questionType`, `questionCount`, `level`, tags...
2. Backend gọi A3 `generate-question-bank-items`.
3. AI trả về danh sách câu hỏi draft đã normalize về schema hiện tại.
4. Kết quả hiện tại chỉ là draft; không auto save vào `QuestionBankItem`.
5. Nếu muốn lưu thật, FE gọi tiếp endpoint question bank tạo item thủ công đã có sẵn.

## 3. Role, dữ liệu được xem và phạm vi dữ liệu

### 3.1. Mỗi role được xem dữ liệu gì

| Role | Dữ liệu xem được trong luồng AI này |
| --- | --- |
| `Student` | Kết quả AI Hint, AI Recommend, AI Speaking của chính submission homework của mình, và kết quả AI Speaking Practice luyện nói tức thì |
| `Teacher` | Kết quả AI quick grade của submission homework; draft câu hỏi AI Creator |
| `TeachingAssistant` | Kết quả AI quick grade của submission homework |
| `ManagementStaff` | Kết quả AI quick grade của submission homework; draft câu hỏi AI Creator |
| `Admin` | Kết quả AI quick grade của submission homework; draft câu hỏi AI Creator |
| `Parent` | Không có API AI nào trong luồng này |
| `Anonymous` | Không có API AI nào trong luồng này |

### 3.2. Phạm vi dữ liệu (own / department / all)

| Role | Phạm vi hiện tại |
| --- | --- |
| `Student` | `own` |
| `Teacher` | `all` trong các endpoint được cấp role; handler hiện chưa enforce own-class cho quick-grade và ai-generate |
| `TeachingAssistant` | `all` trên quick-grade |
| `ManagementStaff` | `all` |
| `Admin` | `all` |
| `Parent` | none |
| `Anonymous` | none |

Ghi chú implementation hiện tại:

- Các API student AI enforce `own` bằng `userContext.StudentId` và so sánh `HomeworkStudent.StudentProfileId`.
- `quick-grade` hiện chỉ check role + `homeworkStudentId`; không có filter theo lớp giáo viên.
- `question-bank/ai-generate` chỉ check role + `ProgramId` tồn tại; không có scope `own/department`.

### 3.3. Các hành động được phép

| Module | Student | Teacher | TeachingAssistant | ManagementStaff | Admin | Parent | Anonymous |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `AI Hint` | view/use | không có | không có | không có | không có | không có | không có |
| `AI Recommend` | view/use | không có | không có | không có | không có | không có | không có |
| `AI Speaking Analysis` | view/use | không có | không có | không có | không có | không có | không có |
| `AI Speaking Practice` | view/use | không có | không có | không có | không có | không có | không có |
| `AI Quick Grade` | không có | view/use/persist | view/use/persist | view/use/persist | view/use/persist | không có | không có |
| `AI Creator` | không có | view/use | không có | view/use | view/use | không có | không có |

## 4. Response contract chung

### 4.1. Success response mặc định

Tất cả endpoint business API ở `Kidzgo.API` đang trả envelope thông qua `MatchOk()`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

- `POST/GET` thành công -> HTTP `200 OK`
- `data` là object response từ handler

### 4.2. Error response business/domain

Các lỗi business từ `Result.Failure(...)` được map sang `ProblemDetails`.

Mẫu tổng quát:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Homework.AiHintNotEnabled",
  "status": 400,
  "detail": "AI hint is not enabled for this homework"
}
```

Map HTTP code hiện tại:

- `Validation` -> `400`
- `NotFound` -> `404`
- `Conflict` -> `409`
- lỗi ngoài dự kiến -> `500`

### 4.3. Error response auth

- `401 Unauthorized`: thiếu token / token không hợp lệ
- `403 Forbidden`: có token nhưng không đủ role

### 4.4. Soft fallback AI

Cần phân biệt 2 trường hợp:

- AI service gọi được, nhưng model fallback -> API vẫn `200 OK`, `aiUsed=false`, `warnings[]` có nội dung giải thích
- `AI-KidzGo` service unreachable / trả payload lỗi không parse được -> backend throw exception và có thể ra `500`

## 5. Danh sách API theo luồng

## 5.1. Student AI Hint

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `POST /api/students/homework/{homeworkStudentId}/hint` | `Student` | Trả về gợi ý để học sinh tiếp tục tự suy nghĩ, không giải hộ đáp án | Path: `homeworkStudentId:Guid` bắt buộc. Body: `currentAnswerText?:string`; `language:string="vi"` | `200`; `data:{ aiUsed, summary, hints[], grammarFocus[], vocabularyFocus[], encouragement, warnings[] }` | `404 Profile.StudentNotFound`; `404 HomeworkSubmission.NotFound`; `400 HomeworkSubmission.Unauthorized`; `400 Homework.AiHintNotEnabled`; `500` nếu AI service unreachable |

Success data chi tiết:

```json
{
  "isSuccess": true,
  "data": {
    "aiUsed": true,
    "summary": "string",
    "hints": ["string"],
    "grammarFocus": ["string"],
    "vocabularyFocus": ["string"],
    "encouragement": "string",
    "warnings": ["string"]
  }
}
```

Ghi chú:

- `currentAnswerText` là optional; nếu không gửi thì backend fallback sang `HomeworkStudent.TextAnswer`.
- API này không đổi `Status`, `Score`, `TeacherFeedback`, `AiFeedback`.

## 5.2. Student AI Recommend

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `POST /api/students/homework/{homeworkStudentId}/recommendations` | `Student` | Gợi ý bài luyện tập cùng dạng dựa trên bài vừa làm và question bank cùng program | Path: `homeworkStudentId:Guid` bắt buộc. Body: `currentAnswerText?:string`; `language:string="vi"`; `maxItems:int=5` | `200`; `data:{ aiUsed, summary, focusSkill, topics[], grammarTags[], vocabularyTags[], recommendedLevels[], practiceTypes[], warnings[], items[] }` | `404 Profile.StudentNotFound`; `404 HomeworkSubmission.NotFound`; `400 HomeworkSubmission.Unauthorized`; `400 Homework.AiRecommendNotEnabled`; `500` nếu AI service unreachable |

Format `items[]`:

```json
{
  "questionBankItemId": "Guid",
  "questionText": "string",
  "questionType": "MultipleChoice|TextInput",
  "options": ["string"],
  "topic": "string|null",
  "skill": "string|null",
  "grammarTags": ["string"],
  "vocabularyTags": ["string"],
  "level": "Easy|Medium|Hard",
  "points": 1,
  "reason": "string"
}
```

Ghi chú:

- `maxItems` không trả lỗi validation; handler tự clamp về khoảng `1..10`.
- Các item đề xuất được lấy từ `QuestionBankItem` cùng `ProgramId` của lớp học sinh.
- Backend ưu tiên `QuestionType` phù hợp với assignment hiện tại nếu map được.

## 5.3. Student AI Speaking Analysis

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `POST /api/students/homework/{homeworkStudentId}/speaking-analysis` | `Student` | Phân tích bài speaking/phonics của học sinh và trả stars, score, từ phát âm chưa chuẩn | Path: `homeworkStudentId:Guid` bắt buộc. Body: `currentTranscript?:string`; `language:string="vi"` | `200`; `data:{ aiUsed, summary, transcript, overallScore, pronunciationScore, fluencyScore, accuracyScore, stars, strengths[], issues[], mispronouncedWords[], wordFeedback[], suggestions[], practicePlan[], confidence{}, warnings[] }` | `404 Profile.StudentNotFound`; `404 HomeworkSubmission.NotFound`; `400 HomeworkSubmission.Unauthorized`; `400 Homework.AiSpeakingNotAvailable`; `400 HomeworkSubmission.InvalidData`; `500` nếu AI service unreachable |

Format `wordFeedback[]`:

```json
{
  "word": "string",
  "heardAs": "string|null",
  "issue": "string",
  "tip": "string"
}
```

Ghi chú:

- Nếu request có `currentTranscript` thì backend ưu tiên transcript này.
- Nếu không có transcript nhưng submission có `AttachmentUrl` là audio/video thì backend gọi A8 media analysis.
- API này chỉ đọc dữ liệu; không persist `Score`, `AiFeedback`, `Status`.

### 5.3.a. Student AI Speaking Practice (không cần submit homework)

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `POST /api/students/ai-speaking/analyze` | `Student` | Luyện nói tức thì: học sinh thu âm, gửi file trực tiếp cho AI và nhận feedback ngay mà không cần đi qua `submit homework` | Body `multipart/form-data`: `file:IFormFile` bắt buộc; `homeworkStudentId?:Guid`; `language:string="vi"`; `mode?:string`; `expectedText?:string`; `targetWords?:string`; `instructions?:string` | `200`; `data:{ aiUsed, summary, transcript, overallScore, pronunciationScore, fluencyScore, accuracyScore, stars, strengths[], issues[], mispronouncedWords[], wordFeedback[], suggestions[], practicePlan[], confidence{}, warnings[] }` | `404 Profile.StudentNotFound`; `404 HomeworkSubmission.NotFound`; `400 HomeworkSubmission.Unauthorized`; `400 Homework.AiSpeakingPracticeFileRequired`; `500` nếu AI service unreachable |

Swagger / form-data mẫu:

| Field | Type | Required | Ví dụ |
| --- | --- | --- | --- |
| `file` | binary | Y | file `student-reading.webm` |
| `homeworkStudentId` | string(Guid) | N | `7d7c38d4-7d8d-4f49-92f3-4d2b70f4ef11` |
| `language` | string | N | `vi` |
| `mode` | string | N | `speaking` hoặc `phonics` |
| `expectedText` | string | N | `Hello, my name is Rex.` |
| `targetWords` | string | N | `hello, name, rex` |
| `instructions` | string | N | `Read slowly and clearly.` |

Ví dụ `curl`:

```bash
curl -X POST "https://your-domain/api/students/ai-speaking/analyze" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@student-reading.webm" \
  -F "language=vi" \
  -F "mode=speaking" \
  -F "expectedText=Hello, my name is Rex." \
  -F "targetWords=hello, name, rex" \
  -F "instructions=Read slowly and clearly."
```

Ghi chú:

- Nếu có `homeworkStudentId`, backend sẽ mượn context từ homework hiện có của chính học sinh đó, nhưng vẫn không submit bài và không đổi `Status`.
- Nếu không có `homeworkStudentId`, backend tạo context `practice` độc lập để AI phân tích file ngay.
- Endpoint này chỉ phù hợp cho audio/video; nếu file không phải audio/video thì backend có thể trả `200` với `aiUsed=false` và `warnings[]`.
- Response format được tái sử dụng từ `speaking-analysis`, nên FE có thể dùng chung UI render feedback.

## 5.4. Teacher / TA AI Quick Grade

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `POST /api/homework/submissions/{homeworkStudentId}/quick-grade` | `Teacher`, `TeachingAssistant`, `ManagementStaff`, `Admin` | Trigger AI chấm nhanh cho submission. Nếu assignment có `SpeakingMode` thì dùng A8, ngược lại dùng A3 | Path: `homeworkStudentId:Guid` bắt buộc. Body: `language:string="vi"`; `instructions?:string`; `rubric?:string`; `expectedAnswerText?:string` | `200`; `data:{ id, assignmentId, isSpeakingAnalysis, aiUsed, persisted, status, score, rawAiScore, rawAiMaxScore, summary, strengths[], issues[], suggestions[], extractedStudentAnswer, stars?, pronunciationScore?, fluencyScore?, accuracyScore?, mispronouncedWords[], wordFeedback[], practicePlan[], confidence{}, warnings[], gradedAt }` | `404 HomeworkSubmission.NotFound`; `400 HomeworkSubmission.NotSubmitted`; `400 HomeworkSubmission.InvalidData`; `500` nếu AI service unreachable |

Success response lưu ý:

- `isSpeakingAnalysis=false`:
  - `rawAiScore` / `rawAiMaxScore` đến từ A3
  - các field speaking có thể `null` hoặc rỗng
- `isSpeakingAnalysis=true`:
  - `rawAiScore` là `overallScore` trên thang 10
  - `stars`, `pronunciationScore`, `fluencyScore`, `accuracyScore`, `mispronouncedWords`, `wordFeedback`, `practicePlan` sẽ có dữ liệu nếu AI trả về

Ghi chú:

- Handler chỉ cho quick grade khi submission đang `Submitted` hoặc `Graded`.
- Nếu AI trả `aiUsed=false` thì:
  - API vẫn `200`
  - `persisted=false`
  - DB không đổi `Status`, `Score`, `AiFeedback`, `GradedAt`
- Nếu AI trả `aiUsed=true` thì:
  - `Status` persist thành `Graded`
  - `Score` được normalize theo `assignment.MaxScore`
  - `AiFeedback` được lưu JSON

## 5.5. Teacher / Staff AI Creator

| Endpoint + Method | Role | Dùng để làm gì | Params / Body | Response success | Response error thường gặp |
| --- | --- | --- | --- | --- | --- |
| `POST /api/question-bank/ai-generate` | `Teacher`, `ManagementStaff`, `Admin` | Sinh draft câu hỏi cho question bank dựa trên topic, level, type và tags | Body: `programId:Guid` bắt buộc; `topic:string` bắt buộc; `questionType:string="MultipleChoice"`; `questionCount:int=5`; `level:string="Medium"`; `skill?:string`; `taskStyle:string="standard"`; `grammarTags:string[]`; `vocabularyTags:string[]`; `instructions?:string`; `language:string="vi"`; `pointsPerQuestion:int=1` | `200`; `data:{ aiUsed, summary, items[], warnings[] }` | `400 Invalid question type`; `400 Invalid level`; `400 Invalid task style`; `400 Homework.AiCreatorTopicRequired`; `400 Homework.AiCreatorQuestionCountInvalid`; `400 Homework.AiCreatorInvalidPoints`; `404 Homework.ProgramNotFound`; `500` nếu AI service unreachable |

Format `items[]`:

```json
{
  "questionText": "string",
  "questionType": "MultipleChoice|TextInput",
  "options": ["string"],
  "correctAnswer": "string",
  "points": 1,
  "explanation": "string|null",
  "topic": "string|null",
  "skill": "string|null",
  "grammarTags": ["string"],
  "vocabularyTags": ["string"],
  "level": "Easy|Medium|Hard"
}
```

Ghi chú:

- `taskStyle` chỉ hợp lệ: `standard`, `translation`
- `questionType` chỉ hợp lệ: `MultipleChoice`, `TextInput`
- Endpoint này hiện chỉ sinh draft; không lưu vào `QuestionBankItem`
- Nếu AI trả về draft không hợp lệ hết, API vẫn có thể `200` với `aiUsed=false` và `warnings[]`

## 5.6. AI service routes nội bộ (không expose trực tiếp cho FE)

Những route sau được backend `.NET` gọi sang `AI-KidzGo`:

| Internal route | Dùng để làm gì |
| --- | --- |
| `POST /a3/generate-hint` | AI Hint |
| `POST /a3/recommend-practice` | AI Recommend |
| `POST /a3/generate-question-bank-items` | AI Creator |
| `POST /a3/grade-text` | Quick grade cho text/quiz/link đã trích text |
| `POST /a3/grade-image` | Quick grade cho image |
| `POST /a8/analyze-transcript` | Speaking analysis từ transcript |
| `POST /a8/analyze-media` | Speaking analysis từ audio/video |

## 6. Status definition

## 6.1. HomeworkStatus

| Status | Ý nghĩa |
| --- | --- |
| `Assigned` | Homework đã được giao cho học sinh, chưa nộp |
| `Submitted` | Học sinh đã nộp bài |
| `Graded` | Bài đã được chấm |
| `Late` | Bài nộp trễ |
| `Missing` | Bài quá hạn không nộp |

## 6.2. Luồng chuyển trạng thái trong luồng AI

- `AI Hint`:
  - Không đổi trạng thái
- `AI Recommend`:
  - Không đổi trạng thái
- `AI Speaking Analysis`:
  - Không đổi trạng thái
- `AI Quick Grade`:
  - `Submitted -> Graded` nếu `aiUsed=true`
  - `Graded -> Graded` khi chấm lại nếu `aiUsed=true`
  - `Submitted/Graded -> giữ nguyên` nếu `aiUsed=false`
  - `Assigned/Late/Missing` không được quick-grade bằng endpoint này
- `AI Creator`:
  - Không tạo status DB nào
  - Không tạo `QuestionBankItem` cho đến khi FE gọi endpoint save thủ công

## 6.3. Định nghĩa giá trị sử dụng trong AI Creator

Đây không phải status, nhưng là giá trị đầu vào/ra cần thống nhất:

### HomeworkQuestionType

| Giá trị | Ý nghĩa |
| --- | --- |
| `MultipleChoice` | Câu hỏi trắc nghiệm, `options[]` bắt buộc, `correctAnswer` là index string |
| `TextInput` | Câu hỏi tự luận ngắn / dịch thuật, `options[]` rỗng, `correctAnswer` là text |

### QuestionLevel

| Giá trị | Ý nghĩa |
| --- | --- |
| `Easy` | Dễ |
| `Medium` | Trung bình |
| `Hard` | Khó |

### TaskStyle

| Giá trị | Ý nghĩa |
| --- | --- |
| `standard` | Sinh câu hỏi thông thường |
| `translation` | Sinh dạng bài dịch, vẫn normalize về `TextInput` hoặc `MultipleChoice` |

## 7. Permission matrix theo role

| Capability | Student | Teacher | TeachingAssistant | ManagementStaff | Admin | Parent | Anonymous |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Gọi AI Hint | Y | - | - | - | - | - | - |
| Gọi AI Recommend | Y | - | - | - | - | - | - |
| Gọi AI Speaking Analysis | Y | - | - | - | - | - | - |
| Gọi AI Speaking Practice | Y | - | - | - | - | - | - |
| Trigger AI Quick Grade | - | Y | Y | Y | Y | - | - |
| Xem kết quả AI Quick Grade | - | Y | Y | Y | Y | - | - |
| Gọi AI Creator | - | Y | - | Y | Y | - | - |
| Lưu draft AI Creator vào Question Bank | - | Y | - | Y | Y | - | - |

Ký hiệu:

- `Y`: được phép
- `-`: không có quyền

## 8. Validation rule và rule kiểm tra dữ liệu

## 8.1. AI Hint

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| User phải có `StudentId` trong context | handler | `404 Profile.StudentNotFound` |
| Submission phải tồn tại | handler | `404 HomeworkSubmission.NotFound` |
| Submission phải thuộc student đang đăng nhập | handler | `400 HomeworkSubmission.Unauthorized` |
| Assignment phải bật `AiHintEnabled` | handler | `400 Homework.AiHintNotEnabled` |
| `language` nếu rỗng thì default `vi` | controller/handler | không trả lỗi |
| `currentAnswerText` optional, fallback sang `TextAnswer` | handler | không trả lỗi |

## 8.2. AI Recommend

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| User phải có `StudentId` trong context | handler | `404 Profile.StudentNotFound` |
| Submission phải tồn tại | handler | `404 HomeworkSubmission.NotFound` |
| Submission phải thuộc student đang đăng nhập | handler | `400 HomeworkSubmission.Unauthorized` |
| Assignment phải bật `AiRecommendEnabled` | handler | `400 Homework.AiRecommendNotEnabled` |
| `maxItems` được clamp về `1..10` | handler | không trả lỗi validation |
| Question bank candidate chỉ lấy trong cùng `ProgramId` của lớp | handler | không trả lỗi; có thể trả `items=[]` |

## 8.3. AI Speaking Analysis

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| User phải có `StudentId` trong context | handler | `404 Profile.StudentNotFound` |
| Submission phải tồn tại | handler | `404 HomeworkSubmission.NotFound` |
| Submission phải thuộc student đang đăng nhập | handler | `400 HomeworkSubmission.Unauthorized` |
| Assignment phải có `SpeakingMode` | handler | `400 Homework.AiSpeakingNotAvailable` |
| Phải có transcript hoặc attachment | handler | `400 HomeworkSubmission.InvalidData` |
| Transcript request được ưu tiên hơn transcript lưu trong DB | handler | không trả lỗi |
| Attachment speaking hợp lệ để AI phân tích phải là audio/video | infra A8 client | không trả lỗi business; có thể `200` với `aiUsed=false` |

### 8.3.a. AI Speaking Practice

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| User phải có `StudentId` trong context | handler | `404 Profile.StudentNotFound` |
| Phải có `file` trong `multipart/form-data` | controller/handler | `400 Homework.AiSpeakingPracticeFileRequired` |
| Nếu có `homeworkStudentId`, submission phải tồn tại | handler | `404 HomeworkSubmission.NotFound` |
| Nếu có `homeworkStudentId`, submission phải thuộc student đang đăng nhập | handler | `400 HomeworkSubmission.Unauthorized` |
| `language` nếu rỗng thì default `vi` | controller/handler | không trả lỗi |
| `mode` là optional; nếu rỗng thì default về `speaking` | handler | không trả lỗi |
| `targetWords` là optional; nếu có thì parse từ chuỗi comma-separated | handler | không trả lỗi |
| `homeworkStudentId` là optional; nếu không có thì backend tạo context practice độc lập | handler | không trả lỗi |
| File hợp lệ để AI phân tích phải là audio/video | infra A8 client | không trả lỗi business; có thể `200` với `aiUsed=false` |

## 8.4. AI Quick Grade

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| Submission phải tồn tại | handler | `404 HomeworkSubmission.NotFound` |
| Submission status phải là `Submitted` hoặc `Graded` | handler | `400 HomeworkSubmission.NotSubmitted` |
| Submission phải có `TextAnswer` hoặc `AttachmentUrl` | handler | `400 HomeworkSubmission.InvalidData` |
| `language` nếu rỗng thì default `vi` | controller/handler | không trả lỗi |
| Nếu assignment có `SpeakingMode` thì route qua A8 | handler | không trả lỗi |
| Nếu assignment không có `SpeakingMode` thì route qua A3 | handler | không trả lỗi |
| Score được scale về `assignment.MaxScore` nếu có | handler | không trả lỗi |
| File unsupported có thể trả `aiUsed=false` thay vì fail business | infra AI client | không trả lỗi business |

## 8.5. AI Creator

| Rule | Áp dụng ở đâu | Lỗi trả về |
| --- | --- | --- |
| `questionType` phải parse được enum | controller | `400 Invalid question type: {value}` |
| `level` phải parse được enum | controller | `400 Invalid level: {value}` |
| `taskStyle` chỉ nhận `standard` hoặc `translation` | controller | `400 Invalid task style: {value}` |
| `topic` không được rỗng | handler | `400 Homework.AiCreatorTopicRequired` |
| `questionCount` phải trong khoảng `1..10` | handler | `400 Homework.AiCreatorQuestionCountInvalid` |
| `pointsPerQuestion > 0` | handler | `400 Homework.AiCreatorInvalidPoints` |
| `programId` phải tồn tại | handler | `404 Homework.ProgramNotFound` |
| AI output chỉ được normalize về `MultipleChoice` và `TextInput` | AI service | draft lỗi bị loại, không phát sinh business error |

## 9. Các trường hợp trả lỗi nổi bật theo luồng

### 9.1. Student AI Hint

- Gọi bằng token không có student được chọn -> `404 Profile.StudentNotFound`
- Gọi vào submission của học sinh khác -> `400 HomeworkSubmission.Unauthorized`
- Homework không bật AI Hint -> `400 Homework.AiHintNotEnabled`

### 9.2. Student AI Recommend

- Gọi vào submission của học sinh khác -> `400 HomeworkSubmission.Unauthorized`
- Homework không bật AI Recommend -> `400 Homework.AiRecommendNotEnabled`
- Không tìm thấy câu hỏi question bank phù hợp -> không lỗi; `200` với `items=[]`

### 9.3. Student AI Speaking Analysis

- Homework không phải speaking -> `400 Homework.AiSpeakingNotAvailable`
- Submission không có transcript và cũng không có attachment -> `400 HomeworkSubmission.InvalidData`
- Attachment không phải audio/video -> có thể `200` với `aiUsed=false`, `warnings[]`

### 9.3.a. Student AI Speaking Practice

- Gọi mà không gửi `file` -> `400 Homework.AiSpeakingPracticeFileRequired`
- Gửi `homeworkStudentId` của submission không tồn tại -> `404 HomeworkSubmission.NotFound`
- Gửi `homeworkStudentId` của học sinh khác -> `400 HomeworkSubmission.Unauthorized`
- Gửi file không phải audio/video -> có thể `200` với `aiUsed=false`, `warnings[]`

### 9.4. Teacher AI Quick Grade

- Submission chưa nộp (`Assigned/Late/Missing`) -> `400 HomeworkSubmission.NotSubmitted`
- Submission không có nội dung để chấm -> `400 HomeworkSubmission.InvalidData`
- AI service reachable nhưng không chấm được -> `200`, `aiUsed=false`, `persisted=false`
- AI service unreachable -> `500`

### 9.5. Teacher AI Creator

- `questionType` sai -> `400 Invalid question type: ...`
- `level` sai -> `400 Invalid level: ...`
- `taskStyle` sai -> `400 Invalid task style: ...`
- `topic` rỗng -> `400 Homework.AiCreatorTopicRequired`
- `questionCount` ngoài `1..10` -> `400 Homework.AiCreatorQuestionCountInvalid`
- `pointsPerQuestion <= 0` -> `400 Homework.AiCreatorInvalidPoints`
- `programId` không tồn tại -> `404 Homework.ProgramNotFound`
- AI trả draft không hợp lệ -> `200` với `aiUsed=false`, `warnings[]`

## 10. Current implementation notes cần lưu ý khi làm tài liệu/UI

1. Student AI endpoints đã xong backend/API nhưng chưa nối UI thật trên Zalo Mini App.
2. Student hiện có thêm endpoint luyện nói tức thì `POST /api/students/ai-speaking/analyze`, dùng `multipart/form-data`, không cần submit homework trước.
3. Teacher `quick-grade` đã xong backend/API nhưng chưa nối UI thật trên Web Portal.
4. `quick-grade` hiện chưa enforce scope `own class` cho `Teacher` và `TeachingAssistant`.
5. `AI Creator` hiện chỉ generate draft; chưa có endpoint `generate-and-save`.
6. `AI Creator` hiện chỉ support schema hiện tại của question bank:
   - `MultipleChoice`
   - `TextInput`
7. `AI Speaking` student-side không persist điểm; chỉ trả feedback để học sinh sửa ngay.
8. `quick-grade` speaking mới persist điểm/feedback vào `HomeworkStudent`.
9. Nếu AI service `AI-KidzGo` down hoặc response không parse được, backend có thể trả `500`.
10. `Kidzgo.API` trong môi trường hiện tại vẫn có issue build nền:
   - `Build FAILED`
   - `0 Error(s)`
   - chỉ thấy `NU1603`
   Điều này không chỉ ra lỗi compile mới từ các thay đổi AI.
