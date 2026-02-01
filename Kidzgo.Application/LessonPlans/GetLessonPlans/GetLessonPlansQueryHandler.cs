using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.GetLessonPlans;

public sealed class GetLessonPlansQueryHandler(
    IDbContext context
) : IQueryHandler<GetLessonPlansQuery, GetLessonPlansResponse>
{
    public async Task<Result<GetLessonPlansResponse>> Handle(
        GetLessonPlansQuery query,
        CancellationToken cancellationToken)
    {
        var lessonPlanQuery = context.LessonPlans
            .Include(lp => lp.Session)
                .ThenInclude(s => s.Class)
            .Include(lp => lp.Template)
            .Include(lp => lp.SubmittedByUser)
            .AsQueryable();

        // Filter by IsDeleted
        if (!query.IncludeDeleted)
        {
            lessonPlanQuery = lessonPlanQuery.Where(lp => !lp.IsDeleted);
        }

        // Filter by session
        if (query.SessionId.HasValue)
        {
            lessonPlanQuery = lessonPlanQuery.Where(lp => lp.SessionId == query.SessionId.Value);
        }

        // Filter by class
        if (query.ClassId.HasValue)
        {
            lessonPlanQuery = lessonPlanQuery.Where(lp => lp.Session.ClassId == query.ClassId.Value);
        }

        // Filter by template
        if (query.TemplateId.HasValue)
        {
            lessonPlanQuery = lessonPlanQuery.Where(lp => lp.TemplateId == query.TemplateId.Value);
        }

        // Filter by submitted by
        if (query.SubmittedBy.HasValue)
        {
            lessonPlanQuery = lessonPlanQuery.Where(lp => lp.SubmittedBy == query.SubmittedBy.Value);
        }

        // Filter by date range (session date)
        if (query.FromDate.HasValue)
        {
            lessonPlanQuery = lessonPlanQuery.Where(lp => 
                lp.Session != null && lp.Session.PlannedDatetime >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            lessonPlanQuery = lessonPlanQuery.Where(lp => 
                lp.Session != null && lp.Session.PlannedDatetime <= query.ToDate.Value);
        }

        // Get total count
        int totalCount = await lessonPlanQuery.CountAsync(cancellationToken);

        // Apply pagination and select
        var lessonPlans = await lessonPlanQuery
            .OrderByDescending(lp => lp.Session != null ? lp.Session.PlannedDatetime : DateTime.MinValue)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(lp => new LessonPlanDto
            {
                Id = lp.Id,
                SessionId = lp.SessionId,
                SessionTitle = lp.Session != null
                    ? $"Session {lp.Session.PlannedDatetime:dd/MM/yyyy HH:mm}"
                    : null,
                SessionDate = lp.Session != null ? lp.Session.PlannedDatetime : null,
                ClassId = lp.Session != null ? lp.Session.ClassId : null,
                ClassCode = lp.Session != null && lp.Session.Class != null ? lp.Session.Class.Code : null,
                TemplateId = lp.TemplateId,
                TemplateLevel = lp.Template != null ? lp.Template.Level : null,
                TemplateSessionIndex = lp.Template != null ? lp.Template.SessionIndex : null,
                PlannedContent = lp.PlannedContent,
                ActualContent = lp.ActualContent,
                ActualHomework = lp.ActualHomework,
                SubmittedBy = lp.SubmittedBy,
                SubmittedByName = lp.SubmittedByUser != null ? lp.SubmittedByUser.Name : null,
                SubmittedAt = lp.SubmittedAt,
            })
            .ToListAsync(cancellationToken);

        var page = new Page<LessonPlanDto>(
            lessonPlans,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetLessonPlansResponse
        {
            LessonPlans = page
        };
    }
}

