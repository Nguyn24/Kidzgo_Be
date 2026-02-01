using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetStarTransactions;

public sealed class GetStarTransactionsQuery : IQuery<GetStarTransactionsResponse>
{
    public Guid StudentProfileId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

