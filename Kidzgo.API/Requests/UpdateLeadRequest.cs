namespace Kidzgo.API.Requests;

public sealed class UpdateLeadRequest
{
    public string? ContactName { get; set; }
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
}

