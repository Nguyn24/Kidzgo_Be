using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.Tickets.AssignTicket;

public sealed class AssignTicketResponse
{
    public Guid Id { get; init; }
    public Guid OpenedByUserId { get; init; }
    public string OpenedByUserName { get; init; } = null!;
    public Guid? OpenedByProfileId { get; init; }
    public string? OpenedByProfileName { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassCode { get; init; }
    public string? ClassTitle { get; init; }
    public TicketCategory Category { get; init; }
    public string Subject { get; init; } = null!;
    public string Message { get; init; } = null!;
    public TicketStatus Status { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

