using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Level)
            .HasMaxLength(100);

        builder.Property(x => x.TotalSessions)
            .IsRequired();

        builder.Property(x => x.DefaultTuitionAmount)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.UnitPriceSession)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.Classes)
            .WithOne(x => x.Program)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TuitionPlans)
            .WithOne(x => x.Program)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LessonPlanTemplates)
            .WithOne(x => x.Program)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
