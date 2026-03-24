using Kidzgo.Domain.Homework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class HomeworkQuestionConfiguration : IEntityTypeConfiguration<HomeworkQuestion>
{
    public void Configure(EntityTypeBuilder<HomeworkQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.HomeworkAssignmentId)
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
            .HasColumnType("jsonb");

        builder.Property(x => x.CorrectAnswer);

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.Explanation);

        builder.HasOne(x => x.HomeworkAssignment)
            .WithMany()
            .HasForeignKey(x => x.HomeworkAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
