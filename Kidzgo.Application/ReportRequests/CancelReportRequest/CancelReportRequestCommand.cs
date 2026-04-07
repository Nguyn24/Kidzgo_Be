using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;

namespace Kidzgo.Application.ReportRequests.CancelReportRequest;

public sealed record CancelReportRequestCommand(Guid Id) : ICommand<ReportRequestDto>;
