using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeacherCompensationSettingsConfiguration : IEntityTypeConfiguration<TeacherCompensationSettings>
{
    public void Configure(EntityTypeBuilder<TeacherCompensationSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StandardSessionDurationMinutes)
            .IsRequired();

        builder.Property(x => x.ForeignTeacherDefaultSessionRate)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.VietnameseTeacherDefaultSessionRate)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.AssistantDefaultSessionRate)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}
