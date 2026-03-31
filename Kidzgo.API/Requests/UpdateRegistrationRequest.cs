namespace Kidzgo.API.Requests;

public sealed class UpdateRegistrationRequest
{
    public DateTime? ExpectedStartDate { get; set; }
    public string? PreferredSchedule { get; set; }
    public string? Note { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public Guid? SecondaryProgramId { get; set; }
    public string? SecondaryProgramSkillFocus { get; set; }
    public bool RemoveSecondaryProgram { get; set; }
}
