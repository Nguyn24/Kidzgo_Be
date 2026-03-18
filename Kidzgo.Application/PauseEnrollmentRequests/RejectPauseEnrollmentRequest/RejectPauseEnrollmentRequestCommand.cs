using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PauseEnrollmentRequests.RejectPauseEnrollmentRequest;

public sealed class RejectPauseEnrollmentRequestCommand : ICommand
{
    public Guid Id { get; init; }
}
