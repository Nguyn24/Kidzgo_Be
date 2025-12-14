using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class StudentLevelConfiguration : IEntityTypeConfiguration<StudentLevel>
{
    public void Configure(EntityTypeBuilder<StudentLevel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.CurrentLevel)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CurrentXp)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.StudentProfile)
            .WithOne(x => x.StudentLevel)
            .HasForeignKey<StudentLevel>(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
