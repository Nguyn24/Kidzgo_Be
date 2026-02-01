using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetStarBalance;

public sealed class GetStarBalanceQuery : IQuery<GetStarBalanceResponse>
{
    public Guid StudentProfileId { get; init; }
}

