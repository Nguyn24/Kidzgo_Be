using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetRewardRedemptionById;

public sealed class GetRewardRedemptionByIdQuery : IQuery<GetRewardRedemptionByIdResponse>
{
    public Guid Id { get; init; }
}

