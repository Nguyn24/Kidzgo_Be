using Kidzgo.Domain.TeachingMaterials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingMaterialViewProgressConfiguration : IEntityTypeConfiguration<TeachingMaterialViewProgress>
{
    public void Configure(EntityTypeBuilder<TeachingMaterialViewProgress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProgressPercent)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.TotalTimeSeconds)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.ViewCount)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.Completed)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.FirstViewedAt)
            .IsRequired();

        builder.Property(x => x.LastViewedAt)
            .IsRequired();

        builder.HasIndex(x => x.TeachingMaterialId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Completed);
        builder.HasIndex(x => new { x.TeachingMaterialId, x.UserId })
            .IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ViewProgress_Percent", "\"ProgressPercent\" >= 0 AND \"ProgressPercent\" <= 100");
        });

        builder.HasOne(x => x.TeachingMaterial)
            .WithMany(x => x.ViewProgresses)
            .HasForeignKey(x => x.TeachingMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
