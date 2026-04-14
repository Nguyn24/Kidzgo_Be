using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.IncidentReports.GetIncidentReportStatistics;

public sealed class GetIncidentReportStatisticsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetIncidentReportStatisticsQuery, GetIncidentReportStatisticsResponse>
{
    public async Task<Result<GetIncidentReportStatisticsResponse>> Handle(
        GetIncidentReportStatisticsQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetIncidentReportStatisticsResponse>(TicketErrors.UserNotFound);
        }

        if (!IncidentReportAccessPolicy.CanViewAll(currentUser.Role))
        {
            return Result.Failure<GetIncidentReportStatisticsResponse>(IncidentReportErrors.Unauthorized);
        }

        var ticketsQuery = context.Tickets
            .AsNoTracking()
            .Include(t => t.OpenedByUser)
            .Include(t => t.AssignedToUser)
            .Where(t => t.IsIncidentReport)
            .AsQueryable();

        if (query.BranchId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.BranchId == query.BranchId.Value);
        }

        if (query.OpenedByUserId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.OpenedByUserId == query.OpenedByUserId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.ClassId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.ClassId == query.ClassId.Value);
        }

        if (query.Category.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.IncidentCategory == query.Category.Value);
        }

        if (query.Status.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.IncidentStatus == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            ticketsQuery = ticketsQuery.Where(t =>
                t.Subject.Contains(keyword) ||
                t.Message.Contains(keyword) ||
                (t.OpenedByUser != null && t.OpenedByUser.Name != null && t.OpenedByUser.Name.Contains(keyword)) ||
                (t.AssignedToUser != null && t.AssignedToUser.Name != null && t.AssignedToUser.Name.Contains(keyword)));
        }

        if (query.CreatedFrom.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.CreatedAt <= query.CreatedTo.Value);
        }

        return new GetIncidentReportStatisticsResponse
        {
            Total = await ticketsQuery.CountAsync(cancellationToken),
            Open = await ticketsQuery.CountAsync(t => t.IncidentStatus == IncidentReportStatus.Open, cancellationToken),
            InProgress = await ticketsQuery.CountAsync(t => t.IncidentStatus == IncidentReportStatus.InProgress, cancellationToken),
            Resolved = await ticketsQuery.CountAsync(t => t.IncidentStatus == IncidentReportStatus.Resolved, cancellationToken),
            Closed = await ticketsQuery.CountAsync(t => t.IncidentStatus == IncidentReportStatus.Closed, cancellationToken),
            Rejected = await ticketsQuery.CountAsync(t => t.IncidentStatus == IncidentReportStatus.Rejected, cancellationToken),
            Unassigned = await ticketsQuery.CountAsync(t => t.AssignedToUserId == null, cancellationToken),
            ByStatus = await ticketsQuery
                .GroupBy(t => t.IncidentStatus)
                .Select(g => new IncidentStatusStatDto
                {
                    Status = g.Key!.Value.ToString(),
                    Count = g.Count()
                })
                .OrderBy(x => x.Status)
                .ToListAsync(cancellationToken),
            ByCategory = await ticketsQuery
                .GroupBy(t => t.IncidentCategory)
                .Select(g => new IncidentCategoryStatDto
                {
                    Category = g.Key!.Value.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Category)
                .ToListAsync(cancellationToken)
        };
    }
}
