using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SessionReportConfiguration : IEntityTypeConfiguration<SessionReport>
{
    public void Configure(EntityTypeBuilder<SessionReport> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.TeacherUserId)
            .IsRequired();

        builder.Property(x => x.ReportDate)
            .IsRequired();

        builder.Property(x => x.Feedback)
            .IsRequired();

        builder.Property(x => x.AiGeneratedSummary)
            .HasColumnType("text");

        builder.Property(x => x.IsMonthlyCompiled)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Unique constraint: one report per student per session
        builder.HasIndex(x => new { x.SessionId, x.StudentProfileId })
            .IsUnique()
            .HasDatabaseName("session_report_unique");

        // Index for date range queries (monthly reports)
        builder.HasIndex(x => new { x.TeacherUserId, x.ReportDate })
            .HasDatabaseName("session_report_teacher_date_idx");

        // Relationships
        builder.HasOne(x => x.Session)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TeacherUser)
            .WithMany()
            .HasForeignKey(x => x.TeacherUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

