using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PdfForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnrollmentConfirmationPdfs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Track = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FormType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PdfUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentConfirmationPdfs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnrollmentConfirmationPdfs_ClassEnrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrollmentConfirmationPdfs_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_EnrollmentId",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_EnrollmentId_Track_FormType_IsAc~",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                columns: new[] { "EnrollmentId", "Track", "FormType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_GeneratedAt",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_RegistrationId",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                column: "RegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrollmentConfirmationPdfs",
                schema: "public");
        }
    }
}
