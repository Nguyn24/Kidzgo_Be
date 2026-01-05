using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.ReactivateEnrollment;

public sealed class ReactivateEnrollmentCommand : ICommand<ReactivateEnrollmentResponse>
{
    public Guid Id { get; init; }
}

