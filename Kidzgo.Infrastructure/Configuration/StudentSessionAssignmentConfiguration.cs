using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class StudentSessionAssignmentConfiguration : IEntityTypeConfiguration<StudentSessionAssignment>
{
    public void Configure(EntityTypeBuilder<StudentSessionAssignment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.ClassEnrollmentId)
            .IsRequired();

        builder.Property(x => x.RegistrationId);

        builder.Property(x => x.Track)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.StudentProfileId);
        builder.HasIndex(x => x.ClassEnrollmentId);
        builder.HasIndex(x => x.RegistrationId);
        builder.HasIndex(x => new { x.SessionId, x.Status });
        builder.HasIndex(x => new { x.StudentProfileId, x.Status });
        builder.HasIndex(x => new { x.SessionId, x.ClassEnrollmentId })
            .IsUnique();

        builder.HasOne(x => x.Session)
            .WithMany(x => x.StudentSessionAssignments)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.StudentSessionAssignments)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ClassEnrollment)
            .WithMany(x => x.StudentSessionAssignments)
            .HasForeignKey(x => x.ClassEnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Registration)
            .WithMany()
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
