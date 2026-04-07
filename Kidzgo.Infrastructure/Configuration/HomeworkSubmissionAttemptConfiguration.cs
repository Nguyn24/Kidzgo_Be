using Kidzgo.Domain.Homework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class HomeworkSubmissionAttemptConfiguration : IEntityTypeConfiguration<HomeworkSubmissionAttempt>
{
    public void Configure(EntityTypeBuilder<HomeworkSubmissionAttempt> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.HomeworkStudentId)
            .IsRequired();

        builder.Property(x => x.AttemptNumber)
            .IsRequired();

        builder.HasIndex(x => new { x.HomeworkStudentId, x.AttemptNumber })
            .IsUnique()
            .HasDatabaseName("homework_submission_attempt_unique");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnType("numeric");

        builder.Property(x => x.AiFeedback)
            .HasColumnType("jsonb");

        builder.Property(x => x.TextAnswer)
            .HasColumnType("text");

        builder.Property(x => x.AttachmentUrl)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.HomeworkStudent)
            .WithMany(x => x.SubmissionAttempts)
            .HasForeignKey(x => x.HomeworkStudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
