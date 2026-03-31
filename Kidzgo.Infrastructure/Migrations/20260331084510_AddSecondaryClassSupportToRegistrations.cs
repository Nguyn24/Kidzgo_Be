using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondaryClassSupportToRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SecondaryClassAssignedDate",
                schema: "public",
                table: "Registrations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryClassId",
                schema: "public",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryEntryType",
                schema: "public",
                table: "Registrations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryProgramId",
                schema: "public",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryProgramSkillFocus",
                schema: "public",
                table: "Registrations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSupplementary",
                schema: "public",
                table: "Programs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecondaryProgramSupplementary",
                schema: "public",
                table: "PlacementTests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryProgramRecommendation",
                schema: "public",
                table: "PlacementTests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryProgramSkillFocus",
                schema: "public",
                table: "PlacementTests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RegistrationId",
                schema: "public",
                table: "ClassEnrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_SecondaryClassId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_SecondaryProgramId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_RegistrationId",
                schema: "public",
                table: "ClassEnrollments",
                column: "RegistrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassEnrollments_Registrations_RegistrationId",
                schema: "public",
                table: "ClassEnrollments",
                column: "RegistrationId",
                principalSchema: "public",
                principalTable: "Registrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Classes_SecondaryClassId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Programs_SecondaryProgramId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryProgramId",
                principalSchema: "public",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassEnrollments_Registrations_RegistrationId",
                schema: "public",
                table: "ClassEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Classes_SecondaryClassId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Programs_SecondaryProgramId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_SecondaryClassId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_SecondaryProgramId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_ClassEnrollments_RegistrationId",
                schema: "public",
                table: "ClassEnrollments");

            migrationBuilder.DropColumn(
                name: "SecondaryClassAssignedDate",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "SecondaryClassId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "SecondaryEntryType",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "SecondaryProgramId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "SecondaryProgramSkillFocus",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "IsSupplementary",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "IsSecondaryProgramSupplementary",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "SecondaryProgramRecommendation",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "SecondaryProgramSkillFocus",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "RegistrationId",
                schema: "public",
                table: "ClassEnrollments");
        }
    }
}
