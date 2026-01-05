using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Tickets.AssignTicket;

public sealed class AssignTicketCommand : ICommand<AssignTicketResponse>
{
    public Guid Id { get; init; }
    public Guid AssignedToUserId { get; init; }
}

