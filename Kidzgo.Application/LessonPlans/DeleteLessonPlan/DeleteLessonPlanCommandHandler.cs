using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.DeleteLessonPlan;

public sealed class DeleteLessonPlanCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteLessonPlanCommand>
{
    public async Task<Result> Handle(
        DeleteLessonPlanCommand command,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .FirstOrDefaultAsync(lp => lp.Id == command.Id, cancellationToken);

        if (lessonPlan is null || lessonPlan.IsDeleted)
        {
            return Result.Failure(LessonPlanErrors.NotFound(command.Id));
        }

        // Soft delete
        lessonPlan.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}