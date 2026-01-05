using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.ChangePin;

public sealed class ChangeUserPinCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IPasswordHasher pinHasher
) : ICommandHandler<ChangeUserPinCommand>
{
    public async Task<Result> Handle(ChangeUserPinCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userId));
        }

        if (!IsValidPin(command.NewPin))
        {
            return Result.Failure(PinErrors.Invalid);
        }

        if (string.IsNullOrWhiteSpace(user.PinHash))
        {
            // Nếu chưa có PIN thì không cho đổi (phải verify/set trước)
            return Result.Failure(PinErrors.NotSet);
        }

        bool ok = pinHasher.Verify(command.CurrentPin, user.PinHash);
        if (!ok)
        {
            return Result.Failure(PinErrors.Wrong);
        }

        user.PinHash = pinHasher.Hash(command.NewPin);

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







