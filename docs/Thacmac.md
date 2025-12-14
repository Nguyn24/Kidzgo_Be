-Có flow gửi thông báo qua email cho user hay không?
Có nhưng mà chưa biết luồng nào cần gửi để tăng tính chuyển nghiệp
Tài khoản web và zalominiapp có thống nhất không ( web là gmail và mật khẩu còn zalo là tài khoản zalo làm sao đồng bộ dư liệu cần tìm hiểu thêm)
- 	Tại sao staff được comment trên report của học sinh (bảng student_monthly_reports)?
Staff comment ( bình luận) trên bảng report để tương tác( nhắc nhở) teacher để sửa lại report như comment trên gg doc vậy á… ( khi comment vào nó sẽ liên kết với thông báo để thông báo cho giáo viên biết để mà 
sửa)
- 	XP để lên level nên tính riêng thay vì tính chung với sao để dễ lên level up rank?
Chốt lại là tính theo sao để lên level chứ hong phải dựa vào experience
-	Kiểm tra placement test thì online hay offline?
Sau khi kiểm tra thủ công ở lớp ( offline), thằng staff có nhiệm vụ là báo kết quả và điểm tới tài khoản student và parent qua notification.
-	Vẫn còn khúc cấn ở bảng placement_test (cách tính điểm đầu vào ở trung tâm)?
Điểm đầu vào ở trung tâm chính là các bài test đã có form tạo sẵn cho 4 kĩ năng ( speaking, listening , reading và writing) theo trình độ. Có nghĩa là 1 bạn vào test thử thì trung tâm sẽ chọn đề mà trung tâm đã soạn sẵn ( form đề tương tự như form tạo đề của teacher chỉ bê qua thôi á), test thẳng trên web ở kĩ năng reading và listening form có sẵn, còn speaking và writing phải có ô nhập điểm và nhận xét. 
- 	Tính lương có liên quan đến hợp đồng thì bên dev được biết không. Có cần chi tiết về bảo hiểm, an sinh, phúc lợi không?
Lương thì staff sẽ có mức lương cố định 1 tháng, còn về teacher parttime thì sẽ có nhiều type ( teacher, assistant teaching và assistant club, sẽ còn nhiều type nữa về role teacher có nghĩa là tự tạo ra nhiều type nữa bởi admin và mức  lương từng types sẽ tính theo tiếng). Còn fulltime teacher thì lương theo tháng ( sau này). 
Hợp đồng đơn giản như vậy: 
Tạo Hợp đồng: Admin hoặc HR nhập các thông tin trên, bao gồm mức lương cơ bản và mức lương đóng BHXH.
Kích hoạt: Sau khi hợp đồng được duyệt và chuyển sang trạng thái "Đang hoạt động".
Hệ thống Tính Lương: Khi chạy payroll_runs, hệ thống sẽ tự động tra cứu Hợp đồng đang "Đang hoạt động" của giáo viên để lấy:
Mức Lương Cơ Bản (để tạo dòng BASE).
Mức Lương Đóng BHXH (để tính khấu trừ DEDUCTION).
Đơn giá Giờ dạy (để tính toán các dòng TEACHING/TA/CLUB...).
-	Cần Thịnh cụ thể cái vấn đề payroll line về component type?
Teaching full time- Thu nhập từ giờ dạy chính thức- Lương cứng khoán theo tháng 15.000.000đ/tháng
Teaching part time-  Thu nhập từ giờ dạy chính thức- Tính theo giờ: 100k/giờ 
TA ( Teaching  assistant)- Thu nhập từ việc làm trợ giảng- Tính theo giờ: 50k/giờ
Workshop- Thu nhập từ việc tham gia hoặc tổ chức workshop- Tính theo giờ: 50k/giờ
Teaching Overtime - Lương làm thêm giờ -  Tính theo giờ: 200k/ giờ
TA Overtime - Lương làm thêm giờ-  Tính theo giờ: 100k/ giờ
Gửi video- Thu nhập từ việc lọc và gửi video quá trình học của các lớp- Tính theo giờ: 50k/giờ
Phụ cấp giao bài và chấm bài - Thu  nhập từ việc chấm và giao BTVN -  Tính theo tháng 100k/lớp/tháng
Phụ cấp khác - Phụ cấp xăng xe, điên thoại, trách nhiệm- Tùy chỉnh
