using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Tickets.GetTicketSLA;

public sealed class GetTicketSLAQuery : IQuery<GetTicketSLAResponse>
{
    public Guid TicketId { get; init; }
}

