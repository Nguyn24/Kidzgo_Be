using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GenerateSessionsFromPattern;

public sealed class GenerateSessionsFromPatternCommand : ICommand<GenerateSessionsFromPatternResponse>
{
    public Guid ClassId { get; init; }

    /// Neu true, chi generate cac sessions tu hien tai tro di.
    /// Neu false, generate tat ca tu StartDate cua Class.
    public bool OnlyFutureSessions { get; init; } = true;
}
