using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.AddReportComment;

/// <summary>
/// UC-182: Staff/Admin comment Monthly Report
/// </summary>
public sealed class AddReportCommentCommand : ICommand<AddReportCommentResponse>
{
    public Guid ReportId { get; init; }
    public string Content { get; init; } = null!;
}

