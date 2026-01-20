using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.AttendanceStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AbsenceType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.MarkedBy);

        builder.Property(x => x.MarkedAt);

        builder.Property(x => x.Note);

        builder.HasIndex(x => new { x.SessionId, x.StudentProfileId })
            .IsUnique()
            .HasDatabaseName("attendance_unique");

        // Relationships
        builder.HasOne(x => x.Session)
            .WithMany(x => x.Attendances)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.Attendances)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MarkedByUser)
            .WithMany(x => x.MarkedAttendances)
            .HasForeignKey(x => x.MarkedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
