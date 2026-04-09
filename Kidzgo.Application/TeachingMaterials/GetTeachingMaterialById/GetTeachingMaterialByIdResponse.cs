namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialById;

public sealed class GetTeachingMaterialByIdResponse
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
    public string FileExtension { get; init; } = null!;
    public long FileSize { get; init; }
    public string FileType { get; init; } = null!;
    public string Category { get; init; } = null!;
    public bool IsEncrypted { get; init; }
    public string EncryptionAlgorithm { get; init; } = null!;
    public string EncryptionKeyVersion { get; init; } = null!;
    public Guid UploadedByUserId { get; init; }
    public string UploadedByName { get; init; } = null!;
    public string PreviewUrl { get; init; } = null!;
    public string PreviewPdfUrl { get; init; } = null!;
    public string DownloadUrl { get; init; } = null!;
    public bool HasPdfPreview { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
