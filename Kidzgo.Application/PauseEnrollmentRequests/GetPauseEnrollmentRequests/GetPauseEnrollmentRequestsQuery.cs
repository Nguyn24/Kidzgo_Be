using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequests;

public sealed class GetPauseEnrollmentRequestsQuery : IPageableQuery, IQuery<Page<PauseEnrollmentRequestResponse>>
{
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public PauseEnrollmentRequestStatus? Status { get; init; }
    public Guid? BranchId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
