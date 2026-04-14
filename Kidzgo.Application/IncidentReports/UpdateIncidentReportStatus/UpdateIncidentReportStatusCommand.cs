using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.IncidentReports.UpdateIncidentReportStatus;

public sealed class UpdateIncidentReportStatusCommand : ICommand<IncidentReportDto>
{
    public Guid Id { get; init; }
    public IncidentReportStatus Status { get; init; }
}
