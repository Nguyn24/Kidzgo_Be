namespace Kidzgo.Application.MonthlyReports.UpdateMonthlyReportDraft;

public sealed class UpdateMonthlyReportDraftResponse
{
    public Guid Id { get; init; }
    public string? DraftContent { get; init; }
    public DateTime UpdatedAt { get; init; }
}

