using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, create some Users for Parent and Student profiles
            migrationBuilder.InsertData(
                schema: "public",
                table: "Users",
                columns: new[] { "Id", "BranchId", "CreatedAt", "Email", "IsActive", "IsDeleted", "Name", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "parent1@example.com", true, false, "Nguyễn Văn A", "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Parent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "parent1" },
                    { new Guid("40000000-0000-0000-0000-000000000002"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "parent2@example.com", true, false, "Trần Thị B", "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Parent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "parent2" },
                    { new Guid("40000000-0000-0000-0000-000000000003"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "parent3@example.com", true, false, "Lê Văn C", "DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF", "Parent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "parent3" }
                });

            // Create Student Profiles
            migrationBuilder.InsertData(
                schema: "public",
                table: "Profiles",
                columns: new[] { "Id", "CreatedAt", "DisplayName", "IsActive", "IsDeleted", "PinHash", "ProfileType", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Văn An", true, false, null, "Student", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Thị Bình", true, false, null, "Student", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000001") },
                    { new Guid("50000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Văn Cường", true, false, null, "Student", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000002") },
                    { new Guid("50000000-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Thị Dung", true, false, null, "Student", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000003") },
                    { new Guid("50000000-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Văn Em", true, false, null, "Student", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000003") }
                });

            // Create Parent Profiles
            migrationBuilder.InsertData(
                schema: "public",
                table: "Profiles",
                columns: new[] { "Id", "CreatedAt", "DisplayName", "IsActive", "IsDeleted", "PinHash", "ProfileType", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Văn A", true, false, null, "Parent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000001") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Thị B", true, false, null, "Parent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000002") },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Văn C", true, false, null, "Parent", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("40000000-0000-0000-0000-000000000003") }
                });

            // Link Parent with Students
            migrationBuilder.InsertData(
                schema: "public",
                table: "ParentStudentLinks",
                columns: new[] { "Id", "CreatedAt", "ParentProfileId", "StudentProfileId" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("60000000-0000-0000-0000-000000000001"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new Guid("70000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("60000000-0000-0000-0000-000000000001"), new Guid("50000000-0000-0000-0000-000000000002") },
                    { new Guid("70000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("60000000-0000-0000-0000-000000000002"), new Guid("50000000-0000-0000-0000-000000000003") },
                    { new Guid("70000000-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("60000000-0000-0000-0000-000000000003"), new Guid("50000000-0000-0000-0000-000000000004") },
                    { new Guid("70000000-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("60000000-0000-0000-0000-000000000003"), new Guid("50000000-0000-0000-0000-000000000005") }
                });

            // Create Tuition Plans
            migrationBuilder.InsertData(
                schema: "public",
                table: "TuitionPlans",
                columns: new[] { "Id", "BranchId", "CreatedAt", "Currency", "IsActive", "IsDeleted", "Name", "ProgramId", "TotalSessions", "TuitionAmount", "UnitPriceSession", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VND", true, false, "Gói 3 tháng - Beginner", new Guid("10000000-0000-0000-0000-000000000001"), 30, 5000000m, 166667m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VND", true, false, "Gói 6 tháng - Beginner", new Guid("10000000-0000-0000-0000-000000000001"), 60, 9000000m, 150000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VND", true, false, "Gói 3 tháng - Intermediate", new Guid("10000000-0000-0000-0000-000000000002"), 36, 6000000m, 166667m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            // Create Classes
            var class1Id = new Guid("80000000-0000-0000-0000-000000000001");
            var class2Id = new Guid("80000000-0000-0000-0000-000000000002");
            var class3Id = new Guid("80000000-0000-0000-0000-000000000003");

            migrationBuilder.InsertData(
                schema: "public",
                table: "Classes",
                columns: new[] { "Id", "AssistantTeacherId", "BranchId", "Capacity", "Code", "CreatedAt", "EndDate", "MainTeacherId", "ProgramId", "SchedulePattern", "StartDate", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { class1Id, null, new Guid("11111111-1111-1111-1111-111111111111"), 15, "CLASS-BEGINNER-001", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 6, 30), null, new Guid("10000000-0000-0000-0000-000000000001"), "FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=17;BYMINUTE=0", new DateOnly(2024, 1, 15), "Active", "Lớp Tiếng Anh Beginner - Khóa 1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { class2Id, null, new Guid("11111111-1111-1111-1111-111111111111"), 20, "CLASS-INTERMEDIATE-001", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 7, 31), null, new Guid("10000000-0000-0000-0000-000000000002"), "FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=18;BYMINUTE=0", new DateOnly(2024, 2, 1), "Active", "Lớp Tiếng Anh Intermediate - Khóa 1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { class3Id, null, new Guid("11111111-1111-1111-1111-111111111111"), 12, "CLASS-BEGINNER-002", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 8, 31), null, new Guid("10000000-0000-0000-0000-000000000001"), "FREQ=WEEKLY;BYDAY=SA;BYHOUR=9;BYMINUTE=0", new DateOnly(2024, 3, 1), "Planned", "Lớp Tiếng Anh Beginner - Khóa 2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            // Create Enrollments
            var now = new DateTime(2024, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc);
            migrationBuilder.InsertData(
                schema: "public",
                table: "ClassEnrollments",
                columns: new[] { "Id", "ClassId", "CreatedAt", "EnrollDate", "Status", "StudentProfileId", "TuitionPlanId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("90000000-0000-0000-0000-000000000001"), class1Id, now, new DateOnly(2024, 1, 10), "Active", new Guid("50000000-0000-0000-0000-000000000001"), new Guid("20000000-0000-0000-0000-000000000001"), now },
                    { new Guid("90000000-0000-0000-0000-000000000002"), class1Id, now.AddDays(1), new DateOnly(2024, 1, 11), "Active", new Guid("50000000-0000-0000-0000-000000000002"), new Guid("20000000-0000-0000-0000-000000000001"), now.AddDays(1) },
                    { new Guid("90000000-0000-0000-0000-000000000003"), class2Id, now.AddDays(2), new DateOnly(2024, 1, 12), "Active", new Guid("50000000-0000-0000-0000-000000000003"), new Guid("20000000-0000-0000-0000-000000000003"), now.AddDays(2) },
                    { new Guid("90000000-0000-0000-0000-000000000004"), class1Id, now.AddDays(3), new DateOnly(2024, 1, 13), "Paused", new Guid("50000000-0000-0000-0000-000000000004"), new Guid("20000000-0000-0000-0000-000000000001"), now.AddDays(5) },
                    { new Guid("90000000-0000-0000-0000-000000000005"), class2Id, now.AddDays(4), new DateOnly(2024, 1, 14), "Dropped", new Guid("50000000-0000-0000-0000-000000000005"), null, now.AddDays(6) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete Enrollments
            migrationBuilder.DeleteData(
                schema: "public",
                table: "ClassEnrollments",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ClassEnrollments",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ClassEnrollments",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ClassEnrollments",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ClassEnrollments",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000005"));

            // Delete Classes
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classes",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classes",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Classes",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000003"));

            // Delete Tuition Plans
            migrationBuilder.DeleteData(
                schema: "public",
                table: "TuitionPlans",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "TuitionPlans",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "TuitionPlans",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            // Delete Parent-Student Links
            migrationBuilder.DeleteData(
                schema: "public",
                table: "ParentStudentLinks",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ParentStudentLinks",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ParentStudentLinks",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ParentStudentLinks",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ParentStudentLinks",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000005"));

            // Delete Profiles
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Profiles",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"));

            // Delete Users
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"));
        }
    }
}
