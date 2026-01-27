namespace Kidzgo.Application.Leads.UpdateLead;

public sealed class UpdateLeadResponse
{
    public Guid Id { get; init; }
    public string ContactName { get; init; } = null!;
    public string? ChildName { get; init; }
    public DateTime? ChildDateOfBirth { get; init; }
    public string? Phone { get; init; }
    public string? ZaloId { get; init; }
    public string? Email { get; init; }
    public string? Company { get; init; }
    public string? Subject { get; init; }
    public Guid? BranchPreference { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
    public DateTime UpdatedAt { get; init; }
}

