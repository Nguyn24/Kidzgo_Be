using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class ReportRequest : Entity
{
    public Guid Id { get; set; }
    public ReportRequestType ReportType { get; set; }
    public ReportRequestStatus Status { get; set; } = ReportRequestStatus.Requested;
    public ReportRequestPriority Priority { get; set; } = ReportRequestPriority.Normal;
    public Guid AssignedTeacherUserId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? TargetStudentProfileId { get; set; }
    public Guid? TargetClassId { get; set; }
    public Guid? TargetSessionId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Message { get; set; }
    public DateTime? DueAt { get; set; }
    public Guid? LinkedSessionReportId { get; set; }
    public Guid? LinkedMonthlyReportId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User AssignedTeacher { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public Profile? TargetStudentProfile { get; set; }
    public Class? TargetClass { get; set; }
    public Session? TargetSession { get; set; }
    public SessionReport? LinkedSessionReport { get; set; }
    public StudentMonthlyReport? LinkedMonthlyReport { get; set; }
}
