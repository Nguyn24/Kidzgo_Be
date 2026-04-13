using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Subject)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Body);

        builder.Property(x => x.Placeholders)
            .HasColumnType("jsonb");

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(new EmailTemplate
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Code = "FORGOT_PASSWORD",
            Subject = "Rex English - Đặt lại mật khẩu của bạn",
            Body = """
                   <p>Xin chào {{user_name}},</p>
                   <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                   <p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>
                   <p><a href="{{reset_link}}">Đặt lại mật khẩu</a></p>
                   <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
                   <p>Trân trọng,<br/>Rex English Team</p>
                   """,
            Placeholders = """["user_name","reset_link"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        builder.HasData(new EmailTemplate
        {
            Id = Guid.Parse("b9f6c8a1-3f57-45c6-8f4b-9f0c2b7d7f10"),
            Code = "PROFILE_CREATED",
            Subject = "KidzGo | Hồ sơ mới đã sẵn sàng xác minh",
            Body = """
                   <div style="margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;">
                     <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f7fb;padding:24px 12px;">
                       <tr>
                         <td align="center">
                           <table role="presentation" width="640" cellspacing="0" cellpadding="0" style="max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);">
                             <tr>
                               <td style="padding:0;background:linear-gradient(135deg,#0ea5e9 0%,#2563eb 100%);">
                                 <div style="padding:28px 30px 24px 30px;color:#ffffff;">
                                   <p style="margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;">KidzGo Learning Center</p>
                                   <h1 style="margin:0;font-size:28px;line-height:1.3;font-weight:700;">Hồ sơ mới đã sẵn sàng</h1>
                                   <p style="margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;">
                                     Xin chào {{recipient_name}}, tài khoản của bạn hiện có {{profile_count}} hồ sơ đã được phê duyệt và sẵn sàng cho bước xác minh.
                                   </p>
                                 </div>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:26px 30px 12px 30px;">
                                 <p style="margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;">
                                   Vui lòng kiểm tra thông tin bên dưới. Mật khẩu đăng nhập và mã PIN phụ huynh hiện đang là mặc định, vui lòng đổi lại sau khi đăng nhập.
                                 </p>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:0 30px 20px 30px;">
                                 <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #dbeafe;border-radius:12px;background:#eff6ff;">
                                   <tr>
                                     <td style="padding:16px 18px;">
                                       <p style="margin:0 0 10px 0;font-size:13px;color:#1d4ed8;text-transform:uppercase;letter-spacing:.04em;">Thông tin tài khoản</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Email đăng nhập:</strong> {{email}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Số điện thoại:</strong> {{phone}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Mật khẩu mặc định:</strong> {{password}}</p>
                                       <p style="margin:0;font-size:14px;"><strong>PIN phụ huynh mặc định:</strong> {{pin}}</p>
                                     </td>
                                   </tr>
                                 </table>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:0 30px 8px 30px;">
                                 <p style="margin:0 0 12px 0;font-size:13px;color:#64748b;">Danh sách hồ sơ đã được duyệt</p>
                                 {{profiles_html}}
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:8px 30px 28px 30px;">
                                 <a href="{{verify_link}}" style="display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;">Xác minh tất cả hồ sơ</a>
                                 <p style="margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;">
                                   Nút xác minh sẽ kích hoạt toàn bộ hồ sơ đã được duyệt của tài khoản này.
                                 </p>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:18px 30px;background:#f8fafc;border-top:1px solid #e2e8f0;">
                                 <p style="margin:0;font-size:12px;line-height:1.7;color:#64748b;">
                                   Nếu bạn không thực hiện thao tác này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.
                                 </p>
                               </td>
                             </tr>
                           </table>
                         </td>
                       </tr>
                     </table>
                   </div>
                   """,
            Placeholders = """["recipient_name","profile_count","profiles_html","email","phone","password","pin","verify_link","profile_names"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        builder.HasData(new EmailTemplate
        {
            Id = Guid.Parse("c1f73d87-8d4c-45c2-bf8f-3d79e2f4b6a1"),
            Code = "PARENT_PIN_RESET",
            Subject = "KidzGo | Yêu cầu đặt lại PIN phụ huynh",
            Body = """
                   <div style="margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;">
                     <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f7fb;padding:24px 12px;">
                       <tr>
                         <td align="center">
                           <table role="presentation" width="640" cellspacing="0" cellpadding="0" style="max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);">
                             <tr>
                               <td style="padding:0;background:linear-gradient(135deg,#f97316 0%,#ea580c 100%);">
                                 <div style="padding:28px 30px 24px 30px;color:#ffffff;">
                                   <p style="margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;">KidzGo Learning Center</p>
                                   <h1 style="margin:0;font-size:28px;line-height:1.3;font-weight:700;">Đặt lại PIN phụ huynh</h1>
                                   <p style="margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;">
                                     Xin chào {{user_name}}, chúng tôi đã nhận được yêu cầu đặt lại PIN cho hồ sơ {{profile_name}}.
                                   </p>
                                 </div>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:26px 30px 12px 30px;">
                                 <p style="margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;">
                                   Để tiếp tục, vui lòng bấm vào nút bên dưới. Liên kết này chỉ có hiệu lực trong 1 giờ.
                                 </p>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:8px 30px 28px 30px;">
                                 <a href="{{reset_link}}" style="display:inline-block;background:#ea580c;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;">Đặt lại PIN</a>
                                 <p style="margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;">
                                   Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.
                                 </p>
                               </td>
                             </tr>
                           </table>
                         </td>
                       </tr>
                     </table>
                   </div>
                   """,
            Placeholders = """["profile_name","user_name","reset_link"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });
    }
}
