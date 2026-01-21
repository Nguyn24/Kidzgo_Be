# Hướng dẫn Cron Expression trong Quartz

## Tổng quan

Quartz sử dụng **6-7 trường** để định nghĩa lịch chạy job (khác với Unix cron chỉ có 5 trường):

```
┌───────────── giây (0-59)
│ ┌─────────── phút (0-59)
│ │ ┌───────── giờ (0-23)
│ │ │ ┌─────── ngày trong tháng (1-31)
│ │ │ │ ┌───── tháng (1-12 hoặc JAN-DEC)
│ │ │ │ │ ┌─── ngày trong tuần (1-7 hoặc SUN-SAT, 1=CN)
│ │ │ │ │ │ ┌─ năm (tùy chọn, 1970-2099)
│ │ │ │ │ │ │
* * * * * ? *
```

## Cấu trúc chi tiết

| Vị trí | Field | Giá trị hợp lệ | Ký hiệu đặc biệt |
|--------|-------|----------------|------------------|
| 1 | Giây | 0-59 | `*` `,` `-` `/` |
| 2 | Phút | 0-59 | `*` `,` `-` `/` |
| 3 | Giờ | 0-23 | `*` `,` `-` `/` |
| 4 | Ngày trong tháng | 1-31 | `*` `,` `-` `?` `/` `L` `W` |
| 5 | Tháng | 1-12 hoặc JAN-DEC | `*` `,` `-` `/` |
| 6 | Ngày trong tuần | 1-7 hoặc SUN-SAT (1=CN) | `*` `,` `-` `?` `/` `L` `#` |
| 7 | Năm | 1970-2099 (tùy chọn) | `*` `,` `-` `/` |

## Ký hiệu đặc biệt

### `*` (Wildcard)
- Đại diện cho **mọi giá trị** trong field đó
- Ví dụ: `*` trong giờ = mọi giờ (0-23)

### `?` (No specific value)
- **Không quan tâm** giá trị
- Chỉ dùng cho **ngày trong tháng** hoặc **ngày trong tuần**
- **Quy tắc**: Không thể dùng `*` và `?` cùng lúc trong cùng một field
- **Quy tắc**: Nếu chỉ định ngày trong tháng → dùng `?` cho ngày trong tuần (và ngược lại)

### `-` (Range)
- Định nghĩa **khoảng giá trị**
- Ví dụ: `10-15` = từ 10 đến 15

### `,` (List)
- **Liệt kê** các giá trị cụ thể
- Ví dụ: `1,3,5` = 1, 3, hoặc 5

### `/` (Increment)
- **Bước nhảy** từ giá trị bắt đầu
- Format: `start/increment`
- Ví dụ: `0/5` = bắt đầu từ 0, mỗi 5 đơn vị (0, 5, 10, 15...)
- Ví dụ: `10/30` = bắt đầu từ 10, mỗi 30 đơn vị (10, 40...)

### `L` (Last)
- **Cuối cùng** của tháng/tuần
- Ví dụ: `L` trong ngày = ngày cuối tháng
- Ví dụ: `L-2` = 2 ngày trước ngày cuối tháng

### `W` (Weekday)
- **Ngày làm việc** gần nhất
- Ví dụ: `15W` = ngày làm việc gần ngày 15 nhất (nếu 15 là CN thì chuyển sang T2)

### `#` (Nth occurrence)
- **Ngày thứ N** trong tuần của tháng
- Format: `day#n`
- Ví dụ: `MON#2` = thứ 2 của tháng (thứ 2 tuần thứ 2)
- Ví dụ: `FRI#1` = thứ 6 đầu tiên của tháng

## Ví dụ phổ biến

### Chạy theo phút
```json
"0 0/1 * * * ?"          // Mỗi phút (00:00, 00:01, 00:02...)
"0 0/5 * * * ?"         // Mỗi 5 phút (00:00, 00:05, 00:10...)
"0 0/15 * * * ?"        // Mỗi 15 phút (00:00, 00:15, 00:30, 00:45)
"0 30 * * * ?"          // Mỗi giờ vào phút thứ 30 (00:30, 01:30, 02:30...)
```

### Chạy theo giờ
```json
"0 0 * * * ?"           // Mỗi giờ vào phút 0 (00:00, 01:00, 02:00...)
"0 0 18 * * ?"          // Mỗi ngày lúc 18:00:00
"0 0 9-17 * * ?"        // Mỗi giờ từ 9h-17h (9:00, 10:00...17:00)
"0 0 0,12 * * ?"        // 00:00 và 12:00 mỗi ngày
```

### Chạy theo ngày
```json
"0 0 0 1 * ?"           // 00:00:00 ngày đầu mỗi tháng
"0 0 18 L * ?"          // 18:00:00 ngày cuối mỗi tháng
"0 0 0 15W * ?"         // 00:00:00 ngày làm việc gần ngày 15 nhất
"0 0 12 ? * MON"        // 12:00:00 mỗi thứ 2
"0 0 12 ? * MON-FRI"    // 12:00:00 từ thứ 2 đến thứ 6
```

### Chạy theo tuần
```json
"0 0 9 ? * MON"         // 09:00:00 mỗi thứ 2
"0 0 18 ? * FRI"        // 18:00:00 mỗi thứ 6
"0 0 10 ? * MON#2"      // 10:00:00 thứ 2 tuần thứ 2 của tháng
"0 0 12 ? * MON#1"      // 12:00:00 thứ 2 đầu tiên của tháng
```

### Chạy theo tháng
```json
"0 0 0 1 JAN ?"         // 00:00:00 ngày 1 tháng 1
"0 0 18 1 * ?"         // 18:00:00 ngày 1 mỗi tháng
"0 0 0 1 1-6 ?"        // 00:00:00 ngày 1 từ tháng 1-6
```

### Kết hợp phức tạp
```json
"0 0/30 9-17 * * ?"     // Mỗi 30 phút từ 9h-17h (9:00, 9:30, 10:00...17:00)
"0 0 9,12,15,18 * * ?"  // 9:00, 12:00, 15:00, 18:00 mỗi ngày
"0 0 18 ? * MON-FRI"    // 18:00:00 từ thứ 2 đến thứ 6
"0 0 0 1,15 * ?"        // 00:00:00 ngày 1 và 15 mỗi tháng
```

## Ví dụ cho Job SyncPlannedToActualSessionsJob

### Chạy mỗi phút (đề xuất cho real-time sync)
```json
"Quartz": {
  "Schedules": {
    "SyncPlannedToActualSessionsJob": "0 0/1 * * * ?"
  }
}
```

### Chạy mỗi 5 phút (tiết kiệm tài nguyên hơn)
```json
"Quartz": {
  "Schedules": {
    "SyncPlannedToActualSessionsJob": "0 0/5 * * * ?"
  }
}
```

### Chạy mỗi 10 phút
```json
"Quartz": {
  "Schedules": {
    "SyncPlannedToActualSessionsJob": "0 0/10 * * * ?"
  }
}
```

### Chạy mỗi giờ
```json
"Quartz": {
  "Schedules": {
    "SyncPlannedToActualSessionsJob": "0 0 * * * ?"
  }
}
```

## Lưu ý quan trọng

1. **`?` vs `*`**: 
   - Dùng `?` cho ngày trong tháng HOẶC ngày trong tuần (không cả hai)
   - Dùng `*` cho các field khác

2. **Ngày trong tuần**:
   - Quartz: `1 = CN`, `2 = T2`, `3 = T3`... `7 = T7`
   - Hoặc dùng: `SUN`, `MON`, `TUE`, `WED`, `THU`, `FRI`, `SAT`

3. **Tháng**:
   - Có thể dùng số (1-12) hoặc tên (`JAN`, `FEB`, `MAR`...)

4. **Timezone**:
   - Quartz mặc định dùng server timezone
   - Có thể cấu hình timezone riêng trong trigger

5. **Validation**:
   - Luôn test cron expression trước khi deploy
   - Có thể dùng online tools như [Cron Expression Generator](https://www.freeformatter.com/cron-expression-generator-quartz.html)

## Tài liệu tham khảo

- [Quartz Scheduler Documentation](https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/crontrigger.html)
- [Cron Expression Tester](https://www.freeformatter.com/cron-expression-generator-quartz.html)

