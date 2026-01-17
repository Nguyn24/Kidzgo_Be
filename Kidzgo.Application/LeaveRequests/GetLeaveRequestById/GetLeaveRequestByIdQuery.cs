using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LeaveRequests.GetLeaveRequestById;

public sealed class GetLeaveRequestByIdQuery : IQuery<GetLeaveRequestByIdResponse>
{
    public Guid Id { get; init; }
}

