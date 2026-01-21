namespace Kidzgo.Application.Abstraction.Storage;

/// Service for uploading files (images and videos) to cloud storage
public interface IFileStorageService
{
    /// Upload a file (image or video) and return the public URL
    /// <param name="fileStream">File stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folder">Folder path in storage (e.g., "tickets", "media", "blog")</param>
    /// <param name="resourceType">Resource type: "image" or "video"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string folder,
        string resourceType = "auto",
        CancellationToken cancellationToken = default);

    /// Delete a file from storage by its public URL
    /// <param name="publicUrl">Public URL of the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteFileAsync(string publicUrl, CancellationToken cancellationToken = default);

    /// Get optimized/transformed URL for an image (e.g., resize, format conversion)
    /// <param name="publicUrl">Original public URL</param>
    /// <param name="width">Desired width (optional)</param>
    /// <param name="height">Desired height (optional)</param>
    /// <param name="format">Desired format (optional, e.g., "webp", "jpg")</param>
    /// <returns>Transformed URL</returns>
    string GetTransformedUrl(string publicUrl, int? width = null, int? height = null, string? format = null);
}

