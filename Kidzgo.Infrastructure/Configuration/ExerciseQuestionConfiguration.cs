using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExerciseQuestionConfiguration : IEntityTypeConfiguration<ExerciseQuestion>
{
    public void Configure(EntityTypeBuilder<ExerciseQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ExerciseId)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.QuestionText)
            .IsRequired();

        builder.Property(x => x.QuestionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Options)
            .HasColumnType("jsonb"); // JSON array for multiple choice options

        builder.Property(x => x.CorrectAnswer);

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.Explanation);

        // Relationships
        builder.HasOne(x => x.Exercise)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

