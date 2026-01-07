# Phân tích Entities - Đủ cho API Task 2 & 3

## Kết luận: ✅ **ĐỦ** để đáp ứng các API

---

## Task 3: Timetable + Class + Session

### ✅ Session Entity ĐỦ cho Timetable

**Session.cs** có đầy đủ thông tin cần thiết:

```csharp
public class Session : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }                    // ✅ Liên kết với Class
    public Guid BranchId { get; set; }                   // ✅ Chi nhánh
    public DateTime PlannedDatetime { get; set; }        // ✅ Thời gian dự kiến
    public DateTime? ActualDatetime { get; set; }       // ✅ Thời gian thực tế
    public Guid? PlannedRoomId { get; set; }            // ✅ Phòng dự kiến
    public Guid? ActualRoomId { get; set; }             // ✅ Phòng thực tế
    public Guid? PlannedTeacherId { get; set; }          // ✅ Giáo viên dự kiến
    public Guid? ActualTeacherId { get; set; }          // ✅ Giáo viên thực tế
    public Guid? PlannedAssistantId { get; set; }      // ✅ Trợ giảng dự kiến
    public Guid? ActualAssistantId { get; set; }        // ✅ Trợ giảng thực tế
    public int DurationMinutes { get; set; }             // ✅ Thời lượng
    public ParticipationType ParticipationType { get; set; }  // ✅ Loại buổi (Main/Makeup/ExtraPaid/Free/Trial)
    public SessionStatus Status { get; set; }           // ✅ Trạng thái (Scheduled/Completed/Cancelled)
    
    // Navigation properties
    public Class Class { get; set; }                    // ✅ Thông tin lớp
    public Branch Branch { get; set; }                  // ✅ Thông tin chi nhánh
    public Classroom? PlannedRoom { get; set; }        // ✅ Thông tin phòng
    public Classroom? ActualRoom { get; set; }
    public User? PlannedTeacher { get; set; }           // ✅ Thông tin giáo viên
    public User? ActualTeacher { get; set; }
    public User? PlannedAssistant { get; set; }
    public User? ActualAssistant { get; set; }
    public LessonPlan? LessonPlan { get; set; }         // ✅ Link lesson plan
    public ICollection<Attendance> Attendances { get; set; }  // ✅ Điểm danh
}
```

### ✅ Relationships đủ để query Timetable

**1. Student Timetable:**
```
Session 
  -> Class 
    -> ClassEnrollments (Status = Active)
      -> StudentProfile (ProfileType = Student)
```

**2. Teacher Timetable:**
```
Session 
  -> PlannedTeacherId / ActualTeacherId = UserId
  HOẶC
Class 
  -> MainTeacherId / AssistantTeacherId = UserId
    -> Sessions
```

**3. Class List cho Student:**
```
Profile (ProfileType = Student)
  -> ClassEnrollments (Status = Active)
    -> Class
```

**4. Class List cho Teacher:**
```
User (Role = Teacher)
  -> MainTeacherClasses / AssistantTeacherClasses
    -> Class
```

---

## Task 2: Master Data

### ✅ Các Entities cần thiết đều có:

1. **Branch** ✅
   - `Id`, `Code`, `Name`, `Address`, `ContactPhone`, `ContactEmail`, `IsActive`
   - Navigation: `Users`, `Classrooms`, `Classes`, `Sessions`

2. **Program** ✅
   - `Id`, `BranchId`, `Name`, `Level` (string), `TotalSessions`, `IsActive`
   - **Lưu ý**: `Level` là string, không phải bảng riêng
   - **Có thể**: GET /levels trả về `SELECT DISTINCT Level FROM Programs WHERE IsActive = true`

3. **Classroom** ✅
   - `Id`, `BranchId`, `Name`, `Capacity`, `IsActive`
   - Navigation: `Branch`, `PlannedRoomSessions`, `ActualRoomSessions`

4. **User** ✅
   - `Id`, `Email`, `Role`, `Name`, `BranchId`, `IsActive`
   - Navigation: `MainTeacherClasses`, `AssistantTeacherClasses`, `PlannedTeacherSessions`, `ActualTeacherSessions`

5. **Profile** ✅
   - `Id`, `UserId`, `ProfileType` (Parent/Student), `DisplayName`, `IsActive`
   - Navigation: `ClassEnrollments`, `Attendances`

### ✅ Các Enum đều có:

1. **AttendanceStatus** ✅
   - `Present`, `Absent`, `Makeup`

2. **SessionStatus** ✅
   - `Scheduled`, `Completed`, `Cancelled`

3. **ParticipationType** ✅
   - `Main`, `Makeup`, `ExtraPaid`, `Free`, `Trial`

4. **ClassStatus** ✅
   - `Planned`, `Active`, `Closed`

5. **EnrollmentStatus** ✅
   - `Active`, `Paused`, `Dropped`

6. **InvoiceStatus** ✅
   - `Pending`, `Paid`, `Overdue`, `Cancelled`

7. **HomeworkStatus** ✅
   - `Assigned`, `Submitted`, `Graded`, `Late`, `Missing`

8. **SessionRoleType** ✅ (cho GET /roles)
   - `MainTeacher`, `Assistant`, `Club`, `Workshop`

---

## Các API có thể implement ngay:

### Task 2: Master Data

1. **GET /api/branches** ✅
   - Query: `Branch` table
   - Filter: Admin thấy all; Staff/Teacher thấy 1 (dựa vào `User.BranchId`)

2. **GET /api/programs** ✅
   - Đã có: `ProgramController.GetPrograms()`

3. **GET /api/levels** ✅
   - Query: `SELECT DISTINCT Level FROM Programs WHERE IsActive = true AND Level IS NOT NULL`
   - Hoặc: Tạo bảng `Levels` riêng nếu cần

4. **GET /api/classrooms** ✅
   - Đã có: `ClassroomController.GetClassrooms()`
   - Route: `/api/classrooms` (không phải `/api/rooms`)

5. **GET /api/roles** ✅
   - Trả về enum `SessionRoleType`: `MainTeacher`, `Assistant`, `Club`, `Workshop`

6. **GET /api/lookups** ✅
   - Trả về tất cả enum values:
     - `attendanceStatus`: AttendanceStatus enum
     - `sessionType`: ParticipationType enum
     - `sessionStatus`: SessionStatus enum
     - `classStatus`: ClassStatus enum
     - `enrollmentStatus`: EnrollmentStatus enum
     - `invoiceStatus`: InvoiceStatus enum
     - `homeworkStatus`: HomeworkStatus enum
     - `sessionRoleType`: SessionRoleType enum

### Task 3: Timetable + Class + Session

1. **GET /teacher/classes** ✅
   - Query: `Classes` WHERE `MainTeacherId = userId` OR `AssistantTeacherId = userId`
   - Include: `Program`, `Branch`, `MainTeacher`, `AssistantTeacher`

2. **GET /students/{studentId}/classes** ✅
   - Query: `ClassEnrollments` WHERE `StudentProfileId = studentId` AND `Status = Active`
   - Include: `Class` -> `Program`, `Branch`, `MainTeacher`, `AssistantTeacher`

3. **GET /students/{studentId}/timetable?from=&to=** ✅
   - Query: `Sessions` 
     - JOIN `Classes` ON `Sessions.ClassId = Classes.Id`
     - JOIN `ClassEnrollments` ON `Classes.Id = ClassEnrollments.ClassId`
     - WHERE `ClassEnrollments.StudentProfileId = studentId` 
       AND `ClassEnrollments.Status = Active`
       AND `Sessions.PlannedDatetime >= from` 
       AND `Sessions.PlannedDatetime <= to`
   - Include: `Class`, `PlannedRoom`, `PlannedTeacher`, `PlannedAssistant`, `LessonPlan`
   - Select: `PlannedDatetime`, `DurationMinutes`, `ParticipationType`, `Status`

4. **GET /teacher/timetable?from=&to=** ✅
   - Query: `Sessions`
     - WHERE (`PlannedTeacherId = userId` OR `ActualTeacherId = userId`)
       AND `PlannedDatetime >= from`
       AND `PlannedDatetime <= to`
   - Include: `Class`, `PlannedRoom`, `PlannedTeacher`, `PlannedAssistant`, `LessonPlan`
   - Select: `PlannedDatetime`, `DurationMinutes`, `ParticipationType`, `Status`

5. **GET /sessions/{sessionId}** ✅
   - Query: `Sessions` WHERE `Id = sessionId`
   - Include: 
     - `Class` -> `Program`, `Branch`
     - `PlannedRoom` / `ActualRoom`
     - `PlannedTeacher` / `ActualTeacher`
     - `PlannedAssistant` / `ActualAssistant`
     - `LessonPlan`
     - `Attendances` -> `StudentProfile` (để tính attendance summary)

---

## Tóm tắt

### ✅ Entities ĐỦ để implement tất cả API

- **Session** có đầy đủ thông tin cho Timetable
- **Relationships** đủ để query Student/Teacher classes và timetable
- **Các enum** đều có sẵn cho lookups
- **Không cần thêm entity mới**

### ⚠️ Lưu ý:

1. **Program.Level** là string, không phải bảng riêng
   - Có thể query `SELECT DISTINCT Level FROM Programs`
   - Hoặc tạo bảng `Levels` riêng nếu cần quản lý tốt hơn

2. **Route naming**:
   - `/api/classrooms` (không phải `/api/rooms`)
   - Có thể tạo alias `/api/rooms` nếu cần

3. **Timetable query**:
   - Sử dụng `PlannedDatetime` cho lịch dự kiến
   - Sử dụng `ActualDatetime` cho lịch thực tế (nếu có)
   - Filter theo `Status != Cancelled` để loại bỏ buổi đã hủy

4. **Attendance Summary**:
   - Có thể tính từ `Session.Attendances`
   - Count theo `AttendanceStatus`: Present/Absent/Makeup

