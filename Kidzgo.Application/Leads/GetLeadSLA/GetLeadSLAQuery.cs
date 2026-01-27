using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.GetLeadSLA;

public sealed class GetLeadSLAQuery : IQuery<GetLeadSLAResponse>
{
    public Guid LeadId { get; init; }
}

