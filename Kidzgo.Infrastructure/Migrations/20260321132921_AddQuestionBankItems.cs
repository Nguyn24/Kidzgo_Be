using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuestionType",
                schema: "public",
                table: "HomeworkQuestions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"HomeworkQuestions\" " +
                "ALTER COLUMN \"Options\" TYPE jsonb USING \"Options\"::jsonb;");

            migrationBuilder.CreateTable(
                name: "QuestionBankItems",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionBankItems_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#2b6cb0;\">Yêu cầu bảo lưu đã được duyệt</h2>\n  <p>Xin chào,</p>\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã được duyệt.</p>\n  <div style=\"background:#f7fafc;border:1px solid #e2e8f0;border-radius:8px;padding:12px;\">\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\n  </div>\n  <p>Vui lòng theo dõi lịch học sau thời gian bảo lưu.</p>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#c53030;\">Yêu cầu bảo lưu bị từ chối</h2>\n  <p>Xin chào,</p>\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã bị từ chối.</p>\n  <div style=\"background:#fff5f5;border:1px solid #fed7d7;border-radius:8px;padding:12px;\">\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\n  </div>\n  <p>Vui lòng liên hệ trung tâm nếu cần hỗ trợ thêm.</p>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#2f855a;\">Kết quả bảo lưu đã được cập nhật</h2>\n  <p>Xin chào,</p>\n  <p>Kết quả bảo lưu của <strong>{{student_name}}</strong> đã được cập nhật.</p>\n  <div style=\"background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;padding:12px;\">\n    <p><strong>Kết quả:</strong> {{outcome}}</p>\n    <p><strong>Ghi chú:</strong> {{outcome_note}}</p>\n  </div>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_ProgramId_Level",
                schema: "public",
                table: "QuestionBankItems",
                columns: new[] { "ProgramId", "Level" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionBankItems",
                schema: "public");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionType",
                schema: "public",
                table: "HomeworkQuestions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.Sql(
                "ALTER TABLE public.\"HomeworkQuestions\" " +
                "ALTER COLUMN \"Options\" TYPE text USING \"Options\"::text;");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#2b6cb0;\">Yêu cầu bảo lưu đã được duyệt</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã được duyệt.</p>\r\n  <div style=\"background:#f7fafc;border:1px solid #e2e8f0;border-radius:8px;padding:12px;\">\r\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\r\n  </div>\r\n  <p>Vui lòng theo dõi lịch học sau thời gian bảo lưu.</p>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#c53030;\">Yêu cầu bảo lưu bị từ chối</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã bị từ chối.</p>\r\n  <div style=\"background:#fff5f5;border:1px solid #fed7d7;border-radius:8px;padding:12px;\">\r\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\r\n  </div>\r\n  <p>Vui lòng liên hệ trung tâm nếu cần hỗ trợ thêm.</p>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#2f855a;\">Kết quả bảo lưu đã được cập nhật</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Kết quả bảo lưu của <strong>{{student_name}}</strong> đã được cập nhật.</p>\r\n  <div style=\"background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;padding:12px;\">\r\n    <p><strong>Kết quả:</strong> {{outcome}}</p>\r\n    <p><strong>Ghi chú:</strong> {{outcome_note}}</p>\r\n  </div>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>");
        }
    }
}
