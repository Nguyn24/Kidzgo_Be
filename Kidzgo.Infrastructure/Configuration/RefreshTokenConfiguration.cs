using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder
            .HasKey(r => r.Id);
        builder
            .Property(r => r.Id)
            .IsRequired();
        builder
            .Property(r => r.Expires)
            .IsRequired();
        builder
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId);
        builder.HasIndex(r => r.Token).IsUnique();
    }
}