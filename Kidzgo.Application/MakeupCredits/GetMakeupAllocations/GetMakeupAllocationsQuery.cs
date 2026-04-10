using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetMakeupAllocations;

public sealed class GetMakeupAllocationsQuery : IQuery<IEnumerable<MakeupAllocationResponse>>
{
    public Guid StudentProfileId { get; set; }
    public bool IncludeCancelled { get; set; }
}

public sealed class GetMakeupAllocationsQueryHandler(IDbContext context)
    : IQueryHandler<GetMakeupAllocationsQuery, IEnumerable<MakeupAllocationResponse>>
{
    public async Task<Result<IEnumerable<MakeupAllocationResponse>>> Handle(
        GetMakeupAllocationsQuery query,
        CancellationToken cancellationToken)
    {
        var rawAllocations = await context.MakeupAllocations
            .AsNoTracking()
            .Where(a => a.MakeupCredit.StudentProfileId == query.StudentProfileId)
            .Where(a => query.IncludeCancelled || a.Status != Domain.Sessions.MakeupAllocationStatus.Cancelled)
            .Select(a => new
            {
                Id = a.Id,
                MakeupCreditId = a.MakeupCreditId,
                TargetSessionId = a.TargetSessionId,
                Status = a.Status.ToString(),
                AssignedBy = a.AssignedBy,
                AssignedAt = a.AssignedAt,
                CreatedAt = a.CreatedAt
            })
            .OrderByDescending(a => a.AssignedAt ?? a.CreatedAt)
            .ToListAsync(cancellationToken);

        var allocations = query.IncludeCancelled
            ? rawAllocations
            : rawAllocations
                .GroupBy(a => a.TargetSessionId)
                .Select(g => g
                    .OrderByDescending(a => a.AssignedAt ?? a.CreatedAt)
                    .First())
                .OrderByDescending(a => a.AssignedAt ?? a.CreatedAt)
                .ToList();

        var response = allocations
            .Select(a => new MakeupAllocationResponse
            {
                Id = a.Id,
                MakeupCreditId = a.MakeupCreditId,
                TargetSessionId = a.TargetSessionId,
                Status = a.Status,
                AssignedBy = a.AssignedBy,
                AssignedAt = a.AssignedAt
            })
            .ToList();

        return response;
    }
}

public sealed class MakeupAllocationResponse
{
    public Guid Id { get; set; }
    public Guid MakeupCreditId { get; set; }
    public Guid TargetSessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedBy { get; set; }
    public DateTime? AssignedAt { get; set; }
}

