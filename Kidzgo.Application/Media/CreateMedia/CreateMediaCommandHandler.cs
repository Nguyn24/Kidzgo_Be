using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.CreateMedia;

public sealed class CreateMediaCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMediaCommand, CreateMediaResponse>
{
    public async Task<Result<CreateMediaResponse>> Handle(CreateMediaCommand command, CancellationToken cancellationToken)
    {
        var uploaderId = userContext.UserId;

        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateMediaResponse>(
                MediaErrors.BranchNotFound);
        }

        // Check if class exists (if provided)
        if (command.ClassId.HasValue)
        {
            bool classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<CreateMediaResponse>(
                    MediaErrors.ClassNotFound);
            }
        }

        // Check if student profile exists (if provided)
        if (command.StudentProfileId.HasValue)
        {
            var studentProfile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value && 
                    p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

            if (studentProfile is null)
            {
                return Result.Failure<CreateMediaResponse>(
                    MediaErrors.StudentNotFound);
            }
        }

        var now = DateTime.UtcNow;
        var media = new MediaAsset
        {
            Id = Guid.NewGuid(),
            UploaderId = uploaderId,
            BranchId = command.BranchId,
            ClassId = command.ClassId,
            StudentProfileId = command.StudentProfileId,
            MonthTag = command.MonthTag,
            Type = command.Type,
            ContentType = command.ContentType,
            Url = command.Url,
            Caption = command.Caption,
            Visibility = command.Visibility,
            ApprovalStatus = ApprovalStatus.Pending,
            IsPublished = false,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.MediaAssets.Add(media);
        await context.SaveChangesAsync(cancellationToken);

        // Query media with navigation properties for response
        var mediaWithNav = await context.MediaAssets
            .Include(m => m.UploaderUser)
            .Include(m => m.Branch)
            .Include(m => m.Class)
            .Include(m => m.StudentProfile)
            .FirstOrDefaultAsync(m => m.Id == media.Id, cancellationToken);

        return new CreateMediaResponse
        {
            Id = mediaWithNav!.Id,
            UploaderId = mediaWithNav.UploaderId,
            UploaderName = mediaWithNav.UploaderUser.Name,
            BranchId = mediaWithNav.BranchId,
            BranchName = mediaWithNav.Branch.Name,
            ClassId = mediaWithNav.ClassId,
            ClassName = mediaWithNav.Class?.Title,
            StudentProfileId = mediaWithNav.StudentProfileId,
            StudentName = mediaWithNav.StudentProfile?.DisplayName,
            MonthTag = mediaWithNav.MonthTag,
            Type = mediaWithNav.Type,
            ContentType = mediaWithNav.ContentType,
            Url = mediaWithNav.Url,
            Caption = mediaWithNav.Caption,
            Visibility = mediaWithNav.Visibility,
            ApprovalStatus = mediaWithNav.ApprovalStatus,
            IsPublished = mediaWithNav.IsPublished,
            CreatedAt = mediaWithNav.CreatedAt
        };
    }
}

