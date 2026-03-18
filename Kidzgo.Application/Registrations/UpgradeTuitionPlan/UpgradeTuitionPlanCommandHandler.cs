using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.UpgradeTuitionPlan.Handler;

public sealed class UpgradeTuitionPlanCommandHandler(
    IDbContext context
) : ICommandHandler<UpgradeTuitionPlanCommand, UpgradeTuitionPlanResponse>
{
    public async Task<Result<UpgradeTuitionPlanResponse>> Handle(
        UpgradeTuitionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // 1. Get current registration
        var registration = await context.Registrations
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(RegistrationErrors.NotFound(command.RegistrationId));
        }

        // 2. Validate registration is active (studying or waiting for class)
        if (registration.Status != RegistrationStatus.Studying && 
            registration.Status != RegistrationStatus.ClassAssigned &&
            registration.Status != RegistrationStatus.WaitingForClass)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                RegistrationErrors.NoActiveRegistrationForUpgrade(registration.StudentProfileId));
        }

        // 3. Get new tuition plan
        var newTuitionPlan = await context.TuitionPlans.FindAsync(
            new object[] { command.NewTuitionPlanId }, 
            cancellationToken);

        if (newTuitionPlan == null)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                RegistrationErrors.TuitionPlanNotFound(command.NewTuitionPlanId));
        }

        // 4. Validate new tuition plan belongs to same program
        if (newTuitionPlan.ProgramId != registration.ProgramId)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                Error.Validation("DifferentProgram", "New tuition plan must belong to the same program"));
        }

        // 5. Validate new tuition plan is different
        if (newTuitionPlan.Id == registration.TuitionPlanId)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                RegistrationErrors.InvalidUpgradeTuitionPlan());
        }

        // 6. Mark original registration as completed
        var oldTotalSessions = registration.TotalSessions;
        var oldTuitionPlanName = registration.TuitionPlan.Name;
        registration.Status = RegistrationStatus.Completed;
        registration.UpdatedAt = now;

        // 7. Create new registration with upgraded tuition plan
        var newRegistration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            TuitionPlanId = newTuitionPlan.Id,
            RegistrationDate = now,
            ExpectedStartDate = now, // Start immediately after upgrade
            PreferredSchedule = registration.PreferredSchedule,
            Note = $"Nâng cấp từ gói {oldTuitionPlanName}",
            Status = registration.ClassId != null ? RegistrationStatus.Studying : RegistrationStatus.WaitingForClass,
            ClassId = registration.ClassId,
            ClassAssignedDate = registration.ClassAssignedDate,
            EntryType = registration.EntryType,
            OriginalRegistrationId = registration.Id,
            OperationType = OperationType.Upgrade,
            TotalSessions = newTuitionPlan.TotalSessions,
            UsedSessions = 0, // Reset used sessions for new package
            RemainingSessions = newTuitionPlan.TotalSessions,
            ExpiryDate = now.AddMonths(GetDurationMonths(newTuitionPlan.TotalSessions, registration.TotalSessions)),
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Registrations.Add(newRegistration);

        // 8. If student has class, also update the ClassEnrollment tuition plan reference
        if (registration.ClassId != null)
        {
            var enrollment = await context.ClassEnrollments
                .FirstOrDefaultAsync(ce => ce.ClassId == registration.ClassId 
                    && ce.StudentProfileId == registration.StudentProfileId
                    && ce.Status == Domain.Classes.EnrollmentStatus.Active,
                    cancellationToken);

            if (enrollment != null)
            {
                enrollment.TuitionPlanId = newTuitionPlan.Id;
                enrollment.UpdatedAt = now;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpgradeTuitionPlanResponse
        {
            OriginalRegistrationId = registration.Id,
            NewRegistrationId = newRegistration.Id,
            OldTuitionPlanName = oldTuitionPlanName,
            NewTuitionPlanName = newTuitionPlan.Name,
            OldTotalSessions = oldTotalSessions,
            NewTotalSessions = newTuitionPlan.TotalSessions,
            AddedSessions = newTuitionPlan.TotalSessions - oldTotalSessions,
            Status = newRegistration.Status.ToString()
        };
    }

    private int GetDurationMonths(int newTotalSessions, int oldTotalSessions)
    {
        // Estimate months based on sessions (assuming ~4 sessions per month)
        var newMonths = newTotalSessions / 4;
        if (newTotalSessions % 4 > 0) newMonths++;
        
        var oldMonths = oldTotalSessions / 4;
        if (oldTotalSessions % 4 > 0) oldMonths++;

        return Math.Max(1, newMonths - oldMonths);
    }
}
