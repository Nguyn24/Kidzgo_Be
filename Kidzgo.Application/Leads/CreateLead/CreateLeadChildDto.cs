using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Leads.CreateLead;

public sealed class CreateLeadChildDto
{
    public string ChildName { get; init; } = null!;
    public DateOnly? Dob { get; init; }
    public Gender Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
}

