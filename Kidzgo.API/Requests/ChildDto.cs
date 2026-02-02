namespace Kidzgo.API.Requests;

public sealed class ChildDto
{
    public string ChildName { get; set; } = null!;
    public DateTime? Dob { get; set; }
    public string? Gender { get; set; }
    public string? ProgramInterest { get; set; }
    public string? Notes { get; set; }
}

