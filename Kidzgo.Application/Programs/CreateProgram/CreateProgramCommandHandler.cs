using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.CreateProgram;

public sealed class CreateProgramCommandHandler(
    IDbContext context
) : ICommandHandler<CreateProgramCommand, CreateProgramResponse>
{
    public async Task<Result<CreateProgramResponse>> Handle(CreateProgramCommand command, CancellationToken cancellationToken)
    {
        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateProgramResponse>(
                Error.NotFound("Program.BranchNotFound", "Branch not found or inactive"));
        }

        var now = DateTime.UtcNow;
        var program = new Program
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            Name = command.Name,
            Level = command.Level,
            TotalSessions = command.TotalSessions,
            DefaultTuitionAmount = command.DefaultTuitionAmount,
            UnitPriceSession = command.UnitPriceSession,
            Description = command.Description,
            IsActive = false, // Mặc định false, cần duyệt qua toggle-status API để active
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Programs.Add(program);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateProgramResponse
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

