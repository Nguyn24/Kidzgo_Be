using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.SelfAssignLead;

public sealed class SelfAssignLeadCommand : ICommand<SelfAssignLeadResponse>
{
    public Guid LeadId { get; init; }
}


