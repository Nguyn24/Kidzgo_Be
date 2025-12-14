using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PayrollPaymentConfiguration : IEntityTypeConfiguration<PayrollPayment>
{
    public void Configure(EntityTypeBuilder<PayrollPayment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.PayrollRunId)
            .IsRequired();

        builder.Property(x => x.StaffUserId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PaidAt);

        builder.Property(x => x.CashbookEntryId);

        // Relationships
        builder.HasOne(x => x.PayrollRun)
            .WithMany(x => x.PayrollPayments)
            .HasForeignKey(x => x.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StaffUser)
            .WithMany(x => x.PayrollPayments)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CashbookEntry)
            .WithMany(x => x.PayrollPayments)
            .HasForeignKey(x => x.CashbookEntryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
