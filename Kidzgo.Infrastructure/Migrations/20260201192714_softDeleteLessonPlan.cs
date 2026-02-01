using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class softDeleteLessonPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "LessonPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedAnswer",
                schema: "public",
                table: "HomeworkAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                schema: "public",
                table: "HomeworkAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rubric",
                schema: "public",
                table: "HomeworkAssignments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "ExpectedAnswer",
                schema: "public",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "Instructions",
                schema: "public",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "Rubric",
                schema: "public",
                table: "HomeworkAssignments");
        }
    }
}
