using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Files.Errors;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Files.UploadFile;

public sealed class UploadFileCommandHandler(
    IFileStorageService fileStorageService,
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

        try
        {
            var url = await fileStorageService.UploadFileAsync(
                command.FileStream,
                command.FileName,
                command.Folder,
                resourceType,
                cancellationToken);

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

