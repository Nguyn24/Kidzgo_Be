using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadChildAndPlacementTestLeadChildId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LeadChildId",
                schema: "public",
                table: "PlacementTests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeadChildren",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Dob = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProgramInterest = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConvertedStudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadChildren", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadChildren_Leads_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "public",
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadChildren_Profiles_ConvertedStudentProfileId",
                        column: x => x.ConvertedStudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_LeadChildId",
                schema: "public",
                table: "PlacementTests",
                column: "LeadChildId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadChildren_ConvertedStudentProfileId",
                schema: "public",
                table: "LeadChildren",
                column: "ConvertedStudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadChildren_LeadId",
                schema: "public",
                table: "LeadChildren",
                column: "LeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTests_LeadChildren_LeadChildId",
                schema: "public",
                table: "PlacementTests",
                column: "LeadChildId",
                principalSchema: "public",
                principalTable: "LeadChildren",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTests_LeadChildren_LeadChildId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropTable(
                name: "LeadChildren",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTests_LeadChildId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "LeadChildId",
                schema: "public",
                table: "PlacementTests");
        }
    }
}
