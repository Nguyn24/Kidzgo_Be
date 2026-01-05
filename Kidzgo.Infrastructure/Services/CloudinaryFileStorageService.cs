using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Kidzgo.Application.Abstraction.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryFileStorageService> _logger;

    public CloudinaryFileStorageService(IConfiguration configuration, ILogger<CloudinaryFileStorageService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary configuration is missing. Please set Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret in appsettings.json");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string folder,
        string resourceType = "auto",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Determine resource type from file extension if "auto"
            if (resourceType == "auto")
            {
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                resourceType = extension switch
                {
                    ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" => "image",
                    ".mp4" or ".mov" or ".avi" or ".wmv" or ".flv" or ".webm" => "video",
                    _ => "image" // Default to image
                };
            }

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var publicId = $"{folder}/{Path.GetFileNameWithoutExtension(uniqueFileName)}";

            RawUploadResult uploadResult;
            if (resourceType == "video")
            {
                var videoUploadParams = new VideoUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    PublicId = publicId,
                    Folder = folder,
                    Overwrite = false,
                    UseFilename = false,
                    UniqueFilename = true,
                    EagerTransforms = new List<Transformation>
                    {
                        new Transformation().Width(1280).Height(720).Crop("limit"),
                        new Transformation().Width(640).Height(360).Crop("limit")
                    },
                    EagerAsync = false
                };
                var videoResult = await _cloudinary.UploadAsync(videoUploadParams);
                uploadResult = new RawUploadResult
                {
                    StatusCode = videoResult.StatusCode,
                    SecureUrl = videoResult.SecureUrl,
                    Url = videoResult.Url,
                    PublicId = videoResult.PublicId,
                    Error = videoResult.Error
                };
            }
            else
            {
                var imageUploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    PublicId = publicId,
                    Folder = folder,
                    Overwrite = false,
                    UseFilename = false,
                    UniqueFilename = true,
                    EagerTransforms = new List<Transformation>
                    {
                        new Transformation().Width(1920).Height(1080).Crop("limit").Quality("auto"),
                        new Transformation().Width(800).Height(600).Crop("limit").Quality("auto"),
                        new Transformation().Width(400).Height(300).Crop("limit").Quality("auto")
                    }
                };
                var imageResult = await _cloudinary.UploadAsync(imageUploadParams);
                uploadResult = new RawUploadResult
                {
                    StatusCode = imageResult.StatusCode,
                    SecureUrl = imageResult.SecureUrl,
                    Url = imageResult.Url,
                    PublicId = imageResult.PublicId,
                    Error = imageResult.Error
                };
            }

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
                throw new Exception($"Failed to upload file: {uploadResult.Error?.Message}");
            }

            _logger.LogInformation("File uploaded successfully: {PublicId}, URL: {Url}", publicId, uploadResult.SecureUrl);
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Cloudinary: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string publicUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract public ID from URL
            var publicId = ExtractPublicIdFromUrl(publicUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Could not extract public ID from URL: {Url}", publicUrl);
                return false;
            }

            // Determine resource type from URL
            var resourceType = publicUrl.Contains("/video/") ? ResourceType.Video : ResourceType.Image;

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            
            if (result.Result == "ok")
            {
                _logger.LogInformation("File deleted successfully: {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning("Failed to delete file: {PublicId}, Result: {Result}", publicId, result.Result);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Cloudinary: {Url}", publicUrl);
            return false;
        }
    }

    public string GetTransformedUrl(string publicUrl, int? width = null, int? height = null, string? format = null)
    {
        try
        {
            var publicId = ExtractPublicIdFromUrl(publicUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                return publicUrl; // Return original if can't extract
            }

            var resourceType = publicUrl.Contains("/video/") ? ResourceType.Video : ResourceType.Image;
            var transformation = new Transformation();

            if (width.HasValue || height.HasValue)
            {
                transformation = transformation.Width(width ?? 0).Height(height ?? 0).Crop("limit");
            }

            if (!string.IsNullOrEmpty(format))
            {
                // Format is added as a parameter in the transformation string
                transformation = transformation.Chain().FetchFormat(format);
            }

            transformation = transformation.Quality("auto");

            string url;
            if (resourceType == ResourceType.Video)
            {
                url = _cloudinary.Api.UrlVideoUp.Transform(transformation).BuildUrl(publicId);
            }
            else
            {
                url = _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
            }

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transformed URL: {Url}", publicUrl);
            return publicUrl; // Return original on error
        }
    }

    private string? ExtractPublicIdFromUrl(string url)
    {
        try
        {
            // Cloudinary URL format: https://res.cloudinary.com/{cloud_name}/{resource_type}/upload/{version}/{public_id}.{format}
            var uri = new Uri(url);
            var segments = uri.Segments;
            
            // Find the index of "upload" segment
            var uploadIndex = Array.IndexOf(segments, "upload/");
            if (uploadIndex == -1)
            {
                return null;
            }

            // Get everything after "upload/" and before the last segment (which is the file extension)
            var publicIdSegments = segments.Skip(uploadIndex + 1).ToList();
            if (publicIdSegments.Count == 0)
            {
                return null;
            }

            // Remove version if present (format: v1234567890/)
            if (publicIdSegments[0].StartsWith("v") && publicIdSegments[0].EndsWith("/"))
            {
                publicIdSegments.RemoveAt(0);
            }

            // Join segments and remove file extension
            var publicId = string.Join("", publicIdSegments);
            var lastDotIndex = publicId.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                publicId = publicId.Substring(0, lastDotIndex);
            }

            return publicId;
        }
        catch
        {
            return null;
        }
    }
}

