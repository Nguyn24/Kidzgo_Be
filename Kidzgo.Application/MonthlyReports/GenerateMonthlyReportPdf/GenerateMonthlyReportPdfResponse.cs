namespace Kidzgo.Application.MonthlyReports.GenerateMonthlyReportPdf;

public sealed class GenerateMonthlyReportPdfResponse
{
    public Guid ReportId { get; init; }
    public string PdfUrl { get; init; } = null!;
    public DateTime PdfGeneratedAt { get; init; }
}

