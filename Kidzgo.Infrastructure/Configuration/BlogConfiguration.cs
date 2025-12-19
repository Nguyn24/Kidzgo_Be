using Kidzgo.Domain.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Summary)
            .HasMaxLength(500);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.FeaturedImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.PublishedAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Index for published blogs (landing page queries)
        builder.HasIndex(x => new { x.IsPublished, x.PublishedAt })
            .HasDatabaseName("blog_published_idx");

        // Relationships
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

