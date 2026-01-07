Auth + Select Profile + PIN + /me


Master data + Timetable/Class/Session - Huy


Attendance + Leave + MakeUp - Huy


Homework + Submission + Grading


Invoices + PayOS create-link + webhook


Tests + Reports + Tickets + Notifications + Upload - Huy

1) Auth + Chọn Profile (CỰC QUAN TRỌNG để FE chạy được app)
Login/Refresh
POST /auth/login (email, password) → token + danh sách profile link


POST /auth/refresh → token mới


POST /auth/logout


Me + Profiles
GET /me → role, branchId, permissions, profile đang chọn


GET /me/profiles → danh sách profile (Parent/Student) thuộc login account


POST /me/select-profile → chọn profileId (set context)


PIN (Parent portal)
POST /parent/pin/verify (profileParentId, pin)


POST /parent/pin/set (set lần đầu) / POST /parent/pin/reset-request (nếu có email flow)



2) Master Data (để FE render dropdown/label)
GET /branches (Admin thấy all; Staff/Teacher thấy 1)


GET /programs (Phonics/Cambridge/Communication…)


GET /levels


GET /rooms?branchId=


GET /roles (main teacher, TA…)


GET /lookups (enum: attendanceStatus, sessionType MAIN/MAKEUP/EXTRA_PAID…, invoiceStatus, homeworkStatus…)


Nhóm này giúp FE dựng UI sạch (select/options) mà không hard-code.

3) Timetable + Class + Session (xương sống học vụ)
Danh sách lớp
Teacher: GET /teacher/classes


Parent/Student: GET /students/{studentId}/classes


Thời khóa biểu
Student: GET /students/{studentId}/timetable?from=&to=


Teacher: GET /teacher/timetable?from=&to=


(Optional) Parent xem theo con: dùng studentId


Chi tiết buổi học (Session)
GET /sessions/{sessionId} → giờ học, phòng, GV, loại buổi, lesson plan link, attendance summary…



4) Attendance + Leave Request + MakeUpCredit (module “khó” nên làm sớm)
Điểm danh (Teacher)
GET /sessions/{sessionId}/attendance → danh sách học sinh + trạng thái hiện tại + cờ MakeUp


POST /sessions/{sessionId}/attendance/submit
 Body: actualTeacherId, list {studentId, status(PRESENT/ABSENT), note?}


Lịch sử điểm danh (Parent/Student)
GET /students/{studentId}/attendance?from=&to=


Leave Requests (Parent/Staff tạo)
POST /leave-requests (studentId, dateFrom, dateTo, reason, type one-day/long, createdAt)


GET /students/{studentId}/leave-requests


Staff duyệt: POST /leave-requests/{id}/approve | POST /leave-requests/{id}/reject | POST /leave-requests/{id}/cancel


MakeUpCredits + Book lịch học bù
GET /students/{studentId}/makeup-credits (balance + history)


GET /students/{studentId}/makeup/options?from=&to= → danh sách session bù hợp lệ


POST /makeup-bookings (studentId, targetSessionId, makeupCreditId)


FE sẽ có đủ để dựng: “Xin nghỉ” → “Theo dõi duyệt” → “Số credit” → “Chọn buổi bù”.

5) Homework + Submission + Grading (để FE có “vòng lặp học tập”)
Homework (Teacher tạo)
POST /classes/{classId}/homeworks


GET /classes/{classId}/homeworks


Homework (Student/Parent xem)
GET /students/{studentId}/homeworks?status=&from=&to=


GET /homeworks/{homeworkId}


Nộp bài
POST /homeworks/{homeworkId}/submissions (studentId, content/text, attachments[])


GET /homeworks/{homeworkId}/submissions?studentId=


Chấm điểm (Teacher)
POST /submissions/{submissionId}/grade (score, comment, aiFeedback?, starsAwarded?)



6) Tests / Placement / Progress (nhập điểm & xem kết quả)
POST /classes/{classId}/tests (type: placement/progress/mid/final…, date, rubric)


GET /classes/{classId}/tests


GET /tests/{testId}


POST /tests/{testId}/scores/bulk (list {studentId, score, comment, attachments?})


Parent/Student: GET /students/{studentId}/tests?type=



7) Monthly Report (AI pipeline có thể stub trước)
GET /teacher/reports?month= → list report draft theo lớp/học viên + status


GET /reports/{reportId} → nội dung HTML/JSON


POST /reports/{reportId}/submit (teacher submit)


Staff/Admin: POST /reports/{reportId}/comment


Staff/Admin: POST /reports/{reportId}/approve


Parent/Student: GET /students/{studentId}/reports?month= + GET /reports/{reportId}/pdf (hoặc exportUrl)


Lúc đầu AI có thể “mock”: BE trả về draft text mẫu để FE làm UI review/approve trước.

8) Invoices + PayOS (để Parent portal chạy phần tài chính)
Invoices (Parent)
GET /parents/{parentId}/invoices?status=


GET /invoices/{invoiceId}


Tạo link/QR PayOS
POST /invoices/{invoiceId}/payos/create-link → checkoutUrl, qrCodeData


Webhook PayOS (BE-only nhưng cần sớm để demo end-to-end)
POST /webhooks/payos → update paid status + ghi Cashbook


Payment history
GET /parents/{parentId}/payments?from=&to=



9) Ticketing + Notifications (để thay Zalo chat rời rạc)
Tickets
POST /tickets (profileId, category: teacher/accountant/ops, subject, message)


GET /tickets?mine=true&status=


GET /tickets/{ticketId}


POST /tickets/{ticketId}/messages (reply thread)


Notifications feed (FE cần để show inbox)
GET /notifications?profileId=


Admin/Staff broadcast: POST /notifications/broadcast (role/branch/class/student filters)


Zalo OA gửi thật có thể làm sau; trước mắt FE vẫn đọc “Notification center” từ DB.

10) Upload/Media (phục vụ homework/report/test evidence + album)
POST /uploads/presign (hoặc POST /uploads multipart) → fileUrl


POST /media (fileUrl, tags: classId, studentId, month, type)


GET /students/{studentId}/media?month=


Staff approve: POST /media/{id}/approve | POST /media/{id}/reject
