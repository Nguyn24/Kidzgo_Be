using Kidzgo.Domain.TeachingMaterials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingMaterialConfiguration : IEntityTypeConfiguration<TeachingMaterial>
{
    public void Configure(EntityTypeBuilder<TeachingMaterial> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.MimeType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FileExtension)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FileType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LessonTitle)
            .HasMaxLength(255);

        builder.Property(x => x.RelativePath)
            .HasMaxLength(500);

        builder.Property(x => x.IsEncrypted)
            .IsRequired();

        builder.Property(x => x.EncryptionAlgorithm)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EncryptionKeyVersion)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PdfPreviewPath)
            .HasMaxLength(500);

        builder.Property(x => x.UploadedByUserId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ProgramId);
        builder.HasIndex(x => new { x.ProgramId, x.UnitNumber, x.LessonNumber });

        builder.HasOne(x => x.Program)
            .WithMany()
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
