using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
{
    public void Configure(EntityTypeBuilder<ExamResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ExamId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnType("numeric");

        builder.Property(x => x.Comment);

        builder.Property(x => x.AttachmentUrls)
            .HasColumnType("jsonb"); // Store as JSON array for multiple image URLs

        builder.Property(x => x.GradedBy);

        builder.Property(x => x.GradedAt);

        // Relationships
        builder.HasOne(x => x.Exam)
            .WithMany(x => x.ExamResults)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.ExamResults)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GradedByUser)
            .WithMany(x => x.GradedExamResults)
            .HasForeignKey(x => x.GradedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
