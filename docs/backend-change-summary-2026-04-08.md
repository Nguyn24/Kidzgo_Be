# Tổng Hợp Các Thay Đổi Backend

Ngày cập nhật: `2026-04-08`

Tài liệu này tổng hợp các thay đổi backend đã được chỉnh trong giai đoạn rà soát gần đây, tập trung vào các nhóm: `Homework`, `Registrations`, `Classes`, `Missions`, `Teacher scope`, và `Gamification`.

---

## 1. Homework

### 1.1. Đồng bộ `HomeworkStreak` cho luồng quiz

- Đã bổ sung logic cộng tiến độ mission `HomeworkStreak` trong luồng submit quiz / multiple-choice.
- File chính: `Kidzgo.Application/Homework/SubmitMultipleChoiceHomework/SubmitMultipleChoiceHomeworkCommandHandler.cs`

### 1.2. Chặn cộng sao lặp khi quiz được nộp lại

- Quiz chỉ cộng `RewardStars` ở lần nộp hợp lệ đầu tiên.
- Tránh trường hợp `allowResubmit = true` nhưng học sinh farm sao bằng cách submit lại nhiều lần.

### 1.3. Ẩn `missionId` khỏi public API của homework

- `missionId` không còn nằm trong request/response public của homework.
- Route public để link homework với mission cũng đã được gỡ khỏi bề mặt API.
- `MissionId` vẫn được giữ ở entity/domain để có thể bật lại sau nếu cần.

### 1.4. Ý nghĩa nghiệp vụ hiện tại

- `HomeworkStreak` hiện được tính theo kiểu: học sinh nộp homework đúng hạn thì được cộng progress.
- Logic này không yêu cầu homework phải gắn `missionId` cụ thể.

---

## 2. Registrations

### 2.1. `Upgrade` đổi sang cập nhật trực tiếp trên registration hiện tại

- API nâng gói không còn tạo registration mới và complete registration cũ.
- Hệ thống hiện cập nhật trực tiếp registration đang có:
  - đổi `TuitionPlanId`
  - tăng `RemainingSessions`
  - tính lại `TotalSessions`
  - giữ nguyên `UsedSessions`
- File chính: `Kidzgo.Application/Registrations/UpgradeTuitionPlan/UpgradeTuitionPlanCommandHandler.cs`

### 2.2. `ExpiryDate`

- `ExpiryDate` vẫn được giữ trên entity `Registration`.
- Hiện tại field này chưa đóng vai trò enforce business logic.
- Tức là:
  - chưa tự đổi status khi hết hạn
  - chưa chặn học tiếp
  - chưa được dùng như source-of-truth cho thời hạn gói

### 2.3. Gợi ý lớp (`suggest-classes`) được siết chặt logic lịch học

- API: `GET /api/registrations/{id}/suggest-classes`
- File chính: `Kidzgo.Application/Registrations/SuggestClasses/SuggestClassesQueryHandler.cs`

Logic cũ:
- match theo kiểu rộng, chỉ cần khớp một phần là được
- ví dụ nhập `thứ 2 sáng` thì lớp `thứ 2 chiều` vẫn có thể được suggest

Logic mới:
- nếu `preferredSchedule` có nhiều tiêu chí thì phải khớp đồng thời
- hỗ trợ parse:
  - thứ cụ thể: `thứ 2`, `thứ 3`, `T2`, `T7`, `CN`
  - buổi: `sáng`, `chiều`, `tối`
  - thời gian cụ thể: `10:00`, `10h`, `10 giờ`, `10am`, `2pm`
- nếu có giờ cụ thể thì backend so tiếp `BYHOUR` và `BYMINUTE`

Lưu ý:
- các filter gốc vẫn giữ nguyên:
  - cùng `ProgramId`
  - cùng `BranchId`
  - status lớp hợp lệ
  - còn chỗ học

---

## 3. Classes

### 3.1. Vá logic `unfull`

- Đã bổ sung helper đồng bộ trạng thái lớp theo số enrollment active:
  - file: `Kidzgo.Application/Classes/ClassCapacityStatusHelper.cs`
- Khi lớp đầy thì lên `Full`
- Khi học viên rời lớp, hủy đăng ký, drop enrollment, transfer, reactivate... thì hệ thống tự tính lại để mở lớp về `Active` hoặc `Recruiting` khi còn chỗ

Các luồng đã được đồng bộ gọi lại logic capacity:
- `AssignClass`
- `TransferClass`
- `CancelRegistration`
- `DropEnrollment`
- `ReactivateEnrollment`
- một phần pause enrollment flow

### 3.2. Thống nhất cách tính slot theo `EnrollmentStatus.Active`

- Việc kiểm tra capacity hiện dựa trên enrollment active, không đếm toàn bộ enrollment lịch sử.

### 3.3. Thêm filter `schedulePattern` cho `GET /api/classes`

- API `GET /api/classes` có thêm query param `schedulePattern`
- Hiện filter theo kiểu partial match trên chuỗi RRULE
- Phù hợp cho các nhu cầu lọc nhanh như:
  - `BYDAY=MO`
  - `FREQ=WEEKLY`

---

## 4. Missions

### 4.1. Tạo mission xong sẽ sinh thông báo web / in-app cho học sinh

- Khi tạo mission, backend không chỉ tạo `Mission` và `MissionProgress`
- Hệ thống còn tạo `Notification` channel `InApp` cho học sinh target
- File chính: `Kidzgo.Application/Missions/CreateMission/CreateMissionCommandHandler.cs`

Hành vi hiện tại:
- học sinh nhận được thông báo ngay khi mission được tạo
- deeplink tạm thời là `/missions`

### 4.2. Khóa phạm vi teacher khi tạo / cập nhật mission

- Teacher chỉ được phép target:
  - lớp mình dạy
  - học sinh mình dạy
  - group học sinh thuộc các lớp mình dạy
- File chính:
  - `Kidzgo.Application/Missions/Shared/TeacherMissionTargetGuard.cs`
  - `Kidzgo.Application/Missions/CreateMission/CreateMissionCommandHandler.cs`
  - `Kidzgo.Application/Missions/UpdateMission/UpdateMissionCommandHandler.cs`

### 4.3. Bổ sung error rõ ràng cho teacher target sai scope

- File: `Kidzgo.Domain/Gamification/Errors/MissionErrors.cs`

---

## 5. Teacher Scope Và API Dành Cho Giáo Viên

### 5.1. API đúng để teacher lấy lớp và học sinh

Teacher nên dùng:
- `GET /api/teacher/classes`
- `GET /api/teacher/classes/{classId}/students`
- `GET /api/teacher/students`

### 5.2. Thêm API `GET /api/teacher/students`

- File:
  - `Kidzgo.API/Controllers/TeacherController.cs`
  - `Kidzgo.Application/Classes/GetTeacherStudents/*`

Chức năng:
- trả danh sách học sinh active thuộc các lớp mà teacher đang dạy
- hỗ trợ:
  - `classId`
  - `searchTerm`
  - `pageNumber`
  - `pageSize`

### 5.3. Teacher bị chặn khỏi API generic lấy toàn bộ profile

- Nếu teacher gọi `GET /api/profiles`, backend trả lỗi scope
- File:
  - `Kidzgo.Application/Profiles/Admin/GetAllProfiles/GetAllProfilesQueryHandler.cs`
  - `Kidzgo.Domain/Users/Errors/ProfileErrors.cs`

Mục tiêu:
- tránh việc FE gọi nhầm API generic rồi show toàn bộ học sinh trong hệ thống

### 5.4. Siết scope các API teacher / class có nguy cơ lộ dữ liệu

Đã khóa thêm:
- `GET /api/classes/{id}/students`
  - nếu caller là teacher thì phải là lớp mình dạy
- `GET /api/teacher/timetable`
  - teacher không thể truyền `teacherUserId` của người khác
- `GET /api/teacher/timesheet`
  - teacher chỉ lấy được timesheet của chính mình

---

## 6. Gamification

### 6.1. Lưu `CancelReason` khi hủy đổi quà

- API `PATCH /api/gamification/reward-redemptions/{id}/cancel` nhận `reason`
- Backend hiện đã lưu lý do này vào `RewardRedemption.CancelReason`

File chính:
- `Kidzgo.Domain/Gamification/RewardRedemption.cs`
- `Kidzgo.Application/Gamification/CancelRewardRedemption/CancelRewardRedemptionCommandHandler.cs`
- `Kidzgo.Infrastructure/Configuration/RewardRedemptionConfiguration.cs`

Response đã expose lại field này ở:
- API cancel
- API list redemption
- API detail redemption
- API `GET /me`

Migration:
- `20260408124500_AddCancelReasonToRewardRedemption.cs`

### 6.2. Lưu snapshot `StarsDeducted`

- Khi đổi quà, backend hiện lưu số sao đã trừ tại thời điểm tạo redemption
- Field mới: `RewardRedemption.StarsDeducted`

Mục đích:
- UI có thể hiển thị lại chính xác sau khi reload
- không bị sai lịch sử nếu giá quà đổi sau này

Response đã expose `StarsDeducted` ở:
- POST tạo redemption
- GET list admin
- GET detail
- GET `/api/gamification/reward-redemptions/me`

Migration:
- `20260408170000_AddStarsDeductedToRewardRedemption.cs`

### 6.3. Refund sao khi cancel dùng snapshot đã lưu

- Khi hủy redemption, backend ưu tiên hoàn lại đúng `StarsDeducted`
- Không còn phụ thuộc tuyệt đối vào `RewardStoreItem.CostStars` hiện tại

---

## 7. Ghi Chú Cho FE

### 7.1. Mission của teacher

Nếu FE là teacher:
- không gọi `GET /api/profiles` để đổ dropdown học sinh
- không gọi API generic để lấy toàn bộ student
- nên dùng:
  - `GET /api/teacher/classes`
  - `GET /api/teacher/classes/{classId}/students`
  - hoặc `GET /api/teacher/students`

### 7.2. Suggest classes

`preferredSchedule` giờ đã được backend hiểu chặt hơn.

Ví dụ hợp lệ:
- `thứ 2 sáng`
- `T2 sáng`
- `thứ 2 10:00`
- `cuối tuần chiều`
- `CN 9h`

Lưu ý:
- nếu FE nhập cả thứ và buổi thì kết quả sẽ hẹp hơn trước
- đây là thay đổi đúng theo mong muốn nghiệp vụ

---

## 8. Build Và Persistence

- `Kidzgo.Application` đã được build thành công sau các thay đổi.
- Với phần `RewardRedemption`, cần apply migration nếu muốn dữ liệu mới được lưu thật xuống database:
  - `20260408124500_AddCancelReasonToRewardRedemption.cs`
  - `20260408170000_AddStarsDeductedToRewardRedemption.cs`

---

## 9. Tóm Tắt Nhanh

Các thay đổi backend đáng chú ý nhất:

- Quiz submit đã cộng `HomeworkStreak` và không cộng sao lặp khi resubmit
- `missionId` bị ẩn khỏi public homework API
- registration upgrade đổi sang update trực tiếp
- class có logic tự `unfull`
- `GET /api/classes` có thêm filter `schedulePattern`
- tạo mission sinh notification in-app ngay cho học sinh
- teacher bị khóa scope tốt hơn khi xem học sinh, timetable, timesheet và khi tạo mission
- có API mới `GET /api/teacher/students`
- reward redemption đã lưu `CancelReason`
- reward redemption đã lưu `StarsDeducted`
- suggest classes không còn match lịch theo kiểu quá rộng
