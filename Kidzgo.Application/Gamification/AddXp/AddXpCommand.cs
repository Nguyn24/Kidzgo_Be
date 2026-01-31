using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.AddXp;

public sealed class AddXpCommand : ICommand<AddXpResponse>
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public string? Reason { get; init; }
}

