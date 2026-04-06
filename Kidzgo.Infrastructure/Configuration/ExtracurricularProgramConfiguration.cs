using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ExtracurricularProgramConfiguration : IEntityTypeConfiguration<ExtracurricularProgram>
{
    public void Configure(EntityTypeBuilder<ExtracurricularProgram> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Capacity).IsRequired();
        builder.Property(x => x.RegisteredCount).IsRequired();
        builder.Property(x => x.Fee).HasColumnType("numeric").IsRequired();
        builder.Property(x => x.Location).HasMaxLength(255);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
