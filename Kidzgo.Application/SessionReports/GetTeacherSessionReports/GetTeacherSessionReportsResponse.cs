using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.SessionReports.GetTeacherSessionReports;

public sealed class GetTeacherSessionReportsResponse
{
    public Guid TeacherUserId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public Page<TeacherSessionReportListItemDto> SessionReports { get; init; } = null!;
}

public sealed class TeacherSessionReportListItemDto
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public DateTime SessionDate { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public DateOnly ReportDate { get; init; }
    public string Feedback { get; init; } = null!;
    public bool IsMonthlyCompiled { get; init; }
    public DateTime CreatedAt { get; init; }
}

