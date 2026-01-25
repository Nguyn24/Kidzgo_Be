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

        // Get StudentId from context (token) - only get data for selected student
        var selectedStudentId = userContext.StudentId;

        if (!selectedStudentId.HasValue)
        {
            return Result.Failure<Page<StudentWithMakeupOrLeaveResponse>>(
                Error.NotFound("StudentId", "No student selected in token"));
        }

        // Verify the student is linked to this parent
        var isLinked = await context.ParentStudentLinks
            .AsNoTracking()
            .AnyAsync(link => link.ParentProfileId == parentProfile.Id && 
                             link.StudentProfileId == selectedStudentId.Value, 
                      cancellationToken);

        if (!isLinked)
        {
            return Result.Failure<Page<StudentWithMakeupOrLeaveResponse>>(
                Error.NotFound("Student", "Student not linked to this parent"));
        }

        // Check if this student has leave requests or makeup credits
        var hasLeaveRequest = await context.LeaveRequests
            .AnyAsync(lr => lr.StudentProfileId == selectedStudentId.Value &&
                           (lr.Status == Domain.Sessions.LeaveRequestStatus.Pending || 
                            lr.Status == Domain.Sessions.LeaveRequestStatus.Approved),
                     cancellationToken);

        var hasMakeupCredit = await context.MakeupCredits
            .AnyAsync(mc => mc.StudentProfileId == selectedStudentId.Value &&
                           mc.Status == Domain.Sessions.MakeupCreditStatus.Available,
                     cancellationToken);

        if (!hasLeaveRequest && !hasMakeupCredit)
        {
            return new Page<StudentWithMakeupOrLeaveResponse>(
                new List<StudentWithMakeupOrLeaveResponse>(),
                0,
                query.PageNumber,
                query.PageSize);
        }

        // Query profile for selected student only
        var profile = await context.Profiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == selectedStudentId.Value && 
                                     p.ProfileType == ProfileType.Student && 
                                     !p.IsDeleted && 
                                     p.IsActive, cancellationToken);

        if (profile == null)
        {
            return new Page<StudentWithMakeupOrLeaveResponse>(
                new List<StudentWithMakeupOrLeaveResponse>(),
                0,
                query.PageNumber,
                query.PageSize);
        }

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim().ToLower();
            if (!profile.DisplayName.ToLower().Contains(searchTerm))
            {
                return new Page<StudentWithMakeupOrLeaveResponse>(
                    new List<StudentWithMakeupOrLeaveResponse>(),
                    0,
                    query.PageNumber,
                    query.PageSize);
            }
        }

        // Get leave request and makeup credit counts
        var leaveRequestCount = await context.LeaveRequests
            .CountAsync(lr => lr.StudentProfileId == selectedStudentId.Value && 
                             (lr.Status == Domain.Sessions.LeaveRequestStatus.Pending || 
                              lr.Status == Domain.Sessions.LeaveRequestStatus.Approved),
                       cancellationToken);

        var makeupCreditCount = await context.MakeupCredits
            .CountAsync(mc => mc.StudentProfileId == selectedStudentId.Value &&
                             mc.Status == Domain.Sessions.MakeupCreditStatus.Available,
                       cancellationToken);

        var result = new List<StudentWithMakeupOrLeaveResponse>
        {
            new StudentWithMakeupOrLeaveResponse
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DisplayName = profile.DisplayName,
                UserEmail = profile.User.Email,
                HasLeaveRequest = leaveRequestCount > 0,
                HasMakeupCredit = makeupCreditCount > 0,
                LeaveRequestCount = leaveRequestCount,
                MakeupCreditCount = makeupCreditCount
            }
        };

        return new Page<StudentWithMakeupOrLeaveResponse>(
            result,
            1,
            query.PageNumber,
            query.PageSize);
    }
}

