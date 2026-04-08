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

namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterial;

public sealed class CreateTeachingMaterialCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITeachingMaterialStorageService storageService
) : ICommandHandler<CreateTeachingMaterialCommand, CreateTeachingMaterialResponse>
{
    private const long MaxFileSize = 100 * 1024 * 1024;

    public async Task<Result<CreateTeachingMaterialResponse>> Handle(
        CreateTeachingMaterialCommand command,
        CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure<CreateTeachingMaterialResponse>(
                TeachingMaterialErrors.ProgramNotFound(command.ProgramId));
        }

        if (command.FileSize <= 0 || command.FileSize > MaxFileSize)
        {
            return Result.Failure<CreateTeachingMaterialResponse>(
                TeachingMaterialErrors.FileTooLarge(MaxFileSize / 1024 / 1024));
        }

        var parsed = TeachingMaterialMetadataParser.Parse(command.FileName, command.RelativePath);
        if (parsed.FileType == TeachingMaterialFileType.Other)
        {
            return Result.Failure<CreateTeachingMaterialResponse>(
                TeachingMaterialErrors.UnsupportedFileType(Path.GetExtension(command.FileName)));
        }

        var category = parsed.Category;
        if (!string.IsNullOrWhiteSpace(command.Category) &&
            Enum.TryParse<TeachingMaterialCategory>(command.Category, ignoreCase: true, out var parsedCategory))
        {
            category = parsedCategory;
        }

        var unitNumber = command.UnitNumber ?? parsed.UnitNumber;
        var lessonNumber = command.LessonNumber ?? parsed.LessonNumber;
        var lessonTitle = string.IsNullOrWhiteSpace(command.LessonTitle) ? parsed.LessonTitle : command.LessonTitle.Trim();
        var relativePath = TeachingMaterialMetadataParser.NormalizeRelativePath(command.RelativePath, command.FileName) ?? parsed.RelativePath;
        var displayName = string.IsNullOrWhiteSpace(command.DisplayName) ? parsed.DisplayName : command.DisplayName.Trim();
        var mimeType = ResolveMimeType(command.ContentType, parsed.FileType, command.FileName);
        var storageFolder = BuildStorageFolder(program, unitNumber, lessonNumber);

        var storedFile = await storageService.SaveEncryptedAsync(
            command.FileStream,
            command.FileName,
            storageFolder,
            cancellationToken);

        var now = VietnamTime.UtcNow();
        var material = new TeachingMaterial
        {
            Id = Guid.NewGuid(),
            ProgramId = program.Id,
            UnitNumber = unitNumber,
            LessonNumber = lessonNumber,
            LessonTitle = lessonTitle,
            RelativePath = relativePath,
            DisplayName = displayName,
            OriginalFileName = command.FileName,
            StoragePath = storedFile.StoragePath,
            MimeType = mimeType,
            FileExtension = Path.GetExtension(command.FileName).ToLowerInvariant(),
            FileSize = command.FileSize,
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
        await context.SaveChangesAsync(cancellationToken);

        return new CreateTeachingMaterialResponse
        {
            Id = material.Id,
            ProgramId = material.ProgramId,
            ProgramName = program.Name,
            UnitNumber = material.UnitNumber,
            LessonNumber = material.LessonNumber,
            LessonTitle = material.LessonTitle,
            RelativePath = material.RelativePath,
            DisplayName = material.DisplayName,
            OriginalFileName = material.OriginalFileName,
            MimeType = material.MimeType,
            FileSize = material.FileSize,
            FileType = material.FileType.ToString(),
            Category = material.Category.ToString(),
            CreatedAt = material.CreatedAt
        };
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
