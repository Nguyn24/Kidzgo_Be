using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetStudentClasses;

public sealed class GetStudentClassesQueryHandler(
    IDbContext context
) : IQueryHandler<GetStudentClassesQuery, GetStudentClassesResponse>
{
    public async Task<Result<GetStudentClassesResponse>> Handle(GetStudentClassesQuery query, CancellationToken cancellationToken)
    {
        // Verify student exists
        var studentExists = await context.Profiles
            .AnyAsync(p => p.Id == query.StudentId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (!studentExists)
        {
            return Result.Failure<GetStudentClassesResponse>(
                Error.NotFound("Student.NotFound", "Student not found"));
        }

        // Get enrollments where student is enrolled (Active status)
        var enrollmentsQuery = context.ClassEnrollments
            .Include(ce => ce.Class)
                .ThenInclude(c => c.Branch)
            .Include(ce => ce.Class)
                .ThenInclude(c => c.Program)
            .Include(ce => ce.Class)
                .ThenInclude(c => c.MainTeacher)
            .Include(ce => ce.Class)
                .ThenInclude(c => c.AssistantTeacher)
            .Include(ce => ce.Class)
                .ThenInclude(c => c.ClassEnrollments)
            .Where(ce => ce.StudentProfileId == query.StudentId && ce.Status == EnrollmentStatus.Active);

        // Get total count
        int totalCount = await enrollmentsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var enrollments = await enrollmentsQuery
            .OrderByDescending(ce => ce.Class.CreatedAt)
            .ThenBy(ce => ce.Class.Title)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var classDtos = enrollments.Select(ce => new StudentClassDto
        {
            Id = ce.Class.Id,
            BranchId = ce.Class.BranchId,
            BranchName = ce.Class.Branch.Name,
            ProgramId = ce.Class.ProgramId,
            ProgramName = ce.Class.Program.Name,
            Code = ce.Class.Code,
            Title = ce.Class.Title,
            MainTeacherId = ce.Class.MainTeacherId,
            MainTeacherName = ce.Class.MainTeacher != null ? ce.Class.MainTeacher.Name : null,
            AssistantTeacherId = ce.Class.AssistantTeacherId,
            AssistantTeacherName = ce.Class.AssistantTeacher != null ? ce.Class.AssistantTeacher.Name : null,
            StartDate = ce.Class.StartDate,
            EndDate = ce.Class.EndDate,
            Status = ce.Class.Status,
            Capacity = ce.Class.Capacity,
            CurrentEnrollmentCount = ce.Class.ClassEnrollments.Count(e => e.Status == EnrollmentStatus.Active),
            SchedulePattern = ce.Class.SchedulePattern,
            EnrollDate = ce.EnrollDate,
            EnrollmentStatus = ce.Status
        }).ToList();

        var page = new Page<StudentClassDto>(
            classDtos,
            query.PageNumber,
            query.PageSize,
            totalCount);

        return Result.Success(new GetStudentClassesResponse
        {
            Classes = page
        });
    }
}

