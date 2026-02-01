using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SessionReports.GetSessionReportById;

public sealed class GetSessionReportByIdQuery : IQuery<GetSessionReportByIdResponse>
{
    public Guid Id { get; init; }
}

