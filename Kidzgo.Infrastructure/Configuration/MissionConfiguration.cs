using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.Scope)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TargetClassId);

        builder.Property(x => x.TargetGroup)
            .HasColumnType("jsonb");

        builder.Property(x => x.MissionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.StartAt);

        builder.Property(x => x.EndAt);

        builder.Property(x => x.RewardStars);

        builder.Property(x => x.RewardExp);

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.TargetClass)
            .WithMany(x => x.TargetMissions)
            .HasForeignKey(x => x.TargetClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedMissions)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MissionProgresses)
            .WithOne(x => x.Mission)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.HomeworkAssignments)
            .WithOne(x => x.Mission)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Exercises)
            .WithOne(x => x.Mission)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
