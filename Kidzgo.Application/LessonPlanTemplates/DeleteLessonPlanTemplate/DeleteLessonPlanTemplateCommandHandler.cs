using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.DeleteLessonPlanTemplate;

public sealed class DeleteLessonPlanTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<DeleteLessonPlanTemplateCommand>
{
    public async Task<Result> Handle(
        DeleteLessonPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure(LessonPlanTemplateErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher)
        {
            return Result.Failure(LessonPlanTemplateErrors.Unauthorized);
        }

        var template = await context.LessonPlanTemplates
            .Include(t => t.LessonPlans)
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (template is null)
        {
            return Result.Failure(LessonPlanTemplateErrors.NotFound(command.Id));
        }

        // Check if template has active lesson plans
        var hasActiveLessonPlans = template.LessonPlans.Any(lp => !lp.IsDeleted);

        if (hasActiveLessonPlans)
        {
            return Result.Failure(LessonPlanTemplateErrors.HasActiveLessonPlans);
        }

        // Soft delete
        template.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
