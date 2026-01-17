using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.ToggleBranchStatus;

public sealed class ToggleBranchStatusCommandHandler(IDbContext context)
    : ICommandHandler<ToggleBranchStatusCommand, ToggleBranchStatusResponse>
{
    public async Task<Result<ToggleBranchStatusResponse>> Handle(ToggleBranchStatusCommand command, CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<ToggleBranchStatusResponse>(BranchErrors.NotFound(command.Id));
        }

        branch.IsActive = command.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new ToggleBranchStatusResponse
        {
            Id = branch.Id,
            IsActive = branch.IsActive
        };
    }
}

