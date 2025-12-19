using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsMainFromClassEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetingLink",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                schema: "public",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "IsMain",
                schema: "public",
                table: "ClassEnrollments");

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                schema: "public",
                table: "RewardRedemptions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ProgressPerQuestion",
                schema: "public",
                table: "Missions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RewardExp",
                schema: "public",
                table: "Missions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuestions",
                schema: "public",
                table: "Missions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrls",
                schema: "public",
                table: "ExamResults",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumMonthlyHours",
                schema: "public",
                table: "Contracts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeRateMultiplier",
                schema: "public",
                table: "Contracts",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceStreaks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    RewardStars = table.Column<int>(type: "integer", nullable: false),
                    RewardExp = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceStreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceStreaks_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    FeaturedImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blogs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ExerciseType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exercises_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyWorkHours",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric", nullable: false),
                    TeachingHours = table.Column<decimal>(type: "numeric", nullable: false),
                    RegularHours = table.Column<decimal>(type: "numeric", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "numeric", nullable: false),
                    TeachingSessions = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyWorkHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyWorkHours_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyWorkHours_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "public",
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyWorkHours_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionReports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    AiGeneratedSummary = table.Column<string>(type: "text", nullable: true),
                    IsMonthlyCompiled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionReports_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReports_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReports_Users_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseQuestions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseQuestions_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "public",
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSubmissions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answers = table.Column<string>(type: "jsonb", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GradedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseSubmissions_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "public",
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseSubmissions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseSubmissions_Users_GradedBy",
                        column: x => x.GradedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSubmissionAnswers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "numeric", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSubmissionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseSubmissionAnswers_ExerciseQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "public",
                        principalTable: "ExerciseQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseSubmissionAnswers_ExerciseSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "public",
                        principalTable: "ExerciseSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "attendance_streak_unique",
                schema: "public",
                table: "AttendanceStreaks",
                columns: new[] { "StudentProfileId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "blog_published_idx",
                schema: "public",
                table: "Blogs",
                columns: new[] { "IsPublished", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CreatedBy",
                schema: "public",
                table: "Blogs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseQuestions_ExerciseId",
                schema: "public",
                table: "ExerciseQuestions",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ClassId",
                schema: "public",
                table: "Exercises",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedBy",
                schema: "public",
                table: "Exercises",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "exercise_submission_answer_unique",
                schema: "public",
                table: "ExerciseSubmissionAnswers",
                columns: new[] { "SubmissionId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSubmissionAnswers_QuestionId",
                schema: "public",
                table: "ExerciseSubmissionAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "exercise_submission_unique",
                schema: "public",
                table: "ExerciseSubmissions",
                columns: new[] { "ExerciseId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSubmissions_GradedBy",
                schema: "public",
                table: "ExerciseSubmissions",
                column: "GradedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSubmissions_StudentProfileId",
                schema: "public",
                table: "ExerciseSubmissions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyWorkHours_ContractId",
                schema: "public",
                table: "MonthlyWorkHours",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "monthly_work_hours_payroll_idx",
                schema: "public",
                table: "MonthlyWorkHours",
                columns: new[] { "BranchId", "Year", "Month", "IsLocked" });

            migrationBuilder.CreateIndex(
                name: "monthly_work_hours_unique",
                schema: "public",
                table: "MonthlyWorkHours",
                columns: new[] { "StaffUserId", "ContractId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_StudentProfileId",
                schema: "public",
                table: "SessionReports",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "session_report_teacher_date_idx",
                schema: "public",
                table: "SessionReports",
                columns: new[] { "TeacherUserId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "session_report_unique",
                schema: "public",
                table: "SessionReports",
                columns: new[] { "SessionId", "StudentProfileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceStreaks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Blogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExerciseSubmissionAnswers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MonthlyWorkHours",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SessionReports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExerciseQuestions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExerciseSubmissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Exercises",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "ItemName",
                schema: "public",
                table: "RewardRedemptions");

            migrationBuilder.DropColumn(
                name: "ProgressPerQuestion",
                schema: "public",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "RewardExp",
                schema: "public",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "TotalQuestions",
                schema: "public",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "AttachmentUrls",
                schema: "public",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "MinimumMonthlyHours",
                schema: "public",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "OvertimeRateMultiplier",
                schema: "public",
                table: "Contracts");

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                schema: "public",
                table: "PlacementTests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                schema: "public",
                table: "ExamResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMain",
                schema: "public",
                table: "ClassEnrollments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
