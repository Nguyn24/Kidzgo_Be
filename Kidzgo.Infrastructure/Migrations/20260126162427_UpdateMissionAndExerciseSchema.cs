using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMissionAndExerciseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressPerQuestion",
                schema: "public",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "TotalQuestions",
                schema: "public",
                table: "Missions");

            migrationBuilder.AddColumn<Guid>(
                name: "MissionId",
                schema: "public",
                table: "Exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_MissionId",
                schema: "public",
                table: "Exercises",
                column: "MissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Missions_MissionId",
                schema: "public",
                table: "Exercises",
                column: "MissionId",
                principalSchema: "public",
                principalTable: "Missions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Missions_MissionId",
                schema: "public",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_MissionId",
                schema: "public",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "MissionId",
                schema: "public",
                table: "Exercises");

            migrationBuilder.AddColumn<decimal>(
                name: "ProgressPerQuestion",
                schema: "public",
                table: "Missions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuestions",
                schema: "public",
                table: "Missions",
                type: "integer",
                nullable: true);
        }
    }
}
