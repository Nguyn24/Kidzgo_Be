namespace Kidzgo.API.Requests;

public sealed class AddClassScheduleSegmentRequest
{
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string SchedulePattern { get; set; } = null!;
    public bool GenerateSessions { get; set; } = true;
    public bool OnlyFutureSessions { get; set; } = true;
}
