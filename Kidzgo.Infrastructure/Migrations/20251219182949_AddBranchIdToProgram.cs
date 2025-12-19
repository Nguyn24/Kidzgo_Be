using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdToProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "public",
                table: "Programs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "PinHash",
                schema: "public",
                table: "Profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programs_BranchId",
                schema: "public",
                table: "Programs",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Branches_BranchId",
                schema: "public",
                table: "Programs",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Branches_BranchId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Programs_BranchId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "public",
                table: "Programs");

            migrationBuilder.AlterColumn<string>(
                name: "PinHash",
                schema: "public",
                table: "Profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
