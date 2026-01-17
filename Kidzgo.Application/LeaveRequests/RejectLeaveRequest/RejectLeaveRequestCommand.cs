using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LeaveRequests.RejectLeaveRequest;

public sealed class RejectLeaveRequestCommand : ICommand
{
    public Guid Id { get; init; }
}

