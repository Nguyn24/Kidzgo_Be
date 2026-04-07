using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportRequests.CreateReportRequest;

public sealed class CreateReportRequestCommand : ICommand<ReportRequestDto>
{
    public ReportRequestType ReportType { get; init; }
    public Guid AssignedTeacherUserId { get; init; }
    public Guid? TargetStudentProfileId { get; init; }
    public Guid? TargetClassId { get; init; }
    public Guid? TargetSessionId { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
    public ReportRequestPriority Priority { get; init; } = ReportRequestPriority.High;
    public string? Message { get; init; }
    public DateTime? DueAt { get; init; }
    public NotificationChannel NotificationChannel { get; init; } = NotificationChannel.InApp;
}
