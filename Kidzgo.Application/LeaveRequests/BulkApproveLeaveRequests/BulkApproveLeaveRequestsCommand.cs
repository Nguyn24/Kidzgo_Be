using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LeaveRequests.BulkApproveLeaveRequests;

public sealed class BulkApproveLeaveRequestsCommand
    : ICommand<BulkApproveLeaveRequestsResponse>
{
    public IReadOnlyList<Guid> Ids { get; init; } = Array.Empty<Guid>();
}
