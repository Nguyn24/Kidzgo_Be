using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.SelectStudentProfile;

public sealed class SelectStudentProfileCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITokenProvider tokenProvider
) : ICommandHandler<SelectStudentProfileCommand, SelectStudentProfileResponse>
{
    public async Task<Result<SelectStudentProfileResponse>> Handle(SelectStudentProfileCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Profile? profile = await context.Profiles
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == command.ProfileId && p.UserId == userId, cancellationToken);

        if (profile is null || profile.ProfileType != ProfileType.Student || profile.IsDeleted || !profile.IsActive)
        {
            return Result.Failure<SelectStudentProfileResponse>(ProfileErrors.Invalid);
        }

        // Generate new token with selected student ID
        string newAccessToken = tokenProvider.Create(profile.User, profile.Id);

        return Result.Success(new SelectStudentProfileResponse
        {
            AccessToken = newAccessToken,
            StudentId = profile.Id
        });
    }
}







