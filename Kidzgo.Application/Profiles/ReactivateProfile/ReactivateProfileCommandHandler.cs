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

        await context.Profiles
            .Where(p => p.UserId == profile.UserId && p.IsApproved)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.IsDeleted, false)
                    .SetProperty(p => p.IsActive, true)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
                cancellationToken);

        return Result.Success();
    }
}

