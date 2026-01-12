using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.DeleteBranch;

public sealed class DeleteBranchCommandHandler(IDbContext context)
    : ICommandHandler<DeleteBranchCommand>
{
    public async Task<Result> Handle(DeleteBranchCommand command, CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (branch is null)
        {
            return Result.Failure(BranchErrors.NotFound(command.Id));
        }

        // Check if branch has active users, classes, etc. (soft delete only)
        // For now, just mark as inactive instead of deleting
        branch.IsActive = false;
        branch.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

