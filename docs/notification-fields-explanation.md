# Notification Fields - Khi nào được tác động

## Các Field trong Notification:

### 1. **Status** (NotificationStatus)
**Enum:** `Pending`, `Sent`, `Failed`

**Khi nào được update:**
- **Pending** (mặc định): Khi tạo notification mới
- **Sent**: Khi gửi notification qua external channel (Zalo/Email/Push) thành công
- **Failed**: Khi gửi notification thất bại

**Hiện tại:**
- ✅ Tạo với `Status = Pending` (trong BroadcastNotificationCommandHandler)
- ❌ Chưa có logic để update `Sent` hoặc `Failed` (cần implement khi tích hợp Zalo/Email/Push)

---

### 2. **SentAt** (DateTime?)
**Khi nào được set:**
- Khi gửi notification qua external channel (Zalo/Email/Push) thành công
- Set = `DateTime.UtcNow` khi `Status` chuyển từ `Pending` → `Sent`

**Hiện tại:**
- ❌ Chưa được set (luôn `null`)
- ✅ Cần implement khi tích hợp Zalo/Email/Push

---

### 3. **TemplateId** (string?)
**Khi nào được set:**
- Khi tạo notification từ `NotificationTemplate` (UC-325)
- Nếu tạo notification thủ công (như hiện tại) → `null`

**Hiện tại:**
- ❌ Chưa được set (luôn `null`)
- ✅ Có thể implement sau khi có NotificationTemplate module

---

### 4. **ReadAt** (DateTime?)
**Khi nào được set:**
- Khi user đọc notification (click vào notification, mở detail, etc.)
- Set = `DateTime.UtcNow` khi user mark as read

**Hiện tại:**
- ❌ Chưa có endpoint để mark as read
- ✅ Chỉ đọc để check `IsRead` (trong GetNotificationsQueryHandler)
- ⚠️ **CẦN IMPLEMENT**: Endpoint `PATCH /api/notifications/{id}/read`

---

## Workflow mong muốn:

### 1. Tạo Notification (Broadcast):
```
Status = Pending
SentAt = null
TemplateId = null (hoặc set nếu dùng template)
ReadAt = null
```

### 2. Gửi Notification (External Channel - Zalo/Email/Push):
```
Status = Sent (nếu thành công)
SentAt = DateTime.UtcNow
HOẶC
Status = Failed (nếu thất bại)
SentAt = null
```

### 3. User đọc Notification:
```
ReadAt = DateTime.UtcNow
(Status và SentAt không đổi)
```

---

## Cần implement:

1. **Mark as Read endpoint:**
   - `PATCH /api/notifications/{id}/read`
   - Update `ReadAt = DateTime.UtcNow`

2. **Send Notification service:**
   - Background job/service để gửi notifications qua Zalo/Email/Push
   - Update `Status` và `SentAt` sau khi gửi

3. **NotificationTemplate integration:**
   - Khi tạo notification từ template → set `TemplateId`

