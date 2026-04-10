using Kidzgo.Domain.TeachingMaterials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingMaterialAnnotationConfiguration : IEntityTypeConfiguration<TeachingMaterialAnnotation>
{
    public void Configure(EntityTypeBuilder<TeachingMaterialAnnotation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasMaxLength(20)
            .HasDefaultValue("#FFD700");

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TeachingMaterialAnnotationType.Note)
            .IsRequired();

        builder.Property(x => x.Visibility)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TeachingMaterialAnnotationVisibility.Private)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.TeachingMaterialId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.TeachingMaterialId, x.SlideNumber });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Annotation_Type", "\"Type\" IN ('Note', 'Highlight', 'Pin')");
            t.HasCheckConstraint("CK_Annotation_Visibility", "\"Visibility\" IN ('Private', 'Class', 'Public')");
        });

        builder.HasOne(x => x.TeachingMaterial)
            .WithMany(x => x.Annotations)
            .HasForeignKey(x => x.TeachingMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
