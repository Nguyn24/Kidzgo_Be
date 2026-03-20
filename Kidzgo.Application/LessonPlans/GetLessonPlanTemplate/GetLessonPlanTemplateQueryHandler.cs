using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.GetLessonPlanTemplate;

public sealed class GetLessonPlanTemplateQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetLessonPlanTemplateQuery, GetLessonPlanTemplateResponse>
{
    public async Task<Result<GetLessonPlanTemplateResponse>> Handle(
        GetLessonPlanTemplateQuery query,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Template)
            .Include(lp => lp.Session)
            .FirstOrDefaultAsync(lp => lp.Id == query.LessonPlanId && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<GetLessonPlanTemplateResponse>(
                LessonPlanErrors.NotFound(query.LessonPlanId));
        }

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetLessonPlanTemplateResponse>(LessonPlanErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher &&
            (lessonPlan.Session is null ||
             (lessonPlan.Session.PlannedTeacherId != currentUser.Id &&
              lessonPlan.Session.ActualTeacherId != currentUser.Id)))
        {
            return Result.Failure<GetLessonPlanTemplateResponse>(LessonPlanErrors.Unauthorized);
        }

        return new GetLessonPlanTemplateResponse
        {
            LessonPlanId = lessonPlan.Id,
            TemplateId = lessonPlan.TemplateId,
            TemplateLevel = lessonPlan.Template?.Level,
            TemplateSessionIndex = lessonPlan.Template?.SessionIndex,
            TemplateStructureJson = lessonPlan.Template?.AttachmentUrl,
            PlannedContent = lessonPlan.PlannedContent,
            IsReadOnly = true
        };
    }
}
