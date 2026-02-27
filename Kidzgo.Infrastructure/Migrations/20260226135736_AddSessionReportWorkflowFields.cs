using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionReportWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "session_report_student_date_idx",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.RenameIndex(
                name: "mission_progress_student_profile_idx",
                schema: "public",
                table: "MissionProgresses",
                newName: "IX_MissionProgresses_StudentProfileId");

            migrationBuilder.RenameIndex(
                name: "homework_student_student_profile_idx",
                schema: "public",
                table: "HomeworkStudents",
                newName: "IX_HomeworkStudents_StudentProfileId");

            migrationBuilder.RenameIndex(
                name: "exam_result_student_profile_idx",
                schema: "public",
                table: "ExamResults",
                newName: "IX_ExamResults_StudentProfileId");

            migrationBuilder.RenameIndex(
                name: "attendance_student_profile_idx",
                schema: "public",
                table: "Attendances",
                newName: "IX_Attendances_StudentProfileId");

            migrationBuilder.AddColumn<string>(
                name: "AiVersion",
                schema: "public",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DraftContent",
                schema: "public",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalContent",
                schema: "public",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                schema: "public",
                table: "SessionReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                schema: "public",
                table: "SessionReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                schema: "public",
                table: "SessionReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                schema: "public",
                table: "SessionReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "SessionReports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedBy",
                schema: "public",
                table: "SessionReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                schema: "public",
                table: "SessionReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionReportId",
                schema: "public",
                table: "ReportComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_ReviewedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_StudentProfileId",
                schema: "public",
                table: "SessionReports",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_SubmittedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_SessionReportId",
                schema: "public",
                table: "ReportComments",
                column: "SessionReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportComments_SessionReports_SessionReportId",
                schema: "public",
                table: "ReportComments",
                column: "SessionReportId",
                principalSchema: "public",
                principalTable: "SessionReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionReports_Users_ReviewedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "ReviewedByUserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionReports_Users_SubmittedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "SubmittedByUserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportComments_SessionReports_SessionReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionReports_Users_ReviewedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionReports_Users_SubmittedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropIndex(
                name: "IX_SessionReports_ReviewedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropIndex(
                name: "IX_SessionReports_StudentProfileId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropIndex(
                name: "IX_SessionReports_SubmittedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropIndex(
                name: "IX_ReportComments_SessionReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.DropColumn(
                name: "AiVersion",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "DraftContent",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "FinalContent",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "SessionReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.RenameIndex(
                name: "IX_MissionProgresses_StudentProfileId",
                schema: "public",
                table: "MissionProgresses",
                newName: "mission_progress_student_profile_idx");

            migrationBuilder.RenameIndex(
                name: "IX_HomeworkStudents_StudentProfileId",
                schema: "public",
                table: "HomeworkStudents",
                newName: "homework_student_student_profile_idx");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResults_StudentProfileId",
                schema: "public",
                table: "ExamResults",
                newName: "exam_result_student_profile_idx");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_StudentProfileId",
                schema: "public",
                table: "Attendances",
                newName: "attendance_student_profile_idx");

            migrationBuilder.CreateIndex(
                name: "session_report_student_date_idx",
                schema: "public",
                table: "SessionReports",
                columns: new[] { "StudentProfileId", "ReportDate" });
        }
    }
}
