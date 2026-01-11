using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamQuestionsAndSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowLateStart",
                schema: "public",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoSubmitOnTimeLimit",
                schema: "public",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LateStartToleranceMinutes",
                schema: "public",
                table: "Exams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreventCopyPaste",
                schema: "public",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PreventNavigation",
                schema: "public",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledStartTime",
                schema: "public",
                table: "Exams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowResultsImmediately",
                schema: "public",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                schema: "public",
                table: "Exams",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "public",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubmissions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutoSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeSpentMinutes = table.Column<int>(type: "integer", nullable: true),
                    AutoScore = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    GradedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeacherComment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "public",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Users_GradedBy",
                        column: x => x.GradedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubmissionAnswers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubmissionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubmissionAnswers_ExamQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "public",
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubmissionAnswers_ExamSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "public",
                        principalTable: "ExamSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_ExamId",
                schema: "public",
                table: "ExamQuestions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissionAnswers_QuestionId",
                schema: "public",
                table: "ExamSubmissionAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissionAnswers_SubmissionId",
                schema: "public",
                table: "ExamSubmissionAnswers",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_ExamId",
                schema: "public",
                table: "ExamSubmissions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_GradedBy",
                schema: "public",
                table: "ExamSubmissions",
                column: "GradedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_StudentProfileId",
                schema: "public",
                table: "ExamSubmissions",
                column: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSubmissionAnswers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExamQuestions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExamSubmissions",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "AllowLateStart",
                schema: "public",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "AutoSubmitOnTimeLimit",
                schema: "public",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "LateStartToleranceMinutes",
                schema: "public",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "PreventCopyPaste",
                schema: "public",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "PreventNavigation",
                schema: "public",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ScheduledStartTime",
                schema: "public",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ShowResultsImmediately",
                schema: "public",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                schema: "public",
                table: "Exams");
        }
    }
}
