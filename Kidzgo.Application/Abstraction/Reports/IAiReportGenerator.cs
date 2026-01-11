namespace Kidzgo.Application.Abstraction.Reports;

/// <summary>
/// Interface for AI-powered Monthly Report generation
/// </summary>
public interface IAiReportGenerator
{
    /// <summary>
    /// Generate draft Monthly Report content from aggregated data
    /// </summary>
    /// <param name="dataJson">JSON string containing aggregated data (attendance, homework, test, mission, notes)</param>
    /// <param name="studentName">Student name for personalization</param>
    /// <param name="month">Report month</param>
    /// <param name="year">Report year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated draft content (HTML or markdown format)</returns>
    Task<string> GenerateDraftAsync(
        string dataJson,
        string studentName,
        int month,
        int year,
        CancellationToken cancellationToken = default);
}

