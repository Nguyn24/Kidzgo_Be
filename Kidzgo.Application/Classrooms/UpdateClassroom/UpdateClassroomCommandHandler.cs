using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classrooms.UpdateClassroom;

public sealed class UpdateClassroomCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateClassroomCommand, UpdateClassroomResponse>
{
    public async Task<Result<UpdateClassroomResponse>> Handle(UpdateClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classroom is null)
        {
            return Result.Failure<UpdateClassroomResponse>(ClassroomErrors.NotFound(command.Id));
        }

        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<UpdateClassroomResponse>(ClassroomErrors.BranchNotFound);
        }

        classroom.BranchId = command.BranchId;
        classroom.Name = command.Name;
        classroom.Capacity = command.Capacity;
        classroom.Note = command.Note;

        await context.SaveChangesAsync(cancellationToken);

        // Re-query with Branch for response
        var updatedClassroom = await context.Classrooms
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        return new UpdateClassroomResponse
        {
            Id = classroom.Id,
            BranchId = classroom.BranchId,
            Name = classroom.Name,
            Capacity = classroom.Capacity,
            Note = classroom.Note,
            IsActive = classroom.IsActive
        };
    }
}

