using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.UpdateRegistration.Handler;

public sealed class UpdateRegistrationCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateRegistrationCommand, UpdateRegistrationResponse>
{
    public async Task<Result<UpdateRegistrationResponse>> Handle(
        UpdateRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();

        var registration = await context.Registrations
            .Include(r => r.TuitionPlan)
            .Include(r => r.SecondaryProgram)
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<UpdateRegistrationResponse>(RegistrationErrors.NotFound(command.Id));
        }

        // Can only update if not completed or cancelled
        if (registration.Status == RegistrationStatus.Completed || 
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<UpdateRegistrationResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "update"));
        }

        if (command.ExpectedStartDate.HasValue)
        {
            registration.ExpectedStartDate = command.ExpectedStartDate;
        }

        if (command.PreferredSchedule != null)
        {
            registration.PreferredSchedule = command.PreferredSchedule;
        }

        if (command.Note != null)
        {
            registration.Note = command.Note;
        }

        if (command.RemoveSecondaryProgram)
        {
            if (registration.SecondaryClassId.HasValue)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("Registration.SecondaryClassAssigned", "Cannot remove the secondary program while a secondary class is assigned"));
            }

            registration.SecondaryProgramId = null;
            registration.SecondaryProgramSkillFocus = null;
            registration.SecondaryClassAssignedDate = null;
            registration.SecondaryEntryType = null;
            registration.SecondaryProgram = null;
        }

        if (command.SecondaryProgramId == registration.ProgramId)
        {
            return Result.Failure<UpdateRegistrationResponse>(
                Error.Validation("Registration.SecondaryProgramDuplicated", "Secondary program must be different from primary program"));
        }

        if (command.SecondaryProgramId.HasValue)
        {
            var secondaryProgram = await context.Programs
                .FirstOrDefaultAsync(
                    p => p.Id == command.SecondaryProgramId.Value && p.IsActive && !p.IsDeleted,
                    cancellationToken);

            if (secondaryProgram is null)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    RegistrationErrors.ProgramNotFound(command.SecondaryProgramId.Value));
            }

            if (secondaryProgram.IsSupplementary)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    RegistrationErrors.SecondarySupplementaryRequiresSeparateRegistration(command.SecondaryProgramId.Value));
            }

            if (registration.SecondaryClassId.HasValue &&
                registration.SecondaryProgramId != command.SecondaryProgramId.Value)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("Registration.SecondaryClassAssigned", "Cannot change the secondary program while a secondary class is assigned"));
            }

            var hasConflict = await context.Registrations.AnyAsync(
                r => r.Id != registration.Id &&
                     r.StudentProfileId == registration.StudentProfileId &&
                     r.Status != RegistrationStatus.Completed &&
                     r.Status != RegistrationStatus.Cancelled &&
                     (r.ProgramId == command.SecondaryProgramId.Value ||
                      r.SecondaryProgramId == command.SecondaryProgramId.Value),
                cancellationToken);

            if (hasConflict)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    RegistrationErrors.AlreadyExists(registration.StudentProfileId, command.SecondaryProgramId.Value));
            }

            registration.SecondaryProgramId = secondaryProgram.Id;
            registration.SecondaryProgram = secondaryProgram;
            registration.SecondaryEntryType = null;
            registration.SecondaryClassAssignedDate = null;
        }

        if (command.SecondaryProgramSkillFocus is not null)
        {
            registration.SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryProgramSkillFocus)
                ? null
                : command.SecondaryProgramSkillFocus.Trim();
        }

        if (command.SecondaryProgramId is null && command.SecondaryProgramSkillFocus is not null && registration.SecondaryProgramId is null)
        {
            return Result.Failure<UpdateRegistrationResponse>(
                Error.Validation("Registration.SecondaryProgramMissing", "Secondary program skill focus can only be set when a secondary program exists"));
        }

        if (command.TuitionPlanId.HasValue && (registration.ClassId != null || registration.SecondaryClassId != null))
        {
            return Result.Failure<UpdateRegistrationResponse>(
                Error.Validation("Registration.ClassAlreadyAssigned", "Tuition plan cannot be changed after any class has been assigned"));
        }

        if (command.TuitionPlanId.HasValue)
        {
            var tuitionPlan = await context.TuitionPlans.FindAsync(
                new object[] { command.TuitionPlanId.Value }, 
                cancellationToken);

            if (tuitionPlan == null)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    RegistrationErrors.TuitionPlanNotFound(command.TuitionPlanId.Value));
            }

            // Validate same program
            if (tuitionPlan.ProgramId != registration.ProgramId)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("DifferentProgram", "Tuition plan must belong to the same program"));
            }

            registration.TuitionPlanId = command.TuitionPlanId.Value;
            registration.TuitionPlan = tuitionPlan;
            registration.TotalSessions = tuitionPlan.TotalSessions;
            registration.RemainingSessions = tuitionPlan.TotalSessions - registration.UsedSessions;
        }

        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateRegistrationResponse
        {
            Id = registration.Id,
            ExpectedStartDate = registration.ExpectedStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = registration.TuitionPlan?.Name,
            SecondaryProgramId = registration.SecondaryProgramId,
            SecondaryProgramName = registration.SecondaryProgram?.Name,
            SecondaryProgramSkillFocus = registration.SecondaryProgramSkillFocus,
            UpdatedAt = now
        };
    }
}
