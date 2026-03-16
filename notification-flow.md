# Notification Flow Guide

## Overview

Tài liệu này mô tả flow xử lý notification trong hệ thống KidzGo.

## Flow Chart

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐
│   Client    │────▶│   Backend    │────▶│   Push Service  │
│  (Mobile/   │     │   (API)      │     │   (FCM/APNS)    │
│   Web)      │     │              │     │                 │
└─────────────┘     └──────────────┘     └─────────────────┘
       ▲                   │                      │
       │                   ▼                      │
       │            ┌──────────────┐              │
       └────────────│  Database    │◀─────────────┘
                    │  (Store      │
                    │   noti log)  │
                    └──────────────┘
```

## Các Loại Notification

### 1. Real-time Notification
- Gửi ngay khi có sự kiện xảy ra
- Ví dụ: tin nhắn mới, cập nhật trạng thái đơn hàng

### 2. Scheduled Notification (Quartz Job)
- Gửi theo lịch đã đặt trước
- Ví dụ: nhắc nhở, thông báo khuyến mãi
- Sử dụng `SendNotificationRemindersJob` với Quartz

### 3. Push Notification
- Gửi qua FCM (Firebase Cloud Messaging) cho Android/Web
- Gửi qua APNS (Apple Push Notification Service) cho iOS

## Backend Flow

### 1. Nhận Request
```
Client ──POST /notifications──▶ API Server
```

### 2. Validate & Process
- Kiểm tra quyền user
- Validate payload
- Lưu vào database

### 3. Gửi qua Push Service
- Gửi đến FCM/APNS
- Xử lý retry khi thất bại

## Database Schema

```sql
-- Notifications table
CREATE TABLE notifications (
    id UUID PRIMARY KEY,
    recipient_user_id UUID NOT NULL,
    recipient_profile_id UUID,
    channel VARCHAR(50) NOT NULL, -- Email, Push, Zalo
    title VARCHAR(255) NOT NULL,
    content TEXT,
    deeplink VARCHAR(500),
    status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, SENT, FAILED
    sent_at TIMESTAMP,
    read_at TIMESTAMP,
    template_id VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW(),
    
    -- Push notification metadata
    target_role VARCHAR(50), -- Role nhận: Parent, Student, Teacher, Staff
    kind VARCHAR(50), -- Loại: report, attendance, homework, message, payment
    priority VARCHAR(20), -- Độ ưu tiên: high, normal, low
    sender_role VARCHAR(50), -- Role gửi: System, Teacher, Admin, Staff
    sender_name VARCHAR(255) -- Tên người gửi/hệ thống
);

-- Device tokens table
CREATE TABLE device_tokens (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    token VARCHAR(500) NOT NULL,
    device_type VARCHAR(20), -- iOS, Android, Web
    device_id VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    last_used_at TIMESTAMP
);
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/notifications | Lấy danh sách notification |
| POST | /api/notifications/broadcast | Admin gửi broadcast notification |
| PATCH | /api/notifications/{id}/read | Đánh dấu đã đọc |
| POST | /api/notifications/{id}/retry | Retry notification thất bại |
| POST | /api/notifications/device-token | Đăng ký device token |
| DELETE | /api/notifications/device-token | Xóa device token (logout) |
| POST | /api/notifications/templates | Tạo notification template |
| GET | /api/notifications/templates | Lấy danh sách templates |
| PUT | /api/notifications/templates/{id} | Cập nhật template |
| DELETE | /api/notifications/templates/{id} | Xóa template |

## Payload Format

### Register Device Token (POST /api/notifications/device-token)
```json
{
  "token": "fcm-token-from-firebase",
  "deviceType": "Web", // iOS, Android, Web
  "deviceId": "optional-device-uuid"
}
```

### Delete Device Token (DELETE /api/notifications/device-token)
```json
{
  "token": "fcm-token-to-delete", // Hoặc
  "deviceId": "uuid-of-device"
}
```

### Broadcast Notification (POST /api/notifications/broadcast)
```json
{
  "title": "Thông báo",
  "content": "Nội dung thông báo",
  "deeplink": "/vi/portal/parent/notifications",
  "channel": "Push", // Email, Push, Zalo
  "role": "Parent", // Gửi theo role
  "branchId": "optional-branch-uuid",
  "classId": "optional-class-uuid",
  "userIds": ["uuid1", "uuid2"], // Hoặc gửi theo user cụ thể
  "profileIds": ["profile-uuid1"]
}
```

### Push Notification Payload Data
```json
{
  "title": "Báo cáo tháng đã công bố",
  "body": "Phụ huynh có thể xem monthly report mới nhất.",
  "targetRole": "Parent",
  "kind": "report",
  "priority": "high",
  "senderRole": "Teacher",
  "senderName": "KidzGo Centre",
  "link": "/vi/portal/parent/notifications",
  "notification_id": "uuid-here",
  "deeplink": "/vi/portal/parent/notifications"
}
```

## Error Handling

| Error Code | Description |
|------------|-------------|
| 400 | Invalid payload |
| 401 | Unauthorized |
| 404 | User not found |
| 500 | Internal server error |

## Firebase Configuration

### Backend (appsettings.json)
```json
{
  "FCM": {
    "ServiceAccountPath": "path/to/firebase-service-account.json",
    "ServiceAccountJson": null // Hoặc JSON string trực tiếp
  }
}
```

### Frontend (Next.js environment variables)
```
NEXT_PUBLIC_FIREBASE_API_KEY=your-api-key
NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN=your-project.firebaseapp.com
NEXT_PUBLIC_FIREBASE_PROJECT_ID=your-project-id
NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET=your-project.appspot.com
NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID=your-sender-id
NEXT_PUBLIC_FIREBASE_APP_ID=your-app-id
NEXT_PUBLIC_FIREBASE_VAPID_KEY=your-vapid-key
```

## Best Practices

1. **Batch Notifications**: Gom nhiều notification thành batch để giảm API calls (FCM hỗ trợ tối đa 500 tokens/batch)
2. **Retry Logic**: Implement exponential backoff khi gửi thất bại
3. **Rate Limiting**: Giới hạn số lượng notification gửi cho user trong 1 khoảng thời gian
4. **Logging**: Log đầy đủ để debug và theo dõi
5. **Unsubscribe**: Cho phép user tắt notification từng loại
6. **Device Token Management**: Xóa token khi user logout hoặc đổi thiết bị

## Security

- Sử dụng JWT để xác thực
- Validate tất cả input
- Không gửi sensitive data trong notification payload
- Sử dụng HTTPS cho tất cả API calls
