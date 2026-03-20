using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.DeleteLessonPlan;

public sealed class DeleteLessonPlanCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<DeleteLessonPlanCommand>
{
    public async Task<Result> Handle(
        DeleteLessonPlanCommand command,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Session)
            .FirstOrDefaultAsync(lp => lp.Id == command.Id, cancellationToken);

        if (lessonPlan is null || lessonPlan.IsDeleted)
        {
            return Result.Failure(LessonPlanErrors.NotFound(command.Id));
        }

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure(LessonPlanErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher &&
            (lessonPlan.Session is null ||
             (lessonPlan.Session.PlannedTeacherId != currentUser.Id &&
              lessonPlan.Session.ActualTeacherId != currentUser.Id)))
        {
            return Result.Failure(LessonPlanErrors.Unauthorized);
        }

        // Soft delete
        lessonPlan.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
