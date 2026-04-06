using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.AssignClass.Handler;

public sealed class AssignClassCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService
) : ICommandHandler<AssignClassCommand, AssignClassResponse>
{
    public async Task<Result<AssignClassResponse>> Handle(
        AssignClassCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var track = RegistrationTrackHelper.NormalizeTrack(command.Track);
        var isSecondaryTrack = track == RegistrationTrackHelper.SecondaryTrack;
        var entryType = RegistrationTrackHelper.ParseEntryType(command.EntryType);

        var registration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.SecondaryProgram)
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

        if (isSecondaryTrack && !registration.SecondaryProgramId.HasValue)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation("Registration.SecondaryProgramMissing", "Registration does not have a secondary program to assign"));
        }

        var currentEntryType = isSecondaryTrack ? registration.SecondaryEntryType : registration.EntryType;
        var currentClassId = isSecondaryTrack ? registration.SecondaryClassId : registration.ClassId;
        var targetProgramId = isSecondaryTrack ? registration.SecondaryProgramId!.Value : registration.ProgramId;

        if (currentEntryType != null &&
            currentEntryType != EntryType.Wait &&
            entryType == EntryType.Wait)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.InvalidStatus(
                    $"Cannot change track '{track}' back to 'Wait' after enrollment has been created.",
                    "assign-class"));
        }

        if (currentClassId.HasValue && entryType != EntryType.Wait)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation("Registration.ClassAlreadyAssigned", $"Track '{track}' already has a class assigned. Use transfer-class instead."));
        }

        var isWait = entryType == EntryType.Wait;
        var classId = command.ClassId;
        var currentActiveEnrollmentCount = 0;

        if (!isWait && !classId.HasValue)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation("Registration.ClassIdRequired", "ClassId is required when assigning a class"));
        }
        
        var classEntity = !isWait && classId.HasValue
            ? await context.Classes
                .Include(c => c.ClassEnrollments)
                .FirstOrDefaultAsync(c => c.Id == classId.Value, cancellationToken)
            : null;

        // 4. For non-wait types, validate class exists
        if (classEntity == null && !isWait)
        {
            return Result.Failure<AssignClassResponse>(RegistrationErrors.ClassNotFound(classId ?? Guid.Empty));
        }

        if (classEntity != null && classEntity.ProgramId != targetProgramId)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.ClassNotMatchingProgram(classEntity.Id, targetProgramId));
        }

        if (classEntity != null)
        {
            var selectionPatternValidation = studentSessionAssignmentService
                .ValidateSelectionPattern(classEntity, command.SessionSelectionPattern);
            if (selectionPatternValidation.IsFailure)
            {
                return Result.Failure<AssignClassResponse>(selectionPatternValidation.Error);
            }
        }

        // 6. Check class status - can only assign to active/recruiting classes (for non-wait types)
        if (classEntity != null)
        {
            currentActiveEnrollmentCount = classEntity.ClassEnrollments
                .Count(ce => ce.Status == EnrollmentStatus.Active);

            ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, currentActiveEnrollmentCount, now);

            if (classEntity.Status == ClassStatus.Completed || 
                classEntity.Status == ClassStatus.Cancelled ||
                classEntity.Status == ClassStatus.Suspended)
            {
                return Result.Failure<AssignClassResponse>(
                    Error.Validation("ClassNotAvailable", $"Class is {classEntity.Status} and cannot accept new students"));
            }

            // 7. Check capacity
            if (currentActiveEnrollmentCount >= classEntity.Capacity)
            {
                return Result.Failure<AssignClassResponse>(RegistrationErrors.ClassFull(classEntity.Id));
            }

            // 8. Check if student is already enrolled in this class
            var alreadyEnrolled = await context.ClassEnrollments
                .AnyAsync(ce => ce.ClassId == classEntity.Id && 
                    ce.StudentProfileId == registration.StudentProfileId &&
                    ce.Status != EnrollmentStatus.Dropped, 
                    cancellationToken);

            if (alreadyEnrolled)
            {
                return Result.Failure<AssignClassResponse>(
                    Error.Conflict("AlreadyEnrolled", "Student is already enrolled in this class"));
            }
        }

        // 9. Handle entry type logic
        string? warningMessage = null;

        // Determine new status based on entry type
        _ = entryType switch
        {
            EntryType.Immediate => RegistrationStatus.Studying,      // Vào học ngay
            EntryType.Makeup => RegistrationStatus.ClassAssigned,     // Đã xếp lớp nhưng cần học bổ trước
            EntryType.Wait => RegistrationStatus.WaitingForClass,    // Chờ lớp mới
            _ => RegistrationStatus.Studying
        };

        // For immediate and makeup, create enrollment
        if (entryType == EntryType.Immediate || entryType == EntryType.Makeup || entryType == EntryType.Retake)
        {
            var enrollment = new ClassEnrollment
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity!.Id,
                StudentProfileId = registration.StudentProfileId,
                EnrollDate = DateOnly.FromDateTime(now),
                Status = EnrollmentStatus.Active,
                TuitionPlanId = registration.TuitionPlanId,
                RegistrationId = registration.Id,
                Track = RegistrationTrackHelper.ToTrackType(track),
                SessionSelectionPattern = command.SessionSelectionPattern,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.ClassEnrollments.Add(enrollment);
            await studentSessionAssignmentService.SyncAssignmentsForEnrollmentAsync(enrollment, cancellationToken);

            // Add warning for mid-course entry
            if (classEntity!.Status == ClassStatus.Active)
            {
                warningMessage = entryType == EntryType.Makeup
                    ? "Học viên sẽ tham gia lớp sau khi hoàn thành buổi học bù."
                    : "Lớp đã bắt đầu. Học viên sẽ tham gia giữa chừng.";
            }

            // Auto set class to Full when capacity is reached
            var previousClassStatus = classEntity.Status;
            ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, currentActiveEnrollmentCount + 1, now);
            if (classEntity.Status == ClassStatus.Full && previousClassStatus != ClassStatus.Full)
            {
                warningMessage = string.IsNullOrEmpty(warningMessage) 
                    ? "Lớp đã đầy sau khi thêm học viên này."
                    : warningMessage + " Lớp đã đầy sau khi thêm học viên này.";
            }
        }
        // For wait type, don't create enrollment - just update registration status
        else if (entryType == EntryType.Wait)
        {
            warningMessage = "Học viên đã được thêm vào danh sách chờ lớp mới.";
        }

        // 10. Update registration
        if (isSecondaryTrack)
        {
            registration.SecondaryClassId = entryType == EntryType.Wait ? null : classEntity?.Id;
            registration.SecondaryClassAssignedDate = entryType == EntryType.Wait ? null : now;
            registration.SecondaryEntryType = entryType;
        }
        else
        {
            registration.ClassId = entryType == EntryType.Wait ? null : classEntity?.Id;
            registration.ClassAssignedDate = entryType == EntryType.Wait ? null : now;
            registration.EntryType = entryType;
        }

        registration.Status = RegistrationTrackHelper.ResolveStatus(registration);
        if (entryType == EntryType.Immediate && !registration.ActualStartDate.HasValue)
        {
            registration.ActualStartDate = now;
        }

        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new AssignClassResponse
        {
            RegistrationId = registration.Id,
            RegistrationStatus = registration.Status.ToString(),
            ClassId = classEntity?.Id ?? Guid.Empty,
            ClassCode = classEntity?.Code ?? string.Empty,
            ClassTitle = classEntity?.Title ?? string.Empty,
            Track = track,
            EntryType = entryType.ToString(),
            ClassAssignedDate = isSecondaryTrack
                ? registration.SecondaryClassAssignedDate ?? now
                : registration.ClassAssignedDate ?? now,
            WarningMessage = warningMessage
        };
    }
}
