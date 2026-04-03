using System.IO.Compression;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials.Errors;

namespace Kidzgo.Application.TeachingMaterials.ImportTeachingMaterials;

public static class ImportTeachingMaterialsUploadBuilder
{
    public static async Task<Result<ImportTeachingMaterialsUploadPackage>> BuildAsync(
        ImportTeachingMaterialsUploadRequest request,
        CancellationToken cancellationToken)
    {
        var importedFiles = new List<ImportTeachingMaterialFile>();
        var openedStreams = new List<Stream>();

        try
        {
            if (request.Archive is not null)
            {
                openedStreams.Add(request.Archive.FileStream);

                if (!request.Archive.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    await DisposeStreamsAsync(openedStreams);
                    return Result.Failure<ImportTeachingMaterialsUploadPackage>(TeachingMaterialErrors.OnlyZipArchivesSupported());
                }

                await AddArchiveEntriesAsync(request.Archive, importedFiles, openedStreams, cancellationToken);
            }

            if (request.File is not null && request.File.FileSize > 0)
            {
                openedStreams.Add(request.File.FileStream);
                importedFiles.Add(new ImportTeachingMaterialFile
                {
                    FileName = request.File.FileName,
                    RelativePath = request.File.FileName,
                    ContentType = request.File.ContentType,
                    FileSize = request.File.FileSize,
                    FileStream = request.File.FileStream
                });
            }

            if (request.Files is not null)
            {
                for (var index = 0; index < request.Files.Count; index++)
                {
                    var file = request.Files[index];
                    if (file.FileSize <= 0)
                    {
                        continue;
                    }

                    openedStreams.Add(file.FileStream);
                    importedFiles.Add(new ImportTeachingMaterialFile
                    {
                        FileName = file.FileName,
                        RelativePath = ResolveRelativePath(request.RelativePaths, index, file.FileName),
                        ContentType = file.ContentType,
                        FileSize = file.FileSize,
                        FileStream = file.FileStream
                    });
                }
            }

            return new ImportTeachingMaterialsUploadPackage(importedFiles, openedStreams);
        }
        catch
        {
            await DisposeStreamsAsync(openedStreams);
            throw;
        }
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
        ImportTeachingMaterialUploadSource archiveFile,
        ICollection<ImportTeachingMaterialFile> importedFiles,
        ICollection<Stream> openedStreams,
        CancellationToken cancellationToken)
    {
        using var archive = new ZipArchive(archiveFile.FileStream, ZipArchiveMode.Read, leaveOpen: true);
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

    private static async Task DisposeStreamsAsync(IEnumerable<Stream> streams)
    {
        foreach (var stream in streams.Distinct())
        {
            await stream.DisposeAsync();
        }
    }
}

public sealed class ImportTeachingMaterialsUploadRequest
{
    public ImportTeachingMaterialUploadSource? File { get; init; }
    public IReadOnlyList<ImportTeachingMaterialUploadSource>? Files { get; init; }
    public IReadOnlyList<string>? RelativePaths { get; init; }
    public ImportTeachingMaterialUploadSource? Archive { get; init; }
}

public sealed class ImportTeachingMaterialUploadSource
{
    public string FileName { get; init; } = null!;
    public string? ContentType { get; init; }
    public long FileSize { get; init; }
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportTeachingMaterialsUploadPackage : IAsyncDisposable
{
    private readonly IReadOnlyCollection<Stream> _openedStreams;

    public ImportTeachingMaterialsUploadPackage(
        IReadOnlyCollection<ImportTeachingMaterialFile> files,
        IReadOnlyCollection<Stream> openedStreams)
    {
        Files = files;
        _openedStreams = openedStreams;
    }

    public IReadOnlyCollection<ImportTeachingMaterialFile> Files { get; }

    public async ValueTask DisposeAsync()
    {
        foreach (var stream in _openedStreams.Distinct())
        {
            await stream.DisposeAsync();
        }
    }
}
