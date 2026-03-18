using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkQuizTimingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                schema: "public",
                table: "HomeworkStudents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowResubmit",
                schema: "public",
                table: "HomeworkAssignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                schema: "public",
                table: "HomeworkAssignments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartedAt",
                schema: "public",
                table: "HomeworkStudents");

            migrationBuilder.DropColumn(
                name: "AllowResubmit",
                schema: "public",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                schema: "public",
                table: "HomeworkAssignments");
        }
    }
}
