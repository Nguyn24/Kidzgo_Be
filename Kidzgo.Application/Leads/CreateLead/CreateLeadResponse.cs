using Kidzgo.Domain.CRM;

namespace Kidzgo.Application.Leads.CreateLead;

public sealed class CreateLeadResponse
{
    public Guid Id { get; init; }
    public string Source { get; init; } = null!;
    public string? Campaign { get; init; }
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
    public string Status { get; init; } = null!;
    public Guid? OwnerStaffId { get; init; }
    public DateTime CreatedAt { get; init; }
}

