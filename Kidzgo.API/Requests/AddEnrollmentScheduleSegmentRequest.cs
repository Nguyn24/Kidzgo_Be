namespace Kidzgo.API.Requests;

public sealed class AddEnrollmentScheduleSegmentRequest
{
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? SessionSelectionPattern { get; set; }
    public bool ClearSessionSelectionPattern { get; set; }
}
