using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.SessionReports.RejectSessionReport;

public sealed class RejectSessionReportResponse
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
    public Guid? ReviewedByUserId { get; init; }
    public string? ReviewedByUserName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
