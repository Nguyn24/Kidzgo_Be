using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ReportCommentConfiguration : IEntityTypeConfiguration<ReportComment>
{
    public void Configure(EntityTypeBuilder<ReportComment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ReportId)
            .IsRequired();

        builder.Property(x => x.CommenterId)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Report)
            .WithMany(x => x.ReportComments)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CommenterUser)
            .WithMany(x => x.ReportComments)
            .HasForeignKey(x => x.CommenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
