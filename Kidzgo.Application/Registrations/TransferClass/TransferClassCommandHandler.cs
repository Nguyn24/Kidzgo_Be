using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.TransferClass.Handler;

public sealed class TransferClassCommandHandler(
    IDbContext context
) : ICommandHandler<TransferClassCommand, TransferClassResponse>
{
    public async Task<Result<TransferClassResponse>> Handle(
        TransferClassCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // 1. Get registration with class info
        var registration = await context.Registrations
            .Include(r => r.Class)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.NotFound(command.RegistrationId));
        }

        // 2. Validate registration is studying
        if (registration.Status != RegistrationStatus.Studying)
        {
            return Result.Failure<TransferClassResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "transfer-class"));
        }

        // 3. Get old class info
        if (registration.ClassId == null)
        {
            return Result.Failure<TransferClassResponse>(
                Error.Validation("NoClassAssigned", "Registration has no class assigned"));
        }

        var oldClassId = registration.ClassId.Value;
        var oldClass = await context.Classes.FindAsync(new object[] { oldClassId }, cancellationToken);

        // 4. Get new class
        var newClass = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == command.NewClassId, cancellationToken);

        if (newClass == null)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.ClassNotFound(command.NewClassId));
        }

        // 5. Validate new class matches program
        if (newClass.ProgramId != registration.ProgramId)
        {
            return Result.Failure<TransferClassResponse>(
                RegistrationErrors.ClassNotMatchingProgram(command.NewClassId, registration.ProgramId));
        }

        // 6. Check new class capacity
        if (newClass.ClassEnrollments.Count >= newClass.Capacity)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.ClassFull(command.NewClassId));
        }

        // 7. Check new class status
        if (newClass.Status != ClassStatus.Active && newClass.Status != ClassStatus.Recruiting)
        {
            return Result.Failure<TransferClassResponse>(
                Error.Validation("ClassNotAvailable", $"Cannot transfer to class with status {newClass.Status}"));
        }

        // 8. Check if same class
        if (oldClassId == command.NewClassId)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.CannotTransferToSameClass());
        }

        // 9. Update old enrollment to dropped
        var oldEnrollment = await context.ClassEnrollments
            .FirstOrDefaultAsync(ce => ce.ClassId == oldClassId 
                && ce.StudentProfileId == registration.StudentProfileId 
                && ce.Status == EnrollmentStatus.Active, 
                cancellationToken);

        if (oldEnrollment != null)
        {
            oldEnrollment.Status = EnrollmentStatus.Dropped;
            oldEnrollment.UpdatedAt = now;
        }

        // 10. Create new enrollment
        var newEnrollment = new ClassEnrollment
        {
            Id = Guid.NewGuid(),
            ClassId = command.NewClassId,
            StudentProfileId = registration.StudentProfileId,
            EnrollDate = DateOnly.FromDateTime(command.EffectiveDate),
            Status = EnrollmentStatus.Active,
            TuitionPlanId = registration.TuitionPlanId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ClassEnrollments.Add(newEnrollment);

        // 11. Update registration
        registration.ClassId = command.NewClassId;
        registration.ClassAssignedDate = now;
        registration.OperationType = OperationType.Transfer;
        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new TransferClassResponse
        {
            RegistrationId = registration.Id,
            OldClassId = oldClassId,
            OldClassName = oldClass?.Title ?? "Unknown",
            NewClassId = newClass.Id,
            NewClassName = newClass.Title,
            EffectiveDate = command.EffectiveDate,
            Status = registration.Status.ToString()
        };
    }
}
