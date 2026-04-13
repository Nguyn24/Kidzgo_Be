using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixGamificationAndMission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProgressMode",
                schema: "public",
                table: "Missions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Count");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrollmentConfirmationPdfGeneratedAt",
                schema: "public",
                table: "ClassEnrollments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EnrollmentConfirmationPdfGeneratedBy",
                schema: "public",
                table: "ClassEnrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnrollmentConfirmationPdfUrl",
                schema: "public",
                table: "ClassEnrollments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressMode",
                schema: "public",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "EnrollmentConfirmationPdfGeneratedAt",
                schema: "public",
                table: "ClassEnrollments");

            migrationBuilder.DropColumn(
                name: "EnrollmentConfirmationPdfGeneratedBy",
                schema: "public",
                table: "ClassEnrollments");

            migrationBuilder.DropColumn(
                name: "EnrollmentConfirmationPdfUrl",
                schema: "public",
                table: "ClassEnrollments");
        }
    }
}
