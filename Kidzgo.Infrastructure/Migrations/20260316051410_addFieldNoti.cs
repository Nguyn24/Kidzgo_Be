using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addFieldNoti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                schema: "public",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                schema: "public",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                schema: "public",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderRole",
                schema: "public",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetRole",
                schema: "public",
                table: "Notifications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderName",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderRole",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TargetRole",
                schema: "public",
                table: "Notifications");
        }
    }
}
