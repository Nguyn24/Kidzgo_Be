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

        // 3. If registration already has EntryType = Immediate (already enrolled in class),
        // cannot change back to Wait
        var newEntryType = command.EntryType?.ToLowerInvariant() switch
        {
            "makeup" => EntryType.Makeup,
            "wait" => EntryType.Wait,
            _ => EntryType.Immediate
        };

        if (registration.EntryType != null &&
            registration.EntryType != EntryType.Wait &&
            newEntryType == EntryType.Wait)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.InvalidStatus(
                    $"Cannot change from EntryType '{registration.EntryType}' back to 'Wait'. Student is already enrolled in a class.",
                    "assign-class"));
        }

        // 4. Get class (only if not wait type - wait means no class yet)
        var isWait = command.EntryType?.ToLowerInvariant() == "wait";
        var classId = command.ClassId;
        
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

        // 5. Validate class matches program (for non-wait types)
        if (classEntity != null && classEntity.ProgramId != registration.ProgramId)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.ClassNotMatchingProgram(classEntity.Id, registration.ProgramId));
        }

        // 6. Check class status - can only assign to active/recruiting classes (for non-wait types)
        if (classEntity != null)
        {
            if (classEntity.Status == ClassStatus.Completed || 
                classEntity.Status == ClassStatus.Cancelled ||
                classEntity.Status == ClassStatus.Suspended)
            {
                return Result.Failure<AssignClassResponse>(
                    Error.Validation("ClassNotAvailable", $"Class is {classEntity.Status} and cannot accept new students"));
            }

            // 7. Check capacity
            if (classEntity.ClassEnrollments.Count >= classEntity.Capacity)
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
        string warningMessage = null;
        var entryType = newEntryType;

        // Determine new status based on entry type
        var newStatus = entryType switch
        {
            EntryType.Immediate => RegistrationStatus.Studying,      // Vào học ngay
            EntryType.Makeup => RegistrationStatus.ClassAssigned,     // Đã xếp lớp nhưng cần học bổ trước
            EntryType.Wait => RegistrationStatus.WaitingForClass,    // Chờ lớp mới
            _ => RegistrationStatus.Studying
        };

        // For immediate and makeup, create enrollment
        if (entryType == EntryType.Immediate || entryType == EntryType.Makeup)
        {
            var enrollment = new ClassEnrollment
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity!.Id,
                StudentProfileId = registration.StudentProfileId,
                EnrollDate = DateOnly.FromDateTime(now),
                Status = EnrollmentStatus.Active,
                TuitionPlanId = registration.TuitionPlanId,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.ClassEnrollments.Add(enrollment);

            // Add warning for mid-course entry
            if (classEntity?.Status == ClassStatus.Active)
            {
                warningMessage = entryType == EntryType.Makeup
                    ? "Học viên sẽ tham gia lớp sau khi hoàn thành buổi học bù."
                    : "Lớp đã bắt đầu. Học viên sẽ tham gia giữa chừng.";
            }

            // Auto set class to Full when capacity is reached
            var newEnrollmentCount = classEntity!.ClassEnrollments.Count + 1;
            if (newEnrollmentCount >= classEntity.Capacity && classEntity.Status != ClassStatus.Full)
            {
                classEntity.Status = ClassStatus.Full;
                classEntity.UpdatedAt = now;
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
        registration.ClassId = entryType == EntryType.Wait ? null : classEntity?.Id;
        registration.ClassAssignedDate = entryType == EntryType.Wait ? null : now;
        registration.EntryType = entryType;
        registration.Status = newStatus;
        registration.ActualStartDate = entryType == EntryType.Immediate ? now : null;
        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new AssignClassResponse
        {
            RegistrationId = registration.Id,
            RegistrationStatus = registration.Status.ToString(),
            ClassId = classEntity?.Id ?? Guid.Empty,
            ClassCode = classEntity?.Code,
            ClassTitle = classEntity?.Title,
            EntryType = entryType.ToString(),
            ClassAssignedDate = registration.ClassAssignedDate ?? now,
            WarningMessage = warningMessage
        };
    }
}
