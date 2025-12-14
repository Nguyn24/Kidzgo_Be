using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ShiftAttendanceConfiguration : IEntityTypeConfiguration<ShiftAttendance>
{
    public void Configure(EntityTypeBuilder<ShiftAttendance> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StaffUserId)
            .IsRequired();

        builder.Property(x => x.ContractId);

        builder.Property(x => x.ShiftDate)
            .IsRequired();

        builder.Property(x => x.ShiftHours)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(50);

        builder.Property(x => x.ApprovedBy);

        builder.Property(x => x.ApprovedAt);

        // Relationships
        builder.HasOne(x => x.StaffUser)
            .WithMany(x => x.ShiftAttendances)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.ShiftAttendances)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany(x => x.ApprovedShiftAttendances)
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
