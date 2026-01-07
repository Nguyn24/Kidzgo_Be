# Phân tích Coverage API - Task 2 & 3

## Task 2: Master Data (để FE render dropdown/label)

### Yêu cầu trong Api-first.md:
- ✅ `GET /branches` (Admin thấy all; Staff/Teacher thấy 1)
- ✅ `GET /programs` (Phonics/Cambridge/Communication…)
- ❌ `GET /levels`
- ✅ `GET /rooms?branchId=` (tương đương với `/api/classrooms`)
- ❌ `GET /roles` (main teacher, TA…)
- ❌ `GET /lookups` (enum: attendanceStatus, sessionType MAIN/MAKEUP/EXTRA_PAID…, invoiceStatus, homeworkStatus…)

### Trạng thái hiện tại:

#### ✅ Đã có:
1. **GET /api/programs** - `ProgramController.GetPrograms()`
   - UC-040: Xem danh sách Programs ✅
   - Có filter theo branchId, searchTerm, isActive
   - Có endpoint `/api/programs/active` cho public

2. **GET /api/classrooms** - `ClassroomController.GetClassrooms()`
   - UC-052: Xem danh sách Classrooms ✅
   - Có filter theo branchId, searchTerm, isActive
   - Có endpoint `/api/classrooms/active` cho public
   - **Lưu ý**: Route là `/api/classrooms` chứ không phải `/api/rooms`

#### ❌ Chưa có:
1. **GET /api/branches**
   - UC-385: Xem danh sách Branches ❌
   - **Yêu cầu**: Admin thấy all; Staff/Teacher thấy 1 branch của mình
   - **Cần**: BranchController với logic phân quyền

2. **GET /api/levels**
   - **Không có use case tương ứng** trong use-cases-list.md
   - **Có thể**: Lấy từ Program.Level hoặc tạo bảng Levels riêng
   - **Cần**: Endpoint để FE lấy danh sách levels

3. **GET /api/roles**
   - **Không có use case tương ứng** trong use-cases-list.md
   - **Có thể**: Enum hoặc lookup table cho SessionRole (MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP)
   - **Cần**: Endpoint để FE lấy danh sách roles

4. **GET /api/lookups**
   - **Không có use case tương ứng** trong use-cases-list.md
   - **Yêu cầu**: Enum values cho:
     - attendanceStatus
     - sessionType (MAIN/MAKEUP/EXTRA_PAID…)
     - invoiceStatus
     - homeworkStatus
     - classStatus
     - enrollmentStatus
     - ... và các enum khác
   - **Cần**: Endpoint để FE lấy tất cả enum values

---

## Task 3: Timetable + Class + Session (xương sống học vụ)

### Yêu cầu trong Api-first.md:
- ❌ `GET /teacher/classes` - Danh sách lớp của Teacher
- ❌ `GET /students/{studentId}/classes` - Danh sách lớp của Student
- ❌ `GET /students/{studentId}/timetable?from=&to=` - Thời khóa biểu của Student
- ❌ `GET /teacher/timetable?from=&to=` - Thời khóa biểu của Teacher
- ❌ `GET /sessions/{sessionId}` - Chi tiết buổi học (Session)

### Trạng thái hiện tại:

#### ✅ Đã có (nhưng chỉ cho Admin/Staff):
1. **GET /api/classes** - `ClassController.GetClasses()`
   - UC-058: Xem danh sách Classes ✅
   - **Nhưng**: Chỉ có `[Authorize(Roles = "Admin,Staff")]`
   - **Thiếu**: Endpoint riêng cho Teacher và Parent/Student

2. **GET /api/classes/{id}** - `ClassController.GetClassById()`
   - UC-059: Xem chi tiết Class ✅
   - **Nhưng**: Chỉ có `[Authorize(Roles = "Admin,Staff")]`
   - **Thiếu**: Endpoint riêng cho Teacher và Parent/Student

#### ❌ Chưa có:
1. **GET /teacher/classes**
   - **Không có use case tương ứng trực tiếp**
   - **Có thể**: Biến thể của UC-058 với filter theo teacherId từ IUserContext
   - **Cần**: TeacherController hoặc endpoint riêng trong ClassController
   - **Logic**: Lấy classes mà teacher là MainTeacher hoặc AssistantTeacher

2. **GET /students/{studentId}/classes**
   - **Không có use case tương ứng trực tiếp**
   - **Có thể**: Dựa trên Enrollment (UC-068: Xem danh sách học sinh trong Class)
   - **Cần**: StudentController hoặc endpoint riêng
   - **Logic**: Lấy classes mà student đã enroll (status ACTIVE)

3. **GET /students/{studentId}/timetable?from=&to=**
   - **Không có use case tương ứng trực tiếp**
   - **Có thể**: Dựa trên UC-077: Xem danh sách Sessions
   - **Cần**: TimetableController hoặc endpoint trong StudentController
   - **Logic**: 
     - Lấy sessions từ classes mà student đã enroll
     - Filter theo date range (from, to)
     - Include: giờ học, phòng, GV, loại buổi

4. **GET /teacher/timetable?from=&to=**
   - **Không có use case tương ứng trực tiếp**
   - **Có thể**: Dựa trên UC-077: Xem danh sách Sessions
   - **Cần**: TimetableController hoặc endpoint trong TeacherController
   - **Logic**:
     - Lấy sessions từ classes mà teacher dạy
     - Filter theo date range (from, to)
     - Include: giờ học, phòng, GV, loại buổi

5. **GET /sessions/{sessionId}**
   - UC-078: Xem chi tiết Session ❌
   - **Cần**: SessionController
   - **Yêu cầu**: 
     - Giờ học, phòng, GV, loại buổi
     - Lesson plan link
     - Attendance summary (số học sinh present/absent)
   - **Có thể cần**: UC-100: Xem danh sách điểm danh của Session

---

## Tổng kết

### Task 2: Master Data
- **Đã có**: 2/6 endpoints (33%)
- **Thiếu**: 4 endpoints
  - GET /api/branches
  - GET /api/levels
  - GET /api/roles
  - GET /api/lookups

### Task 3: Timetable + Class + Session
- **Đã có**: 2/5 endpoints (40%) - nhưng chỉ cho Admin/Staff
- **Thiếu**: 5 endpoints
  - GET /teacher/classes
  - GET /students/{studentId}/classes
  - GET /students/{studentId}/timetable
  - GET /teacher/timetable
  - GET /sessions/{sessionId}

### Use Cases liên quan chưa được cover:
- UC-077: Xem danh sách Sessions (cần cho timetable)
- UC-078: Xem chi tiết Session
- UC-385: Xem danh sách Branches (với phân quyền)

### Đề xuất ưu tiên:
1. **Cao**: GET /sessions/{sessionId} (UC-078) - cần cho attendance và timetable
2. **Cao**: GET /teacher/classes và GET /students/{studentId}/classes - cần cho FE
3. **Trung bình**: GET /api/branches với phân quyền
4. **Trung bình**: GET /api/lookups - giúp FE không hard-code enum
5. **Thấp**: GET /api/levels và GET /api/roles - có thể lấy từ data hiện có

