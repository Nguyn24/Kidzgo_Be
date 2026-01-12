using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LeaveRequests.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestCommand : ICommand
{
    public Guid Id { get; init; }
}

