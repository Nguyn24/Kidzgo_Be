using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequests;

public sealed class GetPauseEnrollmentRequestsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetPauseEnrollmentRequestsQuery, Page<PauseEnrollmentRequestResponse>>
{
    public async Task<Result<Page<PauseEnrollmentRequestResponse>>> Handle(
        GetPauseEnrollmentRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var studentProfileId = request.StudentProfileId ?? userContext.StudentId;

        var query = context.PauseEnrollmentRequests.AsQueryable();

        if (studentProfileId.HasValue)
        {
            query = query.Where(r => r.StudentProfileId == studentProfileId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(r => r.ClassId == request.ClassId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(r => r.Class.BranchId == request.BranchId.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.RequestedAt)
            .ApplyPagination(request.PageNumber, request.PageSize)
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
            .ToListAsync(cancellationToken);

        return new Page<PauseEnrollmentRequestResponse>(items, total, request.PageNumber, request.PageSize);
    }
}
