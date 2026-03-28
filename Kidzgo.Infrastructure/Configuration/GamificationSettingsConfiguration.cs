using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class GamificationSettingsConfiguration : IEntityTypeConfiguration<GamificationSettings>
{
    public void Configure(EntityTypeBuilder<GamificationSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CheckInRewardStars)
            .IsRequired();

        builder.Property(x => x.CheckInRewardExp)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}
