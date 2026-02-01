using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.GetLeadById;

public sealed class GetLeadByIdQuery : IQuery<GetLeadByIdResponse>
{
    public Guid LeadId { get; init; }
}

