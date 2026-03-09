using Kidzgo.Domain.Users;

namespace Kidzgo.API.Requests;

public sealed class CreateLeadChildRequest
{
    public string ChildName { get; set; } = null!;
    public DateOnly? Dob { get; set; }
    public Gender Gender { get; set; }
    public string? ProgramInterest { get; set; }
    public string? Notes { get; set; }
}

