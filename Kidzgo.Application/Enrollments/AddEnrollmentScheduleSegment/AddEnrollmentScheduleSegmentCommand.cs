using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.AddEnrollmentScheduleSegment;

public sealed class AddEnrollmentScheduleSegmentCommand : ICommand<AddEnrollmentScheduleSegmentResponse>
{
    public Guid EnrollmentId { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public string? SessionSelectionPattern { get; init; }
    public bool ClearSessionSelectionPattern { get; init; }
}
