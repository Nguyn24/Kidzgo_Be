using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetMissionRewardRules;

public sealed class GetMissionRewardRulesQueryHandler(IDbContext context)
    : IQueryHandler<GetMissionRewardRulesQuery, GetMissionRewardRulesResponse>
{
    public async Task<Result<GetMissionRewardRulesResponse>> Handle(
        GetMissionRewardRulesQuery query,
        CancellationToken cancellationToken)
    {
        var rulesQuery = context.MissionRewardRules.AsQueryable();

        if (query.MissionType.HasValue)
        {
            rulesQuery = rulesQuery.Where(rule => rule.MissionType == query.MissionType.Value);
        }

        if (query.ProgressMode.HasValue)
        {
            rulesQuery = rulesQuery.Where(rule => rule.ProgressMode == query.ProgressMode.Value);
        }

        if (query.IsActive.HasValue)
        {
            rulesQuery = rulesQuery.Where(rule => rule.IsActive == query.IsActive.Value);
        }

        var totalCount = await rulesQuery.CountAsync(cancellationToken);

        var rules = await rulesQuery
            .OrderBy(rule => rule.MissionType)
            .ThenBy(rule => rule.ProgressMode)
            .ThenBy(rule => rule.TotalRequired)
            .ApplyPagination(query.Page, query.PageSize)
            .Select(rule => new MissionRewardRuleDto
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
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetMissionRewardRulesResponse
        {
            Rules = new Page<MissionRewardRuleDto>(rules, totalCount, query.Page, query.PageSize)
        });
    }
}
