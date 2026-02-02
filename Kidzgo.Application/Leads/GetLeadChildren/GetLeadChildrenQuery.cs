using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.GetLeadChildren;

public sealed class GetLeadChildrenQuery : IQuery<GetLeadChildrenResponse>
{
    public Guid LeadId { get; init; }
}

