using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChildFieldsFromLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildDateOfBirth",
                schema: "public",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ChildName",
                schema: "public",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ProgramInterest",
                schema: "public",
                table: "Leads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChildDateOfBirth",
                schema: "public",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildName",
                schema: "public",
                table: "Leads",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgramInterest",
                schema: "public",
                table: "Leads",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
