using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classrooms.DeleteClassroom;

public sealed class DeleteClassroomCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteClassroomCommand>
{
    public async Task<Result> Handle(DeleteClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classroom is null)
        {
            return Result.Failure(
                Error.NotFound("Classroom.NotFound", "Classroom not found"));
        }

        // Check if classroom is being used by any sessions (planned or actual)
        bool hasSessions = await context.Sessions
            .AnyAsync(s => s.PlannedRoomId == command.Id || s.ActualRoomId == command.Id, cancellationToken);

        if (hasSessions)
        {
            return Result.Failure(
                Error.Conflict("Classroom.HasSessions", "Cannot delete classroom that is being used in sessions"));
        }

        // Hard delete (Classroom doesn't have IsDeleted field)
        context.Classrooms.Remove(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

