using Kidzgo.API.Extensions;
using Kidzgo.Application.Abstraction.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/files")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        IFileStorageService fileStorageService,
        ILogger<FileUploadController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// Upload a file (image or video) to cloud storage
    /// <param name="file">File to upload</param>
    /// <param name="folder">Folder name (e.g., "tickets", "media", "blog"). Default: "uploads"</param>
    /// <param name="resourceType">Resource type: "image", "video", or "auto" (detect from extension). Default: "auto"</param>
    /// <returns>Public URL of the uploaded file</returns>
    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(100_000_000)] // 100MB max
    public async Task<IResult> UploadFile(
        IFormFile file,
        [FromQuery] string folder = "uploads",
        [FromQuery] string resourceType = "auto",
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        // Validate file size (100MB max)
        const long maxFileSize = 100_000_000; // 100MB
        if (file.Length > maxFileSize)
        {
            return Results.BadRequest(new { error = $"File size exceeds maximum allowed size of {maxFileSize / 1_000_000}MB" });
        }

        // Validate file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg", ".mp4", ".mov", ".avi", ".wmv", ".flv", ".webm" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return Results.BadRequest(new { error = $"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var url = await _fileStorageService.UploadFileAsync(
                stream,
                file.FileName,
                folder,
                resourceType,
                cancellationToken);

            _logger.LogInformation("File uploaded successfully: {FileName} -> {Url}", file.FileName, url);

            return Results.Ok(new
            {
                url = url,
                fileName = file.FileName,
                size = file.Length,
                folder = folder
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            return Results.Problem(
                title: "File upload failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    /// Delete a file from cloud storage
    /// <param name="url">Public URL of the file to delete</param>
    [HttpDelete]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DeleteFile(
        [FromQuery] string url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url))
        {
            return Results.BadRequest(new { error = "URL is required" });
        }

        try
        {
            var deleted = await _fileStorageService.DeleteFileAsync(url, cancellationToken);
            
            if (deleted)
            {
                return Results.Ok(new { message = "File deleted successfully" });
            }

            return Results.NotFound(new { error = "File not found or could not be deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Url}", url);
            return Results.Problem(
                title: "File deletion failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    /// Get transformed/optimized URL for an image
    /// <param name="url">Original public URL</param>
    /// <param name="width">Desired width (optional)</param>
    /// <param name="height">Desired height (optional)</param>
    /// <param name="format">Desired format: "webp", "jpg", "png" (optional)</param>
    /// <returns>Transformed URL</returns>
    [HttpGet("transform")]
    [AllowAnonymous]
    public IResult GetTransformedUrl(
        [FromQuery] string url,
        [FromQuery] int? width = null,
        [FromQuery] int? height = null,
        [FromQuery] string? format = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            return Results.BadRequest(new { error = "URL is required" });
        }

        try
        {
            var transformedUrl = _fileStorageService.GetTransformedUrl(url, width, height, format);
            return Results.Ok(new { url = transformedUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transformed URL: {Url}", url);
            return Results.Problem(
                title: "URL transformation failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}

