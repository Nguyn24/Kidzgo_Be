using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixConflictMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiVersion",
                schema: "public",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DraftContent",
                schema: "public",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalContent",
                schema: "public",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                schema: "public",
                table: "SessionReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                schema: "public",
                table: "SessionReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                schema: "public",
                table: "SessionReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "public",
                table: "SessionReports",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                schema: "public",
                table: "SessionReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionReportId",
                schema: "public",
                table: "ReportComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_ReviewedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_SubmittedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_SessionReportId",
                schema: "public",
                table: "ReportComments",
                column: "SessionReportId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReportComment_AtLeastOneReportId",
                schema: "public",
                table: "ReportComments",
                sql: "(\"ReportId\" IS NOT NULL OR \"SessionReportId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportComments_SessionReports_SessionReportId",
                schema: "public",
                table: "ReportComments",
                column: "SessionReportId",
                principalSchema: "public",
                principalTable: "SessionReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionReports_Users_ReviewedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "ReviewedByUserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionReports_Users_SubmittedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "SubmittedByUserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportComments_SessionReports_SessionReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionReports_Users_ReviewedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionReports_Users_SubmittedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropIndex(
                name: "IX_SessionReports_ReviewedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropIndex(
                name: "IX_SessionReports_SubmittedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropIndex(
                name: "IX_ReportComments_SessionReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReportComment_AtLeastOneReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.DropColumn(
                name: "AiVersion",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "DraftContent",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "FinalContent",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                schema: "public",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "SessionReportId",
                schema: "public",
                table: "ReportComments");
        }
    }
}
