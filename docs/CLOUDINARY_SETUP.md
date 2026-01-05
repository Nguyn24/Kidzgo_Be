# Cloudinary Setup Guide

## Tổng quan

Dự án sử dụng **Cloudinary** để lưu trữ và xử lý ảnh/video cho:
- **Use Case 13**: Media Management (ảnh/video của lớp học)
- **Use Case 17**: Ticket attachments
- **Use Case 18**: Blog featured images

## Tại sao chọn Cloudinary?

✅ **Hỗ trợ cả ảnh và video**  
✅ **Image/Video processing tự động** (resize, optimize, thumbnails)  
✅ **CDN tích hợp** (tốc độ tải nhanh)  
✅ **Free tier tốt**: 25GB storage, 25GB bandwidth/tháng  
✅ **Transform on-the-fly** (tạo thumbnail, resize theo yêu cầu)  
✅ **Dễ integrate với .NET**

## Bước 1: Đăng ký tài khoản Cloudinary

1. Truy cập: https://cloudinary.com/users/register/free
2. Đăng ký tài khoản miễn phí (Free tier)
3. Xác nhận email

## Bước 2: Lấy thông tin API

Sau khi đăng nhập vào Dashboard:

1. Vào **Settings** → **Security**
2. Copy các thông tin sau:
   - **Cloud Name**: Tên cloud của bạn (ví dụ: `dxyz123`)
   - **API Key**: Key để authenticate
   - **API Secret**: Secret key (giữ bí mật!)

## Bước 3: Cấu hình trong appsettings.json

Thêm vào `appsettings.Development.json` hoặc `appsettings.json`:

```json
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

**Lưu ý**: 
- Không commit `appsettings.Development.json` có chứa API Secret vào Git
- Sử dụng User Secrets hoặc Environment Variables cho production

## Bước 4: Sử dụng API Upload

### Endpoint Upload File

```
POST /api/files/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Query Parameters:**
- `folder` (optional): Folder name trong Cloudinary (default: "uploads")
  - Ví dụ: `tickets`, `media`, `blog`
- `resourceType` (optional): `image`, `video`, hoặc `auto` (default: `auto`)

**Request Body:**
- `file`: File cần upload (FormData)

**Response:**
```json
{
  "url": "https://res.cloudinary.com/your-cloud/image/upload/v1234567890/uploads/abc123.jpg",
  "fileName": "photo.jpg",
  "size": 1024000,
  "folder": "uploads"
}
```

### Ví dụ sử dụng:

**Upload ảnh cho Ticket:**
```bash
POST /api/files/upload?folder=tickets
Content-Type: multipart/form-data
file: [binary]
```

**Upload video cho Media:**
```bash
POST /api/files/upload?folder=media&resourceType=video
Content-Type: multipart/form-data
file: [binary]
```

**Upload featured image cho Blog:**
```bash
POST /api/files/upload?folder=blog
Content-Type: multipart/form-data
file: [binary]
```

### Endpoint Transform URL

```
GET /api/files/transform?url={publicUrl}&width=800&height=600&format=webp
```

Tạo URL đã được transform (resize, format conversion) mà không cần upload lại.

### Endpoint Delete File

```
DELETE /api/files?url={publicUrl}
Authorization: Bearer {token}
Roles: Admin, Staff
```

## Cấu trúc Folder trong Cloudinary

- `tickets/` - Attachments cho tickets
- `media/` - Ảnh/video của lớp học (Use Case 13)
- `blog/` - Featured images cho blog posts (Use Case 18)
- `uploads/` - Default folder

## Tính năng tự động

### Image Optimization:
- Tự động tạo 3 sizes: 1920x1080, 800x600, 400x300
- Auto quality optimization
- Format conversion (WebP khi có thể)

### Video Optimization:
- Tự động tạo 2 sizes: 1280x720, 640x360
- Video compression

## Pricing

**Free Tier:**
- 25GB storage
- 25GB bandwidth/tháng
- 25,000 monthly transformations

**Paid Plans:**
- Bắt đầu từ $89/tháng (Advanced plan)
- Unlimited storage và bandwidth
- Priority support

## Security Best Practices

1. ✅ **Không commit API Secret vào Git**
2. ✅ **Sử dụng Environment Variables cho production**
3. ✅ **Giới hạn upload size** (hiện tại: 100MB)
4. ✅ **Validate file types** (chỉ cho phép image/video)
5. ✅ **Require authentication** cho upload endpoint

## Troubleshooting

### Lỗi: "Cloudinary configuration is missing"
→ Kiểm tra lại `appsettings.json` có đầy đủ 3 thông tin: CloudName, ApiKey, ApiSecret

### Lỗi: "Invalid API credentials"
→ Kiểm tra lại API Key và API Secret trong Cloudinary Dashboard

### File upload thành công nhưng không thấy trong Dashboard
→ Kiểm tra folder name và public ID trong response URL

## Tài liệu tham khảo

- Cloudinary .NET SDK: https://cloudinary.com/documentation/dotnet_integration
- Cloudinary API Reference: https://cloudinary.com/documentation/image_upload_api_reference
- Transformation Reference: https://cloudinary.com/documentation/image_transformations

