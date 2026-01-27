using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExams;

public sealed class GetExamsQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamsQuery, GetExamsResponse>
{
    public async Task<Result<GetExamsResponse>> Handle(GetExamsQuery query, CancellationToken cancellationToken)
    {
        var examsQuery = context.Exams
            .Include(e => e.Class)
            .Include(e => e.CreatedByUser)
            .AsQueryable();

        // Filter by classId
        if (query.ClassId.HasValue)
        {
            examsQuery = examsQuery.Where(e => e.ClassId == query.ClassId.Value);
        }

        // Filter by exam type
        if (query.ExamType.HasValue)
        {
            examsQuery = examsQuery.Where(e => e.ExamType == query.ExamType.Value);
        }

        // Get total count
        int totalCount = await examsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var exams = await examsQuery
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(e => new ExamDto
            {
                Id = e.Id,
                ClassId = e.ClassId,
                ClassCode = e.Class.Code,
                ClassTitle = e.Class.Title,
                ExamType = e.ExamType.ToString(),
                Date = e.Date,
                MaxScore = e.MaxScore,
                Description = e.Description,
                ScheduledStartTime = e.ScheduledStartTime,
                TimeLimitMinutes = e.TimeLimitMinutes,
                AllowLateStart = e.AllowLateStart,
                LateStartToleranceMinutes = e.LateStartToleranceMinutes,
                AutoSubmitOnTimeLimit = e.AutoSubmitOnTimeLimit,
                PreventCopyPaste = e.PreventCopyPaste,
                PreventNavigation = e.PreventNavigation,
                ShowResultsImmediately = e.ShowResultsImmediately,
                CreatedBy = e.CreatedBy,
                CreatedByName = e.CreatedByUser != null ? e.CreatedByUser.Name : null,
                CreatedAt = e.CreatedAt,
                ResultCount = e.ExamResults.Count
            })
            .ToListAsync(cancellationToken);

        var page = new Page<ExamDto>(
            exams,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetExamsResponse
        {
            Exams = page
        };
    }
}

