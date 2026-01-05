using Kidzgo.Domain.Tickets;

namespace Kidzgo.API.Requests;

public sealed class UpdateTicketStatusRequest
{
    public TicketStatus Status { get; set; }
}

