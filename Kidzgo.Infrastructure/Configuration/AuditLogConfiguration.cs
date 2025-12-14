using Kidzgo.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ActorUserId);

        builder.Property(x => x.ActorProfileId);

        builder.Property(x => x.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EntityId);

        builder.Property(x => x.DataBefore)
            .HasColumnType("jsonb");

        builder.Property(x => x.DataAfter)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.ActorUser)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActorProfile)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.ActorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
