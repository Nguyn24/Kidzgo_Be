using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.DeleteLeadChild;

public sealed class DeleteLeadChildCommand : ICommand<DeleteLeadChildResponse>
{
    public Guid LeadId { get; init; }
    public Guid ChildId { get; init; }
}

