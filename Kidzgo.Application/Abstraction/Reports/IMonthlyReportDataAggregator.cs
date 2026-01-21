namespace Kidzgo.Application.Abstraction.Reports;

/// Interface for aggregating data from various sources for Monthly Report
public interface IMonthlyReportDataAggregator
{
    /// Aggregate all data needed for Monthly Report generation
    /// <param name="studentProfileId">Student profile ID</param>
    /// <param name="month">Report month (1-12)</param>
    /// <param name="year">Report year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON string containing aggregated data (attendance, homework, test, mission, notes)</returns>
    Task<string> AggregateDataAsync(
        Guid studentProfileId,
        int month,
        int year,
        CancellationToken cancellationToken = default);
}

