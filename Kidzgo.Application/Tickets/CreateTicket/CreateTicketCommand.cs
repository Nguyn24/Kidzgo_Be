using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.Tickets.CreateTicket;

public sealed class CreateTicketCommand : ICommand<CreateTicketResponse>
{
    public Guid? OpenedByProfileId { get; init; }
    public Guid BranchId { get; init; }
    public Guid? ClassId { get; init; }
    public TicketCategory Category { get; init; }
    public string Message { get; init; } = null!;
}