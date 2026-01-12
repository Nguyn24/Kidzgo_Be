using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public sealed class CreateLeaveRequestResponse
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid ClassId { get; init; }
    public DateOnly SessionDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Reason { get; init; }
    public int? NoticeHours { get; init; }
    public LeaveRequestStatus Status { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
}

