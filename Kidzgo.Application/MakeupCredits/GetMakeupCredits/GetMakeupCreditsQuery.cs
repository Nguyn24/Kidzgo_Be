using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetMakeupCredits;

public sealed class GetMakeupCreditsQuery : IQuery<IEnumerable<MakeupCreditResponse>>
{
    public Guid StudentProfileId { get; set; }
}

public sealed class GetMakeupCreditsQueryHandler(IDbContext context)
    : IQueryHandler<GetMakeupCreditsQuery, IEnumerable<MakeupCreditResponse>>
{
    public async Task<Result<IEnumerable<MakeupCreditResponse>>> Handle(
        GetMakeupCreditsQuery query,
        CancellationToken cancellationToken)
    {
        var credits = await context.MakeupCredits
            .AsNoTracking()
            .Where(c => c.StudentProfileId == query.StudentProfileId)
            .OrderByDescending(c => c.CreatedAt)
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
            .ToListAsync(cancellationToken);

        return credits;
    }
}

