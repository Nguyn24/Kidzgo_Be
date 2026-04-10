using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.UpsertProgramLeavePolicy;

public sealed class UpsertProgramLeavePolicyCommandHandler(
    IDbContext context,
    IUserContext userContext) : ICommandHandler<UpsertProgramLeavePolicyCommand, UpsertProgramLeavePolicyResponse>
{
    public async Task<Result<UpsertProgramLeavePolicyResponse>> Handle(
        UpsertProgramLeavePolicyCommand command,
        CancellationToken cancellationToken)
    {
        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<UpsertProgramLeavePolicyResponse>(ProgramErrors.NotFound(command.ProgramId));
        }

        if (command.MaxLeavesPerMonth <= 0)
        {
            return Result.Failure<UpsertProgramLeavePolicyResponse>(ProgramLeavePolicyErrors.InvalidMaxLeavesPerMonth);
        }

        var policy = await context.ProgramLeavePolicies
            .FirstOrDefaultAsync(x => x.ProgramId == command.ProgramId, cancellationToken);

        var now = VietnamTime.UtcNow();

        if (policy is null)
        {
            policy = new ProgramLeavePolicy
            {
                Id = Guid.NewGuid(),
                ProgramId = command.ProgramId,
                MaxLeavesPerMonth = command.MaxLeavesPerMonth,
                UpdatedBy = userContext.UserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.ProgramLeavePolicies.Add(policy);
        }
        else
        {
            policy.MaxLeavesPerMonth = command.MaxLeavesPerMonth;
            policy.UpdatedBy = userContext.UserId;
            policy.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpsertProgramLeavePolicyResponse
        {
            ProgramId = policy.ProgramId,
            MaxLeavesPerMonth = policy.MaxLeavesPerMonth,
            UpdatedAt = policy.UpdatedAt,
            UpdatedBy = policy.UpdatedBy
        };
    }
}
