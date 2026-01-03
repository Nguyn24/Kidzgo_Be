using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedDataForBranchesUsersPrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "Branches",
                columns: new[] { "Id", "Address", "Code", "ContactEmail", "ContactPhone", "CreatedAt", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "123 Đường ABC, Quận XYZ, Hà Nội", "HN001", "hanoi@kidzgo.vn", "02412345678", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Chi nhánh Hà Nội", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "456 Đường DEF, Quận UVW, TP.HCM", "HCM001", "hcm@kidzgo.vn", "02898765432", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Chi nhánh TP. Hồ Chí Minh", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "Users",
                columns: new[] { "Id", "BranchId", "CreatedAt", "Email", "IsActive", "IsDeleted", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@kidzgo.vn", true, false, "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Admin", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "Programs",
                columns: new[] { "Id", "BranchId", "DefaultTuitionAmount", "Description", "IsActive", "IsDeleted", "Level", "Name", "TotalSessions", "UnitPriceSession" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), 5000000m, "Khóa học tiếng Anh cho trẻ em mới bắt đầu, tập trung vào phát âm và từ vựng cơ bản.", true, false, "Beginner", "English for Kids - Beginner", 30, 166667m },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), 6000000m, "Khóa học tiếng Anh nâng cao cho trẻ em, phát triển kỹ năng giao tiếp và ngữ pháp.", true, false, "Intermediate", "English for Kids - Intermediate", 36, 166667m },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new Guid("22222222-2222-2222-2222-222222222222"), 5000000m, "Khóa học tiếng Anh cho trẻ em mới bắt đầu, tập trung vào phát âm và từ vựng cơ bản.", true, false, "Beginner", "English for Kids - Beginner", 30, 166667m },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new Guid("22222222-2222-2222-2222-222222222222"), 8000000m, "Khóa học tiếng Anh nâng cao cho thanh thiếu niên, chuẩn bị cho các kỳ thi quốc tế.", true, false, "Advanced", "English for Teens - Advanced", 40, 200000m },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new Guid("11111111-1111-1111-1111-111111111111"), 8000000m, "Khóa học đã tạm ngưng.", false, false, "Advanced", "English for Kids - Advanced (Inactive)", 40, 200000m }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "Users",
                columns: new[] { "Id", "BranchId", "CreatedAt", "Email", "IsActive", "IsDeleted", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "staff@kidzgo.vn", true, false, "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Staff", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "staff" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Branches",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Branches",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
        }
    }
}
