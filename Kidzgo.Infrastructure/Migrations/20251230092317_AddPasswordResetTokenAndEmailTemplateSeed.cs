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
            // Check if table already exists, if not create it
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'public' 
                        AND table_name = 'PasswordResetTokens'
                    ) THEN
                        CREATE TABLE public.""PasswordResetTokens"" (
                            ""Id"" uuid NOT NULL,
                            ""UserId"" uuid NOT NULL,
                            ""Token"" character varying(200) NOT NULL,
                            ""ExpiresAt"" timestamp with time zone NOT NULL,
                            ""UsedAt"" timestamp with time zone,
                            CONSTRAINT ""PK_PasswordResetTokens"" PRIMARY KEY (""Id""),
                            CONSTRAINT ""FK_PasswordResetTokens_Users_UserId"" FOREIGN KEY (""UserId"") 
                                REFERENCES public.""Users"" (""Id"") ON DELETE CASCADE
                        );

                        CREATE INDEX ""IX_PasswordResetTokens_UserId"" 
                            ON public.""PasswordResetTokens"" (""UserId"");
                    END IF;
                END $$;
            ");

            // Insert email template seed data (only if not exists)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM public.""EmailTemplates"" 
                        WHERE ""Id"" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'
                    ) THEN
                        INSERT INTO public.""EmailTemplates"" 
                        (""Id"", ""Body"", ""Code"", ""CreatedAt"", ""IsActive"", ""IsDeleted"", ""Placeholders"", ""Subject"", ""UpdatedAt"")
                        VALUES (
                            'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
                            '<p>Xin chào {{user_name}},</p>
<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>
<p><a href=""{{reset_link}}"">Đặt lại mật khẩu</a></p>
<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
<p>Trân trọng,<br/>Kidzgo Team</p>',
                            'FORGOT_PASSWORD',
                            '2025-12-30 09:23:14.2646162+00',
                            true,
                            false,
                            '[""user_name"",""reset_link""]',
                            'Kidzgo - Đặt lại mật khẩu của bạn',
                            '2025-12-30 09:23:14.2646162+00'
                        );
                    END IF;
                END $$;
            ");
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
