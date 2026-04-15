using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests;

internal static class PlacementTestScheduleAvailability
{
    public const int DefaultDurationMinutes = 60;

    private static readonly UserRole[] EligibleInvigilatorRoles =
    [
        UserRole.Admin,
        UserRole.ManagementStaff,
        UserRole.AccountantStaff,
        UserRole.Teacher
    ];

    public static int NormalizeDuration(int? durationMinutes)
        => durationMinutes.GetValueOrDefault(DefaultDurationMinutes);

    public static async Task<Result> EnsureScheduleAvailableAsync(
        IDbContext context,
        Guid invigilatorUserId,
        Guid roomId,
        DateTime scheduledAt,
        int? durationMinutes,
        Guid? excludePlacementTestId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        var invigilatorResult = await EnsureInvigilatorAvailableAsync(
            context,
            invigilatorUserId,
            scheduledAt,
            durationMinutes,
            excludePlacementTestId,
            cancellationToken);

        if (invigilatorResult.IsFailure)
        {
            return invigilatorResult;
        }

        return await EnsureRoomAvailableAsync(
            context,
            roomId,
            scheduledAt,
            durationMinutes,
            excludePlacementTestId,
            branchId,
            cancellationToken);
    }

    public static async Task<Result> EnsureInvigilatorAssignableAsync(
        IDbContext context,
        Guid invigilatorUserId,
        CancellationToken cancellationToken)
    {
        var invigilator = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == invigilatorUserId && !u.IsDeleted && u.IsActive)
            .Select(u => new
            {
                u.Id,
                u.Role
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (invigilator is null)
        {
            return Result.Failure(PlacementTestErrors.InvigilatorNotFound(invigilatorUserId));
        }

        return EligibleInvigilatorRoles.Contains(invigilator.Role)
            ? Result.Success()
            : Result.Failure(PlacementTestErrors.InvigilatorInvalidRole(invigilatorUserId));
    }

    public static async Task<Result> EnsureRoomAssignableAsync(
        IDbContext context,
        Guid roomId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        var room = await context.Classrooms
            .AsNoTracking()
            .Where(r => r.Id == roomId && r.IsActive)
            .Select(r => new
            {
                r.Id,
                r.BranchId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (room is null)
        {
            return Result.Failure(PlacementTestErrors.RoomNotFound(roomId));
        }

        if (branchId.HasValue && room.BranchId != branchId.Value)
        {
            return Result.Failure(PlacementTestErrors.RoomBranchMismatch(roomId, branchId));
        }

        return Result.Success();
    }

    public static async Task<Result> EnsureInvigilatorAvailableAsync(
        IDbContext context,
        Guid invigilatorUserId,
        DateTime scheduledAt,
        int? durationMinutes,
        Guid? excludePlacementTestId,
        CancellationToken cancellationToken)
    {
        var duration = NormalizeDuration(durationMinutes);
        if (duration <= 0)
        {
            return Result.Failure(PlacementTestErrors.InvalidDuration);
        }

        var assignable = await EnsureInvigilatorAssignableAsync(context, invigilatorUserId, cancellationToken);
        if (assignable.IsFailure)
        {
            return assignable;
        }

        var conflicts = await GetInvigilatorConflictsAsync(
            context,
            invigilatorUserId,
            scheduledAt,
            duration,
            excludePlacementTestId,
            cancellationToken);

        return conflicts.Count == 0
            ? Result.Success()
            : Result.Failure(PlacementTestErrors.InvigilatorUnavailable(invigilatorUserId));
    }

    public static async Task<Result> EnsureRoomAvailableAsync(
        IDbContext context,
        Guid roomId,
        DateTime scheduledAt,
        int? durationMinutes,
        Guid? excludePlacementTestId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        var duration = NormalizeDuration(durationMinutes);
        if (duration <= 0)
        {
            return Result.Failure(PlacementTestErrors.InvalidDuration);
        }

        var assignable = await EnsureRoomAssignableAsync(context, roomId, branchId, cancellationToken);
        if (assignable.IsFailure)
        {
            return assignable;
        }

        var conflicts = await GetRoomConflictsAsync(
            context,
            roomId,
            scheduledAt,
            duration,
            excludePlacementTestId,
            cancellationToken);

        return conflicts.Count == 0
            ? Result.Success()
            : Result.Failure(PlacementTestErrors.RoomUnavailable(roomId));
    }

    public static async Task<List<InvigilatorAvailabilityCandidate>> GetInvigilatorCandidatesAsync(
        IDbContext context,
        DateTime scheduledAt,
        int? durationMinutes,
        Guid? branchId,
        Guid? excludePlacementTestId,
        CancellationToken cancellationToken)
    {
        var duration = NormalizeDuration(durationMinutes);
        if (duration <= 0)
        {
            return [];
        }

        var candidates = await context.Users
            .AsNoTracking()
            .Where(u =>
                u.IsActive &&
                !u.IsDeleted &&
                EligibleInvigilatorRoles.Contains(u.Role) &&
                (!branchId.HasValue || u.BranchId == branchId.Value))
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Name)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.BranchId
            })
            .ToListAsync(cancellationToken);

        var result = new List<InvigilatorAvailabilityCandidate>();

        foreach (var candidate in candidates)
        {
            var conflicts = await GetInvigilatorConflictsAsync(
                context,
                candidate.Id,
                scheduledAt,
                duration,
                excludePlacementTestId,
                cancellationToken);

            result.Add(new InvigilatorAvailabilityCandidate
            {
                UserId = candidate.Id,
                Name = candidate.Name,
                Email = candidate.Email,
                Role = candidate.Role,
                BranchId = candidate.BranchId,
                IsAvailable = conflicts.Count == 0,
                Conflicts = conflicts
            });
        }

        return result;
    }

    public static async Task<List<RoomAvailabilityCandidate>> GetRoomCandidatesAsync(
        IDbContext context,
        DateTime scheduledAt,
        int? durationMinutes,
        Guid? branchId,
        Guid? excludePlacementTestId,
        CancellationToken cancellationToken)
    {
        var duration = NormalizeDuration(durationMinutes);
        if (duration <= 0)
        {
            return [];
        }

        var candidates = await context.Classrooms
            .AsNoTracking()
            .Where(r =>
                r.IsActive &&
                (!branchId.HasValue || r.BranchId == branchId.Value))
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.BranchId,
                r.Capacity
            })
            .ToListAsync(cancellationToken);

        var result = new List<RoomAvailabilityCandidate>();

        foreach (var candidate in candidates)
        {
            var conflicts = await GetRoomConflictsAsync(
                context,
                candidate.Id,
                scheduledAt,
                duration,
                excludePlacementTestId,
                cancellationToken);

            result.Add(new RoomAvailabilityCandidate
            {
                RoomId = candidate.Id,
                Name = candidate.Name,
                BranchId = candidate.BranchId,
                Capacity = candidate.Capacity,
                IsAvailable = conflicts.Count == 0,
                Conflicts = conflicts
            });
        }

        return result;
    }

    public static async Task<List<PlacementTestScheduleConflict>> GetInvigilatorConflictsAsync(
        IDbContext context,
        Guid invigilatorUserId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludePlacementTestId,
        CancellationToken cancellationToken)
    {
        var start = VietnamTime.NormalizeToUtc(scheduledAt);
        var end = start.AddMinutes(durationMinutes);

        var placementTestConflicts = await context.PlacementTests
            .AsNoTracking()
            .Where(pt =>
                pt.Id != excludePlacementTestId &&
                pt.InvigilatorUserId == invigilatorUserId &&
                pt.ScheduledAt.HasValue &&
                pt.Status == PlacementTestStatus.Scheduled &&
                pt.ScheduledAt.Value < end &&
                pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes) > start)
            .Select(pt => new PlacementTestScheduleConflict
            {
                Type = "PlacementTest",
                ReferenceId = pt.Id,
                Title = "Placement test",
                StartAt = pt.ScheduledAt!.Value,
                EndAt = pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        var sessionConflicts = await context.Sessions
            .AsNoTracking()
            .Where(s =>
                s.Status != SessionStatus.Cancelled &&
                (s.PlannedTeacherId == invigilatorUserId ||
                 s.ActualTeacherId == invigilatorUserId ||
                 s.PlannedAssistantId == invigilatorUserId ||
                 s.ActualAssistantId == invigilatorUserId) &&
                (s.ActualDatetime ?? s.PlannedDatetime) < end &&
                (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes) > start)
            .Select(s => new PlacementTestScheduleConflict
            {
                Type = "TeachingSession",
                ReferenceId = s.Id,
                Title = s.Class.Code + " - " + s.Class.Title,
                StartAt = s.ActualDatetime ?? s.PlannedDatetime,
                EndAt = (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        return placementTestConflicts
            .Concat(sessionConflicts)
            .OrderBy(c => c.StartAt)
            .ToList();
    }

    public static async Task<List<PlacementTestScheduleConflict>> GetRoomConflictsAsync(
        IDbContext context,
        Guid roomId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludePlacementTestId,
        CancellationToken cancellationToken)
    {
        var start = VietnamTime.NormalizeToUtc(scheduledAt);
        var end = start.AddMinutes(durationMinutes);

        var roomName = await context.Classrooms
            .AsNoTracking()
            .Where(r => r.Id == roomId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var normalizedRoomName = roomName == null ? null : roomName.Trim().ToLower();

        var placementTestConflicts = await context.PlacementTests
            .AsNoTracking()
            .Where(pt =>
                pt.Id != excludePlacementTestId &&
                pt.ScheduledAt.HasValue &&
                pt.Status == PlacementTestStatus.Scheduled &&
                pt.ScheduledAt.Value < end &&
                pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes) > start &&
                (pt.RoomId == roomId ||
                 (pt.RoomId == null &&
                  normalizedRoomName != null &&
                  pt.Room != null &&
                  pt.Room.Trim().ToLower() == normalizedRoomName)))
            .Select(pt => new PlacementTestScheduleConflict
            {
                Type = "PlacementTest",
                ReferenceId = pt.Id,
                Title = "Placement test",
                StartAt = pt.ScheduledAt!.Value,
                EndAt = pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        var sessionConflicts = await context.Sessions
            .AsNoTracking()
            .Where(s =>
                s.Status != SessionStatus.Cancelled &&
                (s.PlannedRoomId == roomId || s.ActualRoomId == roomId) &&
                (s.ActualDatetime ?? s.PlannedDatetime) < end &&
                (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes) > start)
            .Select(s => new PlacementTestScheduleConflict
            {
                Type = "TeachingSession",
                ReferenceId = s.Id,
                Title = s.Class.Code + " - " + s.Class.Title,
                StartAt = s.ActualDatetime ?? s.PlannedDatetime,
                EndAt = (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        return placementTestConflicts
            .Concat(sessionConflicts)
            .OrderBy(c => c.StartAt)
            .ToList();
    }
}

internal sealed class InvigilatorAvailabilityCandidate
{
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public UserRole Role { get; init; }
    public Guid? BranchId { get; init; }
    public bool IsAvailable { get; init; }
    public List<PlacementTestScheduleConflict> Conflicts { get; init; } = new();
}

internal sealed class RoomAvailabilityCandidate
{
    public Guid RoomId { get; init; }
    public string Name { get; init; } = null!;
    public Guid BranchId { get; init; }
    public int Capacity { get; init; }
    public bool IsAvailable { get; init; }
    public List<PlacementTestScheduleConflict> Conflicts { get; init; } = new();
}

public sealed class PlacementTestScheduleConflict
{
    public string Type { get; init; } = null!;
    public Guid ReferenceId { get; init; }
    public string Title { get; init; } = null!;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
}
