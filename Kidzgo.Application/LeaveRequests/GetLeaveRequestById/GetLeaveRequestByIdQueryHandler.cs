using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.GetLeaveRequestById;

public sealed class GetLeaveRequestByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetLeaveRequestByIdQuery, GetLeaveRequestByIdResponse>
{
    public async Task<Result<GetLeaveRequestByIdResponse>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await context.LeaveRequests
            .Where(l => l.Id == request.Id)
            .Select(l => new GetLeaveRequestByIdResponse
            {
                Id = l.Id,
                StudentProfileId = l.StudentProfileId,
                ClassId = l.ClassId,
                SessionDate = l.SessionDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                NoticeHours = l.NoticeHours,
                Status = l.Status,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt,
                ApprovedBy = l.ApprovedBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return Result.Failure<GetLeaveRequestByIdResponse>(LeaveRequestErrors.NotFound(request.Id));
        }

        return item;
    }
}

