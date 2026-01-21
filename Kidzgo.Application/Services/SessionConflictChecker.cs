using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

/// Service để kiểm tra xung đột phòng/giáo viên khi tạo/cập nhật Session
public sealed class SessionConflictChecker
{
    private readonly IDbContext _context;

    public SessionConflictChecker(IDbContext context)
    {
        _context = context;
    }

    /// Kiểm tra xung đột phòng và giáo viên cho một Session
    public async Task<SessionConflictResult> CheckConflictsAsync(
        Guid sessionId,
        DateTime plannedDatetime,
        int durationMinutes,
        Guid? plannedRoomId,
        Guid? plannedTeacherId,
        Guid? plannedAssistantId,
        CancellationToken cancellationToken = default)
    {
        var sessionStart = plannedDatetime;
        var sessionEnd = sessionStart.AddMinutes(durationMinutes);

        var conflicts = new List<SessionConflict>();

        // Check room conflicts
        if (plannedRoomId.HasValue)
        {
            var roomConflicts = await _context.Sessions
                .Include(s => s.Class)
                .Where(s => s.Id != sessionId &&
                           s.PlannedRoomId == plannedRoomId.Value &&
                           s.Status != SessionStatus.Cancelled &&
                           ((s.PlannedDatetime >= sessionStart && s.PlannedDatetime < sessionEnd) ||
                            (s.ActualDatetime.HasValue && s.ActualDatetime.Value >= sessionStart && s.ActualDatetime.Value < sessionEnd) ||
                            (s.PlannedDatetime <= sessionStart && s.PlannedDatetime.AddMinutes(s.DurationMinutes) > sessionStart)))
                .Select(s => new
                {
                    s.Id,
                    s.ClassId,
                    s.Class.Code,
                    s.Class.Title,
                    s.PlannedDatetime,
                    s.DurationMinutes
                })
                .ToListAsync(cancellationToken);

            foreach (var conflict in roomConflicts)
            {
                conflicts.Add(new SessionConflict
                {
                    Type = ConflictType.Room,
                    SessionId = conflict.Id,
                    ClassId = conflict.ClassId,
                    ClassCode = conflict.Code,
                    ClassTitle = conflict.Title,
                    ConflictDatetime = conflict.PlannedDatetime,
                    DurationMinutes = conflict.DurationMinutes
                });
            }
        }

        // Check teacher conflicts
        if (plannedTeacherId.HasValue)
        {
            var teacherConflicts = await _context.Sessions
                .Include(s => s.Class)
                .Include(s => s.PlannedRoom)
                .Where(s => s.Id != sessionId &&
                           (s.PlannedTeacherId == plannedTeacherId.Value || s.ActualTeacherId == plannedTeacherId.Value) &&
                           s.Status != SessionStatus.Cancelled &&
                           ((s.PlannedDatetime >= sessionStart && s.PlannedDatetime < sessionEnd) ||
                            (s.ActualDatetime.HasValue && s.ActualDatetime.Value >= sessionStart && s.ActualDatetime.Value < sessionEnd) ||
                            (s.PlannedDatetime <= sessionStart && s.PlannedDatetime.AddMinutes(s.DurationMinutes) > sessionStart)))
                .Select(s => new
                {
                    s.Id,
                    s.ClassId,
                    s.Class.Code,
                    s.Class.Title,
                    s.PlannedDatetime,
                    s.DurationMinutes,
                    s.PlannedRoomId,
                    RoomName = s.PlannedRoom != null ? s.PlannedRoom.Name : null
                })
                .ToListAsync(cancellationToken);

            foreach (var conflict in teacherConflicts)
            {
                conflicts.Add(new SessionConflict
                {
                    Type = ConflictType.Teacher,
                    SessionId = conflict.Id,
                    ClassId = conflict.ClassId,
                    ClassCode = conflict.Code,
                    ClassTitle = conflict.Title,
                    ConflictDatetime = conflict.PlannedDatetime,
                    DurationMinutes = conflict.DurationMinutes,
                    RoomId = conflict.PlannedRoomId,
                    RoomName = conflict.RoomName
                });
            }
        }

        // Check assistant conflicts
        if (plannedAssistantId.HasValue)
        {
            var assistantConflicts = await _context.Sessions
                .Include(s => s.Class)
                .Where(s => s.Id != sessionId &&
                           (s.PlannedAssistantId == plannedAssistantId.Value || s.ActualAssistantId == plannedAssistantId.Value) &&
                           s.Status != SessionStatus.Cancelled &&
                           ((s.PlannedDatetime >= sessionStart && s.PlannedDatetime < sessionEnd) ||
                            (s.ActualDatetime.HasValue && s.ActualDatetime.Value >= sessionStart && s.ActualDatetime.Value < sessionEnd) ||
                            (s.PlannedDatetime <= sessionStart && s.PlannedDatetime.AddMinutes(s.DurationMinutes) > sessionStart)))
                .Select(s => new
                {
                    s.Id,
                    s.ClassId,
                    s.Class.Code,
                    s.Class.Title,
                    s.PlannedDatetime,
                    s.DurationMinutes
                })
                .ToListAsync(cancellationToken);

            foreach (var conflict in assistantConflicts)
            {
                conflicts.Add(new SessionConflict
                {
                    Type = ConflictType.Assistant,
                    SessionId = conflict.Id,
                    ClassId = conflict.ClassId,
                    ClassCode = conflict.Code,
                    ClassTitle = conflict.Title,
                    ConflictDatetime = conflict.PlannedDatetime,
                    DurationMinutes = conflict.DurationMinutes
                });
            }
        }

        return new SessionConflictResult
        {
            HasConflicts = conflicts.Count > 0,
            Conflicts = conflicts
        };
    }

    /// Gợi ý các phòng/slot khác khi có xung đột
    public async Task<ConflictSuggestions> GetSuggestionsAsync(
        Guid branchId,
        DateTime plannedDatetime,
        int durationMinutes,
        Guid? excludeRoomId,
        Guid? excludeTeacherId,
        CancellationToken cancellationToken = default)
    {
        var sessionStart = plannedDatetime;
        var sessionEnd = sessionStart.AddMinutes(durationMinutes);

        // Tìm các phòng available trong branch
        var availableRooms = await _context.Classrooms
            .Where(c => c.BranchId == branchId &&
                       c.IsActive &&
                       (!excludeRoomId.HasValue || c.Id != excludeRoomId.Value))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Capacity
            })
            .ToListAsync(cancellationToken);

        var suggestedRooms = new List<SuggestedRoom>();
        foreach (var room in availableRooms)
        {
            // Check xem phòng có available trong slot này không
            var hasConflict = await _context.Sessions
                .AnyAsync(s => s.PlannedRoomId == room.Id &&
                              s.Status != SessionStatus.Cancelled &&
                              ((s.PlannedDatetime >= sessionStart && s.PlannedDatetime < sessionEnd) ||
                               (s.ActualDatetime.HasValue && s.ActualDatetime.Value >= sessionStart && s.ActualDatetime.Value < sessionEnd) ||
                               (s.PlannedDatetime <= sessionStart && s.PlannedDatetime.AddMinutes(s.DurationMinutes) > sessionStart)),
                              cancellationToken);

            if (!hasConflict)
            {
                suggestedRooms.Add(new SuggestedRoom
                {
                    Id = room.Id,
                    Name = room.Name,
                    Capacity = room.Capacity
                });
            }
        }

        // Tìm các slot thay thế (trước/sau 30 phút)
        var alternativeSlots = new List<DateTime>
        {
            sessionStart.AddMinutes(-30),
            sessionStart.AddMinutes(30),
            sessionStart.AddMinutes(-60),
            sessionStart.AddMinutes(60)
        };

        return new ConflictSuggestions
        {
            SuggestedRooms = suggestedRooms,
            AlternativeSlots = alternativeSlots.OrderBy(dt => dt).ToList()
        };
    }
}

public sealed class SessionConflictResult
{
    public bool HasConflicts { get; init; }
    public List<SessionConflict> Conflicts { get; init; } = new();
}

public sealed class SessionConflict
{
    public ConflictType Type { get; init; }
    public Guid SessionId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime ConflictDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
}

public enum ConflictType
{
    Room,
    Teacher,
    Assistant
}

public sealed class ConflictSuggestions
{
    public List<SuggestedRoom> SuggestedRooms { get; init; } = new();
    public List<DateTime> AlternativeSlots { get; init; } = new();
}

public sealed class SuggestedRoom
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
}

