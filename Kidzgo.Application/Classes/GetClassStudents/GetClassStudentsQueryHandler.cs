using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetClassStudents;

public sealed class GetClassStudentsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetClassStudentsQuery, GetClassStudentsResponse>
{
    public async Task<Result<GetClassStudentsResponse>> Handle(GetClassStudentsQuery query, CancellationToken cancellationToken)
    {
        var currentUserRole = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userContext.UserId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(cancellationToken);

        var classExists = currentUserRole == UserRole.Teacher
            ? await context.Classes
                .AsNoTracking()
                .AnyAsync(c =>
                    c.Id == query.ClassId &&
                    (c.MainTeacherId == userContext.UserId || c.AssistantTeacherId == userContext.UserId),
                    cancellationToken)
            : await context.Classes
                .AsNoTracking()
                .AnyAsync(c => c.Id == query.ClassId, cancellationToken);

        if (!classExists)
        {
            return Result.Failure<GetClassStudentsResponse>(ClassErrors.NotFound(query.ClassId));
        }

        var studentQuery = context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => ce.ClassId == query.ClassId)
            .Include(ce => ce.StudentProfile)
                .ThenInclude(p => p.User)
            .AsQueryable();

        var totalCount = await studentQuery.CountAsync(cancellationToken);
        var completedSessionCount = await context.Sessions
            .AsNoTracking()
            .Where(s => s.ClassId == query.ClassId && s.Status == SessionStatus.Completed)
            .CountAsync(cancellationToken);

        var students = await studentQuery
            .OrderBy(ce => ce.StudentProfile.DisplayName)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(ce => new ClassStudentDto
            {
                StudentProfileId = ce.StudentProfileId,
                FullName = ce.StudentProfile.DisplayName,
                AvatarUrl = ce.StudentProfile.AvatarUrl ?? ce.StudentProfile.User.AvatarUrl,
                Email = ce.StudentProfile.User.Email,
                Phone = ce.StudentProfile.User.PhoneNumber,
                EnrollDate = ce.EnrollDate,
                Status = ce.Status.ToString(),
                AttendanceRate = completedSessionCount <= 0
                    ? 0
                    : Math.Round(
                        (decimal)context.Attendances.Count(a =>
                            a.StudentProfileId == ce.StudentProfileId &&
                            a.Session.ClassId == query.ClassId &&
                            a.AttendanceStatus == AttendanceStatus.Present) * 100 / completedSessionCount,
                        2),
                ProgressPercent = completedSessionCount <= 0
                    ? 0
                    : Math.Round(
                        (decimal)context.Attendances.Count(a =>
                            a.StudentProfileId == ce.StudentProfileId &&
                            a.Session.ClassId == query.ClassId) * 100 / completedSessionCount,
                        2),
                Stars = ce.StudentProfile.StarTransactions.Sum(st => st.Amount),
                LastActiveAt = ce.StudentProfile.LastSeenAt
                    ?? ce.StudentProfile.User.LastSeenAt
                    ?? ce.StudentProfile.LastLoginAt
                    ?? ce.StudentProfile.User.LastLoginAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetClassStudentsResponse
        {
            Students = new Page<ClassStudentDto>(students, totalCount, query.PageNumber, query.PageSize)
        });
    }
}
