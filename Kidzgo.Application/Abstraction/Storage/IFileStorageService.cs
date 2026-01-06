namespace Kidzgo.Application.Abstraction.Storage;

/// <summary>
/// Service for uploading files (images and videos) to cloud storage
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file (image or video) and return the public URL
    /// </summary>
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

    /// <summary>
    /// Delete a file from storage by its public URL
    /// </summary>
    /// <param name="publicUrl">Public URL of the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteFileAsync(string publicUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get optimized/transformed URL for an image (e.g., resize, format conversion)
    /// </summary>
    /// <param name="publicUrl">Original public URL</param>
    /// <param name="width">Desired width (optional)</param>
    /// <param name="height">Desired height (optional)</param>
    /// <param name="format">Desired format (optional, e.g., "webp", "jpg")</param>
    /// <returns>Transformed URL</returns>
    string GetTransformedUrl(string publicUrl, int? width = null, int? height = null, string? format = null);
}

