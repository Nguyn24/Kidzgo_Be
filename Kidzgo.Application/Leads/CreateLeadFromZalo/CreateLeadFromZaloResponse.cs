namespace Kidzgo.Application.Leads.CreateLeadFromZalo;

public sealed class CreateLeadFromZaloResponse
{
    public Guid Id { get; init; }
    public bool IsNewLead { get; init; }
    public string Message { get; init; } = null!;
}

