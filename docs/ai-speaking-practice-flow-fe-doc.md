# Luồng AI Speaking Practice cho FE

Ngày cập nhật: 2026-04-07

Màn hình gợi ý: `Student Mini App -> AI Tutor -> Luyện nói`

## 1. Phạm vi và nguồn kiểm tra

Tài liệu này mô tả luồng luyện nói tức thì bằng AI:

> Học sinh thu âm trực tiếp từ mic hoặc chọn file audio/video, FE gửi file lên API `POST /api/students/ai-speaking/analyze`, AI phân tích phát âm và trả feedback ngay. Không cần submit homework trước.

Các file chính liên quan:

| Nhóm | File |
| --- | --- |
| API | `Kidzgo.API/Controllers/StudentController.cs` |
| Request DTO | `Kidzgo.API/Requests/AnalyzeSpeakingPracticeRequest.cs` |
| Application query | `Kidzgo.Application/Homework/AnalyzeSpeakingPractice/*` |
| Response DTO | `Kidzgo.Application/Homework/GetHomeworkSpeakingAnalysis/GetHomeworkSpeakingAnalysisResponse.cs` |
| AI contract | `Kidzgo.Application/Abstraction/Homework/IAiHomeworkAssistant.cs` |
| AI HTTP client | `Kidzgo.Infrastructure/AI/HttpAiHomeworkAssistant.cs` |
| AI A8 schemas | `AI-KidzGo/app/agents/a8_speaking/schemas.py` |
| AI A8 router | `AI-KidzGo/app/agents/a8_speaking/router.py` |
| AI A8 service/prompt | `AI-KidzGo/app/agents/a8_speaking/service.py` |

Ngoài phạm vi:

| Ngoài phạm vi | Ghi chú |
| --- | --- |
| FE thu âm từ mic | FE tự xử lý `getUserMedia`, `MediaRecorder`, preview audio và tạo `Blob/File` |
| Realtime streaming | API hiện tại nhận một file hoàn chỉnh sau khi thu âm xong, chưa phải websocket/realtime |
| Lưu kết quả speaking practice | API hiện tại trả feedback ngay, không persist score/status vào DB |
| Submit homework | Đây là luồng luyện tập độc lập, không thay thế API submit homework |

## 2. Tóm tắt thay đổi mới

Trước đây `POST /api/students/ai-speaking/analyze` nhận file và các field cơ bản như `mode`, `expectedText`, `targetWords`, `instructions`.

Hiện tại API đã được mở rộng thêm:

| Field mới | Ý nghĩa |
| --- | --- |
| `topic` | Chủ đề hội thoại/luyện nói, ví dụ `Family`, `Birthday`, `Animals` |
| `conversationHistory` | Lịch sử hội thoại trước đó, FE có thể gửi plain text hoặc JSON string |

Mục đích của 2 field này:

| Mục đích | Nội dung |
| --- | --- |
| Làm `summary` tự nhiên hơn | AI có thể phản hồi như gia sư trong ngữ cảnh hội thoại, không chỉ chấm kỹ thuật |
| Bám sát topic đang luyện | AI biết học sinh đang nói về chủ đề gì |
| Hỗ trợ conversation practice | FE có thể gửi các lượt thoại trước để AI hiểu ngữ cảnh |

Response không đổi shape. FE vẫn render các field như `summary`, `transcript`, `overallScore`, `stars`, `mispronouncedWords`, `wordFeedback`, `suggestions`, `practicePlan`, `confidence`, `warnings`.

## 3. Role, dữ liệu được xem và phạm vi dữ liệu

### 3.1. Mỗi role được xem dữ liệu gì

| Role | Dữ liệu xem được trong luồng này |
| --- | --- |
| `Student` | Feedback AI của chính lần luyện nói đang gửi lên; nếu truyền `homeworkStudentId`, chỉ được dùng homework submission của chính học sinh đó |
| `Parent` | Không có API riêng trong luồng này |
| `Teacher` | Không có API riêng trong luồng luyện tức thì của student |
| `Admin` | Không có API riêng trong luồng luyện tức thì của student |
| `ManagementStaff` | Không có API riêng trong luồng luyện tức thì của student |
| `AccountantStaff` | Không có API riêng |
| `Anonymous` | Không có quyền |

Ghi chú:

| Ghi chú | Nội dung |
| --- | --- |
| API nằm trong `StudentController` | Controller có `[Authorize]`, nhưng handler yêu cầu token resolve được `StudentId` |
| Nếu token không có Student profile | Trả lỗi `Profile.StudentNotFound` |
| Không lưu feedback | Response trả trực tiếp về FE, hiện không lưu vào `HomeworkStudent.AiFeedback` |

### 3.2. Phạm vi dữ liệu

| Role | Phạm vi hiện tại |
| --- | --- |
| `Student` | `own`, theo `userContext.StudentId` |
| `Parent` | Không có access |
| `Teacher` | Không có access cho endpoint này |
| `Admin` | Không có access riêng cho endpoint này |
| `ManagementStaff` | Không có access riêng cho endpoint này |
| `Anonymous` | Không có access |

### 3.3. Các hành động được phép

| Hành động | Student | Parent | Teacher | Admin | ManagementStaff | AccountantStaff |
| --- | --- | --- | --- | --- | --- | --- |
| Upload file audio/video để AI phân tích ngay | Có | Không | Không | Không | Không | Không |
| Gửi topic/conversationHistory cho AI | Có | Không | Không | Không | Không | Không |
| Gắn context từ homework của chính mình | Có | Không | Không | Không | Không | Không |
| Submit homework | Không thuộc API này | Không | Không | Không | Không | Không |
| Lưu score/status homework | Không | Không | Không | Không | Không | Không |
| Approve/delete | Không | Không | Không | Không | Không | Không |

## 4. Danh sách API

### 4.1. AI Speaking Practice

| Thuộc tính | Nội dung |
| --- | --- |
| Endpoint | `POST /api/students/ai-speaking/analyze` |
| Method | `POST` |
| Content-Type | `multipart/form-data` |
| Role | Authenticated Student |
| Mục đích | FE gửi file audio/video sau khi học sinh thu âm hoặc chọn file; AI trả feedback speaking ngay |
| Persist DB | Không persist score/status/feedback |

Request form-data:

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `file` | `File` / `binary` | Có | File audio/video. FE nên gửi key là `file` hoặc `File` tùy client mapping; Swagger hiện hiển thị `File` |
| `homeworkStudentId` | `Guid?` | Không | Nếu muốn lấy context từ bài homework đang chọn. Không bắt buộc submit homework trước |
| `language` | `string` | Không | Mặc định `vi`; dùng để AI trả feedback tiếng Việt |
| `mode` | `string?` | Không | Ví dụ `speaking`, `phonics`, `reading`; mặc định `speaking` |
| `topic` | `string?` | Không | Chủ đề hội thoại/luyện nói, ví dụ `Family` |
| `expectedText` | `string?` | Không | Câu/đoạn mẫu nếu muốn AI so sánh bài đọc |
| `targetWords` | `string?` | Không | Danh sách từ trọng tâm, dạng comma-separated, ví dụ `world,birthday,family` |
| `conversationHistory` | `string?` | Không | Lịch sử hội thoại, FE có thể gửi plain text hoặc JSON string |
| `instructions` | `string?` | Không | Gợi ý thêm cho AI, ví dụ `Focus on r blends and ending consonants` |

Ví dụ `curl`:

```bash
curl -X POST "https://api.example.com/api/students/ai-speaking/analyze" \
  -H "Authorization: Bearer <token>" \
  -F "file=@speaking.webm;type=audio/webm" \
  -F "language=vi" \
  -F "mode=speaking" \
  -F "topic=Family" \
  -F "targetWords=mother,father,brother" \
  -F "conversationHistory=[{\"role\":\"ai\",\"text\":\"Tell me about your family.\"},{\"role\":\"student\",\"text\":\"My family has four people.\"}]" \
  -F "instructions=Hãy phản hồi như gia sư thân thiện và chỉ ra từ đọc chưa rõ."
```

Ví dụ FE với `FormData`:

```js
const file = new File([recordedBlob], "speaking.webm", {
  type: recordedBlob.type || "audio/webm"
});

const formData = new FormData();
formData.append("file", file);
formData.append("language", "vi");
formData.append("mode", "speaking");
formData.append("topic", "Family");
formData.append("targetWords", "mother,father,brother");
formData.append(
  "conversationHistory",
  JSON.stringify([
    { role: "ai", text: "Tell me about your family." },
    { role: "student", text: "My family has four people." }
  ])
);
formData.append("instructions", "Phản hồi như gia sư thân thiện.");

const response = await fetch("/api/students/ai-speaking/analyze", {
  method: "POST",
  headers: {
    Authorization: `Bearer ${token}`
  },
  body: formData
});
```

Lưu ý FE:

| Lưu ý | Nội dung |
| --- | --- |
| Không tự set `Content-Type` | Khi dùng `fetch` hoặc `axios` với `FormData`, để browser tự set boundary |
| File từ mic vẫn là file | BE không phân biệt file do user chọn hay file tạo từ `MediaRecorder` |
| `conversationHistory` | Gửi string là được; nếu FE dùng JSON thì stringify trước |
| `topic` khi có homework | Nếu truyền `homeworkStudentId` và không truyền `topic`, backend dùng topic từ homework nếu homework có metadata |

## 5. Response success

Success response dùng envelope chung:

```json
{
  "isSuccess": true,
  "data": {
    "aiUsed": true,
    "summary": "Con đọc khá trôi chảy...",
    "transcript": "hello my name is...",
    "overallScore": 8.5,
    "pronunciationScore": 8,
    "fluencyScore": 9,
    "accuracyScore": 8,
    "stars": 4,
    "strengths": ["Good intonation", "Clear vowels"],
    "issues": ["Ending consonants not clear"],
    "mispronouncedWords": ["world", "birthday"],
    "wordFeedback": [
      {
        "word": "world",
        "heardAs": "wold",
        "issue": "Missing 'r' sound",
        "tip": "Try: w-er-ld"
      }
    ],
    "suggestions": ["Practice 'r' blends daily"],
    "practicePlan": ["5 minutes phonics drill"],
    "confidence": {
      "overall": 0.87,
      "pronunciation": 0.82
    },
    "warnings": []
  },
  "message": null
}
```

Ý nghĩa field response:

| Field | Type | Ý nghĩa |
| --- | --- | --- |
| `aiUsed` | `bool` | `true` nếu AI phân tích được; `false` nếu không dùng được AI hoặc file không phù hợp |
| `summary` | `string` | Nhận xét tổng quan. Nếu có `topic/conversationHistory`, AI sẽ cố viết như phản hồi trong hội thoại |
| `transcript` | `string` | Transcript AI trích từ audio/video |
| `overallScore` | `decimal` | Điểm tổng, thang 0-10 |
| `pronunciationScore` | `decimal` | Điểm phát âm |
| `fluencyScore` | `decimal` | Điểm độ trôi chảy |
| `accuracyScore` | `decimal` | Điểm độ đúng so với nội dung/expected text nếu có |
| `stars` | `int` | Số sao, từ 0 đến 5 |
| `strengths` | `string[]` | Điểm tốt |
| `issues` | `string[]` | Vấn đề phát âm/speaking tổng hợp |
| `mispronouncedWords` | `string[]` | Danh sách từ đọc chưa chuẩn |
| `wordFeedback` | `object[]` | Feedback chi tiết theo từng từ |
| `suggestions` | `string[]` | Gợi ý cải thiện |
| `practicePlan` | `string[]` | Kế hoạch luyện tập ngắn |
| `confidence` | `object` | Độ tin cậy do AI trả về |
| `warnings` | `string[]` | Cảnh báo như file không rõ, thiếu transcript, không đủ evidence |

## 6. Response error

### 6.1. Lỗi business/domain

| Trường hợp | HTTP | Code | Message |
| --- | --- | --- | --- |
| Token không resolve được student profile | 404 | `Profile.StudentNotFound` | `Student profile not found` |
| Không gửi file | 400 | `Homework.AiSpeakingPracticeFileRequired` | `An audio or video file is required for instant AI speaking analysis` |
| Truyền `homeworkStudentId` không tồn tại | 404 | `HomeworkSubmission.NotFound` | `Homework submission with Id = '{id}' was not found` |
| Truyền `homeworkStudentId` của học sinh khác | 400 | `HomeworkSubmission.Unauthorized` | `You do not have permission to access this homework submission` |
| Thiếu token hoặc token sai | 401 | Auth | Unauthorized |

Ví dụ lỗi thiếu file:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Homework.AiSpeakingPracticeFileRequired",
  "status": 400,
  "detail": "An audio or video file is required for instant AI speaking analysis"
}
```

### 6.2. Trường hợp không phải error HTTP

Một số trường hợp API vẫn trả `200 OK` nhưng `aiUsed=false`, ví dụ:

| Trường hợp | Response |
| --- | --- |
| File không phải audio/video | `aiUsed=false`, `warnings=["File hien tai khong phai audio/video de AI phan tich."]` |
| AI service không phân tích được | `aiUsed=false`, `warnings` có lý do lỗi |
| Audio/video quá khó nghe | Có thể `aiUsed=true` hoặc `false`, nhưng `warnings` sẽ có cảnh báo |

FE nên xử lý:

| Nếu | FE nên làm |
| --- | --- |
| `aiUsed=true` | Render feedback bình thường |
| `aiUsed=false` | Hiển thị `warnings` và gợi ý học sinh thu lại/chọn file khác |
| `warnings` có nội dung | Render cảnh báo nhẹ, không coi là crash UI |

## 7. Status definition

Luồng `AI Speaking Practice` không tạo hoặc cập nhật status trong DB.

| Status | Có dùng trong API này không | Ý nghĩa |
| --- | --- | --- |
| Homework `Assigned/Submitted/Graded` | Không cập nhật | API luyện nói tức thì không submit homework và không chấm điểm homework |
| Report status | Không liên quan | Không thuộc luồng báo cáo |
| Practice UI state | FE tự quản lý | Ví dụ `idle`, `recording`, `preview`, `processing`, `done`, `error` |

Flow UI gợi ý cho FE:

| UI state | Trigger |
| --- | --- |
| `idle` | Mở màn luyện nói |
| `recording` | Học sinh bấm thu âm |
| `preview` | Học sinh bấm dừng, FE có `Blob/File` |
| `processing` | FE gọi `POST /api/students/ai-speaking/analyze` |
| `done` | API trả feedback |
| `error` | API trả lỗi domain/auth/network |

## 8. Permission matrix theo role

| Capability | Student | Parent | Teacher | Admin | ManagementStaff | AccountantStaff | Anonymous |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Gọi `POST /api/students/ai-speaking/analyze` | Y | - | - | - | - | - | - |
| Gửi file audio/video để AI phân tích | Y | - | - | - | - | - | - |
| Gửi `topic` | Y | - | - | - | - | - | - |
| Gửi `conversationHistory` | Y | - | - | - | - | - | - |
| Dùng `homeworkStudentId` của chính mình làm context | Y | - | - | - | - | - | - |
| Dùng `homeworkStudentId` của học sinh khác | - | - | - | - | - | - | - |
| Lưu score/status homework | - | - | - | - | - | - | - |
| Approve/reject/delete | - | - | - | - | - | - | - |

Ký hiệu:

| Ký hiệu | Ý nghĩa |
| --- | --- |
| `Y` | Được phép |
| `-` | Không có quyền hoặc không thuộc luồng này |

## 9. Validation rule và rule kiểm tra dữ liệu

### 9.1. Rule bắt buộc

| Rule | Nơi xử lý | Lỗi |
| --- | --- | --- |
| Request phải có token hợp lệ | ASP.NET Auth | `401 Unauthorized` |
| Token phải resolve được `StudentId` | Application handler | `404 Profile.StudentNotFound` |
| Phải có file bytes | Application handler | `400 Homework.AiSpeakingPracticeFileRequired` |
| Nếu truyền `homeworkStudentId`, submission phải tồn tại | Application handler | `404 HomeworkSubmission.NotFound` |
| Nếu truyền `homeworkStudentId`, submission phải thuộc học sinh hiện tại | Application handler | `400 HomeworkSubmission.Unauthorized` |

### 9.2. Rule best-effort

| Rule | Hành vi |
| --- | --- |
| `language` rỗng | Backend dùng mặc định `vi` |
| `mode` rỗng | Backend dùng mặc định `speaking`; nếu có homework context thì dùng `SpeakingMode` từ homework |
| `topic` rỗng | Nếu có homework context thì dùng topic từ homework; nếu không có thì bỏ qua |
| `targetWords` rỗng | Nếu có homework context thì dùng target words từ homework; nếu không có thì bỏ qua |
| `expectedText` rỗng | Nếu có homework context thì dùng `SpeakingExpectedText` hoặc `ExpectedAnswer` từ homework |
| `conversationHistory` rỗng | AI vẫn phân tích bình thường, summary sẽ thiên về feedback kỹ thuật hơn |
| File không phải audio/video | API trả `200 OK` với `aiUsed=false` và `warnings` |

### 9.3. Format khuyến nghị

| Field | Khuyến nghị |
| --- | --- |
| `file` | Ưu tiên `audio/webm`, `audio/wav`, `audio/mp4`, `audio/m4a`, hoặc video hợp lệ |
| `targetWords` | Chuỗi comma-separated: `world,birthday,family` |
| `conversationHistory` | JSON string hoặc plain text; backend không parse, chỉ chuyển xuống AI |
| `topic` | Chuỗi ngắn, dễ hiểu: `Family`, `Birthday`, `At the zoo` |

## 10. Các trường hợp trả lỗi nổi bật

### 10.1. Không có file

Request không có `file`.

Response:

```json
{
  "title": "Homework.AiSpeakingPracticeFileRequired",
  "status": 400,
  "detail": "An audio or video file is required for instant AI speaking analysis"
}
```

### 10.2. Token không có student profile

Ví dụ dùng token Admin/Teacher để gọi endpoint này.

Response:

```json
{
  "title": "Profile.StudentNotFound",
  "status": 404,
  "detail": "Student profile not found"
}
```

### 10.3. Dùng homeworkStudentId của học sinh khác

Response:

```json
{
  "title": "HomeworkSubmission.Unauthorized",
  "status": 400,
  "detail": "You do not have permission to access this homework submission"
}
```

### 10.4. File không phải audio/video

Response vẫn có thể là `200 OK`:

```json
{
  "isSuccess": true,
  "data": {
    "aiUsed": false,
    "summary": "Speaking analysis currently supports audio or video attachments only.",
    "transcript": "",
    "overallScore": 0,
    "pronunciationScore": 0,
    "fluencyScore": 0,
    "accuracyScore": 0,
    "stars": 0,
    "strengths": [],
    "issues": [],
    "mispronouncedWords": [],
    "wordFeedback": [],
    "suggestions": ["Co the thu lai voi audio/video ro hon hoac gui transcript neu can."],
    "practicePlan": [],
    "confidence": {},
    "warnings": ["File hien tai khong phai audio/video de AI phan tich."]
  },
  "message": null
}
```

## 11. Checklist FE

| Việc cần làm | Ghi chú |
| --- | --- |
| Xin quyền mic | Dùng `navigator.mediaDevices.getUserMedia({ audio: true })` nếu môi trường hỗ trợ |
| Thu âm | Dùng `MediaRecorder`, gom chunks thành `Blob` |
| Tạo file | `new File([blob], "speaking.webm", { type: blob.type || "audio/webm" })` |
| Cho nghe lại | Nên có preview audio trước khi gửi |
| Tạo `FormData` | Append `file`, `language`, `mode`, `topic`, `conversationHistory`, `targetWords`, `expectedText`, `instructions` nếu có |
| Gọi API | `POST /api/students/ai-speaking/analyze` |
| Render kết quả | Hiển thị `stars`, score, summary, transcript, word feedback và warnings |
| Xử lý `aiUsed=false` | Gợi ý học sinh thu lại hoặc chọn file audio/video khác |
| Không tự set Content-Type | Để browser tự set multipart boundary |
| Không gọi submit homework | Luồng này không cần submit homework trước |

## 12. So sánh với API speaking theo homework

| API | Khi nào dùng | Có cần submit homework trước không | Có persist score/status không |
| --- | --- | --- | --- |
| `POST /api/students/ai-speaking/analyze` | Luyện nói tức thì, tự do, có thể kèm topic/conversation history | Không | Không |
| `POST /api/students/homework/{homeworkStudentId}/speaking-analysis` | Phân tích bài speaking đã gắn với homework/submission | Thường có file/transcript trong submission hoặc truyền transcript | Không persist score/status |
| `POST /api/homework/submissions/{homeworkStudentId}/quick-grade` | Teacher dùng AI chấm nhanh homework speaking | Có submission | Có persist score/status/AI feedback nếu AI chấm được |

