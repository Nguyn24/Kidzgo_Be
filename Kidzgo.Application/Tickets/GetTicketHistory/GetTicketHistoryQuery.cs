using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Tickets.GetTicketHistory;

public sealed class GetTicketHistoryQuery : IQuery<GetTicketHistoryResponse>
{
    public Guid TicketId { get; init; }
}

