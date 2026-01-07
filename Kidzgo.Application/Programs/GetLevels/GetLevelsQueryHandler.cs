using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.GetLevels;

public sealed class GetLevelsQueryHandler(
    IDbContext context
) : IQueryHandler<GetLevelsQuery, GetLevelsResponse>
{
    public async Task<Result<GetLevelsResponse>> Handle(GetLevelsQuery query, CancellationToken cancellationToken)
    {
        var levels = await context.Programs
            .Where(p => p.IsActive && !p.IsDeleted && !string.IsNullOrEmpty(p.Level))
            .Select(p => p.Level!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync(cancellationToken);

        return Result.Success(new GetLevelsResponse
        {
            Levels = levels
        });
    }
}

