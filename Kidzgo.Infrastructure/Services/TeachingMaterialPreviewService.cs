using System.Diagnostics;
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

                // Without an image processing package, keep a cached thumbnail endpoint backed by the PNG export.
                File.Copy(images[index], thumbnailAbsolutePath, overwrite: true);

                slideFiles.Add(new TeachingMaterialSlidePreviewFile
                {
                    SlideNumber = slideNumber,
                    PreviewImagePath = previewCachePath,
                    ThumbnailImagePath = thumbnailCachePath,
                    Width = 1920,
                    Height = 1080
                });
            }

            return slideFiles;
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
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
