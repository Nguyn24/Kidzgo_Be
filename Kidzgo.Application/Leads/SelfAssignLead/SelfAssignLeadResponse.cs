namespace Kidzgo.Application.Leads.SelfAssignLead;

public sealed class SelfAssignLeadResponse
{
    public Guid LeadId { get; init; }
    public Guid OwnerStaffId { get; init; }
    public string OwnerStaffName { get; init; } = null!;
}


