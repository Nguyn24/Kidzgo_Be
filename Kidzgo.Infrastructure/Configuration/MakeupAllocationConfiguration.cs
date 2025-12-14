using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class MakeupAllocationConfiguration : IEntityTypeConfiguration<MakeupAllocation>
{
    public void Configure(EntityTypeBuilder<MakeupAllocation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.MakeupCreditId)
            .IsRequired();

        builder.Property(x => x.TargetSessionId)
            .IsRequired();

        builder.Property(x => x.AssignedBy);

        builder.Property(x => x.AssignedAt);

        // Relationships
        builder.HasOne(x => x.MakeupCredit)
            .WithMany(x => x.MakeupAllocations)
            .HasForeignKey(x => x.MakeupCreditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TargetSession)
            .WithMany(x => x.TargetMakeupAllocations)
            .HasForeignKey(x => x.TargetSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AssignedByUser)
            .WithMany(x => x.AssignedMakeupAllocations)
            .HasForeignKey(x => x.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
