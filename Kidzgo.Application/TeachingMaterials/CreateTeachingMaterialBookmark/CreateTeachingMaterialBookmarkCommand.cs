using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialBookmark;

public sealed class CreateTeachingMaterialBookmarkCommand : ICommand<TeachingMaterialBookmarkResponse>
{
    public Guid TeachingMaterialId { get; init; }
    public string? Note { get; init; }
}
