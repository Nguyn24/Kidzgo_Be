using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SessionRoleConfiguration : IEntityTypeConfiguration<SessionRole>
{
    public void Configure(EntityTypeBuilder<SessionRole> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.StaffUserId)
            .IsRequired();

        builder.Property(x => x.RoleType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PayableUnitPrice)
            .HasColumnType("numeric");

        builder.Property(x => x.PayableAllowance)
            .HasColumnType("numeric");

        // Relationships
        builder.HasOne(x => x.Session)
            .WithMany(x => x.SessionRoles)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StaffUser)
            .WithMany(x => x.SessionRoles)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
