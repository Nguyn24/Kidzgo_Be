using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests.ApprovePauseEnrollmentRequest;
using Kidzgo.Domain.Common;
using MediatR;

namespace Kidzgo.Application.PauseEnrollmentRequests.BulkApprovePauseEnrollmentRequests;

public sealed class BulkApprovePauseEnrollmentRequestsCommandHandler(
    ISender mediator)
    : ICommandHandler<BulkApprovePauseEnrollmentRequestsCommand, BulkApprovePauseEnrollmentRequestsResponse>
{
    public async Task<Result<BulkApprovePauseEnrollmentRequestsResponse>> Handle(
        BulkApprovePauseEnrollmentRequestsCommand request,
        CancellationToken cancellationToken)
    {
        var response = new BulkApprovePauseEnrollmentRequestsResponse();

        foreach (var id in request.Ids.Distinct())
        {
            var result = await mediator.Send(
                new ApprovePauseEnrollmentRequestCommand { Id = id },
                cancellationToken);

            if (result.IsFailure)
            {
                response.Errors.Add(new BulkApprovePauseEnrollmentRequestError
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
