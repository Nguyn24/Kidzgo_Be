using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsToUserIdSuffix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"SessionReports\" RENAME COLUMN \"SubmittedBy\" TO \"SubmittedByUserId\";");
            migrationBuilder.Sql("ALTER TABLE \"SessionReports\" RENAME COLUMN \"ReviewedBy\" TO \"ReviewedByUserId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"SessionReports\" RENAME COLUMN \"SubmittedByUserId\" TO \"SubmittedBy\";");
            migrationBuilder.Sql("ALTER TABLE \"SessionReports\" RENAME COLUMN \"ReviewedByUserId\" TO \"ReviewedBy\";");
        }
    }
}
