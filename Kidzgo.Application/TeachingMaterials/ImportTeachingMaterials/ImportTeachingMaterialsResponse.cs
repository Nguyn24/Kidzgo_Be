namespace Kidzgo.Application.TeachingMaterials.ImportTeachingMaterials;

public sealed class ImportTeachingMaterialsResponse
{
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public IReadOnlyCollection<ImportedTeachingMaterialItem> ImportedItems { get; init; } = [];
    public IReadOnlyCollection<SkippedTeachingMaterialItem> SkippedItems { get; init; } = [];
}

public sealed class ImportedTeachingMaterialItem
{
    public Guid Id { get; init; }
    public string OriginalFileName { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string? RelativePath { get; init; }
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public string FileType { get; init; } = null!;
    public string Category { get; init; } = null!;
}

public sealed class SkippedTeachingMaterialItem
{
    public string FileName { get; init; } = null!;
    public string? RelativePath { get; init; }
    public string Reason { get; init; } = null!;
}
