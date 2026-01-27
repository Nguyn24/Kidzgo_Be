# Thiết kế luồng Lead có nhiều con (Multi-Children Lead & Placement Test)

Tài liệu này mô tả **giải pháp kỹ thuật** để hỗ trợ bài toán:

> 1 parent (Lead) có **nhiều con**, mỗi bé có **placement test riêng**, **profile riêng**, và có thể **ghi danh (enrollment) riêng**.

Hiện tại hệ thống đang theo mô hình:  
**1 Lead ≈ 1 bé**, **1 PlacementTest ↔ 1 Lead**.  
Giải pháp dưới đây mở rộng để vẫn giữ được Lead (parent) nhưng tách **Child** ra riêng.

---

## 1. Mô hình dữ liệu đề xuất

### 1.1. Entity mới: `LeadChild`

Tạo entity/bảng mới để biểu diễn **mỗi bé** thuộc 1 parent lead:

- `Id : Guid`
- `LeadId : Guid` (FK → `Lead.Id`)
- `ChildName : string` (tên bé)
- `Dob : DateTime?` (ngày sinh, nếu FE cung cấp)
- `Gender : string?` (hoặc enum nếu cần)
- `ProgramInterest : string?` (quan tâm chương trình, riêng cho bé)
- `Notes : string?`
- `Status : LeadChildStatus` (mới, mô tả pipeline riêng cho từng bé)
  - Gợi ý enum:
    - `New`
    - `BookedTest`
    - `TestDone`
    - `Enrolled`
    - `Lost`
- `ConvertedStudentProfileId : Guid?` (FK → `Profile.Id` của học sinh)
- `CreatedAt : DateTime`
- `UpdatedAt : DateTime`

### 1.2. Cập nhật `PlacementTest`

Hiện tại:

- `PlacementTest` có:
  - `LeadId : Guid?`
  - `StudentProfileId : Guid?`

Đề xuất bổ sung:

- Thêm:
  - `LeadChildId : Guid?` (FK → `LeadChild.Id`)

Quy ước target tương lai:

- **1 PlacementTest gắn với 1 LeadChild** (và thông qua LeadChild gắn với Lead parent).
- `LeadId` có thể:
  - Giai đoạn đầu: **giữ lại** để tương thích, auto set theo `LeadChild.LeadId`.
  - Giai đoạn sau: **dần dần bỏ** nếu không còn dùng.

### 1.3. Bổ sung enum `LeadChildStatus`

Thêm file mới, ví dụ: `Kidzgo.Domain/CRM/LeadChildStatus.cs`:

- `New`
- `BookedTest`
- `TestDone`
- `Enrolled`
- `Lost`

(Có thể reuse `LeadStatus` nếu muốn, nhưng tách riêng giúp không phá logic cũ.)

---

## 2. Chiến lược migration dữ liệu

Mục tiêu: không làm hỏng dữ liệu/flow hiện tại, cho phép triển khai dần.

### 2.1. Bước 1 – Thêm bảng & cột mới

1. Thêm entity `LeadChild` + configuration trong `Infrastructure`.
2. Thêm cột `LeadChildId` vào bảng `PlacementTests`.
3. Chạy migration:
   - `dotnet ef migrations add AddLeadChildAndPlacementTestLeadChildId`
   - `dotnet ef database update`

### 2.2. Bước 2 – Seed LeadChild mặc định cho dữ liệu cũ

Trong migration (hoặc script SQL riêng), với **mỗi Lead hiện có**:

- Tạo **1 bản ghi `LeadChild` mặc định**:
  - `ChildName` có thể lấy từ:
    - Nếu hiện tại có field nào mang ý nghĩa “tên học sinh” → dùng trực tiếp.
    - Nếu không, tạm dùng `ContactName` hoặc `"Child of {ContactName}"`.
  - `Status` map từ `Lead.Status` hiện tại:
    - `New` → `New`
    - `BookedTest` → `BookedTest`
    - `TestDone` → `TestDone`
    - `Enrolled` → `Enrolled`
    - `Lost` → `Lost`
  - `ConvertedStudentProfileId`:
    - Nếu `Lead.ConvertedStudentProfileId` có dữ liệu → copy sang.

Sau đó, với **mỗi PlacementTest hiện có**:

- Tìm `LeadChild` default tương ứng bằng `LeadId`.
- Set `PlacementTests.LeadChildId = LeadChild.Id`.

### 2.3. Bước 3 – Không bắt buộc nhưng nên làm

Sau khi đã dùng `LeadChild` ổn định:

- Refactor code/EF config để:
  - `PlacementTest.LeadId` chỉ tồn tại để đọc cũ, không dùng cho logic mới.
  - Tất cả logic mới chỉ dùng `LeadChildId`.

---

## 3. Thay đổi / mở rộng API

### 3.1. Public Web Form (tạo lead từ form)

Hiện tại form parent gửi vào gần như tương đương **1 lead = 1 bé**.  
Đề xuất allow payload dạng:

```json
{
  "contactName": "Parent Name",
  "phone": "0912345678",
  "email": "parent@example.com",
  "source": "Landing",
  "campaign": "Summer 2026",
  "branchPreference": "GUID-BRANCH",
  "notes": "Ghi chú chung cho gia đình",
  "children": [
    {
      "childName": "Bé A",
      "dob": "2016-05-01",
      "gender": "Male",
      "programInterest": "IELTS Foundation",
      "notes": "Hơi nhút nhát"
    },
    {
      "childName": "Bé B",
      "dob": "2012-09-20",
      "gender": "Female",
      "programInterest": "Junior Starter"
    }
  ]
}
```

Luồng xử lý:

1. Tạo 1 `Lead` (parent).
2. Với mỗi phần tử `children[]`:
   - Tạo 1 `LeadChild`.
3. Có thể tạo 1 `LeadActivity` ghi chú: "Lead created with N children".

> Giai đoạn đầu, nếu FE chưa sửa, có thể:
> - Mặc định tạo 1 `LeadChild` (giống migration).
> - Sau khi FE hỗ trợ nhiều bé → chuyển qua dùng mảng `children`.

### 3.2. API quản lý children cho Lead

Đề xuất thêm nhóm API:

- `GET /api/leads/{leadId}/children`
  - Trả danh sách `LeadChild` cho lead.
- `POST /api/leads/{leadId}/children`
  - Tạo thêm bé mới cho lead hiện hữu.
- `PUT /api/leads/{leadId}/children/{childId}`
  - Cập nhật thông tin bé.
- (Tuỳ) `DELETE /api/leads/{leadId}/children/{childId}`
  - Soft-delete bé nếu bị nhập nhầm.

### 3.3. Schedule Placement Test theo LeadChild

Hiện tại API:

- `POST /api/placement-tests`
  - Nhận `LeadId`, `StudentProfileId`, `ClassId`, `ScheduledAt`, ...

Đề xuất mở rộng:

- Body mới (đề xuất):

```json
{
  "leadId": "GUID-LEAD (optional nếu đã có childId)",
  "leadChildId": "GUID-LEAD-CHILD",
  "studentProfileId": null,
  "classId": "GUID-CLASS (optional)",
  "scheduledAt": "2026-02-01T09:00:00Z",
  "room": "Test Room 1",
  "invigilatorUserId": "GUID-USER (optional)"
}
```

Rule:

- **Ưu tiên `leadChildId`**:
  - Nếu `leadChildId` có giá trị → validate:
    - `LeadChild` tồn tại.
    - Nếu `leadId` cũng có, phải khớp với `LeadChild.LeadId`.
  - Set:
    - `PlacementTest.LeadChildId = leadChildId`
    - `PlacementTest.LeadId = LeadChild.LeadId` (giữ tương thích).
- Giai đoạn chuyển tiếp:
  - Nếu chỉ có `leadId` mà không có `leadChildId`:
    - Sử dụng **LeadChild default** (tạo nếu chưa có).

### 3.4. Cập nhật kết quả Placement Test & LeadChild.Status

API: `PUT /api/placement-tests/{id}/results`

Logic cần bổ sung:

- Sau khi test đủ điểm và chuyển `PlacementTest.Status = Completed`:
  - Nếu `LeadChildId` không null:
    - Load `LeadChild`.
    - Nếu `LeadChild.Status == BookedTest` → đổi sang `TestDone`.
    - Cập nhật `UpdatedAt`.
    - Tạo `LeadActivity` ghi rõ: "Child {ChildName} placement test completed → status: TestDone".
- Luồng update `Lead.Status`:
  - Có thể **giữ như hiện tại** (Lead.Status = TestDone).
  - Hoặc đổi sang logic:
    - Nếu **tất cả children** của Lead có `Status >= TestDone` thì Lead.Status = `TestDone`.

Khuyến nghị:

- Giữ logic **Lead.Status** như hiện tại để không phá UI/report cũ.
- Bổ sung thêm logic child-level để dùng dần dần.

### 3.5. Convert-to-enrolled theo Child

API hiện tại:  
`POST /api/placement-tests/{id}/convert-to-enrolled`

- Input: `{ "studentProfileId": "GUID" }`
- Logic hiện tại:
  - Đổi `Lead.Status` → `Enrolled`.
  - Set `Lead.ConvertedStudentProfileId`.
  - (Không phân biệt bé nào.)

Đề xuất mở rộng:

1. Thêm `LeadChildId` vào `PlacementTest`, như ở phần trên.
2. Trong `ConvertLeadToEnrolledCommandHandler`:
   - Nếu `PlacementTest.LeadChildId` có giá trị:
     - Load `LeadChild`.
     - Set:
       - `LeadChild.ConvertedStudentProfileId = studentProfileId`
       - `LeadChild.Status = Enrolled`
     - Giữ Lead.Status theo rule:
       - Ví dụ: nếu **ít nhất 1 child Enrolled** → Lead.Status = `Enrolled`.
   - Nếu `LeadChildId` null (dữ liệu cũ):
     - Giữ behavior cũ (convert theo Lead).

3. Tạo `LeadActivity` cụ thể theo bé:
   - `"Child {ChildName} converted to ENROLLED in class {ClassName} (via enrollment API)"`.

> Việc gán vào `Class` vẫn qua `EnrollmentController` như hiện tại.  
> Convert-to-enrolled chỉ đổi status Lead/Child + link với Profile.

---

## 4. Luồng nghiệp vụ đề xuất (end-to-end)

### 4.1. Parent có 2 con, cả hai đều muốn test

1. **Parent gửi form web** với 2 bé:
   - Backend tạo 1 `Lead` + 2 `LeadChild`.
2. **Tư vấn viên book test**:
   - Gọi `POST /api/placement-tests` 2 lần, mỗi lần với 1 `leadChildId`.
3. **Giáo viên nhập điểm test**:
   - Gọi `PUT /api/placement-tests/{id}/results` cho từng bé.
   - PlacementTest chuyển `Completed`, LeadChild chuyển `TestDone`.
4. **Quyết định ghi danh**:
   - Tạo `StudentProfile` cho từng bé cần học.
   - Gọi `POST /api/placement-tests/{id}/convert-to-enrolled` với `studentProfileId` tương ứng.
   - Sau đó, dùng `POST /api/enrollments` để ghi danh vào class cụ thể.
5. **Trường hợp 1 bé học, 1 bé không**:
   - Child A: `Status = Enrolled`.
   - Child B: `Status = TestDone` hoặc `Lost`.
   - Lead parent vẫn giữ để quản lý ở mức gia đình (có thể `Enrolled` hoặc một trạng thái tổng hợp).

---

## 5. Lộ trình triển khai gợi ý

Để triển khai an toàn, có thể chia thành nhiều phase:

### Phase 1 – Chuẩn bị dữ liệu

- Thêm entity `LeadChild`, enum `LeadChildStatus`.
- Thêm `LeadChildId` vào `PlacementTest`.
- Migration + seed **LeadChild default** + map `PlacementTest` → `LeadChild`.
- Chưa đổi API, chỉ thay internal: các handler bắt đầu ưu tiên dùng `LeadChild` nếu có.

### Phase 2 – Mở rộng API nội bộ

- Sửa `SchedulePlacementTest` nhận thêm `leadChildId`.
- Sửa `UpdatePlacementTestResults` + `ConvertLeadToEnrolled` để xử lý `LeadChildId`.
- Thêm API quản lý children cho Lead.

### Phase 3 – Cập nhật FE & form

- FE form web gửi được mảng `children`.
- UI CRM hiển thị children của 1 Lead, show kết quả placement theo từng bé.

### Phase 4 – Dọn dẹp / chuẩn hoá

- Khi đã chạy ổn:
  - Quy ước rõ: mọi placement mới **bắt buộc** phải gắn `LeadChildId`.
  - Cân nhắc dần dần bỏ `PlacementTest.LeadId` và các đoạn code cũ phụ thuộc vào 1:1 Lead–PlacementTest.

---

## 6. Ghi chú khi triển khai

- **Không cần làm tất cả một lúc.** Có thể bắt đầu từ:
  1. Tạo `LeadChild` + migration map dữ liệu cũ.
  2. Sửa `SchedulePlacementTest` & `UpdatePlacementTestResults` để ưu tiên `LeadChildId`.
- Khi code, luôn giữ **backward compatible**:
  - Nếu `leadChildId` không được truyền → fallback về logic cũ (Lead-level).
  - Nếu dữ liệu cũ chưa có `LeadChildId` → dùng LeadChild default.

File này chỉ là **hướng dẫn thiết kế & các bước lớn**.  
Khi bạn sẵn sàng triển khai, có thể làm lần lượt từng bước và chạy migration/EF như các use case trước.***

