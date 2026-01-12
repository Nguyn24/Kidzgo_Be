using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.LeaveRequests.GetLeaveRequests;

public sealed class GetLeaveRequestsQuery : IPageableQuery, IQuery<Page<GetLeaveRequestsResponse>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public LeaveRequestStatus? Status { get; init; }
}

