using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.IncidentReports.GetIncidentReports;

public sealed class GetIncidentReportsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetIncidentReportsQuery, GetIncidentReportsResponse>
{
    public async Task<Result<GetIncidentReportsResponse>> Handle(GetIncidentReportsQuery query, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetIncidentReportsResponse>(TicketErrors.UserNotFound);
        }

        if (!IncidentReportAccessPolicy.CanUseIncidentFlow(currentUser.Role))
        {
            return Result.Failure<GetIncidentReportsResponse>(IncidentReportErrors.InvalidRole);
        }

        var ticketsQuery = context.Tickets
            .AsNoTracking()
            .Include(t => t.OpenedByUser)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TicketComments)
            .Where(t => t.IsIncidentReport)
            .AsQueryable();

        if (!IncidentReportAccessPolicy.CanViewAll(currentUser.Role))
        {
            ticketsQuery = ticketsQuery.Where(t => t.OpenedByUserId == currentUser.Id);
        }

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

        var total = await ticketsQuery.CountAsync(cancellationToken);
        var items = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        return new GetIncidentReportsResponse
        {
            IncidentReports = new Page<IncidentReportDto>(
                items.Select(IncidentReportMapper.ToDto).ToList(),
                total,
                query.PageNumber,
                query.PageSize)
        };
    }
}
