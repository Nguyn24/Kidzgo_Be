using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.CRM;

namespace Kidzgo.Application.Leads.UpdateLeadStatus;

public sealed class UpdateLeadStatusCommand : ICommand<UpdateLeadStatusResponse>
{
    public Guid LeadId { get; init; }
    public LeadStatus Status { get; init; }
}

