using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests;

namespace Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;

public sealed class CreatePauseEnrollmentRequestCommand : ICommand<PauseEnrollmentRequestResponse>
{
    public Guid StudentProfileId { get; init; }
    public Guid ClassId { get; init; }
    public DateOnly PauseFrom { get; init; }
    public DateOnly PauseTo { get; init; }
    public string? Reason { get; init; }
}
