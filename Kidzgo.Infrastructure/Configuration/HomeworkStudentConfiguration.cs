using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class HomeworkStudentConfiguration : IEntityTypeConfiguration<HomeworkStudent>
{
    public void Configure(EntityTypeBuilder<HomeworkStudent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.AssignmentId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.HasIndex(x => new { x.AssignmentId, x.StudentProfileId })
            .IsUnique()
            .HasDatabaseName("homework_student_unique");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SubmittedAt);

        builder.Property(x => x.GradedAt);

        builder.Property(x => x.Score)
            .HasColumnType("numeric");

        builder.Property(x => x.TeacherFeedback);

        builder.Property(x => x.AiFeedback)
            .HasColumnType("jsonb");

        builder.Property(x => x.Attachments)
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(x => x.Assignment)
            .WithMany(x => x.HomeworkStudents)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.HomeworkStudents)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
