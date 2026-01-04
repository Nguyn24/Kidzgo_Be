using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.VerifyParentPin;

public sealed class VerifyParentPinCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IPasswordHasher pinHasher
) : ICommandHandler<VerifyParentPinCommand>
{
    public async Task<Result> Handle(VerifyParentPinCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Profile? profile = await context.Profiles
            .SingleOrDefaultAsync(p => p.Id == command.ProfileId && p.UserId == userId, cancellationToken);

        if (profile is null || profile.ProfileType != ProfileType.Parent || profile.IsDeleted || !profile.IsActive)
        {
            return Result.Failure(ProfileErrors.Invalid);
        }

        if (!IsValidPin(command.Pin))
        {
            return Result.Failure(PinErrors.Invalid);
        }

        if (string.IsNullOrWhiteSpace(profile.PinHash))
        {
            // Chưa có PIN → set PIN lần đầu
            profile.PinHash = pinHasher.Hash(command.Pin);
        }
        else
        {
            bool ok = pinHasher.Verify(command.Pin, profile.PinHash);
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







