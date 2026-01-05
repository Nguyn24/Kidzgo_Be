using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Tickets.GetTicketById;

public sealed class GetTicketByIdQuery : IQuery<GetTicketByIdResponse>
{
    public Guid Id { get; init; }
}

