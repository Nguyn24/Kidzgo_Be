using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.LeaveRequests.GetLeaveRequests;

public sealed class GetLeaveRequestsResponse
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ClassId { get; set; }
    public DateOnly SessionDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Reason { get; set; }
    public int? NoticeHours { get; set; }
    public LeaveRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

