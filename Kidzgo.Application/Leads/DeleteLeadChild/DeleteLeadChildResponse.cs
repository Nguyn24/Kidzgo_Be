namespace Kidzgo.Application.Leads.DeleteLeadChild;

public sealed class DeleteLeadChildResponse
{
    public Guid Id { get; init; }
    public Guid LeadId { get; init; }
    public bool Success { get; init; }
}

