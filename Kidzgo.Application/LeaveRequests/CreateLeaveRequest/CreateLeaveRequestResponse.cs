using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public sealed class CreateLeaveRequestResponse
{
    public List<LeaveRequestItem> LeaveRequests { get; init; } = new();
}

public sealed class LeaveRequestItem
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid ClassId { get; init; }
    public DateOnly SessionDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Reason { get; init; }
    public int? NoticeHours { get; init; }
    public string? Status { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
}

