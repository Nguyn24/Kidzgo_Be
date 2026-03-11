using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LeaveRequests.CancelLeaveRequest;

public sealed class CancelLeaveRequestCommand : ICommand
{
    public Guid Id { get; init; }
}
