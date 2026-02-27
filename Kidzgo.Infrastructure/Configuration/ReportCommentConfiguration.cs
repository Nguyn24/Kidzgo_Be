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

        // ReportId is for StudentMonthlyReport (required)
        builder.Property(x => x.ReportId)
            .IsRequired();

        // SessionReportId is for SessionReport (optional)
        builder.Property(x => x.SessionReportId)
            .IsRequired(false);

        builder.Property(x => x.CommenterId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Check that at least one of ReportId or SessionReportId is set
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_ReportComment_AtLeastOneReportId",
            "(\"ReportId\" IS NOT NULL OR \"SessionReportId\" IS NOT NULL)"));

        // Relationships
        builder.HasOne(x => x.Report)
            .WithMany(x => x.ReportComments)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SessionReport)
            .WithMany(x => x.ReportComments)
            .HasForeignKey(x => x.SessionReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CommenterUser)
            .WithMany(x => x.ReportComments)
            .HasForeignKey(x => x.CommenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
