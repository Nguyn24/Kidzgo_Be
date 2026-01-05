using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.DeleteTuitionPlan;

public sealed class DeleteTuitionPlanCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteTuitionPlanCommand>
{
    public async Task<Result> Handle(DeleteTuitionPlanCommand command, CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure(
                Error.NotFound("TuitionPlan.NotFound", "Tuition Plan not found"));
        }

        // Check if tuition plan is being used by any active enrollments
        bool hasActiveEnrollments = await context.ClassEnrollments
            .AnyAsync(e => e.TuitionPlanId == tuitionPlan.Id && 
                          (e.Status == Domain.Classes.EnrollmentStatus.Active || 
                           e.Status == Domain.Classes.EnrollmentStatus.Paused), 
                      cancellationToken);

        if (hasActiveEnrollments)
        {
            return Result.Failure(
                Error.Conflict("TuitionPlan.HasActiveEnrollments", "Cannot delete tuition plan with active enrollments"));
        }

        // Soft delete
        tuitionPlan.IsDeleted = true;
        tuitionPlan.IsActive = false; // Deactivate when soft deleting
        tuitionPlan.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

