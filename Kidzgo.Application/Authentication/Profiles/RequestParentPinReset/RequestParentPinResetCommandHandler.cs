using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Kidzgo.Domain.Users.Events;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Profiles.RequestParentPinReset;

public sealed class RequestParentPinResetCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<RequestParentPinResetCommand>
{
    public async Task<Result> Handle(RequestParentPinResetCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Profile? profile = await context.Profiles
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == command.ProfileId && p.UserId == userId, cancellationToken);

        if (profile is null || profile.ProfileType != ProfileType.Parent || profile.IsDeleted || !profile.IsActive)
        {
            return Result.Failure(ProfileErrors.Invalid);
        }

        if (string.IsNullOrWhiteSpace(profile.User.Email))
        {
            return Result.Failure(ProfileErrors.EmailNotSet);
        }

        // Raise domain event để gửi email
        profile.Raise(new ParentPinResetRequestDomainEvent(profile.Id, profile.UserId));

        await context.SaveChangesAsync(cancellationToken);

        // Luôn trả về Success để tránh lộ thông tin
        return Result.Success();
    }
}

