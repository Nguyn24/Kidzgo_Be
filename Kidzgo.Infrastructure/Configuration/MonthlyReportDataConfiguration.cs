using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class MonthlyReportDataConfiguration : IEntityTypeConfiguration<MonthlyReportData>
{
    public void Configure(EntityTypeBuilder<MonthlyReportData> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ReportId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.AttendanceData)
            .HasColumnType("jsonb");

        builder.Property(x => x.HomeworkData)
            .HasColumnType("jsonb");

        builder.Property(x => x.TestData)
            .HasColumnType("jsonb");

        builder.Property(x => x.MissionData)
            .HasColumnType("jsonb");

        builder.Property(x => x.NotesData)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Report)
            .WithOne()
            .HasForeignKey<MonthlyReportData>(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one data record per report
        builder.HasIndex(x => x.ReportId)
            .IsUnique();
    }
}

