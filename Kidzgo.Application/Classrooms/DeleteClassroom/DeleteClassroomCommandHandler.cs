using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
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
            return Result.Failure(ClassroomErrors.NotFound(command.Id));
        }

        // Check if classroom is being used by any sessions (planned or actual)
        bool hasSessions = await context.Sessions
            .AnyAsync(s => s.PlannedRoomId == command.Id || s.ActualRoomId == command.Id, cancellationToken);

        if (hasSessions)
        {
            return Result.Failure(ClassroomErrors.HasSessions);
        }

        // Hard delete (Classroom doesn't have IsDeleted field)
        context.Classrooms.Remove(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

