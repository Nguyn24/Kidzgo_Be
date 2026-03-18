using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRegistrationEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Branches_BranchId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Classes_ClassId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Programs_ProgramId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Registrations_OriginalRegistrationId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_TuitionPlans_TuitionPlanId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "public",
                table: "Registrations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "OperationType",
                schema: "public",
                table: "Registrations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntryType",
                schema: "public",
                table: "Registrations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Branches_BranchId",
                schema: "public",
                table: "Registrations",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Classes_ClassId",
                schema: "public",
                table: "Registrations",
                column: "ClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Programs_ProgramId",
                schema: "public",
                table: "Registrations",
                column: "ProgramId",
                principalSchema: "public",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Registrations_OriginalRegistrationId",
                schema: "public",
                table: "Registrations",
                column: "OriginalRegistrationId",
                principalSchema: "public",
                principalTable: "Registrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_TuitionPlans_TuitionPlanId",
                schema: "public",
                table: "Registrations",
                column: "TuitionPlanId",
                principalSchema: "public",
                principalTable: "TuitionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Branches_BranchId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Classes_ClassId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Programs_ProgramId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Registrations_OriginalRegistrationId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_TuitionPlans_TuitionPlanId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "public",
                table: "Registrations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "OperationType",
                schema: "public",
                table: "Registrations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntryType",
                schema: "public",
                table: "Registrations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Branches_BranchId",
                schema: "public",
                table: "Registrations",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Classes_ClassId",
                schema: "public",
                table: "Registrations",
                column: "ClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Programs_ProgramId",
                schema: "public",
                table: "Registrations",
                column: "ProgramId",
                principalSchema: "public",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Registrations_OriginalRegistrationId",
                schema: "public",
                table: "Registrations",
                column: "OriginalRegistrationId",
                principalSchema: "public",
                principalTable: "Registrations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_TuitionPlans_TuitionPlanId",
                schema: "public",
                table: "Registrations",
                column: "TuitionPlanId",
                principalSchema: "public",
                principalTable: "TuitionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
