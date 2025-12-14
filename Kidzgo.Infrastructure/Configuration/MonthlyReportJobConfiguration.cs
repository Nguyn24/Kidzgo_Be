using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class MonthlyReportJobConfiguration : IEntityTypeConfiguration<MonthlyReportJob>
{
    public void Configure(EntityTypeBuilder<MonthlyReportJob> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.StartedAt);

        builder.Property(x => x.FinishedAt);

        builder.Property(x => x.AiPayloadRef);

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany(x => x.MonthlyReportJobs)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
