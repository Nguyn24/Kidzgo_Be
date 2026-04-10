using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Files.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Files.UploadFile;

public sealed class UploadFileCommandHandler(
    IFileStorageService fileStorageService,
    IDbContext context,
    IUserContext userContext,
    ILogger<UploadFileCommandHandler> logger
) : ICommandHandler<UploadFileCommand, UploadFileResponse>
{
    private static readonly Dictionary<string, string[]> AllowedExtensions = new()
    {
        ["image"] = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" },
        ["video"] = new[] { ".mp4", ".mov", ".avi", ".webm", ".mkv" },
        ["document"] = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt" },
        ["excel"] = new[] { ".xlsx", ".xls", ".csv" }
    };

    private static readonly Dictionary<string, long> MaxFileSizes = new()
    {
        ["image"] = 10 * 1024 * 1024,      // 10MB
        ["video"] = 100 * 1024 * 1024,     // 100MB
        ["document"] = 50 * 1024 * 1024,   // 50MB
        ["excel"] = 20 * 1024 * 1024       // 20MB
    };

    public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
    {
        var fileExtension = Path.GetExtension(command.FileName).ToLowerInvariant();
        
        // Auto-detect resource type from extension
        var resourceType = command.ResourceType == "auto" 
            ? DetectResourceType(fileExtension) 
            : command.ResourceType;

        // Get max size for this type
        var maxSize = MaxFileSizes.GetValueOrDefault(resourceType, MaxFileSizes["document"]);
        
        // Validate file size
        if (command.FileSize > maxSize)
        {
            return Result.Failure<UploadFileResponse>(
                FileErrors.FileSizeExceedsLimit(resourceType, maxSize / 1024 / 1024));
        }

        // Validate file extension
        var allowedExts = AllowedExtensions.GetValueOrDefault(resourceType, AllowedExtensions["document"]);
        if (!allowedExts.Contains(fileExtension))
        {
            return Result.Failure<UploadFileResponse>(
                FileErrors.InvalidFileType(resourceType, allowedExts));
        }

        if (command.UpdateUserAvatar || command.UpdateProfileAvatar)
        {
            var avatarContextResult = await ValidateAvatarUpdateContextAsync(cancellationToken);
            if (avatarContextResult.IsFailure)
            {
                return Result.Failure<UploadFileResponse>(avatarContextResult.Error);
            }
        }

        try
        {
            var url = await fileStorageService.UploadFileAsync(
                command.FileStream,
                command.FileName,
                command.Folder,
                resourceType,
                cancellationToken);
            
            if (command.UpdateUserAvatar || command.UpdateProfileAvatar)
            {
                await UpdateAvatarAsync(command, url, cancellationToken);
            }

            logger.LogInformation("File uploaded successfully: {FileName} -> {Url}", command.FileName, url);

            return Result.Success(new UploadFileResponse
            {
                Url = url,
                FileName = command.FileName,
                Size = command.FileSize,
                Folder = command.Folder,
                ResourceType = resourceType
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading file: {FileName}", command.FileName);
            return Result.Failure<UploadFileResponse>(FileErrors.UploadFailed(ex.Message));
        }
    }

    private async Task<Result> ValidateAvatarUpdateContextAsync(CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == userContext.UserId && u.IsActive && !u.IsDeleted,
                cancellationToken);

        if (user is null)
        {
            return Result.Failure(FileErrors.Unauthorized());
        }

        if (user.Role == UserRole.Parent && !userContext.StudentId.HasValue)
        {
            return Result.Failure(FileErrors.ParentProfileSelectionRequired());
        }

        return Result.Success();
    }

    private async Task UpdateAvatarAsync(UploadFileCommand command, string url, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var hasChanges = false;

        if (command.UpdateUserAvatar)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user is not null)
            {
                user.AvatarUrl = url;
                user.AvatarMimeType = command.ContentType;
                user.AvatarFileSize = command.FileSize;
                user.UpdatedAt = VietnamTime.UtcNow();
                hasChanges = true;
            }
        }

        if (command.UpdateProfileAvatar)
        {
            var targetProfileId = command.TargetProfileId ?? userContext.StudentId;

            Profile? profile;
            if (targetProfileId.HasValue)
            {
                profile = await context.Profiles
                    .FirstOrDefaultAsync(p => p.Id == targetProfileId.Value && !p.IsDeleted, cancellationToken);
            }
            else
            {
                profile = await context.Profiles
                    .Where(p => p.UserId == userId && p.IsActive && !p.IsDeleted)
                    .OrderBy(p => p.ProfileType == ProfileType.Student ? 1 : 0)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (profile is not null)
            {
                profile.AvatarUrl = url;
                profile.AvatarMimeType = command.ContentType;
                profile.AvatarFileSize = command.FileSize;
                profile.UpdatedAt = VietnamTime.UtcNow();
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string DetectResourceType(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" => "image",
            ".mp4" or ".mov" or ".avi" or ".webm" or ".mkv" => "video",
            ".pdf" or ".doc" or ".docx" or ".ppt" or ".pptx" or ".txt" => "document",
            ".xlsx" or ".xls" or ".csv" => "excel",
            _ => "document"
        };
    }
}

