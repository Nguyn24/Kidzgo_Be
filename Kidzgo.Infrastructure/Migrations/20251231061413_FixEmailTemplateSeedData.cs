using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEmailTemplateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update email template data if it exists, otherwise insert
            migrationBuilder.Sql(@"
                UPDATE public.""EmailTemplates""
                SET ""Body"" = '<p>Xin chào {{user_name}},</p>
<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>
<p><a href=""{{reset_link}}"">Đặt lại mật khẩu</a></p>
<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
<p>Trân trọng,<br/>Kidzgo Team</p>',
                    ""CreatedAt"" = '2024-01-01 00:00:00+00'::timestamp with time zone,
                    ""UpdatedAt"" = '2024-01-01 00:00:00+00'::timestamp with time zone
                WHERE ""Id"" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'::uuid;
                
                INSERT INTO public.""EmailTemplates"" (""Id"", ""Body"", ""Code"", ""CreatedAt"", ""IsActive"", ""IsDeleted"", ""Placeholders"", ""Subject"", ""UpdatedAt"")
                SELECT 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'::uuid,
                       '<p>Xin chào {{user_name}},</p>
<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>
<p><a href=""{{reset_link}}"">Đặt lại mật khẩu</a></p>
<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
<p>Trân trọng,<br/>Kidzgo Team</p>',
                       'FORGOT_PASSWORD',
                       '2024-01-01 00:00:00+00'::timestamp with time zone,
                       true,
                       false,
                       '[""user_name"",""reset_link""]',
                       'Kidzgo - Đặt lại mật khẩu của bạn',
                       '2024-01-01 00:00:00+00'::timestamp with time zone
                WHERE NOT EXISTS (
                    SELECT 1 FROM public.""EmailTemplates"" WHERE ""Id"" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'::uuid
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: This migration only fixes email template data, so Down() is a no-op
            // The original email template data from previous migration will remain
        }
    }
}
