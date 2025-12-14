using Kidzgo.Domain.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LeadActivityConfiguration : IEntityTypeConfiguration<LeadActivity>
{
    public void Configure(EntityTypeBuilder<LeadActivity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LeadId)
            .IsRequired();

        builder.Property(x => x.ActivityType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Content);

        builder.Property(x => x.NextActionAt);

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Lead)
            .WithMany(x => x.LeadActivities)
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedLeadActivities)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
