using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Reports.Events;

/// <summary>
/// Domain event raised when a Monthly Report is published
/// UC-187: Gửi thông báo khi publish Monthly Report
/// </summary>
public sealed record MonthlyReportPublishedEvent(
    Guid ReportId,
    Guid StudentProfileId,
    int Month,
    int Year
) : IDomainEvent;

