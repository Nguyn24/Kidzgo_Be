using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.UpdateEnrollment;

public sealed class UpdateEnrollmentCommandHandler(
    IDbContext context
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
                Error.NotFound("Enrollment.NotFound", "Enrollment not found"));
        }

        // Update EnrollDate if provided
        if (command.EnrollDate.HasValue)
        {
            enrollment.EnrollDate = command.EnrollDate.Value;
        }

        // Update TuitionPlan if provided
        if (command.TuitionPlanId.HasValue)
        {
            var tuitionPlan = await context.TuitionPlans
                .FirstOrDefaultAsync(tp => tp.Id == command.TuitionPlanId.Value, cancellationToken);

            if (tuitionPlan is null)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    Error.NotFound("Enrollment.TuitionPlanNotFound", "Tuition plan not found"));
            }

            if (!tuitionPlan.IsActive || tuitionPlan.IsDeleted)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    Error.Conflict("Enrollment.TuitionPlanNotAvailable", "Tuition plan is not available"));
            }

            // Check if tuition plan belongs to the same program as the class
            if (tuitionPlan.ProgramId != enrollment.Class.ProgramId)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    Error.Conflict("Enrollment.TuitionPlanProgramMismatch", "Tuition plan must belong to the same program as the class"));
            }

            enrollment.TuitionPlanId = command.TuitionPlanId.Value;
        }

        enrollment.UpdatedAt = DateTime.UtcNow;
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

