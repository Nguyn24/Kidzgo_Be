using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registrations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreferredSchedule = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassAssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EntryType = table.Column<string>(type: "text", nullable: true),
                    OriginalRegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationType = table.Column<string>(type: "text", nullable: true),
                    TotalSessions = table.Column<int>(type: "integer", nullable: false),
                    UsedSessions = table.Column<int>(type: "integer", nullable: false),
                    RemainingSessions = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registrations_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Registrations_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registrations_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registrations_Registrations_OriginalRegistrationId",
                        column: x => x.OriginalRegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Registrations_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_BranchId",
                schema: "public",
                table: "Registrations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ClassId",
                schema: "public",
                table: "Registrations",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_OriginalRegistrationId",
                schema: "public",
                table: "Registrations",
                column: "OriginalRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ProgramId",
                schema: "public",
                table: "Registrations",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_StudentProfileId",
                schema: "public",
                table: "Registrations",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TuitionPlanId",
                schema: "public",
                table: "Registrations",
                column: "TuitionPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrations",
                schema: "public");

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
        }
    }
}
