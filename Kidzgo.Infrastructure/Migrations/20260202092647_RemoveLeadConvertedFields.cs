using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLeadConvertedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Profiles_ConvertedStudentProfileId",
                schema: "public",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedStudentProfileId",
                schema: "public",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedAt",
                schema: "public",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedStudentProfileId",
                schema: "public",
                table: "Leads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConvertedAt",
                schema: "public",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConvertedStudentProfileId",
                schema: "public",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedStudentProfileId",
                schema: "public",
                table: "Leads",
                column: "ConvertedStudentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Profiles_ConvertedStudentProfileId",
                schema: "public",
                table: "Leads",
                column: "ConvertedStudentProfileId",
                principalSchema: "public",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
