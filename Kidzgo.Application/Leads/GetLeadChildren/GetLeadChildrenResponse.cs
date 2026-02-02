namespace Kidzgo.Application.Leads.GetLeadChildren;

public sealed class GetLeadChildrenResponse
{
    public List<LeadChildDto> Children { get; init; } = new();
}

public sealed class LeadChildDto
{
    public Guid Id { get; init; }
    public Guid LeadId { get; init; }
    public string ChildName { get; init; } = null!;
    public DateTime? Dob { get; init; }
    public string? Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
    public string Status { get; init; } = null!;
    public Guid? ConvertedStudentProfileId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

