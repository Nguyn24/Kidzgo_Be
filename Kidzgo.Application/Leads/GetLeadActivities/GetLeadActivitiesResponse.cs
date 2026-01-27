namespace Kidzgo.Application.Leads.GetLeadActivities;

public sealed class GetLeadActivitiesResponse
{
    public Guid LeadId { get; init; }
    public List<LeadActivityDto> Activities { get; init; } = new();
}

public sealed class LeadActivityDto
{
    public Guid Id { get; init; }
    public string ActivityType { get; init; } = null!;
    public string? Content { get; init; }
    public DateTime? NextActionAt { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
}

