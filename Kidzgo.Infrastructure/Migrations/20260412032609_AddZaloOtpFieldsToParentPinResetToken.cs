using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddZaloOtpFieldsToParentPinResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OtpAttemptCount",
                schema: "public",
                table: "ParentPinResetTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OtpCodeHash",
                schema: "public",
                table: "ParentPinResetTokens",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAt",
                schema: "public",
                table: "ParentPinResetTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpVerifiedAt",
                schema: "public",
                table: "ParentPinResetTokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpAttemptCount",
                schema: "public",
                table: "ParentPinResetTokens");

            migrationBuilder.DropColumn(
                name: "OtpCodeHash",
                schema: "public",
                table: "ParentPinResetTokens");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAt",
                schema: "public",
                table: "ParentPinResetTokens");

            migrationBuilder.DropColumn(
                name: "OtpVerifiedAt",
                schema: "public",
                table: "ParentPinResetTokens");
        }
    }
}
