using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.CreateLeadChild;

public sealed class CreateLeadChildCommand : ICommand<CreateLeadChildResponse>
{
    public Guid LeadId { get; init; }
    public string ChildName { get; init; } = null!;
    public DateTime? Dob { get; init; }
    public string? Gender { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
}

