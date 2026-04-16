namespace Kidzgo.Application.Classes.AddClassScheduleSegment;

public sealed class AddClassScheduleSegmentResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid ProgramId { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public string SchedulePattern { get; init; } = null!;
    public int GeneratedSessionsCount { get; init; }
}
