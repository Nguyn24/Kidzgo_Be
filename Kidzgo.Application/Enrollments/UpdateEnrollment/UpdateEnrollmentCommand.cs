using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.UpdateEnrollment;

public sealed class UpdateEnrollmentCommand : ICommand<UpdateEnrollmentResponse>
{
    public Guid Id { get; init; }
    public DateOnly? EnrollDate { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? Track { get; init; }
    public string? SessionSelectionPattern { get; init; }
    public bool ClearSessionSelectionPattern { get; init; }
}

