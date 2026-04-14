using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.IncidentReports.CreateIncidentReport;

public sealed class CreateIncidentReportCommand : ICommand<IncidentReportDetailDto>
{
    public Guid BranchId { get; init; }
    public Guid? ClassId { get; init; }
    public IncidentReportCategory Category { get; init; }
    public string Subject { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? EvidenceUrl { get; init; }
}
