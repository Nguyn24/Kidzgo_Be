namespace Kidzgo.Application.Gamification.GetStarTransactions;

public sealed class GetStarTransactionsResponse
{
    public List<StarTransactionDto> Transactions { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed class StarTransactionDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public string? Reason { get; init; }
    public string SourceType { get; init; } = null!;
    public Guid? SourceId { get; init; }
    public int BalanceAfter { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
}

