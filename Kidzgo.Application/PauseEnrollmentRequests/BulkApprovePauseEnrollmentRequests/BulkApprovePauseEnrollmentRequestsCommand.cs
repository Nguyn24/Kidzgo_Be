using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests.BulkApprovePauseEnrollmentRequests;

namespace Kidzgo.Application.PauseEnrollmentRequests.BulkApprovePauseEnrollmentRequests;

public sealed class BulkApprovePauseEnrollmentRequestsCommand
    : ICommand<BulkApprovePauseEnrollmentRequestsResponse>
{
    public IReadOnlyList<Guid> Ids { get; init; } = Array.Empty<Guid>();
}
