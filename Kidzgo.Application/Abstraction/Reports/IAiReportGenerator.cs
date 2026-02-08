namespace Kidzgo.Application.Abstraction.Reports;

/// Interface for AI-powered Monthly Report generation
public interface IAiReportGenerator
{
    /// Generate draft Monthly Report content from aggregated data
    /// <param name="dataJson">JSON string containing aggregated data (attendance, homework, test, mission, notes)</param>
    /// <param name="studentProfileId">Student profile ID for fetching student info and recent reports</param>
    /// <param name="month">Report month</param>
    /// <param name="year">Report year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated draft content (JSON string containing AI response)</returns>
    Task<string> GenerateDraftAsync(
        string dataJson,
        Guid studentProfileId,
        int month,
        int year,
        CancellationToken cancellationToken = default);
}

