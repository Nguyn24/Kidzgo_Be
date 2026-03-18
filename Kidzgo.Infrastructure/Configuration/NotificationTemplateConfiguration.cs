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
                     <p>Đây là thông báo nhắc nhở về buổi học sắp tới của học sinh <strong>{{student_name}}</strong>:</p>
                     <ul>
                         <li><strong>Học sinh:</strong> {{student_name}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Thời gian:</strong> {{session_start_time}}</li>
                         <li><strong>Địa điểm:</strong> Rex English Center</li>
                         <li><strong>Phòng học:</strong> {{classroom_name}}</li>
                     </ul>
                     <p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>
                     <p>Trân trọng,<br/>Rex English Team</p>
                     """,
            Placeholders = """["session_title","session_start_time","class_name","location","student_name","classroom_name"]""",
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
                     <p>Học sinh <strong>{{student_name}}</strong> có bài tập sắp đến hạn:</p>
                     <ul>
                         <li><strong>Học sinh:</strong> {{student_name}}</li>
                         <li><strong>Tên bài tập:</strong> {{homework_title}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Hạn nộp:</strong> {{due_date}}</li>
                     </ul>
                     <p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>
                     <p>Trân trọng,<br/>Rex English Team</p>
                     """,
            Placeholders = """["homework_title","due_date","class_name","student_name"]""",
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
                     <p>Đây là thông báo nhắc nhở về học phí sắp đến hạn của học sinh <strong>{{student_name}}</strong>:</p>
                     <ul>
                         <li><strong>Học sinh:</strong> {{student_name}}</li>
                         <li><strong>Số tiền:</strong> {{amount}} VNĐ</li>
                         <li><strong>Hạn thanh toán:</strong> {{due_date}}</li>
                     </ul>
                     <p>Vui lòng thanh toán học phí trước thời hạn để đảm bảo việc học không bị gián đoạn.</p>
                     <p>Trân trọng,<br/>Rex English Team</p>
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
                     <p>Đây là thông báo nhắc nhở về buổi bù sắp tới của học sinh <strong>{{student_name}}</strong>:</p>
                     <ul>
                         <li><strong>Học sinh:</strong> {{student_name}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Thời gian:</strong> {{session_start_time}}</li>
                         <li><strong>Địa điểm:</strong> Rex English Center</li>
                         <li><strong>Phòng học:</strong> {{classroom_name}}</li>
                     </ul>
                     <p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>
                     <p>Trân trọng,<br/>Rex English Team</p>
                     """,
            Placeholders = """["session_title","session_start_time","class_name","location","student_name","classroom_name"]""",
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
                     <p>Học sinh <strong>{{student_name}}</strong> có nhiệm vụ sắp đến hạn:</p>
                     <ul>
                         <li><strong>Học sinh:</strong> {{student_name}}</li>
                         <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                         <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>
                     </ul>
                     <p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>
                     <p>Trân trọng,<br/>Rex English Team</p>
                     """,
            Placeholders = """["mission_title","due_date","class_name","student_name"]""",
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
                     <p>Lớp học của học sinh <strong>{{student_name}}</strong> vừa có nội dung mới:</p>
                     <ul>
                         <li><strong>Học sinh:</strong> {{student_name}}</li>
                         <li><strong>Tiêu đề:</strong> {{media_title}}</li>
                         <li><strong>Loại:</strong> {{media_type}}</li>
                         <li><strong>Lớp học:</strong> {{class_name}}</li>
                     </ul>
                     <p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>
                     <p>Trân trọng,<br/>Rex English Team</p>
                     """,
            Placeholders = """["media_title","media_type","class_name","student_name"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Approved (Email)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Code = "PAUSE_ENROLLMENT_APPROVED_EMAIL",
            Channel = NotificationChannel.Email,
            Title = "Yêu cầu bảo lưu đã được duyệt",
            Content = """
                     <div style="font-family: Arial, sans-serif; color:#222; line-height:1.6;">
                       <h2 style="color:#2b6cb0;">Yêu cầu bảo lưu đã được duyệt</h2>
                       <p>Xin chào,</p>
                       <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã được duyệt.</p>
                       <div style="background:#f7fafc;border:1px solid #e2e8f0;border-radius:8px;padding:12px;">
                         <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>
                       </div>
                       <p>Vui lòng theo dõi lịch học sau thời gian bảo lưu.</p>
                       <p>Trân trọng,<br/>KidzGo Team</p>
                     </div>
                     """,
            Placeholders = """["student_name","pause_from","pause_to"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Rejected (Email)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Code = "PAUSE_ENROLLMENT_REJECTED_EMAIL",
            Channel = NotificationChannel.Email,
            Title = "Yêu cầu bảo lưu bị từ chối",
            Content = """
                     <div style="font-family: Arial, sans-serif; color:#222; line-height:1.6;">
                       <h2 style="color:#c53030;">Yêu cầu bảo lưu bị từ chối</h2>
                       <p>Xin chào,</p>
                       <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã bị từ chối.</p>
                       <div style="background:#fff5f5;border:1px solid #fed7d7;border-radius:8px;padding:12px;">
                         <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>
                       </div>
                       <p>Vui lòng liên hệ trung tâm nếu cần hỗ trợ thêm.</p>
                       <p>Trân trọng,<br/>KidzGo Team</p>
                     </div>
                     """,
            Placeholders = """["student_name","pause_from","pause_to"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Outcome Updated (Email)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Code = "PAUSE_ENROLLMENT_OUTCOME_EMAIL",
            Channel = NotificationChannel.Email,
            Title = "Kết quả bảo lưu đã được cập nhật",
            Content = """
                     <div style="font-family: Arial, sans-serif; color:#222; line-height:1.6;">
                       <h2 style="color:#2f855a;">Kết quả bảo lưu đã được cập nhật</h2>
                       <p>Xin chào,</p>
                       <p>Kết quả bảo lưu của <strong>{{student_name}}</strong> đã được cập nhật.</p>
                       <div style="background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;padding:12px;">
                         <p><strong>Kết quả:</strong> {{outcome}}</p>
                         <p><strong>Ghi chú:</strong> {{outcome_note}}</p>
                       </div>
                       <p>Trân trọng,<br/>KidzGo Team</p>
                     </div>
                     """,
            Placeholders = """["student_name","outcome","outcome_note"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Approved (Push)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Code = "PAUSE_ENROLLMENT_APPROVED_PUSH",
            Channel = NotificationChannel.Push,
            Title = "Bảo lưu đã được duyệt",
            Content = "Yêu cầu bảo lưu {{pause_from}} - {{pause_to}} đã được duyệt.",
            Placeholders = """["pause_from","pause_to"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Rejected (Push)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Code = "PAUSE_ENROLLMENT_REJECTED_PUSH",
            Channel = NotificationChannel.Push,
            Title = "Bảo lưu bị từ chối",
            Content = "Yêu cầu bảo lưu {{pause_from}} - {{pause_to}} đã bị từ chối.",
            Placeholders = """["pause_from","pause_to"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Outcome Updated (Push)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Code = "PAUSE_ENROLLMENT_OUTCOME_PUSH",
            Channel = NotificationChannel.Push,
            Title = "Kết quả bảo lưu đã cập nhật",
            Content = "Kết quả bảo lưu: {{outcome}}.",
            Placeholders = """["outcome"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Approved (Zalo OA)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            Code = "PAUSE_ENROLLMENT_APPROVED_ZALO",
            Channel = NotificationChannel.ZaloOa,
            Title = "Bảo lưu đã được duyệt",
            Content = "Yêu cầu bảo lưu của {{student_name}} từ {{pause_from}} đến {{pause_to}} đã được duyệt.",
            Placeholders = """["student_name","pause_from","pause_to"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Rejected (Zalo OA)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            Code = "PAUSE_ENROLLMENT_REJECTED_ZALO",
            Channel = NotificationChannel.ZaloOa,
            Title = "Bảo lưu bị từ chối",
            Content = "Yêu cầu bảo lưu của {{student_name}} từ {{pause_from}} đến {{pause_to}} đã bị từ chối.",
            Placeholders = """["student_name","pause_from","pause_to"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Pause Enrollment - Outcome Updated (Zalo OA)
        builder.HasData(new NotificationTemplate
        {
            Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            Code = "PAUSE_ENROLLMENT_OUTCOME_ZALO",
            Channel = NotificationChannel.ZaloOa,
            Title = "Kết quả bảo lưu đã cập nhật",
            Content = "Kết quả bảo lưu của {{student_name}}: {{outcome}}. {{outcome_note}}",
            Placeholders = """["student_name","outcome","outcome_note"]""",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });
    }
}
