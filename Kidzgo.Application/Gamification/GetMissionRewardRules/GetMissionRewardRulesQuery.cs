using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Gamification.GetMissionRewardRules;

public sealed class GetMissionRewardRulesQuery : IQuery<GetMissionRewardRulesResponse>
{
    public MissionType? MissionType { get; init; }
    public MissionProgressMode? ProgressMode { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
