using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.CreateEnrollment;

public sealed class CreateEnrollmentCommand : ICommand<CreateEnrollmentResponse>
{
    public Guid ClassId { get; init; }
    public Guid StudentProfileId { get; init; }
    public DateOnly EnrollDate { get; init; }
    public Guid? TuitionPlanId { get; init; }
}

