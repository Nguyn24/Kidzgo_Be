using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.AddClassScheduleSegment;

public sealed class AddClassScheduleSegmentCommand : ICommand<AddClassScheduleSegmentResponse>
{
    public Guid ClassId { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public string SchedulePattern { get; init; } = null!;
    public bool GenerateSessions { get; init; } = true;
    public bool OnlyFutureSessions { get; init; } = true;
}
