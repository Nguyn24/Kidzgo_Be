using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceBirthYearWithChildDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthYear",
                schema: "public",
                table: "Leads");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChildDateOfBirth",
                schema: "public",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildDateOfBirth",
                schema: "public",
                table: "Leads");

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                schema: "public",
                table: "Leads",
                type: "integer",
                nullable: true);
        }
    }
}
