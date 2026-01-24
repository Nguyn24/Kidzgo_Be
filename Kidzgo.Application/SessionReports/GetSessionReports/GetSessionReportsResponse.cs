using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.SessionReports.GetSessionReports;

public sealed class GetSessionReportsResponse
{
    public Page<SessionReportListItemDto> SessionReports { get; init; } = null!;
}

public sealed class SessionReportListItemDto
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public DateTime SessionDate { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid TeacherUserId { get; init; }
    public string TeacherName { get; init; } = null!;
    public DateOnly ReportDate { get; init; }
    public string Feedback { get; init; } = null!;
    public bool IsMonthlyCompiled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

