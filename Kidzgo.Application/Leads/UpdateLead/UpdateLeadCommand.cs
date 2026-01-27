using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.UpdateLead;

public sealed class UpdateLeadCommand : ICommand<UpdateLeadResponse>
{
    public Guid LeadId { get; init; }
    public string? ContactName { get; init; }
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
}

