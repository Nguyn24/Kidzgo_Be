namespace Kidzgo.Application.Gamification.AddStars;

public sealed class AddStarsResponse
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public int NewBalance { get; init; }
    public Guid TransactionId { get; init; }
}

