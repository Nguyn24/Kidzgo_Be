# Tài Liệu API FE - Enrollment Confirmation Payment Setting - 2026-04-19

Tài liệu này mô tả 2 API cấu hình thông tin thanh toán dùng trên phiếu xác nhận nhập học PDF trong `RegistrationController.cs`:

- `GET /api/registrations/enrollment-confirmation-payment-setting`
- `PUT /api/registrations/enrollment-confirmation-payment-setting`

Chức năng này cho phép Admin cấu hình thông tin tài khoản trung tâm, logo trên PDF và QR chuyển khoản VietQR. Khi generate phiếu xác nhận nhập học PDF, BE sẽ lấy cấu hình đang active để render phần `Thông tin thanh toán`.

## Tổng quan role và phạm vi dữ liệu

Tất cả API trong controller yêu cầu user đã đăng nhập.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động được phép |
| --- | --- | --- | --- |
| Admin | Cấu hình thanh toán global hoặc theo chi nhánh | `all` | `view`, `create/update`, `activate/deactivate` |
| ManagementStaff | Cấu hình thanh toán global hoặc theo chi nhánh | `all` | `view` |
| Teacher | Không được truy cập | `none` | `none` |
| Parent | Không được truy cập | `none` | `none` |
| Student | Không được truy cập | `none` | `none` |
| Anonymous | Không được truy cập | `none` | `none` |

Ghi chú:

- `branchId = null` nghĩa là cấu hình global.
- `branchId != null` nghĩa là cấu hình riêng cho chi nhánh đó.
- API `GET` nếu không tìm thấy cấu hình riêng của chi nhánh sẽ fallback về cấu hình global.
- API `PUT` là upsert: nếu cấu hình theo scope chưa có thì tạo mới, nếu đã có thì cập nhật.
- PDF chỉ dùng cấu hình có `isActive = true`. Nếu cấu hình chi nhánh inactive, khi generate PDF BE sẽ fallback sang cấu hình global active nếu có.

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
  "title": "Registration.PaymentAccountNameRequired",
  "status": 400,
  "detail": "Payment account name is required."
}
```

## Danh sách API

### 1. GET `/api/registrations/enrollment-confirmation-payment-setting`

Dùng để lấy cấu hình thông tin thanh toán hiện tại cho phiếu xác nhận nhập học PDF.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Query params:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `branchId` | `Guid?` | No | null | Nếu gửi thì lấy cấu hình riêng của chi nhánh. Nếu không có cấu hình riêng, BE fallback về global. Nếu null thì lấy cấu hình global. |

Logic lấy dữ liệu:

| Trường hợp | Kết quả |
| --- | --- |
| Có `branchId` và có setting theo chi nhánh | Trả setting của chi nhánh, `isFallbackToGlobal = false`. |
| Có `branchId` nhưng chưa có setting theo chi nhánh, có setting global | Trả setting global, `isFallbackToGlobal = true`. |
| Có `branchId` nhưng chưa có setting chi nhánh và chưa có setting global | Trả object rỗng cơ bản, `id = null`, `isActive = false`. |
| Không gửi `branchId`, có setting global | Trả setting global. |
| Không gửi `branchId`, chưa có setting global | Trả object rỗng cơ bản, `id = null`, `isActive = false`. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "branchId": "22222222-2222-2222-2222-222222222222",
    "isFallbackToGlobal": false,
    "paymentMethod": "Chuyển khoản",
    "accountName": "TRINH DUC ANH",
    "accountNumber": "089498720",
    "bankName": "Ngân hàng quân đội",
    "bankCode": "MB",
    "bankBin": "970422",
    "vietQrTemplate": "compact2",
    "logoUrl": "https://cdn.example.com/logo.png",
    "qrPreviewUrl": "https://img.vietqr.io/image/970422-089498720-compact2.png?accountName=TRINH%20DUC%20ANH",
    "isActive": true,
    "createdAt": "2026-04-19T08:00:00Z",
    "updatedAt": "2026-04-19T08:30:00Z",
    "updatedBy": "33333333-3333-3333-3333-333333333333"
  }
}
```

Response khi chưa có setting:

```json
{
  "isSuccess": true,
  "data": {
    "id": null,
    "branchId": null,
    "isFallbackToGlobal": false,
    "paymentMethod": "Tiền mặt / Chuyển khoản",
    "accountName": null,
    "accountNumber": null,
    "bankName": null,
    "bankCode": null,
    "bankBin": null,
    "vietQrTemplate": "compact2",
    "logoUrl": null,
    "qrPreviewUrl": null,
    "isActive": false,
    "createdAt": null,
    "updatedAt": null,
    "updatedBy": null
  }
}
```

Lưu ý cho FE:

- `qrPreviewUrl` ở API GET chỉ là QR preview cho tài khoản. QR trên file PDF khi generate sẽ có thêm số tiền học phí khóa và nội dung chuyển khoản.
- Nếu `isFallbackToGlobal = true`, FE nên hiển thị cảnh báo nhẹ kiểu: chi nhánh này đang dùng cấu hình global.

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |
| 500 | Server failure | Lỗi hệ thống hoặc lỗi truy vấn DB ngoài dự kiến. |

### 2. PUT `/api/registrations/enrollment-confirmation-payment-setting`

Dùng để Admin tạo/cập nhật cấu hình thông tin thanh toán và logo dùng trên phiếu xác nhận nhập học PDF.

Roles: `Admin`

Phạm vi dữ liệu: `all`

Body JSON:

```json
{
  "branchId": "22222222-2222-2222-2222-222222222222",
  "paymentMethod": "Chuyển khoản",
  "accountName": "TRINH DUC ANH",
  "accountNumber": "089498720",
  "bankName": "Ngân hàng quân đội",
  "bankCode": "MB",
  "bankBin": "970422",
  "vietQrTemplate": "compact2",
  "logoUrl": "https://cdn.example.com/logo.png",
  "isActive": true
}
```

Các field trong body:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `branchId` | `Guid?` | No | null | Nếu null thì lưu cấu hình global. Nếu có giá trị thì lưu cấu hình riêng cho chi nhánh đó. |
| `paymentMethod` | `string?` | No | `Tiền mặt / Chuyển khoản` | Hình thức thanh toán hiển thị trên PDF. BE trim khoảng trắng. |
| `accountName` | `string` | Yes | - | Chủ tài khoản trung tâm. |
| `accountNumber` | `string` | Yes | - | Số tài khoản nhận tiền. |
| `bankName` | `string` | Yes | - | Tên ngân hàng hiển thị trên PDF. |
| `bankCode` | `string?` | Required nếu không có `bankBin` | null | Mã ngân hàng dùng cho VietQR, ví dụ `MB`. |
| `bankBin` | `string?` | Required nếu không có `bankCode` | null | BIN ngân hàng dùng cho VietQR, ví dụ `970422`. Nên ưu tiên dùng `bankBin` để QR ổn định. |
| `vietQrTemplate` | `string?` | No | `compact2` | Template ảnh QR của VietQR, ví dụ `compact2`. |
| `logoUrl` | `string?` | No | null | Logo hiển thị ở góc phải header PDF. Chỉ nhận absolute `http(s)` URL hoặc `data:image/...`. |
| `isActive` | `bool` | No | true | Bật/tắt cấu hình này. PDF chỉ dùng setting active. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "branchId": "22222222-2222-2222-2222-222222222222",
    "paymentMethod": "Chuyển khoản",
    "accountName": "TRINH DUC ANH",
    "accountNumber": "089498720",
    "bankName": "Ngân hàng quân đội",
    "bankCode": "MB",
    "bankBin": "970422",
    "vietQrTemplate": "compact2",
    "logoUrl": "https://cdn.example.com/logo.png",
    "qrPreviewUrl": "https://img.vietqr.io/image/970422-089498720-compact2.png?accountName=TRINH%20DUC%20ANH",
    "isActive": true,
    "createdAt": "2026-04-19T08:00:00Z",
    "updatedAt": "2026-04-19T08:30:00Z",
    "updatedBy": "33333333-3333-3333-3333-333333333333"
  }
}
```

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `Registration.PaymentAccountNameRequired` | `accountName` null/rỗng/blank. |
| 400 | `Registration.PaymentAccountNumberRequired` | `accountNumber` null/rỗng/blank. |
| 400 | `Registration.PaymentBankNameRequired` | `bankName` null/rỗng/blank. |
| 400 | `Registration.PaymentBankIdentifierRequired` | Cả `bankCode` và `bankBin` đều null/rỗng/blank nên không đủ thông tin tạo VietQR. |
| 400 | `Registration.PaymentLogoUrlInvalid` | `logoUrl` không phải absolute `http(s)` URL và không phải `data:image/...`. |
| 400 | Model validation/body invalid | Body JSON sai format hoặc không bind được request. |
| 404 | `Registration.BranchNotFound` | Có gửi `branchId` nhưng chi nhánh không tồn tại. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không phải `Admin`. |
| 500 | Server failure | Lỗi hệ thống hoặc lỗi DB ngoài dự kiến. |

## Status definition

Payment setting không có lifecycle status riêng. Field trạng thái chính là `isActive`.

| Status/field | Ý nghĩa |
| --- | --- |
| `isActive = true` | Setting được bật. Khi generate PDF, BE có thể dùng setting này. |
| `isActive = false` | Setting bị tắt. Khi generate PDF, BE không dùng setting này. |
| `isFallbackToGlobal = true` | Chỉ có ở response GET. Nghĩa là FE đang hỏi theo `branchId`, nhưng BE không tìm thấy setting riêng nên trả setting global. |
| `id = null`, `isActive = false` | Chưa có setting nào phù hợp để trả về. |

Luồng chuyển trạng thái:

1. Admin gọi `PUT` với `isActive = true` để tạo hoặc bật setting.
2. Admin gọi `PUT` với `isActive = false` để tắt setting.
3. Khi generate PDF:
   - BE ưu tiên setting active theo `branchId` của registration.
   - Nếu không có setting active theo chi nhánh, BE fallback sang setting global active.
   - Nếu không có setting active nào, PDF dùng placeholder mặc định cho thông tin thanh toán và không có QR/logo.

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/registrations/enrollment-confirmation-payment-setting` | Yes | Yes | No | No | No | No |
| `PUT /api/registrations/enrollment-confirmation-payment-setting` | Yes | No | No | No | No | No |

## Validation rule tổng hợp

| Rule | API áp dụng | Kết quả khi sai |
| --- | --- | --- |
| User phải đăng nhập | Tất cả | 401 |
| Role phải là `Admin` hoặc `ManagementStaff` khi xem | GET | 403 |
| Role phải là `Admin` khi cập nhật | PUT | 403 |
| `branchId` nếu gửi lên phải tồn tại | PUT | 404 `Registration.BranchNotFound` |
| `accountName` không được rỗng | PUT | 400 `Registration.PaymentAccountNameRequired` |
| `accountNumber` không được rỗng | PUT | 400 `Registration.PaymentAccountNumberRequired` |
| `bankName` không được rỗng | PUT | 400 `Registration.PaymentBankNameRequired` |
| Phải có ít nhất một trong `bankCode` hoặc `bankBin` | PUT | 400 `Registration.PaymentBankIdentifierRequired` |
| `logoUrl` nếu gửi phải là absolute `http(s)` URL hoặc `data:image/...` | PUT | 400 `Registration.PaymentLogoUrlInvalid` |
| `paymentMethod`, `bankCode`, `bankBin`, `vietQrTemplate`, `logoUrl` được BE trim khoảng trắng | PUT | Không lỗi; value rỗng sau trim được xem là null. |
| Scope setting là duy nhất theo `branchId`/global | PUT | BE update setting cũ nếu đã tồn tại, không tạo trùng. |

## Ghi chú cho FE

- FE nên cho Admin chọn cấu hình `Global` hoặc `Theo chi nhánh`.
- Khi cấu hình VietQR, nên ưu tiên nhập `bankBin` thay vì chỉ nhập `bankCode`.
- `qrPreviewUrl` từ API setting chỉ để preview tài khoản. QR trên PDF sẽ được BE tạo lại theo từng registration để tự điền thêm `amount` và `addInfo`.
- Sau khi Admin cập nhật setting, các PDF đã generate trước đó không tự đổi. FE cần gọi lại API generate PDF với `regenerate=true` nếu muốn xuất lại phiếu theo setting mới.
- Nếu Admin set `isActive = false`, setting vẫn còn trong DB nhưng không được dùng khi generate PDF.
