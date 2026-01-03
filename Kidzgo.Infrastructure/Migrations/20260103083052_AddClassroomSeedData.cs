using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassroomSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "Classrooms",
                columns: new[] { "Id", "BranchId", "Capacity", "IsActive", "Name", "Note" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), 15, true, "Phòng A101", "Có máy chiếu, điều hòa" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), 20, true, "Phòng A102", "Phòng lớn, có bảng tương tác" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), 12, true, "Phòng B201", "Phòng nhỏ, phù hợp lớp ít học sinh" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), new Guid("22222222-2222-2222-2222-222222222222"), 18, true, "Phòng C101", "Có máy chiếu, điều hòa" },
                    { new Guid("30000000-0000-0000-0000-000000000005"), new Guid("22222222-2222-2222-2222-222222222222"), 25, true, "Phòng C102", "Phòng lớn nhất, có bảng tương tác và hệ thống âm thanh" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"));
        }
    }
}
