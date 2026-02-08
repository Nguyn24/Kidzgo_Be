using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.GenerateMonthlyReportDraft;

public sealed record GenerateMonthlyReportDraftCommand(
    Guid ReportId
) : ICommand<GenerateMonthlyReportDraftResponse>;

