using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class MissionRewardRuleConfiguration : IEntityTypeConfiguration<MissionRewardRule>
{
    public void Configure(EntityTypeBuilder<MissionRewardRule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.MissionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProgressMode)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(MissionProgressMode.Count);

        builder.Property(x => x.TotalRequired)
            .IsRequired();

        builder.Property(x => x.RewardStars)
            .IsRequired();

        builder.Property(x => x.RewardExp)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.MissionType, x.ProgressMode, x.TotalRequired })
            .IsUnique();
    }
}
