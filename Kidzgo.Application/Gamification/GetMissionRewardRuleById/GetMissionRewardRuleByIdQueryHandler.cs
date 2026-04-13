using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetMissionRewardRuleById;

public sealed class GetMissionRewardRuleByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetMissionRewardRuleByIdQuery, GetMissionRewardRuleByIdResponse>
{
    public async Task<Result<GetMissionRewardRuleByIdResponse>> Handle(
        GetMissionRewardRuleByIdQuery query,
        CancellationToken cancellationToken)
    {
        var rule = await context.MissionRewardRules
            .FirstOrDefaultAsync(rule => rule.Id == query.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<GetMissionRewardRuleByIdResponse>(
                MissionRewardRuleErrors.NotFound(query.Id));
        }

        return Result.Success(new GetMissionRewardRuleByIdResponse
        {
            Id = rule.Id,
            MissionType = rule.MissionType.ToString(),
            ProgressMode = rule.ProgressMode.ToString(),
            TotalRequired = rule.TotalRequired,
            RewardStars = rule.RewardStars,
            RewardExp = rule.RewardExp,
            IsActive = rule.IsActive,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        });
    }
}
