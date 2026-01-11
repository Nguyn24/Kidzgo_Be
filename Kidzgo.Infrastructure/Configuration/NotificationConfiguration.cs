using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.RecipientUserId)
            .IsRequired();

        builder.Property(x => x.RecipientProfileId);

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Content);

        builder.Property(x => x.Deeplink);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SentAt);

        builder.Property(x => x.ReadAt);

        builder.Property(x => x.TemplateId)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.RecipientUser)
            .WithMany(x => x.ReceivedNotifications)
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RecipientProfile)
            .WithMany(x => x.ReceivedNotifications)
            .HasForeignKey(x => x.RecipientProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
