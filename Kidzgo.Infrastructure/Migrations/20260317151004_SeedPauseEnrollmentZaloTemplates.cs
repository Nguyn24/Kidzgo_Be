using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPauseEnrollmentZaloTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "NotificationTemplates",
                columns: new[] { "Id", "Channel", "Code", "Content", "CreatedAt", "IsActive", "IsDeleted", "Placeholders", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Email", "PAUSE_ENROLLMENT_APPROVED_EMAIL", "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#2b6cb0;\">Yêu cầu bảo lưu đã được duyệt</h2>\n  <p>Xin chào,</p>\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã được duyệt.</p>\n  <div style=\"background:#f7fafc;border:1px solid #e2e8f0;border-radius:8px;padding:12px;\">\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\n  </div>\n  <p>Vui lòng theo dõi lịch học sau thời gian bảo lưu.</p>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Yêu cầu bảo lưu đã được duyệt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Email", "PAUSE_ENROLLMENT_REJECTED_EMAIL", "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#c53030;\">Yêu cầu bảo lưu bị từ chối</h2>\n  <p>Xin chào,</p>\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã bị từ chối.</p>\n  <div style=\"background:#fff5f5;border:1px solid #fed7d7;border-radius:8px;padding:12px;\">\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\n  </div>\n  <p>Vui lòng liên hệ trung tâm nếu cần hỗ trợ thêm.</p>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Yêu cầu bảo lưu bị từ chối", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Email", "PAUSE_ENROLLMENT_OUTCOME_EMAIL", "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#2f855a;\">Kết quả bảo lưu đã được cập nhật</h2>\n  <p>Xin chào,</p>\n  <p>Kết quả bảo lưu của <strong>{{student_name}}</strong> đã được cập nhật.</p>\n  <div style=\"background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;padding:12px;\">\n    <p><strong>Kết quả:</strong> {{outcome}}</p>\n    <p><strong>Ghi chú:</strong> {{outcome_note}}</p>\n  </div>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"outcome\",\"outcome_note\"]", "Kết quả bảo lưu đã được cập nhật", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Push", "PAUSE_ENROLLMENT_APPROVED_PUSH", "Yêu cầu bảo lưu {{pause_from}} - {{pause_to}} đã được duyệt.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"pause_from\",\"pause_to\"]", "Bảo lưu đã được duyệt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Push", "PAUSE_ENROLLMENT_REJECTED_PUSH", "Yêu cầu bảo lưu {{pause_from}} - {{pause_to}} đã bị từ chối.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"pause_from\",\"pause_to\"]", "Bảo lưu bị từ chối", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Push", "PAUSE_ENROLLMENT_OUTCOME_PUSH", "Kết quả bảo lưu: {{outcome}}.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"outcome\"]", "Kết quả bảo lưu đã cập nhật", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "ZaloOa", "PAUSE_ENROLLMENT_APPROVED_ZALO", "Yêu cầu bảo lưu của {{student_name}} từ {{pause_from}} đến {{pause_to}} đã được duyệt.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Bảo lưu đã được duyệt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "ZaloOa", "PAUSE_ENROLLMENT_REJECTED_ZALO", "Yêu cầu bảo lưu của {{student_name}} từ {{pause_from}} đến {{pause_to}} đã bị từ chối.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Bảo lưu bị từ chối", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "ZaloOa", "PAUSE_ENROLLMENT_OUTCOME_ZALO", "Kết quả bảo lưu của {{student_name}}: {{outcome}}. {{outcome_note}}", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"outcome\",\"outcome_note\"]", "Kết quả bảo lưu đã cập nhật", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
        }
    }
}
