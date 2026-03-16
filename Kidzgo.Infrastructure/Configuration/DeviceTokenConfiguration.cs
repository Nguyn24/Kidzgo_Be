using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.DeviceType)
            .HasMaxLength(50);

        builder.Property(x => x.DeviceId)
            .HasMaxLength(200);

        builder.Property(x => x.Role);

        builder.Property(x => x.Browser);

        builder.Property(x => x.BranchId);

        builder.Property(x => x.Locale);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Index for quick lookup
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsActive });
        
        // Unique constraint: one active token per user per device
        builder.HasIndex(x => new { x.UserId, x.DeviceId, x.IsActive })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

