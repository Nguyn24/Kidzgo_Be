using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReports;

/// <summary>
/// Get list of Monthly Reports with filters
/// </summary>
public sealed class GetMonthlyReportsQuery : IQuery<GetMonthlyReportsResponse>
{
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? JobId { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

