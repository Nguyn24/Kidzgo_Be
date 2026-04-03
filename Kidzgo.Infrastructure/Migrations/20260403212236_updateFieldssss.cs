using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateFieldssss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "public",
                table: "NotificationTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NotificationTemplateId",
                schema: "public",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                schema: "public",
                table: "AuditLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "Category",
                value: null);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                column: "Category",
                value: null);

            migrationBuilder.Sql(
                """
                UPDATE public."NotificationTemplates"
                SET "Category" = CASE
                    WHEN "Code" ILIKE '%HOMEWORK%' THEN 'Homework'
                    WHEN "Code" ILIKE '%SESSION%' OR "Code" ILIKE '%MAKEUP%' THEN 'Session'
                    WHEN "Code" ILIKE '%TUITION%' OR "Code" ILIKE '%INVOICE%' OR "Code" ILIKE '%PAY%' THEN 'Finance'
                    WHEN "Code" ILIKE '%MEDIA%' THEN 'Media'
                    WHEN "Code" ILIKE '%MISSION%' OR "Code" ILIKE '%REWARD%' OR "Code" ILIKE '%STAR%' THEN 'Gamification'
                    WHEN "Code" ILIKE '%PAUSE_ENROLLMENT%' OR "Code" ILIKE '%ENROLLMENT%' THEN 'Enrollment'
                    WHEN "Code" ILIKE '%PROFILE%' OR "Code" ILIKE '%ACCOUNT%' OR "Code" ILIKE '%PASSWORD%' OR "Code" ILIKE '%PIN%' THEN 'Account'
                    ELSE "Channel"
                END
                WHERE "Category" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND n."TemplateId" IS NOT NULL
                  AND n."TemplateId" = nt."Id"::text;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND nt."Code" = 'SESSION_REMINDER'
                  AND n."Channel" = 'Email'
                  AND n."Title" ILIKE 'Nhắc nhở: Buổi học%';

                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND nt."Code" = 'HOMEWORK_REMINDER'
                  AND n."Channel" = 'Email'
                  AND n."Title" ILIKE 'Nhắc nhở: Bài tập%';

                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND nt."Code" = 'TUITION_REMINDER'
                  AND n."Channel" = 'Email'
                  AND n."Title" ILIKE 'Nhắc nhở: Học phí%';

                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND nt."Code" = 'MAKEUP_REMINDER'
                  AND n."Channel" = 'Email'
                  AND n."Title" ILIKE 'Nhắc nhở: Buổi bù%';

                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND nt."Code" = 'MISSION_REMINDER'
                  AND n."Channel" = 'Email'
                  AND n."Title" ILIKE 'Nhắc nhở: Nhiệm vụ%';

                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND nt."Code" = 'MEDIA_REMINDER'
                  AND n."Channel" = 'Email'
                  AND n."Title" ILIKE 'Thông báo: Có %';
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."Notifications" AS n
                SET "NotificationTemplateId" = nt."Id"
                FROM public."NotificationTemplates" AS nt
                WHERE n."NotificationTemplateId" IS NULL
                  AND n."Kind" = 'pause_enrollment'
                  AND (
                        (n."Channel" = 'Email' AND n."Title" = 'Yêu cầu bảo lưu đã được duyệt' AND nt."Code" = 'PAUSE_ENROLLMENT_APPROVED_EMAIL') OR
                        (n."Channel" = 'Email' AND n."Title" = 'Yêu cầu bảo lưu bị từ chối' AND nt."Code" = 'PAUSE_ENROLLMENT_REJECTED_EMAIL') OR
                        (n."Channel" = 'Email' AND n."Title" = 'Kết quả bảo lưu đã được cập nhật' AND nt."Code" = 'PAUSE_ENROLLMENT_OUTCOME_EMAIL') OR
                        (n."Channel" = 'Push' AND n."Title" = 'Yêu cầu bảo lưu đã được duyệt' AND nt."Code" = 'PAUSE_ENROLLMENT_APPROVED_PUSH') OR
                        (n."Channel" = 'Push' AND n."Title" = 'Yêu cầu bảo lưu bị từ chối' AND nt."Code" = 'PAUSE_ENROLLMENT_REJECTED_PUSH') OR
                        (n."Channel" = 'Push' AND n."Title" = 'Kết quả bảo lưu đã được cập nhật' AND nt."Code" = 'PAUSE_ENROLLMENT_OUTCOME_PUSH') OR
                        (n."Channel" = 'ZaloOa' AND n."Title" = 'Yêu cầu bảo lưu đã được duyệt' AND nt."Code" = 'PAUSE_ENROLLMENT_APPROVED_ZALO') OR
                        (n."Channel" = 'ZaloOa' AND n."Title" = 'Yêu cầu bảo lưu bị từ chối' AND nt."Code" = 'PAUSE_ENROLLMENT_REJECTED_ZALO') OR
                        (n."Channel" = 'ZaloOa' AND n."Title" = 'Kết quả bảo lưu đã được cập nhật' AND nt."Code" = 'PAUSE_ENROLLMENT_OUTCOME_ZALO')
                      );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationTemplateId",
                schema: "public",
                table: "Notifications",
                column: "NotificationTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationTemplates_NotificationTemplateId",
                schema: "public",
                table: "Notifications",
                column: "NotificationTemplateId",
                principalSchema: "public",
                principalTable: "NotificationTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationTemplates_NotificationTemplateId",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_NotificationTemplateId",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "public",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "NotificationTemplateId",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                schema: "public",
                table: "AuditLogs");
        }
    }
}
