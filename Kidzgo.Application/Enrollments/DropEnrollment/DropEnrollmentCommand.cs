using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.DropEnrollment;

public sealed class DropEnrollmentCommand : ICommand<DropEnrollmentResponse>
{
    public Guid Id { get; init; }
}

