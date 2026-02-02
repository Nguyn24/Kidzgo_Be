namespace Kidzgo.API.Requests;

public sealed class UpdateLeadChildRequest
{
    public string? ChildName { get; set; }
    public DateTime? Dob { get; set; }
    public string? Gender { get; set; }
    public string? ProgramInterest { get; set; }
    public string? Notes { get; set; }
}

