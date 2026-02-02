namespace Kidzgo.Application.Leads.CreateLead;

public sealed class CreateLeadChildDto
{
    public string ChildName { get; init; } = null!;
    public DateTime? Dob { get; init; }
    public string? Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
}

