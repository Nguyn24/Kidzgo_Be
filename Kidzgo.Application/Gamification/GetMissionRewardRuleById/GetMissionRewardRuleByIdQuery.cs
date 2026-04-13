using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetMissionRewardRuleById;

public sealed class GetMissionRewardRuleByIdQuery : IQuery<GetMissionRewardRuleByIdResponse>
{
    public Guid Id { get; init; }
}
