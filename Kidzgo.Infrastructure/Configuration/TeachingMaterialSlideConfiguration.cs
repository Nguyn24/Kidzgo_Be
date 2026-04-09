using Kidzgo.Domain.TeachingMaterials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingMaterialSlideConfiguration : IEntityTypeConfiguration<TeachingMaterialSlide>
{
    public void Configure(EntityTypeBuilder<TeachingMaterialSlide> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreviewImagePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ThumbnailImagePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Width)
            .HasDefaultValue(1920)
            .IsRequired();

        builder.Property(x => x.Height)
            .HasDefaultValue(1080)
            .IsRequired();

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.HasIndex(x => x.TeachingMaterialId);
        builder.HasIndex(x => new { x.TeachingMaterialId, x.SlideNumber })
            .IsUnique();

        builder.HasOne(x => x.TeachingMaterial)
            .WithMany(x => x.Slides)
            .HasForeignKey(x => x.TeachingMaterialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
