using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyMissionTargetAndAddTotalRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TargetStudentId",
                schema: "public",
                table: "Missions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetStudentId",
                schema: "public",
                table: "Missions");
        }
    }
}
