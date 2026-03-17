namespace Kidzgo.Application.PauseEnrollmentRequests.BulkApprovePauseEnrollmentRequests;

public sealed class BulkApprovePauseEnrollmentRequestsResponse
{
    public List<Guid> ApprovedIds { get; init; } = new();
    public List<BulkApprovePauseEnrollmentRequestError> Errors { get; init; } = new();
}

public sealed class BulkApprovePauseEnrollmentRequestError
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Message { get; init; } = null!;
}
