using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExerciseSubmissionConfiguration : IEntityTypeConfiguration<ExerciseSubmission>
{
    public void Configure(EntityTypeBuilder<ExerciseSubmission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ExerciseId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.Answers)
            .HasColumnType("jsonb"); // JSON object: { questionId: answer }

        builder.Property(x => x.Score)
            .HasColumnType("numeric");

        builder.Property(x => x.SubmittedAt)
            .IsRequired();

        builder.Property(x => x.GradedAt);

        builder.Property(x => x.GradedBy);

        // Unique constraint: one submission per student per exercise
        builder.HasIndex(x => new { x.ExerciseId, x.StudentProfileId })
            .IsUnique()
            .HasDatabaseName("exercise_submission_unique");

        // Relationships
        builder.HasOne(x => x.Exercise)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.ExerciseId)
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

