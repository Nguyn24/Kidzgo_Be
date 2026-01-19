using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.LeaveRequests.GetLeaveRequestById;

public sealed class GetLeaveRequestByIdResponse
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ClassId { get; set; }
    public DateOnly SessionDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Reason { get; set; }
    public int? NoticeHours { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

