using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.CancelSession;

public sealed class CancelSessionCommand : ICommand
{
    public Guid SessionId { get; init; }
}



