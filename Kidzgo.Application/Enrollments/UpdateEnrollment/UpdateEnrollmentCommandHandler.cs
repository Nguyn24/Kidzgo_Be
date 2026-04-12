using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.UpdateEnrollment;

public sealed class UpdateEnrollmentCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService
) : ICommandHandler<UpdateEnrollmentCommand, UpdateEnrollmentResponse>
{
    public async Task<Result<UpdateEnrollmentResponse>> Handle(UpdateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<UpdateEnrollmentResponse>(
                EnrollmentErrors.NotFound(command.Id));
        }

        // Update EnrollDate if provided
        if (command.EnrollDate.HasValue)
        {
            enrollment.EnrollDate = command.EnrollDate.Value;
        }

        if (command.Track is not null)
        {
            enrollment.Track = RegistrationTrackHelper.ToTrackType(command.Track);
        }

        if (command.ClearSessionSelectionPattern)
        {
            enrollment.SessionSelectionPattern = null;
        }
        else if (command.SessionSelectionPattern is not null)
        {
            var selectionPatternValidation = studentSessionAssignmentService
                .ValidateSelectionPattern(enrollment.Class, command.SessionSelectionPattern);
            if (selectionPatternValidation.IsFailure)
            {
                return Result.Failure<UpdateEnrollmentResponse>(selectionPatternValidation.Error);
            }

            enrollment.SessionSelectionPattern = command.SessionSelectionPattern;
        }

        // Update TuitionPlan if provided
        if (command.TuitionPlanId.HasValue)
        {
            var tuitionPlan = await context.TuitionPlans
                .FirstOrDefaultAsync(tp => tp.Id == command.TuitionPlanId.Value, cancellationToken);

            if (tuitionPlan is null)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanNotFound);
            }

            if (!tuitionPlan.IsActive || tuitionPlan.IsDeleted)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanNotAvailable);
            }

            // Check if tuition plan belongs to the same program as the class
            if (tuitionPlan.ProgramId != enrollment.Class.ProgramId)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanProgramMismatch);
            }

            enrollment.TuitionPlanId = command.TuitionPlanId.Value;
        }

        var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
            enrollment.StudentProfileId,
            enrollment.ClassId,
            enrollment.EnrollDate,
            enrollment.SessionSelectionPattern,
            cancellationToken,
            excludeEnrollmentId: enrollment.Id,
            excludeLegacyClassId: enrollment.ClassId);
        if (conflictResult.IsFailure)
        {
            return Result.Failure<UpdateEnrollmentResponse>(conflictResult.Error);
        }

        enrollment.UpdatedAt = VietnamTime.UtcNow();
        await studentSessionAssignmentService.SyncAssignmentsForEnrollmentAsync(enrollment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Navigation properties are already loaded from the initial query
        return new UpdateEnrollmentResponse
        {
            Id = enrollment.Id,
            ClassId = enrollment.ClassId,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            StudentProfileId = enrollment.StudentProfileId,
            StudentName = enrollment.StudentProfile.DisplayName,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status,
            TuitionPlanId = enrollment.TuitionPlanId,
            TuitionPlanName = enrollment.TuitionPlan?.Name
        };
    }
}

