using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Name",
                value: "Admin User");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "Name",
                value: "Staff User");

            migrationBuilder.InsertData(
                schema: "public",
                table: "Users",
                columns: new[] { "Id", "BranchId", "CreatedAt", "Email", "IsActive", "IsDeleted", "Name", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher1@kidzgo.vn", true, false, "Cô Lan", "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Teacher", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher1" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher2@kidzgo.vn", true, false, "Cô Hoa", "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Teacher", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher2" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher3@kidzgo.vn", true, false, "Thầy Nam", "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Teacher", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "Users");
        }
    }
}
