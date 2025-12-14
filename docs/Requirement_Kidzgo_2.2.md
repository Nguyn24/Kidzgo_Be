3.1. Tên Đồ án Capstone
Tiếng Anh: Academic and Financial Management System for KidzGo English Center


Tiếng Việt: Hệ thống quản trị học vụ và tài chính cho Trung tâm tiếng Anh KidzGo


Tên viết tắt: KidzGo



a. Bối cảnh
Trung tâm tiếng Anh KidzGo là một trung tâm tiếng Anh tư nhân tập trung vào học viên nhỏ tuổi (4–15 tuổi), cung cấp các chương trình như Phonics, Cambridge Starters–Movers–Flyers và Giao tiếp. Trong định hướng phát triển, KidzGo dự kiến có nhiều chi nhánh (branch) tại các quận khác nhau, với mô hình quản lý tập trung.
Hiện tại, phần lớn hoạt động vẫn được quản lý thủ công, dẫn đến nhiều vấn đề:
Báo cáo tiến độ tháng & nhận xét giáo viên cho từng học viên được lập thủ công trên Excel, tách theo từng lớp và từng học viên, mất thời gian, khó chuẩn hóa, khó tra cứu lịch sử.


Thu học phí, tính lương giáo viên/nhân viên, theo dõi chi phí (tài liệu giảng dạy, cơ sở vật chất, tiền thuê phòng, sự kiện, lương staff…) rải rác trong nhiều file Excel khác nhau → khó kiểm soát dòng tiền thu – chi – tồn, thiếu minh bạch, khó đối soát.


Bài tập về nhà được giao và nộp qua các nhóm Zalo, không có cấu trúc thống nhất → khó theo dõi lịch sử bài tập, khó biết học sinh nào làm/không làm, không có điểm tổng hợp.


Nhắc lịch học, nhắc bài tập, nhắc học phí, thông báo sự kiện chủ yếu gửi tay qua Zalo → dễ bị trôi trong đoạn chat dài, phụ huynh khó tìm lại thông tin quan trọng (hóa đơn, TKB, link bài tập…).


Chưa có thư viện giáo án/bài giảng chuẩn hóa theo từng buổi; mỗi giáo viên có thể soạn cách khác nhau → khó duy trì chất lượng giảng dạy đồng nhất giữa các lớp, khó kiểm soát nội dung đã dạy.


Điểm danh vẫn ghi tay hoặc bằng bảng tính, chưa gắn rõ với chính sách học bù 24 giờ (MakeUpCredit) → dễ hiểu nhầm quyền lợi học bù, dễ thiếu công bằng giữa các học viên.


Bài kiểm tra xếp lớp (placement) và kiểm tra định kỳ (progress test, mid-term, final) vẫn thực hiện trên giấy hoặc form rời, điểm nhập lại thủ công vào Excel → khó tổng hợp, khó xem theo từng học viên xuyên suốt các khóa.


Nhiều học viên trong một giai đoạn phải học 2–3 lớp khác nhau (lớp chính + lớp bổ trợ kỹ năng + lớp học bù) → Excel + Zalo không đủ để xếp lớp, quản lý TKB tổng, theo dõi quyền lợi buổi học và học phí tương ứng.


Phụ huynh không có một cổng tập trung duy nhất để xem thời khóa biểu, bài tập, điểm danh, báo cáo, thông tin học phí, media (video/ảnh hoạt động) của con → thông tin phân tán khắp nơi (Zalo chat, Excel, Google Sheet, file PDF rời…).


Về quản lý nhân sự & lương, trung tâm cần:


Tính lương giáo viên/trợ giảng theo role (main teacher, teaching assistant, giáo viên phụ trách CLB/sự kiện) và số buổi đứng lớp.


Tính lương staff vận hành/kế toán/lễ tân theo hợp đồng lao động (full-time, part-time theo ca/giờ), chấm công, phụ cấp, thưởng/phạt.


Nhưng hiện tại phần lớn được xử lý bán thủ công trên Excel, dễ sai sót, khó audit.


Để giải quyết các vấn đề trên và sẵn sàng mở rộng nhiều chi nhánh, KidzGo hướng tới xây dựng một nền tảng tập trung với các trụ cột:
Hệ thống quản lý học vụ – tài chính – vận hành trên web cho admin, staff, giáo viên.


Zalo Mini App cho phụ huynh/học viên, tích hợp chặt với Zalo Official Account (OA).


Tích hợp PayOS để thu học phí qua QR/link và đối soát tự động.


Tích hợp dịch vụ AI để hỗ trợ chấm bài, luyện nói, sinh báo cáo tháng nháp cho từng học viên.


Áp dụng chính sách học bù 24h qua module MakeUpCredit.


Hệ thống gamification (mission, sao, level, phần thưởng) để tăng động lực tự học.


Multi-branch: quản lý nhiều chi nhánh; admin xem toàn hệ thống, giáo viên/staff gắn với một chi nhánh cố định.


Chuỗi quy trình end-to-end mà hệ thống hướng tới chuẩn hóa:
Lead & Tư vấn → Kiểm tra xếp lớp (thực hiện thủ công, nhập điểm lên hệ thống) → Ghi danh & Đăng ký → Xếp lớp (kể cả kết hợp đa lớp) → Giáo án & Bài tập → Điểm danh & Học bù (chính sách 24h, MakeUpCredit) → Bài kiểm tra định kỳ → Ghi chú buổi học → Báo cáo tháng hỗ trợ AI → Nhiệm vụ & Phần thưởng gamification → Thu học phí qua PayOS QR → Công nợ & Sổ quỹ → Chuẩn bị lương (GV + staff) & Theo dõi chi phí.

b. Giải pháp đề xuất
KidzGo được đề xuất dưới dạng nền tảng web kết hợp với Zalo Mini App dành cho phụ huynh và học viên, tích hợp chặt chẽ với Zalo OA, PayOS, dịch vụ AI và tài khoản ngân hàng.
1. Kiến trúc multi-branch & phân quyền
Hệ thống hỗ trợ nhiều chi nhánh:


Admin có thể đăng nhập và xem/điều hành toàn bộ chi nhánh (hoặc lọc theo từng chi nhánh).


Teacher / Staff (Operations, Accountant, Reception…) được gán một chi nhánh cố định, khi đăng nhập chỉ truy cập được dữ liệu của chi nhánh đó.


Tất cả lớp học, học viên, TKB, sổ quỹ, báo cáo đều gắn với Branch, đảm bảo tách biệt dữ liệu nhưng vẫn có tổng quan cho admin.


2. Tài khoản đăng nhập chung, chọn profile Parent/Student với khi chọn role parent phải nhập mã PIN
Người dùng đăng nhập bằng tài khoản đăng nhập chung (email + mật khẩu, ví dụ Gmail).


Sau khi login, hệ thống hiển thị danh sách profile thuộc tài khoản đó:


1 profile phụ huynh (Parent).


1 hoặc nhiều profile học sinh (Student).





Người dùng chọn profile để vào portal tương ứng:


Chọn Student → vào Student Portal.


Chọn Parent → vào Parent Portal. Yêu cầu phải có mã pin





Chức năng của Student/Parent và Teacher/Staff được tách rõ ràng về UI và permission.


3. Giao tiếp “Zalo-first” (Zalo Official Account doanh nghiệp)
Sử dụng Zalo Official Account (tài khoản doanh nghiệp) để:


Gửi nhắc lịch học, nhắc bài tập, lịch học bù.


Thông báo về báo cáo tháng đã sẵn sàng.


Nhắc đóng học phí, thông báo hóa đơn mới, thông báo sự kiện/CLB/workshop.


Mỗi thông báo có thể chứa deep link:


Link mở Zalo Mini App KidzGo đến đúng màn hình (TKB, hóa đơn, báo cáo, nhiệm vụ, album media…).


Hoặc link mở web portal nếu phụ huynh dùng máy tính.


4. Zalo Mini App cho Phụ huynh / Học viên
Zalo Mini App là kênh chính cho phụ huynh/học viên:
Xem thời khóa biểu, điểm danh, MakeUpCredits, bài tập.


Nhận thông báo thay đổi lịch học, hạn nộp bài, nhiệm vụ, nhắc học phí.


Xem báo cáo tiến độ tháng (HTML/PDF).


Xem kết quả kiểm tra xếp lớp, kiểm tra định kỳ.


Tham gia nhiệm vụ (mission), xem sao/level, xem và đổi phần thưởng.


Mở mã QR PayOS trực tiếp để thanh toán học phí.


Xem album media theo tháng:


Video/ảnh hoạt động chung (theo lớp).


Video/ảnh cá nhân từng học sinh (clip đọc thơ, speaking, work sample…).


5. Quản lý học vụ thông minh (lớp, buổi, điểm danh, học bù, giáo án, bài tập)
Quản lý class, session, timetable:


Mỗi lớp có lịch cố định, số buổi, giáo viên chính, trợ giảng, phòng học, chi nhánh.


Điểm danh & học bù với MakeUpCredit và luật 24h:


Giáo viên chỉ cần đánh dấu Present/Absent cho từng học sinh và bấm “Nộp điểm danh”.


Phụ huynh xin nghỉ được xử lý qua module Leave Requests của staff.


Hệ thống tự kết hợp:


Thông tin xin nghỉ (có phép ≥/≤24h, nghỉ dài ngày).


Kết quả điểm danh (Absent).


→ Tự gán loại vắng (có phép ≥24h, <24h, không phép, nghỉ dài ngày).


Tự động tạo MakeUpCredit nếu đủ điều kiện (≥24h hoặc nghỉ dài đã chấp thuận).


Staff dùng module Học bù để:


Dựa trên MakeUpCredit, tìm lớp tương đương.


Xếp học bù công bằng, nhất quán (Participation type = MAKEUP).


Giáo án chuẩn hóa theo từng buổi:


Admin/Staff tạo giáo án khung cho từng chương trình/khóa, theo format chi tiết giống file Excel (Period – Date – Teacher – Time – Book – Skills – Classwork – Required Materials – Homework – Note).


Khi mở lớp, hệ thống sinh sẵn danh sách buổi học và phiếu giáo án cho từng ngày.


Giáo viên xem giáo án khung, sau buổi học điền lại thực tế dạy, homework thực tế, ghi chú, rồi submit.


Form giao bài tập chuẩn:


Giáo viên dùng một form cố định để tạo bài tập:


Tên bài tập, mô tả, lớp/buổi áp dụng.


Sách, trang, kỹ năng, loại bài (video reading, workbook pages, quiz…).


Hạn nộp, cách nộp (file, ảnh, text, link…).


Thang điểm, sao thưởng, gắn với mission (nếu có).


Hệ thống tự gán bài tập cho từng học sinh trong lớp, theo dõi trạng thái Assigned – Submitted – Graded – Late – Missing.


Ghi danh & lead:


Nhận lead từ form tư vấn trên landing page hoặc Zalo.


Lên lịch và ghi nhận kết quả kiểm tra xếp lớp (test vẫn làm thủ công, nhập điểm lên hệ thống).


Hệ thống đề xuất lớp phù hợp theo độ tuổi, trình độ, lịch trống.


6. Kiểm tra định kỳ & nhập điểm
Các bài kiểm tra định kỳ (progress test, mid-term, final, speaking test…) được tổ chức thủ công tại lớp.


Teacher chọn lớp + loại bài kiểm tra, khai báo thang điểm, ngày thi, mô tả.


Hệ thống hiển thị danh sách học sinh, giáo viên nhập điểm & nhận xét ngắn cho từng bạn, có thể đính kèm ảnh/scan bài làm.


Kết quả:


Lưu vào hồ sơ học viên.


Dùng làm input cho báo cáo tháng/cuối khóa và cho phụ huynh xem trong portal.


7. Báo cáo tháng hỗ trợ AI (report pipeline)
Mỗi buổi học, bên cạnh điểm danh và giáo án, giáo viên có thể ghi nhận xét nhanh cho từng học sinh (thái độ, tham gia, điểm mạnh/yếu trong bài học).


Cuối tháng, hệ thống tổng hợp:


Attendance, homework completion, kết quả test.


Mission/sao/level.


Ghi chú từng buổi.


Gửi dữ liệu này lên dịch vụ AI để sinh bản nháp báo cáo tháng cho từng học sinh:


Đánh giá kỹ năng (listening/speaking/reading/writing).


Thái độ, chăm chỉ, homework.


Đề xuất cải thiện & định hướng.


Teacher xem từng bản nháp:


Chỉnh sửa nội dung cho chính xác, cá nhân hóa.


Bấm “Nộp báo cáo cho duyệt”.


Staff/Admin có quyền:


Xem danh sách báo cáo, trạng thái (Draft – Chờ duyệt – Đã duyệt – Trả lại).


Bình luận cho giáo viên (yêu cầu chỉnh sửa).


Tự chỉnh sửa nội dung nếu cần.


Phê duyệt (Approved).


Khi báo cáo được phê duyệt:


Nội dung được khóa (chỉ admin mới mở lại).


Hệ thống publish lên Parent/Student Portal và gửi thông báo Zalo OA/Mini App:


“Báo cáo tháng 5/2025 của bé Minh đã sẵn sàng”.


8. Gamification: Mission – Streak – Sao – Level – Cửa hàng phần thưởng
Staff/Teacher tạo Mission gắn với hoạt động cụ thể:


Ví dụ: “Làm đủ bài tập 7 ngày”, “Đọc RazKids 5 ngày liên tục”, “Không nghỉ không phép trong tuần”, “Nộp video reading Poetry 8”…


Mỗi mission có:


Tên, mô tả, đối tượng áp dụng (lớp/nhóm/học sinh).


Loại nhiệm vụ, thời gian hiệu lực.


Số sao thưởng khi hoàn thành.


Hệ thống theo dõi:


Streak làm bài/đọc sách.


Trạng thái nhiệm vụ của từng học sinh (Assigned, In Progress, Completed, Expired).


Khi học sinh hoàn thành:


Tự cộng sao (star points).


Sao tích lũy dùng để:


Tăng level học viên.


Đổi quà trong “Cửa hàng phần thưởng” (voucher, quà, ưu đãi do trung tâm định nghĩa).


Teacher/Staff có thể cộng/trừ sao thủ công với lý do (thưởng/phạt đặc biệt).


9. Quản lý tài chính, PayOS, sổ quỹ & lương
Hóa đơn & thanh toán:


Hệ thống xuất hóa đơn cho:


Học phí khóa chính.


Học phí lớp bổ trợ/học thêm.


Phí học bù thu thêm (nếu policy có).


Phí tài liệu, phí sự kiện/CLB.


Tích hợp PayOS:


Mỗi hóa đơn có QR động / link thanh toán riêng.


Khi phụ huynh chuyển khoản, PayOS gọi webhook → hệ thống tự:


Đánh dấu hóa đơn đã thanh toán.


Cập nhật công nợ & báo cáo tuổi nợ.


Ghi dòng Thu vào sổ quỹ (cash-in).


Học phí theo khóa & nhiều lớp:


Học phí khóa chính tính trọn gói theo số buổi (vd 24 buổi/3 tháng).


Mỗi buổi/điểm danh có phân loại:


MAIN – buổi thuộc lớp chính, đã nằm trong học phí.


MAKEUP – buổi học bù, dùng MakeUpCredit, không thu thêm.


EXTRA_PAID – buổi học thêm/lớp bổ trợ, tính phí thêm.


FREE/TRIAL – buổi miễn phí/học thử.


Hệ thống dùng đơn giá tham chiếu theo buổi (UnitPrice) để tính tiền cho EXTRA_PAID nếu cần, tránh double-charge.


Sổ quỹ nội bộ:


Mọi khoản thu (PayOS, tiền mặt) được ghi Thu với mô tả, loại thu.


Mọi khoản chi (lương GV, lương staff, chi phí cơ sở vật chất, sự kiện…) được ghi Chi, liên kết với bảng lương/hóa đơn chi.


Hệ thống hiển thị số dư thu – chi – tồn theo kỳ, theo chi nhánh.


Lương giáo viên / trợ giảng:


Lương dạy học không tính tay mà dựa trên dữ liệu session + role:


Mỗi buổi có PlannedTeacher và ActualTeacher (giáo viên thực sự dạy, xác nhận qua nút “Nộp điểm danh”).


Mỗi giáo viên trong buổi được gắn role:


Main Teacher.


Teaching Assistant.


Giáo viên CLB/Workshop/Event.


Mỗi role + chương trình có đơn giá/buổi & phụ cấp riêng.


Lương kỳ = tổng (số buổi thực dạy × đơn giá) + phụ cấp (nếu có).


Lương staff vận hành/kế toán/lễ tân & hợp đồng lao động:


Hệ thống lưu hợp đồng lao động:


Loại hợp đồng (thử việc, thời vụ, có thời hạn, không thời hạn).


Ngày hiệu lực (từ–đến), lương cơ bản, đơn giá giờ/ca, phụ cấp cố định.


Với staff full-time: lương tháng dựa trên lương cơ bản ± điều chỉnh (chấm công, nghỉ phép không lương, thưởng/phạt).


Với staff part-time: có dữ liệu ca làm việc/giờ công, đơn giá, phụ cấp OT/event.


Bảng lương kỳ gồm:


Teaching staff (theo buổi/role).


Non-teaching staff (theo hợp đồng & chấm công).


Sau khi kế toán chuyển khoản lương qua Internet Banking:


Đánh dấu từng dòng lương “Đã trả”.


Tự động ghi Chi tương ứng trong sổ quỹ.


10. Dashboard, sự kiện & kỳ thi nội bộ
Dashboard học vụ:


Tỷ lệ điểm danh theo lớp/chương trình/chi nhánh.


Tỷ lệ hoàn thành bài tập.


Mức độ sử dụng MakeUpCredit.


Phân bố level / sao / mission completion.


Dashboard tài chính:


Doanh thu theo tháng/kỳ/chương trình/chi nhánh.


Tỷ lệ thu qua PayOS vs tiền mặt.


Công nợ, tuổi nợ, dòng tiền thu–chi–tồn.


Sự kiện & kỳ thi nội bộ:


Quản lý các CLB, workshop, sự kiện theo mùa.


Quản lý lịch & kết quả các kỳ kiểm tra nội bộ/thi lên lớp, gắn kết quả với hồ sơ học viên.



c. Yêu cầu chức năng (Functional Requirements)
1. Khách truy cập (Customer / Visitor – Landing Page)
Xem giới thiệu KidzGo, triết lý giáo dục, phương pháp giảng dạy.


Xem chương trình & cấp độ: Phonics, Starters–Movers–Flyers, Giao tiếp cho từng độ tuổi.


Xem thông tin chi nhánh: địa điểm, lịch học, thông tin liên hệ.


Liên hệ trung tâm qua Zalo OA, Messenger, điện thoại.


Gửi form tư vấn; dữ liệu lead được lưu vào module CRM đơn giản để staff follow-up.



2. Người dùng – Phụ huynh / Học viên (Parent / Student Portal)
2.1. Đăng nhập & chọn profile
Đăng nhập bằng email + mật khẩu (Login Account).


Chọn profile sau khi login:


Parent profile hoặc Student profile tương ứng.


Nếu tài khoản có profile Teacher/Staff:


Khi chọn Teacher/Staff, hệ thống yêu cầu nhập PIN.


2.2. Thời khóa biểu & điểm danh
Xem TKB theo tuần/tháng, lọc theo con (nếu phụ huynh có nhiều con).


Nhận nhắc lịch học qua Zalo OA/Mini App trước giờ học.


Xem lịch sử điểm danh từng buổi:


Present, Nghỉ phép ≥24h, Nghỉ phép <24h, Nghỉ không phép, Học bù (MakeUp).


Xem danh sách MakeUpCredits hiện có và các buổi học bù đã được xếp.


2.3. Bài tập & nộp bài
Xem danh sách bài tập (theo lớp, theo hạn nộp, theo trạng thái).


Xem hướng dẫn chi tiết, sách, trang, kỹ năng yêu cầu.


Nộp bài:


Upload file/ảnh, nhập text, gửi link (tùy loại bài).


Xem điểm, nhận xét của giáo viên và (nếu có) kết quả chấm tự động từ AI.


Xem lịch sử bài tập đã làm/đã trễ/chưa nộp.


2.4. Báo cáo & kiểm tra
Xem báo cáo tháng (HTML hoặc tải PDF) do AI hỗ trợ và giáo viên/nhà trường phê duyệt.


Xem kết quả:


Kiểm tra xếp lớp (placement).


Kiểm tra định kỳ (progress, mid-term, final, speaking…).


Xem lịch sử báo cáo & điểm kiểm tra xuyên suốt các khóa.


2.5. Thanh toán
Xem danh sách hóa đơn:


Học phí khóa chính.


Học phí lớp bổ trợ/học thêm.


Phí tài liệu, sự kiện…


Xem trạng thái thanh toán: Chưa trả, Đã trả, Quá hạn.


Nhấn để mở mã QR PayOS cho từng hóa đơn và thanh toán trong app ngân hàng.


Xem lịch sử thanh toán (theo tháng, theo con, theo chương trình).


2.6. Gamification & nhiệm vụ
Xem danh sách nhiệm vụ hiện tại của học sinh:


Streak homework, streak reading, nhiệm vụ lớp, nhiệm vụ cá nhân.


Theo dõi:


Streak làm bài/đọc sách.


Số sao/điểm hiện có và tổng tích lũy.


Level của học sinh và điều kiện lên level tiếp theo.


Xem cửa hàng phần thưởng:


Danh sách quà/voucher, số sao cần để đổi.


2.7. Media & nhật ký học tập
Xem album theo tháng:


Ảnh/video hoạt động chung của lớp.


Ảnh/video cá nhân học sinh (reading video, speaking video, hình bài làm…).


Cho phép phụ huynh tải (nếu trung tâm cho phép) hoặc chỉ xem.


2.8. Giao tiếp & hỗ trợ
Nhận thông báo từ Zalo OA/Mini App về:


Hóa đơn mới, báo cáo mới, TKB thay đổi, nhiệm vụ mới…


Gửi phản hồi, góp ý, hoặc mở ticket hỗ trợ:


Chọn lớp, giáo viên, bộ phận (kế toán, vận hành…).


Xem lịch sử ticket và phản hồi từ nhà trường.



3. Giáo viên / Trợ giảng (Teacher / Teaching Assistant Portal)
3.1. Đăng nhập & chi nhánh
Đăng nhập qua tài khoản riêng hoặc profile Teacher.


Nếu dùng chung với tài khoản phụ huynh, phải nhập PIN khi chọn profile Teacher.


Sau login, hệ thống cố định chi nhánh của giáo viên, không cho tự đổi.


3.2. Quản lý lớp & thời khóa biểu
Xem danh sách lớp được phân công (main / TA).


Xem TKB cá nhân:


Buổi dạy, phòng, lớp, chương trình, chi nhánh.


Xem chi tiết từng buổi (Session):


Giáo án khung, danh sách học sinh, link điểm danh, bài tập, kiểm tra.


3.3. Giáo án & thực tế dạy
Truy cập giáo án buổi học theo format chuẩn (giống file Excel):


Warm up, từng sách, từng hoạt động, thời lượng, kỹ năng, Classwork, Required materials, Homework.


Sau khi dạy:


Xác nhận nội dung đã thực hiện, đánh dấu phần nào dạy/chưa dạy.


Cập nhật homework thực tế đã giao (nếu khác khung).


Ghi chú về buổi học (tình hình lớp, vấn đề phát sinh).


Submit giáo án thực tế.


3.4. Điểm danh & học bù
Màn hình điểm danh đơn giản:


Danh sách học sinh + tick Present/Absent.


Sau khi chọn, bấm “Nộp điểm danh”:


Hệ thống lưu điểm danh học sinh.


Gán ActualTeacher = giáo viên đang đăng nhập, TeacherAttendanceStatus = Present.


Hệ thống tự:

Kết hợp với Leave Requests đã duyệt để xác định loại vắng.


Tạo MakeUpCredit (nếu đủ điều kiện).


Giáo viên không cần nhớ luật 24h hay phân loại vắng.


3.5. Bài tập & chấm điểm
Tạo bài tập bằng form chuẩn:


Chọn lớp/buổi, sách, trang, kỹ năng, hạn nộp, cách nộp, thang điểm, sao thưởng.


Giao bài cho cả lớp hoặc từng học sinh.


Xem danh sách bài nộp:


Xem nội dung, file, ảnh, link.


Chấm điểm, viết nhận xét.


Nếu bài có AI:


Xem kết quả chấm tự động (score, feedback) → có thể chỉnh sửa trước khi lưu.


3.6. Kiểm tra định kỳ
Tạo kỳ kiểm tra cho lớp: loại test, ngày, thang điểm, mô tả.


Nhập điểm và nhận xét cho từng học sinh sau khi thi.


Upload ảnh/scan bài test (nếu cần).


Kết quả dùng cho báo cáo & viewer của phụ huynh.


3.7. Ghi chú buổi học & báo cáo tháng (AI)
Trong mỗi buổi, với từng học sinh:


Ghi quick notes: thái độ, tham gia, khó khăn, tiến bộ.


Cuối tháng:


Xem bản nháp báo cáo do AI tạo dựa trên attendance, homework, test, mission, notes.


Sửa lại nội dung, bổ sung ví dụ cụ thể.


Gửi báo cáo cho Staff/Admin phê duyệt.


3.8. Mission & gamification
Tạo mission cho lớp hoặc cho nhóm học sinh:


Cấu hình điều kiện, thời gian, sao thưởng.


Đánh dấu hoàn thành một số nhiệm vụ đặc biệt cần xác nhận thủ công (ví dụ: project, reading log).


Xem sao/level của học sinh để dùng làm động lực trong lớp.


3.9. Ticket & phản hồi phụ huynh
Xem các ticket liên quan đến lớp mình:


Ví dụ: phụ huynh hỏi về homework, xin hỗ trợ thêm tài liệu.


Trả lời hoặc phối hợp với staff/manager để giải quyết.



4. Nhân sự kế toán (Accountant Staff)
Quản lý hóa đơn & biên lai:


Tạo hóa đơn học phí, học thêm, tài liệu, sự kiện.


Xuất biên lai PDF nếu phụ huynh cần in.


Tích hợp PayOS & đối soát:


Tạo link/QR PayOS cho hóa đơn.


Nhận webhook thanh toán thành công → tự đối soát và cập nhật trạng thái.


Quản lý công nợ & tuổi nợ:


Xem danh sách hóa đơn chưa thanh toán, quá hạn theo học sinh/lớp/chương trình.


Xuất báo cáo tuổi nợ.


Quản lý sổ quỹ & chi phí:


Ghi nhận mọi khoản thu/chi.


Nhập chi phí thủ công, đính kèm ảnh hóa đơn, phân loại chi phí.


Chuẩn bị lương:


Lấy dữ liệu buổi dạy (Session) & role giáo viên để tính lương GV/TA.


Lấy dữ liệu hợp đồng, chấm công, ca làm việc để tính lương staff.


Ghi nhận thưởng, phụ cấp, phạt.


Xuất bảng lương (CSV/XLSX/PDF) để chuyển khoản ngân hàng.


Sau khi chuyển, đánh dấu “Đã trả” và ghi chi phí vào sổ quỹ.


Báo cáo & xuất file:


Doanh thu, chi phí, lợi nhuận đơn giản theo kỳ, chi nhánh.



5. Nhân sự vận hành (Management Staff – Operations)
Lead & ghi danh:


Nhận lead từ landing page & Zalo.


Lên lịch kiểm tra xếp lớp, nhập kết quả.


Ghi danh và xếp học viên vào lớp phù hợp (theo chi nhánh, trình độ, lịch).


Xếp lớp & TKB:


Tạo/cập nhật lớp, thời khóa biểu, giáo viên, phòng học.


Giải quyết xung đột lịch (giáo viên, phòng).


Hỗ trợ mô hình đa lớp (học sinh học lớp chính + lớp bổ trợ + lớp bù).


Học bù & MakeUpCredit:


Duyệt yêu cầu nghỉ (Leave Request): 1 buổi hoặc nghỉ dài ngày.


Hệ thống tự map buổi vắng & tạo MakeUpCredit nếu đủ điều kiện.


Sử dụng module học bù để:


Tìm lớp tương đương.


Xếp học bù công bằng, hợp lý, tránh trùng lịch.


Giáo án & chất lượng:


Quản lý thư viện giáo án khung cho từng chương trình/cấp độ/chi nhánh.


Gán giáo án cho lớp, giám sát việc thực hiện qua phiếu giáo án thực tế.


Báo cáo & nhắc nhở:


Theo dõi tiến độ giáo viên hoàn thành:


Điểm danh.


Ghi chú buổi học.


Bài tập.


Báo cáo tháng.


Gửi nhắc nhở qua hệ thống/ Zalo OA nội bộ.


Hồ sơ học viên & giao tiếp:


Xem toàn bộ lịch sử của mỗi học viên:


Lớp đã học, điểm danh, MakeUpCredit, test, báo cáo, mission/sao.


Gửi thông báo broadcast cho một nhóm phụ huynh/lớp/chi nhánh.



6. Quản lý / Admin (Manager / Admin)
Dashboard tổng quan & KPI:


KPI học vụ: attendance, homework completion, MakeUpCredit usage, mission completion.


KPI tài chính: doanh thu, tỷ lệ thu tiền, công nợ, dòng tiền, phân tách theo chi nhánh.


Quản lý người dùng & phân quyền (RBAC):


Tạo/sửa/vô hiệu hóa tài khoản nhân sự, giáo viên, TA, admin.


Cấu hình quyền theo role, theo chi nhánh.


Quản lý học vụ & sự kiện:


Giám sát lớp, phòng, chương trình, lịch tổng.


Quản lý lịch các event: CLB, workshop, sự kiện nội bộ, kỳ thi lớn.


Phê duyệt báo cáo tháng:


Duyệt hoặc yêu cầu chỉnh sửa báo cáo tháng của học sinh (do AI + giáo viên soạn).


Xuất file để lưu trữ nội bộ.


Cấu hình & chính sách:


Cấu hình:


Chính sách học bù (24h, số MakeUpCredit tối đa).


Quy tắc mission, level, sao & phần thưởng.


Đơn giá lương theo chương trình/role.


Các tham số hệ thống khác (số buổi/khóa, kỳ báo cáo…).



d. Yêu cầu phi chức năng (Non-functional Requirements)
Khả năng tích hợp (Interoperability)
Tích hợp với Zalo OA, PayOS, dịch vụ AI, hệ thống tài khoản ngân hàng (qua API hoặc file).


Hỗ trợ các trình duyệt hiện đại (Chrome, Firefox, Edge).


Layout responsive cho laptop, tablet, và tối ưu trải nghiệm trong Zalo Mini App (mobile-first).


Hiệu năng (Performance)
Các thao tác người dùng phổ biến (xem TKB, xem báo cáo, xem hóa đơn, điểm danh) nên phản hồi trong vòng ~1 giây trong điều kiện tải bình thường.


Webhook PayOS, yêu cầu AI & job tính lương được xử lý qua tác vụ nền bất đồng bộ để giao diện luôn mượt.


Khả năng mở rộng (Scalability)
Thiết kế ban đầu cho 1 trung tâm, nhưng dễ mở rộng nhiều chi nhánh.


Kiến trúc mô-đun: dễ thêm chương trình mới, module hoạt động ngoại khóa, thi online… mà không phải viết lại toàn hệ thống.


Bảo mật (Security)
Áp dụng RBAC cho tất cả vai trò người dùng, phân tách rõ quyền giữa Admin/Staff/Teacher/Parent/Student.


Mã hóa dữ liệu nhạy cảm khi truyền (TLS/HTTPS) và khi lưu trong CSDL (password, PIN, token).


Cơ chế xác thực, chính sách mật khẩu & PIN phù hợp cho web và Zalo Mini App.


Giới hạn truy cập theo chi nhánh đối với giáo viên & staff.


Độ tin cậy (Reliability)
Cơ chế retry tự động cho webhook PayOS, job gửi báo cáo, job AI.


Backup CSDL định kỳ; có quy trình restore.


Lưu log giao dịch, log sửa dữ liệu quan trọng (audit trail).


Tính dễ sử dụng (Usability)
Giao diện rõ ràng, tối giản cho giáo viên (chỉ cần vài cú click để điểm danh, giao bài, nhập điểm).


Nhân sự mới có thể sử dụng các chức năng chính sau 15–30 phút training.


Phụ huynh có thể:


Xem TKB, bài tập, báo cáo.


Thanh toán học phí.


Xem album media, nhiệm vụ của con.
 → chỉ với rất ít thao tác chạm trong Zalo Mini App.



CÁC VẤN ĐỀ VÀ GIẢI PHÁP NGHIỆP VỤ
(Dùng để giải thích rõ hơn các lựa chọn thiết kế hệ thống)
1. Thu học phí, trả lương nhưng không có OpenAPI ngân hàng
(giữ nguyên như bạn đã viết – đã rất rõ)
 → Dùng PayOS cho chiều thu (IN), bảng lương + sổ quỹ cho chiều chi (OUT), đối soát thủ công nhưng có hệ thống hỗ trợ.
2. Học phí theo khóa, nhưng học sinh có 1 lớp chính + nhiều lớp khác
(giữ nguyên phần bạn đã viết về UnitPrice, phân loại MAIN/MAKEUP/EXTRA_PAID/FREE)
3. Học sinh học khóa 3 tháng, off 2 tuần – bù & thanh toán sao?
(giữ nguyên phần LeaveRequest, MakeUpCredit, học phí không giảm, nhắc thanh toán theo Invoice Due Date)
4. Điểm danh giáo viên & ai được tính lương khi dạy thay
(giữ nguyên phần PlannedTeacher/ActualTeacher, nút “Nộp điểm danh” là chứng nhận)
5. Lương giáo viên / trợ giảng được tính từ dữ liệu nào?
(giữ nguyên phần Session + role + đơn giá, phụ cấp event)
6. Lương staff vận hành / kế toán / lễ tân dựa vào đâu?
(giữ nguyên phần full-time vs part-time, ShiftAttendance, BaseSalary…)
7. Sổ quỹ & liên kết với PayOS + lương
(giữ nguyên phần Cash-in/Cash-out + ảnh chứng từ)
8. Quản lý xin nghỉ, điểm danh & MakeUpCredits (GV chỉ Present/Absent, Staff xử lý luật 24h & nghỉ dài)
(giữ nguyên phần bạn đã có)

9. Multi-branch & phân quyền chi nhánh
Vấn đề
Trung tâm có (hoặc sẽ có) nhiều chi nhánh, nhưng hiện tại mọi thứ còn xử lý chung chung một bộ Excel → khó biết doanh thu, chi phí, số lớp theo từng chi nhánh.


Nếu không quản lý theo chi nhánh, giáo viên/staff có thể vô tình sửa nhầm dữ liệu không thuộc cơ sở mình.


Giải pháp
Mỗi lớp, học viên, hóa đơn, sổ quỹ, hợp đồng nhân sự… đều gắn với Branch.


Admin được phép xem và thao tác trên tất cả chi nhánh.


Teacher/Staff chỉ được gán vào một Branch cố định:


Khi đăng nhập, hệ thống luôn set context = branch đó.


Không thể đổi sang branch khác nếu không có quyền admin.



10. Tài khoản dùng chung & chọn profile Parent/Student/Teacher + PIN
Vấn đề
Nhiều phụ huynh có 2–3 con học tại trung tâm; một số giáo viên cũng đồng thời là phụ huynh.


Nếu tạo tài khoản hoàn toàn tách biệt sẽ gây rối (quá nhiều email/mật khẩu), khó cho phụ huynh.


Giải pháp
Dùng Login Account chung (email + mật khẩu).


Sau login, hiển thị danh sách profile:


Parent, các Student (con), (option) Teacher/Staff.


Khi chọn Teacher/Staff, bắt buộc nhập PIN:


Ngăn trường hợp phụ huynh/học sinh chạm nhầm vào profile giáo viên.


Tăng bảo mật cho quyền truy cập nội bộ.



11. Nhiệm vụ – sao – level – cửa hàng phần thưởng
Vấn đề
Khó duy trì động lực cho học sinh nhỏ tuổi chỉ bằng điểm số.


Muốn có cơ chế game hóa, nhưng vẫn phải gắn chặt vào bài tập/hoạt động thật.


Giải pháp
Xây dựng module Mission:


Nhiệm vụ được tạo bởi Teacher/Staff dựa trên bài tập, attendance, reading.


Hệ thống tự đo streak/thời gian hoàn thành.


Hoàn thành mission → cộng sao.


Sao dùng để:


Nâng level của học viên.


Đổi quà trong cửa hàng phần thưởng.


Sao được quản lý bằng transaction có lý do, tránh lạm dụng.



12. Giáo án khung và phiếu giáo án thực tế từng buổi
Vấn đề
Không có giáo án chuẩn cho từng buổi, mỗi giáo viên soạn một kiểu.


Khi nhiều lớp cùng cấp độ, khó đảm bảo học giống nhau; khó kiểm tra chất lượng.


Giải pháp
Admin/Staff thiết kế giáo án khung theo khóa/chương trình:


Cấu trúc tương tự file Excel mẫu (Warm up, từng sách, từng activity, thời lượng…).


Khi mở lớp, hệ thống sinh các phiếu giáo án cho từng buổi/period.


Giáo viên:


Dựa trên giáo án khung để dạy.


Sau buổi học, điền lại “thực tế dạy” + homework + ghi chú.


Tài liệu này:


Giúp quản lý xem lớp có dạy đúng plan hay không.


Là input cho AI trong báo cáo.



13. Media – album theo tháng cho phụ huynh
Vấn đề
Hiện tại video/ảnh quá trình học được gửi rải rác qua Zalo – khó tra cứu, khó lưu trữ lâu dài.


Giải pháp
Tạo module Media Gallery:


Teacher/Staff upload video/ảnh hoạt động:


Gắn tag: lớp, học sinh (nếu cá nhân), loại hoạt động, tháng.


Trong Parent/Student Portal:


Mục “Album theo tháng”:


Album lớp (ảnh hoạt động chung).


Album cá nhân (clip/ảnh riêng của con).


Giúp phụ huynh theo dõi hành trình của con như “nhóm Zalo có tổ chức”.



14. Hợp đồng lao động có thời hạn & tính lương staff
Vấn đề
Nhiều staff có hợp đồng thời hạn (6 tháng, 1 năm…) hoặc làm theo ca.


Nếu không quản lý hợp đồng trong hệ thống, việc tính lương rất dễ sai (tính cho thời gian ngoài hợp đồng, quên phụ cấp…).


Giải pháp
Mỗi nhân sự có bản Hợp đồng lao động trong hệ thống:


Loại hợp đồng, thời hạn, lương cơ bản, đơn giá giờ/ca, phụ cấp cố định.


Khi tính lương:


Hệ thống chỉ sử dụng hợp đồng còn hiệu lực trong kỳ.


Nếu hợp đồng hết hạn, admin phải gia hạn/tạo hợp đồng mới → tránh trả lương “vô hạn”.


Lương staff lấy từ:


BaseSalary (full-time) + chấm công + phụ cấp.


Số giờ/ca × đơn giá (part-time) + phụ cấp event/OT.



15. Luồng báo cáo tháng hỗ trợ AI
Vấn đề
Báo cáo tháng đang làm tay trên Word/Excel, tốn rất nhiều thời gian của giáo viên.


Nội dung giữa các lớp không đồng nhất, nhiều khi thiếu dữ liệu (attendance, homework, test, mission…).


Giải pháp
Mỗi buổi, giáo viên ghi nhận notes nhanh cho từng học sinh.


Cuối tháng:


Hệ thống gom toàn bộ dữ liệu tháng của từng học sinh.


Gửi dịch vụ AI sinh draft báo cáo theo format chuẩn.


Teacher:


Chỉnh sửa, cá nhân hóa.


Submit để Staff/Admin phê duyệt.


Staff/Admin:


Có quyền comment, chỉnh sửa, approve.


Báo cáo được:


Lưu trữ tập trung.


Hiển thị cho phụ huynh trong portal, giúp minh bạch & chuyên nghiệp.

