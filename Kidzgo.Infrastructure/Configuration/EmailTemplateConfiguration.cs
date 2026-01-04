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
        var seedTime = new DateTime(2025, 12, 30, 9, 23, 14, 264, DateTimeKind.Utc).AddTicks(6162);

        builder.HasData(new EmailTemplate
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Code = "FORGOT_PASSWORD",
            Subject = "Kidzgo - Đặt lại mật khẩu của bạn",
            Body = """
                   <p>Xin chào {{user_name}},</p>
                   <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                   <p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>
                   <p><a href="{{reset_link}}">Đặt lại mật khẩu</a></p>
                   <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
                   <p>Trân trọng,<br/>Kidzgo Team</p>
                   """,
            Placeholders = """["user_name","reset_link"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedTime,
            UpdatedAt = seedTime
        });
    }
}
