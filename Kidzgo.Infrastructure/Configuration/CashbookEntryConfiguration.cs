using Kidzgo.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class CashbookEntryConfiguration : IEntityTypeConfiguration<CashbookEntry>
{
    public void Configure(EntityTypeBuilder<CashbookEntry> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.RelatedType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.RelatedId);

        builder.Property(x => x.EntryDate)
            .IsRequired();

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.AttachmentUrl);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany(x => x.CashbookEntries)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedCashbookEntries)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PayrollPayments)
            .WithOne(x => x.CashbookEntry)
            .HasForeignKey(x => x.CashbookEntryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
