using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.IncidentReports.Shared;

internal static class IncidentReportStateMapper
{
    public static TicketStatus ToTicketStatus(IncidentReportStatus status)
    {
        return status switch
        {
            IncidentReportStatus.Open => TicketStatus.Open,
            IncidentReportStatus.InProgress => TicketStatus.InProgress,
            IncidentReportStatus.Resolved => TicketStatus.Resolved,
            IncidentReportStatus.Closed => TicketStatus.Closed,
            IncidentReportStatus.Rejected => TicketStatus.Closed,
            _ => TicketStatus.Open
        };
    }
}
