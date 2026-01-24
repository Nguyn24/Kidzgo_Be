namespace Kidzgo.Application.SessionReports.CreateSessionReport;

public sealed class CreateSessionReportResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public DateTime SessionDate { get; init; }
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

