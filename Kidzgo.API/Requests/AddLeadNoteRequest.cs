using Kidzgo.Domain.CRM;

namespace Kidzgo.API.Requests;

public sealed class AddLeadNoteRequest
{
    public string Content { get; set; } = null!;
    public ActivityType ActivityType { get; set; } = ActivityType.Note;
    public DateTime? NextActionAt { get; set; }
}

