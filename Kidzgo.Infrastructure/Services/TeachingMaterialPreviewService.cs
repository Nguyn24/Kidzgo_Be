using System.Buffers.Binary;
using System.IO.Compression;
using System.Diagnostics;
using System.Xml.Linq;
using Kidzgo.Application.Abstraction.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.Services;

public sealed class TeachingMaterialPreviewService(
    IConfiguration configuration,
    ILogger<TeachingMaterialPreviewService> logger
) : ITeachingMaterialPreviewService
{
    private readonly string _storageRoot = ResolveStorageRoot(configuration);
    private readonly string _libreOfficePath = configuration["TeachingMaterials:LibreOfficePath"]
                                               ?? configuration["LibreOffice:Path"]
                                               ?? "soffice";

    public async Task<TeachingMaterialPdfPreviewFile> GeneratePdfPreviewAsync(
        Guid materialId,
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var tempDirectory = CreateTempDirectory(materialId);
        try
        {
            var inputPath = await WriteInputAsync(tempDirectory, originalFileName, content, cancellationToken);
            await RunLibreOfficeAsync(
                $"--headless --convert-to pdf --outdir \"{tempDirectory}\" \"{inputPath}\"",
                tempDirectory,
                cancellationToken);

            var outputPath = Directory
                .EnumerateFiles(tempDirectory, "*.pdf")
                .FirstOrDefault(path => !path.Equals(inputPath, StringComparison.OrdinalIgnoreCase));

            if (outputPath is null || !File.Exists(outputPath))
            {
                throw new InvalidOperationException("LibreOffice did not create a PDF output file");
            }

            var cachePath = Path.Combine("previews", materialId.ToString("N"), $"{materialId:N}_preview.pdf");
            var absoluteCachePath = ResolveCachePath(cachePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteCachePath)!);
            File.Copy(outputPath, absoluteCachePath, overwrite: true);

            return new TeachingMaterialPdfPreviewFile
            {
                CachePath = cachePath,
                Content = await File.ReadAllBytesAsync(absoluteCachePath, cancellationToken)
            };
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    public async Task<IReadOnlyList<TeachingMaterialSlidePreviewFile>> GenerateSlidePreviewsAsync(
        Guid materialId,
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var tempDirectory = CreateTempDirectory(materialId);
        try
        {
            var inputPath = await WriteInputAsync(tempDirectory, originalFileName, content, cancellationToken);
            var notesBySlide = ExtractSpeakerNotesBySlide(originalFileName, content);
            await RunLibreOfficeAsync(
                $"--headless --convert-to png --outdir \"{tempDirectory}\" \"{inputPath}\"",
                tempDirectory,
                cancellationToken);

            var images = Directory
                .EnumerateFiles(tempDirectory, "*.png")
                .Where(path => !path.Equals(inputPath, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (images.Count == 0)
            {
                throw new InvalidOperationException("LibreOffice did not create slide image output files");
            }

            var slideFiles = new List<TeachingMaterialSlidePreviewFile>();
            var cacheFolder = Path.Combine("previews", materialId.ToString("N"), "slides");

            for (var index = 0; index < images.Count; index++)
            {
                var slideNumber = index + 1;
                var previewCachePath = Path.Combine(cacheFolder, $"{materialId:N}_slide_{slideNumber}.png");
                var thumbnailCachePath = Path.Combine(cacheFolder, $"{materialId:N}_thumb_{slideNumber}.png");
                var previewAbsolutePath = ResolveCachePath(previewCachePath);
                var thumbnailAbsolutePath = ResolveCachePath(thumbnailCachePath);

                Directory.CreateDirectory(Path.GetDirectoryName(previewAbsolutePath)!);
                File.Copy(images[index], previewAbsolutePath, overwrite: true);
                CreateThumbnail(images[index], thumbnailAbsolutePath, 300);

                var (width, height) = ReadPngDimensions(images[index]);

                slideFiles.Add(new TeachingMaterialSlidePreviewFile
                {
                    SlideNumber = slideNumber,
                    PreviewImagePath = previewCachePath,
                    ThumbnailImagePath = thumbnailCachePath,
                    Width = width,
                    Height = height,
                    Notes = notesBySlide.TryGetValue(slideNumber, out var notes) ? notes : null
                });
            }

            return slideFiles;
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private void CreateThumbnail(string sourceImagePath, string targetImagePath, int targetWidth)
    {
        try
        {
            // Keep thumbnail generation dependency-free for now.
            // If a dedicated image processing library is introduced later,
            // this can be replaced with an actual resize implementation.
            _ = targetWidth;
            File.Copy(sourceImagePath, targetImagePath, overwrite: true);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to prepare thumbnail for teaching material preview '{SourceImagePath}'. Falling back to the full-size PNG.",
                sourceImagePath);

            File.Copy(sourceImagePath, targetImagePath, overwrite: true);
        }
    }

    private static (int Width, int Height) ReadPngDimensions(string imagePath)
    {
        const int requiredBytes = 24;
        var buffer = new byte[requiredBytes];

        using var stream = File.OpenRead(imagePath);
        var bytesRead = stream.Read(buffer, 0, requiredBytes);
        if (bytesRead < requiredBytes)
        {
            return (1920, 1080);
        }

        if (buffer[12] != (byte)'I' || buffer[13] != (byte)'H' || buffer[14] != (byte)'D' || buffer[15] != (byte)'R')
        {
            return (1920, 1080);
        }

        var width = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(20, 4));

        return width > 0 && height > 0 ? (width, height) : (1920, 1080);
    }

    private IReadOnlyDictionary<int, string?> ExtractSpeakerNotesBySlide(string originalFileName, byte[] content)
    {
        if (!string.Equals(Path.GetExtension(originalFileName), ".pptx", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<int, string?>();
        }

        try
        {
            using var stream = new MemoryStream(content, writable: false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

            var presentationDocument = LoadXmlDocument(archive, "ppt/presentation.xml");
            var presentationRelationships = LoadXmlDocument(archive, "ppt/_rels/presentation.xml.rels");
            if (presentationDocument is null || presentationRelationships is null)
            {
                return new Dictionary<int, string?>();
            }

            XNamespace presentationNamespace = "http://schemas.openxmlformats.org/presentationml/2006/main";
            XNamespace relationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            XNamespace packageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";

            var slideTargetsByRelationshipId = presentationRelationships.Root?
                .Elements(packageRelationshipNamespace + "Relationship")
                .Where(element =>
                    string.Equals(
                        (string?)element.Attribute("Type"),
                        "http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide",
                        StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    element => (string?)element.Attribute("Id") ?? string.Empty,
                    element => NormalizeZipPath("ppt", (string?)element.Attribute("Target")))
                ?? new Dictionary<string, string>();

            var slideRelationshipIds = presentationDocument.Root?
                .Descendants(presentationNamespace + "sldId")
                .Select(element => (string?)element.Attribute(relationshipNamespace + "id"))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList()
                ?? [];

            var notesBySlide = new Dictionary<int, string?>();
            for (var index = 0; index < slideRelationshipIds.Count; index++)
            {
                var relationshipId = slideRelationshipIds[index]!;
                if (!slideTargetsByRelationshipId.TryGetValue(relationshipId, out var slidePath) ||
                    string.IsNullOrWhiteSpace(slidePath))
                {
                    continue;
                }

                var slideDirectory = Path.GetDirectoryName(slidePath.Replace('/', Path.DirectorySeparatorChar))
                    ?.Replace(Path.DirectorySeparatorChar, '/')
                    ?? "ppt/slides";
                var slideFileName = Path.GetFileName(slidePath);
                var slideRelationshipsPath = $"{slideDirectory}/_rels/{slideFileName}.rels";
                var slideRelationships = LoadXmlDocument(archive, slideRelationshipsPath);
                if (slideRelationships?.Root is null)
                {
                    continue;
                }

                var notesTarget = slideRelationships.Root
                    .Elements(packageRelationshipNamespace + "Relationship")
                    .FirstOrDefault(element =>
                        string.Equals(
                            (string?)element.Attribute("Type"),
                            "http://schemas.openxmlformats.org/officeDocument/2006/relationships/notesSlide",
                            StringComparison.OrdinalIgnoreCase))
                    ?.Attribute("Target")
                    ?.Value;

                if (string.IsNullOrWhiteSpace(notesTarget))
                {
                    continue;
                }

                var notesPath = NormalizeZipPath(slideDirectory, notesTarget);
                var notesDocument = LoadXmlDocument(archive, notesPath);
                if (notesDocument is null)
                {
                    continue;
                }

                XNamespace drawingNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";
                var noteLines = notesDocument
                    .Descendants(drawingNamespace + "t")
                    .Select(element => element.Value?.Trim())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();

                var notes = noteLines.Count == 0 ? null : string.Join(Environment.NewLine, noteLines);
                notesBySlide[index + 1] = string.IsNullOrWhiteSpace(notes) ? null : notes;
            }

            return notesBySlide;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to extract speaker notes from teaching material '{OriginalFileName}'.",
                originalFileName);

            return new Dictionary<int, string?>();
        }
    }

    private static XDocument? LoadXmlDocument(ZipArchive archive, string entryPath)
    {
        var normalizedEntryPath = entryPath.Replace('\\', '/');
        var entry = archive.GetEntry(normalizedEntryPath);
        if (entry is null)
        {
            return null;
        }

        using var entryStream = entry.Open();
        return XDocument.Load(entryStream);
    }

    private static string NormalizeZipPath(string baseDirectory, string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return baseDirectory.Replace('\\', '/').Trim('/');
        }

        var normalizedBaseDirectory = baseDirectory.Replace('\\', '/').Trim('/');
        if (!normalizedBaseDirectory.EndsWith('/'))
        {
            normalizedBaseDirectory += "/";
        }

        var baseUri = new Uri($"https://local/{normalizedBaseDirectory}", UriKind.Absolute);
        var resolvedUri = new Uri(baseUri, target.Replace('\\', '/'));
        return Uri.UnescapeDataString(resolvedUri.AbsolutePath).TrimStart('/');
    }

    private async Task RunLibreOfficeAsync(string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _libreOfficePath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo)
            ?? throw new InvalidOperationException("Could not start LibreOffice process");

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            logger.LogError(
                "LibreOffice preview generation failed with exit code {ExitCode}. Stdout: {Stdout}. Stderr: {Stderr}",
                process.ExitCode,
                stdout,
                stderr);

            throw new InvalidOperationException($"LibreOffice exited with code {process.ExitCode}");
        }
    }

    private async Task<string> WriteInputAsync(
        string tempDirectory,
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(originalFileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "input";
        }

        var inputPath = Path.Combine(tempDirectory, safeFileName);
        await File.WriteAllBytesAsync(inputPath, content, cancellationToken);
        return inputPath;
    }

    private string ResolveCachePath(string cachePath)
    {
        Directory.CreateDirectory(_storageRoot);
        var fullRoot = Path.GetFullPath(_storageRoot);
        var fullPath = Path.GetFullPath(Path.Combine(fullRoot, cachePath));
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Teaching material preview cache path is outside storage root");
        }

        return fullPath;
    }

    private static string CreateTempDirectory(Guid materialId)
    {
        var path = Path.Combine(Path.GetTempPath(), "kidzgo-teaching-materials", materialId.ToString("N"), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup only.
        }
    }

    private static string ResolveStorageRoot(IConfiguration configuration)
    {
        var configuredPath = configuration["TeachingMaterials:StoragePath"];
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        var publicStorageBasePath = configuration["FileStorage:Local:BasePath"];
        if (!string.IsNullOrWhiteSpace(publicStorageBasePath))
        {
            var parentPath = Directory.GetParent(publicStorageBasePath)?.FullName;
            if (!string.IsNullOrWhiteSpace(parentPath))
            {
                return Path.Combine(parentPath, "kidzgo-teaching-materials-private");
            }
        }

        return Path.Combine(AppContext.BaseDirectory, "kidzgo-teaching-materials-private");
    }
}
