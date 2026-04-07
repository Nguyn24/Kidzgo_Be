using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkSubmissionAttemptsAndMaxAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportRequests_AssignedTeacherUserId",
                schema: "public",
                table: "ReportRequests");

            migrationBuilder.DropColumn(
                name: "AllowResubmit",
                schema: "public",
                table: "HomeworkAssignments");

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                schema: "public",
                table: "HomeworkAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "HomeworkSubmissionAttempts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true),
                    AiFeedback = table.Column<string>(type: "jsonb", nullable: true),
                    TextAnswer = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkSubmissionAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkSubmissionAttempts_HomeworkStudents_HomeworkStudent~",
                        column: x => x.HomeworkStudentId,
                        principalSchema: "public",
                        principalTable: "HomeworkStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "homework_submission_attempt_unique",
                schema: "public",
                table: "HomeworkSubmissionAttempts",
                columns: new[] { "HomeworkStudentId", "AttemptNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeworkSubmissionAttempts",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                schema: "public",
                table: "HomeworkAssignments");

            migrationBuilder.AddColumn<bool>(
                name: "AllowResubmit",
                schema: "public",
                table: "HomeworkAssignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_AssignedTeacherUserId",
                schema: "public",
                table: "ReportRequests",
                column: "AssignedTeacherUserId");
        }
    }
}
