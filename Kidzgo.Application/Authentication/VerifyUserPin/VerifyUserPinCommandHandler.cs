using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.VerifyUserPin;

public sealed class VerifyUserPinCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IPasswordHasher pinHasher
) : ICommandHandler<VerifyUserPinCommand>
{
    public async Task<Result> Handle(VerifyUserPinCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userId));
        }

        if (!IsValidPin(command.Pin))
        {
            return Result.Failure(PinErrors.Invalid);
        }

        if (string.IsNullOrWhiteSpace(user.PinHash))
        {
            // Chưa có PIN → set PIN lần đầu
            user.PinHash = pinHasher.Hash(command.Pin);
        }
        else
        {
            bool ok = pinHasher.Verify(command.Pin, user.PinHash);
            if (!ok)
            {
                return Result.Failure(PinErrors.Wrong);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidPin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length >= 10)
        {
            return false;
        }

        return pin.All(char.IsDigit);
    }
}







