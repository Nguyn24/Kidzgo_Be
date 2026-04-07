using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;

namespace Kidzgo.Application.ReportRequests.GetReportRequestById;

public sealed record GetReportRequestByIdQuery(Guid Id) : IQuery<ReportRequestDto>;
