using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.PublishSessionReport;

public sealed record PublishSessionReportCommand(Guid SessionReportId) : ICommand<PublishSessionReportResponse>;
