using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStorageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AvatarFileSize",
                schema: "public",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarMimeType",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AvatarFileSize",
                schema: "public",
                table: "Profiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarMimeType",
                schema: "public",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                schema: "public",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "public",
                table: "MediaAssets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                schema: "public",
                table: "MediaAssets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                schema: "public",
                table: "MediaAssets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                schema: "public",
                table: "MediaAssets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipScope",
                schema: "public",
                table: "MediaAssets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentUrl",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<long>(
                name: "AttachmentFileSize",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentMimeType",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentOriginalFileName",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CoverImageFileSize",
                schema: "public",
                table: "LessonPlans",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageMimeType",
                schema: "public",
                table: "LessonPlans",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                schema: "public",
                table: "LessonPlans",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MediaFileSize",
                schema: "public",
                table: "LessonPlans",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaMimeType",
                schema: "public",
                table: "LessonPlans",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                schema: "public",
                table: "LessonPlans",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                schema: "public",
                table: "LessonPlans",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarFileSize",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarMimeType",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarFileSize",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AvatarMimeType",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "FileSize",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "MimeType",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "OwnershipScope",
                schema: "public",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "AttachmentFileSize",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "AttachmentMimeType",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "AttachmentOriginalFileName",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "CoverImageFileSize",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "CoverImageMimeType",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "MediaFileSize",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "MediaMimeType",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "MediaType",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentUrl",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
