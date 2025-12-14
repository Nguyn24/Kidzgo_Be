using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.PeriodStart)
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ApprovedBy);

        builder.Property(x => x.PaidAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany(x => x.PayrollRuns)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany(x => x.ApprovedPayrollRuns)
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PayrollLines)
            .WithOne(x => x.PayrollRun)
            .HasForeignKey(x => x.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PayrollPayments)
            .WithOne(x => x.PayrollRun)
            .HasForeignKey(x => x.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
