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

        // Seed một template mẫu cho quên mật khẩu
        // Sử dụng static DateTime thay vì DateTime.UtcNow để tránh PendingModelChangesWarning
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
            Subject = "KidzGo | Hồ sơ {{profile_name}} đã được tạo thành công",
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
                                     Xin chào {{profile_name}}, tài khoản {{profile_type}} của bạn đã được tạo và sẵn sàng cho bước xác minh.
                                   </p>
                                 </div>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:26px 30px 12px 30px;">
                                 <p style="margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;">
                                   Vui lòng kiểm tra thông tin bên dưới. Nếu cần chỉnh sửa, bạn có thể cập nhật nhanh chỉ với 1 lần nhấp.
                                 </p>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:0 30px 20px 30px;">
                                 <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #e2e8f0;border-radius:12px;background:#f8fafc;">
                                   <tr>
                                     <td style="padding:16px 18px;">
                                       <p style="margin:0 0 8px 0;font-size:13px;color:#64748b;">Thông tin hồ sơ</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Tên hiển thị:</strong> {{profile_name}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Họ tên:</strong> {{full_name}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Loại hồ sơ:</strong> {{profile_type}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Giới tính:</strong> {{gender}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Ngày sinh:</strong> {{birth_day}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Email:</strong> {{email}}</p>
                                       <p style="margin:0 0 6px 0;font-size:14px;"><strong>Số điện thoại:</strong> {{phone}}</p>
                                       <p style="margin:0 0 0 0;font-size:14px;"><strong>Zalo ID:</strong> {{zalo_id}}</p>
                                     </td>
                                   </tr>
                                 </table>
                               </td>
                             </tr>
                             <tr>
                               <td style="padding:6px 30px 28px 30px;">
                                 <table role="presentation" cellspacing="0" cellpadding="0">
                                   <tr>
                                     <td style="padding-right:10px;">
                                       <a href="{{verify_link}}" style="display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;">Xác minh hồ sơ</a>
                                     </td>
                                     <td>
                                       <a href="{{update_link}}" style="display:inline-block;background:#ffffff;color:#1d4ed8;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;border:1px solid #bfdbfe;">Cập nhật thông tin</a>
                                     </td>
                                   </tr>
                                 </table>
                                 <p style="margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;">
                                   Thời gian tạo hồ sơ: {{created_at}}
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
            Placeholders = """["profile_name","profile_type","email","phone","full_name","gender","birth_day","zalo_id","verify_link","update_link","created_at"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });
    }
}
