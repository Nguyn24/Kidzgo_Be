using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.GetProgramById;

public sealed class GetProgramByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetProgramByIdQuery, GetProgramByIdResponse>
{
    public async Task<Result<GetProgramByIdResponse>> Handle(GetProgramByIdQuery query, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .Include(p => p.Branch)
            .Where(p => p.Id == query.Id && !p.IsDeleted)
            .Select(p => new GetProgramByIdResponse
            {
                Id = p.Id,
                BranchId = p.BranchId,
                BranchName = p.Branch.Name,
                Name = p.Name,
                Level = p.Level,
                TotalSessions = p.TotalSessions,
                DefaultTuitionAmount = p.DefaultTuitionAmount,
                UnitPriceSession = p.UnitPriceSession,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (program is null)
        {
            return Result.Failure<GetProgramByIdResponse>(
                Error.NotFound("Program.NotFound", "Program not found"));
        }

        return program;
    }
}

