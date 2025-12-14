using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LessonPlanConfiguration : IEntityTypeConfiguration<LessonPlan>
{
    public void Configure(EntityTypeBuilder<LessonPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.HasIndex(x => x.SessionId)
            .IsUnique()
            .HasDatabaseName("session_unique");

        builder.Property(x => x.TemplateId);

        builder.Property(x => x.PlannedContent)
            .HasColumnType("jsonb");

        builder.Property(x => x.ActualContent)
            .HasColumnType("jsonb");

        builder.Property(x => x.ActualHomework);

        builder.Property(x => x.TeacherNotes);

        builder.Property(x => x.SubmittedBy);

        builder.Property(x => x.SubmittedAt);

        // Relationships
        builder.HasOne(x => x.Session)
            .WithOne(x => x.LessonPlan)
            .HasForeignKey<LessonPlan>(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Template)
            .WithMany(x => x.LessonPlans)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubmittedByUser)
            .WithMany(x => x.SubmittedLessonPlans)
            .HasForeignKey(x => x.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
