using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Files.Errors;

public static class FileErrors 
{
    public static Error FileSizeExceedsLimit(string resourceType, long maxSizeMB) =>
        Error.Validation(
            "File.SizeExceedsLimit",
            $"File size exceeds maximum allowed size of {maxSizeMB}MB for {resourceType}");

    public static Error InvalidFileType(string resourceType, string[] allowedExtensions) =>
        Error.Validation(
            "File.InvalidFileType",
            $"File type is not allowed for {resourceType}. Allowed types: {string.Join(", ", allowedExtensions)}");

    public static Error UploadFailed(string message) =>
        Error.Failure(
            "File.UploadFailed",
            $"Failed to upload file: {message}");

    public static Error FileNotFound(string url) =>
        Error.NotFound(
            "File.NotFound",
            $"File not found: {url}");

    public static Error DeleteFailed(string message) =>
        Error.Failure(
            "File.DeleteFailed",
            $"Failed to delete file: {message}");

    public static Error UrlRequired() =>
        Error.Validation(
            "File.UrlRequired",
            "URL is required");

    public static Error TransformationFailed(string message) =>
        Error.Failure(
            "File.TransformationFailed",
            $"Failed to transform file: {message}");

    public static Error DownloadUrlGenerationFailed(string message) =>
        Error.Failure(
            "File.DownloadUrlGenerationFailed",
            $"Failed to generate download URL: {message}");

    public static Error NoFileProvided() =>
        Error.Validation(
            "File.NoFileProvided",
            "No file provided");

    public static Error InvalidMimeType(string mimeType, string[] allowedMimeTypes) =>
        Error.Validation(
            "File.InvalidMimeType",
            $"MIME type '{mimeType}' is not allowed. Allowed types: {string.Join(", ", allowedMimeTypes)}");

    public static Error FileTooSmall(long minSize) =>
        Error.Validation(
            "File.FileTooSmall",
            $"File size is too small. Minimum size: {minSize} bytes");

    public static Error StorageError(string message) =>
        Error.Failure(
            "File.StorageError",
            $"Storage error: {message}");

    public static Error Unauthorized() =>
        Error.Validation(
            "File.Unauthorized",
            "Unauthorized to perform this operation");

    public static Error ParentProfileSelectionRequired() =>
        Error.Validation(
            "File.ParentProfileSelectionRequired",
            "Parent account must select a student profile before updating avatar");
}

