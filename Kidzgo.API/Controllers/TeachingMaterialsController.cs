using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.TeachingMaterials.DownloadTeachingMaterial;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterialById;
using Kidzgo.Application.TeachingMaterials.GetLessonBundle;
using Kidzgo.Application.TeachingMaterials.GetTeachingMaterials;
using Kidzgo.Application.TeachingMaterials.ImportTeachingMaterials;
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
    private readonly ISender _mediator;

    public TeachingMaterialsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin, Parent")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin,Parent")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin,Parent")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin,Parent")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin,Parent")]
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

}
