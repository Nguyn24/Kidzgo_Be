using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPauseEnrollmentRequestHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PauseEnrollmentRequestHistories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PauseEnrollmentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PauseFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PauseTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PauseEnrollmentRequestHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_ClassEnrollments_Enrollment~",
                        column: x => x.EnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_PauseEnrollmentRequests_Pau~",
                        column: x => x.PauseEnrollmentRequestId,
                        principalSchema: "public",
                        principalTable: "PauseEnrollmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_CancelledBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "CancelledBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_ChangedBy",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_ClassId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_EnrollmentId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_PauseEnrollmentRequestId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "PauseEnrollmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_StudentProfileId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "StudentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PauseEnrollmentRequests_Users_CancelledBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "CancelledBy",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PauseEnrollmentRequests_Users_CancelledBy",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropTable(
                name: "PauseEnrollmentRequestHistories",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_PauseEnrollmentRequests_CancelledBy",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                schema: "public",
                table: "PauseEnrollmentRequests");
        }
    }
}
