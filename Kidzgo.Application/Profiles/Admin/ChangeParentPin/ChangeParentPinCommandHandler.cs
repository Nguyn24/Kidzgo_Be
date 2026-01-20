using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.Admin.ChangeParentPin;

public sealed class ChangeParentPinCommandHandler(
    IDbContext context,
    IPasswordHasher pinHasher
) : ICommandHandler<ChangeParentPinCommand>
{
    public async Task<Result> Handle(ChangeParentPinCommand command, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .SingleOrDefaultAsync(p => p.Id == command.ProfileId, cancellationToken);

        if (profile is null || profile.ProfileType != ProfileType.Parent)
        {
            return Result.Failure(ProfileErrors.ParentNotFound);
        }

        if (profile.IsDeleted || !profile.IsActive)
        {
            return Result.Failure(ProfileErrors.Invalid);
        }

        if (!IsValidPin(command.NewPin))
        {
            return Result.Failure(PinErrors.Invalid);
        }

        profile.PinHash = pinHasher.Hash(command.NewPin);
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
