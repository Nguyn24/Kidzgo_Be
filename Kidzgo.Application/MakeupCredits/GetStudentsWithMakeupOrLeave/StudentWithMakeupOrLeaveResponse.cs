namespace Kidzgo.Application.MakeupCredits.GetStudentsWithMakeupOrLeave;

public sealed class StudentWithMakeupOrLeaveResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public bool HasLeaveRequest { get; set; }
    public bool HasMakeupCredit { get; set; }
    public int LeaveRequestCount { get; set; }
    public int MakeupCreditCount { get; set; }
}

