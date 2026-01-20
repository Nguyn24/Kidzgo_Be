using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetTeacherClassStudents;

public sealed class GetTeacherClassStudentsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeacherClassStudentsQuery, GetTeacherClassStudentsResponse>
{
    public async Task<Result<GetTeacherClassStudentsResponse>> Handle(GetTeacherClassStudentsQuery query, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Ensure the requested class belongs to this teacher (main/assistant).
        // Return NotFound to avoid leaking class existence across teachers.
        var classExistsForTeacher = await context.Classes
            .AnyAsync(c => c.Id == query.ClassId && (c.MainTeacherId == userId || c.AssistantTeacherId == userId), cancellationToken);

        if (!classExistsForTeacher)
        {
            return Result.Failure<GetTeacherClassStudentsResponse>(ClassErrors.NotFound(query.ClassId));
        }

        var enrollmentsQuery = context.ClassEnrollments
            .AsNoTracking()
            .Include(e => e.StudentProfile)
            .ThenInclude(p => p.User)
            .Where(e => e.ClassId == query.ClassId && e.Status == EnrollmentStatus.Active);

        var totalCount = await enrollmentsQuery.CountAsync(cancellationToken);

        var students = await enrollmentsQuery
            .OrderBy(e => e.StudentProfile.DisplayName)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(e => new TeacherClassStudentDto
            {
                EnrollmentId = e.Id,
                StudentProfileId = e.StudentProfileId,
                StudentUserId = e.StudentProfile.UserId,
                StudentName = e.StudentProfile.DisplayName,
                StudentEmail = e.StudentProfile.User.Email,
                EnrollDate = e.EnrollDate,
                Status = e.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var page = new Page<TeacherClassStudentDto>(
            students,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetTeacherClassStudentsResponse
        {
            Students = page
        });
    }
}


