using Kidzgo.Domain.Users;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ProfileType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PinHash)
            .HasMaxLength(97); // PBKDF2-SHA512 format: 64-char hash + '-' + 32-char salt = 97 chars

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(x => x.Profiles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ParentLinks)
            .WithOne(x => x.ParentProfile)
            .HasForeignKey(x => x.ParentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StudentLinks)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ClassEnrollments)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LeaveRequests)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attendances)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MakeupCredits)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.HomeworkStudents)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ExamResults)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StudentMonthlyReports)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MissionProgresses)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StarTransactions)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentLevel)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey<StudentLevel>(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RewardRedemptions)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PlacementTests)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MediaAssets)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Invoices)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ConvertedLeads)
            .WithOne(x => x.ConvertedStudentProfile)
            .HasForeignKey(x => x.ConvertedStudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ReceivedNotifications)
            .WithOne(x => x.RecipientProfile)
            .HasForeignKey(x => x.RecipientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OpenedTickets)
            .WithOne(x => x.OpenedByProfile)
            .HasForeignKey(x => x.OpenedByProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TicketComments)
            .WithOne(x => x.CommenterProfile)
            .HasForeignKey(x => x.CommenterProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AuditLogs)
            .WithOne(x => x.ActorProfile)
            .HasForeignKey(x => x.ActorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
