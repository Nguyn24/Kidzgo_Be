using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.UnlinkParentStudent;

public sealed class UnlinkParentStudentCommandHandler(IDbContext context)
    : ICommandHandler<UnlinkParentStudentCommand>
{
    public async Task<Result> Handle(UnlinkParentStudentCommand command, CancellationToken cancellationToken)
    {
        var link = await context.ParentStudentLinks
            .FirstOrDefaultAsync(l => l.ParentProfileId == command.ParentProfileId && 
                l.StudentProfileId == command.StudentProfileId, cancellationToken);

        if (link is null)
        {
            return Result.Failure(ProfileErrors.LinkNotFound);
        }

        context.ParentStudentLinks.Remove(link);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

