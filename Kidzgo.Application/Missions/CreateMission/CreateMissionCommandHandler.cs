using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.CreateMission;

public sealed class CreateMissionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMissionCommand, CreateMissionResponse>
{
    public async Task<Result<CreateMissionResponse>> Handle(
        CreateMissionCommand command,
        CancellationToken cancellationToken)
    {
        // Validate scope requirements
        if (command.Scope == MissionScope.Class && !command.TargetClassId.HasValue)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        if (command.Scope == MissionScope.Group && string.IsNullOrWhiteSpace(command.TargetGroup))
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        // Validate date range
        if (command.StartAt.HasValue && command.EndAt.HasValue && command.EndAt.Value <= command.StartAt.Value)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidDateRange);
        }

        // Verify class exists if scope is Class
        if (command.Scope == MissionScope.Class && command.TargetClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.TargetClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<CreateMissionResponse>(
                    MissionErrors.ClassNotFound);
            }
        }

        var now = DateTime.UtcNow;
        var userId = userContext.UserId;

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

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            Scope = command.Scope,
            TargetClassId = command.TargetClassId,
            TargetGroup = targetGroup,
            MissionType = command.MissionType,
            StartAt = startAtUtc,
            EndAt = endAtUtc,
            RewardStars = command.RewardStars,
            RewardExp = command.RewardExp,
            CreatedBy = userId,
            CreatedAt = now
        };

        context.Missions.Add(mission);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateMissionResponse
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
            RewardExp = mission.RewardExp,
            CreatedBy = mission.CreatedBy,
            CreatedAt = mission.CreatedAt
        };
    }
}

