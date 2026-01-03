using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNameCreatedAtUpdatedAtToTuitionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Name column (nullable first to handle existing data)
            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "TuitionPlans",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            // Update existing rows with a default name based on ID
            migrationBuilder.Sql(@"
                UPDATE ""TuitionPlans""
                SET ""Name"" = 'Gói học phí ' || ""Id""::text
                WHERE ""Name"" IS NULL;
            ");

            // Make Name required (non-nullable)
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "TuitionPlans",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false);

            // Add CreatedAt and UpdatedAt with default value using SQL NOW()
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "TuitionPlans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "TuitionPlans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "TuitionPlans");
        }
    }
}
