using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class EnrollmentConfirmationPdfConfiguration : IEntityTypeConfiguration<EnrollmentConfirmationPdf>
{
    public void Configure(EntityTypeBuilder<EnrollmentConfirmationPdf> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.RegistrationId)
            .IsRequired();

        builder.Property(x => x.EnrollmentId)
            .IsRequired();

        builder.Property(x => x.Track)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FormType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PdfUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.Property(x => x.GeneratedBy);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SnapshotJson)
            .HasColumnType("jsonb");

        builder.HasOne(x => x.Registration)
            .WithMany()
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Enrollment)
            .WithMany()
            .HasForeignKey(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RegistrationId);
        builder.HasIndex(x => x.EnrollmentId);
        builder.HasIndex(x => x.GeneratedAt);
        builder.HasIndex(x => new { x.EnrollmentId, x.Track, x.FormType, x.IsActive });
    }
}
