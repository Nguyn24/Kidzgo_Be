using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class MissionProgressConfiguration : IEntityTypeConfiguration<MissionProgress>
{
    public void Configure(EntityTypeBuilder<MissionProgress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.MissionId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.HasIndex(x => new { x.MissionId, x.StudentProfileId })
            .IsUnique()
            .HasDatabaseName("mission_progress_unique");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ProgressValue)
            .HasColumnType("numeric");

        builder.Property(x => x.CompletedAt);

        builder.Property(x => x.VerifiedBy);

        // Relationships
        builder.HasOne(x => x.Mission)
            .WithMany(x => x.MissionProgresses)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.MissionProgresses)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.VerifiedByUser)
            .WithMany(x => x.VerifiedMissionProgresses)
            .HasForeignKey(x => x.VerifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
