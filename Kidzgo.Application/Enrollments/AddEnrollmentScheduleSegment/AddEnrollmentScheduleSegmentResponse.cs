namespace Kidzgo.Application.Enrollments.AddEnrollmentScheduleSegment;

public sealed class AddEnrollmentScheduleSegmentResponse
{
    public Guid Id { get; init; }
    public Guid EnrollmentId { get; init; }
    public Guid ClassId { get; init; }
    public Guid ProgramId { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public string? SessionSelectionPattern { get; init; }
    public string? ActiveSessionSelectionPattern { get; init; }
}
