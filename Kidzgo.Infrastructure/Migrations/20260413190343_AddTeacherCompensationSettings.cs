using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherCompensationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeacherCompensationType",
                schema: "public",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeacherCompensationSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StandardSessionDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ForeignTeacherDefaultSessionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    VietnameseTeacherDefaultSessionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AssistantDefaultSessionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCompensationSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "TeacherCompensationSettings",
                columns: new[]
                {
                    "Id",
                    "StandardSessionDurationMinutes",
                    "ForeignTeacherDefaultSessionRate",
                    "VietnameseTeacherDefaultSessionRate",
                    "AssistantDefaultSessionRate",
                    "CreatedAt",
                    "UpdatedAt"
                },
                values: new object[]
                {
                    1,
                    90,
                    0m,
                    0m,
                    0m,
                    new DateTime(2026, 4, 13, 19, 3, 43, DateTimeKind.Utc),
                    null
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherCompensationSettings",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "TeacherCompensationType",
                schema: "public",
                table: "Users");
        }
    }
}
