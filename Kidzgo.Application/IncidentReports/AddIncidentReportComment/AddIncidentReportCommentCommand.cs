using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.IncidentReports.AddIncidentReportComment;

public sealed class AddIncidentReportCommentCommand : ICommand<IncidentReportCommentDto>
{
    public Guid TicketId { get; init; }
    public string Message { get; init; } = null!;
    public string? AttachmentUrl { get; init; }
    public IncidentReportCommentType CommentType { get; init; }
}
