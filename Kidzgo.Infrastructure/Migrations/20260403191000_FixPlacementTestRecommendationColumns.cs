using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260403191000_FixPlacementTestRecommendationColumns")]
    public partial class FixPlacementTestRecommendationColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public."PlacementTests"
                ADD COLUMN IF NOT EXISTS "ProgramRecommendationId" uuid NULL;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE public."PlacementTests"
                ADD COLUMN IF NOT EXISTS "SecondaryProgramRecommendationId" uuid NULL;
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_PlacementTests_ProgramRecommendationId"
                ON public."PlacementTests" ("ProgramRecommendationId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_PlacementTests_SecondaryProgramRecommendationId"
                ON public."PlacementTests" ("SecondaryProgramRecommendationId");
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_PlacementTests_Programs_ProgramRecommendationId'
                    ) THEN
                        ALTER TABLE public."PlacementTests"
                        ADD CONSTRAINT "FK_PlacementTests_Programs_ProgramRecommendationId"
                        FOREIGN KEY ("ProgramRecommendationId")
                        REFERENCES public."Programs" ("Id")
                        ON DELETE RESTRICT;
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_PlacementTests_Programs_SecondaryProgramRecommendationId'
                    ) THEN
                        ALTER TABLE public."PlacementTests"
                        ADD CONSTRAINT "FK_PlacementTests_Programs_SecondaryProgramRecommendationId"
                        FOREIGN KEY ("SecondaryProgramRecommendationId")
                        REFERENCES public."Programs" ("Id")
                        ON DELETE RESTRICT;
                    END IF;
                END
                $$;
                """);

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
