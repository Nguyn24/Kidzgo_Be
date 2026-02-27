using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.GetLessonPlanTemplate;

public sealed class GetLessonPlanTemplateQueryHandler(
    IDbContext context
) : IQueryHandler<GetLessonPlanTemplateQuery, GetLessonPlanTemplateResponse>
{
    public async Task<Result<GetLessonPlanTemplateResponse>> Handle(
        GetLessonPlanTemplateQuery query,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Template)
            .FirstOrDefaultAsync(lp => lp.Id == query.LessonPlanId && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<GetLessonPlanTemplateResponse>(
                LessonPlanErrors.NotFound(query.LessonPlanId));
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