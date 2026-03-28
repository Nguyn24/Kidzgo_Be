using Kidzgo.Domain.Common;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterials;

public sealed class GetTeachingMaterialsResponse
{
    public Page<TeachingMaterialDto> Materials { get; init; } = null!;
}

public sealed class TeachingMaterialDto
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string ProgramCode { get; init; } = null!;
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public string? RelativePath { get; init; }
    public string DisplayName { get; init; } = null!;
    public string OriginalFileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public long FileSize { get; init; }
    public string FileType { get; init; } = null!;
    public string Category { get; init; } = null!;
    public Guid UploadedByUserId { get; init; }
    public string UploadedByName { get; init; } = null!;
    public bool IsEncrypted { get; init; }
    public string DownloadUrl { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
