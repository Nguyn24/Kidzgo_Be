using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExerciseSubmissionAnswerConfiguration : IEntityTypeConfiguration<ExerciseSubmissionAnswer>
{
    public void Configure(EntityTypeBuilder<ExerciseSubmissionAnswer> builder)
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
            .HasColumnType("numeric");

        builder.Property(x => x.TeacherFeedback);

        // Unique constraint: one answer per question per submission
        builder.HasIndex(x => new { x.SubmissionId, x.QuestionId })
            .IsUnique()
            .HasDatabaseName("exercise_submission_answer_unique");

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

