namespace Kidzgo.Application.MonthlyReports.AddReportComment;

public sealed class AddReportCommentResponse
{
    public Guid Id { get; init; }
    public Guid ReportId { get; init; }
    public Guid CommenterId { get; init; }
    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}

