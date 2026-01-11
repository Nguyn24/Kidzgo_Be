using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExamSubmissionAnswerConfiguration : IEntityTypeConfiguration<ExamSubmissionAnswer>
{
    public void Configure(EntityTypeBuilder<ExamSubmissionAnswer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.SubmissionId)
            .IsRequired();

        builder.Property(x => x.QuestionId)
            .IsRequired();

        builder.Property(x => x.Answer)
            .IsRequired();

        builder.Property(x => x.IsCorrect)
            .IsRequired();

        builder.Property(x => x.PointsAwarded)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.TeacherFeedback);

        builder.Property(x => x.AnsweredAt);

        // Relationships
        builder.HasOne(x => x.Submission)
            .WithMany(x => x.SubmissionAnswers)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

