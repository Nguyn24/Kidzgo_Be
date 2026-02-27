namespace Kidzgo.Application.SessionReports.AddSessionReportComment;

public sealed class AddSessionReportCommentResponse
{
    public Guid Id { get; init; }
    public Guid? SessionReportId { get; init; }
    public Guid CommenterId { get; init; }
    public string CommenterName { get; init; } = null!;
    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}
