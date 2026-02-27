using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.AddSessionReportComment;

public sealed record AddSessionReportCommentCommand(
    Guid SessionReportId,
    string Content
) : ICommand<AddSessionReportCommentResponse>;
