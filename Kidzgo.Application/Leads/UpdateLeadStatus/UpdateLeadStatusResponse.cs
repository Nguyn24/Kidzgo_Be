namespace Kidzgo.Application.Leads.UpdateLeadStatus;

public sealed class UpdateLeadStatusResponse
{
    public Guid LeadId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? FirstResponseAt { get; init; }
}

