using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LeaveRequests.ApproveLeaveRequest;
using Kidzgo.Domain.Common;
using MediatR;

namespace Kidzgo.Application.LeaveRequests.BulkApproveLeaveRequests;

public sealed class BulkApproveLeaveRequestsCommandHandler(
    ISender mediator)
    : ICommandHandler<BulkApproveLeaveRequestsCommand, BulkApproveLeaveRequestsResponse>
{
    public async Task<Result<BulkApproveLeaveRequestsResponse>> Handle(
        BulkApproveLeaveRequestsCommand request,
        CancellationToken cancellationToken)
    {
        var response = new BulkApproveLeaveRequestsResponse();

        foreach (var id in request.Ids.Distinct())
        {
            var result = await mediator.Send(
                new ApproveLeaveRequestCommand { Id = id },
                cancellationToken);

            if (result.IsFailure)
            {
                response.Errors.Add(new BulkApproveLeaveRequestError
                {
                    Id = id,
                    Code = result.Error.Code,
                    Message = result.Error.Description
                });
                continue;
            }

            response.ApprovedIds.Add(id);
        }

        return response;
    }
}
