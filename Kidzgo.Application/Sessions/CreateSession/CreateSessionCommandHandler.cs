using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.CreateSession;

public sealed class CreateSessionCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker
) : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    public async Task<Result<CreateSessionResponse>> Handle(CreateSessionCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateSessionResponse>(ClassErrors.NotFound(command.ClassId));
        }

        // Only allow creating sessions for Planned or Active classes
        if (classEntity.Status is not ClassStatus.Planned and not ClassStatus.Active)
        {
            return Result.Failure<CreateSessionResponse>(SessionErrors.InvalidClassStatus);
        }

        var now = DateTime.UtcNow;

        var plannedUtc = command.PlannedDatetime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(command.PlannedDatetime, DateTimeKind.Utc)
            : command.PlannedDatetime.ToUniversalTime();

        // Check for conflicts (warning only, không block)
        var conflictResult = await conflictChecker.CheckConflictsAsync(
            Guid.Empty, // New session, no ID yet
            plannedUtc,
            command.DurationMinutes,
            command.PlannedRoomId,
            command.PlannedTeacherId,
            command.PlannedAssistantId,
            cancellationToken);

        // Note: Conflicts are logged but don't block creation - business logic decision

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ClassId = classEntity.Id,
            BranchId = classEntity.BranchId,
            PlannedDatetime = plannedUtc,
            PlannedRoomId = command.PlannedRoomId,
            PlannedTeacherId = command.PlannedTeacherId,
            PlannedAssistantId = command.PlannedAssistantId,
            DurationMinutes = command.DurationMinutes,
            ParticipationType = command.ParticipationType,
            Status = SessionStatus.Scheduled,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Sessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateSessionResponse
        {
            Id = session.Id,
            ClassId = session.ClassId,
            BranchId = session.BranchId,
            PlannedDatetime = session.PlannedDatetime,
            DurationMinutes = session.DurationMinutes
        };
    }
}
