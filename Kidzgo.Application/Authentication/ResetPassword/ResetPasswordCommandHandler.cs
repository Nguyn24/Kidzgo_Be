using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IDbContext context,
    IPasswordHasher passwordHasher
) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var token = await context.PasswordResetTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.Token == command.Token, cancellationToken);

        if (token is null || token.IsUsed)
        {
            return Result.Failure(UserErrors.InvalidCurrentPassword); // TODO: tạo error riêng cho token invalid
        }

        var user = token.User;

        // Update password
        user.PasswordHash = passwordHasher.Hash(command.NewPassword);

        token.UsedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}


