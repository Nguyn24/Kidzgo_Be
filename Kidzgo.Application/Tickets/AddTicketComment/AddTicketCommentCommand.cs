using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Tickets.AddTicketComment;

public sealed class AddTicketCommentCommand : ICommand<AddTicketCommentResponse>
{
    public Guid TicketId { get; init; }
    public Guid CommenterUserId { get; init; }
    public Guid? CommenterProfileId { get; init; }
    public string Message { get; init; } = null!;
    public string? AttachmentUrl { get; init; }
}

