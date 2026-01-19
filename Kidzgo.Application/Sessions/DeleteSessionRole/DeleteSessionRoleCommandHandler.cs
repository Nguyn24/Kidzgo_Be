using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.DeleteSessionRole;

public sealed class DeleteSessionRoleCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteSessionRoleCommand>
{
    public async Task<Result> Handle(
        DeleteSessionRoleCommand command,
        CancellationToken cancellationToken)
    {
        var sessionRole = await context.SessionRoles
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionRoleId, cancellationToken);

        if (sessionRole is null)
        {
            return Result.Failure(
                Error.NotFound("SessionRole.NotFound", "Session role not found"));
        }

        context.SessionRoles.Remove(sessionRole);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}