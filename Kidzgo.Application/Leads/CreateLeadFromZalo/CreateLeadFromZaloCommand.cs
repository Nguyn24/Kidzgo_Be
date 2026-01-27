using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Leads.CreateLeadFromZalo;

public sealed class CreateLeadFromZaloCommand : ICommand<CreateLeadFromZaloResponse>
{
    public string? ZaloUserId { get; init; }
    public string? ZaloOAId { get; init; }
    public string? Message { get; init; }
    public string ContactName { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public Guid? BranchPreference { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
}

