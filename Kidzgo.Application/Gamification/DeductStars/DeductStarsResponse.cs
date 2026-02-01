namespace Kidzgo.Application.Gamification.DeductStars;

public sealed class DeductStarsResponse
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public int NewBalance { get; init; }
    public Guid TransactionId { get; init; }
}

