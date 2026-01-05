using Kidzgo.Domain.Tickets;

namespace Kidzgo.API.Requests;

public sealed class CreateTicketRequest
{
    public Guid OpenedByUserId { get; set; }
    public Guid? OpenedByProfileId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public TicketCategory Category { get; set; }
    public string Message { get; set; } = null!;
}

