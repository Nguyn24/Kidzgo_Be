using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExamSubmissionConfiguration : IEntityTypeConfiguration<ExamSubmission>
{
    public void Configure(EntityTypeBuilder<ExamSubmission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ExamId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.ActualStartTime);

        builder.Property(x => x.SubmittedAt);

        builder.Property(x => x.AutoSubmittedAt);

        builder.Property(x => x.TimeSpentMinutes);

        builder.Property(x => x.AutoScore)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.FinalScore)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.GradedBy);

        builder.Property(x => x.GradedAt);

        builder.Property(x => x.TeacherComment);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Exam)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GradedByUser)
            .WithMany()
            .HasForeignKey(x => x.GradedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SubmissionAnswers)
            .WithOne(x => x.Submission)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

