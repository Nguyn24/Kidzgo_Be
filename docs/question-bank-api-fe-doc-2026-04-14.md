# Question Bank API FE Doc - 2026-04-14

Tai lieu nay mo ta cac API trong `QuestionBankController.cs` va phan vua cap nhat cho FE:

- Them API `POST /api/question-bank/ai-generate/from-file` de AI Creator gen cau hoi tu file upload truoc.
- Neu khong upload file, API moi fallback ve cac field nhu UI AI Creator hien tai.
- Tang gioi han `questionCount` cua AI Creator tu 10 len 50 cau/request.
- BE extract text tu file va gui sang AI service bang `source_text` va `source_file_name`.
- API AI Creator chi tra ve draft cau hoi, chua tu dong luu vao DB. FE muon luu thi goi tiep `POST /api/question-bank`.

## Tong quan role va pham vi du lieu

Tat ca API trong controller yeu cau user da dang nhap vi controller co `[Authorize]`.

| Role | Du lieu duoc xem | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Teacher | Question bank items, AI draft items | all trong pham vi API hien tai; co the loc theo `programId`, `level` | view, create, import, ai_generate |
| ManagementStaff | Question bank items, AI draft items | all trong pham vi API hien tai; co the loc theo `programId`, `level` | view, create, import, ai_generate |
| Admin | Question bank items, AI draft items | all trong pham vi API hien tai; co the loc theo `programId`, `level` | view, create, import, ai_generate |
| Parent | Khong duoc truy cap | none | none |
| Student | Khong duoc truy cap | none | none |
| Role khac/anonymous | Khong duoc truy cap | none | none |

Ghi chu:

- Hien tai BE chua enforce scope `own` hoac `department` cho Question Bank.
- Field `CreatedBy` co luu nguoi tao khi create/import, nhung API list hien tai khong filter theo `CreatedBy`.
- Controller chua co API edit, approve, delete cho Question Bank.

## Response format chung

Success tu `MatchOk()` duoc boc trong `ApiResult<T>`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error tu domain result tra ve dang ProblemDetails:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Homework.AiCreatorQuestionCountInvalid",
  "status": 400,
  "detail": "Question count must be between 1 and 50"
}
```

Mot so validation truc tiep trong controller tra `400 Bad Request` voi body dang string hoac object:

```json
"Invalid level: Expert"
```

```json
{
  "error": "No file provided"
}
```

## Danh sach API

### 1. GET `/api/question-bank`

Dung de lay danh sach cau hoi trong question bank.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Pham vi du lieu: all; FE co the loc bang `programId` va `level`.

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `programId` | `Guid?` | No | null | Loc cau hoi theo chuong trinh/goi hoc. Neu null hoac empty Guid thi lay tat ca. |
| `level` | `string?` | No | null | `Easy`, `Medium`, `Hard`. |
| `pageNumber` | `int` | No | 1 | Trang can lay. |
| `pageSize` | `int` | No | 10 | So item moi trang. |

Success response:

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

Error response:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | `Invalid level: {level}` | `level` khong parse duoc thanh `Easy`, `Medium`, `Hard`. |
| 401 | Unauthorized | Chua dang nhap hoac token khong hop le. |
| 403 | Forbidden | Role khong thuoc `Teacher`, `ManagementStaff`, `Admin`. |

### 2. POST `/api/question-bank`

Dung de tao va luu cau hoi thu cong vao question bank. Day cung la API FE nen goi sau khi user review AI draft va muon luu cau hoi.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Pham vi du lieu: create vao `programId` duoc gui trong body.

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

Body fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program ton tai trong DB. |
| `items` | `array` | Yes | Danh sach cau hoi can tao, toi thieu 1 item. |
| `items[].questionText` | `string` | Yes | Noi dung cau hoi, khong duoc rong. |
| `items[].questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput`. |
| `items[].options` | `array<string>` | Required voi `MultipleChoice` | Dap an lua chon, toi thieu 2 option. |
| `items[].correctAnswer` | `string` | Yes | Dap an dung. Voi `MultipleChoice`, BE normalize theo option text. |
| `items[].points` | `int` | Yes | Phai lon hon 0. Default DTO la 1. |
| `items[].explanation` | `string?` | No | Giai thich dap an. |
| `items[].topic` | `string?` | No | Chu de cau hoi. |
| `items[].skill` | `string?` | No | Ky nang, vi du `grammar`, `vocabulary`, `reading`. |
| `items[].grammarTags` | `array<string>?` | No | Tag ngu phap. |
| `items[].vocabularyTags` | `array<string>?` | No | Tag tu vung. |
| `items[].level` | `string` | Yes | `Easy`, `Medium`, `Hard`. |

Success response:

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

Error response:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | `Invalid question type: {questionType}` | `questionType` khong parse duoc thanh enum. |
| 400 | `Invalid level: {level}` | `level` khong parse duoc thanh enum. |
| 400 | `Homework.NoQuestionsProvided` | `items` null hoac rong. |
| 400 | `Homework.InvalidQuestionText` | `questionText` rong. |
| 400 | `Homework.InsufficientOptions` | MultipleChoice co it hon 2 options. |
| 400 | `Homework.InvalidCorrectAnswer` | `correctAnswer` khong match option hop le. |
| 400 | `Homework.InvalidPoints` | `points <= 0`. |
| 404 | `Homework.ProgramNotFound` | `programId` khong ton tai. |
| 401 | Unauthorized | Chua dang nhap hoac token khong hop le. |
| 403 | Forbidden | Role khong hop le. |

### 3. POST `/api/question-bank/import`

Dung de import file cau hoi co cau truc vao question bank va luu thang vao DB.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Content-Type: `multipart/form-data`

Query params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program can import cau hoi vao. |

Form-data:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `file` | `IFormFile` | Yes | File cau hoi co cau truc. Max request size cua endpoint: 20 MB. |

File support:

| File type | Rule |
| --- | --- |
| `.csv` | Header row + data rows. |
| `.xlsx`, `.xls` | Lay sheet dau tien, row dau la header. |
| `.docx` | Can co table dau tien; row dau la header. |
| `.pdf` | Text phai co header row, delimiter la `,`, `|`, hoac tab. |

Required columns:

| Column | Required | Mo ta |
| --- | --- | --- |
| `QuestionText` | Yes | Noi dung cau hoi. |
| `Options` | Yes voi MultipleChoice | Cac option cach nhau bang `|`. |
| `CorrectAnswer` | Yes | Dap an dung. |
| `Level` | Yes | `Easy`, `Medium`, `Hard`. |

Optional columns:

`QuestionType`, `Topic`, `Skill`, `GrammarTags`, `VocabularyTags`, `Points`, `Explanation`.

Header alias BE dang ho tro:

| Field noi bo | Header aliases |
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

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "importedCount": 12
  }
}
```

Error response:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | `{ "error": "No file provided" }` | Khong gui file hoac file length = 0. |
| 400 | `Homework.UnsupportedQuestionBankFileType` | Extension khong nam trong danh sach import support. |
| 400 | `Homework.InvalidQuestionBankFile` | File rong, thieu header, khong co row hop le, khong doc duoc table/text. |
| 400 | `Homework.InvalidQuestionBankRow` | Loi tren tung row, vi du thieu `QuestionText`, invalid `Level`, invalid `Points`, option khong hop le. |
| 404 | `Homework.ProgramNotFound` | `programId` khong ton tai. |
| 401 | Unauthorized | Chua dang nhap hoac token khong hop le. |
| 403 | Forbidden | Role khong hop le. |

### 4. POST `/api/question-bank/ai-generate`

Dung de AI Creator gen draft cau hoi tu cac field tren UI. API nay khong upload file va khong luu DB.

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

Body fields:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `programId` | `Guid` | Yes | - | Program context cho AI generation. |
| `topic` | `string` | Yes | empty | Bat buoc khi khong co source file/text. |
| `questionType` | `string` | No | `MultipleChoice` | `MultipleChoice` hoac `TextInput`. |
| `questionCount` | `int` | No | 5 | Tu 1 den 50. |
| `level` | `string` | No | `Medium` | `Easy`, `Medium`, `Hard`. |
| `skill` | `string?` | No | null | Ky nang can gen. |
| `taskStyle` | `string` | No | `standard` | `standard` hoac `translation`. |
| `grammarTags` | `array<string>` | No | [] | Tag ngu phap. |
| `vocabularyTags` | `array<string>` | No | [] | Tag tu vung. |
| `instructions` | `string?` | No | null | Yeu cau bo sung cho AI. |
| `language` | `string` | No | `vi` | Ngon ngu response. |
| `pointsPerQuestion` | `int` | No | 1 | Diem moi cau, phai lon hon 0. |

Success response:

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

Error response:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | `Invalid question type: {questionType}` | `questionType` invalid. |
| 400 | `Invalid level: {level}` | `level` invalid. |
| 400 | `Invalid task style: {taskStyle}` | `taskStyle` khac `standard` va `translation`. |
| 400 | `Homework.AiCreatorTopicRequired` | `topic` rong va khong co source text/file. |
| 400 | `Homework.AiCreatorQuestionCountInvalid` | `questionCount < 1` hoac `questionCount > 50`. |
| 400 | `Homework.AiCreatorInvalidPoints` | `pointsPerQuestion <= 0`. |
| 404 | `Homework.ProgramNotFound` | `programId` khong ton tai. |
| 500 | Server failure | AI service loi hoac khong goi duoc. |
| 401 | Unauthorized | Chua dang nhap hoac token khong hop le. |
| 403 | Forbidden | Role khong hop le. |

### 5. POST `/api/question-bank/ai-generate/from-file`

Dung de AI Creator gen draft cau hoi tu file upload. Neu FE khong gui `file`, API fallback ve field `topic`, `questionType`, `questionCount`, ... nhu endpoint JSON.

Roles: `Teacher`, `ManagementStaff`, `Admin`

Content-Type: `multipart/form-data`

Request size limit: 20 MB.

Form-data fields:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `programId` | `Guid` | Yes | - | Program context cho AI generation. |
| `file` | `IFormFile?` | No | null | File nguon de AI doc truoc. Neu null thi fallback theo `topic`. |
| `topic` | `string?` | Required neu khong co file | null | Chu de. Neu co file va topic rong, BE dung ten file lam topic fallback. |
| `questionType` | `string` | No | `MultipleChoice` | `MultipleChoice` hoac `TextInput`. |
| `questionCount` | `int` | No | 10 | Tu 1 den 50. |
| `level` | `string` | No | `Medium` | `Easy`, `Medium`, `Hard`. |
| `skill` | `string?` | No | null | Ky nang can gen. |
| `taskStyle` | `string` | No | `standard` | `standard` hoac `translation`. |
| `grammarTags` | `array<string>` hoac comma-separated repeated form value | No | [] | BE co normalize tag cach nhau bang dau phay. |
| `vocabularyTags` | `array<string>` hoac comma-separated repeated form value | No | [] | BE co normalize tag cach nhau bang dau phay. |
| `instructions` | `string?` | No | null | Yeu cau bo sung cho AI. |
| `language` | `string` | No | `vi` | Ngon ngu response. |
| `pointsPerQuestion` | `int` | No | 1 | Diem moi cau, phai lon hon 0. |

Supported source file de AI extract text:

| File type | Xu ly |
| --- | --- |
| `.txt`, `.md`, `.csv`, `.json`, `.xml` | Doc text UTF-8/BOM-aware. |
| `.html`, `.htm` | Strip HTML tags va decode HTML entities. |
| `.docx` | Doc paragraph text. |
| `.pdf` | Doc page text bang PdfPig. |
| `.xlsx`, `.xls` | Doc cell values bang ExcelDataReader. |

BE normalize whitespace va cat source text toi da 50,000 ky tu truoc khi gui sang AI service.

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

Success response giong `POST /api/question-bank/ai-generate`:

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

Error response:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | `{ "error": "File is empty" }` | FE gui field `file` nhung file length = 0. |
| 400 | `Invalid question type: {questionType}` | `questionType` invalid. |
| 400 | `Invalid level: {level}` | `level` invalid. |
| 400 | `Invalid task style: {taskStyle}` | `taskStyle` khac `standard` va `translation`. |
| 400 | `Homework.AiCreatorTopicRequired` | Khong co file/source text va `topic` rong. |
| 400 | `Homework.AiCreatorQuestionCountInvalid` | `questionCount < 1` hoac `questionCount > 50`. |
| 400 | `Homework.AiCreatorInvalidPoints` | `pointsPerQuestion <= 0`. |
| 400 | `Homework.UnsupportedQuestionBankFileType` | File source khong thuoc danh sach support. |
| 400 | `Homework.InvalidQuestionBankFile` | File rong, khong co text doc duoc, hoac exception khi extract text. |
| 404 | `Homework.ProgramNotFound` | `programId` khong ton tai. |
| 500 | Server failure | AI service loi hoac khong goi duoc. |
| 401 | Unauthorized | Chua dang nhap hoac token khong hop le. |
| 403 | Forbidden | Role khong hop le. |

## Status definition

Question Bank hien tai khong co business status rieng.

| Status | Y nghia |
| --- | --- |
| N/A | `QuestionBankItem` khong co status column. |

Enum lien quan nhung khong phai status:

| Enum | Gia tri | Y nghia |
| --- | --- | --- |
| `QuestionLevel` | `Easy`, `Medium`, `Hard` | Muc do cau hoi. Dung de filter va render UI. |
| `HomeworkQuestionType` | `MultipleChoice`, `TextInput` | Loai cau hoi. |
| `aiUsed` | `true/false` | Co AI tham gia tao draft hay khong. Day la field response AI, khong phai lifecycle status. |

Luong chuyen trang thai: khong co status transition trong DB.

Luong FE de luu cau hoi AI:

1. FE goi `POST /api/question-bank/ai-generate` hoac `POST /api/question-bank/ai-generate/from-file`.
2. BE tra ve draft trong `data.items`.
3. User review/chinh sua tren UI.
4. FE goi `POST /api/question-bank` voi cac item da confirm de luu vao DB.

## Permission matrix theo role

| API | Teacher | ManagementStaff | Admin | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/question-bank` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/import` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/ai-generate` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/ai-generate/from-file` | Yes | Yes | Yes | No | No | No |

## Validation rule tong hop

| Rule | API ap dung | Ket qua khi sai |
| --- | --- | --- |
| User phai dang nhap | Tat ca | 401 |
| Role phai la `Teacher`, `ManagementStaff`, `Admin` | Tat ca | 403 |
| `programId` phai ton tai | Create, import, ai-generate, ai-generate/from-file | 404 `Homework.ProgramNotFound` |
| `level` phai la `Easy`, `Medium`, `Hard` | GET filter, create, AI generate | 400 |
| `questionType` phai la `MultipleChoice` hoac `TextInput` | Create, AI generate | 400 |
| `taskStyle` phai la `standard` hoac `translation` | AI generate | 400 |
| `items` phai co it nhat 1 item | Create | 400 `Homework.NoQuestionsProvided` |
| `questionText` khong rong | Create, import | 400 `Homework.InvalidQuestionText` hoac `Homework.InvalidQuestionBankRow` |
| MultipleChoice phai co >= 2 options | Create, import | 400 `Homework.InsufficientOptions` hoac `Homework.InvalidQuestionBankRow` |
| `correctAnswer` phai hop le | Create, import | 400 `Homework.InvalidCorrectAnswer` hoac `Homework.InvalidQuestionBankRow` |
| `points` / `pointsPerQuestion` phai > 0 | Create, import, AI generate | 400 |
| `questionCount` tu 1 den 50 | AI generate | 400 `Homework.AiCreatorQuestionCountInvalid` |
| AI generate can `topic` neu khong co source file/text | AI generate | 400 `Homework.AiCreatorTopicRequired` |
| Import DB can file co cau truc va header bat buoc | Import | 400 `Homework.InvalidQuestionBankFile` |
| AI source file phai co extension support va text doc duoc | ai-generate/from-file | 400 `Homework.UnsupportedQuestionBankFileType` hoac `Homework.InvalidQuestionBankFile` |

## Luu y cho AI service

BE da gui them 2 field vao payload `AiQuestionBankGenerationRequest`:

```json
{
  "source_text": "Extracted plain text from uploaded file...",
  "source_file_name": "unit-animals.pdf"
}
```

AI service endpoint `/a3/generate-question-bank-items` can doc 2 field nay de uu tien noi dung file. Neu AI service bo qua field moi, BE van chay nhung cau hoi co the khong bam sat file upload.
