using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequestById;

public sealed class GetPauseEnrollmentRequestByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetPauseEnrollmentRequestByIdQuery, PauseEnrollmentRequestResponse>
{
    public async Task<Result<PauseEnrollmentRequestResponse>> Handle(
        GetPauseEnrollmentRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await context.PauseEnrollmentRequests
            .Select(r => new PauseEnrollmentRequestResponse
            {
                Id = r.Id,
                StudentProfileId = r.StudentProfileId,
                ClassId = r.ClassId,
                PauseFrom = r.PauseFrom,
                PauseTo = r.PauseTo,
                Reason = r.Reason,
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt,
                ApprovedBy = r.ApprovedBy,
                ApprovedAt = r.ApprovedAt,
                CancelledBy = r.CancelledBy,
                CancelledAt = r.CancelledAt,
                Outcome = r.Outcome.HasValue ? r.Outcome.Value.ToString() : null,
                OutcomeNote = r.OutcomeNote,
                OutcomeBy = r.OutcomeBy,
                OutcomeAt = r.OutcomeAt
            })
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure<PauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        return item;
    }
}
