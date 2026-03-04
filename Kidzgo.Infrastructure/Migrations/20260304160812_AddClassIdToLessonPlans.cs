using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassIdToLessonPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                schema: "public",
                table: "LessonPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileUrl",
                schema: "public",
                table: "Blogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentImageUrl",
                schema: "public",
                table: "Blogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "public"."LessonPlans" lp
                SET "ClassId" = s."ClassId"
                FROM "public"."Sessions" s
                WHERE lp."SessionId" = s."Id";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassId",
                schema: "public",
                table: "LessonPlans",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_ClassId",
                schema: "public",
                table: "LessonPlans",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlans_Classes_ClassId",
                schema: "public",
                table: "LessonPlans",
                column: "ClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlans_Classes_ClassId",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlans_ClassId",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "ClassId",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "AttachmentFileUrl",
                schema: "public",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "AttachmentImageUrl",
                schema: "public",
                table: "Blogs");
        }
    }
}
