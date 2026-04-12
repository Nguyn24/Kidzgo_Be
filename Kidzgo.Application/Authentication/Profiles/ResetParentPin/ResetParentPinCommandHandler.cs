using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Profiles.ResetParentPin;

public sealed class ResetParentPinCommandHandler(
    IDbContext context,
    IPasswordHasher pinHasher
) : ICommandHandler<ResetParentPinCommand>
{
    public async Task<Result> Handle(ResetParentPinCommand command, CancellationToken cancellationToken)
    {
        if (!IsValidPin(command.NewPin))
        {
            return Result.Failure(PinErrors.Invalid);
        }

        ParentPinResetToken? token = await context.ParentPinResetTokens
            .Include(t => t.Profile)
            .ThenInclude(p => p.User)
            .SingleOrDefaultAsync(t => t.Token == command.Token, cancellationToken);

        if (token is null || token.IsUsed)
        {
            return Result.Failure(PinErrors.InvalidResetToken);
        }

        if (token.RequiresOtpVerification && token.OtpVerifiedAt is null)
        {
            return Result.Failure(PinErrors.OtpNotVerified);
        }

        Profile profile = token.Profile;
        if (profile.ProfileType != ProfileType.Parent || profile.IsDeleted || !profile.IsActive)
        {
            return Result.Failure(ProfileErrors.Invalid);
        }

        profile.PinHash = pinHasher.Hash(command.NewPin);
        token.UsedAt = VietnamTime.UtcNow();

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
