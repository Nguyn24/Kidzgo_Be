using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.DeleteProfile;

public sealed class DeleteProfileCommandHandler(IDbContext context)
    : ICommandHandler<DeleteProfileCommand>
{
    public async Task<Result> Handle(DeleteProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.Id));
        }

        // Soft delete
        profile.IsDeleted = true;
        profile.IsActive = false;
        profile.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

