using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class programPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultMakeupClassId",
                schema: "public",
                table: "Programs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProgramLeavePolicies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxLeavesPerMonth = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramLeavePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramLeavePolicies_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramLeavePolicies_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                column: "Body",
                value: "<p>Xin chào {{user_name}},</p>\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_DefaultMakeupClassId",
                schema: "public",
                table: "Programs",
                column: "DefaultMakeupClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramLeavePolicies_ProgramId",
                schema: "public",
                table: "ProgramLeavePolicies",
                column: "ProgramId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramLeavePolicies_UpdatedBy",
                schema: "public",
                table: "ProgramLeavePolicies",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Classes_DefaultMakeupClassId",
                schema: "public",
                table: "Programs",
                column: "DefaultMakeupClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Classes_DefaultMakeupClassId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropTable(
                name: "ProgramLeavePolicies",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Programs_DefaultMakeupClassId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "DefaultMakeupClassId",
                schema: "public",
                table: "Programs");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                column: "Body",
                value: "<p>Xin chào {{user_name}},</p>\r\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\r\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\r\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\r\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");
        }
    }
}
