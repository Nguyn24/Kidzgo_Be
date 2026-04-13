using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableForFlowGamification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MissionRewardRules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProgressMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Count"),
                    TotalRequired = table.Column<int>(type: "integer", nullable: false),
                    RewardStars = table.Column<int>(type: "integer", nullable: false),
                    RewardExp = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionRewardRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionRewardRules_MissionType_ProgressMode_TotalRequired",
                schema: "public",
                table: "MissionRewardRules",
                columns: new[] { "MissionType", "ProgressMode", "TotalRequired" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissionRewardRules",
                schema: "public");
        }
    }
}
