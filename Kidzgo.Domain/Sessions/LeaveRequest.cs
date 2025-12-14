using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Domain.Sessions;

public class LeaveRequest : Entity
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
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    public Profile StudentProfile { get; set; } = null!;
    public Class Class { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
}
