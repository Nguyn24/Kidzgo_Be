using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.GenerateMonthlyReportPdf;

public sealed record GenerateMonthlyReportPdfCommand(Guid ReportId) : ICommand<GenerateMonthlyReportPdfResponse>;

