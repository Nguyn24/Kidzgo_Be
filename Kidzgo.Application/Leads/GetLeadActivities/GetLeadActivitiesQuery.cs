using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.GetLeadActivities;

public sealed class GetLeadActivitiesQuery : IQuery<GetLeadActivitiesResponse>
{
    public Guid LeadId { get; init; }
}

