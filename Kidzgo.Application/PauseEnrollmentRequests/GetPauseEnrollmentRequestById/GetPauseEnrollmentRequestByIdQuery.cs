using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests;

namespace Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequestById;

public sealed class GetPauseEnrollmentRequestByIdQuery : IQuery<PauseEnrollmentRequestResponse>
{
    public Guid Id { get; init; }
}
