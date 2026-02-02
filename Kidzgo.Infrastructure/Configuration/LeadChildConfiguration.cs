using Kidzgo.Domain.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LeadChildConfiguration : IEntityTypeConfiguration<LeadChild>
{
    public void Configure(EntityTypeBuilder<LeadChild> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LeadId)
            .IsRequired();

        builder.Property(x => x.ChildName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Dob);

        builder.Property(x => x.Gender)
            .HasMaxLength(20);

        builder.Property(x => x.ProgramInterest)
            .HasMaxLength(255);

        builder.Property(x => x.Notes);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ConvertedStudentProfileId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Lead)
            .WithMany(x => x.LeadChildren)
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ConvertedStudentProfile)
            .WithMany()
            .HasForeignKey(x => x.ConvertedStudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PlacementTests)
            .WithOne(x => x.LeadChild)
            .HasForeignKey(x => x.LeadChildId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

