using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChildFieldsToLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                schema: "public",
                table: "Leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildName",
                schema: "public",
                table: "Leads",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthYear",
                schema: "public",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ChildName",
                schema: "public",
                table: "Leads");
        }
    }
}
