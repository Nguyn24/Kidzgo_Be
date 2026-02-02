using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.UpdateLeadChild;

public sealed class UpdateLeadChildCommand : ICommand<UpdateLeadChildResponse>
{
    public Guid LeadId { get; init; }
    public Guid ChildId { get; init; }
    public string? ChildName { get; init; }
    public DateTime? Dob { get; init; }
    public string? Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
}

