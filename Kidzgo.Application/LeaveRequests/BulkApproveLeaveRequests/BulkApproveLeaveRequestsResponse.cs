namespace Kidzgo.Application.LeaveRequests.BulkApproveLeaveRequests;

public sealed class BulkApproveLeaveRequestsResponse
{
    public List<Guid> ApprovedIds { get; init; } = new();
    public List<BulkApproveLeaveRequestError> Errors { get; init; } = new();
}

public sealed class BulkApproveLeaveRequestError
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Message { get; init; } = null!;
}
