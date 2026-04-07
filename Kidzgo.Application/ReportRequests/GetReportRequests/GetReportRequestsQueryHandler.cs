using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportRequests.GetReportRequests;

public sealed class GetReportRequestsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetReportRequestsQuery, GetReportRequestsResponse>
{
    public async Task<Result<GetReportRequestsResponse>> Handle(
        GetReportRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetReportRequestsResponse>(
                Error.NotFound("User.NotFound", "User was not found"));
        }

        var requestsQuery = context.ReportRequests
            .Include(r => r.AssignedTeacher)
            .Include(r => r.RequestedByUser)
            .Include(r => r.TargetStudentProfile)
            .Include(r => r.TargetClass)
            .Include(r => r.TargetSession)
            .AsQueryable();

        if (currentUser.Role == UserRole.Teacher)
        {
            requestsQuery = requestsQuery.Where(r => r.AssignedTeacherUserId == currentUser.Id);
        }

        if (query.ReportType.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.ReportType == query.ReportType.Value);
        }

        if (query.Status.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.Priority == query.Priority.Value);
        }

        if (query.AssignedTeacherUserId.HasValue && currentUser.Role != UserRole.Teacher)
        {
            requestsQuery = requestsQuery.Where(r => r.AssignedTeacherUserId == query.AssignedTeacherUserId.Value);
        }

        if (query.TargetStudentProfileId.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.TargetStudentProfileId == query.TargetStudentProfileId.Value);
        }

        if (query.TargetClassId.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.TargetClassId == query.TargetClassId.Value);
        }

        if (query.Month.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.Month == query.Month.Value);
        }

        if (query.Year.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.Year == query.Year.Value);
        }

        var totalCount = await requestsQuery.CountAsync(cancellationToken);

        var requests = await requestsQuery
            .OrderBy(r => r.Status == ReportRequestStatus.Requested ? 0 :
                          r.Status == ReportRequestStatus.InProgress ? 1 :
                          r.Status == ReportRequestStatus.Submitted ? 2 :
                          r.Status == ReportRequestStatus.Rejected ? 3 :
                          r.Status == ReportRequestStatus.Approved ? 4 : 5)
            .ThenBy(r => r.Priority == ReportRequestPriority.Urgent ? 0 :
                         r.Priority == ReportRequestPriority.High ? 1 :
                         r.Priority == ReportRequestPriority.Normal ? 2 : 3)
            .ThenBy(r => r.DueAt == null)
            .ThenBy(r => r.DueAt)
            .ThenByDescending(r => r.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var page = new Page<ReportRequestDto>(
            requests.Select(ReportRequestMapper.ToDto).ToList(),
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetReportRequestsResponse
        {
            ReportRequests = page
        };
    }
}
