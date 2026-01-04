using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PinHash)
            .HasMaxLength(255);

        builder.Property(x => x.Username)
            .HasMaxLength(100);

        builder.Property(x => x.Role)
            .HasConversion<string>()   
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.BranchId);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Profiles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MainTeacherClasses)
            .WithOne(x => x.MainTeacher)
            .HasForeignKey(x => x.MainTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AssistantTeacherClasses)
            .WithOne(x => x.AssistantTeacher)
            .HasForeignKey(x => x.AssistantTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PlannedTeacherSessions)
            .WithOne(x => x.PlannedTeacher)
            .HasForeignKey(x => x.PlannedTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PlannedAssistantSessions)
            .WithOne(x => x.PlannedAssistant)
            .HasForeignKey(x => x.PlannedAssistantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ActualTeacherSessions)
            .WithOne(x => x.ActualTeacher)
            .HasForeignKey(x => x.ActualTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ActualAssistantSessions)
            .WithOne(x => x.ActualAssistant)
            .HasForeignKey(x => x.ActualAssistantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ApprovedLeaveRequests)
            .WithOne(x => x.ApprovedByUser)
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MarkedAttendances)
            .WithOne(x => x.MarkedByUser)
            .HasForeignKey(x => x.MarkedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AssignedMakeupAllocations)
            .WithOne(x => x.AssignedByUser)
            .HasForeignKey(x => x.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CreatedLessonPlanTemplates)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SubmittedLessonPlans)
            .WithOne(x => x.SubmittedByUser)
            .HasForeignKey(x => x.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CreatedHomeworkAssignments)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CreatedExams)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.GradedExamResults)
            .WithOne(x => x.GradedByUser)
            .HasForeignKey(x => x.GradedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SubmittedReports)
            .WithOne(x => x.SubmittedByUser)
            .HasForeignKey(x => x.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ReviewedReports)
            .WithOne(x => x.ReviewedByUser)
            .HasForeignKey(x => x.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ReportComments)
            .WithOne(x => x.CommenterUser)
            .HasForeignKey(x => x.CommenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CreatedMissions)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.VerifiedMissionProgresses)
            .WithOne(x => x.VerifiedByUser)
            .HasForeignKey(x => x.VerifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CreatedStarTransactions)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.HandledRewardRedemptions)
            .WithOne(x => x.HandledByUser)
            .HasForeignKey(x => x.HandledBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.OwnedLeads)
            .WithOne(x => x.OwnerStaffUser)
            .HasForeignKey(x => x.OwnerStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.InvigilatedPlacementTests)
            .WithOne(x => x.InvigilatorUser)
            .HasForeignKey(x => x.InvigilatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CreatedLeadActivities)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.UploadedMediaAssets)
            .WithOne(x => x.UploaderUser)
            .HasForeignKey(x => x.UploaderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.IssuedInvoices)
            .WithOne(x => x.IssuedByUser)
            .HasForeignKey(x => x.IssuedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ConfirmedPayments)
            .WithOne(x => x.ConfirmedByUser)
            .HasForeignKey(x => x.ConfirmedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CreatedCashbookEntries)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Contracts)
            .WithOne(x => x.StaffUser)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ShiftAttendances)
            .WithOne(x => x.StaffUser)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ApprovedShiftAttendances)
            .WithOne(x => x.ApprovedByUser)
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SessionRoles)
            .WithOne(x => x.StaffUser)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ApprovedPayrollRuns)
            .WithOne(x => x.ApprovedByUser)
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PayrollLines)
            .WithOne(x => x.StaffUser)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PayrollPayments)
            .WithOne(x => x.StaffUser)
            .HasForeignKey(x => x.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ReceivedNotifications)
            .WithOne(x => x.RecipientUser)
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OpenedTickets)
            .WithOne(x => x.OpenedByUser)
            .HasForeignKey(x => x.OpenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AssignedTickets)
            .WithOne(x => x.AssignedToUser)
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TicketComments)
            .WithOne(x => x.CommenterUser)
            .HasForeignKey(x => x.CommenterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AuditLogs)
            .WithOne(x => x.ActorUser)
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}