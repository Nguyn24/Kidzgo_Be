namespace Kidzgo.API.Requests;

public sealed class UpdateRegistrationRequest
{
    public DateTime? ExpectedStartDate { get; set; }
    public string? PreferredSchedule { get; set; }
    public string? Note { get; set; }
    public Guid? TuitionPlanId { get; set; }
}
