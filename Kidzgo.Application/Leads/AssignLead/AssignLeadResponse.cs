namespace Kidzgo.Application.Leads.AssignLead;

public sealed class AssignLeadResponse
{
    public Guid LeadId { get; init; }
    public Guid OwnerStaffId { get; init; }
    public string? OwnerStaffName { get; init; }
}

