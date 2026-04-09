namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialBookmark;

public sealed class TeachingMaterialBookmarkResponse
{
    public Guid BookmarkId { get; init; }
    public Guid MaterialId { get; init; }
    public string DisplayName { get; init; } = null!;
    public string FileType { get; init; } = null!;
    public string ProgramName { get; init; } = null!;
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}
