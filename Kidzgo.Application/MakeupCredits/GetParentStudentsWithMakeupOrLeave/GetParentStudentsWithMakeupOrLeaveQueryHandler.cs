using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.MakeupCredits.GetStudentsWithMakeupOrLeave;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetParentStudentsWithMakeupOrLeave;

public sealed class GetParentStudentsWithMakeupOrLeaveQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentStudentsWithMakeupOrLeaveQuery, Page<StudentWithMakeupOrLeaveResponse>>
{
    public async Task<Result<Page<StudentWithMakeupOrLeaveResponse>>> Handle(
        GetParentStudentsWithMakeupOrLeaveQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get parent profile for current user
        var parentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && 
                                     p.ProfileType == ProfileType.Parent && 
                                     !p.IsDeleted && 
                                     p.IsActive, cancellationToken);

        if (parentProfile == null)
        {
            return Result.Failure<Page<StudentWithMakeupOrLeaveResponse>>(
                Error.NotFound("ParentProfile", "Parent profile not found for current user"));
        }

        // Get student profile IDs linked to this parent
        var linkedStudentProfileIds = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link => link.ParentProfileId == parentProfile.Id)
            .Select(link => link.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (!linkedStudentProfileIds.Any())
        {
            return new Page<StudentWithMakeupOrLeaveResponse>(
                new List<StudentWithMakeupOrLeaveResponse>(),
                0,
                query.PageNumber,
                query.PageSize);
        }

        // Get distinct student profile IDs that have leave requests or makeup credits
        var studentsWithLeaveRequests = context.LeaveRequests
            .Where(lr => linkedStudentProfileIds.Contains(lr.StudentProfileId) &&
                        (lr.Status == Domain.Sessions.LeaveRequestStatus.Pending || 
                         lr.Status == Domain.Sessions.LeaveRequestStatus.Approved))
            .Select(lr => lr.StudentProfileId)
            .Distinct();

        var studentsWithMakeupCredits = context.MakeupCredits
            .Where(mc => linkedStudentProfileIds.Contains(mc.StudentProfileId) &&
                        mc.Status == Domain.Sessions.MakeupCreditStatus.Available)
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

        // Query profiles with filters (already filtered to linked students)
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

