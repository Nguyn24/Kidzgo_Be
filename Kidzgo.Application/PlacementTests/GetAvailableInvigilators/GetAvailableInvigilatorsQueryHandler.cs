using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PlacementTests;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;

namespace Kidzgo.Application.PlacementTests.GetAvailableInvigilators;

public sealed class GetAvailableInvigilatorsQueryHandler(
    IDbContext context
) : IQueryHandler<GetAvailableInvigilatorsQuery, GetAvailableInvigilatorsResponse>
{
    public async Task<Result<GetAvailableInvigilatorsResponse>> Handle(
        GetAvailableInvigilatorsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.ScheduledAt == default)
        {
            return Result.Failure<GetAvailableInvigilatorsResponse>(
                Error.Validation("PlacementTest.ScheduledAtRequired", "ScheduledAt is required"));
        }

        var duration = PlacementTestScheduleAvailability.NormalizeDuration(query.DurationMinutes);
        if (duration <= 0)
        {
            return Result.Failure<GetAvailableInvigilatorsResponse>(PlacementTestErrors.InvalidDuration);
        }

        var scheduledAtUtc = VietnamTime.NormalizeToUtc(query.ScheduledAt);
        var invigilatorCandidates = await PlacementTestScheduleAvailability.GetInvigilatorCandidatesAsync(
            context,
            scheduledAtUtc,
            duration,
            query.BranchId,
            query.ExcludePlacementTestId,
            cancellationToken);

        var roomCandidates = await PlacementTestScheduleAvailability.GetRoomCandidatesAsync(
            context,
            scheduledAtUtc,
            duration,
            query.BranchId,
            query.ExcludePlacementTestId,
            cancellationToken);

        var items = invigilatorCandidates
            .Where(candidate => query.IncludeUnavailable || candidate.IsAvailable)
            .Select(candidate => new AvailableInvigilatorDto
            {
                UserId = candidate.UserId,
                Name = candidate.Name,
                Email = candidate.Email,
                Role = candidate.Role.ToString(),
                BranchId = candidate.BranchId,
                IsAvailable = candidate.IsAvailable,
                Conflicts = query.IncludeUnavailable ? candidate.Conflicts : new List<PlacementTestScheduleConflict>()
            })
            .ToList();

        var rooms = roomCandidates
            .Where(candidate => query.IncludeUnavailable || candidate.IsAvailable)
            .Select(candidate => new AvailableRoomDto
            {
                RoomId = candidate.RoomId,
                Name = candidate.Name,
                BranchId = candidate.BranchId,
                Capacity = candidate.Capacity,
                IsAvailable = candidate.IsAvailable,
                Conflicts = query.IncludeUnavailable ? candidate.Conflicts : new List<PlacementTestScheduleConflict>()
            })
            .ToList();

        return new GetAvailableInvigilatorsResponse
        {
            ScheduledAt = scheduledAtUtc,
            EndAt = scheduledAtUtc.AddMinutes(duration),
            DurationMinutes = duration,
            Items = items,
            Rooms = rooms
        };
    }
}
