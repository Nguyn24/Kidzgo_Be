using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.AssignLead;

public sealed class AssignLeadCommand : ICommand<AssignLeadResponse>
{
    public Guid LeadId { get; init; }
    public Guid OwnerStaffId { get; init; }
}

