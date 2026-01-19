# Phân tích API Task 8 (Invoices + PayOS) vs Use Cases

## Yêu cầu từ Api-first.md (Task 8)

1. **GET /parents/{parentId}/invoices?status=**
   - Xem danh sách invoices của parent cụ thể
   - Filter theo status

2. **GET /invoices/{invoiceId}**
   - Xem chi tiết invoice

3. **POST /invoices/{invoiceId}/payos/create-link**
   - Tạo PayOS payment link
   - Trả về: `checkoutUrl`, `qrCodeData`

4. **POST /webhooks/payos**
   - Webhook từ PayOS khi thanh toán thành công
   - Update invoice status → PAID
   - Ghi Cashbook Entry

5. **GET /parents/{parentId}/payments?from=&to=**
   - Xem lịch sử payments của parent
   - Filter theo date range

---

## Use Cases hiện có trong use-cases-list.md

### ✅ Đã có (tương ứng):

- **UC-255**: Xem chi tiết Invoice ✅
- **UC-260**: Sinh PayOS payment link ✅
- **UC-261**: Sinh PayOS QR code ✅ (có thể gộp với UC-260)
- **UC-264**: Thanh toán qua PayOS (webhook) ✅
- **UC-269**: Cập nhật Invoice status sau thanh toán ✅
- **UC-274**: Gắn Cashbook Entry với Invoice (CASH_IN) ✅

### ⚠️ Thiếu hoặc chưa rõ:

1. **UC-254: Xem danh sách Invoices**
   - ❌ **Thiếu**: Filter theo `parentId` cụ thể
   - ❌ **Thiếu**: Filter theo `status` (PENDING/PAID/OVERDUE)
   - 📝 **Cần bổ sung**: UC-254a hoặc mở rộng UC-254 để bao gồm filter theo parentId và status

2. **UC-270: Xem lịch sử Payments của Invoice**
   - ❌ **Khác scope**: UC-270 là xem payments của **Invoice**, không phải của **Parent**
   - 📝 **Cần bổ sung**: UC-270a: Xem lịch sử Payments của Parent (filter theo from/to)

3. **UC-264: Thanh toán qua PayOS (webhook)**
   - ⚠️ **Chưa rõ**: Có ghi Cashbook Entry không?
   - 📝 **Cần làm rõ**: UC-264 có bao gồm việc tự động tạo Cashbook Entry (UC-274) hay không?

---

## Kết luận

### ✅ **Đã đủ cơ bản nhưng cần bổ sung:**

1. **Use cases đã có đủ** cho các chức năng chính:
   - Xem chi tiết invoice ✅
   - Tạo PayOS link/QR ✅
   - Webhook PayOS ✅
   - Cập nhật invoice status ✅
   - Ghi Cashbook ✅

2. **Cần bổ sung/chi tiết hóa:**

   - **UC-254a**: Xem danh sách Invoices của Parent (filter theo parentId, status)
   - **UC-270a**: Xem lịch sử Payments của Parent (filter theo from/to)
   - **Làm rõ UC-264**: Xác nhận rằng webhook PayOS tự động ghi Cashbook Entry

### 📋 **Đề xuất cập nhật use-cases-list.md:**

```markdown
### 14.1. Invoices
- UC-253: Tạo Invoice (MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE)
- UC-254: Xem danh sách Invoices
- UC-254a: Xem danh sách Invoices của Parent (filter theo parentId, status) ⭐ NEW
- UC-255: Xem chi tiết Invoice
- UC-256: Cập nhật Invoice
- UC-257: Hủy Invoice (CANCELLED)
- UC-258: Tạo Invoice Lines
- UC-259: Gắn session_ids vào Invoice Line
- UC-260: Sinh PayOS payment link
- UC-261: Sinh PayOS QR code
- UC-262: Xem trạng thái Invoice (PENDING/PAID/OVERDUE)
- UC-263: Đánh dấu Invoice OVERDUE

### 14.2. Payments
- UC-264: Thanh toán qua PayOS (webhook) - tự động update invoice status + ghi Cashbook Entry ⭐ UPDATED
- UC-265: Thanh toán bằng tiền mặt
- UC-266: Thanh toán chuyển khoản
- UC-267: Xác nhận thanh toán (Staff)
- UC-268: Upload chứng từ thanh toán
- UC-269: Cập nhật Invoice status sau thanh toán
- UC-270: Xem lịch sử Payments của Invoice
- UC-270a: Xem lịch sử Payments của Parent (filter theo from/to) ⭐ NEW
```

---

## Trả lời câu hỏi

**Q: File use case list đã đủ đáp ứng yêu cầu task 8 của file api first không hay nó là 1 luồng khác?**

**A:** 
- ✅ **Đã đủ cơ bản** - Các use cases hiện có đã cover được các chức năng chính của task 8
- ⚠️ **Nhưng thiếu một số chi tiết**:
  - Filter invoices theo parentId (UC-254a)
  - Xem payment history của parent (UC-270a)
- 📝 **Không phải luồng khác** - Đây là cùng một luồng, chỉ cần bổ sung thêm các use cases chi tiết hơn để match chính xác với API endpoints trong Api-first.md

