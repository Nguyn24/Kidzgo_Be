using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class destroyExcersiteEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseSubmissionAnswers",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ExerciseType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                        name: "FK_Exercises_Missions_MissionId",
                        column: x => x.MissionId,
                        principalSchema: "public",
                        principalTable: "Missions",
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
                name: "ExerciseQuestions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
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
                    GradedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answers = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "IX_Exercises_MissionId",
                schema: "public",
                table: "Exercises",
                column: "MissionId");

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
        }
    }
}
