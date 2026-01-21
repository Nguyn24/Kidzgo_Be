using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateSessionsByClass;

public sealed class UpdateSessionsByClassCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker
) : ICommandHandler<UpdateSessionsByClassCommand, UpdateSessionsByClassResponse>
{
    public async Task<Result<UpdateSessionsByClassResponse>> Handle(
        UpdateSessionsByClassCommand command,
        CancellationToken cancellationToken)
    {
        // Kiểm tra class tồn tại
        var classExists = await context.Classes
            .AnyAsync(c => c.Id == command.ClassId, cancellationToken);

        if (!classExists)
        {
            return Result.Failure<UpdateSessionsByClassResponse>(
                ClassErrors.NotFound(command.ClassId));
        }

        // Build query để lấy sessions cần update
        var query = context.Sessions
            .Where(s => s.ClassId == command.ClassId);

        // Nếu có danh sách SessionIds cụ thể, chỉ update các sessions đó
        if (command.SessionIds != null && command.SessionIds.Count > 0)
        {
            query = query.Where(s => command.SessionIds.Contains(s.Id));
        }

        // Filter theo status nếu có
        if (command.FilterByStatus.HasValue)
        {
            query = query.Where(s => s.Status == command.FilterByStatus.Value);
        }

        // Filter theo FromDate nếu có
        if (command.FromDate.HasValue)
        {
            var fromDateUtc = command.FromDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.FromDate.Value, DateTimeKind.Utc)
                : command.FromDate.Value.ToUniversalTime();
            query = query.Where(s => s.PlannedDatetime >= fromDateUtc);
        }

        // Chỉ update các sessions có thể update được (không phải Cancelled hoặc Completed)
        query = query.Where(s => s.Status != SessionStatus.Cancelled && s.Status != SessionStatus.Completed);

        var sessions = await query.ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return Result.Success(new UpdateSessionsByClassResponse
            {
                UpdatedSessionsCount = 0,
                UpdatedSessionIds = new List<Guid>(),
                SkippedSessionIds = new List<Guid>(),
                Errors = new List<string> { "Không tìm thấy sessions nào để update" }
            });
        }

        var updatedSessionIds = new List<Guid>();
        var skippedSessionIds = new List<Guid>();
        var errors = new List<string>();
        var now = DateTime.UtcNow;

        foreach (var session in sessions)
        {
            try
            {
                // Kiểm tra xem có field nào cần update không
                bool hasChanges = false;

                // Xác định các giá trị sẽ được sử dụng sau khi update
                var plannedUtc = command.PlannedDatetime.HasValue
                    ? (command.PlannedDatetime.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(command.PlannedDatetime.Value, DateTimeKind.Utc)
                        : command.PlannedDatetime.Value.ToUniversalTime())
                    : session.PlannedDatetime;
                
                var duration = command.DurationMinutes ?? session.DurationMinutes;
                var roomId = command.PlannedRoomId ?? session.PlannedRoomId;
                var teacherId = command.PlannedTeacherId ?? session.PlannedTeacherId;
                var assistantId = command.PlannedAssistantId ?? session.PlannedAssistantId;

                // Kiểm tra conflict nếu có thay đổi về datetime, room, hoặc teacher
                bool needsConflictCheck = command.PlannedDatetime.HasValue ||
                                        command.PlannedRoomId.HasValue ||
                                        command.PlannedTeacherId.HasValue ||
                                        command.PlannedAssistantId.HasValue ||
                                        command.DurationMinutes.HasValue;

                if (needsConflictCheck)
                {
                    var conflictResult = await conflictChecker.CheckConflictsAsync(
                        session.Id,
                        plannedUtc,
                        duration,
                        roomId,
                        teacherId,
                        assistantId,
                        cancellationToken);

                    if (conflictResult.HasConflicts)
                    {
                        var conflictMessages = conflictResult.Conflicts
                            .Select(c => $"{c.Type}: {c.ClassCode} - {c.ClassTitle} vào {c.ConflictDatetime:dd/MM/yyyy HH:mm}")
                            .ToList();
                        errors.Add($"Session {session.Id}: Xung đột - {string.Join(", ", conflictMessages)}");
                        skippedSessionIds.Add(session.Id);
                        continue;
                    }
                }

                // Update PlannedDatetime nếu có
                if (command.PlannedDatetime.HasValue)
                {
                    session.PlannedDatetime = plannedUtc;
                    hasChanges = true;
                }

                // Update DurationMinutes nếu có
                if (command.DurationMinutes.HasValue)
                {
                    session.DurationMinutes = command.DurationMinutes.Value;
                    hasChanges = true;
                }

                // Update PlannedRoomId nếu có
                if (command.PlannedRoomId.HasValue)
                {
                    session.PlannedRoomId = command.PlannedRoomId.Value;
                    hasChanges = true;
                }

                // Update PlannedTeacherId nếu có
                if (command.PlannedTeacherId.HasValue)
                {
                    session.PlannedTeacherId = command.PlannedTeacherId.Value;
                    hasChanges = true;
                }

                // Update PlannedAssistantId nếu có
                if (command.PlannedAssistantId.HasValue)
                {
                    session.PlannedAssistantId = command.PlannedAssistantId.Value;
                    hasChanges = true;
                }

                // Update ParticipationType nếu có
                if (command.ParticipationType.HasValue)
                {
                    session.ParticipationType = command.ParticipationType.Value;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    session.UpdatedAt = now;
                    updatedSessionIds.Add(session.Id);
                }
                else
                {
                    skippedSessionIds.Add(session.Id);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Session {session.Id}: {ex.Message}");
                skippedSessionIds.Add(session.Id);
            }
        }

        if (updatedSessionIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new UpdateSessionsByClassResponse
        {
            UpdatedSessionsCount = updatedSessionIds.Count,
            UpdatedSessionIds = updatedSessionIds,
            SkippedSessionIds = skippedSessionIds,
            Errors = errors
        });
    }
}

