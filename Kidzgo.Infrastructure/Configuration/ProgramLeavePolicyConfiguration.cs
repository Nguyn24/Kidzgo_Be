using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ProgramLeavePolicyConfiguration : IEntityTypeConfiguration<ProgramLeavePolicy>
{
    public void Configure(EntityTypeBuilder<ProgramLeavePolicy> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.MaxLeavesPerMonth)
            .IsRequired();

        builder.Property(x => x.UpdatedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ProgramId)
            .IsUnique();

        builder.HasOne(x => x.Program)
            .WithMany()
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
