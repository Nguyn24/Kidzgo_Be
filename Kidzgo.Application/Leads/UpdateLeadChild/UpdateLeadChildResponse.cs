namespace Kidzgo.Application.Leads.UpdateLeadChild;

public sealed class UpdateLeadChildResponse
{
    public Guid Id { get; init; }
    public Guid LeadId { get; init; }
    public string ChildName { get; init; } = null!;
    public DateTime? Dob { get; init; }
    public string? Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
    public string Status { get; init; } = null!;
    public DateTime UpdatedAt { get; init; }
}

