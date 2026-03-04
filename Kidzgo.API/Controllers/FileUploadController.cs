using Kidzgo.API.Extensions;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Files.UploadFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/files")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        ISender mediator,
        IFileStorageService fileStorageService,
        ILogger<FileUploadController> logger)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload any file (auto-detect type from extension)
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(100_000_000)]
    public async Task<IResult> UploadFile(
        IFormFile file,
        [FromQuery] string folder = "general",
        [FromQuery] string resourceType = "auto",
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new UploadFileCommand
        {
            FileName = file.FileName,
            FileSize = file.Length,
            Folder = folder,
            ResourceType = resourceType,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        return result.Match(
            success => Results.Ok(new
            {
                url = success.Url,
                fileName = success.FileName,
                size = success.Size,
                folder = success.Folder,
                resourceType = success.ResourceType
            }),
            failure => Results.BadRequest(new { error = failure.Error.Description })
        );
    }

    /// <summary>
    /// Upload avatar for user
    /// </summary>
    [HttpPost("avatar")]
    [Authorize]
    [RequestSizeLimit(5_242_880)]
    public async Task<IResult> UploadAvatar(
        IFormFile file,
        [FromQuery] Guid? profileId = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "current-user";

        var command = new UploadFileCommand
        {
            FileName = file.FileName,
            FileSize = file.Length,
            Folder = $"avatars/{userId}",
            ResourceType = "image",
            ContentType = file.ContentType,
            UpdateUserAvatar = true,
            UpdateProfileAvatar = true,
            TargetProfileId = profileId,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        return result.Match(
            success => Results.Ok(new
            {
                url = success.Url,
                fileName = success.FileName,
                size = success.Size
            }),
            failure => Results.BadRequest(new { error = failure.Error.Description })
        );
    }

    /// <summary>
    /// Upload blog media (image or video)
    /// </summary>
    [HttpPost("blog")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IResult> UploadBlogMedia(
        IFormFile file,
        [FromQuery] bool isVideo = false,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var folder = isVideo ? "blog/videos" : "blog/images";
        var resourceType = isVideo ? "video" : "image";

        var command = new UploadFileCommand
        {
            FileName = file.FileName,
            FileSize = file.Length,
            Folder = folder,
            ResourceType = resourceType,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        return result.Match(
            success => Results.Ok(new
            {
                url = success.Url,
                fileName = success.FileName,
                size = success.Size,
                folder = success.Folder,
                resourceType = success.ResourceType
            }),
            failure => Results.BadRequest(new { error = failure.Error.Description })
        );
    }

    /// <summary>
    /// Upload lesson plan media (image or video)
    /// </summary>
    [HttpPost("lesson-plan/media")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IResult> UploadLessonPlanMedia(
        IFormFile file,
        [FromQuery] bool isVideo = false,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var folder = isVideo ? "lesson-plans/videos" : "lesson-plans/images";
        var resourceType = isVideo ? "video" : "image";

        var command = new UploadFileCommand
        {
            FileName = file.FileName,
            FileSize = file.Length,
            Folder = folder,
            ResourceType = resourceType,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        return result.Match(
            success => Results.Ok(new
            {
                url = success.Url,
                fileName = success.FileName,
                size = success.Size,
                folder = success.Folder,
                resourceType = success.ResourceType
            }),
            failure => Results.BadRequest(new { error = failure.Error.Description })
        );
    }

    /// <summary>
    /// Upload lesson plan template document
    /// </summary>
    [HttpPost("lesson-plan/template")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IResult> UploadLessonPlanTemplate(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new UploadFileCommand
        {
            FileName = file.FileName,
            FileSize = file.Length,
            Folder = "lesson-plan-templates",
            ResourceType = "document",
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        return result.Match(
            success => Results.Ok(new
            {
                url = success.Url,
                fileName = success.FileName,
                size = success.Size,
                folder = success.Folder,
                resourceType = success.ResourceType
            }),
            failure => Results.BadRequest(new { error = failure.Error.Description })
        );
    }

    /// <summary>
    /// Upload lesson plan materials
    /// </summary>
    [HttpPost("lesson-plan/materials")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IResult> UploadLessonPlanMaterials(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new UploadFileCommand
        {
            FileName = file.FileName,
            FileSize = file.Length,
            Folder = "lesson-plan-materials",
            ResourceType = "document",
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        return result.Match(
            success => Results.Ok(new
            {
                url = success.Url,
                fileName = success.FileName,
                size = success.Size,
                folder = success.Folder,
                resourceType = success.ResourceType
            }),
            failure => Results.BadRequest(new { error = failure.Error.Description })
        );
    }

    /// <summary>
    /// Upload monthly Errorreport (Excel)
    /// </summary>
    [HttpPost("reports/monthly")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(20_971_520)]
    public async Task<IResult> UploadMonthlyReport(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new UploadFileCommand
        {
            FileName = file.FileName,
            FileSize = file.Length,
            Folder = "reports/monthly",
            ResourceType = "excel",
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        return result.Match(
            success => Results.Ok(new
            {
                url = success.Url,
                fileName = success.FileName,
                size = success.Size,
                folder = success.Folder,
                resourceType = success.ResourceType
            }),
            failure => Results.BadRequest(new { error = failure.Error.Description })
        );
    }

    /// <summary>
    /// Delete a file from storage
    /// </summary>
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
            return Results.Problem(title: "Deletion failed", detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// Get transformed URL for image
    /// </summary>
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
            return Results.Problem(title: "Transformation failed", detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// Get download URL for a file
    /// </summary>
    [HttpGet("download")]
    [AllowAnonymous]
    public IResult GetDownloadUrl([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return Results.BadRequest(new { error = "URL is required" });
        }

        try
        {
            var downloadUrl = _fileStorageService.GetDownloadUrl(url);
            return Results.Ok(new { url = downloadUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL: {Url}", url);
            return Results.Problem(title: "Download URL generation failed", detail: ex.Message, statusCode: 500);
        }
    }
}
