using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;
using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialBookmark;
using Kidzgo.Application.TeachingMaterials.DeleteTeachingMaterialAnnotation;
using Kidzgo.Application.TeachingMaterials.DeleteTeachingMaterialBookmark;
using Kidzgo.Application.TeachingMaterials.DownloadTeachingMaterial;
using Kidzgo.Application.TeachingMaterials.GetLessonMaterialProgress;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialById;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialAnnotations;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialBookmarks;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialPreviewPdf;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlideImage;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlides;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialViewProgress;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialViewProgressSummary;
using Kidzgo.Application.TeachingMaterials.GetLessonBundle;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterials;
using Kidzgo.Application.TeachingMaterials.ImportTeachingMaterials;
using Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialAnnotation;
using Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialViewProgress;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/teaching-materials")]
[ApiController]
[Authorize]
public class TeachingMaterialsController : ControllerBase
{
    private const string WriteRoles = "Teacher,ManagementStaff,Admin";
    private const string ReadRoles = "Teacher,ManagementStaff,Admin,Student,Parent";
    private const string SummaryRoles = "Teacher,Admin";

    private readonly ISender _mediator;

    public TeachingMaterialsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize(Roles = WriteRoles)]
    [RequestSizeLimit(314_572_800)]
    public async Task<IResult> Upload(
        [FromForm] UploadTeachingMaterialRequest request,
        CancellationToken cancellationToken = default)
    {
        var uploadBuildResult = await ImportTeachingMaterialsUploadBuilder.BuildAsync(
            new ImportTeachingMaterialsUploadRequest
            {
                Archive = request.Archive is null
                    ? null
                    : new ImportTeachingMaterialUploadSource
                    {
                        FileName = request.Archive.FileName,
                        ContentType = request.Archive.ContentType,
                        FileSize = request.Archive.Length,
                        FileStream = request.Archive.OpenReadStream()
                    },
                File = request.File is null
                    ? null
                    : new ImportTeachingMaterialUploadSource
                    {
                        FileName = request.File.FileName,
                        ContentType = request.File.ContentType,
                        FileSize = request.File.Length,
                        FileStream = request.File.OpenReadStream()
                    },
                Files = request.Files?.Select(file => new ImportTeachingMaterialUploadSource
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FileStream = file.OpenReadStream()
                }).ToList(),
                RelativePaths = request.RelativePaths
            },
            cancellationToken);

        if (uploadBuildResult.IsFailure)
        {
            return CustomResults.Problem(uploadBuildResult);
        }

        await using var uploadPackage = uploadBuildResult.Value;

        if (uploadPackage.Files.Count == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new ImportTeachingMaterialsCommand
        {
            ProgramId = request.ProgramId,
            UnitNumber = request.UnitNumber,
            LessonNumber = request.LessonNumber,
            LessonTitle = request.LessonTitle,
            DisplayName = request.DisplayName,
            Category = request.Category,
            Files = uploadPackage.Files
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> Get(
        [FromQuery] Guid? programId,
        [FromQuery] int? unitNumber,
        [FromQuery] int? lessonNumber,
        [FromQuery] string? fileType,
        [FromQuery] string? category,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeachingMaterialsQuery
        {
            ProgramId = programId,
            UnitNumber = unitNumber,
            LessonNumber = lessonNumber,
            FileType = fileType,
            Category = category,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("lesson-bundle")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetLessonBundle(
        [FromQuery] Guid programId,
        [FromQuery] int unitNumber,
        [FromQuery] int lessonNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetLessonBundleQuery
        {
            ProgramId = programId,
            UnitNumber = unitNumber,
            LessonNumber = lessonNumber
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialByIdQuery
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}/preview")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> Preview(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DownloadTeachingMaterialQuery
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.Match(
            success => Results.File(success.Content, success.MimeType, enableRangeProcessing: true),
            failure => CustomResults.Problem(failure));
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> Download(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DownloadTeachingMaterialQuery
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.Match(
            success => Results.File(success.Content, success.MimeType, success.FileName),
            failure => CustomResults.Problem(failure));
    }

    [HttpGet("{id:guid}/preview-pdf")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> PreviewPdf(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialPreviewPdfQuery
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.Match(
            success => Results.File(success.Content, success.MimeType, enableRangeProcessing: true),
            failure => CustomResults.Problem(failure));
    }

    [HttpGet("{id:guid}/slides")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetSlides(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialSlidesQuery
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}/slides/{slideNumber:int}/preview")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetSlidePreview(
        Guid id,
        int slideNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialSlideImageQuery
        {
            TeachingMaterialId = id,
            SlideNumber = slideNumber,
            ImageKind = TeachingMaterialSlideImageKind.Preview
        }, cancellationToken);

        return result.Match(
            success => Results.File(success.Content, success.MimeType, enableRangeProcessing: true),
            failure => CustomResults.Problem(failure));
    }

    [HttpGet("{id:guid}/slides/{slideNumber:int}/thumbnail")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetSlideThumbnail(
        Guid id,
        int slideNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialSlideImageQuery
        {
            TeachingMaterialId = id,
            SlideNumber = slideNumber,
            ImageKind = TeachingMaterialSlideImageKind.Thumbnail
        }, cancellationToken);

        return result.Match(
            success => Results.File(success.Content, success.MimeType, enableRangeProcessing: true),
            failure => CustomResults.Problem(failure));
    }

    [HttpPost("{id:guid}/view-progress")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> UpdateViewProgress(
        Guid id,
        [FromBody] UpdateTeachingMaterialViewProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new UpdateTeachingMaterialViewProgressCommand
        {
            TeachingMaterialId = id,
            ProgressPercent = request.ProgressPercent,
            LastSlideViewed = request.LastSlideViewed,
            TotalTimeSeconds = request.TotalTimeSeconds
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}/view-progress")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetViewProgress(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialViewProgressQuery
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}/view-progress/summary")]
    [Authorize(Roles = SummaryRoles)]
    public async Task<IResult> GetViewProgressSummary(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialViewProgressSummaryQuery
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("lesson-progress")]
    [Authorize(Roles = SummaryRoles)]
    public async Task<IResult> GetLessonProgress(
        [FromQuery] Guid programId,
        [FromQuery] int unitNumber,
        [FromQuery] int lessonNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetLessonMaterialProgressQuery
        {
            ProgramId = programId,
            UnitNumber = unitNumber,
            LessonNumber = lessonNumber
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("{id:guid}/bookmark")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> CreateBookmark(
        Guid id,
        [FromBody] CreateTeachingMaterialBookmarkRequest? request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new CreateTeachingMaterialBookmarkCommand
        {
            TeachingMaterialId = id,
            Note = request?.Note
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("{id:guid}/bookmark")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> DeleteBookmark(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteTeachingMaterialBookmarkCommand
        {
            TeachingMaterialId = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("bookmarks")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetBookmarks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialBookmarksQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("{id:guid}/annotations")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> CreateAnnotation(
        Guid id,
        [FromBody] CreateTeachingMaterialAnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new CreateTeachingMaterialAnnotationCommand
        {
            TeachingMaterialId = id,
            SlideNumber = request.SlideNumber,
            Content = request.Content,
            Color = request.Color,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Type = request.Type,
            Visibility = request.Visibility
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("{id:guid}/annotations")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> GetAnnotations(
        Guid id,
        [FromQuery] int? slideNumber,
        [FromQuery] string visibility = "All",
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTeachingMaterialAnnotationsQuery
        {
            TeachingMaterialId = id,
            SlideNumber = slideNumber,
            Visibility = visibility
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPut("annotations/{annotationId:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> UpdateAnnotation(
        Guid annotationId,
        [FromBody] UpdateTeachingMaterialAnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new UpdateTeachingMaterialAnnotationCommand
        {
            AnnotationId = annotationId,
            Content = request.Content,
            Color = request.Color,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Type = request.Type,
            Visibility = request.Visibility
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("annotations/{annotationId:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IResult> DeleteAnnotation(
        Guid annotationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteTeachingMaterialAnnotationCommand
        {
            AnnotationId = annotationId
        }, cancellationToken);

        return result.MatchOk();
    }
}
