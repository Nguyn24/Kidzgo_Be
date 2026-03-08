using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.ReactivateUser;

public sealed class ReactivateUserCommandHandler(IDbContext context)
    : ICommandHandler<ReactivateUserCommand, ReactivateUserCommandResponse>
{
    public async Task<Result<ReactivateUserCommandResponse>> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        
        if (user == null)
        {
            return Result.Failure<ReactivateUserCommandResponse>(UserErrors.NotFound(request.UserId));
        }

        if (!user.IsActive)
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            // Reactivate all profiles of this user (both Parent and Student)
            var allProfiles = await context.Profiles
                .Where(p => p.UserId == user.Id &&
                           !p.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var profile in allProfiles)
            {
                profile.IsActive = true;
                profile.IsDeleted = false;
                profile.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new ReactivateUserCommandResponse
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            UpdatedAt = user.UpdatedAt
        });
    }
}
