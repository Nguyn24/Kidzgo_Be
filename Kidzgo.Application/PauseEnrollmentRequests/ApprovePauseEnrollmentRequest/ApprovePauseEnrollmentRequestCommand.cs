using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PauseEnrollmentRequests.ApprovePauseEnrollmentRequest;

public sealed class ApprovePauseEnrollmentRequestCommand : ICommand
{
    public Guid Id { get; init; }
}
