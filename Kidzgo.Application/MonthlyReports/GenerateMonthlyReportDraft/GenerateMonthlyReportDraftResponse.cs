namespace Kidzgo.Application.MonthlyReports.GenerateMonthlyReportDraft;

public sealed record GenerateMonthlyReportDraftResponse(
    Guid Id,
    string? DraftContent,
    DateTime UpdatedAt
);

