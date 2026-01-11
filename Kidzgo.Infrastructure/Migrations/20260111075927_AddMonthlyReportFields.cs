using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyReportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                schema: "public",
                table: "StudentMonthlyReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "StudentMonthlyReports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                schema: "public",
                table: "StudentMonthlyReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PdfGeneratedAt",
                schema: "public",
                table: "StudentMonthlyReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                schema: "public",
                table: "StudentMonthlyReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "StudentMonthlyReports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "MonthlyReportJobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "public",
                table: "MonthlyReportJobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "public",
                table: "MonthlyReportJobs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "public",
                table: "MonthlyReportJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "MonthlyReportJobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "MonthlyReportData",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    AttendanceData = table.Column<string>(type: "jsonb", nullable: true),
                    HomeworkData = table.Column<string>(type: "jsonb", nullable: true),
                    TestData = table.Column<string>(type: "jsonb", nullable: true),
                    MissionData = table.Column<string>(type: "jsonb", nullable: true),
                    NotesData = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyReportData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyReportData_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonthlyReportData_StudentMonthlyReports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "public",
                        principalTable: "StudentMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_ClassId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_JobId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportJobs_CreatedBy",
                schema: "public",
                table: "MonthlyReportJobs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportData_ReportId",
                schema: "public",
                table: "MonthlyReportData",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportData_StudentProfileId",
                schema: "public",
                table: "MonthlyReportData",
                column: "StudentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyReportJobs_Users_CreatedBy",
                schema: "public",
                table: "MonthlyReportJobs",
                column: "CreatedBy",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMonthlyReports_Classes_ClassId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "ClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMonthlyReports_MonthlyReportJobs_JobId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "JobId",
                principalSchema: "public",
                principalTable: "MonthlyReportJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyReportJobs_Users_CreatedBy",
                schema: "public",
                table: "MonthlyReportJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentMonthlyReports_Classes_ClassId",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentMonthlyReports_MonthlyReportJobs_JobId",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropTable(
                name: "MonthlyReportData",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_StudentMonthlyReports_ClassId",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropIndex(
                name: "IX_StudentMonthlyReports_JobId",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyReportJobs_CreatedBy",
                schema: "public",
                table: "MonthlyReportJobs");

            migrationBuilder.DropColumn(
                name: "ClassId",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropColumn(
                name: "JobId",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropColumn(
                name: "PdfGeneratedAt",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropColumn(
                name: "PdfUrl",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "StudentMonthlyReports");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "MonthlyReportJobs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "MonthlyReportJobs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "public",
                table: "MonthlyReportJobs");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "public",
                table: "MonthlyReportJobs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "MonthlyReportJobs");
        }
    }
}
