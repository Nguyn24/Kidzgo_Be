using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetMakeupCreditById;

public sealed class GetMakeupCreditByIdQuery : IQuery<MakeupCreditResponse>
{
    public Guid Id { get; set; }
}

public sealed class GetMakeupCreditByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetMakeupCreditByIdQuery, MakeupCreditResponse>
{
    public async Task<Result<MakeupCreditResponse>> Handle(
        GetMakeupCreditByIdQuery query,
        CancellationToken cancellationToken)
    {
        var credit = await context.MakeupCredits
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new MakeupCreditResponse
            {
                Id = c.Id,
                StudentProfileId = c.StudentProfileId,
                SourceSessionId = c.SourceSessionId,
                Status = c.Status,
                CreatedReason = c.CreatedReason,
                ExpiresAt = c.ExpiresAt,
                UsedSessionId = c.UsedSessionId,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (credit is null)
        {
            return Result.Failure<MakeupCreditResponse>(MakeupCreditErrors.NotFound(query.Id));
        }

        return credit;
    }
}

