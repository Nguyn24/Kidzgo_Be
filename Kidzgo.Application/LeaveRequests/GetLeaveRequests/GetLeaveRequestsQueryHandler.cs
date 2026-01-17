using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.GetLeaveRequests;

public sealed class GetLeaveRequestsQueryHandler(IDbContext context)
    : IQueryHandler<GetLeaveRequestsQuery, Page<GetLeaveRequestsResponse>>
{
    public async Task<Result<Page<GetLeaveRequestsResponse>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = context.LeaveRequests
            .AsQueryable();

        if (request.StudentProfileId.HasValue)
        {
            query = query.Where(l => l.StudentProfileId == request.StudentProfileId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(l => l.ClassId == request.ClassId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.RequestedAt)
            .ApplyPagination(request.PageNumber, request.PageSize)
            .Select(l => new GetLeaveRequestsResponse
            {
                Id = l.Id,
                StudentProfileId = l.StudentProfileId,
                ClassId = l.ClassId,
                SessionDate = l.SessionDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                NoticeHours = l.NoticeHours,
                Status = l.Status,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt
            })
            .ToListAsync(cancellationToken);

        return new Page<GetLeaveRequestsResponse>(items, total, request.PageNumber, request.PageSize);
    }
}

