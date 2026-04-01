using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class field : Migration
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
                name: "SessionId",
                schema: "public",
                table: "LeaveRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RegistrationId",
                schema: "public",
                table: "ClassEnrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionSelectionPattern",
                schema: "public",
                table: "ClassEnrollments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Track",
                schema: "public",
                table: "ClassEnrollments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "StudentSessionAssignments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Track = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSessionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_ClassEnrollments_ClassEnrollmentId",
                        column: x => x.ClassEnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_LeaveRequests_SessionId",
                schema: "public",
                table: "LeaveRequests",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_RegistrationId",
                schema: "public",
                table: "ClassEnrollments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_ClassEnrollmentId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "ClassEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_RegistrationId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_SessionId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_SessionId_ClassEnrollmentId",
                schema: "public",
                table: "StudentSessionAssignments",
                columns: new[] { "SessionId", "ClassEnrollmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_SessionId_Status",
                schema: "public",
                table: "StudentSessionAssignments",
                columns: new[] { "SessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_StudentProfileId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_StudentProfileId_Status",
                schema: "public",
                table: "StudentSessionAssignments",
                columns: new[] { "StudentProfileId", "Status" });

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
                name: "FK_LeaveRequests_Sessions_SessionId",
                schema: "public",
                table: "LeaveRequests",
                column: "SessionId",
                principalSchema: "public",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_LeaveRequests_Sessions_SessionId",
                schema: "public",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Classes_SecondaryClassId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Programs_SecondaryProgramId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropTable(
                name: "StudentSessionAssignments",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_SecondaryClassId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_SecondaryProgramId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_SessionId",
                schema: "public",
                table: "LeaveRequests");

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
                name: "SessionId",
                schema: "public",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "RegistrationId",
                schema: "public",
                table: "ClassEnrollments");

            migrationBuilder.DropColumn(
                name: "SessionSelectionPattern",
                schema: "public",
                table: "ClassEnrollments");

            migrationBuilder.DropColumn(
                name: "Track",
                schema: "public",
                table: "ClassEnrollments");
        }
    }
}
