using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class MonthlyWorkHoursConfiguration : IEntityTypeConfiguration<MonthlyWorkHours>
{
    public void Configure(EntityTypeBuilder<MonthlyWorkHours> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StaffUserId)
            .IsRequired();

        builder.Property(x => x.ContractId)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.TotalHours)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.TeachingHours)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.RegularHours)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.OvertimeHours)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.TeachingSessions)
            .IsRequired();

        builder.Property(x => x.IsLocked)
            .IsRequired();

        // Unique constraint: one record per staff per contract per month
        builder.HasIndex(x => new { x.StaffUserId, x.ContractId, x.Year, x.Month })
            .IsUnique()
            .HasDatabaseName("monthly_work_hours_unique");

        // Index for payroll queries
        builder.HasIndex(x => new { x.BranchId, x.Year, x.Month, x.IsLocked })
            .HasDatabaseName("monthly_work_hours_payroll_idx");

        // Relationships
        builder.HasOne(x => x.StaffUser)
            .WithMany()
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Contract)
            .WithMany()
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

