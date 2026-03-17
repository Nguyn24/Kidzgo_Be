using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PauseEnrollmentRequests.CancelPauseEnrollmentRequest;

public sealed class CancelPauseEnrollmentRequestCommand : ICommand
{
    public Guid Id { get; init; }
}
