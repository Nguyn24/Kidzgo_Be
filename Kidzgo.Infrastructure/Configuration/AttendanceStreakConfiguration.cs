using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class AttendanceStreakConfiguration : IEntityTypeConfiguration<AttendanceStreak>
{
    public void Configure(EntityTypeBuilder<AttendanceStreak> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.AttendanceDate)
            .IsRequired();

        builder.Property(x => x.CurrentStreak)
            .IsRequired();

        builder.Property(x => x.RewardStars)
            .IsRequired();

        builder.Property(x => x.RewardExp)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Unique constraint: one attendance per student per day
        builder.HasIndex(x => new { x.StudentProfileId, x.AttendanceDate })
            .IsUnique()
            .HasDatabaseName("attendance_streak_unique");

        // Relationships
        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

