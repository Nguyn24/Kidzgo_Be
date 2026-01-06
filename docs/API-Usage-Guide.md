# API Usage Guide - Kidzgo Backend

Tài liệu này mô tả chi tiết cách sử dụng tất cả các API endpoints trong hệ thống Kidzgo.

## Mục lục

1. [Authentication APIs](#1-authentication-apis)
2. [Email Template APIs](#2-email-template-apis)
3. [Program APIs](#3-program-apis)
4. [Tuition Plan APIs](#4-tuition-plan-apis)
5. [Classroom APIs](#5-classroom-apis)
6. [Class APIs](#6-class-apis)
7. [Enrollment APIs](#7-enrollment-apis)
8. [Ticket APIs](#8-ticket-apis)
9. [Blog APIs](#9-blog-apis)
10. [File Upload APIs](#10-file-upload-apis)

---

## 1. Authentication APIs

Base URL: `/api/auth/`

### 1.1. Login

**Endpoint:** `POST /api/auth/login`

**Mô tả:** Đăng nhập vào hệ thống và nhận access token cùng refresh token.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string",
    "expiresIn": 3600
  }
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Email hoặc password không đúng
- `404 Not Found`: User không tồn tại

---

### 1.2. Refresh Token

**Endpoint:** `POST /api/auth/refresh-token`

**Mô tả:** Lấy access token mới bằng refresh token.

**Authorization:** Không cần (Public)

**Request Body:**
```json
"refresh_token_string"
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new_refresh_token_string",
    "expiresIn": 3600
  }
}
```

---

### 1.3. Change Password

**Endpoint:** `PUT /api/auth/change-password`

**Mô tả:** Thay đổi mật khẩu của user đang đăng nhập.

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "currentPassword": "oldPassword123",
  "newPassword": "newPassword456"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Mật khẩu hiện tại không đúng
- `401 Unauthorized`: Chưa đăng nhập

---

### 1.4. Forget Password

**Endpoint:** `POST /api/auth/forget-password`

**Mô tả:** Gửi email reset password.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

---

### 1.5. Reset Password

**Endpoint:** `POST /api/auth/reset-password`

**Mô tả:** Đặt lại mật khẩu mới bằng token từ email.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "token": "reset_token_from_email",
  "newPassword": "newPassword123"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Token không hợp lệ hoặc đã hết hạn

---

## 2. Email Template APIs

Base URL: `/api/`

### 2.1. Get Email Template

**Endpoint:** `GET /api/email/get`

**Mô tả:** Lấy thông tin email template.

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "header": "Email Header",
    "mainContent": "Main Content",
    "content": "Full Content"
  }
}
```

---

### 2.2. Update Email Template

**Endpoint:** `PUT /api/email/update-email`

**Mô tả:** Cập nhật email template.

**Authorization:** Required (Role: Admin)

**Request Body:**
```json
{
  "id": "guid",
  "header": "New Header",
  "mainContent": "New Main Content",
  "content": "New Full Content"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "header": "New Header",
    "mainContent": "New Main Content",
    "content": "New Full Content"
  }
}
```

---

## 3. Program APIs

Base URL: `/api/programs`

### 3.1. Create Program

**Endpoint:** `POST /api/programs`

**Mô tả:** Tạo một Program mới. Program mới tạo sẽ có `IsActive = false` mặc định, cần duyệt qua API toggle-status để kích hoạt.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Tiếng Anh Trẻ Em",
  "level": "Beginner",
  "totalSessions": 30,
  "defaultTuitionAmount": 5000000,
  "unitPriceSession": 166667,
  "description": "Chương trình học tiếng Anh cho trẻ em"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Tiếng Anh Trẻ Em",
    "level": "Beginner",
    "totalSessions": 30,
    "defaultTuitionAmount": 5000000,
    "unitPriceSession": 166667,
    "description": "Chương trình học tiếng Anh cho trẻ em",
    "isActive": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/programs/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Branch không tồn tại hoặc không active
- `409 Conflict`: Tên Program đã tồn tại

---

### 3.2. Get Programs

**Endpoint:** `GET /api/programs`

**Mô tả:** Lấy danh sách Programs với phân trang và filter. Kết quả được sắp xếp theo `CreatedAt` giảm dần (mới nhất trước).

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Program
- `isActive` (bool?, optional): Lọc theo trạng thái active
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/programs?branchId=11111111-1111-1111-1111-111111111111&isActive=true&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "name": "Tiếng Anh Trẻ Em",
        "level": "Beginner",
        "totalSessions": 30,
        "defaultTuitionAmount": 5000000,
        "unitPriceSession": 166667,
        "description": "Chương trình học tiếng Anh cho trẻ em",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 3.3. Get Active Programs

**Endpoint:** `GET /api/programs/active`

**Mô tả:** Lấy danh sách tất cả Programs đang active (IsActive = true). Không cần authorization.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Program
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Tương tự như Get Programs, nhưng chỉ trả về Programs có `isActive = true`

---

### 3.4. Get Program By ID

**Endpoint:** `GET /api/programs/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Program.

**Authorization:** Không cần (Public)

**Path Parameters:**
- `id` (Guid): ID của Program

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Tiếng Anh Trẻ Em",
    "level": "Beginner",
    "totalSessions": 30,
    "defaultTuitionAmount": 5000000,
    "unitPriceSession": 166667,
    "description": "Chương trình học tiếng Anh cho trẻ em",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại

---

### 3.5. Update Program

**Endpoint:** `PUT /api/programs/{id}`

**Mô tả:** Cập nhật thông tin Program.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Program

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Tiếng Anh Trẻ Em - Updated",
  "level": "Intermediate",
  "totalSessions": 40,
  "defaultTuitionAmount": 6000000,
  "unitPriceSession": 150000,
  "description": "Chương trình học tiếng Anh cho trẻ em - Updated"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Tiếng Anh Trẻ Em - Updated",
    "level": "Intermediate",
    "totalSessions": 40,
    "defaultTuitionAmount": 6000000,
    "unitPriceSession": 150000,
    "description": "Chương trình học tiếng Anh cho trẻ em - Updated",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-02T00:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại
- `409 Conflict`: Tên Program đã tồn tại (nếu đổi tên)

---

### 3.6. Delete Program

**Endpoint:** `DELETE /api/programs/{id}`

**Mô tả:** Xóa mềm Program (set IsDeleted = true, IsActive = false).

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Program

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại
- `409 Conflict`: Program đang có Tuition Plans liên quan

---

### 3.7. Toggle Program Status

**Endpoint:** `PATCH /api/programs/{id}/toggle-status`

**Mô tả:** Kích hoạt/Vô hiệu hóa Program (toggle IsActive).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Program

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": true
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại hoặc đã bị xóa

---

## 4. Tuition Plan APIs

Base URL: `/api/tuition-plans`

### 4.1. Create Tuition Plan

**Endpoint:** `POST /api/tuition-plans`

**Mô tả:** Tạo một Tuition Plan mới. Tuition Plan mới tạo sẽ có `IsActive = false` mặc định. Nếu `branchId` là `null`, Tuition Plan sẽ áp dụng cho tất cả các chi nhánh.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": null,
  "programId": "12345678-1234-1234-1234-123456789012",
  "name": "Gói 30 buổi - Khuyến mãi",
  "totalSessions": 30,
  "tuitionAmount": 4500000,
  "unitPriceSession": 150000,
  "currency": "VND"
}
```

**Lưu ý:** Để tạo Tuition Plan cho tất cả chi nhánh, gửi `branchId: null` hoặc không gửi field này.

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": null,
    "programId": "12345678-1234-1234-1234-123456789012",
    "name": "Gói 30 buổi - Khuyến mãi",
    "totalSessions": 30,
    "tuitionAmount": 4500000,
    "unitPriceSession": 150000,
    "currency": "VND",
    "isActive": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/tuition-plans/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Program không tồn tại hoặc không active

---

### 4.2. Get Tuition Plans

**Endpoint:** `GET /api/tuition-plans`

**Mô tả:** Lấy danh sách Tuition Plans với phân trang và filter. Kết quả được sắp xếp theo `CreatedAt` giảm dần. Nếu filter theo `branchId`, sẽ trả về cả Tuition Plans của branch đó và Tuition Plans global (branchId = null).

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID (sẽ bao gồm cả global plans)
- `programId` (Guid?, optional): Lọc theo Program ID
- `isActive` (bool?, optional): Lọc theo trạng thái active
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/tuition-plans?branchId=11111111-1111-1111-1111-111111111111&programId=12345678-1234-1234-1234-123456789012&isActive=true
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": null,
        "programId": "12345678-1234-1234-1234-123456789012",
        "name": "Gói 30 buổi - Khuyến mãi",
        "totalSessions": 30,
        "tuitionAmount": 4500000,
        "unitPriceSession": 150000,
        "currency": "VND",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 4.3. Get Active Tuition Plans

**Endpoint:** `GET /api/tuition-plans/active`

**Mô tả:** Lấy danh sách tất cả Tuition Plans đang active (IsActive = true). Không cần authorization.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `programId` (Guid?, optional): Lọc theo Program ID
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Tương tự như Get Tuition Plans, nhưng chỉ trả về Tuition Plans có `isActive = true`

---

### 4.4. Get Tuition Plan By ID

**Endpoint:** `GET /api/tuition-plans/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Tuition Plan.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": null,
    "programId": "12345678-1234-1234-1234-123456789012",
    "name": "Gói 30 buổi - Khuyến mãi",
    "totalSessions": 30,
    "tuitionAmount": 4500000,
    "unitPriceSession": 150000,
    "currency": "VND",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### 4.5. Update Tuition Plan

**Endpoint:** `PUT /api/tuition-plans/{id}`

**Mô tả:** Cập nhật thông tin Tuition Plan.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Request Body:**
```json
{
  "branchId": null,
  "programId": "12345678-1234-1234-1234-123456789012",
  "name": "Gói 30 buổi - Khuyến mãi - Updated",
  "totalSessions": 30,
  "tuitionAmount": 4000000,
  "unitPriceSession": 133333,
  "currency": "VND"
}
```

**Response (200 OK):** Tương tự như Create Tuition Plan

---

### 4.6. Delete Tuition Plan

**Endpoint:** `DELETE /api/tuition-plans/{id}`

**Mô tả:** Xóa mềm Tuition Plan (set IsDeleted = true, IsActive = false).

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Tuition Plan không tồn tại
- `409 Conflict`: Tuition Plan đang có enrollments liên quan

---

### 4.7. Toggle Tuition Plan Status

**Endpoint:** `PATCH /api/tuition-plans/{id}/toggle-status`

**Mô tả:** Kích hoạt/Vô hiệu hóa Tuition Plan (toggle IsActive).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": true
  }
}
```

---

## 5. Classroom APIs

Base URL: `/api/classrooms`

### 5.1. Create Classroom

**Endpoint:** `POST /api/classrooms`

**Mô tả:** Tạo một Classroom mới. Classroom mới tạo sẽ có `IsActive = false` mặc định.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Phòng học A1",
  "capacity": 20,
  "note": "Phòng học có máy lạnh và máy chiếu"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Phòng học A1",
    "capacity": 20,
    "note": "Phòng học có máy lạnh và máy chiếu",
    "isActive": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/classrooms/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Branch không tồn tại hoặc không active

---

### 5.2. Get Classrooms

**Endpoint:** `GET /api/classrooms`

**Mô tả:** Lấy danh sách Classrooms với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Classroom
- `isActive` (bool?, optional): Lọc theo trạng thái active
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "name": "Phòng học A1",
        "capacity": 20,
        "note": "Phòng học có máy lạnh và máy chiếu",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 5.3. Get Active Classrooms

**Endpoint:** `GET /api/classrooms/active`

**Mô tả:** Lấy danh sách tất cả Classrooms đang active (IsActive = true). Không cần authorization.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Classroom
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Tương tự như Get Classrooms, nhưng chỉ trả về Classrooms có `isActive = true`

---

### 5.4. Get Classroom By ID

**Endpoint:** `GET /api/classrooms/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Classroom.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Phòng học A1",
    "capacity": 20,
    "note": "Phòng học có máy lạnh và máy chiếu",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### 5.5. Update Classroom

**Endpoint:** `PUT /api/classrooms/{id}`

**Mô tả:** Cập nhật thông tin Classroom.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Phòng học A1 - Updated",
  "capacity": 25,
  "note": "Phòng học có máy lạnh, máy chiếu và bảng thông minh"
}
```

**Response (200 OK):** Tương tự như Create Classroom

---

### 5.6. Delete Classroom

**Endpoint:** `DELETE /api/classrooms/{id}`

**Mô tả:** Xóa cứng Classroom (hard delete). Chỉ xóa được nếu Classroom không có active sessions.

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Classroom không tồn tại
- `409 Conflict`: Classroom đang có active sessions

---

### 5.7. Toggle Classroom Status

**Endpoint:** `PATCH /api/classrooms/{id}/toggle-status`

**Mô tả:** Kích hoạt/Vô hiệu hóa Classroom (toggle IsActive).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": true
  }
}
```

---

## 6. Class APIs

Base URL: `/api/classes`

### 6.1. Create Class

**Endpoint:** `POST /api/classes`

**Mô tả:** Tạo một Class mới. Class mới tạo sẽ có `Status = Planned` mặc định. **Lưu ý:** Main Teacher và Assistant Teacher phải thuộc cùng branch với Class.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "programId": "12345678-1234-1234-1234-123456789012",
  "code": "ENG2-2024",
  "title": "Tiếng Anh Lớp 2 - 2024",
  "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "startDate": "2024-01-15",
  "endDate": "2024-04-15",
  "capacity": 15,
  "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "programId": "12345678-1234-1234-1234-123456789012",
    "code": "ENG2-2024",
    "title": "Tiếng Anh Lớp 2 - 2024",
    "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "startDate": "2024-01-15",
    "endDate": "2024-04-15",
    "status": "Planned",
    "capacity": 15,
    "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
  }
}
```

**Headers:**
- `Location: /api/classes/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Branch, Program, hoặc Teacher không tồn tại
- `409 Conflict`: 
  - Class code đã tồn tại
  - Teacher không thuộc cùng branch với Class

---

### 6.2. Get Classes

**Endpoint:** `GET /api/classes`

**Mô tả:** Lấy danh sách Classes với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `programId` (Guid?, optional): Lọc theo Program ID
- `status` (string?, optional): Lọc theo status (Planned, Active, Closed)
- `searchTerm` (string?, optional): Tìm kiếm theo class code hoặc title
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/classes?branchId=11111111-1111-1111-1111-111111111111&status=Active&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "programId": "12345678-1234-1234-1234-123456789012",
        "code": "ENG2-2024",
        "title": "Tiếng Anh Lớp 2 - 2024",
        "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "mainTeacherName": "Nguyễn Văn A",
        "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "assistantTeacherName": "Trần Thị B",
        "startDate": "2024-01-15",
        "endDate": "2024-04-15",
        "status": "Active",
        "capacity": 15,
        "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 6.3. Get Class By ID

**Endpoint:** `GET /api/classes/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Class.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "programId": "12345678-1234-1234-1234-123456789012",
    "code": "ENG2-2024",
    "title": "Tiếng Anh Lớp 2 - 2024",
    "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "mainTeacherName": "Nguyễn Văn A",
    "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "assistantTeacherName": "Trần Thị B",
    "startDate": "2024-01-15",
    "endDate": "2024-04-15",
    "status": "Active",
    "capacity": 15,
    "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
  }
}
```

---

### 6.4. Update Class

**Endpoint:** `PUT /api/classes/{id}`

**Mô tả:** Cập nhật thông tin Class. **Lưu ý:** Main Teacher và Assistant Teacher phải thuộc cùng branch với Class.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "programId": "12345678-1234-1234-1234-123456789012",
  "code": "ENG2-2024-UPDATED",
  "title": "Tiếng Anh Lớp 2 - 2024 - Updated",
  "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "startDate": "2024-01-20",
  "endDate": "2024-04-20",
  "capacity": 20,
  "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=19;BYMINUTE=0"
}
```

**Response (200 OK):** Tương tự như Create Class

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class, Branch, Program, hoặc Teacher không tồn tại
- `409 Conflict`: 
  - Class code đã tồn tại (nếu đổi code)
  - Teacher không thuộc cùng branch với Class

---

### 6.5. Delete Class

**Endpoint:** `DELETE /api/classes/{id}`

**Mô tả:** Xóa mềm Class bằng cách set `Status = Closed`. Chỉ xóa được nếu Class không có active enrollments.

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Class

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class không tồn tại
- `409 Conflict`: Class đang có active enrollments

---

### 6.6. Change Class Status

**Endpoint:** `PATCH /api/classes/{id}/status`

**Mô tả:** Thay đổi trạng thái Class (Planned → Active → Closed). Không thể chuyển từ Closed về Planned.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Request Body:**
```json
{
  "status": "Active"
}
```

**Status Values:**
- `"Planned"`: Class đã được lên kế hoạch
- `"Active"`: Class đang hoạt động
- `"Closed"`: Class đã đóng

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Active"
  }
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Không thể chuyển từ Closed về Planned
- `404 Not Found`: Class không tồn tại

---

### 6.7. Assign Teacher

**Endpoint:** `PATCH /api/classes/{id}/assign-teacher`

**Mô tả:** Gán Main Teacher và/hoặc Assistant Teacher cho Class. **Lưu ý:** Teacher phải thuộc cùng branch với Class.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Request Body:**
```json
{
  "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
}
```

**Lưu ý:** Có thể chỉ gán `mainTeacherId` hoặc chỉ `assistantTeacherId`, hoặc cả hai. Để xóa teacher, gửi `null`.

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "classId": "guid",
    "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "mainTeacherName": "Nguyễn Văn A",
    "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "assistantTeacherName": "Trần Thị B"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class hoặc Teacher không tồn tại
- `409 Conflict`: Teacher không thuộc cùng branch với Class

---

### 6.8. Check Class Capacity

**Endpoint:** `GET /api/classes/{id}/capacity`

**Mô tả:** Kiểm tra capacity của Class (số lượng học sinh hiện tại và capacity tối đa).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "classId": "guid",
    "capacity": 15,
    "currentEnrollmentCount": 10,
    "availableSlots": 5
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class không tồn tại

---

## 7. Enrollment APIs

Base URL: `/api/enrollments`

### 7.1. Create Enrollment

**Endpoint:** `POST /api/enrollments`

**Mô tả:** Ghi danh học sinh vào Class. Enrollment mới tạo sẽ có `Status = Active` mặc định.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "classId": "guid",
  "studentProfileId": "guid",
  "enrollDate": "2024-01-15",
  "tuitionPlanId": "guid"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn A",
    "enrollDate": "2024-01-15",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Gói 30 buổi",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/enrollments/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Class, Student Profile, hoặc Tuition Plan không tồn tại
- `409 Conflict`: 
  - Học sinh đã được ghi danh vào Class này
  - Class đã đầy (không còn slot)
  - Tuition Plan không thuộc Program của Class

---

### 7.2. Get Enrollments

**Endpoint:** `GET /api/enrollments`

**Mô tả:** Xem danh sách học sinh trong Class với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `classId` (Guid?, optional): Lọc theo Class ID
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile ID
- `status` (string?, optional): Lọc theo status (Active, Paused, Dropped)
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/enrollments?classId=guid&status=Active&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "studentProfileId": "guid",
        "studentName": "Nguyễn Văn A",
        "enrollDate": "2024-01-15",
        "status": "Active",
        "tuitionPlanId": "guid",
        "tuitionPlanName": "Gói 30 buổi",
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-15T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 7.3. Get Enrollment By ID

**Endpoint:** `GET /api/enrollments/{id}`

**Mô tả:** Xem chi tiết Enrollment.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn A",
    "enrollDate": "2024-01-15",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Gói 30 buổi",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại

---

### 7.4. Update Enrollment

**Endpoint:** `PUT /api/enrollments/{id}`

**Mô tả:** Cập nhật thông tin Enrollment (chủ yếu là EnrollDate và TuitionPlanId).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Request Body:**
```json
{
  "enrollDate": "2024-01-20",
  "tuitionPlanId": "guid"
}
```

**Response (200 OK):** Tương tự như Get Enrollment By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment hoặc Tuition Plan không tồn tại
- `409 Conflict`: Tuition Plan không thuộc Program của Class

---

### 7.5. Pause Enrollment

**Endpoint:** `PATCH /api/enrollments/{id}/pause`

**Mô tả:** Tạm dừng Enrollment (chuyển status từ Active → Paused).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Paused",
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại
- `409 Conflict`: Enrollment không ở trạng thái Active

---

### 7.6. Drop Enrollment

**Endpoint:** `PATCH /api/enrollments/{id}/drop`

**Mô tả:** Hủy Enrollment (chuyển status → Dropped).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Dropped",
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại

---

### 7.7. Reactivate Enrollment

**Endpoint:** `PATCH /api/enrollments/{id}/reactivate`

**Mô tả:** Kích hoạt lại Enrollment (chuyển status từ Paused → Active). Kiểm tra capacity trước khi reactivate.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Active",
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại
- `409 Conflict`: 
  - Enrollment không ở trạng thái Paused
  - Class đã đầy (không còn slot)

---

### 7.8. Assign Tuition Plan

**Endpoint:** `PATCH /api/enrollments/{id}/assign-tuition-plan`

**Mô tả:** Gán Tuition Plan cho Enrollment.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Request Body:**
```json
{
  "tuitionPlanId": "guid"
}
```

**Response (200 OK):** Tương tự như Get Enrollment By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment hoặc Tuition Plan không tồn tại
- `409 Conflict`: 
  - Tuition Plan không active
  - Tuition Plan không thuộc Program của Class

---

### 7.9. Get Student Enrollment History

**Endpoint:** `GET /api/enrollments/student/{studentProfileId}`

**Mô tả:** Xem lịch sử Enrollment của học sinh.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `studentProfileId` (Guid): ID của Student Profile

**Query Parameters:**
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "enrollDate": "2024-01-15",
        "status": "Active",
        "tuitionPlanId": "guid",
        "tuitionPlanName": "Gói 30 buổi",
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-15T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

## 8. Ticket APIs

Base URL: `/api/tickets`

### 8.1. Create Ticket

**Endpoint:** `POST /api/tickets`

**Mô tả:** Parent/Student tạo Ticket. Ticket mới tạo sẽ có `Status = Open` mặc định.

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "openedByUserId": "guid",
  "openedByProfileId": "guid",
  "branchId": "guid",
  "classId": "guid",
  "category": "Homework",
  "message": "Tôi cần hỗ trợ về bài tập về nhà"
}
```

**Category Values:**
- `"Homework"`: Vấn đề về bài tập
- `"Finance"`: Vấn đề về tài chính
- `"Schedule"`: Vấn đề về lịch học
- `"Tech"`: Vấn đề kỹ thuật

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "openedByUserId": "guid",
    "openedByUserName": "Nguyễn Văn A",
    "openedByProfileId": "guid",
    "openedByProfileName": "Phụ huynh Nguyễn Văn A",
    "branchId": "guid",
    "branchName": "Chi nhánh Quận 1",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "category": "Homework",
    "message": "Tôi cần hỗ trợ về bài tập về nhà",
    "status": "Open",
    "assignedToUserId": null,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/tickets/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: User, Branch, Class, hoặc Profile không tồn tại

---

### 8.2. Get Tickets

**Endpoint:** `GET /api/tickets`

**Mô tả:** Xem danh sách Tickets với phân trang và filter.

**Authorization:** Required (Bearer Token)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `openedByUserId` (Guid?, optional): Lọc theo người tạo
- `assignedToUserId` (Guid?, optional): Lọc theo người được gán
- `status` (string?, optional): Lọc theo status (Open, InProgress, Resolved, Closed)
- `category` (string?, optional): Lọc theo category (Homework, Finance, Schedule, Tech)
- `classId` (Guid?, optional): Lọc theo Class ID
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/tickets?status=Open&category=Homework&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "openedByUserId": "guid",
        "openedByUserName": "Nguyễn Văn A",
        "openedByProfileId": "guid",
        "openedByProfileName": "Phụ huynh Nguyễn Văn A",
        "branchId": "guid",
        "branchName": "Chi nhánh Quận 1",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "category": "Homework",
        "message": "Tôi cần hỗ trợ về bài tập về nhà",
        "status": "Open",
        "assignedToUserId": null,
        "assignedToUserName": null,
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-15T10:00:00Z",
        "commentCount": 0
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 8.3. Get Ticket By ID

**Endpoint:** `GET /api/tickets/{id}`

**Mô tả:** Xem chi tiết Ticket bao gồm tất cả comments.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "openedByUserId": "guid",
    "openedByUserName": "Nguyễn Văn A",
    "openedByProfileId": "guid",
    "openedByProfileName": "Phụ huynh Nguyễn Văn A",
    "branchId": "guid",
    "branchName": "Chi nhánh Quận 1",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "category": "Homework",
    "message": "Tôi cần hỗ trợ về bài tập về nhà",
    "status": "InProgress",
    "assignedToUserId": "guid",
    "assignedToUserName": "Staff Nguyễn Thị B",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T11:00:00Z",
    "comments": [
      {
        "id": "guid",
        "commenterUserId": "guid",
        "commenterUserName": "Staff Nguyễn Thị B",
        "commenterProfileId": null,
        "commenterProfileName": null,
        "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
        "attachmentUrl": null,
        "createdAt": "2024-01-15T11:00:00Z"
      }
    ]
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket không tồn tại

---

### 8.4. Assign Ticket

**Endpoint:** `PATCH /api/tickets/{id}/assign`

**Mô tả:** Gán Ticket cho Staff/Teacher. Tự động chuyển status từ Open → InProgress.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Request Body:**
```json
{
  "assignedToUserId": "guid"
}
```

**Response (200 OK):** Tương tự như Get Ticket By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket hoặc User không tồn tại
- `409 Conflict`: 
  - User không phải Staff/Teacher
  - User không thuộc cùng branch với Ticket

---

### 8.5. Update Ticket Status

**Endpoint:** `PATCH /api/tickets/{id}/status`

**Mô tả:** Cập nhật trạng thái Ticket.

**Authorization:** Required (Roles: Admin, Staff, Teacher)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Request Body:**
```json
{
  "status": "Resolved"
}
```

**Status Values:**
- `"Open"`: Ticket mới tạo
- `"InProgress"`: Đang xử lý
- `"Resolved"`: Đã giải quyết
- `"Closed"`: Đã đóng

**Response (200 OK):** Tương tự như Get Ticket By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket không tồn tại

---

### 8.6. Add Ticket Comment

**Endpoint:** `POST /api/tickets/{id}/comments`

**Mô tả:** Thêm comment vào Ticket. Nếu Staff/Teacher comment vào ticket Open, tự động chuyển status → InProgress.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Request Body:**
```json
{
  "commenterUserId": "guid",
  "commenterProfileId": "guid",
  "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
  "attachmentUrl": "https://res.cloudinary.com/..."
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "ticketId": "guid",
    "commenterUserId": "guid",
    "commenterUserName": "Staff Nguyễn Thị B",
    "commenterProfileId": null,
    "commenterProfileName": null,
    "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
    "attachmentUrl": null,
    "createdAt": "2024-01-15T11:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/tickets/{id}/comments/{commentId}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Ticket, User, hoặc Profile không tồn tại
- `409 Conflict`: Ticket đã bị Closed

---

### 8.7. Get Ticket History

**Endpoint:** `GET /api/tickets/{id}/history`

**Mô tả:** Xem lịch sử Ticket (timeline: Created, StatusChanged, Assigned, Comments).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "ticketId": "guid",
    "ticketStatus": "InProgress",
    "history": [
      {
        "type": "Created",
        "commentId": null,
        "commenterUserId": "guid",
        "commenterUserName": "Nguyễn Văn A",
        "commenterProfileId": "guid",
        "commenterProfileName": "Phụ huynh Nguyễn Văn A",
        "message": "Ticket created: Tôi cần hỗ trợ về bài tập về nhà",
        "attachmentUrl": null,
        "timestamp": "2024-01-15T10:00:00Z"
      },
      {
        "type": "Assigned",
        "commentId": null,
        "commenterUserId": "guid",
        "commenterUserName": "Staff Nguyễn Thị B",
        "commenterProfileId": null,
        "commenterProfileName": null,
        "message": "Ticket assigned to Staff Nguyễn Thị B",
        "attachmentUrl": null,
        "timestamp": "2024-01-15T10:30:00Z"
      },
      {
        "type": "Comment",
        "commentId": "guid",
        "commenterUserId": "guid",
        "commenterUserName": "Staff Nguyễn Thị B",
        "commenterProfileId": null,
        "commenterProfileName": null,
        "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
        "attachmentUrl": null,
        "timestamp": "2024-01-15T11:00:00Z"
      }
    ]
  }
}
```

---

### 8.8. Get Ticket SLA

**Endpoint:** `GET /api/tickets/{id}/sla`

**Mô tả:** Theo dõi SLA phản hồi Ticket (first response time, SLA compliance).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "ticketId": "guid",
    "ticketStatus": "InProgress",
    "createdAt": "2024-01-15T10:00:00Z",
    "firstResponseAt": "2024-01-15T11:00:00Z",
    "timeToFirstResponseHours": 1.0,
    "slaTargetHours": 24,
    "isSLACompliant": true,
    "isSLAOverdue": false,
    "totalComments": 3,
    "staffCommentCount": 2
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket không tồn tại

---

## 9. Blog APIs

Base URL: `/api/blogs`

### 9.1. Create Blog

**Endpoint:** `POST /api/blogs`

**Mô tả:** Admin/Staff tạo Blog Post. Blog mới tạo sẽ có `IsPublished = false` mặc định. `CreatedBy` tự động lấy từ JWT token.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "title": "10 Cách Học Tiếng Anh Hiệu Quả",
  "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
  "content": "<p>Nội dung blog post...</p>",
  "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
    "content": "<p>Nội dung blog post...</p>",
    "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
    "createdBy": "guid",
    "createdByName": "Admin Nguyễn Văn A",
    "isPublished": false,
    "isDeleted": false,
    "publishedAt": null,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/blogs/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: User không tồn tại hoặc không phải Admin/Staff

---

### 9.2. Get Blogs

**Endpoint:** `GET /api/blogs`

**Mô tả:** Xem danh sách Blog Posts với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `createdBy` (Guid?, optional): Lọc theo creator user ID
- `isPublished` (bool?, optional): Lọc theo published status
- `includeDeleted` (bool?, optional): Include deleted blogs (default: false)
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/blogs?isPublished=true&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "10 Cách Học Tiếng Anh Hiệu Quả",
        "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
        "content": "<p>Nội dung blog post...</p>",
        "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
        "createdBy": "guid",
        "createdByName": "Admin Nguyễn Văn A",
        "isPublished": true,
        "isDeleted": false,
        "publishedAt": "2024-01-16T10:00:00Z",
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-16T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 9.3. Get Blog By ID

**Endpoint:** `GET /api/blogs/{id}`

**Mô tả:** Xem chi tiết Blog Post.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
    "content": "<p>Nội dung blog post...</p>",
    "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
    "createdBy": "guid",
    "createdByName": "Admin Nguyễn Văn A",
    "isPublished": true,
    "isDeleted": false,
    "publishedAt": "2024-01-16T10:00:00Z",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-16T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại

---

### 9.4. Update Blog

**Endpoint:** `PUT /api/blogs/{id}`

**Mô tả:** Cập nhật Blog Post.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Request Body:**
```json
{
  "title": "10 Cách Học Tiếng Anh Hiệu Quả - Updated",
  "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em - Updated",
  "content": "<p>Nội dung blog post đã được cập nhật...</p>",
  "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image-updated.jpg"
}
```

**Response (200 OK):** Tương tự như Get Blog By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: Blog đã bị xóa

---

### 9.5. Delete Blog

**Endpoint:** `DELETE /api/blogs/{id}`

**Mô tả:** Xóa mềm Blog Post (set IsDeleted = true).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "isDeleted": true,
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: Blog đã bị xóa

---

### 9.6. Publish Blog

**Endpoint:** `PATCH /api/blogs/{id}/publish`

**Mô tả:** Publish Blog Post (set IsPublished = true, PublishedAt = DateTime.UtcNow).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "isPublished": true,
    "publishedAt": "2024-01-16T10:00:00Z",
    "updatedAt": "2024-01-16T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: 
  - Blog đã bị xóa
  - Blog đã được publish

---

### 9.7. Unpublish Blog

**Endpoint:** `PATCH /api/blogs/{id}/unpublish`

**Mô tả:** Unpublish Blog Post (set IsPublished = false).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "isPublished": false,
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: 
  - Blog đã bị xóa
  - Blog chưa được publish

---

### 9.8. Get Published Blogs

**Endpoint:** `GET /api/blogs/published`

**Mô tả:** Hiển thị Blog Post trên Landing Page. Public endpoint - không cần authentication.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "10 Cách Học Tiếng Anh Hiệu Quả",
        "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
        "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
        "createdBy": "guid",
        "createdByName": "Admin Nguyễn Văn A",
        "publishedAt": "2024-01-16T10:00:00Z",
        "createdAt": "2024-01-15T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

**Lưu ý:** Response không bao gồm `content` để tiết kiệm bandwidth. Sử dụng `GET /api/blogs/{id}` để lấy full content.

---

## 10. File Upload APIs

Base URL: `/api/files`

### 10.1. Upload File

**Endpoint:** `POST /api/files/upload`

**Mô tả:** Upload file (ảnh hoặc video) lên Cloudinary và trả về public URL. Hỗ trợ cho:
- Ticket attachments
- Blog featured images
- Media assets (Use Case 13)

**Authorization:** Required (Bearer Token)

**Query Parameters:**
- `folder` (string, optional): Folder name trong Cloudinary (default: "uploads")
  - Ví dụ: `tickets`, `media`, `blog`
- `resourceType` (string, optional): `image`, `video`, hoặc `auto` (default: `auto`)
  - `auto`: Tự động detect từ file extension

**Request Body:**
- `file` (FormData): File cần upload (multipart/form-data)

**File Size Limit:** 100MB

**Allowed File Types:**
- Images: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`, `.svg`
- Videos: `.mp4`, `.mov`, `.avi`, `.wmv`, `.flv`, `.webm`

**Example Request (cURL):**
```bash
curl -X 'POST' \
  'https://localhost:7235/api/files/upload?folder=blog&resourceType=auto' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer {token}' \
  -H 'Content-Type: multipart/form-data' \
  -F 'file=@image.jpg;type=image/jpeg'
```

**Response (200 OK):**
```json
{
  "url": "https://res.cloudinary.com/dbvjdx05r/image/upload/v1767628773/blog/388e480a-505a-44d6-a54a-0b9b99a03727.jpg",
  "fileName": "image.jpg",
  "size": 15203,
  "folder": "blog"
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: 
  - File không được cung cấp
  - File size vượt quá 100MB
  - File type không được phép
- `401 Unauthorized`: Chưa đăng nhập
- `500 Internal Server Error`: Lỗi upload lên Cloudinary

---

### 10.2. Delete File

**Endpoint:** `DELETE /api/files?url={publicUrl}`

**Mô tả:** Xóa file từ Cloudinary.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `url` (string, required): Public URL của file cần xóa

**Example Request:**
```
DELETE /api/files?url=https://res.cloudinary.com/dbvjdx05r/image/upload/v1767628773/blog/388e480a-505a-44d6-a54a-0b9b99a03727.jpg
```

**Response (200 OK):**
```json
{
  "message": "File deleted successfully"
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: URL không được cung cấp
- `404 Not Found`: File không tồn tại hoặc không thể xóa

---

### 10.3. Get Transformed URL

**Endpoint:** `GET /api/files/transform?url={publicUrl}&width=800&height=600&format=webp`

**Mô tả:** Tạo URL đã được transform (resize, format conversion) mà không cần upload lại.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `url` (string, required): Original public URL
- `width` (int?, optional): Desired width
- `height` (int?, optional): Desired height
- `format` (string?, optional): Desired format (`webp`, `jpg`, `png`)

**Example Request:**
```
GET /api/files/transform?url=https://res.cloudinary.com/.../image.jpg&width=800&height=600&format=webp
```

**Response (200 OK):**
```json
{
  "url": "https://res.cloudinary.com/.../c_limit,h_600,w_800,f_webp,q_auto/image.jpg"
}
```

---

## Lưu ý chung

### Authentication

Hầu hết các API yêu cầu authentication. Để sử dụng, bạn cần:

1. Đăng nhập qua `POST /api/auth/login` để lấy `accessToken`
2. Gửi `accessToken` trong header của mỗi request:
   ```
   Authorization: Bearer {accessToken}
   ```

### Phân quyền

- **Admin**: Có quyền truy cập tất cả các API
- **Staff**: Có quyền truy cập hầu hết các API, trừ một số API xóa (chỉ Admin)
- **Public**: Một số API không cần authentication (được đánh dấu `[AllowAnonymous]`)

### Response Format

Tất cả các API đều trả về response theo format:

```json
{
  "isSuccess": true,
  "data": { ... }
}
```

Khi có lỗi:

```json
{
  "isSuccess": false,
  "error": {
    "code": "Error.Code",
    "message": "Error message"
  }
}
```

### Pagination

Các API list đều hỗ trợ phân trang:

- `pageNumber`: Số trang (bắt đầu từ 1)
- `pageSize`: Số lượng items mỗi trang (mặc định: 10)

Response sẽ bao gồm:
- `items`: Danh sách items
- `pageNumber`: Số trang hiện tại
- `pageSize`: Số lượng items mỗi trang
- `totalCount`: Tổng số items
- `totalPages`: Tổng số trang

### Date Format

- Date fields sử dụng format: `YYYY-MM-DD` (ví dụ: `2024-01-15`)
- DateTime fields sử dụng format ISO 8601: `YYYY-MM-DDTHH:mm:ssZ` (ví dụ: `2024-01-01T00:00:00Z`)

### Validation Rules

1. **Teacher và Branch**: Teacher phải thuộc cùng branch với Class khi tạo/cập nhật/assign teacher
2. **Program và Branch**: Program phải thuộc branch được chỉ định
3. **Class Code**: Phải unique trong toàn hệ thống
4. **Status Transitions**: 
   - Class: Planned → Active → Closed (không thể quay lại)
   - Program/Tuition Plan/Classroom: Toggle IsActive

### Error Codes

- `400 Bad Request`: Dữ liệu không hợp lệ
- `401 Unauthorized`: Chưa đăng nhập hoặc token không hợp lệ
- `403 Forbidden`: Không có quyền truy cập
- `404 Not Found`: Resource không tồn tại
- `409 Conflict`: Xung đột dữ liệu (ví dụ: code đã tồn tại, có ràng buộc)

---

## Swagger UI

Bạn có thể truy cập Swagger UI để xem và test các API tại:
```
https://localhost:{port}/swagger
```

Trong Swagger UI, bạn có thể:
- Xem tất cả các endpoints
- Xem request/response schemas
- Test API trực tiếp
- Xem authorization requirements

---

*Tài liệu này được cập nhật lần cuối: 2025-01-05*

