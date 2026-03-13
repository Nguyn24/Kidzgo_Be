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
    : ICommandHandler<ApproveProfileCommand, ApproveProfileResponse>
{
    public async Task<Result<ApproveProfileResponse>> Handle(ApproveProfileCommand command, CancellationToken cancellationToken)
    {
        var result = new ApproveProfileResponse();

        if (command.Id == null || !command.Id.Any())
            return Result.Success(result);

        var ids = command.Id.Distinct().ToList();
        var profiles = await context.Profiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.IsApproved
            })
            .ToListAsync(cancellationToken);

        var foundIds = profiles.Select(p => p.Id).ToHashSet();

        result.NotFound = ids
            .Where(id => !foundIds.Contains(id))
            .ToList();

        var alreadyApproved = profiles
            .Where(p => p.IsApproved)
            .Select(p => p.Id)
            .ToList();

        result.AlreadyApproved = alreadyApproved;

        var idsToApprove = profiles
            .Where(p => !p.IsApproved)
            .Select(p => p.Id)
            .ToList();

        if (!idsToApprove.Any())
            return Result.Success(result);

        var now = DateTime.UtcNow;

        // BULK UPDATE (1 SQL query)
        var updatedCount = await context.Profiles
            .Where(p => idsToApprove.Contains(p.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.IsApproved, true)
                .SetProperty(p => p.UpdatedAt, now),
                cancellationToken);

        result.ApprovedCount = updatedCount;

        // Query lại profile vừa approve để publish event
        var approvedProfiles = await context.Profiles
            .Include(p => p.User)
            .Where(p => idsToApprove.Contains(p.Id))
            .ToListAsync(cancellationToken);

        const string defaultPassword = "123456";
        const string defaultPin = "1234";
        await Task.WhenAll(
            approvedProfiles.Select(profile =>
                mediator.Publish(new ProfileCreatedDomainEvent(
                    profile.Id,
                    profile.UserId,
                    profile.ProfileType.ToString(),
                    profile.DisplayName,
                    profile.FullName ?? string.Empty,
                    defaultPassword,
                    defaultPin,
                    profile.Gender?.ToString() ?? string.Empty,
                    profile.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
                    profile.ZaloId ?? string.Empty,
                    profile.User?.Email ?? string.Empty,
                    profile.User?.PhoneNumber ?? string.Empty,
                    profile.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                ), cancellationToken)
            )
        );

        return Result.Success(result);
    }
}


