using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.CreateEnrollment;

public sealed class CreateEnrollmentCommandHandler(
    IDbContext context
) : ICommandHandler<CreateEnrollmentCommand, CreateEnrollmentResponse>
{
    public async Task<Result<CreateEnrollmentResponse>> Handle(CreateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        // Check if class exists and is active
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                Error.NotFound("Enrollment.ClassNotFound", "Class not found"));
        }

        if (classEntity.Status != ClassStatus.Active && classEntity.Status != ClassStatus.Planned)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                Error.Conflict("Enrollment.ClassNotAvailable", "Class is not available for enrollment"));
        }

        // Check if student profile exists and is a student
        var studentProfile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (studentProfile is null)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                Error.NotFound("Enrollment.StudentNotFound", "Student profile not found or is not a student"));
        }

        // Check if student is already enrolled in this class with Active status
        bool alreadyEnrolled = await context.ClassEnrollments
            .AnyAsync(ce => ce.ClassId == command.ClassId 
                && ce.StudentProfileId == command.StudentProfileId 
                && ce.Status == EnrollmentStatus.Active, cancellationToken);

        if (alreadyEnrolled)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                Error.Conflict("Enrollment.AlreadyEnrolled", "Student is already enrolled in this class"));
        }

        // Check class capacity
        int currentEnrollmentCount = classEntity.ClassEnrollments
            .Count(ce => ce.Status == EnrollmentStatus.Active);

        if (currentEnrollmentCount >= classEntity.Capacity)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                Error.Conflict("Enrollment.ClassFull", "Class has reached its capacity"));
        }

        // Check if tuition plan exists and is active (if provided)
        if (command.TuitionPlanId.HasValue)
        {
            var tuitionPlan = await context.TuitionPlans
                .FirstOrDefaultAsync(tp => tp.Id == command.TuitionPlanId.Value, cancellationToken);

            if (tuitionPlan is null)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    Error.NotFound("Enrollment.TuitionPlanNotFound", "Tuition plan not found"));
            }

            if (!tuitionPlan.IsActive || tuitionPlan.IsDeleted)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    Error.Conflict("Enrollment.TuitionPlanNotAvailable", "Tuition plan is not available"));
            }

            // Check if tuition plan belongs to the same program as the class
            if (tuitionPlan.ProgramId != classEntity.ProgramId)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    Error.Conflict("Enrollment.TuitionPlanProgramMismatch", "Tuition plan must belong to the same program as the class"));
            }
        }

        var now = DateTime.UtcNow;
        var enrollment = new ClassEnrollment
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            StudentProfileId = command.StudentProfileId,
            EnrollDate = command.EnrollDate,
            Status = EnrollmentStatus.Active,
            TuitionPlanId = command.TuitionPlanId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ClassEnrollments.Add(enrollment);
        await context.SaveChangesAsync(cancellationToken);

        // Query enrollment with navigation properties for response
        var enrollmentWithNav = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == enrollment.Id, cancellationToken);

        return new CreateEnrollmentResponse
        {
            Id = enrollmentWithNav!.Id,
            ClassId = enrollmentWithNav.ClassId,
            ClassCode = enrollmentWithNav.Class.Code,
            ClassTitle = enrollmentWithNav.Class.Title,
            StudentProfileId = enrollmentWithNav.StudentProfileId,
            StudentName = enrollmentWithNav.StudentProfile.DisplayName,
            EnrollDate = enrollmentWithNav.EnrollDate,
            Status = enrollmentWithNav.Status,
            TuitionPlanId = enrollmentWithNav.TuitionPlanId,
            TuitionPlanName = enrollmentWithNav.TuitionPlan?.Name
        };
    }
}

