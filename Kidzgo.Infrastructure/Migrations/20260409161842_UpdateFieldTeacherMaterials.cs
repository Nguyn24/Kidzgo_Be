using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldTeacherMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PdfPreviewFileSize",
                schema: "public",
                table: "TeachingMaterials",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PdfPreviewGeneratedAt",
                schema: "public",
                table: "TeachingMaterials",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfPreviewPath",
                schema: "public",
                table: "TeachingMaterials",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeachingMaterialAnnotations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlideNumber = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "#FFD700"),
                    PositionX = table.Column<double>(type: "double precision", nullable: true),
                    PositionY = table.Column<double>(type: "double precision", nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Note"),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Private"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialAnnotations", x => x.Id);
                    table.CheckConstraint("CK_Annotation_Type", "\"Type\" IN ('Note', 'Highlight', 'Pin')");
                    table.CheckConstraint("CK_Annotation_Visibility", "\"Visibility\" IN ('Private', 'Class', 'Public')");
                    table.ForeignKey(
                        name: "FK_TeachingMaterialAnnotations_TeachingMaterials_TeachingMater~",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialAnnotations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterialBookmarks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialBookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialBookmarks_TeachingMaterials_TeachingMateria~",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterialSlides",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlideNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviewImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false, defaultValue: 1920),
                    Height = table.Column<int>(type: "integer", nullable: false, defaultValue: 1080),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialSlides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialSlides_TeachingMaterials_TeachingMaterialId",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterialViewProgresses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastSlideViewed = table.Column<int>(type: "integer", nullable: true),
                    TotalTimeSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ViewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FirstViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialViewProgresses", x => x.Id);
                    table.CheckConstraint("CK_ViewProgress_Percent", "\"ProgressPercent\" >= 0 AND \"ProgressPercent\" <= 100");
                    table.ForeignKey(
                        name: "FK_TeachingMaterialViewProgresses_TeachingMaterials_TeachingMa~",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialViewProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialAnnotations_TeachingMaterialId",
                schema: "public",
                table: "TeachingMaterialAnnotations",
                column: "TeachingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialAnnotations_TeachingMaterialId_SlideNumber",
                schema: "public",
                table: "TeachingMaterialAnnotations",
                columns: new[] { "TeachingMaterialId", "SlideNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialAnnotations_UserId",
                schema: "public",
                table: "TeachingMaterialAnnotations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialBookmarks_TeachingMaterialId_UserId",
                schema: "public",
                table: "TeachingMaterialBookmarks",
                columns: new[] { "TeachingMaterialId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialBookmarks_UserId",
                schema: "public",
                table: "TeachingMaterialBookmarks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialSlides_TeachingMaterialId",
                schema: "public",
                table: "TeachingMaterialSlides",
                column: "TeachingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialSlides_TeachingMaterialId_SlideNumber",
                schema: "public",
                table: "TeachingMaterialSlides",
                columns: new[] { "TeachingMaterialId", "SlideNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_Completed",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                column: "Completed");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_TeachingMaterialId",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                column: "TeachingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_TeachingMaterialId_UserId",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                columns: new[] { "TeachingMaterialId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_UserId",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeachingMaterialAnnotations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterialBookmarks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterialSlides",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterialViewProgresses",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "PdfPreviewFileSize",
                schema: "public",
                table: "TeachingMaterials");

            migrationBuilder.DropColumn(
                name: "PdfPreviewGeneratedAt",
                schema: "public",
                table: "TeachingMaterials");

            migrationBuilder.DropColumn(
                name: "PdfPreviewPath",
                schema: "public",
                table: "TeachingMaterials");
        }
    }
}
