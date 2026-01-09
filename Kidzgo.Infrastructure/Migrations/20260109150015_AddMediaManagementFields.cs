using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                schema: "public",
                table: "MediaAssets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "public",
                table: "MediaAssets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById",
                schema: "public",
                table: "MediaAssets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                schema: "public",
                table: "MediaAssets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "MediaAssets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                schema: "public",
                table: "MediaAssets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "MediaAssets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_ApprovedById",
                schema: "public",
                table: "MediaAssets",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssets_Users_ApprovedById",
                schema: "public",
                table: "MediaAssets",
                column: "ApprovedById",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_Users_ApprovedById",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropIndex(
                name: "IX_MediaAssets_ApprovedById",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "ContentType",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "MediaAssets");
        }
    }
}
