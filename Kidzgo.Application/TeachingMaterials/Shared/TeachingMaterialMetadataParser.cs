using System.Text.RegularExpressions;
using Kidzgo.Domain.TeachingMaterials;

namespace Kidzgo.Application.TeachingMaterials.Shared;

internal static partial class TeachingMaterialMetadataParser
{
    public static string? GetRootSegment(string? relativePath, string fileName)
    {
        var normalized = NormalizeRelativePath(relativePath, fileName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return segments.Length == 0 ? null : segments[0];
    }

    public static string? TrimRootSegment(string? relativePath, string fileName)
    {
        var normalized = NormalizeRelativePath(relativePath, fileName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length <= 1)
        {
            return null;
        }

        return string.Join('/', segments.Skip(1));
    }

    public static ParsedTeachingMaterialMetadata Parse(string fileName, string? relativePath)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var normalizedRelativePath = NormalizeRelativePath(relativePath, fileName);
        var source = ResolveLessonSource(normalizedRelativePath, fileNameWithoutExtension);

        var unitNumber = (int?)null;
        var lessonNumber = (int?)null;
        string? lessonTitle = null;

        var match = LessonPattern().Match(source);
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out var parsedUnit))
            {
                unitNumber = parsedUnit;
            }

            if (int.TryParse(match.Groups[2].Value, out var parsedLesson))
            {
                lessonNumber = parsedLesson;
            }

            lessonTitle = NormalizeTitle(match.Groups[3].Value);
        }

        var fileType = extension switch
        {
            ".pdf" => TeachingMaterialFileType.Pdf,
            ".ppt" or ".pptx" => TeachingMaterialFileType.Presentation,
            ".mp3" or ".wav" or ".m4a" => TeachingMaterialFileType.Audio,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => TeachingMaterialFileType.Image,
            ".mp4" or ".mov" or ".avi" or ".webm" or ".mkv" => TeachingMaterialFileType.Video,
            ".doc" or ".docx" or ".txt" => TeachingMaterialFileType.Document,
            ".xls" or ".xlsx" or ".csv" => TeachingMaterialFileType.Spreadsheet,
            ".zip" or ".rar" or ".7z" => TeachingMaterialFileType.Archive,
            _ => TeachingMaterialFileType.Other
        };

        var category = fileType switch
        {
            TeachingMaterialFileType.Presentation when unitNumber.HasValue && lessonNumber.HasValue => TeachingMaterialCategory.LessonSlide,
            TeachingMaterialFileType.Audio or TeachingMaterialFileType.Image or TeachingMaterialFileType.Video
                when unitNumber.HasValue && lessonNumber.HasValue => TeachingMaterialCategory.LessonAsset,
            TeachingMaterialFileType.Pdf when !unitNumber.HasValue && !lessonNumber.HasValue => TeachingMaterialCategory.ProgramDocument,
            _ => lessonNumber.HasValue ? TeachingMaterialCategory.Supplementary : TeachingMaterialCategory.Other
        };

        return new ParsedTeachingMaterialMetadata
        {
            UnitNumber = unitNumber,
            LessonNumber = lessonNumber,
            LessonTitle = lessonTitle,
            RelativePath = normalizedRelativePath,
            DisplayName = NormalizeTitle(fileNameWithoutExtension),
            FileType = fileType,
            Category = category
        };
    }

    public static string? NormalizeRelativePath(string? relativePath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var normalized = relativePath
            .Replace('\\', '/')
            .Trim()
            .Trim('/');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^fileName.Length].TrimEnd('/');
        }

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string ResolveLessonSource(string? normalizedRelativePath, string fileNameWithoutExtension)
    {
        if (!string.IsNullOrWhiteSpace(normalizedRelativePath))
        {
            var segments = normalizedRelativePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Sample convention:
            // ProgramRoot/UNIT 1-L4-LANGUAGE PRACTICE - Animal Audio Files/file.wav
            // The nearest segment carrying "UNIT x-Ly-Title" is the authoritative lesson source.
            var matchedSegment = segments
                .Reverse()
                .FirstOrDefault(segment => LessonPattern().IsMatch(segment));

            if (!string.IsNullOrWhiteSpace(matchedSegment))
            {
                return matchedSegment;
            }
        }

        return fileNameWithoutExtension;
    }

    private static string NormalizeTitle(string value)
    {
        var primary = value.Split(" - ", 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];
        return primary.Replace('_', ' ').Trim();
    }

    [GeneratedRegex(@"UNIT\s*(\d+)\s*-\s*L(\d+)\s*-\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex LessonPattern();
}

internal sealed class ParsedTeachingMaterialMetadata
{
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public string? RelativePath { get; init; }
    public string DisplayName { get; init; } = null!;
    public TeachingMaterialFileType FileType { get; init; }
    public TeachingMaterialCategory Category { get; init; }
}
