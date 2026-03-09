using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Leads.CreateLeadChild;

public sealed class CreateLeadChildCommand : ICommand<CreateLeadChildResponse>
{
    public Guid LeadId { get; init; }
    public string ChildName { get; init; } = null!;
    public DateOnly? Dob { get; init; }
    public Gender Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
}

