using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetMakeupAllocations;

public sealed class GetMakeupAllocationsQuery : IQuery<IEnumerable<MakeupAllocationResponse>>
{
    public Guid StudentProfileId { get; set; }
}

public sealed class GetMakeupAllocationsQueryHandler(IDbContext context)
    : IQueryHandler<GetMakeupAllocationsQuery, IEnumerable<MakeupAllocationResponse>>
{
    public async Task<Result<IEnumerable<MakeupAllocationResponse>>> Handle(
        GetMakeupAllocationsQuery query,
        CancellationToken cancellationToken)
    {
        var allocations = await context.MakeupAllocations
            .AsNoTracking()
            .Where(a => a.MakeupCredit.StudentProfileId == query.StudentProfileId)
            .Select(a => new MakeupAllocationResponse
            {
                Id = a.Id,
                MakeupCreditId = a.MakeupCreditId,
                TargetSessionId = a.TargetSessionId,
                AssignedBy = a.AssignedBy,
                AssignedAt = a.AssignedAt
            })
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync(cancellationToken);

        return allocations;
    }
}

public sealed class MakeupAllocationResponse
{
    public Guid Id { get; set; }
    public Guid MakeupCreditId { get; set; }
    public Guid TargetSessionId { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime? AssignedAt { get; set; }
}

