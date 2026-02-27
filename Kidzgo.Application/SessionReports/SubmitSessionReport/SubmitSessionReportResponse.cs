using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.SessionReports.SubmitSessionReport;

public sealed class SubmitSessionReportResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid TeacherUserId { get; init; }
    public string TeacherName { get; init; } = null!;
    public DateOnly ReportDate { get; init; }
    public string Feedback { get; init; } = null!;
    public ReportStatus Status { get; init; }
    public Guid? SubmittedByUserId { get; init; }
    public string? SubmittedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
