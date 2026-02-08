using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.UpdateMonthlyReportDraft;

/// <summary>
/// UC-180: Teacher chỉnh sửa draft Monthly Report
/// </summary>
public sealed class UpdateMonthlyReportDraftCommand : ICommand<UpdateMonthlyReportDraftResponse>
{
    public Guid ReportId { get; init; }
    public string DraftContent { get; init; } = null!;
}

