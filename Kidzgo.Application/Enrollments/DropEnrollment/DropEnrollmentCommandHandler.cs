using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.DropEnrollment;

public sealed class DropEnrollmentCommandHandler(
    IDbContext context
) : ICommandHandler<DropEnrollmentCommand, DropEnrollmentResponse>
{
    public async Task<Result<DropEnrollmentResponse>> Handle(DropEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<DropEnrollmentResponse>(
                Error.NotFound("Enrollment.NotFound", "Enrollment not found"));
        }

        if (enrollment.Status == EnrollmentStatus.Dropped)
        {
            return Result.Failure<DropEnrollmentResponse>(
                Error.Conflict("Enrollment.AlreadyDropped", "Enrollment is already dropped"));
        }

        enrollment.Status = EnrollmentStatus.Dropped;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new DropEnrollmentResponse
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

