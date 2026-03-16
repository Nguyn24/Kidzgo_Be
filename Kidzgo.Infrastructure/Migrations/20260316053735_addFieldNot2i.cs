using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addFieldNot2i : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "public",
                table: "DeviceTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Browser",
                schema: "public",
                table: "DeviceTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                schema: "public",
                table: "DeviceTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "public",
                table: "DeviceTokens",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "public",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "Browser",
                schema: "public",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "Locale",
                schema: "public",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "public",
                table: "DeviceTokens");
        }
    }
}
