using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.DeductXp;

public sealed class DeductXpCommand : ICommand<DeductXpResponse>
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public string? Reason { get; init; }
}

