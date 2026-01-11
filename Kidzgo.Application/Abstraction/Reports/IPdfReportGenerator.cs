namespace Kidzgo.Application.Abstraction.Reports;

/// <summary>
/// Interface for PDF generation from Monthly Report content
/// </summary>
public interface IPdfReportGenerator
{
    /// <summary>
    /// Generate PDF file from HTML content
    /// </summary>
    /// <param name="htmlContent">HTML content of the report</param>
    /// <param name="studentName">Student name for filename</param>
    /// <param name="month">Report month</param>
    /// <param name="year">Report year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URL to the generated PDF file</returns>
    Task<string> GeneratePdfAsync(
        string htmlContent,
        string studentName,
        int month,
        int year,
        CancellationToken cancellationToken = default);
}

