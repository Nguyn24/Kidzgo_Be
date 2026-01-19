using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.CompleteSession;

public sealed class CompleteSessionCommand : ICommand
{
    public Guid SessionId { get; init; }
    public DateTime? ActualDatetime { get; init; }
}





