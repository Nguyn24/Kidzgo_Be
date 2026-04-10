using Kidzgo.Domain.TeachingMaterials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingMaterialBookmarkConfiguration : IEntityTypeConfiguration<TeachingMaterialBookmark>
{
    public void Configure(EntityTypeBuilder<TeachingMaterialBookmark> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.TeachingMaterialId, x.UserId })
            .IsUnique();

        builder.HasOne(x => x.TeachingMaterial)
            .WithMany(x => x.Bookmarks)
            .HasForeignKey(x => x.TeachingMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
