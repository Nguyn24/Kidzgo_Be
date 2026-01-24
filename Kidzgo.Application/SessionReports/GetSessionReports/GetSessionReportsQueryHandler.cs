using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.GetSessionReports;

public sealed class GetSessionReportsQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionReportsQuery, GetSessionReportsResponse>
{
    public async Task<Result<GetSessionReportsResponse>> Handle(
        GetSessionReportsQuery query,
        CancellationToken cancellationToken)
    {
        var reportsQuery = context.SessionReports
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .AsQueryable();

        if (query.SessionId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.SessionId == query.SessionId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.TeacherUserId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.TeacherUserId == query.TeacherUserId.Value);
        }

        if (query.ClassId.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.Session.ClassId == query.ClassId.Value);
        }

        if (query.FromDate.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.ReportDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            reportsQuery = reportsQuery.Where(sr => sr.ReportDate <= query.ToDate.Value);
        }

        var totalCount = await reportsQuery.CountAsync(cancellationToken);

        var items = await reportsQuery
            .OrderByDescending(sr => sr.ReportDate)
            .ThenByDescending(sr => sr.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(sr => new SessionReportListItemDto
            {
                Id = sr.Id,
                SessionId = sr.SessionId,
                SessionDate = sr.Session.PlannedDatetime,
                ClassId = sr.Session.ClassId,
                ClassCode = sr.Session.Class.Code,
                ClassTitle = sr.Session.Class.Title,
                StudentProfileId = sr.StudentProfileId,
                StudentName = sr.StudentProfile.DisplayName,
                TeacherUserId = sr.TeacherUserId,
                TeacherName = sr.TeacherUser.Name,
                ReportDate = sr.ReportDate,
                Feedback = sr.Feedback,
                IsMonthlyCompiled = sr.IsMonthlyCompiled,
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<SessionReportListItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetSessionReportsResponse
        {
            SessionReports = page
        };
    }
}

