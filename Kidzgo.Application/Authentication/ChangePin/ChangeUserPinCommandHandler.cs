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

        bool userExists = await context.Users
            .AnyAsync(u => u.Id == userId, cancellationToken);

        if (!userExists)
        {
            return Result.Failure(UserErrors.NotFound(userId));
        }

        if (!IsValidPin(command.NewPin))
        {
            return Result.Failure(PinErrors.Invalid);
        }

        Profile? profile = await GetCurrentParentProfileAsync(userId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ProfileErrors.Invalid);
        }

        if (string.IsNullOrWhiteSpace(profile.PinHash))
        {
            return Result.Failure(PinErrors.NotSet);
        }

        bool ok = pinHasher.Verify(command.CurrentPin, profile.PinHash);
        if (!ok)
        {
            return Result.Failure(PinErrors.Wrong);
        }

        profile.PinHash = pinHasher.Hash(command.NewPin);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private Task<Profile?> GetCurrentParentProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        IQueryable<Profile> query = context.Profiles
            .Where(p => p.UserId == userId &&
                        p.ProfileType == ProfileType.Parent &&
                        !p.IsDeleted &&
                        p.IsActive);

        if (userContext.ParentId.HasValue)
        {
            query = query.Where(p => p.Id == userContext.ParentId.Value);
        }

        return query
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
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
