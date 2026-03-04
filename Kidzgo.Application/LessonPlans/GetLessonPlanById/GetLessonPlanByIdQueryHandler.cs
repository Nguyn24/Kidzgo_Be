using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.GetLessonPlanById;

public sealed class GetLessonPlanByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetLessonPlanByIdQuery, GetLessonPlanByIdResponse>
{
    public async Task<Result<GetLessonPlanByIdResponse>> Handle(
        GetLessonPlanByIdQuery query,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Class)
            .Include(lp => lp.Session)
            .Include(lp => lp.Template)
            .Include(lp => lp.SubmittedByUser)
            .FirstOrDefaultAsync(lp => lp.Id == query.Id && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<GetLessonPlanByIdResponse>(
                LessonPlanErrors.NotFound(query.Id));
        }

        return new GetLessonPlanByIdResponse
        {
            Id = lessonPlan.Id,
            ClassId = lessonPlan.ClassId,
            ClassCode = lessonPlan.Class?.Code,
            SessionId = lessonPlan.SessionId,
            SessionTitle = lessonPlan.Session != null
                ? $"Session {lessonPlan.Session.PlannedDatetime:dd/MM/yyyy HH:mm}"
                : null,
            SessionDate = lessonPlan.Session?.PlannedDatetime,
            TemplateId = lessonPlan.TemplateId,
            TemplateLevel = lessonPlan.Template?.Level,
            TemplateSessionIndex = lessonPlan.Template?.SessionIndex,
            PlannedContent = lessonPlan.PlannedContent,
            ActualContent = lessonPlan.ActualContent,
            ActualHomework = lessonPlan.ActualHomework,
            TeacherNotes = lessonPlan.TeacherNotes,
            SubmittedBy = lessonPlan.SubmittedBy,
            SubmittedByName = lessonPlan.SubmittedByUser?.Name,
            SubmittedAt = lessonPlan.SubmittedAt,
        };
    }
}

