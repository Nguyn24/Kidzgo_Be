using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.SessionDate)
            .IsRequired();

        builder.Property(x => x.EndDate);

        builder.Property(x => x.Reason);

        builder.Property(x => x.NoticeHours);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RequestedAt)
            .IsRequired();

        builder.Property(x => x.ApprovedBy);

        builder.Property(x => x.ApprovedAt);

        // Relationships
        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Class)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany(x => x.ApprovedLeaveRequests)
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
