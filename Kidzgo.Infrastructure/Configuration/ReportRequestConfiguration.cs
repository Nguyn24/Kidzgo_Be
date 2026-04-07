using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ReportRequestConfiguration : IEntityTypeConfiguration<ReportRequest>
{
    public void Configure(EntityTypeBuilder<ReportRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReportType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AssignedTeacherUserId)
            .IsRequired();

        builder.Property(x => x.RequestedByUserId)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.AssignedTeacherUserId, x.Status, x.Priority, x.DueAt })
            .HasDatabaseName("report_request_teacher_queue_idx");

        builder.HasIndex(x => new { x.ReportType, x.TargetClassId, x.Month, x.Year })
            .HasDatabaseName("report_request_type_class_month_idx");

        builder.HasIndex(x => new { x.ReportType, x.TargetStudentProfileId, x.Month, x.Year })
            .HasDatabaseName("report_request_type_student_month_idx");

        builder.HasOne(x => x.AssignedTeacher)
            .WithMany()
            .HasForeignKey(x => x.AssignedTeacherUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetStudentProfile)
            .WithMany()
            .HasForeignKey(x => x.TargetStudentProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.TargetClass)
            .WithMany()
            .HasForeignKey(x => x.TargetClassId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.TargetSession)
            .WithMany()
            .HasForeignKey(x => x.TargetSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.LinkedSessionReport)
            .WithMany()
            .HasForeignKey(x => x.LinkedSessionReportId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.LinkedMonthlyReport)
            .WithMany()
            .HasForeignKey(x => x.LinkedMonthlyReportId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
