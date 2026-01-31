namespace Kidzgo.Application.MakeupCredits.SuggestSessions;

public sealed class SuggestedSessionResponse
{
    public Guid SessionId { get; set; }
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public string ClassTitle { get; set; } = null!;
    public string ProgramName { get; set; } = null!;
    public string? ProgramLevel { get; set; }
    public Guid BranchId { get; set; }
    public DateTime PlannedDatetime { get; set; }
    public DateTime PlannedEndDatetime { get; set; }
}