using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.PauseEnrollment;

public sealed class PauseEnrollmentCommand : ICommand<PauseEnrollmentResponse>
{
    public Guid Id { get; init; }
}

