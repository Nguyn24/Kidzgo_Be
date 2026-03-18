namespace Kidzgo.Application.Registrations.CreateRegistration;

public sealed class CreateRegistrationResponse
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid TuitionPlanId { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public DateTime RegistrationDate { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public string Status { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public DateTime CreatedAt { get; init; }
}
