using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fieldHomeWork : Migration
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
                name: "AttachmentUrl",
                schema: "public",
                table: "HomeworkAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_StudentProfileId",
                schema: "public",
                table: "SessionReports",
                column: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionReports_StudentProfileId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                schema: "public",
                table: "HomeworkAssignments");

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
