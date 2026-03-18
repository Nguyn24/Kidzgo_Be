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
        var now = DateTime.UtcNow;

        var registration = await context.Registrations
            .Include(r => r.TuitionPlan)
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

        // Update fields
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

        // TuitionPlan can only be changed if student is not yet in a class
        if (command.TuitionPlanId.HasValue && registration.ClassId == null)
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
            UpdatedAt = now
        };
    }
}
