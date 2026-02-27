using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.RejectSessionReport;

public sealed record RejectSessionReportCommand(Guid SessionReportId) : ICommand<RejectSessionReportResponse>;
