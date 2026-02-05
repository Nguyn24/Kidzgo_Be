using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Template : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceTokens",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "NotificationTemplates",
                columns: new[] { "Id", "Channel", "Code", "Content", "CreatedAt", "IsActive", "IsDeleted", "Placeholders", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Email", "SESSION_REMINDER", "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi học sắp tới của bạn:</p>\n<ul>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> {{location}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"session_title\",\"session_start_time\",\"class_name\",\"location\"]", "Nhắc nhở: Buổi học {{session_title}} sắp bắt đầu", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Email", "HOMEWORK_REMINDER", "<p>Xin chào,</p>\n<p>Bạn có bài tập sắp đến hạn:</p>\n<ul>\n    <li><strong>Tên bài tập:</strong> {{homework_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn nộp:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"homework_title\",\"due_date\",\"class_name\"]", "Nhắc nhở: Bài tập {{homework_title}} sắp đến hạn", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Email", "TUITION_REMINDER", "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về học phí sắp đến hạn:</p>\n<ul>\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\n    <li><strong>Số tiền:</strong> {{amount}} VNĐ</li>\n    <li><strong>Hạn thanh toán:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng thanh toán học phí trước thời hạn để đảm bảo việc học không bị gián đoạn.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"amount\",\"due_date\"]", "Nhắc nhở: Học phí của {{student_name}} sắp đến hạn", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Email", "MAKEUP_REMINDER", "<p>Xin chào,</p>\n<p>Đây là thông báo nhắc nhở về buổi bù sắp tới:</p>\n<ul>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\n    <li><strong>Địa điểm:</strong> {{location}}</li>\n</ul>\n<p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"session_title\",\"session_start_time\",\"class_name\",\"location\"]", "Nhắc nhở: Buổi bù {{session_title}} sắp bắt đầu", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Email", "MISSION_REMINDER", "<p>Xin chào,</p>\n<p>Bạn có nhiệm vụ sắp đến hạn:</p>\n<ul>\n    <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n    <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>\n</ul>\n<p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"mission_title\",\"due_date\",\"class_name\"]", "Nhắc nhở: Nhiệm vụ {{mission_title}} sắp kết thúc", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Email", "MEDIA_REMINDER", "<p>Xin chào,</p>\n<p>Lớp học của bạn vừa có nội dung mới:</p>\n<ul>\n    <li><strong>Tiêu đề:</strong> {{media_title}}</li>\n    <li><strong>Loại:</strong> {{media_type}}</li>\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\n</ul>\n<p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"media_title\",\"media_type\",\"class_name\"]", "Thông báo: Có {{media_type}} mới từ lớp {{class_name}}", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId",
                schema: "public",
                table: "DeviceTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId_DeviceId_IsActive",
                schema: "public",
                table: "DeviceTokens",
                columns: new[] { "UserId", "DeviceId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId_IsActive",
                schema: "public",
                table: "DeviceTokens",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceTokens",
                schema: "public");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));
        }
    }
}
