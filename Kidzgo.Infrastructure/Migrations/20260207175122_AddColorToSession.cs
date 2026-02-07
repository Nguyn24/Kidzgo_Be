using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "public",
                table: "Sessions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                columns: new[] { "Body", "Subject" },
                values: new object[] { "<p>Xin chào {{user_name}},</p>\r\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\r\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\r\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\r\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", "Rex English - Đặt lại mật khẩu của bạn" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi học sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>\n<p>Trân trọng,<br/>Rex English Team</p>", "[\"session_title\",\"session_start_time\",\"class_name\",\"location\",\"student_name\",\"classroom_name\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Học sinh <strong>{{student_name}}</strong> có bài tập sắp đến hạn:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Tên bài tập:</strong> {{homework_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn nộp:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>\n<p>Trân trọng,<br/>Rex English Team</p>", "[\"homework_title\",\"due_date\",\"class_name\",\"student_name\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Content",
                value: "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về học phí sắp đến hạn của học sinh <strong>{{student_name}}</strong>:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Số tiền:</strong> {{amount}} VNĐ</li>\n    <li><strong>Hạn thanh toán:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng thanh toán học phí trước thời hạn để đảm bảo việc học không bị gián đoạn.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi bù sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>\n<p>Trân trọng,<br/>Rex English Team</p>", "[\"session_title\",\"session_start_time\",\"class_name\",\"location\",\"student_name\",\"classroom_name\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Học sinh <strong>{{student_name}}</strong> có nhiệm vụ sắp đến hạn:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>\n<p>Trân trọng,<br/>Rex English Team</p>", "[\"mission_title\",\"due_date\",\"class_name\",\"student_name\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Lớp học của học sinh <strong>{{student_name}}</strong> vừa có nội dung mới:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Tiêu đề:</strong> {{media_title}}</li>\n    <li><strong>Loại:</strong> {{media_type}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n</ul>\n<p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>\n<p>Trân trọng,<br/>Rex English Team</p>", "[\"media_title\",\"media_type\",\"class_name\",\"student_name\"]" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                schema: "public",
                table: "Sessions");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                columns: new[] { "Body", "Subject" },
                values: new object[] { "<p>Xin chào {{user_name}},</p>\r\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\r\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\r\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\r\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\r\n<p>Trân trọng,<br/>Kidzgo Team</p>", "Kidzgo - Đặt lại mật khẩu của bạn" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi học sắp tới của bạn:</p>\n<ul>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> {{location}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", "[\"session_title\",\"session_start_time\",\"class_name\",\"location\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Bạn có bài tập sắp đến hạn:</p>\n<ul>\n    <li><strong>Tên bài tập:</strong> {{homework_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn nộp:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", "[\"homework_title\",\"due_date\",\"class_name\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Content",
                value: "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về học phí sắp đến hạn:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Số tiền:</strong> {{amount}} VNĐ</li>\n    <li><strong>Hạn thanh toán:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng thanh toán học phí trước thời hạn để đảm bảo việc học không bị gián đoạn.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi bù sắp tới:</p>\n<ul>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> {{location}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", "[\"session_title\",\"session_start_time\",\"class_name\",\"location\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Bạn có nhiệm vụ sắp đến hạn:</p>\n<ul>\n    <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", "[\"mission_title\",\"due_date\",\"class_name\"]" });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "Content", "Placeholders" },
                values: new object[] { "<p>Xin chào,</p>\n<p>Lớp học của bạn vừa có nội dung mới:</p>\n<ul>\n    <li><strong>Tiêu đề:</strong> {{media_title}}</li>\n    <li><strong>Loại:</strong> {{media_type}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n</ul>\n<p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", "[\"media_title\",\"media_type\",\"class_name\"]" });
        }
    }
}
