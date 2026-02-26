using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fieldHomeworkStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attachments",
                schema: "public",
                table: "HomeworkStudents");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                schema: "public",
                table: "HomeworkStudents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextAnswer",
                schema: "public",
                table: "HomeworkStudents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HomeworkQuestions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<int>(type: "integer", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkQuestions_HomeworkAssignments_HomeworkAssignmentId",
                        column: x => x.HomeworkAssignmentId,
                        principalSchema: "public",
                        principalTable: "HomeworkAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkQuestions_HomeworkAssignmentId",
                schema: "public",
                table: "HomeworkQuestions",
                column: "HomeworkAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeworkQuestions",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                schema: "public",
                table: "HomeworkStudents");

            migrationBuilder.DropColumn(
                name: "TextAnswer",
                schema: "public",
                table: "HomeworkStudents");

            migrationBuilder.AddColumn<string>(
                name: "Attachments",
                schema: "public",
                table: "HomeworkStudents",
                type: "jsonb",
                nullable: true);
        }
    }
}
