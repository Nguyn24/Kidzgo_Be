using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class StudentMonthlyReportConfiguration : IEntityTypeConfiguration<StudentMonthlyReport>
{
    public void Configure(EntityTypeBuilder<StudentMonthlyReport> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.DraftContent)
            .HasColumnType("jsonb");

        builder.Property(x => x.FinalContent)
            .HasColumnType("jsonb");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AiVersion)
            .HasMaxLength(50);

        builder.Property(x => x.ClassId);

        builder.Property(x => x.JobId);

        builder.Property(x => x.PdfUrl);

        builder.Property(x => x.PdfGeneratedAt);

        builder.Property(x => x.SubmittedBy);

        builder.Property(x => x.ReviewedBy);

        builder.Property(x => x.ReviewedAt);

        builder.Property(x => x.PublishedAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Job)
            .WithMany(x => x.Reports)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.StudentMonthlyReports)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SubmittedByUser)
            .WithMany(x => x.SubmittedReports)
            .HasForeignKey(x => x.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany(x => x.ReviewedReports)
            .HasForeignKey(x => x.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ReportComments)
            .WithOne(x => x.Report)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
