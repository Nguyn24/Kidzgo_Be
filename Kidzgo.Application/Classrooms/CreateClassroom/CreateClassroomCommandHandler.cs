using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classrooms.CreateClassroom;

public sealed class CreateClassroomCommandHandler(
    IDbContext context
) : ICommandHandler<CreateClassroomCommand, CreateClassroomResponse>
{
    public async Task<Result<CreateClassroomResponse>> Handle(CreateClassroomCommand command, CancellationToken cancellationToken)
    {
        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateClassroomResponse>(
                Error.NotFound("Classroom.BranchNotFound", "Branch not found or inactive"));
        }

        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            Name = command.Name,
            Capacity = command.Capacity,
            Note = command.Note,
            IsActive = false, // Mặc định false, cần duyệt qua toggle-status API để active
        };

        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateClassroomResponse
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

