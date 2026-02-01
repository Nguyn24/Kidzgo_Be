using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.CRM;

namespace Kidzgo.Application.Leads.CreateLead;

public sealed class CreateLeadCommand : ICommand<CreateLeadResponse>
{
    public LeadSource Source { get; init; }
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
    public Guid? OwnerStaffId { get; init; }
}

