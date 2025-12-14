using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PayrollLineConfiguration : IEntityTypeConfiguration<PayrollLine>
{
    public void Configure(EntityTypeBuilder<PayrollLine> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.PayrollRunId)
            .IsRequired();

        builder.Property(x => x.StaffUserId)
            .IsRequired();

        builder.Property(x => x.ComponentType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.SourceId);

        builder.Property(x => x.Amount)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.IsPaid)
            .IsRequired();

        builder.Property(x => x.PaidAt);

        // Relationships
        builder.HasOne(x => x.PayrollRun)
            .WithMany(x => x.PayrollLines)
            .HasForeignKey(x => x.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StaffUser)
            .WithMany(x => x.PayrollLines)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
