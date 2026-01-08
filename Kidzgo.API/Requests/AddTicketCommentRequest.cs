namespace Kidzgo.API.Requests;

public sealed class AddTicketCommentRequest
{
    public Guid? CommenterProfileId { get; set; }
    public string Message { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
}