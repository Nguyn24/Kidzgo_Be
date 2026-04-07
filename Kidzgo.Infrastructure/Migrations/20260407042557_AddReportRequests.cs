using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportRequests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedTeacherUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetStudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Month = table.Column<int>(type: "integer", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LinkedSessionReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedMonthlyReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Classes_TargetClassId",
                        column: x => x.TargetClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Profiles_TargetStudentProfileId",
                        column: x => x.TargetStudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_SessionReports_LinkedSessionReportId",
                        column: x => x.LinkedSessionReportId,
                        principalSchema: "public",
                        principalTable: "SessionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Sessions_TargetSessionId",
                        column: x => x.TargetSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_StudentMonthlyReports_LinkedMonthlyReportId",
                        column: x => x.LinkedMonthlyReportId,
                        principalSchema: "public",
                        principalTable: "StudentMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Users_AssignedTeacherUserId",
                        column: x => x.AssignedTeacherUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_AssignedTeacherUserId",
                schema: "public",
                table: "ReportRequests",
                column: "AssignedTeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_LinkedMonthlyReportId",
                schema: "public",
                table: "ReportRequests",
                column: "LinkedMonthlyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_LinkedSessionReportId",
                schema: "public",
                table: "ReportRequests",
                column: "LinkedSessionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_RequestedByUserId",
                schema: "public",
                table: "ReportRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_TargetClassId",
                schema: "public",
                table: "ReportRequests",
                column: "TargetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_TargetSessionId",
                schema: "public",
                table: "ReportRequests",
                column: "TargetSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_TargetStudentProfileId",
                schema: "public",
                table: "ReportRequests",
                column: "TargetStudentProfileId");

            migrationBuilder.CreateIndex(
                name: "report_request_teacher_queue_idx",
                schema: "public",
                table: "ReportRequests",
                columns: new[] { "AssignedTeacherUserId", "Status", "Priority", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "report_request_type_class_month_idx",
                schema: "public",
                table: "ReportRequests",
                columns: new[] { "ReportType", "TargetClassId", "Month", "Year" });

            migrationBuilder.CreateIndex(
                name: "report_request_type_student_month_idx",
                schema: "public",
                table: "ReportRequests",
                columns: new[] { "ReportType", "TargetStudentProfileId", "Month", "Year" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportRequests",
                schema: "public");
        }
    }
}
