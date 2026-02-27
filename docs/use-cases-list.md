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

## 10. Session Reports 🟡

**Lưu ý**: Session Reports được sử dụng làm nguồn dữ liệu chính cho Monthly Reports (Use Case 11). Khi tạo Monthly Report, hệ thống sẽ aggregate tất cả Session Reports trong tháng đó.

### 10.1. Báo cáo buổi học
- UC-163: Teacher tạo Session Report
- UC-164: Teacher ghi feedback cho từng học sinh
- UC-165: Xem danh sách Session Reports
- UC-166: Xem chi tiết Session Report
- UC-167: Cập nhật Session Report
- UC-168: Filter Session Reports theo date range
- UC-169: Xem Session Reports của giáo viên trong tháng

### 10.2. Tổng hợp báo cáo tháng
- UC-170: Tổng hợp Session Reports theo date range (dùng cho Monthly Report)
- UC-171: AI generate summary từ Session Reports (dùng cho Monthly Report)
- UC-172: Teacher xem và chỉnh sửa AI summary
- UC-173: Đánh dấu Session Report đã được tổng hợp (is_monthly_compiled) - tự động khi tạo Monthly Report

### 10.3. Review và Publish Session Reports
- UC-174: AI generate draft Session Report (từ feedback và summary)
- UC-175: Teacher submit Session Report cho duyệt (DRAFT → REVIEW)
- UC-176: Staff/Admin approve Session Report (REVIEW → APPROVED)
- UC-177: Staff/Admin reject Session Report (REVIEW → REJECTED)
- UC-178: Publish Session Report (APPROVED → PUBLISHED)
- UC-179: Parent xem Session Report của con (chỉ xem các report đã PUBLISHED)
- UC-180: Student xem Session Report của mình (chỉ xem các report đã PUBLISHED)
- UC-180*: Admin/ManagementStaff comment trên Session Report

---

## 11. Monthly Reports (AI) 🟡

**Lưu ý**: Monthly Reports được tổng hợp từ nhiều nguồn dữ liệu, trong đó **Session Reports (Use Case 10)** là nguồn quan trọng nhất.

### 11.1. Tạo và quản lý Monthly Reports
- UC-182: Tạo Monthly Report Job
- UC-183: Gom dữ liệu cho Monthly Report (session reports, attendance, homework, test, mission, notes)
- UC-184: AI tạo draft Monthly Report
- UC-185: Xem danh sách Monthly Report Jobs
- UC-186: Xem trạng thái Monthly Report Job (PENDING/GENERATING/DONE/FAILED)

### 11.2. Review và Publish Monthly Reports
- UC-187: Teacher xem draft Monthly Report
- UC-188: Teacher chỉnh sửa draft Monthly Report
- UC-189: Teacher submit Monthly Report (REVIEW)
- UC-190: Staff/Admin approve Monthly Report
- UC-191: Staff/Admin reject Monthly Report
- UC-192: Publish Monthly Report
- UC-193: Parent/Student xem Monthly Report
- UC-194: Gửi thông báo khi publish Monthly Report
- UC-195: Comment trên Monthly Report

---

## 12. Gamification 🟡

### 12.1. Missions
- UC-195: Tạo Mission (CLASS/STUDENT/GROUP scope)
- UC-196: Xem danh sách Missions
- UC-197: Xem chi tiết Mission
- UC-198: Cập nhật Mission
- UC-199: Xóa Mission
- UC-200: Thiết lập reward_stars cho Mission
- UC-201: Thiết lập reward_exp cho Mission
- UC-202: Thiết lập total_questions cho Mission
- UC-203: Thiết lập progress_per_question cho Mission
- UC-204: Track progress của Mission
- UC-205: Hoàn thành Mission (COMPLETED)
- UC-206: Xem progress bar của Mission

### 12.2. Stars & XP
- UC-207: Cộng Stars khi hoàn thành Mission
- UC-208: Cộng Stars khi hoàn thành Homework
- UC-209: Cộng Stars khi điểm danh (Attendance Streak)
- UC-210: Cộng XP khi hoàn thành Mission
- UC-211: Cộng XP khi điểm danh (Attendance Streak)
- UC-212: Teacher/Staff cộng Stars thủ công
- UC-213: Teacher/Staff trừ Stars thủ công
- UC-214: Teacher/Staff cộng XP thủ công
- UC-215: Teacher/Staff trừ XP thủ công
- UC-216: Xem lịch sử Star Transactions
- UC-217: Xem balance Stars hiện tại
- UC-218: Tính Level từ XP
- UC-219: Xem Level và XP hiện tại

### 12.3. Attendance Streak
- UC-220: Học sinh điểm danh hàng ngày
- UC-221: Tự động tạo Attendance Streak record
- UC-222: Cộng Stars khi điểm danh (1 star)
- UC-223: Cộng XP khi điểm danh (5 exp)
- UC-224: Tính current_streak (số ngày liên tiếp)
- UC-225: Cập nhật max_streak
- UC-226: Xem Attendance Streak của học sinh
- UC-227: Reset streak khi bỏ lỡ ngày

### 12.4. Reward Store
- UC-228: Tạo Reward Store Item
- UC-229: Xem danh sách Reward Store Items
- UC-230: Xem chi tiết Reward Store Item
- UC-231: Cập nhật Reward Store Item
- UC-232: Xóa mềm Reward Store Item
- UC-233: Thiết lập cost_stars cho Item
- UC-234: Quản lý quantity của Item

### 12.5. Reward Redemption
- UC-235: Học sinh đổi quà (Request)
- UC-236: Xem danh sách Reward Redemptions
- UC-237: Xem chi tiết Reward Redemption
- UC-238: Staff duyệt Reward Redemption (APPROVED)
- UC-239: Staff từ chối Reward Redemption (CANCELLED)
- UC-240: Staff trao quà (DELIVERED)
- UC-241: Học sinh xác nhận nhận quà (RECEIVED)
- UC-242: Tự động xác nhận sau 3 ngày (nếu không xác nhận)
- UC-243: Lưu item_name tại thời điểm đổi
- UC-244: Trừ Stars khi đổi quà

---

## 13. Media Management 🟢

### 13.1. Upload và quản lý Media
- UC-245: Teacher/Staff upload ảnh/video
- UC-245a: Generate presigned URL for upload (S3/Cloud Storage)
- UC-246: Gắn tag Class cho Media
- UC-247: Gắn tag Student cho Media
- UC-248: Gắn tag Tháng (YYYY-MM) cho Media
- UC-248a: Gắn tag Type cho Media (HOMEWORK/REPORT/TEST/ALBUM/CLASS_PHOTO)
- UC-249: Thiết lập visibility (CLASS_ONLY/PERSONAL/PUBLIC_PARENT)
- UC-250: Xem danh sách Media
- UC-251: Xem chi tiết Media
- UC-252: Cập nhật Media
- UC-253: Xóa Media
- UC-254: Staff/Admin duyệt Media
- UC-255: Publish Media lên gallery

### 13.2. Xem Media
- UC-256: Parent/Student xem album lớp
- UC-257: Parent/Student xem album cá nhân
- UC-258: Filter Media theo tháng
- UC-259: Download Media (nếu được phép)

---

## 14. Finance Management 🔴

### 14.1. Invoices
- UC-260: Tạo Invoice (MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE)
- UC-261: Xem danh sách Invoices
- UC-261a: Xem danh sách Invoices của Parent (filter theo parentId, status)
- UC-262: Xem chi tiết Invoice
- UC-263: Cập nhật Invoice
- UC-264: Hủy Invoice (CANCELLED)
- UC-265: Tạo Invoice Lines
- UC-266: Gắn session_ids vào Invoice Line
- UC-267: Sinh PayOS payment link
- UC-268: Sinh PayOS QR code
- UC-269: Xem trạng thái Invoice (PENDING/PAID/OVERDUE)
- UC-270: Đánh dấu Invoice OVERDUE

### 14.2. Payments
- UC-271: Thanh toán qua PayOS (webhook) - tự động update invoice status + ghi Cashbook Entry
- UC-272: Thanh toán bằng tiền mặt
- UC-273: Thanh toán chuyển khoản
- UC-274: Xác nhận thanh toán (Staff)
- UC-275: Upload chứng từ thanh toán
- UC-276: Cập nhật Invoice status sau thanh toán
- UC-277: Xem lịch sử Payments của Invoice
- UC-277a: Xem lịch sử Payments của Parent (filter theo from/to)

### 14.3. Cashbook
**Role: AccountantStaff, Admin, ManagementStaff**
- UC-278: Tạo Cashbook Entry (CASH_IN/CASH_OUT)
- UC-279: Xem danh sách Cashbook Entries
- UC-280: Xem chi tiết Cashbook Entry
- UC-281: Gắn Cashbook Entry với Invoice (CASH_IN)
- UC-282: Gắn Cashbook Entry với Payroll Payment (CASH_OUT)
- UC-283: Gắn Cashbook Entry với Expense (CASH_OUT)
- UC-284: Filter Cashbook theo date range
- UC-285: Xem tổng thu/chi theo tháng

### 14.4. Công nợ
**Role: AccountantStaff, Admin, ManagementStaff**
- UC-286: Xem danh sách công nợ
- UC-287: Xem công nợ của học sinh
- UC-288: Xem tuổi nợ
- UC-289: Báo cáo công nợ
- UC-290: Gửi nhắc nhở công nợ

---

## 15. Payroll Management 🔴

### 15.1. Contracts
- UC-291: Tạo Contract
- UC-292: Xem danh sách Contracts
- UC-293: Xem chi tiết Contract
- UC-294: Cập nhật Contract
- UC-295: Thiết lập base_salary
- UC-296: Thiết lập hourly_rate
- UC-297: Thiết lập minimum_monthly_hours
- UC-298: Thiết lập overtime_rate_multiplier
- UC-299: Thiết lập allowance_fixed
- UC-300: Thiết lập social_insurance_salary
- UC-301: Kích hoạt/Vô hiệu hóa Contract

### 15.2. Shift Attendance (Staff)
- UC-302: Tạo Shift Attendance
- UC-303: Xem danh sách Shift Attendance
- UC-304: Xem chi tiết Shift Attendance
- UC-305: Cập nhật Shift Attendance
- UC-306: Duyệt Shift Attendance
- UC-307: Tính regular_hours từ Shift Attendance

### 15.3. Monthly Work Hours
- UC-308: Tạo Monthly Work Hours
- UC-309: Tính teaching_hours từ Sessions (Teacher)
- UC-310: Tính regular_hours từ Shift Attendance (Staff)
- UC-311: Tính total_hours
- UC-312: Tính overtime_hours
- UC-313: Lock Monthly Work Hours (is_locked)
- UC-314: Xem Monthly Work Hours
- UC-315: Cập nhật Monthly Work Hours (trước khi lock)

### 15.4. Payroll Runs
- UC-316: Tạo Payroll Run (DRAFT)
- UC-317: Xem danh sách Payroll Runs
- UC-318: Xem chi tiết Payroll Run
- UC-319: Tính Payroll Lines từ Session Roles (Teacher)
- UC-320: Tính Payroll Lines từ Contracts (Staff)
- UC-321: Tính Payroll Lines Overtime từ Monthly Work Hours
- UC-322: Thêm Payroll Line ALLOWANCE
- UC-323: Thêm Payroll Line DEDUCTION
- UC-324: Duyệt Payroll Run (APPROVED)
- UC-325: Xem tổng lương của nhân viên

### 15.5. Payroll Payments
- UC-326: Tạo Payroll Payment
- UC-327: Thanh toán lương (BANK_TRANSFER/CASH)
- UC-328: Ghi Cashbook Entry cho Payroll Payment
- UC-329: Cập nhật Payroll Lines là đã thanh toán
- UC-330: Đánh dấu Payroll Run đã thanh toán (PAID)
- UC-331: Xem lịch sử Payroll Payments

---

## 16. Notifications 🔴

### 16.1. Gửi thông báo
- UC-332: Tạo Notification Template
- UC-333: Xem danh sách Notification Templates + xem chi tiết
- UC-334: Cập nhật Notification Template
- UC-334a: Xóa mềm Noti template
- UC-335: Gửi Notification qua Zalo OA
- UC-336: Gửi Notification qua Email (event domain)
- UC-337: Gửi Notification qua Push
- UC-338: Gửi thông báo nhắc lịch học
- UC-339: Gửi thông báo nhắc bài tập
- UC-340: Gửi thông báo nhắc học phí
- UC-341: Gửi thông báo buổi bù (makeup)
- UC-342: Gửi thông báo Mission
- UC-343: Gửi thông báo Media
- UC-344: Gửi thông báo Monthly Report
- UC-345: Xem trạng thái Notification (PENDING/SENT/FAILED)
- UC-346: Retry Notification nếu FAILED

---

## 17. Tickets & Support 🟢

### 17.1. Quản lý Tickets
- UC-347: Parent/Student tạo Ticket
- UC-348: Xem danh sách Tickets
- UC-349: Xem chi tiết Ticket
- UC-350: Gán Ticket cho Staff/Teacher
- UC-351: Cập nhật trạng thái Ticket (OPEN/IN_PROGRESS/RESOLVED/CLOSED)
- UC-352: Thêm comment vào Ticket
- UC-353: Upload attachment vào Ticket
- UC-354: Xem lịch sử Ticket
- UC-355: Theo dõi SLA phản hồi Ticket

---

## 18. Blog Management 🟢

### 18.1. Quản lý Blog Posts
- UC-356: Admin/Staff tạo Blog Post
- UC-357: Xem danh sách Blog Posts
- UC-358: Xem chi tiết Blog Post
- UC-359: Cập nhật Blog Post
- UC-360: Xóa mềm Blog Post
- UC-361: Publish Blog Post
- UC-362: Unpublish Blog Post
- UC-363: Hiển thị Blog Post trên Landing Page
- UC-364: Upload featured image cho Blog Post

---

## 19. Dashboard & Reports 🟡

### 19.1. Dashboard
- UC-365: Xem Dashboard học vụ (attendance, homework completion, MakeUpCredit usage)
- UC-366: Xem Dashboard tài chính (doanh thu, PayOS vs cash, công nợ)
- UC-367: Xem Dashboard nhân sự (payroll, work hours)
- UC-368: Xem Dashboard Lead (conversion, no-show, touch count)
- UC-369: Filter Dashboard theo Branch
- UC-370: Filter Dashboard theo date range

### 19.2. Audit & Logs
- UC-371: Xem Audit Logs
- UC-372: Filter Audit Logs theo actor
- UC-373: Filter Audit Logs theo entity
- UC-374: Xem data_before và data_after trong Audit Log
- UC-375: Backup dữ liệu
- UC-376: Restore dữ liệu

---

## 20. System Administration 🟢

### 20.1. User Management
- UC-377: Tạo User
- UC-378: Xem danh sách Users
- UC-379: Xem chi tiết User
- UC-380: Cập nhật User
- UC-381: Xóa User
- UC-382: Gán role cho User
- UC-383: Gán branch cho Teacher/Staff

### 20.2. Profile Management
- UC-384: Tạo Profile (Parent/Student)
- UC-385: Xem danh sách Profiles
- UC-386: Xem chi tiết Profile
- UC-387: Cập nhật Profile
- UC-388: Xóa Profile
- UC-389: Link Parent với Student
- UC-390: Unlink Parent với Student

### 20.3. Branch Management
- UC-391: Tạo Branch
- UC-392: Xem danh sách Branches
- UC-393: Xem chi tiết Branch
- UC-394: Cập nhật Branch
- UC-395: Xóa Branch
- UC-396: Kích hoạt/Vô hiệu hóa Branch

---

**Tổng số Use Cases**: 397

### 🟢 Dễ (CRUD cơ bản, logic đơn giản)
- Program & Class Management: 28
- Enrollment: 9
- Exams & Test Results: 11
- Session Reports: 19
- Media Management: 15
- Tickets & Support: 9
- Blog Management: 9
- System Administration: 20

### 🟡 Trung bình (Logic nghiệp vụ phức tạp, tích hợp hệ thống)
- Authentication & Authorization: 12
- CRM & Lead Management: 26
- Session & Schedule Management: 15
- Leave Request & Attendance: 14
- Makeup Credit Management: 12
- Homework & Exercises: 35
- Monthly Reports (AI): 14
- Gamification: 38
- Dashboard & Reports: 12

### 🔴 Khó (Tính toán phức tạp, workflow nhiều bước, tích hợp bên ngoài)
- Finance Management: 31
- Notifications: 15
- Payroll Management: 41
