using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public sealed class CreateLeaveRequestCommand : ICommand<CreateLeaveRequestResponse>
{
    public Guid StudentProfileId { get; init; }
    public Guid ClassId { get; init; }
    public DateOnly SessionDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Reason { get; init; }
}

