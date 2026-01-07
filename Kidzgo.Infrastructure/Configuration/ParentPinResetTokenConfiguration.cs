using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ParentPinResetTokenConfiguration : IEntityTypeConfiguration<ParentPinResetToken>
{
    public void Configure(EntityTypeBuilder<ParentPinResetToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasOne(x => x.Profile)
            .WithMany()
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

