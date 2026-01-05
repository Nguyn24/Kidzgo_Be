using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.Tickets.GetTicketHistory;

public sealed class GetTicketHistoryResponse
{
    public Guid TicketId { get; init; }
    public TicketStatus TicketStatus { get; init; }
    public List<TicketHistoryItemDto> History { get; init; } = new();
}

public sealed class TicketHistoryItemDto
{
    public string Type { get; init; } = null!; // Created, Comment, StatusChanged, Assigned
    public Guid? CommentId { get; init; }
    public Guid CommenterUserId { get; init; }
    public string CommenterUserName { get; init; } = null!;
    public Guid? CommenterProfileId { get; init; }
    public string? CommenterProfileName { get; init; }
    public string Message { get; init; } = null!;
    public string? AttachmentUrl { get; init; }
    public DateTime Timestamp { get; init; }
}

