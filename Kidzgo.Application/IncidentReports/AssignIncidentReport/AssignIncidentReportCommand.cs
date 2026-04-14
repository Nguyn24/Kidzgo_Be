using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;

namespace Kidzgo.Application.IncidentReports.AssignIncidentReport;

public sealed class AssignIncidentReportCommand : ICommand<IncidentReportDto>
{
    public Guid Id { get; init; }
    public Guid AssignedToUserId { get; init; }
}
