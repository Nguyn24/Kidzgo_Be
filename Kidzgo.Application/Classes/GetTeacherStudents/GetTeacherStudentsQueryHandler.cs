using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetTeacherStudents;

public sealed class GetTeacherStudentsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeacherStudentsQuery, GetTeacherStudentsResponse>
{
    public async Task<Result<GetTeacherStudentsResponse>> Handle(GetTeacherStudentsQuery query, CancellationToken cancellationToken)
    {
        var teacherUserId = userContext.UserId;

        if (query.ClassId.HasValue)
        {
            var classExistsForTeacher = await context.Classes
                .AsNoTracking()
                .AnyAsync(c =>
                    c.Id == query.ClassId.Value &&
                    (c.MainTeacherId == teacherUserId || c.AssistantTeacherId == teacherUserId),
                    cancellationToken);

            if (!classExistsForTeacher)
            {
                return Result.Failure<GetTeacherStudentsResponse>(ClassErrors.NotFound(query.ClassId.Value));
            }
        }

        var enrollmentsQuery = context.ClassEnrollments
            .AsNoTracking()
            .Where(e =>
                e.Status == EnrollmentStatus.Active &&
                (e.Class.MainTeacherId == teacherUserId || e.Class.AssistantTeacherId == teacherUserId));

        if (query.ClassId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(e => e.ClassId == query.ClassId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim().ToLower();
            enrollmentsQuery = enrollmentsQuery.Where(e =>
                e.StudentProfile.DisplayName.ToLower().Contains(searchTerm) ||
                e.StudentProfile.User.Email.ToLower().Contains(searchTerm));
        }

        var groupedQuery = enrollmentsQuery
            .GroupBy(e => new
            {
                e.StudentProfileId,
                e.StudentProfile.UserId,
                e.StudentProfile.DisplayName,
                e.StudentProfile.User.Email
            });

        var totalCount = await groupedQuery.CountAsync(cancellationToken);

        var students = await groupedQuery
            .OrderBy(g => g.Key.DisplayName)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(g => new TeacherStudentDto
            {
                StudentProfileId = g.Key.StudentProfileId,
                StudentUserId = g.Key.UserId,
                StudentName = g.Key.DisplayName,
                StudentEmail = g.Key.Email
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetTeacherStudentsResponse
        {
            Students = new Page<TeacherStudentDto>(students, totalCount, query.PageNumber, query.PageSize)
        });
    }
}
