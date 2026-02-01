using Kidzgo.Domain.CRM;

namespace Kidzgo.API.Requests;

public sealed class CreateLeadRequest
{
    public LeadSource Source { get; set; }
    public string? Campaign { get; set; }
    public string ContactName { get; set; } = null!;
    public string? ChildName { get; set; }
    public DateTime? ChildDateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? ZaloId { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Subject { get; set; }
    public Guid? BranchPreference { get; set; }
    public string? ProgramInterest { get; set; }
    public string? Notes { get; set; }
    public Guid? OwnerStaffId { get; set; }
}

