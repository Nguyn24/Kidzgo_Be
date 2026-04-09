namespace Kidzgo.Application.TeachingMaterials.GetLessonBundle;

public sealed class GetLessonBundleResponse
{
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string ProgramCode { get; init; } = null!;
    public int UnitNumber { get; init; }
    public int LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public TeachingMaterialBundleItem? PrimaryPresentation { get; init; }
    public IReadOnlyCollection<TeachingMaterialBundleItem> Presentations { get; init; } = [];
    public IReadOnlyCollection<TeachingMaterialBundleItem> AudioFiles { get; init; } = [];
    public IReadOnlyCollection<TeachingMaterialBundleItem> ImageFiles { get; init; } = [];
    public IReadOnlyCollection<TeachingMaterialBundleItem> VideoFiles { get; init; } = [];
    public IReadOnlyCollection<TeachingMaterialBundleItem> Documents { get; init; } = [];
    public IReadOnlyCollection<TeachingMaterialBundleItem> SupplementaryFiles { get; init; } = [];
    public IReadOnlyCollection<TeachingMaterialBundleItem> OtherFiles { get; init; } = [];
}

public sealed class TeachingMaterialBundleItem
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = null!;
    public string OriginalFileName { get; init; } = null!;
    public string? RelativePath { get; init; }
    public string MimeType { get; init; } = null!;
    public string FileExtension { get; init; } = null!;
    public long FileSize { get; init; }
    public string FileType { get; init; } = null!;
    public string Category { get; init; } = null!;
    public string PreviewUrl { get; init; } = null!;
    public string PreviewPdfUrl { get; init; } = null!;
    public string DownloadUrl { get; init; } = null!;
    public bool HasPdfPreview { get; init; }
    public DateTime CreatedAt { get; init; }
}
