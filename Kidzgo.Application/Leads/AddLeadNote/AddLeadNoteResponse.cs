namespace Kidzgo.Application.Leads.AddLeadNote;

public sealed class AddLeadNoteResponse
{
    public Guid ActivityId { get; init; }
    public Guid LeadId { get; init; }
    public string ActivityType { get; init; } = null!;
    public string Content { get; init; } = null!;
    public DateTime? NextActionAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

