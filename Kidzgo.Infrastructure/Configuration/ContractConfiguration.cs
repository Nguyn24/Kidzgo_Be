using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StaffUserId)
            .IsRequired();

        builder.Property(x => x.ContractType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate);

        builder.Property(x => x.BaseSalary)
            .HasColumnType("numeric");

        builder.Property(x => x.HourlyRate)
            .HasColumnType("numeric");

        builder.Property(x => x.AllowanceFixed)
            .HasColumnType("numeric");

        builder.Property(x => x.MinimumMonthlyHours)
            .HasColumnType("numeric");

        builder.Property(x => x.OvertimeRateMultiplier)
            .HasColumnType("numeric");

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.StaffUser)
            .WithMany(x => x.Contracts)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Contracts)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ShiftAttendances)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
