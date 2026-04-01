namespace Kidzgo.Application.Registrations.UpdateRegistration;

public sealed class UpdateRegistrationResponse
{
    public Guid Id { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? TuitionPlanName { get; init; }
    public Guid? SecondaryProgramId { get; init; }
    public string? SecondaryProgramName { get; init; }
    public string? SecondaryProgramSkillFocus { get; init; }
    public DateTime UpdatedAt { get; init; }
}
