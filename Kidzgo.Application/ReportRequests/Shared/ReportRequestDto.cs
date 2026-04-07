namespace Kidzgo.Application.ReportRequests.Shared;

public sealed class ReportRequestDto
{
    public Guid Id { get; init; }
    public string ReportType { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string Priority { get; init; } = null!;
    public Guid AssignedTeacherUserId { get; init; }
    public string? AssignedTeacherName { get; init; }
    public Guid RequestedByUserId { get; init; }
    public string? RequestedByName { get; init; }
    public Guid? TargetStudentProfileId { get; init; }
    public string? TargetStudentName { get; init; }
    public Guid? TargetClassId { get; init; }
    public string? TargetClassCode { get; init; }
    public string? TargetClassTitle { get; init; }
    public Guid? TargetSessionId { get; init; }
    public DateTime? TargetSessionDate { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
    public string? Message { get; init; }
    public DateTime? DueAt { get; init; }
    public Guid? LinkedSessionReportId { get; init; }
    public Guid? LinkedMonthlyReportId { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
