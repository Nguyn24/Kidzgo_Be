using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeClassEnrollmentFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                schema: "public",
                table: "LeaveRequests",
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
                name: "IX_LeaveRequests_SessionId",
                schema: "public",
                table: "LeaveRequests",
                column: "SessionId");

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
                name: "FK_LeaveRequests_Sessions_SessionId",
                schema: "public",
                table: "LeaveRequests",
                column: "SessionId",
                principalSchema: "public",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Sessions_SessionId",
                schema: "public",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "StudentSessionAssignments",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_SessionId",
                schema: "public",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "public",
                table: "LeaveRequests");

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
