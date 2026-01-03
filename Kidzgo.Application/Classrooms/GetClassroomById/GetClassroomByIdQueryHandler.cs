using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classrooms.GetClassroomById;

public sealed class GetClassroomByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetClassroomByIdQuery, GetClassroomByIdResponse>
{
    public async Task<Result<GetClassroomByIdResponse>> Handle(GetClassroomByIdQuery query, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .Include(c => c.Branch)
            .Where(c => c.Id == query.Id)
            .Select(c => new GetClassroomByIdResponse
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                Name = c.Name,
                Capacity = c.Capacity,
                Note = c.Note,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (classroom is null)
        {
            return Result.Failure<GetClassroomByIdResponse>(
                Error.NotFound("Classroom.NotFound", "Classroom not found"));
        }

        return classroom;
    }
}

