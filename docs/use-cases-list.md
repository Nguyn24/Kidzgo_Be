# Danh sách Use Cases - KidzGo


**Chú thích độ khó**:
- 🟢 **Dễ**: CRUD cơ bản, logic đơn giản, không có tích hợp phức tạp
- 🟡 **Trung bình**: Logic nghiệp vụ phức tạp, có tích hợp với hệ thống khác, cần xử lý edge cases, AI
- 🔴 **Khó**: Tính toán phức tạp, workflow nhiều bước, xử lý đồng thời, tích hợp bên ngoài (PayOS, Zalo OA)

---

## 1. Authentication & Authorization 🟡

### 1.1. Đăng nhập & Portal
- UC-001: Đăng nhập bằng Email + Password
- UC-002: Đăng nhập qua Zalo Mini App (SSO)
- UC-003: Hiển thị danh sách Profiles sau login (Parent/Student)
- UC-004: Chọn Profile Parent (yêu cầu nhập PIN)
- UC-005: Chọn Profile Student (không cần PIN)
- UC-006: Đăng nhập Portal nội bộ (Admin/Teacher/Staff)
- UC-007: Xác thực PIN cho Parent Profile
- UC-009: Đăng xuất
- UC-010: Quên mật khẩu
- UC-011: Đổi mật khẩu
- UC-012: Đổi PIN

---

## 2. CRM & Lead Management 🟡

### 2.1. Quản lý Lead
- UC-013: Tạo Lead từ form web
- UC-014: Tạo Lead từ Zalo
- UC-015: Tạo Lead từ Referral
- UC-016: Tạo Lead từ Offline
- UC-017: Xem danh sách Leads
- UC-018: Xem chi tiết Lead
- UC-019: Cập nhật thông tin Lead
- UC-020: Gán Lead cho Staff
- UC-021: Cập nhật trạng thái Lead (NEW/CONTACTED/BOOKED_TEST/TEST_DONE/ENROLLED/LOST)
- UC-022: Ghi chú Lead
- UC-023: Xem lịch sử hoạt động Lead
- UC-024: Theo dõi SLA phản hồi đầu tiên
- UC-025: Theo dõi số lần chạm (touch count)
- UC-026: Lên lịch follow-up tiếp theo

### 2.2. Placement Test
- UC-027: Đặt lịch Placement Test
- UC-028: Xem danh sách Placement Test
- UC-029: Cập nhật thông tin Placement Test
- UC-030: Hủy Placement Test
- UC-031: Đánh dấu NO_SHOW
- UC-032: Nhập kết quả Placement Test (listening/speaking/reading/writing)
- UC-033: Nhập điểm tổng (result_score)
- UC-034: Nhập level recommendation
- UC-035: Nhập program recommendation
- UC-036: Upload file kết quả (attachment_url)
- UC-037: Ghi chú Placement Test
- UC-038: Chuyển Lead sang ENROLLED sau Placement Test

---

## 3. Program & Class Management 🟢

### 3.1. Quản lý Program
- UC-039: Tạo Program
- UC-040: Xem danh sách Programs
- UC-041: Xem chi tiết Program
- UC-042: Cập nhật Program
- UC-043: Xóa mềm Program
- UC-044: Kích hoạt/Vô hiệu hóa Program

### 3.2. Quản lý Tuition Plan
- UC-045: Tạo Tuition Plan
- UC-046: Xem danh sách Tuition Plans
- UC-047: Xem chi tiết Tuition Plan
- UC-048: Cập nhật Tuition Plan
- UC-049: Xóa mềm Tuition Plan
- UC-050: Kích hoạt/Vô hiệu hóa Tuition Plan

### 3.3. Quản lý Classroom
- UC-051: Tạo Classroom
- UC-052: Xem danh sách Classrooms
- UC-053: Xem chi tiết Classroom
- UC-054: Cập nhật Classroom
- UC-055: Xóa Classroom
- UC-056: Kích hoạt/Vô hiệu hóa Classroom

### 3.4. Quản lý Class
- UC-057: Tạo Class
- UC-058: Xem danh sách Classes
- UC-059: Xem chi tiết Class
- UC-060: Cập nhật Class
- UC-061: Xóa mềm Class
- UC-062: Thay đổi trạng thái Class (PLANNED/ACTIVE/CLOSED)
- UC-063: Gán Main Teacher cho Class
- UC-064: Gán Assistant Teacher cho Class
- UC-065: Thiết lập schedule pattern (RRULE/JSON)
- UC-066: Kiểm tra capacity trước khi ghi danh

---

## 4. Enrollment 🟢

### 4.1. Ghi danh học sinh
- UC-067: Ghi danh học sinh vào Class
- UC-068: Xem danh sách học sinh trong Class
- UC-069: Xem chi tiết Enrollment
- UC-070: Cập nhật Enrollment
- UC-071: Tạm dừng Enrollment (PAUSED)
- UC-072: Hủy Enrollment (DROPPED)
- UC-073: Kích hoạt lại Enrollment
- UC-074: Gán Tuition Plan cho Enrollment
- UC-075: Xem lịch sử Enrollment của học sinh

---

## 5. Session & Schedule Management 🟡

### 5.1. Tạo và quản lý Sessions
- UC-076: Sinh Sessions từ schedule pattern (thêm 1 use case tạo session thủ công)
- UC-077: Xem danh sách Sessions
- UC-078: Xem chi tiết Session
- UC-079: Cập nhật Session (giờ/phòng/giáo viên)
- UC-080: Hủy Session (CANCELLED)
- UC-081: Đánh dấu Session hoàn thành (COMPLETED)
- UC-082: Kiểm tra xung đột phòng/giáo viên
- UC-083: Gợi ý phòng/slot khác khi xung đột
- UC-084: Gửi thông báo đổi lịch Session

### 5.2. Session Roles
- UC-085: Tạo Session Role (MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP)
- UC-086: Xem danh sách Session Roles của Session
- UC-087: Cập nhật Session Role
- UC-088: Xóa Session Role
- UC-089: Thiết lập payable_unit_price cho Session Role
- UC-090: Thiết lập payable_allowance cho Session Role

---

## 6. Leave Request & Attendance 🟡

### 6.1. Yêu cầu nghỉ học
- UC-091: Tạo Leave Request (Parent)
- UC-092: Tạo Leave Request (Staff thay)
- UC-093: Xem danh sách Leave Requests
- UC-094: Xem chi tiết Leave Request
- UC-095: Duyệt Leave Request (APPROVED)
- UC-096: Từ chối Leave Request (REJECTED)
- UC-097: Tự động duyệt Leave Request (≥24h notice)
- UC-098: Tạo MakeUpCredit khi duyệt (≥24h notice)

### 6.2. Điểm danh
- UC-099: Điểm danh học sinh (PRESENT/ABSENT/MAKEUP)
- UC-100: Xem danh sách điểm danh của Session
- UC-101: Xem lịch sử điểm danh của học sinh
- UC-102: Tự động gán absence_type (WITH_NOTICE_24H/UNDER_24H/NO_NOTICE/LONG_TERM)
- UC-103: Hiển thị cờ MakeUp khi có credit
- UC-104: Cập nhật điểm danh (nếu cần)

---

## 7. Makeup Credit Management 🟡

### 7.1. Quản lý Makeup Credit
- UC-105: Xem danh sách Makeup Credits của học sinh
- UC-106: Xem chi tiết Makeup Credit
- UC-107: Xem trạng thái Makeup Credit (AVAILABLE/USED/EXPIRED)
- UC-108: Tự động tạo Makeup Credit (từ Leave Request ≥24h)
- UC-109: Tạo Makeup Credit thủ công
- UC-110: Đánh dấu Makeup Credit đã sử dụng
- UC-111: Đánh dấu Makeup Credit hết hạn

### 7.2. Phân bổ Makeup
- UC-112: Đề xuất danh sách buổi bù phù hợp
- UC-113: Parent chọn buổi bù
- UC-114: Tạo Makeup Allocation
- UC-115: Tự động trừ MakeUpCredit khi phân bổ
- UC-116: Xem danh sách Makeup Allocations

---

## 8. Homework & Exercises 🟡

### 8.1. Quản lý Homework
- UC-117: Tạo Homework Assignment
- UC-118: Xem danh sách Homework Assignments
- UC-119: Xem chi tiết Homework Assignment
- UC-120: Cập nhật Homework Assignment
- UC-121: Xóa Homework Assignment
- UC-122: Tự động assign homework cho tất cả học sinh lớp
- UC-123: Gắn Homework với Mission
- UC-124: Thiết lập reward stars cho Homework

### 8.2. Nộp và chấm Homework
- UC-125: Học sinh nộp Homework (FILE/IMAGE/TEXT/LINK/QUIZ)
- UC-126: Xem danh sách Homework của bản thân 
- UC-126a: Xem danh sách Homework đã nộp
- UC-127: Xem chi tiết Homework submission
- UC-128: Teacher chấm Homework (GRADED)
- UC-129: Nhập điểm và feedback cho Homework
- UC-130: AI chấm Homework (nếu có)
- UC-131: Xem/chỉnh sửa kết quả AI chấm
- UC-132: Đánh dấu Homework quá hạn (LATE/MISSING)
- UC-133: Xem lịch sử Homework của học sinh

### 8.3. Quản lý Exercises (Quiz/Form)
- UC-134: Tạo Exercise (READING/LISTENING/WRITING)
- UC-135: Xem danh sách Exercises
- UC-136: Xem chi tiết Exercise
- UC-137: Cập nhật Exercise
- UC-138: Xóa mềm Exercise
- UC-139: Tạo Exercise Question (MULTIPLE_CHOICE/TEXT_INPUT)
- UC-140: Cập nhật Exercise Question
- UC-141: Xóa Exercise Question
- UC-142: Thiết lập correct_answer cho Question
- UC-143: Thiết lập points cho Question
- UC-144: Thiết lập options (JSON) cho Multiple Choice

### 8.4. Nộp và chấm Exercises
- UC-145: Học sinh làm Exercise
- UC-146: Học sinh nộp Exercise
- UC-147: Tự động chấm Multiple Choice
- UC-148: Teacher chấm Text Input (Writing)
- UC-149: Xem kết quả Exercise
- UC-150: Xem chi tiết từng câu trả lời
- UC-151: Nhập feedback cho câu trả lời

---

## 9. Exams & Test Results 🟢

### 9.1. Quản lý Exams
- UC-152: Tạo Exam (PLACEMENT/PROGRESS/MIDTERM/FINAL/SPEAKING)
- UC-153: Xem danh sách Exams
- UC-154: Xem chi tiết Exam
- UC-155: Cập nhật Exam
- UC-156: Xóa Exam
- UC-152a: Thiết lập thời gian thi (ScheduledStartTime, TimeLimitMinutes) cho Exam
- UC-152b: Thiết lập settings (AutoSubmitOnTimeLimit, PreventCopyPaste, PreventNavigation) cho Exam

### 9.2. Nhập và quản lý Exam Results
- UC-157: Nhập Exam Result cho học sinh
- UC-158: Xem danh sách Exam Results
- UC-159: Xem chi tiết Exam Result
- UC-160: Cập nhật Exam Result
- UC-161: Upload nhiều ảnh scan bài làm (JSON)
- UC-162: Xem lịch sử Exam Results của học sinh

---

## 10. Session Reports 🟢

### 10.1. Báo cáo buổi học
- UC-163: Teacher tạo Session Report
- UC-164: Teacher ghi feedback cho từng học sinh
- UC-165: Xem danh sách Session Reports
- UC-166: Xem chi tiết Session Report
- UC-167: Cập nhật Session Report
- UC-168: Filter Session Reports theo date range
- UC-169: Xem Session Reports của giáo viên trong tháng

### 10.2. Tổng hợp báo cáo tháng
- UC-170: Tổng hợp Session Reports theo date range
- UC-171: AI generate summary từ Session Reports
- UC-172: Teacher xem và chỉnh sửa AI summary
- UC-173: Đánh dấu Session Report đã được tổng hợp (is_monthly_compiled)

---

## 11. Monthly Reports (AI) 🟡

### 11.1. Tạo và quản lý Monthly Reports
- UC-174: Tạo Monthly Report Job
- UC-175: Gom dữ liệu cho Monthly Report (attendance, homework, test, mission, notes)
- UC-176: AI tạo draft Monthly Report
- UC-177: Xem danh sách Monthly Report Jobs
- UC-178: Xem trạng thái Monthly Report Job (PENDING/GENERATING/DONE/FAILED)

### 11.2. Review và Publish Monthly Reports
- UC-179: Teacher xem draft Monthly Report
- UC-180: Teacher chỉnh sửa draft Monthly Report
- UC-181: Teacher submit Monthly Report (REVIEW)
- UC-182: Staff/Admin comment Monthly Report
- UC-183: Staff/Admin approve Monthly Report
- UC-184: Staff/Admin reject Monthly Report
- UC-185: Publish Monthly Report
- UC-186: Parent/Student xem Monthly Report
- UC-187: Gửi thông báo khi publish Monthly Report

---

## 12. Gamification 🟡

### 12.1. Missions
- UC-188: Tạo Mission (CLASS/STUDENT/GROUP scope)
- UC-189: Xem danh sách Missions
- UC-190: Xem chi tiết Mission
- UC-191: Cập nhật Mission
- UC-192: Xóa Mission
- UC-193: Thiết lập reward_stars cho Mission
- UC-194: Thiết lập reward_exp cho Mission
- UC-195: Thiết lập total_questions cho Mission
- UC-196: Thiết lập progress_per_question cho Mission
- UC-197: Track progress của Mission
- UC-198: Hoàn thành Mission (COMPLETED)
- UC-199: Xem progress bar của Mission

### 12.2. Stars & XP
- UC-200: Cộng Stars khi hoàn thành Mission
- UC-201: Cộng Stars khi hoàn thành Homework
- UC-202: Cộng Stars khi điểm danh (Attendance Streak)
- UC-203: Cộng XP khi hoàn thành Mission
- UC-204: Cộng XP khi điểm danh (Attendance Streak)
- UC-205: Teacher/Staff cộng Stars thủ công
- UC-206: Teacher/Staff trừ Stars thủ công
- UC-207: Teacher/Staff cộng XP thủ công
- UC-208: Teacher/Staff trừ XP thủ công
- UC-209: Xem lịch sử Star Transactions
- UC-210: Xem balance Stars hiện tại
- UC-211: Tính Level từ XP
- UC-212: Xem Level và XP hiện tại

### 12.3. Attendance Streak
- UC-213: Học sinh điểm danh hàng ngày
- UC-214: Tự động tạo Attendance Streak record
- UC-215: Cộng Stars khi điểm danh (1 star)
- UC-216: Cộng XP khi điểm danh (5 exp)
- UC-217: Tính current_streak (số ngày liên tiếp)
- UC-218: Cập nhật max_streak
- UC-219: Xem Attendance Streak của học sinh
- UC-220: Reset streak khi bỏ lỡ ngày

### 12.4. Reward Store
- UC-221: Tạo Reward Store Item
- UC-222: Xem danh sách Reward Store Items
- UC-223: Xem chi tiết Reward Store Item
- UC-224: Cập nhật Reward Store Item
- UC-225: Xóa mềm Reward Store Item
- UC-226: Thiết lập cost_stars cho Item
- UC-227: Quản lý quantity của Item

### 12.5. Reward Redemption
- UC-228: Học sinh đổi quà (Request)
- UC-229: Xem danh sách Reward Redemptions
- UC-230: Xem chi tiết Reward Redemption
- UC-231: Staff duyệt Reward Redemption (APPROVED)
- UC-232: Staff từ chối Reward Redemption (CANCELLED)
- UC-233: Staff trao quà (DELIVERED)
- UC-234: Học sinh xác nhận nhận quà (RECEIVED)
- UC-235: Tự động xác nhận sau 3 ngày (nếu không xác nhận)
- UC-236: Lưu item_name tại thời điểm đổi
- UC-237: Trừ Stars khi đổi quà

---

## 13. Media Management 🟢

### 13.1. Upload và quản lý Media
- UC-238: Teacher/Staff upload ảnh/video
- UC-238a: Generate presigned URL for upload (S3/Cloud Storage)
- UC-239: Gắn tag Class cho Media
- UC-240: Gắn tag Student cho Media
- UC-241: Gắn tag Tháng (YYYY-MM) cho Media
- UC-241a: Gắn tag Type cho Media (HOMEWORK/REPORT/TEST/ALBUM/CLASS_PHOTO)
- UC-242: Thiết lập visibility (CLASS_ONLY/PERSONAL/PUBLIC_PARENT)
- UC-243: Xem danh sách Media
- UC-244: Xem chi tiết Media
- UC-245: Cập nhật Media
- UC-246: Xóa Media
- UC-247: Staff/Admin duyệt Media
- UC-248: Publish Media lên gallery

### 13.2. Xem Media
- UC-249: Parent/Student xem album lớp
- UC-250: Parent/Student xem album cá nhân
- UC-251: Filter Media theo tháng
- UC-252: Download Media (nếu được phép)

---

## 14. Finance Management 🔴

### 14.1. Invoices
- UC-253: Tạo Invoice (MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE)
- UC-254: Xem danh sách Invoices
- UC-254a: Xem danh sách Invoices của Parent (filter theo parentId, status)
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
- UC-264: Thanh toán qua PayOS (webhook) - tự động update invoice status + ghi Cashbook Entry
- UC-265: Thanh toán bằng tiền mặt
- UC-266: Thanh toán chuyển khoản
- UC-267: Xác nhận thanh toán (Staff)
- UC-268: Upload chứng từ thanh toán
- UC-269: Cập nhật Invoice status sau thanh toán
- UC-270: Xem lịch sử Payments của Invoice
- UC-270a: Xem lịch sử Payments của Parent (filter theo from/to)

### 14.3. Cashbook
**Role: AccountantStaff, Admin, ManagementStaff**
- UC-271: Tạo Cashbook Entry (CASH_IN/CASH_OUT)
- UC-272: Xem danh sách Cashbook Entries
- UC-273: Xem chi tiết Cashbook Entry
- UC-274: Gắn Cashbook Entry với Invoice (CASH_IN)
- UC-275: Gắn Cashbook Entry với Payroll Payment (CASH_OUT)
- UC-276: Gắn Cashbook Entry với Expense (CASH_OUT)
- UC-277: Filter Cashbook theo date range
- UC-278: Xem tổng thu/chi theo tháng

### 14.4. Công nợ
**Role: AccountantStaff, Admin, ManagementStaff**
- UC-279: Xem danh sách công nợ
- UC-280: Xem công nợ của học sinh
- UC-281: Xem tuổi nợ
- UC-282: Báo cáo công nợ
- UC-283: Gửi nhắc nhở công nợ

---

## 15. Payroll Management 🔴

### 15.1. Contracts
- UC-284: Tạo Contract
- UC-285: Xem danh sách Contracts
- UC-286: Xem chi tiết Contract
- UC-287: Cập nhật Contract
- UC-288: Thiết lập base_salary
- UC-289: Thiết lập hourly_rate
- UC-290: Thiết lập minimum_monthly_hours
- UC-291: Thiết lập overtime_rate_multiplier
- UC-292: Thiết lập allowance_fixed
- UC-293: Thiết lập social_insurance_salary
- UC-294: Kích hoạt/Vô hiệu hóa Contract

### 15.2. Shift Attendance (Staff)
- UC-295: Tạo Shift Attendance
- UC-296: Xem danh sách Shift Attendance
- UC-297: Xem chi tiết Shift Attendance
- UC-298: Cập nhật Shift Attendance
- UC-299: Duyệt Shift Attendance
- UC-300: Tính regular_hours từ Shift Attendance

### 15.3. Monthly Work Hours
- UC-301: Tạo Monthly Work Hours
- UC-302: Tính teaching_hours từ Sessions (Teacher)
- UC-303: Tính regular_hours từ Shift Attendance (Staff)
- UC-304: Tính total_hours
- UC-305: Tính overtime_hours
- UC-306: Lock Monthly Work Hours (is_locked)
- UC-307: Xem Monthly Work Hours
- UC-308: Cập nhật Monthly Work Hours (trước khi lock)

### 15.4. Payroll Runs
- UC-309: Tạo Payroll Run (DRAFT)
- UC-310: Xem danh sách Payroll Runs
- UC-311: Xem chi tiết Payroll Run
- UC-312: Tính Payroll Lines từ Session Roles (Teacher)
- UC-313: Tính Payroll Lines từ Contracts (Staff)
- UC-314: Tính Payroll Lines Overtime từ Monthly Work Hours
- UC-315: Thêm Payroll Line ALLOWANCE
- UC-316: Thêm Payroll Line DEDUCTION
- UC-317: Duyệt Payroll Run (APPROVED)
- UC-318: Xem tổng lương của nhân viên

### 15.5. Payroll Payments
- UC-319: Tạo Payroll Payment
- UC-320: Thanh toán lương (BANK_TRANSFER/CASH)
- UC-321: Ghi Cashbook Entry cho Payroll Payment
- UC-322: Cập nhật Payroll Lines là đã thanh toán
- UC-323: Đánh dấu Payroll Run đã thanh toán (PAID)
- UC-324: Xem lịch sử Payroll Payments

---

## 16. Notifications 🔴

### 16.1. Gửi thông báo
- UC-325: Tạo Notification Template
- UC-326: Xem danh sách Notification Templates
- UC-327: Cập nhật Notification Template
- UC-328: Gửi Notification qua Zalo OA
- UC-329: Gửi Notification qua Email
- UC-330: Gửi Notification qua Push
- UC-331: Gửi thông báo nhắc lịch học
- UC-332: Gửi thông báo nhắc bài tập
- UC-333: Gửi thông báo nhắc học phí
- UC-334: Gửi thông báo buổi bù
- UC-335: Gửi thông báo Mission
- UC-336: Gửi thông báo Media
- UC-337: Gửi thông báo Monthly Report
- UC-338: Xem trạng thái Notification (PENDING/SENT/FAILED)
- UC-339: Retry Notification nếu FAILED

---

## 17. Tickets & Support 🟢

### 17.1. Quản lý Tickets
- UC-340: Parent/Student tạo Ticket
- UC-341: Xem danh sách Tickets
- UC-342: Xem chi tiết Ticket
- UC-343: Gán Ticket cho Staff/Teacher
- UC-344: Cập nhật trạng thái Ticket (OPEN/IN_PROGRESS/RESOLVED/CLOSED)
- UC-345: Thêm comment vào Ticket
- UC-346: Upload attachment vào Ticket
- UC-347: Xem lịch sử Ticket
- UC-348: Theo dõi SLA phản hồi Ticket

---

## 18. Blog Management 🟢

### 18.1. Quản lý Blog Posts
- UC-349: Admin/Staff tạo Blog Post
- UC-350: Xem danh sách Blog Posts
- UC-351: Xem chi tiết Blog Post
- UC-352: Cập nhật Blog Post
- UC-353: Xóa mềm Blog Post
- UC-354: Publish Blog Post
- UC-355: Unpublish Blog Post
- UC-356: Hiển thị Blog Post trên Landing Page
- UC-357: Upload featured image cho Blog Post

---

## 19. Dashboard & Reports 🟡

### 19.1. Dashboard
- UC-358: Xem Dashboard học vụ (attendance, homework completion, MakeUpCredit usage)
- UC-359: Xem Dashboard tài chính (doanh thu, PayOS vs cash, công nợ)
- UC-360: Xem Dashboard nhân sự (payroll, work hours)
- UC-361: Xem Dashboard Lead (conversion, no-show, touch count)
- UC-362: Filter Dashboard theo Branch
- UC-363: Filter Dashboard theo date range

### 19.2. Audit & Logs
- UC-364: Xem Audit Logs
- UC-365: Filter Audit Logs theo actor
- UC-366: Filter Audit Logs theo entity
- UC-367: Xem data_before và data_after trong Audit Log
- UC-368: Backup dữ liệu
- UC-369: Restore dữ liệu

---

## 20. System Administration 🟢

### 20.1. User Management
- UC-370: Tạo User
- UC-371: Xem danh sách Users
- UC-372: Xem chi tiết User
- UC-373: Cập nhật User
- UC-374: Xóa User
- UC-375: Gán role cho User
- UC-376: Gán branch cho Teacher/Staff

### 20.2. Profile Management
- UC-377: Tạo Profile (Parent/Student)
- UC-378: Xem danh sách Profiles
- UC-379: Xem chi tiết Profile
- UC-380: Cập nhật Profile
- UC-381: Xóa Profile
- UC-382: Link Parent với Student
- UC-383: Unlink Parent với Student

### 20.3. Branch Management
- UC-384: Tạo Branch
- UC-385: Xem danh sách Branches
- UC-386: Xem chi tiết Branch
- UC-387: Cập nhật Branch
- UC-388: Xóa Branch
- UC-389: Kích hoạt/Vô hiệu hóa Branch

---

**Tổng số Use Cases**: 389

### 🟢 Dễ (CRUD cơ bản, logic đơn giản)
- Program & Class Management: 28
- Enrollment: 9
- Exams & Test Results: 11
- Session Reports: 11
- Media Management: 15
- Tickets & Support: 9
- Blog Management: 9
- System Administration: 20
**Tổng**: 112 use cases

### 🟡 Trung bình (Logic nghiệp vụ phức tạp, tích hợp hệ thống, AI đã có người cover)
- Authentication & Authorization: 12
- CRM & Lead Management: 26
- Session & Schedule Management: 15
- Leave Request & Attendance: 14
- Makeup Credit Management: 12
- Homework & Exercises: 35
- Monthly Reports (AI): 14
- Gamification: 49
- Dashboard & Reports: 12
**Tổng**: 189 use cases

### 🔴 Khó (Tính toán phức tạp, workflow nhiều bước, tích hợp bên ngoài)
- Finance Management: 31
- Notifications: 15
- Payroll Management: 41
**Tổng**: 87 use cases


**Tỷ lệ**: Dễ 25% | Trung bình 52% | Khó 22%

