using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FieldLessonTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StructureJson",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.AddColumn<string>(
                name: "StructureJson",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "jsonb",
                nullable: true);
        }
    }
}
