using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.Tickets.UpdateTicketStatus;

public sealed class UpdateTicketStatusCommand : ICommand<UpdateTicketStatusResponse>
{
    public Guid Id { get; init; }
    public TicketStatus Status { get; init; }
}

