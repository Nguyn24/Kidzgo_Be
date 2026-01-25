using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.UpdateProgram;

public sealed class UpdateProgramCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateProgramCommand, UpdateProgramResponse>
{
    public async Task<Result<UpdateProgramResponse>> Handle(UpdateProgramCommand command, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure<UpdateProgramResponse>(ProgramErrors.NotFound(command.Id));
        }

        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<UpdateProgramResponse>(ProgramErrors.BranchNotFound);
        }

        program.BranchId = command.BranchId;
        program.Name = command.Name;
        program.Level = command.Level;
        program.TotalSessions = command.TotalSessions;
        program.DefaultTuitionAmount = command.DefaultTuitionAmount;
        program.UnitPriceSession = command.UnitPriceSession;
        program.Description = command.Description;
        program.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateProgramResponse
        {
            Id = program.Id,
            BranchId = program.BranchId,
            Name = program.Name,
            Level = program.Level,
            TotalSessions = program.TotalSessions,
            DefaultTuitionAmount = program.DefaultTuitionAmount,
            UnitPriceSession = program.UnitPriceSession,
            Description = program.Description,
            IsActive = program.IsActive
        };
    }
}

