using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class MakeupCreditConfiguration : IEntityTypeConfiguration<MakeupCredit>
{
    public void Configure(EntityTypeBuilder<MakeupCredit> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.SourceSessionId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedReason)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ExpiresAt);

        builder.Property(x => x.UsedSessionId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.MakeupCredits)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SourceSession)
            .WithMany(x => x.SourceMakeupCredits)
            .HasForeignKey(x => x.SourceSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UsedSession)
            .WithMany(x => x.UsedMakeupCredits)
            .HasForeignKey(x => x.UsedSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MakeupAllocations)
            .WithOne(x => x.MakeupCredit)
            .HasForeignKey(x => x.MakeupCreditId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
