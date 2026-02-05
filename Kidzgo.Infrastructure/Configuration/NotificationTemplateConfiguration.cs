using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Content);

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

        // Seed notification templates
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // UC-331: Session Reminder Template
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Code = "SESSION_REMINDER",
            Channel = NotificationChannel.Email,
            Title = "Nhắc nhở: Buổi học {{session_title}} sắp bắt đầu",
            Content = """
                     <p>Xin chào,</p>
                     <p>Đây là thông báo nhắc nhở về buổi học sắp tới của bạn:</p>
                     <ul>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Thời gian:</strong> {{session_start_time}}</li>
                         <li><strong>Địa điểm:</strong> {{location}}</li>
                     </ul>
                     <p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>
                     <p>Trân trọng,<br/>Kidzgo Team</p>
                     """,
            Placeholders = """["session_title","session_start_time","class_name","location"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // UC-332: Homework Reminder Template
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Code = "HOMEWORK_REMINDER",
            Channel = NotificationChannel.Email,
            Title = "Nhắc nhở: Bài tập {{homework_title}} sắp đến hạn",
            Content = """
                     <p>Xin chào,</p>
                     <p>Bạn có bài tập sắp đến hạn:</p>
                     <ul>
                         <li><strong>Tên bài tập:</strong> {{homework_title}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Hạn nộp:</strong> {{due_date}}</li>
                     </ul>
                     <p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>
                     <p>Trân trọng,<br/>Kidzgo Team</p>
                     """,
            Placeholders = """["homework_title","due_date","class_name"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // UC-333: Tuition Reminder Template
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Code = "TUITION_REMINDER",
            Channel = NotificationChannel.Email,
            Title = "Nhắc nhở: Học phí của {{student_name}} sắp đến hạn",
            Content = """
                     <p>Xin chào,</p>
                     <p>Đây là thông báo nhắc nhở về học phí sắp đến hạn:</p>
                     <ul>
                         <li><strong>Học sinh:</strong> {{student_name}}</li>
                         <li><strong>Số tiền:</strong> {{amount}} VNĐ</li>
                         <li><strong>Hạn thanh toán:</strong> {{due_date}}</li>
                     </ul>
                     <p>Vui lòng thanh toán học phí trước thời hạn để đảm bảo việc học không bị gián đoạn.</p>
                     <p>Trân trọng,<br/>Kidzgo Team</p>
                     """,
            Placeholders = """["student_name","amount","due_date"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // UC-334: Makeup Reminder Template
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Code = "MAKEUP_REMINDER",
            Channel = NotificationChannel.Email,
            Title = "Nhắc nhở: Buổi bù {{session_title}} sắp bắt đầu",
            Content = """
                     <p>Xin chào,</p>
                     <p>Đây là thông báo nhắc nhở về buổi bù sắp tới:</p>
                     <ul>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Thời gian:</strong> {{session_start_time}}</li>
                         <li><strong>Địa điểm:</strong> {{location}}</li>
                     </ul>
                     <p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>
                     <p>Trân trọng,<br/>Kidzgo Team</p>
                     """,
            Placeholders = """["session_title","session_start_time","class_name","location"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // UC-335: Mission Reminder Template
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Code = "MISSION_REMINDER",
            Channel = NotificationChannel.Email,
            Title = "Nhắc nhở: Nhiệm vụ {{mission_title}} sắp kết thúc",
            Content = """
                     <p>Xin chào,</p>
                     <p>Bạn có nhiệm vụ sắp đến hạn:</p>
                     <ul>
                         <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>
                     </ul>
                     <p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>
                     <p>Trân trọng,<br/>Kidzgo Team</p>
                     """,
            Placeholders = """["mission_title","due_date","class_name"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // UC-336: Media Reminder Template
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Code = "MEDIA_REMINDER",
            Channel = NotificationChannel.Email,
            Title = "Thông báo: Có {{media_type}} mới từ lớp {{class_name}}",
            Content = """
                     <p>Xin chào,</p>
                     <p>Lớp học của bạn vừa có nội dung mới:</p>
                     <ul>
                         <li><strong>Tiêu đề:</strong> {{media_title}}</li>
                         <li><strong>Loại:</strong> {{media_type}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                     </ul>
                     <p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>
                     <p>Trân trọng,<br/>Kidzgo Team</p>
                     """,
            Placeholders = """["media_title","media_type","class_name"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });
    }
}
