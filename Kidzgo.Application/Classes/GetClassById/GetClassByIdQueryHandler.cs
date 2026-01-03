using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetClassById;

public sealed class GetClassByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetClassByIdQuery, GetClassByIdResponse>
{
    public async Task<Result<GetClassByIdResponse>> Handle(GetClassByIdQuery query, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.Branch)
            .Include(c => c.Program)
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .Where(c => c.Id == query.Id)
            .Select(c => new GetClassByIdResponse
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
                CurrentEnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active),
                SchedulePattern = c.SchedulePattern,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<GetClassByIdResponse>(
                Error.NotFound("Class.NotFound", "Class not found"));
        }

        return classEntity;
    }
}

