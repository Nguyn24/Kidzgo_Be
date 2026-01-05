using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokenAndEmailTemplateSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "EmailTemplates",
                columns: new[] { "Id", "Body", "Code", "CreatedAt", "IsActive", "IsDeleted", "Placeholders", "Subject", "UpdatedAt" },
                values: new object[] { new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), "<p>Xin chào {{user_name}},</p>\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\n<p>Trân trọng,<br/>Kidzgo Team</p>", "FORGOT_PASSWORD", new DateTime(2025, 12, 30, 9, 23, 14, 264, DateTimeKind.Utc).AddTicks(6162), true, false, "[\"user_name\",\"reset_link\"]", "Kidzgo - Đặt lại mật khẩu của bạn", new DateTime(2025, 12, 30, 9, 23, 14, 264, DateTimeKind.Utc).AddTicks(6162) });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                schema: "public",
                table: "PasswordResetTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTokens",
                schema: "public");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));
        }
    }
}
