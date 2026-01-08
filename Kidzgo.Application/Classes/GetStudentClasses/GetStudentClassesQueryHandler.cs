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
        // Verify student exists and is active
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == query.StudentId, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<GetStudentClassesResponse>(
                Error.NotFound("Student.NotFound", "Student profile not found"));
        }

        if (profile.ProfileType != Domain.Users.ProfileType.Student)
        {
            return Result.Failure<GetStudentClassesResponse>(
                Error.NotFound("Student.NotFound", "Profile is not a student"));
        }

        if (!profile.IsActive || profile.IsDeleted)
        {
            return Result.Failure<GetStudentClassesResponse>(
                Error.NotFound("Student.NotFound", "Student profile is inactive or deleted"));
        }

        // Get enrollments where student is enrolled (Status = Active) and Class exists
        var enrollmentsQuery = context.ClassEnrollments
            .Where(ce => ce.StudentProfileId == query.StudentId && ce.Status == EnrollmentStatus.Active)
            .Where(ce => ce.Class != null);

        // Get total count
        int totalCount = await enrollmentsQuery.CountAsync(cancellationToken);

        // Load enrollments with all related data, order and paginate
        var enrollments = await enrollmentsQuery
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.Branch)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.Program)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.MainTeacher)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.AssistantTeacher)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.ClassEnrollments)
            .OrderByDescending(ce => ce.Class!.CreatedAt)
            .ThenBy(ce => ce.Class!.Title)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var classDtos = enrollments.Select(ce => new StudentClassDto
        {
            Id = ce.Class.Id,
            BranchId = ce.Class.BranchId,
            BranchName = ce.Class.Branch.Name,
            ProgramId = ce.Class.ProgramId,
            ProgramName = ce.Class.Program != null ? ce.Class.Program.Name : null,
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
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetStudentClassesResponse
        {
            Classes = page
        });
    }
}

