using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.LinkParentStudent;

public sealed class LinkParentStudentCommandHandler(IDbContext context)
    : ICommandHandler<LinkParentStudentCommand, LinkParentStudentResponse>
{
    public async Task<Result<LinkParentStudentResponse>> Handle(LinkParentStudentCommand command, CancellationToken cancellationToken)
    {
        // Verify parent profile exists and is Parent type
        var parentProfile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.ParentProfileId && 
                p.ProfileType == ProfileType.Parent && 
                !p.IsDeleted && 
                p.IsActive, cancellationToken);

        if (parentProfile is null)
        {
            return Result.Failure<LinkParentStudentResponse>(ProfileErrors.ParentNotFound);
        }

        // Verify student profile exists and is Student type
        var studentProfile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && 
                p.ProfileType == ProfileType.Student && 
                !p.IsDeleted && 
                p.IsActive, cancellationToken);

        if (studentProfile is null)
        {
            return Result.Failure<LinkParentStudentResponse>(ProfileErrors.StudentNotFound);
        }

        // Check if link already exists
        bool linkExists = await context.ParentStudentLinks
            .AnyAsync(l => l.ParentProfileId == command.ParentProfileId && 
                l.StudentProfileId == command.StudentProfileId, cancellationToken);

        if (linkExists)
        {
            return Result.Failure<LinkParentStudentResponse>(ProfileErrors.LinkAlreadyExists);
        }

        var link = new ParentStudentLink
        {
            Id = Guid.NewGuid(),
            ParentProfileId = command.ParentProfileId,
            StudentProfileId = command.StudentProfileId,
            CreatedAt = DateTime.UtcNow
        };

        context.ParentStudentLinks.Add(link);
        await context.SaveChangesAsync(cancellationToken);

        return new LinkParentStudentResponse
        {
            Id = link.Id,
            ParentProfileId = link.ParentProfileId,
            StudentProfileId = link.StudentProfileId,
            CreatedAt = link.CreatedAt
        };
    }
}

