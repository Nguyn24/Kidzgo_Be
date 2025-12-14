using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class StarTransactionConfiguration : IEntityTypeConfiguration<StarTransaction>
{
    public void Configure(EntityTypeBuilder<StarTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(100);

        builder.Property(x => x.SourceType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.SourceId);

        builder.Property(x => x.BalanceAfter)
            .IsRequired();

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.StarTransactions)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedStarTransactions)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
