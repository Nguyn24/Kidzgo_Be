using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.SessionReports.ApproveSessionReport;

public sealed class ApproveSessionReportResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid TeacherUserId { get; init; }
    public string TeacherName { get; init; } = null!;
    public ReportStatus Status { get; init; }
    public DateOnly ReportDate { get; init; }
    public string Feedback { get; init; } = null!;
    public Guid? ReviewedBy { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
