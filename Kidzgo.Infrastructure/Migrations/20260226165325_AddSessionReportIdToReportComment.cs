using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionReportIdToReportComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportComments_SessionReports_SessionReportId",
                schema: "public",
                table: "ReportComments");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportComments_SessionReports_SessionReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReportComment_AtLeastOneReportId",
                schema: "public",
                table: "ReportComments");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportComments_SessionReports_SessionReportId",
                schema: "public",
                table: "ReportComments",
                column: "SessionReportId",
                principalSchema: "public",
                principalTable: "SessionReports",
                principalColumn: "Id");
        }
    }
}
