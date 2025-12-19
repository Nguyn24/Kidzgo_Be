using Kidzgo.Domain.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PlacementTestConfiguration : IEntityTypeConfiguration<PlacementTest>
{
    public void Configure(EntityTypeBuilder<PlacementTest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LeadId);

        builder.Property(x => x.StudentProfileId);

        builder.Property(x => x.ScheduledAt);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Room)
            .HasMaxLength(100);

        builder.Property(x => x.InvigilatorUserId);

        builder.Property(x => x.ResultScore)
            .HasColumnType("numeric");

        builder.Property(x => x.ListeningScore)
            .HasColumnType("numeric");

        builder.Property(x => x.SpeakingScore)
            .HasColumnType("numeric");

        builder.Property(x => x.ReadingScore)
            .HasColumnType("numeric");

        builder.Property(x => x.WritingScore)
            .HasColumnType("numeric");

        builder.Property(x => x.LevelRecommendation)
            .HasMaxLength(100);

        builder.Property(x => x.ProgramRecommendation)
            .HasMaxLength(100);

        builder.Property(x => x.Notes);

        builder.Property(x => x.AttachmentUrl);

        // Relationships
        builder.HasOne(x => x.Lead)
            .WithMany(x => x.PlacementTests)
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.PlacementTests)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany(x => x.PlacementTests)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvigilatorUser)
            .WithMany(x => x.InvigilatedPlacementTests)
            .HasForeignKey(x => x.InvigilatorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
