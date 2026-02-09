using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToStudentMonthlyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean up duplicate records before adding unique constraint
            // Keep the most recent report (highest Id) for each (StudentProfileId, Month, Year) combination
            // Also delete related MonthlyReportData for duplicate reports
            migrationBuilder.Sql(@"
                -- Delete MonthlyReportData for duplicate reports (keep data for reports we're keeping)
                DELETE FROM public.""MonthlyReportData""
                WHERE ""ReportId"" IN (
                    SELECT r1.""Id""
                    FROM public.""StudentMonthlyReports"" r1
                    INNER JOIN public.""StudentMonthlyReports"" r2
                        ON r1.""StudentProfileId"" = r2.""StudentProfileId""
                        AND r1.""Month"" = r2.""Month""
                        AND r1.""Year"" = r2.""Year""
                        AND r1.""Id"" < r2.""Id""
                );

                -- Delete duplicate StudentMonthlyReports (keep the one with highest Id)
                DELETE FROM public.""StudentMonthlyReports"" r1
                USING public.""StudentMonthlyReports"" r2
                WHERE r1.""Id"" < r2.""Id""
                  AND r1.""StudentProfileId"" = r2.""StudentProfileId""
                  AND r1.""Month"" = r2.""Month""
                  AND r1.""Year"" = r2.""Year"";
            ");

            migrationBuilder.DropIndex(
                name: "IX_StudentMonthlyReports_StudentProfileId",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Content",
                value: "<p>Xin chào,</p>\r\n<p>Đây là thông báo nhắc nhở về buổi học sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\r\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\r\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\r\n</ul>\r\n<p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Content",
                value: "<p>Xin chào,</p>\r\n<p>Học sinh <strong>{{student_name}}</strong> có bài tập sắp đến hạn:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Tên bài tập:</strong> {{homework_title}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Hạn nộp:</strong> {{due_date}}</li>\r\n</ul>\r\n<p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Content",
                value: "<p>Xin chào,</p>\r\n<p>Đây là thông báo nhắc nhở về học phí sắp đến hạn của học sinh <strong>{{student_name}}</strong>:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Số tiền:</strong> {{amount}} VNĐ</li>\r\n    <li><strong>Hạn thanh toán:</strong> {{due_date}}</li>\r\n</ul>\r\n<p>Vui lòng thanh toán học phí trước thời hạn để đảm bảo việc học không bị gián đoạn.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Content",
                value: "<p>Xin chào,</p>\r\n<p>Đây là thông báo nhắc nhở về buổi bù sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\r\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\r\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\r\n</ul>\r\n<p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "Content",
                value: "<p>Xin chào,</p>\r\n<p>Học sinh <strong>{{student_name}}</strong> có nhiệm vụ sắp đến hạn:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>\r\n</ul>\r\n<p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Content",
                value: "<p>Xin chào,</p>\r\n<p>Lớp học của học sinh <strong>{{student_name}}</strong> vừa có nội dung mới:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Tiêu đề:</strong> {{media_title}}</li>\r\n    <li><strong>Loại:</strong> {{media_type}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n</ul>\r\n<p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_StudentProfileId_Month_Year",
                schema: "public",
                table: "StudentMonthlyReports",
                columns: new[] { "StudentProfileId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentMonthlyReports_StudentProfileId_Month_Year",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Content",
                value: "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi học sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Content",
                value: "<p>Xin chào,</p>\n<p>Học sinh <strong>{{student_name}}</strong> có bài tập sắp đến hạn:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Tên bài tập:</strong> {{homework_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn nộp:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

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
                column: "Content",
                value: "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi bù sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "Content",
                value: "<p>Xin chào,</p>\n<p>Học sinh <strong>{{student_name}}</strong> có nhiệm vụ sắp đến hạn:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Content",
                value: "<p>Xin chào,</p>\n<p>Lớp học của học sinh <strong>{{student_name}}</strong> vừa có nội dung mới:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Tiêu đề:</strong> {{media_title}}</li>\n    <li><strong>Loại:</strong> {{media_type}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n</ul>\n<p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_StudentProfileId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "StudentProfileId");
        }
    }
}
