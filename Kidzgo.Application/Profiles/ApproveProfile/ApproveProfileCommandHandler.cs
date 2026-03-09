using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Kidzgo.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.ApproveProfile;

public sealed class ApproveProfileCommandHandler(IDbContext context, IMediator mediator)
    : ICommandHandler<ApproveProfileCommand>
{
    public async Task<Result> Handle(ApproveProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.Id));
        }

        if (profile.IsApproved)
        {
            return Result.Failure(ProfileErrors.ProfileAlreadyApproved);
        }
        profile.IsApproved = true;
        profile.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        // Publish domain event for sending welcome email after profile is approved
        await mediator.Publish(new ProfileCreatedDomainEvent(
            profile.Id,
            profile.UserId,
            profile.ProfileType.ToString(),
            profile.DisplayName,
            profile.FullName ?? string.Empty,
            profile.Gender?.ToString() ?? string.Empty,
            profile.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
            profile.ZaloId ?? string.Empty,
            profile.User?.Email ?? string.Empty,
            profile.User?.PhoneNumber ?? string.Empty,
            profile.CreatedAt.ToString("dd/MM/yyyy HH:mm")
        ), cancellationToken);

        return Result.Success();
    }
}


