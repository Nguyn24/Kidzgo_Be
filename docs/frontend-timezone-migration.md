# Frontend Timezone Migration

## Mục tiêu

Backend đã được chuẩn hóa theo rule:

- Dữ liệu thời gian tuyệt đối được lưu bằng UTC.
- API `DateTime` được trả về dưới dạng ISO 8601 có offset.
- Nếu FE gửi `DateTime` không có offset, backend tạm hiểu là `Asia/Ho_Chi_Minh`.

FE nên cập nhật để không còn phụ thuộc vào fallback này.

## Thay đổi bắt buộc cho FE

### 1. Tất cả field `DateTime` phải gửi theo ISO 8601 có offset

Ưu tiên gửi dạng:

```text
2026-04-09T08:00:00+07:00
```

Hoặc UTC rõ ràng:

```text
2026-04-09T01:00:00Z
```

Không nên tiếp tục gửi:

```text
2026-04-09T08:00:00
```

vì chuỗi này không có timezone, backend chỉ đang fallback coi là giờ VN.

### 2. Tất cả field `DateTime` từ response phải parse như timezone-aware string

Backend hiện trả `DateTime` theo dạng:

```json
"2026-04-09T08:00:00+07:00"
```

FE không được:

- cắt chuỗi thủ công
- tự cộng `+7`
- giả định string trả về là UTC thuần
- giả định format cũ kiểu `dd/MM/yyyy hh:mm:ss tt`

### 3. `DateOnly` vẫn giữ dạng `yyyy-MM-dd`

Ví dụ:

```json
"2026-04-09"
```

Các field chỉ có ngày vẫn nên gửi/nhận theo format này, không thêm giờ.

### 4. Query param thời gian cũng phải gửi có offset

Ví dụ:

```text
/api/teacher/timetable?from=2026-04-01T00:00:00+07:00&to=2026-04-30T23:59:59+07:00
```

Không nên gửi:

```text
/api/teacher/timetable?from=2026-04-01&to=2026-04-30
```

trừ khi endpoint thực sự nhận `DateOnly`.

## Breaking Changes FE sẽ thấy

### 1. Format response `DateTime` đã đổi

Trước đây FE có thể đang nhận:

```text
23/11/2025 10:46:41 PM
```

Hiện tại FE sẽ nhận:

```text
2026-04-09T08:00:00+07:00
```

### 2. Không được tự convert `UTC -> +7` thêm lần nữa

Vì response đã có offset rõ ràng. Nếu FE tiếp tục cộng `+7` thủ công thì giờ sẽ bị lệch.

### 3. Một số field derived theo ngày/giờ local đã đổi đúng theo múi giờ VN

Các field như:

- `PlannedDate`
- `StartTime`
- `EndTime`
- các filter `today`
- các logic `from/to`

giờ đều đang bám theo `Asia/Ho_Chi_Minh`, không còn vô tình lấy ngày UTC.

## Checklist FE cần làm

- Đổi mọi nơi gửi `DateTime` sang ISO 8601 có offset.
- Đổi mọi nơi parse `DateTime` response sang parser có timezone.
- Xóa logic cộng/trừ `7 giờ` thủ công.
- Xóa mọi parser đang chờ format `dd/MM/yyyy hh:mm:ss tt`.
- Kiểm tra lại các màn hình:
  - timetable
  - session
  - homework due date
  - placement test
  - leave request
  - pause enrollment
  - finance/dashboard
- Kiểm tra query params `from`, `to`, `startDate`, `endDate`, `plannedDatetime`, `dueAt`, `scheduledAt`, `effectiveDate`.

## Khuyến nghị implementation

Nếu FE đang dùng:

- `dayjs`: dùng `dayjs(string)` hoặc `dayjs.tz` nếu project đã cài timezone plugin
- `luxon`: dùng `DateTime.fromISO(...)`
- native JS: dùng `new Date(isoString)` nhưng cần cẩn thận lúc format hiển thị

Rule an toàn:

1. Lưu state transport bằng ISO string.
2. Chỉ parse sang object date khi cần render/tính toán.
3. Khi submit ngược lại API, serialize lại thành ISO 8601 có offset.

## Ví dụ

### Request body đúng

```json
{
  "plannedDatetime": "2026-04-09T08:00:00+07:00",
  "dueAt": "2026-04-12T23:59:59+07:00"
}
```

### Response đúng

```json
{
  "plannedDatetime": "2026-04-09T08:00:00+07:00",
  "plannedDate": "2026-04-09",
  "startTime": "08:00:00",
  "endTime": "09:30:00"
}
```

## Tạm thời backend còn hỗ trợ fallback

Nếu FE gửi `2026-04-09T08:00:00` không offset, backend hiện vẫn hiểu là giờ VN.

Đây chỉ là backward-compatibility tạm thời, không nên coi là contract chuẩn cho FE mới.

## Nguồn contract hiện tại trong backend

- [DateTimeJsonConverter.cs](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/Kidzgo.API/Extensions/DateTimeJsonConverter.cs)
- [DateTimeModelBinder.cs](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/Kidzgo.API/ModelBinding/DateTimeModelBinder.cs)
- [SwaggerExtensions.cs](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/Kidzgo.API/Extensions/SwaggerExtensions.cs)
- [VietnamTime.cs](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/Kidzgo.Domain/Time/VietnamTime.cs)
