using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.ApproveSessionReport;

public sealed record ApproveSessionReportCommand(Guid SessionReportId) : ICommand<ApproveSessionReportResponse>;
