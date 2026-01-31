using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.DeductStars;

public sealed class DeductStarsCommand : ICommand<DeductStarsResponse>
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public string? Reason { get; init; }
}

