using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.ToggleTuitionPlanStatus;

public sealed class ToggleTuitionPlanStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ToggleTuitionPlanStatusCommand, ToggleTuitionPlanStatusResponse>
{
    public async Task<Result<ToggleTuitionPlanStatusResponse>> Handle(ToggleTuitionPlanStatusCommand command, CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<ToggleTuitionPlanStatusResponse>(
                Error.NotFound("TuitionPlan.NotFound", "Tuition Plan not found"));
        }

        tuitionPlan.IsActive = !tuitionPlan.IsActive;
        tuitionPlan.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new ToggleTuitionPlanStatusResponse
        {
            Id = tuitionPlan.Id,
            IsActive = tuitionPlan.IsActive
        };
    }
}

