using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetTeacherClasses;

public sealed class GetTeacherClassesQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeacherClassesQuery, GetTeacherClassesResponse>
{
    public async Task<Result<GetTeacherClassesResponse>> Handle(GetTeacherClassesQuery query, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get classes where teacher is MainTeacher or AssistantTeacher
        var classesQuery = context.Classes
            .Include(c => c.Branch)
            .Include(c => c.Program)
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .Where(c => c.MainTeacherId == userId || c.AssistantTeacherId == userId);

        // Get total count
        int totalCount = await classesQuery.CountAsync(cancellationToken);

        // Apply pagination
        var classes = await classesQuery
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Title)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new TeacherClassDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                ProgramId = c.ProgramId,
                ProgramName = c.Program.Name,
                Code = c.Code,
                Title = c.Title,
                MainTeacherId = c.MainTeacherId,
                MainTeacherName = c.MainTeacher != null ? c.MainTeacher.Name : null,
                AssistantTeacherId = c.AssistantTeacherId,
                AssistantTeacherName = c.AssistantTeacher != null ? c.AssistantTeacher.Name : null,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                Capacity = c.Capacity,
                CurrentEnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active),
                SchedulePattern = c.SchedulePattern,
                Role = c.MainTeacherId == userId ? "MainTeacher" : "AssistantTeacher"
            })
            .ToListAsync(cancellationToken);

        var page = new Page<TeacherClassDto>(
            classes,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetTeacherClassesResponse
        {
            Classes = page
        });
    }
}

