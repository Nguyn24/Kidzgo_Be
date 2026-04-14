# Tài Liệu API FE - Question Bank - 2026-04-14

Tài liệu này mô tả các API trong `QuestionBankController.cs` và phần vừa cập nhật cho FE:

- Thêm API `POST /api/question-bank/ai-generate/from-file` để AI Creator sinh câu hỏi từ file upload trước.
- Nếu không upload file, API mới fallback về các field như UI AI Creator hiện tại.
- Tăng giới hạn `questionCount` của AI Creator từ 10 lên 50 câu/request.
- BE trích xuất text từ file và gửi sang AI service bằng `source_text` và `source_file_name`.
- API AI Creator chỉ trả về draft câu hỏi, chưa tự động lưu vào DB. FE muốn lưu thì gọi tiếp `POST /api/question-bank`.

## Tổng quan role và phạm vi dữ liệu

Tất cả API trong controller yêu cầu user đã đăng nhập vì controller có `[Authorize]`.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động được phép |
| --- | --- | --- | --- |
| Teacher | Question bank items, AI draft items | `all` trong phạm vi API hiện tại; có thể lọc theo `programId`, `level` | `view`, `create`, `import`, `ai_generate` |
| ManagementStaff | Question bank items, AI draft items | `all` trong phạm vi API hiện tại; có thể lọc theo `programId`, `level` | `view`, `create`, `import`, `ai_generate` |
| Admin | Question bank items, AI draft items | `all` trong phạm vi API hiện tại; có thể lọc theo `programId`, `level` | `view`, `create`, `import`, `ai_generate` |
| Parent | Không được truy cập | `none` | `none` |
| Student | Không được truy cập | `none` | `none` |
| Role khác/anonymous | Không được truy cập | `none` | `none` |

Ghi chú:

- Hiện tại BE chưa enforce scope `own` hoặc `department` cho Question Bank.
- Field `CreatedBy` có lưu người tạo khi create/import, nhưng API list hiện tại không filter theo `CreatedBy`.
- Controller chưa có API edit, approve, delete cho Question Bank.

## Định dạng response chung

Success từ `MatchOk()` được bọc trong `ApiResult<T>`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error từ domain result trả về dạng ProblemDetails:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Homework.AiCreatorQuestionCountInvalid",
  "status": 400,
  "detail": "Question count must be between 1 and 50"
}
```

Một số validation trực tiếp trong controller trả `400 Bad Request` với body dạng string hoặc object:

```json
"Invalid level: Expert"
```

```json
{
  "error": "No file provided"
}
```

## Danh sách API

### 1. GET `/api/question-bank`

Dùng để lấy danh sách câu hỏi trong question bank.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Phạm vi dữ liệu: `all`; FE có thể lọc bằng `programId` và `level`.

Query params:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `programId` | `Guid?` | No | null | Lọc câu hỏi theo chương trình/gói học. Nếu null hoặc empty Guid thì lấy tất cả. |
| `level` | `string?` | No | null | `Easy`, `Medium`, `Hard`. |
| `pageNumber` | `int` | No | 1 | Trang cần lấy. |
| `pageSize` | `int` | No | 10 | Số item mỗi trang. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "items": {
      "items": [
        {
          "id": "11111111-1111-1111-1111-111111111111",
          "programId": "22222222-2222-2222-2222-222222222222",
          "questionText": "Choose the correct answer.",
          "questionType": "MultipleChoice",
          "options": ["A", "B", "C", "D"],
          "correctAnswer": "A",
          "points": 1,
          "explanation": "A is correct.",
          "topic": "Animals",
          "skill": "vocabulary",
          "grammarTags": [],
          "vocabularyTags": ["animal"],
          "level": "Medium",
          "createdAt": "2026-04-14T08:00:00Z"
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

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `Invalid level: {level}` | `level` không parse được thành `Easy`, `Medium`, `Hard`. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Teacher`, `ManagementStaff`, `Admin`. |

### 2. POST `/api/question-bank`

Dùng để tạo và lưu câu hỏi thủ công vào question bank. Đây cũng là API FE nên gọi sau khi user review AI draft và muốn lưu câu hỏi.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Phạm vi dữ liệu: create vào `programId` được gửi trong body.

Body JSON:

```json
{
  "programId": "22222222-2222-2222-2222-222222222222",
  "items": [
    {
      "questionText": "Choose the correct answer.",
      "questionType": "MultipleChoice",
      "options": ["Cat", "Run"],
      "correctAnswer": "Cat",
      "points": 1,
      "explanation": "Cat is a noun.",
      "topic": "Nouns",
      "skill": "grammar",
      "grammarTags": ["noun"],
      "vocabularyTags": [],
      "level": "Easy"
    }
  ]
}
```

Các field trong body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phải tồn tại trong DB. |
| `items` | `array` | Yes | Danh sách câu hỏi cần tạo, tối thiểu 1 item. |
| `items[].questionText` | `string` | Yes | Nội dung câu hỏi, không được rỗng. |
| `items[].questionType` | `string` | Yes | `MultipleChoice` hoặc `TextInput`. |
| `items[].options` | `array<string>` | Required với `MultipleChoice` | Đáp án lựa chọn, tối thiểu 2 option. |
| `items[].correctAnswer` | `string` | Yes | Đáp án đúng. Với `MultipleChoice`, BE normalize theo option text. |
| `items[].points` | `int` | Yes | Phải lớn hơn 0. Default DTO là 1. |
| `items[].explanation` | `string?` | No | Giải thích đáp án. |
| `items[].topic` | `string?` | No | Chủ đề câu hỏi. |
| `items[].skill` | `string?` | No | Kỹ năng, ví dụ `grammar`, `vocabulary`, `reading`. |
| `items[].grammarTags` | `array<string>?` | No | Tag ngữ pháp. |
| `items[].vocabularyTags` | `array<string>?` | No | Tag từ vựng. |
| `items[].level` | `string` | Yes | `Easy`, `Medium`, `Hard`. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "11111111-1111-1111-1111-111111111111",
        "programId": "22222222-2222-2222-2222-222222222222",
        "questionText": "Choose the correct answer.",
        "questionType": "MultipleChoice",
        "options": ["Cat", "Run"],
        "correctAnswer": "Cat",
        "points": 1,
        "explanation": "Cat is a noun.",
        "topic": "Nouns",
        "skill": "grammar",
        "grammarTags": ["noun"],
        "vocabularyTags": [],
        "level": "Easy",
        "createdAt": "2026-04-14T08:00:00Z"
      }
    ]
  }
}
```

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `Invalid question type: {questionType}` | `questionType` không parse được thành enum. |
| 400 | `Invalid level: {level}` | `level` không parse được thành enum. |
| 400 | `Homework.NoQuestionsProvided` | `items` null hoặc rỗng. |
| 400 | `Homework.InvalidQuestionText` | `questionText` rỗng. |
| 400 | `Homework.InsufficientOptions` | MultipleChoice có ít hơn 2 options. |
| 400 | `Homework.InvalidCorrectAnswer` | `correctAnswer` không match option hợp lệ. |
| 400 | `Homework.InvalidPoints` | `points <= 0`. |
| 404 | `Homework.ProgramNotFound` | `programId` không tồn tại. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không hợp lệ. |

### 3. POST `/api/question-bank/import`

Dùng để import file câu hỏi có cấu trúc vào question bank và lưu thẳng vào DB.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Content-Type: `multipart/form-data`

Query params:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program cần import câu hỏi vào. |

Form-data:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `file` | `IFormFile` | Yes | File câu hỏi có cấu trúc. Max request size của endpoint: 20 MB. |

File hỗ trợ:

| File type | Rule |
| --- | --- |
| `.csv` | Header row + data rows. |
| `.xlsx`, `.xls` | Lấy sheet đầu tiên, row đầu là header. |
| `.docx` | Cần có table đầu tiên; row đầu là header. |
| `.pdf` | Text phải có header row, delimiter là `,`, `|`, hoặc tab. |

Các cột bắt buộc:

| Column | Required | Mô tả |
| --- | --- | --- |
| `QuestionText` | Yes | Nội dung câu hỏi. |
| `Options` | Yes với MultipleChoice | Các option cách nhau bằng `|`. |
| `CorrectAnswer` | Yes | Đáp án đúng. |
| `Level` | Yes | `Easy`, `Medium`, `Hard`. |

Các cột không bắt buộc:

`QuestionType`, `Topic`, `Skill`, `GrammarTags`, `VocabularyTags`, `Points`, `Explanation`.

Header alias BE đang hỗ trợ:

| Field nội bộ | Header aliases |
| --- | --- |
| `QuestionText` | `questiontext`, `question_text`, `question` |
| `Options` | `options`, `choices` |
| `CorrectAnswer` | `correctanswer`, `correct_answer`, `answer`, `correct` |
| `Level` | `level`, `difficulty` |
| `Points` | `points`, `score` |
| `Explanation` | `explanation`, `explain` |
| `QuestionType` | `questiontype`, `question_type`, `type` |
| `Topic` | `topic`, `subject` |
| `Skill` | `skill`, `skills` |
| `GrammarTags` | `grammartags`, `grammar_tags`, `grammar` |
| `VocabularyTags` | `vocabularytags`, `vocabulary_tags`, `vocabulary`, `vocabtags`, `vocab_tags` |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "importedCount": 12
  }
}
```

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `{ "error": "No file provided" }` | Không gửi file hoặc file length = 0. |
| 400 | `Homework.UnsupportedQuestionBankFileType` | Extension không nằm trong danh sách import support. |
| 400 | `Homework.InvalidQuestionBankFile` | File rỗng, thiếu header, không có row hợp lệ, không đọc được table/text. |
| 400 | `Homework.InvalidQuestionBankRow` | Lỗi trên từng row, ví dụ thiếu `QuestionText`, invalid `Level`, invalid `Points`, option không hợp lệ. |
| 404 | `Homework.ProgramNotFound` | `programId` không tồn tại. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không hợp lệ. |

### 4. POST `/api/question-bank/ai-generate`

Dùng để AI Creator sinh draft câu hỏi từ các field trên UI. API này không upload file và không lưu DB.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Body JSON:

```json
{
  "programId": "22222222-2222-2222-2222-222222222222",
  "topic": "Animals",
  "questionType": "MultipleChoice",
  "questionCount": 10,
  "level": "Medium",
  "skill": "vocabulary",
  "taskStyle": "standard",
  "grammarTags": [],
  "vocabularyTags": ["animal"],
  "instructions": "Focus on common animals for kids.",
  "language": "vi",
  "pointsPerQuestion": 1
}
```

Các field trong body:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `programId` | `Guid` | Yes | - | Program context cho AI generation. |
| `topic` | `string` | Yes | empty | Bắt buộc khi không có source file/text. |
| `questionType` | `string` | No | `MultipleChoice` | `MultipleChoice` hoặc `TextInput`. |
| `questionCount` | `int` | No | 5 | Từ 1 đến 50. |
| `level` | `string` | No | `Medium` | `Easy`, `Medium`, `Hard`. |
| `skill` | `string?` | No | null | Kỹ năng cần sinh. |
| `taskStyle` | `string` | No | `standard` | `standard` hoặc `translation`. |
| `grammarTags` | `array<string>` | No | [] | Tag ngữ pháp. |
| `vocabularyTags` | `array<string>` | No | [] | Tag từ vựng. |
| `instructions` | `string?` | No | null | Yêu cầu bổ sung cho AI. |
| `language` | `string` | No | `vi` | Ngôn ngữ response. |
| `pointsPerQuestion` | `int` | No | 1 | Điểm mỗi câu, phải lớn hơn 0. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "aiUsed": true,
    "summary": "Generated 10 draft questions.",
    "items": [
      {
        "questionText": "Which animal says meow?",
        "questionType": "MultipleChoice",
        "options": ["Cat", "Dog", "Bird", "Fish"],
        "correctAnswer": "Cat",
        "points": 1,
        "explanation": "A cat says meow.",
        "topic": "Animals",
        "skill": "vocabulary",
        "grammarTags": [],
        "vocabularyTags": ["animal"],
        "level": "Medium"
      }
    ],
    "warnings": []
  }
}
```

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `Invalid question type: {questionType}` | `questionType` invalid. |
| 400 | `Invalid level: {level}` | `level` invalid. |
| 400 | `Invalid task style: {taskStyle}` | `taskStyle` khác `standard` và `translation`. |
| 400 | `Homework.AiCreatorTopicRequired` | `topic` rỗng và không có source text/file. |
| 400 | `Homework.AiCreatorQuestionCountInvalid` | `questionCount < 1` hoặc `questionCount > 50`. |
| 400 | `Homework.AiCreatorInvalidPoints` | `pointsPerQuestion <= 0`. |
| 404 | `Homework.ProgramNotFound` | `programId` không tồn tại. |
| 500 | Server failure | AI service lỗi hoặc không gọi được. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không hợp lệ. |

### 5. POST `/api/question-bank/ai-generate/from-file`

Dùng để AI Creator sinh draft câu hỏi từ file upload. Nếu FE không gửi `file`, API fallback về field `topic`, `questionType`, `questionCount`, ... như endpoint JSON.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Content-Type: `multipart/form-data`

Request size limit: 20 MB.

Các field form-data:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `programId` | `Guid` | Yes | - | Program context cho AI generation. |
| `file` | `IFormFile?` | No | null | File nguồn để AI đọc trước. Nếu null thì fallback theo `topic`. |
| `topic` | `string?` | Required nếu không có file | null | Chủ đề. Nếu có file và topic rỗng, BE dùng tên file làm topic fallback. |
| `questionType` | `string` | No | `MultipleChoice` | `MultipleChoice` hoặc `TextInput`. |
| `questionCount` | `int` | No | 10 | Từ 1 đến 50. |
| `level` | `string` | No | `Medium` | `Easy`, `Medium`, `Hard`. |
| `skill` | `string?` | No | null | Kỹ năng cần sinh. |
| `taskStyle` | `string` | No | `standard` | `standard` hoặc `translation`. |
| `grammarTags` | `array<string>` hoặc comma-separated repeated form value | No | [] | BE có normalize tag cách nhau bằng dấu phẩy. |
| `vocabularyTags` | `array<string>` hoặc comma-separated repeated form value | No | [] | BE có normalize tag cách nhau bằng dấu phẩy. |
| `instructions` | `string?` | No | null | Yêu cầu bổ sung cho AI. |
| `language` | `string` | No | `vi` | Ngôn ngữ response. |
| `pointsPerQuestion` | `int` | No | 1 | Điểm mỗi câu, phải lớn hơn 0. |

File nguồn được hỗ trợ để AI extract text:

| File type | Xử lý |
| --- | --- |
| `.txt`, `.md`, `.csv`, `.json`, `.xml` | Đọc text UTF-8/BOM-aware. |
| `.html`, `.htm` | Strip HTML tags và decode HTML entities. |
| `.docx` | Đọc paragraph text. |
| `.pdf` | Đọc page text bằng PdfPig. |
| `.xlsx`, `.xls` | Đọc cell values bằng ExcelDataReader. |

BE normalize whitespace và cắt source text tối đa 50,000 ký tự trước khi gửi sang AI service.

Example form-data:

```text
programId=22222222-2222-2222-2222-222222222222
file=@unit-animals.pdf
topic=Animals
questionType=MultipleChoice
questionCount=50
level=Medium
skill=vocabulary
taskStyle=standard
vocabularyTags=animal,pet
language=vi
pointsPerQuestion=1
```

Response thành công giống `POST /api/question-bank/ai-generate`:

```json
{
  "isSuccess": true,
  "data": {
    "aiUsed": true,
    "summary": "Generated draft questions from source file.",
    "items": [
      {
        "questionText": "Which animal is mentioned in the source?",
        "questionType": "MultipleChoice",
        "options": ["Cat", "Car", "Chair", "Cloud"],
        "correctAnswer": "Cat",
        "points": 1,
        "explanation": "The source mentions cat.",
        "topic": "Animals",
        "skill": "vocabulary",
        "grammarTags": [],
        "vocabularyTags": ["animal"],
        "level": "Medium"
      }
    ],
    "warnings": []
  }
}
```

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `{ "error": "File is empty" }` | FE gửi field `file` nhưng file length = 0. |
| 400 | `Invalid question type: {questionType}` | `questionType` invalid. |
| 400 | `Invalid level: {level}` | `level` invalid. |
| 400 | `Invalid task style: {taskStyle}` | `taskStyle` khác `standard` và `translation`. |
| 400 | `Homework.AiCreatorTopicRequired` | Không có file/source text và `topic` rỗng. |
| 400 | `Homework.AiCreatorQuestionCountInvalid` | `questionCount < 1` hoặc `questionCount > 50`. |
| 400 | `Homework.AiCreatorInvalidPoints` | `pointsPerQuestion <= 0`. |
| 400 | `Homework.UnsupportedQuestionBankFileType` | File source không thuộc danh sách support. |
| 400 | `Homework.InvalidQuestionBankFile` | File rỗng, không có text đọc được, hoặc exception khi extract text. |
| 404 | `Homework.ProgramNotFound` | `programId` không tồn tại. |
| 500 | Server failure | AI service lỗi hoặc không gọi được. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không hợp lệ. |

## Định nghĩa status

Question Bank hiện tại không có business status riêng.

| Status | Ý nghĩa |
| --- | --- |
| N/A | `QuestionBankItem` không có status column. |

Enum liên quan nhưng không phải status:

| Enum | Giá trị | Ý nghĩa |
| --- | --- | --- |
| `QuestionLevel` | `Easy`, `Medium`, `Hard` | Mức độ câu hỏi. Dùng để filter và render UI. |
| `HomeworkQuestionType` | `MultipleChoice`, `TextInput` | Loại câu hỏi. |
| `aiUsed` | `true/false` | Có AI tham gia tạo draft hay không. Đây là field response AI, không phải lifecycle status. |

Luồng chuyển trạng thái: không có status transition trong DB.

Luồng FE để lưu câu hỏi AI:

1. FE gọi `POST /api/question-bank/ai-generate` hoặc `POST /api/question-bank/ai-generate/from-file`.
2. BE trả về draft trong `data.items`.
3. User review/chỉnh sửa trên UI.
4. FE gọi `POST /api/question-bank` với các item đã confirm để lưu vào DB.

## Ma trận quyền theo role

| API | Teacher | ManagementStaff | Admin | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/question-bank` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/import` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/ai-generate` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/ai-generate/from-file` | Yes | Yes | Yes | No | No | No |

## Validation rule tổng hợp

| Rule | API áp dụng | Kết quả khi sai |
| --- | --- | --- |
| User phải đăng nhập | Tất cả | 401 |
| Role phải là `Teacher`, `ManagementStaff`, `Admin` | Tất cả | 403 |
| `programId` phải tồn tại | Create, import, ai-generate, ai-generate/from-file | 404 `Homework.ProgramNotFound` |
| `level` phải là `Easy`, `Medium`, `Hard` | GET filter, create, AI generate | 400 |
| `questionType` phải là `MultipleChoice` hoặc `TextInput` | Create, AI generate | 400 |
| `taskStyle` phải là `standard` hoặc `translation` | AI generate | 400 |
| `items` phải có ít nhất 1 item | Create | 400 `Homework.NoQuestionsProvided` |
| `questionText` không rỗng | Create, import | 400 `Homework.InvalidQuestionText` hoặc `Homework.InvalidQuestionBankRow` |
| MultipleChoice phải có >= 2 options | Create, import | 400 `Homework.InsufficientOptions` hoặc `Homework.InvalidQuestionBankRow` |
| `correctAnswer` phải hợp lệ | Create, import | 400 `Homework.InvalidCorrectAnswer` hoặc `Homework.InvalidQuestionBankRow` |
| `points` / `pointsPerQuestion` phải > 0 | Create, import, AI generate | 400 |
| `questionCount` từ 1 đến 50 | AI generate | 400 `Homework.AiCreatorQuestionCountInvalid` |
| AI generate cần `topic` nếu không có source file/text | AI generate | 400 `Homework.AiCreatorTopicRequired` |
| Import DB cần file có cấu trúc và header bắt buộc | Import | 400 `Homework.InvalidQuestionBankFile` |
| AI source file phải có extension support và text đọc được | ai-generate/from-file | 400 `Homework.UnsupportedQuestionBankFileType` hoặc `Homework.InvalidQuestionBankFile` |

## Lưu ý cho AI service

BE đã gửi thêm 2 field vào payload `AiQuestionBankGenerationRequest`:

```json
{
  "source_text": "Extracted plain text from uploaded file...",
  "source_file_name": "unit-animals.pdf"
}
```

AI service endpoint `/a3/generate-question-bank-items` cần đọc 2 field này để ưu tiên nội dung file. Nếu AI service bỏ qua field mới, BE vẫn chạy nhưng câu hỏi có thể không bám sát file upload.
