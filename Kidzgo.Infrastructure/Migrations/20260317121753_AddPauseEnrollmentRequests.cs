using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPauseEnrollmentRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PauseEnrollmentRequests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    PauseFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PauseTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    OutcomeNote = table.Column<string>(type: "text", nullable: true),
                    OutcomeBy = table.Column<Guid>(type: "uuid", nullable: true),
                    OutcomeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PauseEnrollmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Users_OutcomeBy",
                        column: x => x.OutcomeBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ApprovedBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ClassId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_OutcomeBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "OutcomeBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_StudentProfileId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PauseEnrollmentRequests",
                schema: "public");
        }
    }
}
