using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.AssignClass.Handler;

public sealed class AssignClassCommandHandler(
    IDbContext context
) : ICommandHandler<AssignClassCommand, AssignClassResponse>
{
    public async Task<Result<AssignClassResponse>> Handle(
        AssignClassCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // 1. Get registration
        var registration = await context.Registrations
            .Include(r => r.Program)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<AssignClassResponse>(RegistrationErrors.NotFound(command.RegistrationId));
        }

        // 2. Validate registration status - can only assign if in valid state
        if (registration.Status == RegistrationStatus.Completed || 
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "assign-class"));
        }

        // 3. Get class
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity == null)
        {
            return Result.Failure<AssignClassResponse>(RegistrationErrors.ClassNotFound(command.ClassId));
        }

        // 4. Validate class matches program
        if (classEntity.ProgramId != registration.ProgramId)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.ClassNotMatchingProgram(command.ClassId, registration.ProgramId));
        }

        // 5. Check class status - can only assign to active/recruiting classes
        if (classEntity.Status == ClassStatus.Completed || 
            classEntity.Status == ClassStatus.Cancelled ||
            classEntity.Status == ClassStatus.Suspended)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation("ClassNotAvailable", $"Class is {classEntity.Status} and cannot accept new students"));
        }

        // 6. Check capacity
        if (classEntity.ClassEnrollments.Count >= classEntity.Capacity)
        {
            return Result.Failure<AssignClassResponse>(RegistrationErrors.ClassFull(command.ClassId));
        }

        // 7. Check if student is already enrolled in this class
        var alreadyEnrolled = await context.ClassEnrollments
            .AnyAsync(ce => ce.ClassId == command.ClassId && 
                ce.StudentProfileId == registration.StudentProfileId &&
                ce.Status != EnrollmentStatus.Dropped, 
                cancellationToken);

        if (alreadyEnrolled)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Conflict("AlreadyEnrolled", "Student is already enrolled in this class"));
        }

        // 8. Handle entry type
        string warningMessage = null;
        bool classAlreadyStarted = classEntity.Status == ClassStatus.Active;

        if (classAlreadyStarted && command.EntryType == "immediate")
        {
            // This is fine - student will join mid-course
            warningMessage = "Lớp đã bắt đầu. Học viên sẽ tham gia giữa chừng.";
        }
        else if (command.EntryType == "wait")
        {
            // Student wants to wait for next class - but we're assigning now
            warningMessage = "Học viên đang chờ lớp mới nhưng đã được xếp vào lớp hiện tại.";
        }

        // 9. Create ClassEnrollment
        var enrollment = new ClassEnrollment
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            StudentProfileId = registration.StudentProfileId,
            EnrollDate = DateOnly.FromDateTime(now),
            Status = EnrollmentStatus.Active,
            TuitionPlanId = registration.TuitionPlanId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ClassEnrollments.Add(enrollment);

        // 10. Update registration
        registration.ClassId = command.ClassId;
        registration.ClassAssignedDate = now;
        registration.EntryType = Enum.Parse<EntryType>(command.EntryType);
        registration.Status = RegistrationStatus.Studying;
        registration.ActualStartDate = now;
        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new AssignClassResponse
        {
            RegistrationId = registration.Id,
            RegistrationStatus = registration.Status.ToString(),
            ClassId = classEntity.Id,
            ClassCode = classEntity.Code,
            ClassTitle = classEntity.Title,
            EntryType = command.EntryType,
            ClassAssignedDate = now,
            WarningMessage = warningMessage
        };
    }
}
