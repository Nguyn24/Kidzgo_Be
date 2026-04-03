using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260403194000_DropLegacyPlacementTestRecommendationColumns")]
    public partial class DropLegacyPlacementTestRecommendationColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public."PlacementTests"
                DROP COLUMN IF EXISTS "IsSecondaryProgramSupplementary";
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE public."PlacementTests"
                DROP COLUMN IF EXISTS "ProgramRecommendation";
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE public."PlacementTests"
                DROP COLUMN IF EXISTS "SecondaryProgramRecommendation";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
