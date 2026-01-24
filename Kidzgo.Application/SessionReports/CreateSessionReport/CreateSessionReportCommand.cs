using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.CreateSessionReport;

public sealed class CreateSessionReportCommand : ICommand<CreateSessionReportResponse>
{
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public DateOnly ReportDate { get; init; }
    public string Feedback { get; init; } = null!;
}

