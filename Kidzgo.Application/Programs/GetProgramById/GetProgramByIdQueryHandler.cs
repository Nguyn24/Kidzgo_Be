using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
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
                IsMakeup = p.IsMakeup,
                IsSupplementary = p.IsSupplementary,
                DefaultMakeupClassId = p.DefaultMakeupClassId,
                Code = p.Code,
                DefaultTuitionAmount = p.DefaultTuitionAmount,
                UnitPriceSession = p.UnitPriceSession,
                Description = p.Description,
                IsActive = p.IsActive,
                TotalSessions = p.TuitionPlans
                    .Where(tp => tp.IsActive && !tp.IsDeleted)
                    .Select(tp => (int?)tp.TotalSessions)
                    .Max() ?? 0,
                ClassCount = p.Classes.Count(c => c.Status != Domain.Classes.ClassStatus.Cancelled),
                StudentCount = p.Classes
                    .SelectMany(c => c.ClassEnrollments)
                    .Count(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (program is null)
        {
            return Result.Failure<GetProgramByIdResponse>(ProgramErrors.NotFound(query.Id));
        }

        return program;
    }
}

