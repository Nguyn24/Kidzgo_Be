using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Logout;

public sealed class LogoutCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var refreshTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);

        if (refreshTokens.Any())
        {
            context.RefreshTokens.RemoveRange(refreshTokens);
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}

