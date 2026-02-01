using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.UpdateSessionReport;

public sealed class UpdateSessionReportCommand : ICommand<UpdateSessionReportResponse>
{
    public Guid Id { get; init; }
    public string Feedback { get; init; } = null!;
}

