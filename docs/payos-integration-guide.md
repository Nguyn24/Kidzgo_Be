# Hướng dẫn tích hợp PayOS - Chi tiết

## 1. Lấy PayOS Credentials

### Bước 1: Đăng ký tài khoản PayOS
1. Truy cập: https://pay.payos.vn/
2. Đăng ký tài khoản doanh nghiệp
3. Xác thực thông tin doanh nghiệp (theo yêu cầu của PayOS)

### Bước 2: Lấy thông tin từ PayOS Dashboard
Sau khi đăng nhập vào PayOS Dashboard:

1. **Vào mục "Tích hợp" hoặc "Integration"**
2. **Lấy các thông tin sau:**

   - **Client ID** (`ClientId`):
     - Tìm trong phần "Thông tin tài khoản" hoặc "Account Information"
     - Đây là số ID của tài khoản PayOS của bạn
     - Ví dụ: `123456`

   - **API Key** (`ApiKey`):
     - Vào mục "API Keys" hoặc "Khóa API"
     - Tạo mới hoặc copy API Key hiện có
     - Đây là chuỗi secret key để authenticate API calls
     - Ví dụ: `a1b2c3d4-e5f6-7890-abcd-ef1234567890`

   - **Checksum Key** (`ChecksumKey`):
     - Cũng trong mục "API Keys" hoặc "Khóa API"
     - Đây là key để verify webhook signature
     - Ví dụ: `x1y2z3w4-v5u6-7890-t987-s654321fedcba`

### Bước 3: Cấu hình Webhook URL
Trong PayOS Dashboard:
1. Vào mục "Webhook" hoặc "Cài đặt Webhook"
2. Đặt Webhook URL: `https://your-backend-domain.com/webhooks/payos`
   - Ví dụ: `https://kidzgo-be.onrender.com/webhooks/payos`
3. Lưu lại để PayOS gọi về khi có thanh toán

---

## 2. Cấu hình trong Backend

### Bước 1: Thêm PayOS config vào `appsettings.json` hoặc Environment Variables

**Option 1: Thêm vào `appsettings.Production.json` (cho production):**

```json
{
  "PayOS": {
    "BaseUrl": "https://api.payos.vn",
    "ClientId": 123456,
    "ApiKey": "your-actual-api-key-from-payos-dashboard",
    "ChecksumKey": "your-actual-checksum-key-from-payos-dashboard",
    "ReturnUrl": "https://your-frontend-domain.com/invoices/{invoiceId}/success",
    "CancelUrl": "https://your-frontend-domain.com/invoices/{invoiceId}/cancel"
  }
}
```

**Lưu ý về `ReturnUrl` và `CancelUrl`:**
- **Về mặt PayOS API:** Có thể không bắt buộc về mặt kỹ thuật (tùy version API)
- **Về mặt UX:** **Rất quan trọng** - đây là URL để PayOS redirect user về sau khi thanh toán/hủy
- **Khuyến nghị:** Nên cấu hình cả 2 để user có trải nghiệm tốt:
  - `ReturnUrl`: User được redirect về đây sau khi thanh toán thành công
  - `CancelUrl`: User được redirect về đây nếu hủy thanh toán
- `{invoiceId}` sẽ được thay thế tự động bằng Invoice ID thực tế

**Option 2: Dùng Environment Variables (khuyến nghị cho production):**

Trên Render hoặc server, set các biến môi trường:
```bash
PayOS__ClientId=123456
PayOS__ApiKey=your-actual-api-key
PayOS__ChecksumKey=your-actual-checksum-key
PayOS__ReturnUrl=https://your-frontend-domain.com/invoices/{invoiceId}/success
PayOS__CancelUrl=https://your-frontend-domain.com/invoices/{invoiceId}/cancel
```

**Lưu ý về `ReturnUrl` và `CancelUrl`:**

- **Về mặt PayOS API:** Có thể không bắt buộc về mặt kỹ thuật (tùy version API của PayOS)
- **Về mặt UX:** **Rất quan trọng** - đây là URL để PayOS redirect user về sau khi thanh toán/hủy
  - Nếu không có `ReturnUrl`: User sẽ không biết quay về đâu sau khi thanh toán thành công
  - Nếu không có `CancelUrl`: User sẽ không biết quay về đâu nếu hủy thanh toán
- **Khuyến nghị:** Nên cấu hình cả 2 để user có trải nghiệm tốt
- `{invoiceId}` trong `ReturnUrl` và `CancelUrl` sẽ được thay thế tự động bằng Invoice ID thực tế khi tạo payment link
- Frontend URL phải là domain thật của bạn (không dùng localhost cho production)

**Nếu bạn không muốn dùng ReturnUrl/CancelUrl:**
- Có thể để giá trị rỗng hoặc URL tạm thời
- Nhưng user sẽ không có cách quay về frontend sau khi thanh toán/hủy
- PayOS sẽ chỉ hiển thị trang kết quả của họ

---

## 3. Luồng hoạt động chi tiết

### Luồng 1: Tạo Payment Link (Parent muốn thanh toán)

```
┌─────────┐         ┌──────────┐         ┌─────────┐         ┌─────────┐
│ Frontend│         │  Backend │         │  PayOS  │         │Database│
│ (Parent)│         │   API    │         │   API   │         │         │
└────┬────┘         └────┬─────┘         └────┬────┘         └────┬────┘
     │                    │                     │                    │
     │ 1. POST            │                     │                    │
     │ /api/invoices/{id} │                     │                    │
     │ /payos/create-link │                     │                    │
     │───────────────────>│                     │                    │
     │                    │                     │                    │
     │                    │ 2. Load Invoice     │                    │
     │                    │ from DB             │                    │
     │                    │────────────────────>│                    │
     │                    │<────────────────────│                    │
     │                    │                     │                    │
     │                    │ 3. Generate         │                    │
     │                    │ OrderCode           │                    │
     │                    │                     │                    │
     │                    │ 4. POST             │                    │
     │                    │ /v2/payment-requests│                    │
     │                    │────────────────────>│                    │
     │                    │                     │                    │
     │                    │ 5. Response:       │                    │
     │                    │ {checkoutUrl, qrCode}│                    │
     │                    │<────────────────────│                    │
     │                    │                     │                    │
     │                    │ 6. Save to DB:      │                    │
     │                    │ PayosOrderCode,    │                    │
     │                    │ PayosPaymentLink,   │                    │
     │                    │ PayosQr             │                    │
     │                    │────────────────────>│                    │
     │                    │                     │                    │
     │ 7. Response:       │                     │                    │
     │ {checkoutUrl,      │                     │                    │
     │  qrCodeData}       │                     │                    │
     │<───────────────────│                     │                    │
     │                    │                     │                    │
     │ 8. Redirect user   │                     │                    │
     │ to checkoutUrl     │                     │                    │
     │───────────────────────────────────────────>│                    │
     │                    │                     │                    │
```

**Chi tiết các bước:**

1. **Frontend gọi API:**
   ```http
   POST /api/invoices/{invoiceId}/payos/create-link
   Authorization: Bearer {token}
   ```

2. **Backend xử lý (`CreatePayOSLinkCommandHandler`):**
   - Load Invoice từ DB
   - Generate `orderCode` từ Invoice ID
   - Convert `Amount` từ thousands VND → VND (đồng)
   - Build items từ InvoiceLines
   - Gọi PayOS API: `POST /v2/payment-requests`
   - Lưu `PayosOrderCode`, `PayosPaymentLink`, `PayosQr` vào Invoice
   - Trả về `checkoutUrl` và `qrCodeData`

3. **Frontend nhận response:**
   ```json
   {
     "invoiceId": "guid",
     "checkoutUrl": "https://pay.payos.vn/web/...",
     "qrCodeData": "data:image/png;base64,..."
   }
   ```

4. **Frontend redirect user:**
   - Option 1: Redirect browser đến `checkoutUrl`
   - Option 2: Hiển thị QR code từ `qrCodeData` để user quét

---

### Luồng 2: Thanh toán và Webhook (PayOS gọi về Backend)

```
┌─────────┐         ┌──────────┐         ┌─────────┐         ┌─────────┐
│  PayOS  │         │  Backend │         │Database│         │Frontend │
│ Gateway │         │   API    │         │         │         │(Parent) │
└────┬────┘         └────┬─────┘         └────┬────┘         └────┬────┘
     │                    │                     │                    │
     │ User thanh toán    │                     │                    │
     │ trên PayOS        │                     │                    │
     │                    │                     │                    │
     │ 1. POST            │                     │                    │
     │ /webhooks/payos    │                     │                    │
     │ (với signature)    │                     │                    │
     │───────────────────>│                     │                    │
     │                    │                     │                    │
     │                    │ 2. Verify           │                    │
     │                    │ Signature           │                    │
     │                    │                     │                    │
     │                    │ 3. Find Invoice     │                    │
     │                    │ by OrderCode        │                    │
     │                    │────────────────────>│                    │
     │                    │<────────────────────│                    │
     │                    │                     │                    │
     │                    │ 4. Check            │                    │
     │                    │ Idempotency         │                    │
     │                    │                     │                    │
     │                    │ 5. Create Payment   │                    │
     │                    │ record              │                    │
     │                    │────────────────────>│                    │
     │                    │                     │                    │
     │                    │ 6. Update Invoice   │                    │
     │                    │ Status = Paid       │                    │
     │                    │────────────────────>│                    │
     │                    │                     │                    │
     │                    │ 7. (Optional)       │                    │
     │                    │ Create CashbookEntry│                    │
     │                    │────────────────────>│                    │
     │                    │                     │                    │
     │ 8. Response:       │                     │                    │
     │ {success: true}    │                     │                    │
     │<───────────────────│                     │                    │
     │                    │                     │                    │
     │ 9. Redirect user   │                     │                    │
     │ to ReturnUrl       │                     │                    │
     │───────────────────────────────────────────────────────────────>│
     │                    │                     │                    │
     │                    │                     │ 10. Frontend       │
     │                    │                     │ refresh invoice    │
     │                    │                     │ status             │
     │                    │                     │<───────────────────│
```

**Chi tiết các bước:**

1. **User thanh toán trên PayOS:**
   - User điền thông tin thanh toán
   - PayOS xử lý thanh toán

2. **PayOS gọi Webhook:**
   ```http
   POST /webhooks/payos
   x-payos-signature: {signature}
   Content-Type: application/json
   
   {
     "code": 0,
     "desc": "SUCCESS",
     "data": {
       "orderCode": 123456789,
       "amount": 1000000,
       "description": "...",
       "reference": "PAYOS_REF_123",
       "transactionDateTime": "2025-01-19T15:30:00Z",
       ...
     },
     "signature": "..."
   }
   ```

3. **Backend xử lý (`ProcessPayOSWebhookCommandHandler`):**
   - Verify signature (dùng ChecksumKey)
   - Tìm Invoice theo `PayosOrderCode`
   - Check idempotency (tránh xử lý trùng)
   - Tạo Payment record
   - Update Invoice.Status = Paid
   - (Optional) Tạo CashbookEntry

4. **PayOS redirect user về Frontend:**
   - PayOS redirect đến `ReturnUrl` (nếu thanh toán thành công)
   - Hoặc `CancelUrl` (nếu user hủy)

5. **Frontend refresh:**
   - Frontend gọi `GET /api/invoices/{id}` để check status mới

---

## 4. Testing

### Test với PayOS Sandbox (nếu có)

PayOS thường có môi trường sandbox để test. Cấu hình:

```json
{
  "PayOS": {
    "BaseUrl": "https://api-sandbox.payos.vn",  // Sandbox URL
    "ClientId": 123456,
    "ApiKey": "sandbox-api-key",
    "ChecksumKey": "sandbox-checksum-key",
    ...
  }
}
```

### Test Webhook locally

Nếu bạn muốn test webhook trên local, có thể dùng **ngrok**:

1. **Cài ngrok:**
   ```bash
   # Download từ https://ngrok.com/
   ngrok http 5000  # Port của backend
   ```

2. **Lấy public URL:**
   ```
   Forwarding: https://abc123.ngrok.io -> http://localhost:5000
   ```

3. **Cấu hình Webhook URL trong PayOS Dashboard:**
   ```
   https://abc123.ngrok.io/webhooks/payos
   ```

---

## 5. Checklist triển khai

- [ ] Đăng ký tài khoản PayOS
- [ ] Lấy `ClientId`, `ApiKey`, `ChecksumKey` từ PayOS Dashboard
- [ ] Cấu hình PayOS trong `appsettings.Production.json` hoặc Environment Variables
- [ ] Set Webhook URL trong PayOS Dashboard: `https://your-backend.com/webhooks/payos`
- [ ] Cấu hình `ReturnUrl` và `CancelUrl` (frontend URLs)
- [ ] Chạy migration: `dotnet ef database update`
- [ ] Test tạo payment link: `POST /api/invoices/{id}/payos/create-link`
- [ ] Test webhook (có thể dùng ngrok cho local)
- [ ] Verify Payment record được tạo sau khi thanh toán
- [ ] Verify Invoice.Status được update thành Paid

---

## 6. Troubleshooting

### Lỗi: "PayOS API error: 401 Unauthorized"
- **Nguyên nhân:** `ApiKey` hoặc `ClientId` sai
- **Giải pháp:** Kiểm tra lại credentials trong PayOS Dashboard

### Lỗi: "Invalid webhook signature"
- **Nguyên nhân:** `ChecksumKey` sai hoặc signature không match
- **Giải pháp:** 
  - Kiểm tra lại `ChecksumKey`
  - Verify cách PayOS gửi signature (có thể là hex hoặc base64)

### Lỗi: "Invoice not found" trong webhook
- **Nguyên nhân:** `PayosOrderCode` không match
- **Giải pháp:** 
  - Kiểm tra cách generate `orderCode` trong `CreatePayOSLinkCommandHandler`
  - Verify `PayosOrderCode` được lưu đúng vào DB

### Payment link không hoạt động
- **Nguyên nhân:** `ReturnUrl` hoặc `CancelUrl` không hợp lệ
- **Giải pháp:** 
  - Đảm bảo URLs là HTTPS (không dùng HTTP)
  - Đảm bảo domain được whitelist trong PayOS (nếu cần)

---

## 7. Security Best Practices

1. **Không commit credentials vào Git:**
   - Dùng Environment Variables cho production
   - Thêm `appsettings.Production.json` vào `.gitignore`

2. **Verify webhook signature:**
   - Luôn verify signature trước khi xử lý webhook
   - Không tin tưởng data từ webhook nếu signature không hợp lệ

3. **Idempotency:**
   - Check `ReferenceCode` đã tồn tại trước khi tạo Payment mới
   - Tránh xử lý trùng khi PayOS gọi webhook nhiều lần

4. **HTTPS only:**
   - Chỉ dùng HTTPS cho production
   - PayOS yêu cầu HTTPS cho webhook và return URLs

---

## 8. API Endpoints Reference

### Tạo Payment Link
```http
POST /api/invoices/{invoiceId}/payos/create-link
Authorization: Bearer {token}
```

**Response:**
```json
{
  "invoiceId": "guid",
  "checkoutUrl": "https://pay.payos.vn/web/...",
  "qrCodeData": "data:image/png;base64,..."
}
```

### Webhook (PayOS gọi về)
```http
POST /webhooks/payos
x-payos-signature: {signature}
Content-Type: application/json
```

**Request Body:**
```json
{
  "code": 0,
  "desc": "SUCCESS",
  "data": {
    "orderCode": 123456789,
    "amount": 1000000,
    "reference": "PAYOS_REF_123",
    ...
  }
}
```

---

## 9. Ví dụ Frontend Integration

### React/Next.js Example:

```typescript
// Tạo payment link
const createPaymentLink = async (invoiceId: string) => {
  const response = await fetch(
    `/api/invoices/${invoiceId}/payos/create-link`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  const data = await response.json();
  
  // Option 1: Redirect
  window.location.href = data.checkoutUrl;
  
  // Option 2: Show QR Code
  // <img src={data.qrCodeData} alt="QR Code" />
};

// Sau khi PayOS redirect về
useEffect(() => {
  const urlParams = new URLSearchParams(window.location.search);
  const invoiceId = urlParams.get('invoiceId');
  
  if (invoiceId) {
    // Refresh invoice status
    fetchInvoiceStatus(invoiceId);
  }
}, []);
```

---

## 10. Liên hệ Support

Nếu gặp vấn đề với PayOS:
- **PayOS Support:** https://pay.payos.vn/support
- **PayOS Documentation:** https://payos.vn/docs
- **PayOS API Reference:** https://payos.vn/docs/api

