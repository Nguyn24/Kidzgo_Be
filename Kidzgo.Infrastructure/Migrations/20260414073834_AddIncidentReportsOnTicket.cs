using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentReportsOnTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IncidentCategory",
                schema: "public",
                table: "Tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentEvidenceUrl",
                schema: "public",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentStatus",
                schema: "public",
                table: "Tickets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsIncidentReport",
                schema: "public",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IncidentCommentType",
                schema: "public",
                table: "TicketComments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncidentCategory",
                schema: "public",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IncidentEvidenceUrl",
                schema: "public",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IncidentStatus",
                schema: "public",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsIncidentReport",
                schema: "public",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IncidentCommentType",
                schema: "public",
                table: "TicketComments");
        }
    }
}
