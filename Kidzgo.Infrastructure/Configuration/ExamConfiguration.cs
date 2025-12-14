using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.ExamType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.MaxScore)
            .HasColumnType("numeric");

        builder.Property(x => x.Description);

        builder.Property(x => x.CreatedBy);

        // Relationships
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Exams)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedExams)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ExamResults)
            .WithOne(x => x.Exam)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
