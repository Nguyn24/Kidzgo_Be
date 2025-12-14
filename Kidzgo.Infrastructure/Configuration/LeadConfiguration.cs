using Kidzgo.Domain.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Campaign)
            .HasMaxLength(100);

        builder.Property(x => x.ContactName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.ZaloId)
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.BranchPreference);

        builder.Property(x => x.ProgramInterest)
            .HasMaxLength(255);

        builder.Property(x => x.Notes);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.OwnerStaffId);

        builder.Property(x => x.FirstResponseAt);

        builder.Property(x => x.TouchCount)
            .IsRequired();

        builder.Property(x => x.NextActionAt);

        builder.Property(x => x.ConvertedStudentProfileId);

        builder.Property(x => x.ConvertedAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.BranchPreferenceNavigation)
            .WithMany(x => x.PreferredLeads)
            .HasForeignKey(x => x.BranchPreference)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OwnerStaffUser)
            .WithMany(x => x.OwnedLeads)
            .HasForeignKey(x => x.OwnerStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ConvertedStudentProfile)
            .WithMany(x => x.ConvertedLeads)
            .HasForeignKey(x => x.ConvertedStudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PlacementTests)
            .WithOne(x => x.Lead)
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LeadActivities)
            .WithOne(x => x.Lead)
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
