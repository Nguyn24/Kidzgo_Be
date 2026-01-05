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
    IUserContext userContext
) : ICommandHandler<SelectStudentProfileCommand>
{
    public async Task<Result> Handle(SelectStudentProfileCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Profile? profile = await context.Profiles
            .SingleOrDefaultAsync(p => p.Id == command.ProfileId && p.UserId == userId, cancellationToken);

        if (profile is null || profile.ProfileType != ProfileType.Student || profile.IsDeleted || !profile.IsActive)
        {
            return Result.Failure(ProfileErrors.Invalid);
        }

        // Không cần làm gì thêm phía backend; FE dùng profileId để set context
        return Result.Success();
    }
}







