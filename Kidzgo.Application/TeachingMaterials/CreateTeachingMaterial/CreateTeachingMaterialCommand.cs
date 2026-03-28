using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterial;

public sealed class CreateTeachingMaterialCommand : ICommand<CreateTeachingMaterialResponse>
{
    public Guid ProgramId { get; init; }
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public string? RelativePath { get; init; }
    public string? DisplayName { get; init; }
    public string? Category { get; init; }
    public string FileName { get; init; } = null!;
    public string? ContentType { get; init; }
    public long FileSize { get; init; }
    public Stream FileStream { get; init; } = null!;
}
