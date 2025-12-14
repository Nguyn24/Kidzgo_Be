using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ClassEnrollmentConfiguration : IEntityTypeConfiguration<ClassEnrollment>
{
    public void Configure(EntityTypeBuilder<ClassEnrollment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.EnrollDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IsMain)
            .IsRequired();

        builder.Property(x => x.TuitionPlanId);

        // Relationships
        builder.HasOne(x => x.Class)
            .WithMany(x => x.ClassEnrollments)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.ClassEnrollments)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TuitionPlan)
            .WithMany(x => x.ClassEnrollments)
            .HasForeignKey(x => x.TuitionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
