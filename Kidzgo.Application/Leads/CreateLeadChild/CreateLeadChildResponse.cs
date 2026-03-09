using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Leads.CreateLeadChild;

public sealed class CreateLeadChildResponse
{
    public Guid Id { get; init; }
    public Guid LeadId { get; init; }
    public string ChildName { get; init; } = null!;
    public DateOnly? Dob { get; init; }
    public string? Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

