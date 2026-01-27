namespace Kidzgo.Application.Leads.GetLeadSLA;

public sealed class GetLeadSLAResponse
{
    public Guid LeadId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? FirstResponseAt { get; init; }
    public TimeSpan? TimeToFirstResponse { get; init; }
    public int SLATargetHours { get; init; } = 24; // Default SLA: 24 hours
    public bool? IsSLACompliant { get; init; }
    public bool IsSLAOverdue { get; init; }
}

