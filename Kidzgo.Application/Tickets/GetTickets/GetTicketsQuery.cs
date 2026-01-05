using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.Tickets.GetTickets;

public sealed class GetTicketsQuery : IQuery<GetTicketsResponse>, IPageableQuery
{
    public Guid? BranchId { get; init; }
    public Guid? OpenedByUserId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public TicketStatus? Status { get; init; }
    public TicketCategory? Category { get; init; }
    public Guid? ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

