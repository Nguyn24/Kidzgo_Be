using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.UpdateMission;

public sealed class UpdateMissionCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateMissionCommand, UpdateMissionResponse>
{
    public async Task<Result<UpdateMissionResponse>> Handle(
        UpdateMissionCommand command,
        CancellationToken cancellationToken)
    {
        var mission = await context.Missions
            .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

        if (mission is null)
        {
            return Result.Failure<UpdateMissionResponse>(
                MissionErrors.NotFound(command.Id));
        }

        // Validate scope requirements
        if (command.Scope == Domain.Gamification.MissionScope.Class && !command.TargetClassId.HasValue)
        {
            return Result.Failure<UpdateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        if (command.Scope == Domain.Gamification.MissionScope.Group && string.IsNullOrWhiteSpace(command.TargetGroup))
        {
            return Result.Failure<UpdateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        // Validate date range
        if (command.StartAt.HasValue && command.EndAt.HasValue && command.EndAt.Value <= command.StartAt.Value)
        {
            return Result.Failure<UpdateMissionResponse>(
                MissionErrors.InvalidDateRange);
        }

        // Verify class exists if scope is Class
        if (command.Scope == Domain.Gamification.MissionScope.Class && command.TargetClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.TargetClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<UpdateMissionResponse>(
                    MissionErrors.ClassNotFound);
            }
        }

        // Convert DateTime to UTC if provided
        DateTime? startAtUtc = command.StartAt.HasValue
            ? command.StartAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.StartAt.Value, DateTimeKind.Utc)
                : command.StartAt.Value.ToUniversalTime()
            : null;

        DateTime? endAtUtc = command.EndAt.HasValue
            ? command.EndAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.EndAt.Value, DateTimeKind.Utc)
                : command.EndAt.Value.ToUniversalTime()
            : null;

        // Convert empty string to null for TargetGroup (jsonb field)
        string? targetGroup = string.IsNullOrWhiteSpace(command.TargetGroup) ? null : command.TargetGroup;

        // Update mission
        mission.Title = command.Title;
        mission.Description = command.Description;
        mission.Scope = command.Scope;
        mission.TargetClassId = command.TargetClassId;
        mission.TargetGroup = targetGroup;
        mission.MissionType = command.MissionType;
        mission.StartAt = startAtUtc;
        mission.EndAt = endAtUtc;
        mission.RewardStars = command.RewardStars;
        mission.RewardExp = command.RewardExp;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateMissionResponse
        {
            Id = mission.Id,
            Title = mission.Title,
            Description = mission.Description,
            Scope = mission.Scope.ToString(),
            TargetClassId = mission.TargetClassId,
            TargetGroup = mission.TargetGroup,
            MissionType = mission.MissionType.ToString(),
            StartAt = mission.StartAt,
            EndAt = mission.EndAt,
            RewardStars = mission.RewardStars,
            RewardExp = mission.RewardExp
        };
    }
}

