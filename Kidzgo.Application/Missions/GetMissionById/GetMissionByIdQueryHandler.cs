using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.GetMissionById;

public sealed class GetMissionByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetMissionByIdQuery, GetMissionByIdResponse>
{
    public async Task<Result<GetMissionByIdResponse>> Handle(
        GetMissionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var mission = await context.Missions
            .Include(m => m.TargetClass)
            .Include(m => m.CreatedByUser)
            .FirstOrDefaultAsync(m => m.Id == query.Id, cancellationToken);

        if (mission is null)
        {
            return Result.Failure<GetMissionByIdResponse>(
                MissionErrors.NotFound(query.Id));
        }

        return new GetMissionByIdResponse
        {
            Id = mission.Id,
            Title = mission.Title,
            Description = mission.Description,
            Scope = mission.Scope.ToString(),
            TargetClassId = mission.TargetClassId,
            TargetClassCode = mission.TargetClass != null ? mission.TargetClass.Code : null,
            TargetClassTitle = mission.TargetClass != null ? mission.TargetClass.Title : null,
            TargetGroup = mission.TargetGroup,
            MissionType = mission.MissionType.ToString(),
            StartAt = mission.StartAt,
            EndAt = mission.EndAt,
            RewardStars = mission.RewardStars,
            RewardExp = mission.RewardExp,
            CreatedBy = mission.CreatedBy,
            CreatedByName = mission.CreatedByUser != null ? mission.CreatedByUser.Name : null,
            CreatedAt = mission.CreatedAt
        };
    }
}

