using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.ImportTeachingMaterials;

public sealed class ImportTeachingMaterialsCommand : ICommand<ImportTeachingMaterialsResponse>
{
    public Guid? ProgramId { get; init; }
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public string? DisplayName { get; init; }
    public string? Category { get; init; }
    public IReadOnlyCollection<ImportTeachingMaterialFile> Files { get; init; } = [];
}

public sealed class ImportTeachingMaterialFile
{
    public string FileName { get; init; } = null!;
    public string? RelativePath { get; init; }
    public string? ContentType { get; init; }
    public long FileSize { get; init; }
    public Stream FileStream { get; init; } = null!;
}
