using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTableForEnrollmentFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                schema: "public",
                table: "PlacementTests",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                schema: "public",
                table: "PlacementTests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutcomeCompletedAt",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OutcomeCompletedBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReassignedClassId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReassignedEnrollmentId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassEnrollmentScheduleSegments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    SessionSelectionPattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEnrollmentScheduleSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassEnrollmentScheduleSegments_ClassEnrollments_ClassEnrol~",
                        column: x => x.ClassEnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassScheduleSegments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    SchedulePattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassScheduleSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassScheduleSegments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("c1f73d87-8d4c-45c2-bf8f-3d79e2f4b6a1"),
                column: "Body",
                value: "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\r\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\r\n    <tr>\r\n      <td align=\"center\">\r\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\r\n          <tr>\r\n            <td style=\"padding:0;background:linear-gradient(135deg,#f97316 0%,#ea580c 100%);\">\r\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\r\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\r\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Đặt lại PIN phụ huynh</h1>\r\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\r\n                  Xin chào {{user_name}}, chúng tôi đã nhận được yêu cầu đặt lại PIN cho hồ sơ {{profile_name}}.\r\n                </p>\r\n              </div>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:26px 30px 12px 30px;\">\r\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\r\n                Để tiếp tục, vui lòng bấm vào nút bên dưới. Liên kết này chỉ có hiệu lực trong 1 giờ.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:8px 30px 28px 30px;\">\r\n              <a href=\"{{reset_link}}\" style=\"display:inline-block;background:#ea580c;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Đặt lại PIN</a>\r\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\r\n                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</div>");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_RoomId",
                schema: "public",
                table: "PlacementTests",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_OutcomeCompletedBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "OutcomeCompletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ReassignedClassId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ReassignedClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ReassignedEnrollmentId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ReassignedEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollmentScheduleSegments_ClassEnrollmentId_Effective~",
                schema: "public",
                table: "ClassEnrollmentScheduleSegments",
                columns: new[] { "ClassEnrollmentId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassScheduleSegments_ClassId_EffectiveFrom",
                schema: "public",
                table: "ClassScheduleSegments",
                columns: new[] { "ClassId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PauseEnrollmentRequests_ClassEnrollments_ReassignedEnrollme~",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ReassignedEnrollmentId",
                principalSchema: "public",
                principalTable: "ClassEnrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PauseEnrollmentRequests_Classes_ReassignedClassId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ReassignedClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PauseEnrollmentRequests_Users_OutcomeCompletedBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "OutcomeCompletedBy",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTests_Classrooms_RoomId",
                schema: "public",
                table: "PlacementTests",
                column: "RoomId",
                principalSchema: "public",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PauseEnrollmentRequests_ClassEnrollments_ReassignedEnrollme~",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PauseEnrollmentRequests_Classes_ReassignedClassId",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PauseEnrollmentRequests_Users_OutcomeCompletedBy",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTests_Classrooms_RoomId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropTable(
                name: "ClassEnrollmentScheduleSegments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ClassScheduleSegments",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTests_RoomId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropIndex(
                name: "IX_PauseEnrollmentRequests_OutcomeCompletedBy",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PauseEnrollmentRequests_ReassignedClassId",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PauseEnrollmentRequests_ReassignedEnrollmentId",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "RoomId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "OutcomeCompletedAt",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "OutcomeCompletedBy",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "ReassignedClassId",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "ReassignedEnrollmentId",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("c1f73d87-8d4c-45c2-bf8f-3d79e2f4b6a1"),
                column: "Body",
                value: "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\n    <tr>\n      <td align=\"center\">\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\n          <tr>\n            <td style=\"padding:0;background:linear-gradient(135deg,#f97316 0%,#ea580c 100%);\">\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Đặt lại PIN phụ huynh</h1>\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\n                  Xin chào {{user_name}}, chúng tôi đã nhận được yêu cầu đặt lại PIN cho hồ sơ {{profile_name}}.\n                </p>\n              </div>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:26px 30px 12px 30px;\">\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\n                Để tiếp tục, vui lòng bấm vào nút bên dưới. Liên kết này chỉ có hiệu lực trong 1 giờ.\n              </p>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:8px 30px 28px 30px;\">\n              <a href=\"{{reset_link}}\" style=\"display:inline-block;background:#ea580c;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Đặt lại PIN</a>\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\n                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\n              </p>\n            </td>\n          </tr>\n        </table>\n      </td>\n    </tr>\n  </table>\n</div>");
        }
    }
}
