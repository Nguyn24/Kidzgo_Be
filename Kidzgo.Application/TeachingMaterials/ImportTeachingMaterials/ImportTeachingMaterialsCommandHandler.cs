using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.ImportTeachingMaterials;

public sealed class ImportTeachingMaterialsCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITeachingMaterialStorageService storageService
) : ICommandHandler<ImportTeachingMaterialsCommand, ImportTeachingMaterialsResponse>
{
    private const long MaxFileSize = 100 * 1024 * 1024;

    public async Task<Result<ImportTeachingMaterialsResponse>> Handle(
        ImportTeachingMaterialsCommand command,
        CancellationToken cancellationToken)
    {
        var files = command.Files
            .Where(f => !ShouldSkipSystemFile(f.FileName))
            .ToList();

        if (files.Count == 0)
        {
            return Result.Failure<ImportTeachingMaterialsResponse>(TeachingMaterialErrors.NoFilesProvided());
        }

        var programResult = await ResolveProgramAsync(command, files, cancellationToken);
        if (programResult.IsFailure)
        {
            return Result.Failure<ImportTeachingMaterialsResponse>(programResult.Error);
        }

        var program = programResult.Value;
        var importedItems = new List<ImportedTeachingMaterialItem>();
        var skippedItems = new List<SkippedTeachingMaterialItem>();
        var now = VietnamTime.UtcNow();

        foreach (var file in files)
        {
            if (file.FileSize <= 0)
            {
                skippedItems.Add(CreateSkipped(file, "File is empty"));
                continue;
            }

            if (file.FileSize > MaxFileSize)
            {
                skippedItems.Add(CreateSkipped(file, $"File exceeds {MaxFileSize / 1024 / 1024}MB"));
                continue;
            }

            var relativePathWithoutRoot = TeachingMaterialMetadataParser.TrimRootSegment(file.RelativePath, file.FileName);
            var parsed = TeachingMaterialMetadataParser.Parse(file.FileName, relativePathWithoutRoot);
            if (parsed.FileType == TeachingMaterialFileType.Other)
            {
                skippedItems.Add(CreateSkipped(file, $"Unsupported file type: {Path.GetExtension(file.FileName)}"));
                continue;
            }

            var category = parsed.Category;
            if (!string.IsNullOrWhiteSpace(command.Category) &&
                Enum.TryParse<TeachingMaterialCategory>(command.Category, ignoreCase: true, out var parsedCategory))
            {
                category = parsedCategory;
            }

            var applyManualOverrides = files.Count == 1;
            var unitNumber = applyManualOverrides ? command.UnitNumber ?? parsed.UnitNumber : parsed.UnitNumber;
            var lessonNumber = applyManualOverrides ? command.LessonNumber ?? parsed.LessonNumber : parsed.LessonNumber;
            var lessonTitle = applyManualOverrides && !string.IsNullOrWhiteSpace(command.LessonTitle)
                ? command.LessonTitle.Trim()
                : parsed.LessonTitle;
            var relativePath = TeachingMaterialMetadataParser.NormalizeRelativePath(relativePathWithoutRoot, file.FileName) ?? parsed.RelativePath;
            var displayName = applyManualOverrides && !string.IsNullOrWhiteSpace(command.DisplayName)
                ? command.DisplayName.Trim()
                : parsed.DisplayName;
            var mimeType = ResolveMimeType(file.ContentType, parsed.FileType, file.FileName);
            var storageFolder = BuildStorageFolder(program, unitNumber, lessonNumber);

            file.FileStream.Position = 0;
            var storedFile = await storageService.SaveEncryptedAsync(
                file.FileStream,
                file.FileName,
                storageFolder,
                cancellationToken);

            var material = new TeachingMaterial
            {
                Id = Guid.NewGuid(),
                ProgramId = program.Id,
                UnitNumber = unitNumber,
                LessonNumber = lessonNumber,
                LessonTitle = lessonTitle,
                RelativePath = relativePath,
                DisplayName = displayName,
                OriginalFileName = file.FileName,
                StoragePath = storedFile.StoragePath,
                MimeType = mimeType,
                FileExtension = Path.GetExtension(file.FileName).ToLowerInvariant(),
                FileSize = file.FileSize,
                FileType = parsed.FileType,
                Category = category,
                IsEncrypted = true,
                EncryptionAlgorithm = storedFile.EncryptionAlgorithm,
                EncryptionKeyVersion = storedFile.EncryptionKeyVersion,
                UploadedByUserId = userContext.UserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.TeachingMaterials.Add(material);
            importedItems.Add(new ImportedTeachingMaterialItem
            {
                Id = material.Id,
                OriginalFileName = material.OriginalFileName,
                DisplayName = material.DisplayName,
                RelativePath = material.RelativePath,
                UnitNumber = material.UnitNumber,
                LessonNumber = material.LessonNumber,
                LessonTitle = material.LessonTitle,
                FileType = material.FileType.ToString(),
                Category = material.Category.ToString()
            });
        }

        if (importedItems.Count == 0)
        {
            return Result.Failure<ImportTeachingMaterialsResponse>(TeachingMaterialErrors.NoSupportedFilesFound());
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ImportTeachingMaterialsResponse
        {
            ProgramId = program.Id,
            ProgramName = program.Name,
            ImportedCount = importedItems.Count,
            SkippedCount = skippedItems.Count,
            ImportedItems = importedItems,
            SkippedItems = skippedItems
        };
    }

    private async Task<Result<Program>> ResolveProgramAsync(
        ImportTeachingMaterialsCommand command,
        IReadOnlyCollection<ImportTeachingMaterialFile> files,
        CancellationToken cancellationToken)
    {
        if (command.ProgramId.HasValue)
        {
            var programById = await context.Programs
                .FirstOrDefaultAsync(p => p.Id == command.ProgramId.Value && p.IsActive && !p.IsDeleted, cancellationToken);

            return programById is null
                ? Result.Failure<Program>(TeachingMaterialErrors.ProgramNotFound(command.ProgramId.Value))
                : programById;
        }

        var roots = files
            .Select(f => TeachingMaterialMetadataParser.GetRootSegment(f.RelativePath, f.FileName))
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roots.Count == 0)
        {
            return Result.Failure<Program>(TeachingMaterialErrors.ProgramCouldNotBeInferred());
        }

        if (roots.Count > 1)
        {
            return Result.Failure<Program>(TeachingMaterialErrors.MultipleProgramRoots(string.Join(", ", roots)));
        }

        var normalizedRoot = NormalizeProgramKey(roots[0]!);
        var programs = await context.Programs
            .Where(p => p.IsActive && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        var program = programs.FirstOrDefault(p => IsProgramMatch(p, normalizedRoot));
        return program is null
            ? Result.Failure<Program>(TeachingMaterialErrors.ProgramNameNotFound(roots[0]!))
            : program;
    }

    private static bool ShouldSkipSystemFile(string fileName)
    {
        return fileName.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase) ||
               fileName.Equals("thumbs.db", StringComparison.OrdinalIgnoreCase) ||
               fileName.Equals(".ds_store", StringComparison.OrdinalIgnoreCase);
    }

    private static SkippedTeachingMaterialItem CreateSkipped(ImportTeachingMaterialFile file, string reason)
    {
        return new SkippedTeachingMaterialItem
        {
            FileName = file.FileName,
            RelativePath = TeachingMaterialMetadataParser.NormalizeRelativePath(file.RelativePath, file.FileName),
            Reason = reason
        };
    }

    private static bool IsProgramMatch(Program program, string normalizedRoot)
    {
        var candidates = new[]
        {
            NormalizeProgramKey(program.Name),
            NormalizeProgramKey(program.Code)
        };

        return candidates.Any(candidate =>
            candidate == normalizedRoot ||
            candidate == $"{normalizedRoot}s" ||
            normalizedRoot == $"{candidate}s");
    }

    private static string NormalizeProgramKey(string value)
    {
        var filtered = value
            .Trim()
            .Where(char.IsLetterOrDigit)
            .ToArray();

        return new string(filtered).ToLowerInvariant();
    }

    private static string BuildStorageFolder(Program program, int? unitNumber, int? lessonNumber)
    {
        var programSegment = string.IsNullOrWhiteSpace(program.Code) ? program.Name : program.Code;
        programSegment = SanitizePathSegment(programSegment);

        var segments = new List<string> { programSegment };
        if (unitNumber.HasValue)
        {
            segments.Add($"unit-{unitNumber.Value:D2}");
        }

        if (lessonNumber.HasValue)
        {
            segments.Add($"lesson-{lessonNumber.Value:D2}");
        }

        return Path.Combine(segments.ToArray());
    }

    private static string SanitizePathSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(value
            .Trim()
            .Select(ch => invalidChars.Contains(ch) ? '-' : ch)
            .ToArray());

        return string.IsNullOrWhiteSpace(cleaned) ? "program" : cleaned;
    }

    private static string ResolveMimeType(string? contentType, TeachingMaterialFileType fileType, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            return contentType;
        }

        return fileType switch
        {
            TeachingMaterialFileType.Pdf => "application/pdf",
            TeachingMaterialFileType.Presentation when fileName.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase)
                => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            TeachingMaterialFileType.Presentation => "application/vnd.ms-powerpoint",
            TeachingMaterialFileType.Audio when fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) => "audio/mpeg",
            TeachingMaterialFileType.Audio => "audio/wav",
            TeachingMaterialFileType.Image => "image/*",
            TeachingMaterialFileType.Video => "video/*",
            TeachingMaterialFileType.Spreadsheet => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            TeachingMaterialFileType.Document => "application/octet-stream",
            TeachingMaterialFileType.Archive => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }
}
