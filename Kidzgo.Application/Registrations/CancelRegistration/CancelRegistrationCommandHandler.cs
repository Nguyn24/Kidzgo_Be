using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.CancelRegistration.Handler;

public sealed class CancelRegistrationCommandHandler(
    IDbContext context
) : ICommandHandler<CancelRegistrationCommand, CancelRegistrationResponse>
{
    public async Task<Result<CancelRegistrationResponse>> Handle(
        CancelRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var registration = await context.Registrations
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<CancelRegistrationResponse>(RegistrationErrors.NotFound(command.Id));
        }

        // Validate status - cannot cancel if already completed or cancelled
        if (registration.Status == RegistrationStatus.Completed || 
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<CancelRegistrationResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "cancel"));
        }

        // If student is studying, need to drop from class first
        if (registration.Status == RegistrationStatus.Studying && registration.ClassId != null)
        {
            // Drop the enrollment
            var enrollment = await context.ClassEnrollments
                .FirstOrDefaultAsync(ce => ce.ClassId == registration.ClassId 
                    && ce.StudentProfileId == registration.StudentProfileId
                    && ce.Status == Domain.Classes.EnrollmentStatus.Active,
                    cancellationToken);

            if (enrollment != null)
            {
                enrollment.Status = Domain.Classes.EnrollmentStatus.Dropped;
                enrollment.UpdatedAt = now;
            }
        }

        // Update registration status
        registration.Status = RegistrationStatus.Cancelled;
        registration.Note = string.IsNullOrEmpty(registration.Note) 
            ? command.Reason 
            : $"{registration.Note} | Cancel reason: {command.Reason}";
        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new CancelRegistrationResponse
        {
            Id = registration.Id,
            Status = registration.Status.ToString(),
            Reason = command.Reason,
            CancelledAt = now
        };
    }
}
