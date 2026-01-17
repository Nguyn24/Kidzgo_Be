using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.GetSessions;

public sealed class GetSessionsQuery : IQuery<GetSessionsResponse>, IPageableQuery
{
    public Guid? ClassId { get; init; }
    public Guid? BranchId { get; init; }
    public SessionStatus? Status { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}



