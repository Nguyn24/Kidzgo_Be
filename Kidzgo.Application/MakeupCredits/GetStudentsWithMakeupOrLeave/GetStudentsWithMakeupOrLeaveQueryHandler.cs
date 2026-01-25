using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetStudentsWithMakeupOrLeave;

public sealed class GetStudentsWithMakeupOrLeaveQueryHandler(IDbContext context)
    : IQueryHandler<GetStudentsWithMakeupOrLeaveQuery, Page<StudentWithMakeupOrLeaveResponse>>
{
    public async Task<Result<Page<StudentWithMakeupOrLeaveResponse>>> Handle(
        GetStudentsWithMakeupOrLeaveQuery query,
        CancellationToken cancellationToken)
    {
        // Get distinct student profile IDs that have leave requests or makeup credits
        var studentsWithLeaveRequests = context.LeaveRequests
            .Where(lr => lr.Status == Domain.Sessions.LeaveRequestStatus.Pending || 
                        lr.Status == Domain.Sessions.LeaveRequestStatus.Approved)
            .Select(lr => lr.StudentProfileId)
            .Distinct();

        var studentsWithMakeupCredits = context.MakeupCredits
            .Where(mc => mc.Status == Domain.Sessions.MakeupCreditStatus.Available)
            .Select(mc => mc.StudentProfileId)
            .Distinct();

        var studentProfileIds = await studentsWithLeaveRequests
            .Union(studentsWithMakeupCredits)
            .ToListAsync(cancellationToken);

        if (!studentProfileIds.Any())
        {
            return new Page<StudentWithMakeupOrLeaveResponse>(
                new List<StudentWithMakeupOrLeaveResponse>(),
                0,
                query.PageNumber,
                query.PageSize);
        }

        // Query profiles with filters
        var profilesQuery = context.Profiles
            .Include(p => p.User)
            .Where(p => studentProfileIds.Contains(p.Id) && 
                       p.ProfileType == ProfileType.Student && 
                       !p.IsDeleted && 
                       p.IsActive)
            .AsQueryable();

        // Apply search by display name
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim().ToLower();
            profilesQuery = profilesQuery.Where(p => 
                p.DisplayName.ToLower().Contains(searchTerm));
        }

        // Apply branch filter
        if (query.BranchId.HasValue)
        {
            profilesQuery = profilesQuery.Where(p => 
                p.ClassEnrollments.Any(ce => ce.Class.BranchId == query.BranchId.Value));
        }

        var totalCount = await profilesQuery.CountAsync(cancellationToken);

        // Get leave request and makeup credit counts for each student
        var profiles = await profilesQuery
            .OrderBy(p => p.DisplayName)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(p => new
            {
                Profile = p,
                LeaveRequestCount = context.LeaveRequests
                    .Count(lr => lr.StudentProfileId == p.Id && 
                                (lr.Status == Domain.Sessions.LeaveRequestStatus.Pending || 
                                 lr.Status == Domain.Sessions.LeaveRequestStatus.Approved)),
                MakeupCreditCount = context.MakeupCredits
                    .Count(mc => mc.StudentProfileId == p.Id && 
                               mc.Status == Domain.Sessions.MakeupCreditStatus.Available)
            })
            .ToListAsync(cancellationToken);

        var result = profiles.Select(p => new StudentWithMakeupOrLeaveResponse
        {
            Id = p.Profile.Id,
            UserId = p.Profile.UserId,
            DisplayName = p.Profile.DisplayName,
            UserEmail = p.Profile.User.Email,
            HasLeaveRequest = p.LeaveRequestCount > 0,
            HasMakeupCredit = p.MakeupCreditCount > 0,
            LeaveRequestCount = p.LeaveRequestCount,
            MakeupCreditCount = p.MakeupCreditCount
        }).ToList();

        return new Page<StudentWithMakeupOrLeaveResponse>(
            result,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}

