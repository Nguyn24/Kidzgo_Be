using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.ReactivateProfile;

public sealed class ReactivateProfileCommandHandler(IDbContext context)
    : ICommandHandler<ReactivateProfileCommand>
{
    public async Task<Result> Handle(ReactivateProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.Id));
        }

        // if (!profile.IsDeleted)
        // {
        //     return Result.Failure(ProfileErrors.ProfileNotDeleted);
        // }

        // Reactivate: set IsDeleted = false and IsActive = true
        profile.IsDeleted = false;
        profile.IsActive = true;
        profile.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

