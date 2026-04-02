# Teaching Materials API Usage Guide

## 1. Mục đích

Tài liệu này mô tả cách hoạt động của nhóm API `TeachingMaterials`, bao gồm:

- Chức năng của từng endpoint
- Format request/response
- Các lỗi thường gặp
- Việc frontend cần làm để upload, hiển thị danh sách, hiển thị bundle, preview file và download file

Base route:

```text
/api/teaching-materials
```

## 2. Authentication và role

Tất cả endpoint trong controller này đều yêu cầu đăng nhập JWT Bearer.

Controller:

- `Kidzgo.API/Controllers/TeachingMaterialsController.cs`

Role được phép:

- `Teacher`
- `ManagementStaff`
- `Admin`

Header cần gửi:

```http
Authorization: Bearer <access_token>
```

Lưu ý quan trọng:

- Nếu frontend chỉ mở `previewUrl` trực tiếp bằng cách paste URL lên browser thì browser sẽ không tự gắn Bearer token.
- Vì vậy `preview`/`download` chỉ hoạt động ổn khi:
  - gọi từ Swagger sau khi đã `Authorize`
  - gọi từ frontend bằng `fetch`/`axios` có Bearer token
  - gọi từ Postman/Insomnia có Bearer token

## 3. Tổng quan flow

Flow chuẩn:

1. Upload tài liệu qua `POST /api/teaching-materials/upload`
2. Lấy danh sách qua `GET /api/teaching-materials`
3. Từ danh sách lấy ra:
   - `id`
   - `previewUrl`
   - `downloadUrl`
   - `programId`
   - `unitNumber`
   - `lessonNumber`
4. Gọi:
   - `GET /api/teaching-materials/{id}` để xem metadata chi tiết
   - `GET /api/teaching-materials/lesson-bundle` để lấy bộ tài liệu theo `program + unit + lesson`
   - `GET /api/teaching-materials/{id}/preview` để xem trực tiếp
   - `GET /api/teaching-materials/{id}/download` để tải file

## 4. Ý nghĩa các loại ID

### 4.1 `programId`

Đây là ID của chương trình học.

Ví dụ:

```text
f7abde94-3f8d-44c0-a2ea-e7b8099c437d
```

`programId` được dùng để:

- upload tài liệu cho program
- filter danh sách tài liệu
- lấy bundle theo bài học

### 4.2 `id` trong `/{id}/preview` và `/{id}/download`

Đây là `TeachingMaterial.Id`, tức ID của từng file tài liệu cụ thể, không phải `programId`.

Ví dụ từ response `GET /api/teaching-materials`:

```json
{
  "id": "3c7ce48f-5ea0-4890-b170-da7f93ecd4ea",
  "programId": "f7abde94-3f8d-44c0-a2ea-e7b8099c437d",
  "previewUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/preview",
  "downloadUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/download"
}
```

Trong ví dụ trên:

- `preview id` = `3c7ce48f-5ea0-4890-b170-da7f93ecd4ea`
- `download id` = `3c7ce48f-5ea0-4890-b170-da7f93ecd4ea`

## 5. File được lưu như thế nào

Teaching materials không được public trực tiếp như `/storage/...`.

Backend hiện đang:

- lưu file ở local disk private
- mã hóa file trước khi lưu
- khi preview/download thì đọc file private, giải mã, rồi stream bytes về response

Điều này có nghĩa:

- `GET` metadata chỉ trả về thông tin file
- file thật chỉ xuất hiện khi gọi `preview` hoặc `download`
- không có public URL cố định để frontend gắn trực tiếp như ảnh CDN thông thường

## 6. Endpoint chi tiết

## 6.1 Upload teaching materials

### Endpoint

```http
POST /api/teaching-materials/upload
Content-Type: multipart/form-data
```

### Mục đích

Upload một file, nhiều file, hoặc một file `.zip`.

### Request fields

Theo `Kidzgo.API/Requests/UploadTeachingMaterialRequest.cs`:

- `programId: Guid?`
- `unitNumber: int?`
- `lessonNumber: int?`
- `lessonTitle: string?`
- `displayName: string?`
- `category: string?`
- `file: IFormFile?`
- `files: List<IFormFile>?`
- `relativePaths: List<string>?`
- `archive: IFormFile?`

### Cách backend xử lý

- Nếu có `archive`, backend chỉ chấp nhận file `.zip`
- Nếu upload 1 file riêng lẻ thì có thể override thủ công:
  - `unitNumber`
  - `lessonNumber`
  - `lessonTitle`
  - `displayName`
  - `category`
- Nếu upload nhiều file hoặc zip, backend cố parse metadata từ tên file / cấu trúc folder
- Các file unsupported sẽ bị skip

### Response success

Response được bọc trong envelope chuẩn:

```json
{
  "isSuccess": true,
  "data": {
    "programId": "f7abde94-3f8d-44c0-a2ea-e7b8099c437d",
    "programName": "Starter",
    "importedCount": 2,
    "skippedCount": 1,
    "importedItems": [
      {
        "id": "3c7ce48f-5ea0-4890-b170-da7f93ecd4ea",
        "originalFileName": "UNIT 1-L2-READING _ WRITING.pptx",
        "displayName": "UNIT 1-L2-READING _ WRITING",
        "relativePath": null,
        "unitNumber": 1,
        "lessonNumber": 2,
        "lessonTitle": "READING _ WRITING",
        "fileType": "Presentation",
        "category": "LessonSlide"
      }
    ],
    "skippedItems": [
      {
        "fileName": "desktop.ini",
        "relativePath": null,
        "reason": "File is empty"
      }
    ]
  }
}
```

## 6.2 Get all teaching materials

### Endpoint

```http
GET /api/teaching-materials
```

### Query params

- `programId: Guid?`
- `unitNumber: int?`
- `lessonNumber: int?`
- `fileType: string?`
- `category: string?`
- `searchTerm: string?`
- `pageNumber: int = 1`
- `pageSize: int = 20`

### Mục đích

API chính để frontend render:

- table/list tài liệu
- search/filter
- metadata để mở detail, preview, download

### Response success

```json
{
  "isSuccess": true,
  "data": {
    "materials": {
      "items": [
        {
          "id": "3c7ce48f-5ea0-4890-b170-da7f93ecd4ea",
          "programId": "f7abde94-3f8d-44c0-a2ea-e7b8099c437d",
          "programName": "Starter",
          "programCode": "STARTER_",
          "unitNumber": 1,
          "lessonNumber": 2,
          "lessonTitle": "READING _ WRITING",
          "relativePath": null,
          "displayName": "UNIT 1-L2-READING _ WRITING",
          "originalFileName": "UNIT 1-L2-READING _ WRITING.pptx",
          "mimeType": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
          "fileSize": 14700381,
          "fileType": "Presentation",
          "category": "LessonSlide",
          "uploadedByUserId": "05adf46b-5ed7-4401-91ef-aa8439c4dd87",
          "uploadedByName": "Administrator",
          "isEncrypted": true,
          "previewUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/preview",
          "downloadUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/download",
          "createdAt": "2026-04-02T14:22:19.782205Z",
          "updatedAt": "2026-04-02T14:22:19.782205Z"
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

### Frontend dùng API này để làm gì

- render grid/table tài liệu
- hiển thị icon theo `fileType`
- hiển thị badge theo `category`
- lấy `id` để mở detail
- lấy `previewUrl` để xem trực tiếp
- lấy `downloadUrl` để tải

## 6.3 Get teaching material by id

### Endpoint

```http
GET /api/teaching-materials/{id}
```

### Mục đích

Lấy metadata chi tiết của 1 file.

### Response success

```json
{
  "isSuccess": true,
  "data": {
    "id": "3c7ce48f-5ea0-4890-b170-da7f93ecd4ea",
    "programId": "f7abde94-3f8d-44c0-a2ea-e7b8099c437d",
    "programName": "Starter",
    "programCode": "STARTER_",
    "unitNumber": 1,
    "lessonNumber": 2,
    "lessonTitle": "READING _ WRITING",
    "relativePath": null,
    "displayName": "UNIT 1-L2-READING _ WRITING",
    "originalFileName": "UNIT 1-L2-READING _ WRITING.pptx",
    "mimeType": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    "fileExtension": ".pptx",
    "fileSize": 14700381,
    "fileType": "Presentation",
    "category": "LessonSlide",
    "isEncrypted": true,
    "encryptionAlgorithm": "AES-256-CBC",
    "encryptionKeyVersion": "v1",
    "uploadedByUserId": "05adf46b-5ed7-4401-91ef-aa8439c4dd87",
    "uploadedByName": "Administrator",
    "previewUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/preview",
    "downloadUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/download",
    "createdAt": "2026-04-02T14:22:19.782205Z",
    "updatedAt": "2026-04-02T14:22:19.782205Z"
  }
}
```

### Frontend dùng API này để làm gì

- mở drawer/modal chi tiết
- hiển thị thêm metadata nâng cao
- confirm trước khi preview/download

## 6.4 Get lesson bundle

### Endpoint

```http
GET /api/teaching-materials/lesson-bundle
```

### Query params bắt buộc

- `programId: Guid`
- `unitNumber: int`
- `lessonNumber: int`

### Mục đích

Lấy toàn bộ tài liệu của một bài học, đã group sẵn theo loại file để UI render dễ hơn.

### Lưu ý rất quan trọng

API này không hoạt động nếu chỉ truyền `programId`.

Sai:

```text
GET /api/teaching-materials/lesson-bundle?programId=f7abde94-3f8d-44c0-a2ea-e7b8099c437d
```

Đúng:

```text
GET /api/teaching-materials/lesson-bundle?programId=f7abde94-3f8d-44c0-a2ea-e7b8099c437d&unitNumber=1&lessonNumber=2
```

### Response success

```json
{
  "isSuccess": true,
  "data": {
    "programId": "f7abde94-3f8d-44c0-a2ea-e7b8099c437d",
    "programName": "Starter",
    "programCode": "STARTER_",
    "unitNumber": 1,
    "lessonNumber": 2,
    "lessonTitle": "READING _ WRITING",
    "primaryPresentation": {
      "id": "3c7ce48f-5ea0-4890-b170-da7f93ecd4ea",
      "displayName": "UNIT 1-L2-READING _ WRITING",
      "originalFileName": "UNIT 1-L2-READING _ WRITING.pptx",
      "relativePath": null,
      "mimeType": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
      "fileExtension": ".pptx",
      "fileSize": 14700381,
      "fileType": "Presentation",
      "category": "LessonSlide",
      "previewUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/preview",
      "downloadUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/download",
      "createdAt": "2026-04-02T14:22:19.782205Z"
    },
    "presentations": [],
    "audioFiles": [],
    "imageFiles": [],
    "videoFiles": [],
    "documents": [],
    "supplementaryFiles": [],
    "otherFiles": []
  }
}
```

### Frontend dùng API này để làm gì

- render lesson page theo từng nhóm tài liệu
- show section:
  - Presentation
  - Audio
  - Image
  - Video
  - Documents
  - Supplementary
  - Others

## 6.5 Preview file trực tiếp

### Endpoint

```http
GET /api/teaching-materials/{id}/preview
```

### Mục đích

Xem trực tiếp file trên UI/browser thay vì tải xuống.

### Response success

API này không trả JSON. Nó trả binary content inline với đúng `Content-Type`.

Ví dụ:

- ảnh: `image/png`, `image/jpeg`
- PDF: `application/pdf`
- audio: `audio/mpeg`
- video: `video/mp4`
- PPTX: `application/vnd.openxmlformats-officedocument.presentationml.presentation`

### Lưu ý

- Route này vẫn cần Bearer token
- Dán URL trực tiếp lên browser thường không dùng được nếu browser không mang token
- Phù hợp nhất cho frontend gọi bằng `fetch`/`axios`, nhận blob rồi render
- `ppt/pptx/doc/xlsx` thường không preview native tốt trên browser dù backend đã trả đúng file

## 6.6 Download file

### Endpoint

```http
GET /api/teaching-materials/{id}/download
```

### Mục đích

Tải file về máy người dùng.

### Response success

API này không trả JSON. Nó trả binary content kèm file name để browser hiểu là file download.

### Khi nào nên dùng

- user bấm nút download
- browser không hỗ trợ preview
- file là `ppt`, `pptx`, `doc`, `docx`, `xlsx`

## 7. Các enum quan trọng

### 7.1 `fileType`

Các giá trị có thể gặp:

- `Pdf`
- `Presentation`
- `Audio`
- `Document`
- `Image`
- `Video`
- `Spreadsheet`
- `Archive`
- `Other`

### 7.2 `category`

Các giá trị có thể gặp:

- `ProgramDocument`
- `LessonSlide`
- `LessonAsset`
- `Supplementary`
- `Other`

## 8. Response envelope chuẩn

Các API metadata (`upload`, `get all`, `get by id`, `lesson-bundle`) đều trả:

```json
{
  "isSuccess": true,
  "data": { }
}
```

Các API binary (`preview`, `download`) không dùng envelope này.

## 9. Error format

Khi lỗi, backend trả `ProblemDetails`.

Format chung:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "TeachingMaterial.NotFound",
  "status": 404,
  "detail": "Teaching material with Id = '...' was not found",
  "traceId": "..."
}
```

### Mapping status code

- Validation -> `400`
- NotFound -> `404`
- Conflict -> `409`
- Unexpected -> `500`

## 10. Danh sách error message thường gặp

### Upload errors

#### `400 TeachingMaterial.NoFilesProvided`

```json
{
  "title": "TeachingMaterial.NoFilesProvided",
  "status": 400,
  "detail": "No teaching material files were provided"
}
```

#### `404 TeachingMaterial.ProgramNotFound`

```json
{
  "title": "TeachingMaterial.ProgramNotFound",
  "status": 404,
  "detail": "Program with Id = '...' was not found or inactive"
}
```

#### `404 TeachingMaterial.ProgramNameNotFound`

```json
{
  "title": "TeachingMaterial.ProgramNameNotFound",
  "status": 404,
  "detail": "Program matching '...' was not found or inactive"
}
```

#### `400 TeachingMaterial.ProgramCouldNotBeInferred`

```json
{
  "title": "TeachingMaterial.ProgramCouldNotBeInferred",
  "status": 400,
  "detail": "Program could not be inferred from uploaded folder structure. Provide ProgramId or include the program folder as the first path segment"
}
```

#### `400 TeachingMaterial.MultipleProgramRoots`

```json
{
  "title": "TeachingMaterial.MultipleProgramRoots",
  "status": 400,
  "detail": "Uploaded files contain multiple program roots: ..."
}
```

#### `400 TeachingMaterial.NoSupportedFilesFound`

```json
{
  "title": "TeachingMaterial.NoSupportedFilesFound",
  "status": 400,
  "detail": "No supported teaching material files were found in the upload"
}
```

### Get / preview / download errors

#### `404 TeachingMaterial.NotFound`

```json
{
  "title": "TeachingMaterial.NotFound",
  "status": 404,
  "detail": "Teaching material with Id = '...' was not found"
}
```

#### `404 TeachingMaterial.StoredFileMissing`

```json
{
  "title": "TeachingMaterial.StoredFileMissing",
  "status": 404,
  "detail": "Encrypted content for teaching material '...' was not found"
}
```

#### `404 TeachingMaterial.LessonBundleNotFound`

```json
{
  "title": "TeachingMaterial.LessonBundleNotFound",
  "status": 404,
  "detail": "Teaching material bundle was not found for program '...', unit '1', lesson '2'"
}
```

### Auth errors

#### `401 Unauthorized`

```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "Authentication required to access this resource."
}
```

#### `403 Forbidden`

```json
{
  "status": 403,
  "title": "Forbidden",
  "detail": "You do not have permission to access this resource."
}
```

## 11. Frontend cần làm gì

## 11.1 Danh sách tài liệu

Frontend nên dùng:

```text
GET /api/teaching-materials
```

UI nên hiển thị:

- `displayName`
- `originalFileName`
- `programName`
- `unitNumber`
- `lessonNumber`
- `lessonTitle`
- `fileType`
- `category`
- `fileSize`
- `uploadedByName`
- `createdAt`

Nên có actions:

- `Preview`
- `Download`
- `View detail`

## 11.2 Trang lesson materials

Frontend nên dùng:

```text
GET /api/teaching-materials/lesson-bundle?programId=...&unitNumber=...&lessonNumber=...
```

UI nên group theo:

- `primaryPresentation`
- `presentations`
- `audioFiles`
- `imageFiles`
- `videoFiles`
- `documents`
- `supplementaryFiles`
- `otherFiles`

## 11.3 Preview trên UI

Khuyến nghị dùng `fetch`/`axios` + blob:

```ts
const response = await fetch(`${baseUrl}${previewUrl}`, {
  headers: {
    Authorization: `Bearer ${token}`,
  },
});

if (!response.ok) {
  throw new Error("Preview failed");
}

const blob = await response.blob();
const objectUrl = URL.createObjectURL(blob);
```

Sau đó render:

- ảnh:

```html
<img src="{objectUrl}" />
```

- PDF:

```html
<iframe src="{objectUrl}"></iframe>
```

- audio:

```html
<audio controls src="{objectUrl}"></audio>
```

- video:

```html
<video controls src="{objectUrl}"></video>
```

## 11.4 Download trên UI

Frontend gọi `downloadUrl` với Bearer token, lấy blob và trigger download.

Khuyến nghị:

- Nếu `fileType` là `Presentation`, `Document`, `Spreadsheet`
  - ưu tiên nút download
- Nếu `fileType` là `Image`, `Pdf`, `Audio`, `Video`
  - ưu tiên nút preview

## 11.5 Không nên làm gì

- Không dùng `programId` cho `preview` hoặc `download`
- Không gọi `lesson-bundle` chỉ với `programId`
- Không assume `previewUrl` là public URL mở trực tiếp được trên browser
- Không assume mọi file đều preview native được, đặc biệt là `pptx`, `docx`, `xlsx`

## 12. Ví dụ thực tế với dữ liệu đang có

Từ response hiện tại:

```json
{
  "id": "3c7ce48f-5ea0-4890-b170-da7f93ecd4ea",
  "programId": "f7abde94-3f8d-44c0-a2ea-e7b8099c437d",
  "unitNumber": 1,
  "lessonNumber": 2,
  "previewUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/preview",
  "downloadUrl": "/api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/download"
}
```

Request đúng:

### Preview

```text
GET /api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/preview
```

### Download

```text
GET /api/teaching-materials/3c7ce48f-5ea0-4890-b170-da7f93ecd4ea/download
```

### Bundle

```text
GET /api/teaching-materials/lesson-bundle?programId=f7abde94-3f8d-44c0-a2ea-e7b8099c437d&unitNumber=1&lessonNumber=2
```

## 13. Kết luận

Tách biệt trách nhiệm của các API như sau:

- `upload`: tạo dữ liệu
- `get all`: lấy list cho UI
- `get by id`: lấy detail metadata
- `lesson-bundle`: lấy nhóm tài liệu theo lesson
- `preview`: stream file để xem trực tiếp
- `download`: stream file để tải xuống

Nếu frontend bám đúng flow trên thì đã đủ để xây:

- trang quản lý tài liệu
- lesson material viewer
- preview modal/page
- download action
