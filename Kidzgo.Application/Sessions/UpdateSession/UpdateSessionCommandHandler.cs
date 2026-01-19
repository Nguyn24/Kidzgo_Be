using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateSession;

public sealed class UpdateSessionCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker
) : ICommandHandler<UpdateSessionCommand, UpdateSessionResponse>
{
    public async Task<Result<UpdateSessionResponse>> Handle(UpdateSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<UpdateSessionResponse>(SessionErrors.NotFound(command.SessionId));
        }

        if (session.Status is SessionStatus.Cancelled or SessionStatus.Completed)
        {
            return Result.Failure<UpdateSessionResponse>(SessionErrors.InvalidStatus);
        }

        var plannedUtc = command.PlannedDatetime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(command.PlannedDatetime, DateTimeKind.Utc)
            : command.PlannedDatetime.ToUniversalTime();

        // Check for conflicts (warning only, không block)
        var conflictResult = await conflictChecker.CheckConflictsAsync(
            session.Id,
            plannedUtc,
            command.DurationMinutes,
            command.PlannedRoomId,
            command.PlannedTeacherId,
            command.PlannedAssistantId,
            cancellationToken);

        // Note: Conflicts are logged but don't block update - business logic decision

        session.PlannedDatetime = plannedUtc;
        session.DurationMinutes = command.DurationMinutes;
        session.PlannedRoomId = command.PlannedRoomId;
        session.PlannedTeacherId = command.PlannedTeacherId;
        session.PlannedAssistantId = command.PlannedAssistantId;
        session.ParticipationType = command.ParticipationType;
        session.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateSessionResponse
        {
            Id = session.Id,
            PlannedDatetime = session.PlannedDatetime,
            DurationMinutes = session.DurationMinutes
        };
    }
}
