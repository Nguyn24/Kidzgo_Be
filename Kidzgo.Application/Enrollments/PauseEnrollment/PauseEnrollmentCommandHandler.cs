using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.PauseEnrollment;

public sealed class PauseEnrollmentCommandHandler(
    IDbContext context
) : ICommandHandler<PauseEnrollmentCommand, PauseEnrollmentResponse>
{
    public async Task<Result<PauseEnrollmentResponse>> Handle(PauseEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<PauseEnrollmentResponse>(
                Error.NotFound("Enrollment.NotFound", "Enrollment not found"));
        }

        if (enrollment.Status != EnrollmentStatus.Active)
        {
            return Result.Failure<PauseEnrollmentResponse>(
                Error.Conflict("Enrollment.InvalidStatus", "Only active enrollments can be paused"));
        }

        enrollment.Status = EnrollmentStatus.Paused;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new PauseEnrollmentResponse
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

