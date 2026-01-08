namespace Kidzgo.Application.Tickets.AddTicketComment;

public sealed class AddTicketCommentResponse
{
    public Guid Id { get; init; }
    public Guid TicketId { get; init; }
    public Guid CommenterUserId { get; init; }
    public string CommenterUserName { get; init; } = null!;
    public Guid? CommenterProfileId { get; init; }
    public string? CommenterProfileName { get; init; }
    public string Message { get; init; } = null!;
    public string? AttachmentUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}