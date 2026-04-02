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
using System.IO.Compression;

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
        var importedFiles = new List<ImportTeachingMaterialFile>();
        var openedStreams = new List<Stream>();

        try
        {
            if (request.Archive is not null)
            {
                if (!request.Archive.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.BadRequest(new { error = "Only .zip archives are supported for archive upload" });
                }

                await AddArchiveEntriesAsync(request.Archive, importedFiles, openedStreams, cancellationToken);
            }

            if (request.File is not null && request.File.Length > 0)
            {
                var stream = request.File.OpenReadStream();
                openedStreams.Add(stream);
                importedFiles.Add(new ImportTeachingMaterialFile
                {
                    FileName = request.File.FileName,
                    RelativePath = request.File.FileName,
                    ContentType = request.File.ContentType,
                    FileSize = request.File.Length,
                    FileStream = stream
                });
            }

            if (request.Files is not null)
            {
                for (var index = 0; index < request.Files.Count; index++)
                {
                    var file = request.Files[index];
                    if (file.Length <= 0)
                    {
                        continue;
                    }

                    var stream = file.OpenReadStream();
                    openedStreams.Add(stream);
                    importedFiles.Add(new ImportTeachingMaterialFile
                    {
                        FileName = file.FileName,
                        RelativePath = ResolveRelativePath(request.RelativePaths, index, file.FileName),
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        FileStream = stream
                    });
                }
            }

            if (importedFiles.Count == 0)
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
                Files = importedFiles
            };

            var result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }
        finally
        {
            foreach (var stream in openedStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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

    private static string ResolveRelativePath(IReadOnlyList<string>? relativePaths, int index, string fallbackFileName)
    {
        if (relativePaths is null || index >= relativePaths.Count || string.IsNullOrWhiteSpace(relativePaths[index]))
        {
            return fallbackFileName;
        }

        return relativePaths[index];
    }

    private static async Task AddArchiveEntriesAsync(
        IFormFile archiveFile,
        ICollection<ImportTeachingMaterialFile> importedFiles,
        ICollection<Stream> openedStreams,
        CancellationToken cancellationToken)
    {
        await using var archiveStream = archiveFile.OpenReadStream();
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
        var archiveRoot = Path.GetFileNameWithoutExtension(archiveFile.FileName);

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }

            await using var entryStream = entry.Open();
            var memoryStream = new MemoryStream();
            await entryStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            openedStreams.Add(memoryStream);

            importedFiles.Add(new ImportTeachingMaterialFile
            {
                FileName = entry.Name,
                RelativePath = NormalizeArchiveEntryPath(entry.FullName, archiveRoot),
                ContentType = null,
                FileSize = memoryStream.Length,
                FileStream = memoryStream
            });
        }
    }

    private static string NormalizeArchiveEntryPath(string entryFullName, string archiveRoot)
    {
        var normalized = entryFullName
            .Replace('\\', '/')
            .Trim()
            .Trim('/');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return archiveRoot;
        }

        return normalized.Contains('/', StringComparison.Ordinal)
            ? normalized
            : $"{archiveRoot}/{normalized}";
    }
}
