using Kidzgo.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.InvoiceId)
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.PaidAt);

        builder.Property(x => x.ReferenceCode)
            .HasMaxLength(100);

        builder.Property(x => x.ConfirmedBy);

        builder.Property(x => x.EvidenceUrl);

        // Relationships
        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ConfirmedByUser)
            .WithMany(x => x.ConfirmedPayments)
            .HasForeignKey(x => x.ConfirmedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
