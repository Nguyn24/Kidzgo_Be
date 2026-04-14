using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;

namespace Kidzgo.Application.IncidentReports.GetIncidentReportById;

public sealed record GetIncidentReportByIdQuery(Guid Id) : IQuery<IncidentReportDetailDto>;
