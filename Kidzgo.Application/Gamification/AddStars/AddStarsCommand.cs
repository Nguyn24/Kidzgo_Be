using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.AddStars;

public sealed class AddStarsCommand : ICommand<AddStarsResponse>
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public string? Reason { get; init; }
}

