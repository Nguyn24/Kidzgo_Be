using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.CRM;

namespace Kidzgo.Application.Leads.AddLeadNote;

public sealed class AddLeadNoteCommand : ICommand<AddLeadNoteResponse>
{
    public Guid LeadId { get; init; }
    public string Content { get; init; } = null!;
    public ActivityType ActivityType { get; init; } = ActivityType.Note;
    public DateTime? NextActionAt { get; init; }
}

