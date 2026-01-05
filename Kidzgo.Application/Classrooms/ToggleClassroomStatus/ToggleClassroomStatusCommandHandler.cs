using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classrooms.ToggleClassroomStatus;

public sealed class ToggleClassroomStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ToggleClassroomStatusCommand, ToggleClassroomStatusResponse>
{
    public async Task<Result<ToggleClassroomStatusResponse>> Handle(ToggleClassroomStatusCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classroom is null)
        {
            return Result.Failure<ToggleClassroomStatusResponse>(
                Error.NotFound("Classroom.NotFound", "Classroom not found"));
        }

        classroom.IsActive = !classroom.IsActive;
        await context.SaveChangesAsync(cancellationToken);

        return new ToggleClassroomStatusResponse
        {
            Id = classroom.Id,
            IsActive = classroom.IsActive
        };
    }
}

