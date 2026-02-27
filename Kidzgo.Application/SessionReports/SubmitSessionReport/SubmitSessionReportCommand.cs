using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.SubmitSessionReport;

public sealed record SubmitSessionReportCommand(Guid SessionReportId) : ICommand<SubmitSessionReportResponse>;
